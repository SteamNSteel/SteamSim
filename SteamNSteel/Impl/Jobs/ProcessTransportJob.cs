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

			var adjacentTransport = (SteamTransport) _transport.GetAdjacentTransport(ForgeDirection.NORTH);
			if (adjacentTransport != null)
			{
				adjacentTransports.Add(stateMachine.GetJobDataForTransport(adjacentTransport));
			}
			adjacentTransport = (SteamTransport) _transport.GetAdjacentTransport(ForgeDirection.EAST);
			if (adjacentTransport != null)
			{
				adjacentTransports.Add(stateMachine.GetJobDataForTransport(adjacentTransport));
			}
			adjacentTransport = (SteamTransport) _transport.GetAdjacentTransport(ForgeDirection.SOUTH);
			if (adjacentTransport != null)
			{
				adjacentTransports.Add(stateMachine.GetJobDataForTransport(adjacentTransport));
			}
			adjacentTransport = (SteamTransport) _transport.GetAdjacentTransport(ForgeDirection.WEST);
			if (adjacentTransport != null)
			{
				adjacentTransports.Add(stateMachine.GetJobDataForTransport(adjacentTransport));
			}
			_horizontalAdjacentTransports = adjacentTransports.ToArray();


			adjacentTransport = (SteamTransport)_transport.GetAdjacentTransport(ForgeDirection.UP);
			if (adjacentTransport != null)
			{
				adjacentTransports.Add(stateMachine.GetJobDataForTransport(adjacentTransport));
			}
			adjacentTransport = (SteamTransport)_transport.GetAdjacentTransport(ForgeDirection.DOWN);
			if (adjacentTransport != null)
			{
				adjacentTransports.Add(stateMachine.GetJobDataForTransport(adjacentTransport));
			}
			_allAdjacentTransports = adjacentTransports.ToArray();

			adjacentTransport = (SteamTransport) _transport.GetAdjacentTransport(ForgeDirection.UP);
			_transportAbove = adjacentTransport == null ? null : stateMachine.GetJobDataForTransport(adjacentTransport);
			adjacentTransport = (SteamTransport) _transport.GetAdjacentTransport(ForgeDirection.DOWN);
			_transportBelow = adjacentTransport == null ? null : stateMachine.GetJobDataForTransport(adjacentTransport);
			
			_transportData = stateMachine.GetJobDataForTransport(_transport);
		}

		private void CondenseSteam()
		{
			var steamStored = _transport.GetSteamStored();

			var usableSteam = steamStored - _transportData.newSteam;
			if (usableSteam <= 0) return;

			var newCondensation = usableSteam * _config.CondensationRatePerTick * ((100 - _transport.GetTemperature()) / 100);
			newCondensation = _transport.TakeSteam(newCondensation);
			var waterGained = newCondensation * _config.SteamToWaterRatio;
			_transport.AddCondensate(waterGained);
			_transportData.newCondensation += waterGained;
		}

		private void CalculateUnitHeat()
		{
			var unitTemperature = _transport.GetTemperature();
			var tempDifference = _transport.GetCalculatedSteamDensity() - unitTemperature;
			var temperature = unitTemperature + (_transport.GetHeatConductivity() * (tempDifference / 100));
			if (temperature > 100)
			{
				temperature = 100;
			}
			if (temperature < 0)
			{
				temperature = 0;
			}
			_transport.SetTemperature(temperature);
		}

		private void TransferSteam()
		{
			var usableSteam = _transport.GetSteamStored() - _transportData.newSteam;

			if (usableSteam <= 0) return;

			TransferSteam(usableSteam);
		}

		private void TransferSteam(double usableSteam)
		{
			_eligibleTransportData.Clear();
			double steamSpaceAvailable = 0;

			foreach (var neighbourUnit in _allAdjacentTransports)
			{
				if (_transportData.SteamFlowSourceUnits.Contains(neighbourUnit))
				{
					continue;
				}

				//Steam providers can always push?
				var neighbourSteamStored = neighbourUnit.transport.GetSteamStored();
				var neighbourMaximumSteam = neighbourUnit.transport.GetCalculatedMaximumSteam();
				if (neighbourSteamStored < neighbourMaximumSteam && neighbourSteamStored < usableSteam)
				{
					_eligibleTransportData.Add(neighbourUnit);
					steamSpaceAvailable += (neighbourMaximumSteam - neighbourSteamStored);
				}
			}

			if (_transportBelow != null && _transport.GetCalculatedSteamDensity() >= _config.EQUILIBRIUM && !_transportData.SteamFlowSourceUnits.Contains(_transportBelow))
			{
				var neighbourSteamStored = _transportBelow.transport.GetSteamStored();
				var neighbourMaximumSteam = _transportBelow.transport.GetCalculatedMaximumSteam();
				if (neighbourSteamStored < neighbourMaximumSteam && neighbourSteamStored < usableSteam)
				{
					_eligibleTransportData.Add(_transportBelow);
					steamSpaceAvailable += (neighbourMaximumSteam - neighbourSteamStored);
				}
			}

			var originalSteamStored = usableSteam;
			foreach (var neighbourTransport in _eligibleTransportData)
			{
				var neighbourSteamStored = neighbourTransport.transport.GetSteamStored();
				var neighbourMaximumSteam = neighbourTransport.transport.GetCalculatedMaximumSteam();
				var ratio = (neighbourMaximumSteam - neighbourSteamStored) / steamSpaceAvailable;

				var amountTransferred = originalSteamStored * ratio;

				if (neighbourSteamStored + amountTransferred > neighbourMaximumSteam)
				{
					amountTransferred = neighbourMaximumSteam - neighbourSteamStored;
				}

				amountTransferred = amountTransferred * _config.TransferRatio;

				amountTransferred = _transport.TakeSteam(amountTransferred);

				neighbourTransport.VerifyTick();
				neighbourTransport.transport.AddSteam(amountTransferred);
				neighbourTransport.newSteam += amountTransferred;
				neighbourTransport.SteamFlowSourceUnits.Add(_transportData);
			}
		}

		private void TransferWater()
		{
			var usableWater = _transport.GetWaterStored() - _transportData.newCondensation;

			if (usableWater <= 0) return;
			//First, work on any units above
			if (_transportBelow != null && !_transportData.WaterFlowSourceUnits.Contains(_transportBelow))
			{
				TransferWaterBelow(usableWater);
			}

			if (usableWater > 0 && _horizontalAdjacentTransports.Any())
			{
				TransferWaterAcross(usableWater);
			}
		}

		private void TransferWaterAcross(double waterUsedAtStart)
		{
			_eligibleTransportData.Clear();
			double waterSpaceAvailable = 0;

			Random r = new Random();
			var next = r.Next(_horizontalAdjacentTransports.Length);
			var nextTransport = _horizontalAdjacentTransports.ElementAt(next);

			var neighbourWaterStored = nextTransport.transport.GetWaterStored();
			var neighbourMaximumWater = nextTransport.transport.GetMaximumWater();
			if (neighbourWaterStored >= neighbourMaximumWater || !(neighbourWaterStored < waterUsedAtStart))
			{ 
				return;
			}

			var waterStored = _transportData.transport.GetWaterStored();
			if (neighbourWaterStored >= waterStored)
			{
				return;
			}

			var desiredTransfer = (waterStored - neighbourWaterStored)/_horizontalAdjacentTransports.Length;
			foreach (var steamTransportTransientData in _horizontalAdjacentTransports)
			{
				var takeCondensate = _transport.TakeCondensate(desiredTransfer);
				steamTransportTransientData.transport.AddCondensate(takeCondensate);
			}



			/*foreach (var neighbourUnit in _horizontalAdjacentTransports)
			{
				if (_transportData.WaterFlowSourceUnits.Contains(neighbourUnit))
				{
					continue;
				}

				var neighbourWaterStored = neighbourUnit.transport.GetWaterStored();
				var neighbourMaximumWater = neighbourUnit.transport.GetMaximumWater();
				if (neighbourWaterStored < neighbourMaximumWater && neighbourWaterStored < waterUsedAtStart)
				{
					_eligibleTransportData.Add(neighbourUnit);
					waterSpaceAvailable += (neighbourMaximumWater - neighbourWaterStored);
				}
			}

			var originalWaterStored = waterUsedAtStart;
			foreach (var neighbourUnit in _eligibleTransportData)
			{
				var neighbourWaterStored = neighbourUnit.transport.GetWaterStored();
				var neighbourMaximumWater = neighbourUnit.transport.GetMaximumWater();

				var ratio = (neighbourMaximumWater - neighbourWaterStored) / waterSpaceAvailable;

				var amountTransferred = originalWaterStored * ratio;

				if (neighbourWaterStored + amountTransferred > neighbourMaximumWater)
				{
					amountTransferred = neighbourMaximumWater - neighbourWaterStored;
				}

				var amountTransferred2 = _transport.TakeCondensate(amountTransferred);
				
				neighbourUnit.VerifyTick();
				neighbourUnit.transport.AddCondensate(amountTransferred2);
				neighbourUnit.newCondensation += amountTransferred2;
				neighbourUnit.WaterFlowSourceUnits.Add(_transportData);
			}*/
		}

		private void TransferWaterBelow(double usableWater)
		{
			var neighbourWaterStored = _transportBelow.transport.GetWaterStored();
			var neighbourMaximumWater = _transportBelow.transport.GetMaximumWater();

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

			if (_transport.GetShouldDebug())
			{
				Console.WriteLine($"Condensate Transferred {amountTransferred}");
			}

			amountTransferred = _transport.TakeCondensate( amountTransferred);

			if (_transport.GetShouldDebug())
			{
				Console.WriteLine($"Condensate Transferred {amountTransferred}");
			}

			_transportBelow.VerifyTick();
			_transportBelow.transport.AddCondensate(amountTransferred);
			_transportBelow.newCondensation += amountTransferred;
			_transportBelow.WaterFlowSourceUnits.Add(_transportData);
		}
	}
}
