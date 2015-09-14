using System.Collections.Generic;
using System.Linq;
using Steam.API;
using SteamNSteel.Jobs;

namespace SteamNSteel.Impl.Jobs
{
	public class ProcessTransportJob : IJob
	{
		private readonly SteamTransport unit;
		private readonly SteamNSteelConfiguration _config;
		private SteamTransport[] _horizontalAdjacentTransports;

		internal ProcessTransportJob(SteamTransport unit, SteamNSteelConfiguration config)
		{
			this.unit = unit;
			_config = config;
		}

		public void Execute()
		{
			_horizontalAdjacentTransports = new[]
			{
				(SteamTransport)unit.GetAdjacentTransport(ForgeDirection.NORTH),
				(SteamTransport)unit.GetAdjacentTransport(ForgeDirection.EAST),
				(SteamTransport)unit.GetAdjacentTransport(ForgeDirection.SOUTH),
				(SteamTransport)unit.GetAdjacentTransport(ForgeDirection.WEST),
			};

			TransferSteam();
			CalculateUnitHeat();
			TransferWater();
			CondenseSteam();
		}

		private void CondenseSteam()
		{
			var steamStored = unit.GetSteamStored();

			var usableSteam = steamStored - unit.NewSteam;
			if (usableSteam <= 0) return;

			var newCondensation = usableSteam * _config.CondensationRatePerTick * ((100 - unit.GetTemperature()) / 100) * _config.TickLength;
			newCondensation = unit.TakeSteam((int)newCondensation);
			var waterGained = newCondensation * _config.SteamToWaterRatio;
			unit.AddCondensate((int)waterGained);
		}

		private void CalculateUnitHeat()
		{
			var unitTemperature = unit.GetTemperature();
			var tempDifference = unit.GetCalculatedSteamDensity() - unitTemperature;
			var temperature = unitTemperature + (unit.GetHeatConductivity() * (tempDifference / 100));
			if (temperature > 100)
			{
				temperature = 100;
			}
			if (temperature < 0)
			{
				temperature = 0;
			}
			unit.SetTemperature(temperature);
		}

		private void TransferSteam()
		{
			var usableSteam = unit.GetSteamStored() - unit.NewSteam;

			if (usableSteam <= 0) return;

			var unitAbove = unit.GetAdjacentTransport(ForgeDirection.UP);
			//First, work on any units above
			if (unitAbove != null && !unit.SteamFlowSourceUnits.Contains(unitAbove))
			{
				TransferSteamAbove(usableSteam);
			}
			if (usableSteam > 0)
			{
				TransferSteamAcross(usableSteam);
			}
		}

		private void TransferSteamAcross(double usableSteam)
		{
			var eligibleUnits = new List<ISteamTransport>();
			double steamSpaceAvailable = 0;

			foreach (var neighbourUnit in _horizontalAdjacentTransports)
			{
				if (unit.SteamFlowSourceUnits.Contains(neighbourUnit))
				{
					continue;
				}
				//Steam providers can always push?
				var neighbourSteamStored = neighbourUnit.GetSteamStored();
				var neighbourMaximumSteam = neighbourUnit.GetCalculatedMaximumSteam();
				if (neighbourSteamStored < neighbourMaximumSteam && neighbourSteamStored < usableSteam)
				{
					eligibleUnits.Add(neighbourUnit);
					steamSpaceAvailable += (neighbourMaximumSteam - neighbourSteamStored);
				}
			}
			var transportBelow = unit.GetAdjacentTransport(ForgeDirection.NORTH);
            if (transportBelow != null && unit.GetCalculatedSteamDensity() >= _config.EQUILIBRIUM && !unit.SteamFlowSourceUnits.Contains(transportBelow))
			{
				var neighbourSteamStored = transportBelow.GetSteamStored();
				var neighbourMaximumSteam = transportBelow.GetCalculatedMaximumSteam();
				if (neighbourSteamStored < neighbourMaximumSteam && neighbourSteamStored < usableSteam)
				{
					eligibleUnits.Add(transportBelow);
					steamSpaceAvailable += (neighbourMaximumSteam - neighbourSteamStored);
				}
			}

			var originalSteamStored = usableSteam;
			foreach (var neighbourUnit in eligibleUnits)
			{
				var neighbourSteamStored = neighbourUnit.GetSteamStored();
				var neighbourMaximumSteam = neighbourUnit.GetCalculatedMaximumSteam();
				var ratio = (neighbourMaximumSteam - neighbourSteamStored) / steamSpaceAvailable;

				var amountTransferred = originalSteamStored * ratio;

				if (neighbourSteamStored + amountTransferred > neighbourMaximumSteam)
				{
					amountTransferred = neighbourMaximumSteam - neighbourSteamStored;
				}

				amountTransferred = amountTransferred * _config.TransferRatio;

				amountTransferred = unit.TakeSteam((int)amountTransferred);
				unit.AddSteam((int)amountTransferred);
				neighbourUnit.SteamFlowSourceUnits.Add(unit);
			}
		}

