using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace SteamPipes
{
	public static class SteamManager
	{
		private static readonly ConcurrentDictionary<PointI2D, SteamUnit> SteamUnits = new ConcurrentDictionary<PointI2D, SteamUnit>();
		private static readonly ConcurrentDictionary<PointI2D, ISteamProvider> SteamProviders = new ConcurrentDictionary<PointI2D, ISteamProvider>();
		private const decimal TransferRatio = 0.8m;
		private const int TicksPerSecond = 20;

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
			if (steamUnit is ISteamProvider) { 
				SteamProviders.TryAdd(point, (ISteamProvider)steamUnit);
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

		private static int _currentStep;
		private static long _previousTick = Stopwatch.GetTimestamp();

		public static void StepSimulation(decimal timeElapsed)
		{
			

			_currentStep++;

			HashSet<SteamUnit> unprocessedUnits = new HashSet<SteamUnit>(SteamUnits.Values);
			//Ideally this would be prepopulated with all the sources.

			Queue<SteamUnit> unitsToProcess = new Queue<SteamUnit>();
			foreach (var steamProvider in SteamProviders.Values)
			{
				SteamUnit unit = (SteamUnit)steamProvider;
				unit.FlowSourceUnits.Clear();
				
				unitsToProcess.Enqueue(unit);
			}

			while (unprocessedUnits.Count > 0)
			{
				//This would hit if we've found a network with no sources.
				//I could check to make sure that there is steam somewhere in it and disable the network if neccessary.
				if (unitsToProcess.Count == 0)
				{
					var steamUnit = unprocessedUnits.First();
					steamUnit.FlowSourceUnits.Clear();
					steamUnit.NewSteam = 0;
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
								connection.FlowSourceUnits.Clear();
								connection.NewSteam = 0;
								unitsToProcess.Enqueue(connection);
							}
						}

						if (ConsumeSteam(unit, timeElapsed)) continue;
						
						TransferSteam(unit, timeElapsed);
						ProduceSteam(unit, timeElapsed);
					}
				}
			}
		}

		private static void TransferSteam(SteamUnit unit, decimal timeElapsed)
		{
			var usableSteam = unit.SteamStored - unit.NewSteam;

			if (usableSteam <= 0) return;
			//First, work on any units above
			if (unit.UnitAbove != null && !unit.FlowSourceUnits.Contains(unit.UnitAbove))
			{
				TransferSteamAbove(unit, usableSteam, timeElapsed);
			}
			if (usableSteam > 0 && unit.HorizontalAdjacentConnections.Any())
			{
				TransferSteamAcross(unit, usableSteam, timeElapsed);
			}
			CalculateUnitHeat(unit);
		}

		private static void CalculateUnitHeat(SteamUnit unit)
		{
			var tempDifference = unit.SteamDensity - (double)unit.Temperature;
			decimal temperature = unit.Temperature + (unit.HeatConductivity * (decimal)(tempDifference / 100));
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

				//unit.NewSteam = steamProduced;
				if (unit.SteamStored + steamProduced > unit.MaxSteam)
				{
					steamProduced = unit.MaxSteam - unit.SteamStored;
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

		private static void TransferSteamAcross(SteamUnit unit, decimal usableSteam, decimal timeElapsed)
		{
			List<SteamUnit> eligibleUnits = new List<SteamUnit>();
			decimal steamSpaceAvailable = 0;
			foreach (var neighbourUnit in unit.HorizontalAdjacentConnections)
			{
				if (unit.FlowSourceUnits.Contains(neighbourUnit))
				{
					continue;
				}
				//Steam providers can always push?
				if (neighbourUnit.SteamStored < neighbourUnit.MaxSteam &&
				    (neighbourUnit.SteamStored < usableSteam || unit is ISteamProvider || neighbourUnit is ISteamConsumer))
				{
					eligibleUnits.Add(neighbourUnit);
					steamSpaceAvailable += (neighbourUnit.MaxSteam - neighbourUnit.SteamStored);
				}
			}

			var originalSteamStored = usableSteam;
			foreach (var neighbourUnit in eligibleUnits)
			{
				var ratio = (neighbourUnit.MaxSteam - neighbourUnit.SteamStored)/steamSpaceAvailable;

				var amountTransferred = originalSteamStored*timeElapsed*ratio;

				if (neighbourUnit.SteamStored + amountTransferred > neighbourUnit.MaxSteam)
				{
					amountTransferred = neighbourUnit.MaxSteam - neighbourUnit.SteamStored;
				}
				//amountTransferred = Math.Ceiling(amountTransferred * TransferRatio);
				amountTransferred = amountTransferred*TransferRatio;
				if (unit.SteamStored - amountTransferred < 0)
				{
					amountTransferred = unit.SteamStored;
				}

				neighbourUnit.SteamStored += amountTransferred;
				neighbourUnit.NewSteam += amountTransferred;
				unit.SteamStored -= amountTransferred;
				neighbourUnit.FlowSourceUnits.Add(unit);
			}
		}

		private static void TransferSteamAbove(SteamUnit unit, decimal usableSteam, decimal timeElapsed)
		{
			if (unit.UnitAbove.SteamStored <= unit.UnitAbove.MaxSteam)
			{
				var amountTransferred = usableSteam*timeElapsed;

				if (unit.UnitAbove.SteamStored + amountTransferred > unit.UnitAbove.MaxSteam)
				{
					amountTransferred = unit.UnitAbove.MaxSteam - unit.UnitAbove.SteamStored;
				}

				//Ignore transfer ratio when going up.
				//amountTransferred = Math.Ceiling(amountTransferred);

				if (usableSteam - amountTransferred < 0)
				{
					amountTransferred = usableSteam;
				}
				unit.UnitAbove.SteamStored += amountTransferred;
				unit.UnitAbove.NewSteam += amountTransferred;
				unit.SteamStored -= amountTransferred;
				unit.UnitAbove.FlowSourceUnits.Add(unit);
			}
		}

		

		public static void StartSimulationThread()
		{
			Thread thread = new Thread(Start) {Name = "Simluation Thread"};
			thread.Start();
		}

		private static bool paused = false;
		private static bool running = true;
		private static void Start()
		{
			_previousTick = Stopwatch.GetTimestamp();
			while (running)
			{
				if (!paused)
				{
					var thisTick = Stopwatch.GetTimestamp();
					var timeElapsed = (thisTick - _previousTick) / (decimal)Stopwatch.Frequency;
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