namespace SteamNSteel.Impl
{
	internal class SteamTransportTransientData
	{
		private object lockObj = new object();
		private int tickLastUpdated = 0;
		public SteamTransportTransientData(SteamTransport transport)
		{
			this.transport = transport;
			//WaterFlowSourceUnits = new List<SteamTransportTransientData>(6);
			//SteamFlowSourceUnits = new List<SteamTransportTransientData>(6);
		}

		internal void VerifyTick()
		{
			lock (lockObj)
			{
				if (tickLastUpdated != TheMod.CurrentTick)
				{
					PreviousState.SteamStored = transport.GetSteamStored();
					PreviousState.CondensationStored = transport.GetWaterStored();
					PreviousState.Temperature = transport.GetTemperature();
					PreviousState.MaximumCondensation = transport.GetMaximumWater();
					PreviousState.ActualMaximumSteam = SteamMaths.CalculateMaximumSteam(
						PreviousState.CondensationStored,
						transport.GetMaximumWater(), 
						transport.GetMaximumSteam()
					);
					PreviousState.SteamDensity = SteamMaths.CalculateSteamDensity(PreviousState.SteamStored, PreviousState.ActualMaximumSteam);
					CondensationAdded = 0;
					SteamAdded = 0;
					tickLastUpdated = TheMod.CurrentTick;
				}
			}
		}

		private readonly SteamTransport transport;

		public PreviousTransportState PreviousState { get; } = new PreviousTransportState();

		internal class PreviousTransportState
		{
			public double SteamStored { get; internal set; }
			public double Temperature { get; internal set; }
			public double CondensationStored { get; internal set; }
			public double MaximumCondensation { get; internal set; }
			public double ActualMaximumSteam { get; internal set; }
			public double SteamDensity { get; internal set; }
		}

		internal class NewTransportState : SteamTransportTransientData.PreviousTransportState
		{

		}

		//public double newSteam;
		//public double newCondensation;

		//public List<SteamTransportTransientData> WaterFlowSourceUnits { get; set; }
		//public List<SteamTransportTransientData> SteamFlowSourceUnits { get; set; }

		/*internal class StateData : ISteamTransport
		{
			void AddSteam(double unitsOfSteam);

			void AddCondensate(double unitsOfWater);

			double TakeSteam(double desiredUnitsOfSteam);

			double TakeCondensate(double desiredUnitsOfWater);

			double GetSteamStored();
			double GetWaterStored();
			double GetMaximumWater();
		}*/

		public double TakeSteam(double amount)
		{
			var amountTaken = transport.TakeSteam(amount);
			//TODO: subtract from SteamAdded?
			return amountTaken;
		}

		public double TakeCondensate(double amount)
		{
			var amountTaken = transport.TakeCondensate(amount);
			//TODO: subtract from CondensationAdded?
			return amountTaken;
		}

		public void AddCondensate(double waterGained)
		{
			transport.AddCondensate(waterGained);
			CondensationAdded += waterGained;
		}

		public void AddSteam(double amount)
		{
			transport.AddSteam(amount);
			SteamAdded += amount;
		}

		public double CondensationAdded { get; private set; }

		public double SteamAdded { get; private set; }

		public double Temperature
		{
			get { return transport.GetTemperature(); }
			set
			{
				var temperature = value;
				if (temperature > 100)
				{
					temperature = 100;
				}
				if (temperature < 0)
				{
					temperature = 0;
				}
				transport.SetTemperature(temperature);
			}
		}

		public double UsableSteam => PreviousState.SteamStored - SteamAdded;
		public double UsableWater => PreviousState.CondensationStored - CondensationAdded;
		public bool Debug => transport.GetShouldDebug();

		public int TickLastUpdated
		{
			get { return tickLastUpdated; }
		}

		public SteamTransport Transport => transport;
	}
}