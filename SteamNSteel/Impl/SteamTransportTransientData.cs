namespace SteamNSteel.Impl
{
	internal class SteamTransportTransientData
	{
		private object lockObj = new object();
		private int tickLastUpdated = 0;
		public SteamTransportTransientData(SteamTransport transport)
		{
			this.transport = transport;
		}

		internal void verifyTick()
		{
			lock (lockObj)
			{
				if (tickLastUpdated != TheMod.CurrentTick)
				{
					_previousState.SteamStored = transport.getSteamStored();
					_previousState.CondensationStored = transport.getWaterStored();
					_previousState.Temperature = transport.getTemperature();
					_previousState.MaximumCondensation = transport.getMaximumWater();
					_previousState.ActualMaximumSteam = SteamMaths.calculateMaximumSteam(
						_previousState.CondensationStored,
						transport.getMaximumWater(), 
						transport.getMaximumSteam()
					);
					_previousState.SteamDensity = SteamMaths.calculateSteamDensity(_previousState.SteamStored, _previousState.ActualMaximumSteam);
					_condensationAdded = 0;
					_steamAdded = 0;
					tickLastUpdated = TheMod.CurrentTick;
				}
			}
		}

		private readonly SteamTransport transport;
		private double _condensationAdded;
		private double _steamAdded;
		private readonly PreviousTransportState _previousState = new PreviousTransportState();

		public PreviousTransportState getPreviousState()
		{
			return _previousState;
		}

		internal class PreviousTransportState
		{
			public double SteamStored { get; internal set; }
			public double Temperature { get; internal set; }
			public double CondensationStored { get; internal set; }
			public double MaximumCondensation { get; internal set; }
			public double ActualMaximumSteam { get; internal set; }
			public double SteamDensity { get; internal set; }
		}

		public double takeSteam(double amount)
		{
			double amountTaken = transport.takeSteam(amount);
			//TODO: subtract from SteamAdded?
			return amountTaken;
		}

		public double takeCondensate(double amount)
		{
			double amountTaken = transport.takeCondensate(amount);
			//TODO: subtract from CondensationAdded?
			return amountTaken;
		}

		public void addCondensate(double waterGained)
		{
			transport.addCondensate(waterGained);
			_condensationAdded += waterGained;
		}

		public void addSteam(double amount)
		{
			transport.addSteam(amount);
			_steamAdded += amount;
		}

		public double getCondensationAdded()
		{
			return _condensationAdded;
		}

		public double getSteamAdded()
		{
			return _steamAdded;
		}

		public double getTemperature()
		{
				return transport.getTemperature();
		}

		public void setTemperature(double value) { 
			double temperature = value;
			if (temperature > 100)
			{
				temperature = 100;
			}
			if (temperature < 0)
			{
				temperature = 0;
			}
			transport.setTemperature(temperature);
		}

		public double getUsableSteam()
		{
			return _previousState.SteamStored - _steamAdded;
		}

		public double getUsableWater()
		{
			return _previousState.CondensationStored - _condensationAdded;
		}

		public bool getDebug()
		{
			return transport.getShouldDebug();
		}

		public int getTickLastUpdated()
		{
			return tickLastUpdated;
		}

		public SteamTransport getTransport()
		{
			return transport;
		}
	}
}