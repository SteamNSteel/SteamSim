using System.Collections.Generic;
using System.Diagnostics;
using Steam.API;

namespace SteamNSteel.Impl
{
	internal class SteamTransportTransientData
	{
		private object lockObj = new object();
		private int tickLastUpdated = 0;
		public SteamTransportTransientData(SteamTransport transport)
		{
			this.transport = transport;
			WaterFlowSourceUnits = new List<SteamTransportTransientData>();
			SteamFlowSourceUnits = new List<SteamTransportTransientData>();
		}

		internal void VerifyTick()
		{
			lock (lockObj)
			{
				if (tickLastUpdated != TheMod.CurrentTick)
				{
					newSteam = 0;
					newCondensation = 0;
					WaterFlowSourceUnits.Clear();
					SteamFlowSourceUnits.Clear();
					
				}
			}
			
		}

		public readonly ISteamTransport transport;
		public double newSteam;
		public double newCondensation;
		public List<SteamTransportTransientData> WaterFlowSourceUnits { get; set; }
		public List<SteamTransportTransientData> SteamFlowSourceUnits { get; set; }
	}
}