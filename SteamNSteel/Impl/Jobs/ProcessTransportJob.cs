using System;
using System.Collections.Generic;
using System.Linq;
using Steam.API;
using SteamNSteel.Jobs;

namespace SteamNSteel.Impl.Jobs
{
	public class ProcessTransportJob : IJob
	{
		internal readonly SteamTransport _transport;
		private readonly INotifyTransportJobComplete _notificationRecipient;
		private readonly List<SteamTransportTransientData> _eligibleTransportData = new List<SteamTransportTransientData>();
		private readonly SteamNSteelConfiguration _config;
		private SteamTransportTransientData[] _horizontalAdjacentTransports;
		private SteamTransportTransientData[] _allAdjacentTransports;
		private SteamTransportTransientData _transportData;
		private SteamTransportTransientData _transportAbove;
		private SteamTransportTransientData _transportBelow;

		internal ProcessTransportJob(SteamTransport transport, INotifyTransportJobComplete notificationRecipient , SteamNSteelConfiguration config)
		{
			_transport = transport;
			_notificationRecipient = notificationRecipient;
			_config = config;
		}

		public void Execute()
		{
			try
			{
				if (_transportData == null || _transport.StructureChanged)
				{
					UpdateLocalData();

					_transport.StructureChanged = false;
				}

				_transportData.VerifyTick();

				TransferSteam();
				CalculateUnitHeat();
				TransferWater();
				CondenseSteam();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}

			_notificationRecipient.JobComplete();
		}

		private void UpdateLocalData()
		{
			var stateMachine = TheMod.SteamTransportStateMachine;
			List<SteamTransportTransientData> adjacentTransports = new List<SteamTransportTransientData>();

			var adjacentTransport = (SteamTransport) _transport.GetAdjacentTransport(EnumFacing.NORTH);
			if (adjacentTransport != null)
			{
				adjacentTransports.Add(stateMachine.GetJobDataForTransport(adjacentTransport));
			}
			adjacentTransport = (SteamTransport) _transport.GetAdjacentTransport(EnumFacing.EAST);
			if (adjacentTransport != null)
			{
				adjacentTransports.Add(stateMachine.GetJobDataForTransport(adjacentTransport));
			}
			adjacentTransport = (SteamTransport) _transport.GetAdjacentTransport(EnumFacing.SOUTH);
			if (adjacentTransport != null)
			{
				adjacentTransports.Add(stateMachine.GetJobDataForTransport(adjacentTransport));
			}
			adjacentTransport = (SteamTransport) _transport.GetAdjacentTransport(EnumFacing.WEST);
			if (adjacentTransport != null)
			{
				adjacentTransports.Add(stateMachine.GetJobDataForTransport(adjacentTransport));
			}
			_horizontalAdjacentTransports = adjacentTransports.ToArray();

			adjacentTransport = (SteamTransport)_transport.GetAdjacentTransport(EnumFacing.UP);
			_transportAbove = adjacentTransport == null ? null : stateMachine.GetJobDataForTransport(adjacentTransport);
			if (_transportAbove != null)
			{
				adjacentTransports.Add(_transportAbove);
			}
			adjacentTransport = (SteamTransport)_transport.GetAdjacentTransport(EnumFacing.DOWN);
			_transportBelow = adjacentTransport == null ? null : stateMachine.GetJobDataForTransport(adjacentTransport);
			if (_transportBelow != null)
			{
				adjacentTransports.Add(_transportBelow);
			}

			_allAdjacentTransports = adjacentTransports.ToArray();
			_transportData = stateMachine.GetJobDataForTransport(_transport);
		}

		private void CondenseSteam()
		{
			var usableSteam = _transportData.PreviousState.SteamStored;

			var newCondensation = usableSteam * _config.CondensationRatePerTick * ((100 - _transportData.PreviousState.Temperature) / 100);
			var takenCondensation = _transportData.TakeSteam(newCondensation);
			var waterGained = takenCondensation * _config.SteamToWaterRatio;
			_transportData.AddCondensate(waterGained);
		}

		private void CalculateUnitHeat()
		{
			var unitTemperature = _transportData.PreviousState.Temperature;
			var tempDifference = _transportData.PreviousState.SteamDensity - unitTemperature;

			var temperature = unitTemperature + (_transport.GetHeatConductivity() * (tempDifference / 100));
			_transportData.Temperature = temperature;
		}

