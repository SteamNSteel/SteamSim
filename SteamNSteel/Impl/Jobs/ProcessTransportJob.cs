using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Steam.API;
using SteamNSteel.Jobs;

namespace SteamNSteel.Impl.Jobs
{
	public class ProcessTransportJob : IJob
	{
		private readonly SteamTransport unit;

		internal ProcessTransportJob(SteamTransport unit)
		{
			this.unit = unit;
		}

		public void Execute()
		{
			TransferSteam();
			CalculateUnitHeat();
			TransferWater();
			CondenseSteam();
		}

		private void CondenseSteam()
		{
			var usableSteam = unit.SteamStored - unit.NewSteam;
			if (usableSteam <= 0) return;

			var newCondensation = usableSteam * CondensationRatePerTick * ((100 - unit.Temperature) / 100) * TicksPerSecond * timeElapsed;

			if (unit.SteamStored - newCondensation < 0)
			{
				newCondensation = unit.SteamStored;
			}
			var waterGained = newCondensation * SteamToWaterRatio;
			unit.SteamStored -= newCondensation;
			unit.WaterStored += waterGained;
		}

		private void CalculateUnitHeat()
		{
			var tempDifference = unit.SteamDensity - (double)unit.Temperature;
			var temperature = unit.Temperature + (unit.HeatConductivity * (decimal)(tempDifference / 100));
			if (temperature > 100)
			{
				temperature = 100;
			}
			if (temperature < 0)
			{
				temperature = 0;
			}
			unit.Temperature = temperature;
		}

		private void TransferSteam()
		{
			var usableSteam = unit.SteamStored - unit.NewSteam;

			if (usableSteam <= 0) return;
			//First, work on any units above
			if (unit.UnitAbove != null && !unit.SteamFlowSourceUnits.Contains(unit.UnitAbove))
			{
				TransferSteamAbove(unit, usableSteam, timeElapsed);
			}
			if (usableSteam > 0)
			{
				TransferSteamAcross(unit, usableSteam, timeElapsed);
			}
		}

		private void TransferSteamAcross(SteamUnit unit, decimal usableSteam)
		{
			var eligibleUnits = new List<SteamUnit>();
			decimal steamSpaceAvailable = 0;
			foreach (var neighbourUnit in unit.HorizontalAdjacentConnections)
			{
				if (unit.SteamFlowSourceUnits.Contains(neighbourUnit))
				{
					continue;
				}
				//Steam providers can always push?
				if (neighbourUnit.SteamStored < neighbourUnit.ActualMaxSteam &&
					(neighbourUnit.SteamStored < usableSteam))
				{
					eligibleUnits.Add(neighbourUnit);
					steamSpaceAvailable += (neighbourUnit.ActualMaxSteam - neighbourUnit.SteamStored);
				}
			}

			if (unit.UnitBelow != null && unit.SteamDensity >= EQUILIBRIUM && !unit.SteamFlowSourceUnits.Contains(unit.UnitBelow))
			{
				var neighbourUnit = unit.UnitBelow;
				if (neighbourUnit.SteamStored < neighbourUnit.ActualMaxSteam &&
					(neighbourUnit.SteamStored < usableSteam))
				{
					eligibleUnits.Add(neighbourUnit);
					steamSpaceAvailable += (neighbourUnit.ActualMaxSteam - neighbourUnit.SteamStored);
				}
			}

			var originalSteamStored = usableSteam;
			foreach (var neighbourUnit in eligibleUnits)
			{
				var ratio = (neighbourUnit.ActualMaxSteam - neighbourUnit.SteamStored) / steamSpaceAvailable;

				var amountTransferred = originalSteamStored * timeElapsed * ratio;

				if (neighbourUnit.SteamStored + amountTransferred > neighbourUnit.ActualMaxSteam)
				{
					amountTransferred = neighbourUnit.ActualMaxSteam - neighbourUnit.SteamStored;
				}

				amountTransferred = amountTransferred * TransferRatio;
				if (unit.SteamStored - amountTransferred < 0)
				{
					amountTransferred = unit.SteamStored;
				}

				neighbourUnit.SteamStored += amountTransferred;
				neighbourUnit.NewSteam += amountTransferred;
				unit.SteamStored -= amountTransferred;
				neighbourUnit.SteamFlowSourceUnits.Add(unit);
			}
		}

