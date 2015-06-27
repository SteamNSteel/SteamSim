using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace SteamPipes
{
	internal class SteamManager
	{
		private static readonly ConcurrentDictionary<PointI2D, SteamUnit> SteamUnits = new ConcurrentDictionary<PointI2D, SteamUnit>();
		private const decimal TRANSFER_RATIO = 0.5m;

		internal static SteamUnit CreateSteamUnit(int column, int row)
		{
			SteamUnit existingUnit;
			var point = new PointI2D(column, row);
			if (SteamUnits.TryGetValue(point, out existingUnit))
			{
				return existingUnit;
			}
			var steamUnit = new SteamUnit(column, row);
			

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
			SteamUnits.TryRemove(new PointI2D(steamUnit.X, steamUnit.Y), out removedUnit);
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

		private static int currentStep;
		private static long previousTick = Stopwatch.GetTimestamp();

		public static void StepSimulation()
		{
			var thisTick = Stopwatch.GetTimestamp();
			var timeElapsed = (thisTick - previousTick) / (decimal)Stopwatch.Frequency;
			if (timeElapsed > 1) timeElapsed = 1;
			previousTick = thisTick;

			currentStep++;

			HashSet<SteamUnit> unprocessedUnits = new HashSet<SteamUnit>(SteamUnits.Values);
			//Ideally this would be prepopulated with all the sources.
			Queue<SteamUnit> unitsToProcess = new Queue<SteamUnit>();

			while (unprocessedUnits.Count > 0)
			{
				//This would hit if we've found a network with no sources.
				//I could check to make sure that there is steam somewhere in it and disable the network if neccessary.
				if (unitsToProcess.Count == 0)
				{
					var steamUnit = unprocessedUnits.First();
					steamUnit.FlowSourceUnits.Clear();
					unitsToProcess.Enqueue(steamUnit);
				}
				while (unitsToProcess.Count > 0)
				{
					var unit = unitsToProcess.Dequeue();
					lock (unit)
					{
						unprocessedUnits.Remove(unit);
						if (unit.ProcessPass == currentStep) continue; //Already processed.
						
						unit.ProcessPass = currentStep;
						foreach (var connection in unit.AllAdjacentConnections)
						{
							if (connection.ProcessPass != currentStep)
							{
								connection.FlowSourceUnits.Clear();
								unitsToProcess.Enqueue(connection);
							}
						}
						if (unit.SteamStored <= 0) continue;
						//First, work on any units above
						if (unit.UnitAbove != null && !unit.FlowSourceUnits.Contains(unit.UnitAbove))
						{
							if (unit.UnitAbove.SteamStored <= unit.UnitAbove.MaxSteam)
							{
								var amountTransferredUp = Math.Ceiling((unit.SteamStored)*TRANSFER_RATIO*timeElapsed);

								if (unit.UnitAbove.SteamStored + amountTransferredUp > unit.UnitAbove.MaxSteam)
								{
									amountTransferredUp = unit.UnitAbove.MaxSteam - unit.UnitAbove.SteamStored;
								}
								unit.UnitAbove.SteamStored += amountTransferredUp;
								unit.SteamStored -= amountTransferredUp;
								unit.UnitAbove.FlowSourceUnits.Add(unit);
							}
						}
						if (unit.SteamStored > 0 && unit.HorizontalAdjacentConnections.Any())
						{
							List<SteamUnit> eligibleUnits = new List<SteamUnit>();
							decimal steamSpaceAvailable = 0;
							foreach (var neighbourUnit in unit.HorizontalAdjacentConnections)
							{
								if (unit.FlowSourceUnits.Contains(neighbourUnit))
								{
									continue;
								}
								if (neighbourUnit.SteamStored <= neighbourUnit.MaxSteam && neighbourUnit.SteamStored < unit.SteamStored)
								{
									eligibleUnits.Add(neighbourUnit);
									steamSpaceAvailable += (neighbourUnit.MaxSteam - neighbourUnit.SteamStored);
								}
							}

							var originalSteamStored = unit.SteamStored;
							foreach (var neighbourUnit in eligibleUnits)
							{
								var ratio = (neighbourUnit.MaxSteam - neighbourUnit.SteamStored)/steamSpaceAvailable;

								var amountTransferredAcross = Math.Ceiling(originalSteamStored*TRANSFER_RATIO*timeElapsed*ratio);

								if (neighbourUnit.SteamStored + amountTransferredAcross > neighbourUnit.MaxSteam)
								{
									amountTransferredAcross = neighbourUnit.MaxSteam - neighbourUnit.SteamStored;
								}
								neighbourUnit.SteamStored += amountTransferredAcross;
								unit.SteamStored -= amountTransferredAcross;
								neighbourUnit.FlowSourceUnits.Add(unit);
							}
						}
					}
				}
			}
		}

		public static void StartSimulationThread()
		{
			Thread thread = new Thread(Start);
			thread.Name = "Simluation Thread";
			thread.Start();
		}

		private static bool paused = false;
		private static void Start()
		{
			previousTick = Stopwatch.GetTimestamp();
			while (true)
			{
				if (!paused)
				{
					StepSimulation();
					Thread.Sleep(1000/60);
				}
			}
		}
	}
}