using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace SteamPipes.API
{
	public static class SteamManager
	{
		private const decimal TransferRatio = 0.8m;
		private const decimal SteamToWaterRatio = 0.8m;
		private const decimal CondensationRatePerTick = 0.01m;
		private const int TicksPerSecond = 20;

		private static readonly ConcurrentDictionary<PointI2D, SteamUnit> SteamUnits =
			new ConcurrentDictionary<PointI2D, SteamUnit>();

		private static readonly ConcurrentDictionary<PointI2D, ISteamProvider> SteamProviders =
			new ConcurrentDictionary<PointI2D, ISteamProvider>();

		private static int _currentStep;
		private static long _previousTick = Stopwatch.GetTimestamp();
		private static readonly bool paused = false;
		private static bool running = true;
		private static double EQUILIBRIUM = 75;

		internal static SteamUnit CreateSteamUnit<TType>(int column, int row) where TType : SteamUnit, new()
		{
			SteamUnit existingUnit;
			var point = new PointI2D(column, row);
			if (SteamUnits.TryGetValue(point, out existingUnit))
			{
				return existingUnit;
			}
			var steamUnit = new TType {X = column, Y = row};


			if (SteamUnits.TryGetValue(new PointI2D(column, row - 1), out existingUnit))
			{
				steamUnit.UnitAbove = existingUnit;
				lock (existingUnit)
				{
					existingUnit.UnitBelow = steamUnit;
				}
			}

			if (SteamUnits.TryGetValue(new PointI2D(column, row + 1), out existingUnit))
			{
				steamUnit.UnitBelow = existingUnit;
				lock (existingUnit)
				{
					existingUnit.UnitAbove = steamUnit;
				}
			}
			if (SteamUnits.TryGetValue(new PointI2D(column - 1, row), out existingUnit))
			{
				steamUnit.UnitLeft = existingUnit;
				lock (existingUnit)
				{
					existingUnit.UnitRight = steamUnit;
				}
			}
			if (SteamUnits.TryGetValue(new PointI2D(column + 1, row), out existingUnit))
			{
				steamUnit.UnitRight = existingUnit;
				lock (existingUnit)
				{
					existingUnit.UnitLeft = steamUnit;
				}
			}
			SteamUnits.TryAdd(point, steamUnit);
			if (steamUnit is ISteamProvider)
			{
				SteamProviders.TryAdd(point, (ISteamProvider) steamUnit);
			}
			return steamUnit;
		}

		internal static void RemoveSteamUnit(SteamUnit steamUnit)
		{
			lock (steamUnit)
			{
				if (steamUnit.UnitAbove != null)
				{
					lock (steamUnit.UnitAbove)
					{
						steamUnit.UnitAbove.UnitBelow = null;
					}
				}
				if (steamUnit.UnitBelow != null)
				{
					lock (steamUnit.UnitBelow)
					{
						steamUnit.UnitBelow.UnitAbove = null;
					}
				}
				if (steamUnit.UnitLeft != null)
				{
					lock (steamUnit.UnitLeft)
					{
						steamUnit.UnitLeft.UnitRight = null;
					}
				}
				if (steamUnit.UnitRight != null)
				{
					lock (steamUnit.UnitRight)
					{
						steamUnit.UnitRight.UnitLeft = null;
					}
				}
			}
			SteamUnit removedUnit;
			ISteamProvider removedProvider;
			var point = new PointI2D(steamUnit.X, steamUnit.Y);
			SteamUnits.TryRemove(point, out removedUnit);
			SteamProviders.TryRemove(point, out removedProvider);
		}

		public static void InjectSteam(long amount, SteamUnit steamUnit)
		{
			if (steamUnit.SteamStored + amount <= steamUnit.MaxSteam)
			{
				steamUnit.SteamStored += amount;
			}
		}

		public static void RemoveSteam(long amount, SteamUnit steamUnit)
		{
			if (steamUnit.SteamStored - amount >= 0)
			{
				steamUnit.SteamStored -= amount;
			}
			else
			{
				steamUnit.SteamStored = 0;
			}
		}

		public static void StepSimulation(decimal timeElapsed)
		{
			_currentStep++;

			var unprocessedUnits = new HashSet<SteamUnit>(SteamUnits.Values);
			//Ideally this would be prepopulated with all the sources.

			var unitsToProcess = new Queue<SteamUnit>();
			foreach (var steamProvider in SteamProviders.Values)
			{
				var unit = (SteamUnit) steamProvider;
				unit.SteamFlowSourceUnits.Clear();
				unit.WaterFlowSourceUnits.Clear();

				unitsToProcess.Enqueue(unit);
			}

			while (unprocessedUnits.Count > 0)
			{
				//This would hit if we've found a network with no sources.
				//I could check to make sure that there is steam somewhere in it and disable the network if neccessary.
				if (unitsToProcess.Count == 0)
				{
					var steamUnit = unprocessedUnits.First();
					steamUnit.SteamFlowSourceUnits.Clear();
					steamUnit.WaterFlowSourceUnits.Clear();
					steamUnit.NewSteam = 0;
					steamUnit.NewWater = 0;
					unitsToProcess.Enqueue(steamUnit);
				}
				while (unitsToProcess.Count > 0)
				{
					var unit = unitsToProcess.Dequeue();
					lock (unit)
					{
						unprocessedUnits.Remove(unit);
						if (unit.ProcessPass == _currentStep) continue; //Already processed.

						unit.ProcessPass = _currentStep;
						foreach (var connection in unit.AllAdjacentConnections)
						{
							if (connection.ProcessPass != _currentStep)
							{
								connection.SteamFlowSourceUnits.Clear();
								connection.NewSteam = 0;
								connection.WaterFlowSourceUnits.Clear();
								connection.NewWater = 0;
								unitsToProcess.Enqueue(connection);
							}
						}

						if (ConsumeSteam(unit, timeElapsed)) continue;

						TransferSteam(unit, timeElapsed);
						CalculateUnitHeat(unit);
						ProduceSteam(unit, timeElapsed);
						TransferWater(unit, timeElapsed);
						CondenseSteam(unit, timeElapsed);
					}
				}
			}
		}

		private static void CondenseSteam(SteamUnit unit, decimal timeElapsed)
		{
			var usableSteam = unit.SteamStored - unit.NewSteam;
			if (usableSteam <= 0) return;

			var newCondensation = usableSteam*CondensationRatePerTick*((100 - unit.Temperature)/100)*TicksPerSecond*timeElapsed;

			if (unit.SteamStored - newCondensation < 0)
			{
				newCondensation = unit.SteamStored;
			}
			var waterGained = newCondensation*SteamToWaterRatio;
			unit.SteamStored -= newCondensation;
			unit.WaterStored += waterGained;
		}

		private static void CalculateUnitHeat(SteamUnit unit)
		{
			var tempDifference = unit.SteamDensity - (double) unit.Temperature;
			var temperature = unit.Temperature + (unit.HeatConductivity*(decimal) (tempDifference/100));
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

		private static void ProduceSteam(SteamUnit unit, decimal timeElapsed)
		{
			var steamProvider = unit as ISteamProvider;
			if (steamProvider != null)
			{
				var steamProduced = steamProvider.AmountPerTick*TicksPerSecond*timeElapsed;

				if (unit.SteamStored + steamProduced > unit.ActualMaxSteam)
				{
					steamProduced = unit.ActualMaxSteam - unit.SteamStored;
				}
				unit.SteamStored += steamProduced;
			}
		}

		private static bool ConsumeSteam(SteamUnit unit, decimal timeElapsed)
		{
			var steamConsumer = unit as ISteamConsumer;
			if (steamConsumer != null)
			{
				var amountConsumed = steamConsumer.AmountPerTick*TicksPerSecond*timeElapsed;
				if (amountConsumed > unit.SteamStored)
				{
					amountConsumed = unit.SteamStored;
				}
				unit.SteamStored -= amountConsumed;
				return true;
			}
			return false;
		}

		private static void TransferSteam(SteamUnit unit, decimal timeElapsed)
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

		private static void TransferSteamAcross(SteamUnit unit, decimal usableSteam, decimal timeElapsed)
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
				    (neighbourUnit.SteamStored < usableSteam || unit is ISteamProvider || neighbourUnit is ISteamConsumer))
				{
					eligibleUnits.Add(neighbourUnit);
					steamSpaceAvailable += (neighbourUnit.ActualMaxSteam - neighbourUnit.SteamStored);
				}
			}

			if (unit.UnitBelow != null && unit.SteamDensity >= EQUILIBRIUM && !unit.SteamFlowSourceUnits.Contains(unit.UnitBelow))
			{
				var neighbourUnit = unit.UnitBelow;
				if (neighbourUnit.SteamStored < neighbourUnit.ActualMaxSteam &&
				    (neighbourUnit.SteamStored < usableSteam || unit is ISteamProvider || neighbourUnit is ISteamConsumer))
				{
					eligibleUnits.Add(neighbourUnit);
					steamSpaceAvailable += (neighbourUnit.ActualMaxSteam - neighbourUnit.SteamStored);
				}
			}

			var originalSteamStored = usableSteam;
			foreach (var neighbourUnit in eligibleUnits)
			{
				var ratio = (neighbourUnit.ActualMaxSteam - neighbourUnit.SteamStored)/steamSpaceAvailable;

				var amountTransferred = originalSteamStored*timeElapsed*ratio;

				if (neighbourUnit.SteamStored + amountTransferred > neighbourUnit.ActualMaxSteam)
				{
					amountTransferred = neighbourUnit.ActualMaxSteam - neighbourUnit.SteamStored;
				}

				amountTransferred = amountTransferred*TransferRatio;
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

		private static void TransferSteamAbove(SteamUnit unit, decimal usableSteam, decimal timeElapsed)
		{
			var unitAbove = unit.UnitAbove;
			if (unitAbove.SteamStored <= unitAbove.ActualMaxSteam)
			{
				var amountTransferred = usableSteam*timeElapsed;

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

		private static void TransferWater(SteamUnit unit, decimal timeElapsed)
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

		private static void TransferWaterAcross(SteamUnit unit, decimal waterUsedAtStart, decimal timeElapsed)
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
				var ratio = (neighbourUnit.MaxWater - neighbourUnit.WaterStored)/waterSpaceAvailable;

				var amountTransferred = originalWaterStored*timeElapsed*ratio;

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

		private static void TransferWaterBelow(SteamUnit unit, decimal usableWater, decimal timeElapsed)
		{
			var unitBelow = unit.UnitBelow;
			if (unitBelow.WaterStored <= unitBelow.MaxWater)
			{
				var amountTransferred = usableWater*timeElapsed;

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

		public static void StartSimulationThread()
		{
			var thread = new Thread(Start) {Name = "Simulation Thread"};
			thread.Start();
		}

		private static void Start()
		{
			_previousTick = Stopwatch.GetTimestamp();
			while (running)
			{
				if (!paused)
				{
					var thisTick = Stopwatch.GetTimestamp();
					var timeElapsed = (thisTick - _previousTick)/(decimal) Stopwatch.Frequency;
					if (timeElapsed > 1) timeElapsed = 1;
					_previousTick = thisTick;

					StepSimulation(timeElapsed);
					Thread.Sleep(1000/30);
					//Thread.Sleep(1000);
				}
			}
		}

		public static void Stop()
		{
			running = false;
		}
	}
}