		private void TransferSteamAbove(decimal usableSteam)
		{
			var unitAbove = unit.GetAdjacentTransport(ForgeDirection.UP);
			var neighbourSteamStored = unitAbove.GetSteamStored();
			var neighbourMaximumSteam = unitAbove.GetCalculatedMaximumSteam();
			if (neighbourSteamStored <= neighbourMaximumSteam)
			{
				var amountTransferred = usableSteam;

				if (neighbourSteamStored + amountTransferred > neighbourMaximumSteam)
				{
					amountTransferred = neighbourMaximumSteam - neighbourSteamStored;
				}

				amountTransferred = unit.TakeSteam((int)amountTransferred);
				unitAbove.AddSteam((int)amountTransferred);
				unitAbove.SteamFlowSourceUnits.Add(unit);
			}
		}

		private void TransferWater()
		{
			var usableWater = unit.GetWaterStored() - unit.NewWater;

			if (usableWater <= 0) return;
			var unitBelow = unit.GetAdjacentTransport(ForgeDirection.DOWN);
			//First, work on any units above
			if (unitBelow != null && !unit.WaterFlowSourceUnits.Contains(unitBelow))
			{
				TransferWaterBelow(usableWater);
			}
			if (usableWater > 0 && _horizontalAdjacentTransports.Any())
			{
				TransferWaterAcross(usableWater);
			}
		}

		private void TransferWaterAcross(decimal waterUsedAtStart)
		{
			var eligibleUnits = new List<SteamTransport>();
			decimal waterSpaceAvailable = 0;
			foreach (var neighbourUnit in _horizontalAdjacentTransports)
			{
				if (unit.WaterFlowSourceUnits.Contains(neighbourUnit))
				{
					continue;
				}

				var neighbourWaterStored = neighbourUnit.GetWaterStored();
				var neighbourMaximumWater = neighbourUnit.GetMaximumWater();
				//Steam providers can always push?
				if (neighbourWaterStored < neighbourMaximumWater &&
					(neighbourWaterStored < waterUsedAtStart))
				{
					eligibleUnits.Add(neighbourUnit);
					waterSpaceAvailable += (neighbourMaximumWater - neighbourWaterStored);
				}
			}

			var originalWaterStored = waterUsedAtStart;
			foreach (var neighbourUnit in eligibleUnits)
			{
				var neighbourWaterStored = neighbourUnit.GetWaterStored();
				var neighbourMaximumWater = neighbourUnit.GetMaximumWater();

				var ratio = (neighbourMaximumWater - neighbourWaterStored) / waterSpaceAvailable;

				var amountTransferred = originalWaterStored * ratio;

				if (neighbourWaterStored + amountTransferred > neighbourMaximumWater)
				{
					amountTransferred = neighbourMaximumWater - neighbourWaterStored;
				}

				amountTransferred = unit.TakeCondensate((int)amountTransferred);
				neighbourUnit.AddCondensate((int)amountTransferred);
				
				neighbourUnit.WaterFlowSourceUnits.Add(unit);
			}
		}

		private void TransferWaterBelow(decimal usableWater)
		{
			
			var unitAbove = unit.GetAdjacentTransport(ForgeDirection.DOWN);
			var neighbourWaterStored = unitAbove.GetWaterStored();
			var neighbourMaximumWater = unitAbove.GetMaximumWater();
			if (neighbourWaterStored <= neighbourMaximumWater)
			{
				var amountTransferred = usableWater;

				if (neighbourWaterStored + amountTransferred > neighbourMaximumWater)
				{
					amountTransferred = neighbourMaximumWater - neighbourWaterStored;
				}

				if (usableWater - amountTransferred < 0)
				{
					amountTransferred = usableWater;
				}

				amountTransferred = unit.TakeCondensate((int) amountTransferred);
				unitAbove.AddCondensate((int)amountTransferred);
				unitAbove.WaterFlowSourceUnits.Add(unit);
			}
		}
	}
}