		private void TransferSteam()
		{
			var usableSteam = _transportData.PreviousState.SteamStored;

			if (usableSteam <= 0) return;

			TransferSteam(usableSteam);
		}

		private void TransferSteam(double usableSteam)
		{
			_eligibleTransportData.Clear();
			double steamSpaceAvailable = 0;

			foreach (var neighbourUnit in _allAdjacentTransports)
			{
				//Steam providers can always push?
				var neighbourSteamStored = neighbourUnit.PreviousState.SteamStored;
				var neighbourMaximumSteam = neighbourUnit.PreviousState.ActualMaximumSteam;
				if (neighbourSteamStored < neighbourMaximumSteam && neighbourSteamStored < usableSteam)
				{
					_eligibleTransportData.Add(neighbourUnit);
					steamSpaceAvailable += (neighbourMaximumSteam - neighbourSteamStored);
				}
			}

			var calculatedSteamDensity = _transportData.PreviousState.SteamDensity;
			if (_transportBelow != null && calculatedSteamDensity >= _config.EQUILIBRIUM && _transportBelow.PreviousState.SteamStored < _transportData.PreviousState.SteamStored)
			{
				var neighbourSteamStored = _transportBelow.PreviousState.SteamStored;
				var neighbourMaximumSteam = _transportBelow.PreviousState.ActualMaximumSteam;
				if (neighbourSteamStored < neighbourMaximumSteam && neighbourSteamStored < usableSteam)
				{
					_eligibleTransportData.Add(_transportBelow);
					steamSpaceAvailable += (neighbourMaximumSteam - neighbourSteamStored);
				}
			}

			var originalSteamStored = usableSteam;
			foreach (var neighbourTransport in _eligibleTransportData)
			{
				var neighbourSteamStored = neighbourTransport.PreviousState.SteamStored;
				var neighbourMaximumSteam = neighbourTransport.PreviousState.ActualMaximumSteam;

				var ratio = (neighbourMaximumSteam - neighbourSteamStored) / steamSpaceAvailable;

				var amountTransferred = originalSteamStored * ratio;

				if (neighbourSteamStored + amountTransferred > neighbourMaximumSteam)
				{
					amountTransferred = neighbourMaximumSteam - neighbourSteamStored;
				}

				amountTransferred = amountTransferred * _config.TransferRatio;

				amountTransferred = _transportData.TakeSteam(amountTransferred);

				neighbourTransport.VerifyTick();
				neighbourTransport.AddSteam(amountTransferred);
			}
		}

		private void TransferWater()
		{
			var usableWater = _transportData.PreviousState.CondensationStored;

			if (usableWater <= 0)
			{
				TransferWaterFromHigherPoint();
				return;
			}
			//First, work on any units above
			if (_transportBelow != null)
			{
				TransferWaterBelow(usableWater);
			}

			if (usableWater > 0 && _horizontalAdjacentTransports.Any())
			{
				TransferWaterAcross(usableWater);
			}

			TransferWaterFromHigherPoint();
		}