		private void TransferSteamAbove(decimal usableSteam)
		{
			var unitAbove = unit.UnitAbove;
			if (unitAbove.SteamStored <= unitAbove.ActualMaxSteam)
			{
				var amountTransferred = usableSteam * timeElapsed;

				if (unitAbove.SteamStored + amountTransferred > unitAbove.ActualMaxSteam)
				{
					amountTransferred = unitAbove.ActualMaxSteam - unitAbove.SteamStored;
				}

				if (usableSteam - amountTransferred < 0)
				{
					amountTransferred = usableSteam;
				}
				unitAbove.SteamStored += amountTransferred;
				unitAbove.NewSteam += amountTransferred;
				unit.SteamStored -= amountTransferred;
				unitAbove.SteamFlowSourceUnits.Add(unit);
			}
		}

		private void TransferWater()
		{
			var usableWater = unit.WaterStored - unit.NewWater;

			if (usableWater <= 0) return;
			//First, work on any units above
			if (unit.UnitBelow != null && !unit.WaterFlowSourceUnits.Contains(unit.UnitBelow))
			{
				TransferWaterBelow(unit, usableWater, timeElapsed);
			}
			if (usableWater > 0 && unit.HorizontalAdjacentConnections.Any())
			{
				TransferWaterAcross(unit, usableWater, timeElapsed);
			}
		}

		private static void TransferWaterAcross(decimal waterUsedAtStart)
		{
			var eligibleUnits = new List<SteamUnit>();
			decimal waterSpaceAvailable = 0;
			foreach (var neighbourUnit in unit.HorizontalAdjacentConnections)
			{
				if (unit.WaterFlowSourceUnits.Contains(neighbourUnit))
				{
					continue;
				}
				//Steam providers can always push?
				if (neighbourUnit.WaterStored < neighbourUnit.MaxWater &&
					(neighbourUnit.WaterStored < waterUsedAtStart))
				{
					eligibleUnits.Add(neighbourUnit);
					waterSpaceAvailable += (neighbourUnit.MaxWater - neighbourUnit.WaterStored);
				}
			}

			var originalWaterStored = waterUsedAtStart;
			foreach (var neighbourUnit in eligibleUnits)
			{
				var ratio = (neighbourUnit.MaxWater - neighbourUnit.WaterStored) / waterSpaceAvailable;

				var amountTransferred = originalWaterStored * timeElapsed * ratio;

				if (neighbourUnit.WaterStored + amountTransferred > neighbourUnit.MaxWater)
				{
					amountTransferred = neighbourUnit.MaxWater - neighbourUnit.WaterStored;
				}

				if (unit.WaterStored - amountTransferred < 0)
				{
					amountTransferred = unit.WaterStored;
				}

				neighbourUnit.WaterStored += amountTransferred;
				neighbourUnit.NewWater += amountTransferred;
				unit.WaterStored -= amountTransferred;
				neighbourUnit.WaterFlowSourceUnits.Add(unit);
			}
		}

		private void TransferWaterBelow(decimal usableWater)
		{
			
			var unitBelow = unit.GetAdjacentTransport(ForgeDirection.DOWN);
			if (unitBelow.WaterStored <= unitBelow.MaxWater)
			{
				var amountTransferred = usableWater * timeElapsed;

				if (unitBelow.WaterStored + amountTransferred > unitBelow.MaxWater)
				{
					amountTransferred = unitBelow.MaxWater - unitBelow.WaterStored;
				}

				if (usableWater - amountTransferred < 0)
				{
					amountTransferred = usableWater;
				}
				unitBelow.WaterStored += amountTransferred;
				unitBelow.NewWater += amountTransferred;
				unit.WaterStored -= amountTransferred;
				unitBelow.WaterFlowSourceUnits.Add(unit);
			}
		}
	}
}
