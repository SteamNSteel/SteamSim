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

		public void execute()
		{
			try
			{
				if (_transportData == null || _transport.StructureChanged)
				{
					updateLocalData();

					_transport.StructureChanged = false;
				}

				_transportData.verifyTick();

				transferSteam();
				calculateUnitHeat();
				transferWater();
				condenseSteam();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}

			_notificationRecipient.jobComplete();
		}

		private void updateLocalData()
		{
			SteamTransportStateMachine stateMachine = TheMod.SteamTransportStateMachine;
			List<SteamTransportTransientData> adjacentTransports = new List<SteamTransportTransientData>();

			SteamTransport adjacentTransport = (SteamTransport) _transport.getAdjacentTransport(EnumFacing.NORTH);
			if (adjacentTransport != null)
			{
				adjacentTransports.Add(stateMachine.getJobDataForTransport(adjacentTransport));
			}
			adjacentTransport = (SteamTransport) _transport.getAdjacentTransport(EnumFacing.EAST);
			if (adjacentTransport != null)
			{
				adjacentTransports.Add(stateMachine.getJobDataForTransport(adjacentTransport));
			}
			adjacentTransport = (SteamTransport) _transport.getAdjacentTransport(EnumFacing.SOUTH);
			if (adjacentTransport != null)
			{
				adjacentTransports.Add(stateMachine.getJobDataForTransport(adjacentTransport));
			}
			adjacentTransport = (SteamTransport) _transport.getAdjacentTransport(EnumFacing.WEST);
			if (adjacentTransport != null)
			{
				adjacentTransports.Add(stateMachine.getJobDataForTransport(adjacentTransport));
			}
			_horizontalAdjacentTransports = adjacentTransports.ToArray();

			adjacentTransport = (SteamTransport)_transport.getAdjacentTransport(EnumFacing.UP);
			_transportAbove = adjacentTransport == null ? null : stateMachine.getJobDataForTransport(adjacentTransport);
			if (_transportAbove != null)
			{
				adjacentTransports.Add(_transportAbove);
			}
			adjacentTransport = (SteamTransport)_transport.getAdjacentTransport(EnumFacing.DOWN);
			_transportBelow = adjacentTransport == null ? null : stateMachine.getJobDataForTransport(adjacentTransport);
			if (_transportBelow != null)
			{
				adjacentTransports.Add(_transportBelow);
			}

			_allAdjacentTransports = adjacentTransports.ToArray();
			_transportData = stateMachine.getJobDataForTransport(_transport);
		}

		private void condenseSteam()
		{
			double usableSteam = _transportData.getPreviousState().SteamStored;

			double newCondensation = usableSteam * _config.CondensationRatePerTick * ((100 - _transportData.getPreviousState().Temperature) / 100);
			double takenCondensation = _transportData.takeSteam(newCondensation);
			double waterGained = takenCondensation * _config.SteamToWaterRatio;
			_transportData.addCondensate(waterGained);
		}

		private void calculateUnitHeat()
		{
			double unitTemperature = _transportData.getPreviousState().Temperature;
			double tempDifference = _transportData.getPreviousState().SteamDensity - unitTemperature;

			double temperature = unitTemperature + (_transport.getHeatConductivity() * (tempDifference / 100));
			_transportData.setTemperature(temperature);
		}

		private void transferSteam()
		{
			double usableSteam = _transportData.getPreviousState().SteamStored;

			if (usableSteam <= 0) return;

			transferSteam(usableSteam);
		}

		private void transferSteam(double usableSteam)
		{
			_eligibleTransportData.Clear();
			double steamSpaceAvailable = 0;

			foreach (SteamTransportTransientData neighbourUnit in _allAdjacentTransports)
			{
				//Steam providers can always push?
				double neighbourSteamStored = neighbourUnit.getPreviousState().SteamStored;
				double neighbourMaximumSteam = neighbourUnit.getPreviousState().ActualMaximumSteam;
				if (neighbourSteamStored < neighbourMaximumSteam && neighbourSteamStored < usableSteam)
				{
					_eligibleTransportData.Add(neighbourUnit);
					steamSpaceAvailable += (neighbourMaximumSteam - neighbourSteamStored);
				}
			}

			double calculatedSteamDensity = _transportData.getPreviousState().SteamDensity;
			if (_transportBelow != null && calculatedSteamDensity >= _config.EQUILIBRIUM && _transportBelow.getPreviousState().SteamStored < _transportData.getPreviousState().SteamStored)
			{
				double neighbourSteamStored = _transportBelow.getPreviousState().SteamStored;
				double neighbourMaximumSteam = _transportBelow.getPreviousState().ActualMaximumSteam;
				if (neighbourSteamStored < neighbourMaximumSteam && neighbourSteamStored < usableSteam)
				{
					_eligibleTransportData.Add(_transportBelow);
					steamSpaceAvailable += (neighbourMaximumSteam - neighbourSteamStored);
				}
			}

			double originalSteamStored = usableSteam;
			foreach (SteamTransportTransientData neighbourTransport in _eligibleTransportData)
			{
				double neighbourSteamStored = neighbourTransport.getPreviousState().SteamStored;
				double neighbourMaximumSteam = neighbourTransport.getPreviousState().ActualMaximumSteam;

				double ratio = (neighbourMaximumSteam - neighbourSteamStored) / steamSpaceAvailable;

				double amountTransferred = originalSteamStored * ratio;

				if (neighbourSteamStored + amountTransferred > neighbourMaximumSteam)
				{
					amountTransferred = neighbourMaximumSteam - neighbourSteamStored;
				}

				amountTransferred = amountTransferred * _config.TransferRatio;

				amountTransferred = _transportData.takeSteam(amountTransferred);

				neighbourTransport.verifyTick();
				neighbourTransport.addSteam(amountTransferred);
			}
		}

		private void transferWater()
		{
			var usableWater = _transportData.getPreviousState().CondensationStored;

			if (usableWater <= 0)
			{
				transferWaterFromHigherPoint();
				return;
			}
			//First, work on any units above
			if (_transportBelow != null)
			{
				transferWaterBelow(usableWater);
			}

			if (usableWater > 0 && _horizontalAdjacentTransports.Any())
			{
				transferWaterAcross(usableWater);
			}

			transferWaterFromHigherPoint();
		}

		private void transferWaterFromHigherPoint()
		{
			if (_transportData.getDebug())
			{
				Console.WriteLine($"HERE! {_transport.getTransportLocation()}");
			}

			if (_transportBelow == null || !(_transportData.getUsableSteam() < _transportData.getPreviousState().ActualMaximumSteam))
			{
				return;
			}
			var previousTransportState = _transportBelow.getPreviousState();
			if (!(Math.Abs(previousTransportState.CondensationStored - previousTransportState.MaximumCondensation) < 100))
			{
				return;
			}

			Stack<SearchData> elementsToSearch = new Stack<SearchData>();
			HashSet<SteamTransportLocation> visitedLocations = new HashSet<SteamTransportLocation>();
			visitedLocations.Add(_transport.getTransportLocation());
			elementsToSearch.Push(new SearchData(_transportBelow.getTransport(), 1));
			SearchData candidate = null;
			Boolean validScenario = true;
			while (validScenario && elementsToSearch.Any())
			{
				SearchData searchData = elementsToSearch.Pop();

				SteamTransport transport = searchData.Transport;
				int depth = searchData.Depth;
				SteamTransportLocation steamTransportLocation = transport.getTransportLocation();
				Console.WriteLine($"Checking transport @ {steamTransportLocation} - {depth} - {_transport.getShouldDebug()}");
				visitedLocations.Add(steamTransportLocation);
			
				if (depth <= 0 && (candidate == null || depth < candidate.Depth))
				{
					if (searchData.Depth < 0 || (searchData.Depth == 0 && searchData.Transport.getWaterStored() >= (_transport.getWaterStored() + 5)))
					{
						candidate = searchData;
					}
				}

				foreach (EnumFacing direction in EnumFacing.VALID_DIRECTIONS)
				{
					SteamTransport adjacentTransport = (SteamTransport)transport.getAdjacentTransport(direction);
					if (adjacentTransport != null && !visitedLocations.Contains(adjacentTransport.getTransportLocation()))
					{
						SteamTransportTransientData steamTransportTransientData = TheMod.SteamTransportStateMachine.getJobDataForTransport(adjacentTransport);
						SteamTransportTransientData.PreviousTransportState nextPreviousData = steamTransportTransientData.getPreviousState();

						if ((direction == EnumFacing.EAST || direction == EnumFacing.WEST || direction == EnumFacing.NORTH ||
						     direction == EnumFacing.SOUTH) &&
						    nextPreviousData.CondensationStored < nextPreviousData.MaximumCondensation - 10)
						{
							validScenario = false;
							break;
						}

						if (nextPreviousData.CondensationStored > 10)
						{
							int newDepth = depth + direction.offsetY;
							elementsToSearch.Push(new SearchData(adjacentTransport, newDepth));
						}
					}
				}
			}

			if (candidate != null)
			{
				Console.WriteLine($"Updating from candidate {candidate.Transport.getTransportLocation()} - {candidate.Depth}");
				double condensate;
				if (candidate.Depth == 0)
				{
					condensate = candidate.Transport.getWaterStored() / 2;
					if (condensate > 100)
					{
						condensate = 100;
					}
				}
				else
				{
					condensate = 100;
				}
				
				double actualCondensate = candidate.Transport.takeCondensate(condensate);
				_transportData.addCondensate(actualCondensate);
			}
		}

		private void transferWaterAcross(double waterUsedAtStart)
		{
			_eligibleTransportData.Clear();

			if (_horizontalAdjacentTransports.Length == 0)
			{
				return;
			}

			var elementIndex = _transportData.getTickLastUpdated()%_horizontalAdjacentTransports.Length;
			SteamTransportTransientData nextTransport = _horizontalAdjacentTransports.ElementAt(elementIndex);

			if (nextTransport == null)
			{
				return;
			}
			nextTransport.verifyTick();

			double neighbourWaterStored = nextTransport.getPreviousState().CondensationStored;
			double neighbourMaximumWater = nextTransport.getPreviousState().MaximumCondensation;
			if (neighbourWaterStored >= neighbourMaximumWater || !(neighbourWaterStored < waterUsedAtStart))
			{ 
				return;
			}

			double waterStored = _transportData.getPreviousState().CondensationStored;
			if (neighbourWaterStored >= waterStored)
			{
				return;
			}

			double desiredTransfer = (waterStored - neighbourWaterStored)/(_horizontalAdjacentTransports.Length + 1);
			foreach (SteamTransportTransientData steamTransportTransientData in _horizontalAdjacentTransports)
			{
				double takeCondensate = _transportData.takeCondensate(desiredTransfer);
				steamTransportTransientData.addCondensate(takeCondensate);
			}
		}

		private void transferWaterBelow(double usableWater)
		{
			double neighbourWaterStored = _transportBelow.getPreviousState().CondensationStored;
			double neighbourMaximumWater = _transportBelow.getPreviousState().MaximumCondensation;

			if (!(neighbourWaterStored < neighbourMaximumWater)) return;

			double amountTransferred = usableWater;

			if (neighbourWaterStored + amountTransferred > neighbourMaximumWater)
			{
				amountTransferred = neighbourMaximumWater - neighbourWaterStored;
			}

			if (usableWater - amountTransferred < 0)
			{
				amountTransferred = usableWater;
			}

			amountTransferred = _transportData.takeCondensate(amountTransferred);

			_transportBelow.verifyTick();
			_transportBelow.addCondensate(amountTransferred);
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
	}


}