		private void TransferWaterFromHigherPoint()
		{
			if (_transportData.Debug)
			{
				Console.WriteLine($"HERE! {_transport.GetTransportLocation()}");
			}

			if (_transportBelow == null || !(_transportData.UsableSteam < _transportData.PreviousState.ActualMaximumSteam))
			{
				return;
			}
			var previousTransportState = _transportBelow.PreviousState;
			if (!(Math.Abs(previousTransportState.CondensationStored - previousTransportState.MaximumCondensation) < 100))
			{
				return;
			}

			Stack<SearchData> elementsToSearch = new Stack<SearchData>();
			HashSet<SteamTransportLocation> visitedLocations = new HashSet<SteamTransportLocation>();
			elementsToSearch.Push(new SearchData(_transportBelow.Transport, 1));
			SearchData candidate = null;
			Boolean validScenario = true;
			while (validScenario && elementsToSearch.Any())
			{
				
				var searchData = elementsToSearch.Pop();

				var transport = searchData.Transport;
				var depth = searchData.Depth;
				var steamTransportLocation = transport.GetTransportLocation();
				Console.WriteLine($"Checking transport @ {steamTransportLocation} - {depth} - {_transport.GetShouldDebug()}");
				visitedLocations.Add(steamTransportLocation);
			
				if (depth <= 0 && (candidate == null || depth < candidate.Depth))
				{
					if (searchData.Depth == 0 && searchData.Transport.GetSteamStored() <= transport.GetSteamStored())
					{
						candidate = searchData;
					}
				}

				foreach (var direction in EnumFacing.VALID_DIRECTIONS)
				{
					var adjacentTransport = (SteamTransport)transport.GetAdjacentTransport(direction);
					if (adjacentTransport != null && !visitedLocations.Contains(adjacentTransport.GetTransportLocation()))
					{
						var steamTransportTransientData = TheMod.SteamTransportStateMachine.GetJobDataForTransport(adjacentTransport);
						var nextPreviousData = steamTransportTransientData.PreviousState;

						if ((direction == EnumFacing.EAST || direction == EnumFacing.WEST || direction == EnumFacing.NORTH ||
						     direction == EnumFacing.SOUTH) &&
						    nextPreviousData.CondensationStored < nextPreviousData.MaximumCondensation - 10)
						{
							validScenario = false;
							break;
						}

						if (nextPreviousData.CondensationStored > 10)
						{
							var newDepth = depth + direction.offsetY;
							elementsToSearch.Push(new SearchData(adjacentTransport, newDepth));
						}
					}
					else
					{
						//Console.WriteLine($"Not a Valid location @ {transport.GetTransportLocation().Offset(direction)}");
					}
				}
			}

			if (candidate != null)
			{
				if (candidate.Depth == 0)
				{
					var takeCondensation = candidate.Transport.TakeCondensate(1);
					_transportData.AddCondensate(takeCondensation);
				}
				else
				{
					var takeCondensation = candidate.Transport.TakeCondensate(100);
					_transportData.AddCondensate(takeCondensation);
				}
			}
		}

		private class SearchData
		{
			internal readonly SteamTransport Transport;
			internal readonly int Depth;

			public SearchData(ISteamTransport transport, int depth)
			{
				Transport = (SteamTransport)transport;
				Depth = depth;
			}
		}

		private void TransferWaterAcross(double waterUsedAtStart)
		{
			_eligibleTransportData.Clear();

			if (_horizontalAdjacentTransports.Length == 0)
			{
				return;
			}

			var elementIndex = _transportData.TickLastUpdated%_horizontalAdjacentTransports.Length;
			SteamTransportTransientData nextTransport = _horizontalAdjacentTransports.ElementAt(elementIndex);

			if (nextTransport == null)
			{
				return;
			}
			nextTransport.VerifyTick();

			var neighbourWaterStored = nextTransport.PreviousState.CondensationStored;
			var neighbourMaximumWater = nextTransport.PreviousState.MaximumCondensation;
			if (neighbourWaterStored >= neighbourMaximumWater || !(neighbourWaterStored < waterUsedAtStart))
			{ 
				return;
			}

			var waterStored = _transportData.PreviousState.CondensationStored;
			if (neighbourWaterStored >= waterStored)
			{
				return;
			}

			var desiredTransfer = (waterStored - neighbourWaterStored)/(_horizontalAdjacentTransports.Length + 1);
			foreach (var steamTransportTransientData in _horizontalAdjacentTransports)
			{
				var takeCondensate = _transportData.TakeCondensate(desiredTransfer);
				steamTransportTransientData.AddCondensate(takeCondensate);
			}
		}

		private void TransferWaterBelow(double usableWater)
		{
			var neighbourWaterStored = _transportBelow.PreviousState.CondensationStored;
			var neighbourMaximumWater = _transportBelow.PreviousState.MaximumCondensation;

			if (!(neighbourWaterStored < neighbourMaximumWater)) return;

			var amountTransferred = usableWater;

			if (neighbourWaterStored + amountTransferred > neighbourMaximumWater)
			{
				amountTransferred = neighbourMaximumWater - neighbourWaterStored;
			}

			if (usableWater - amountTransferred < 0)
			{
				amountTransferred = usableWater;
			}

			if (_transportData.Debug)
			{
				Console.WriteLine($"Condensate Transferred {amountTransferred}");
			}

			amountTransferred = _transportData.TakeCondensate(amountTransferred);

			if (_transportData.Debug)
			{
				Console.WriteLine($"Condensate Transferred {amountTransferred}");
			}

			_transportBelow.VerifyTick();
			_transportBelow.AddCondensate(amountTransferred);
		}
	}
}
