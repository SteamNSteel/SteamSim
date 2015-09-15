using System.Collections.Generic;
using Steam.API;

namespace SteamNSteel.Impl
{
	internal class SteamTransportTransientData
	{
		public SteamTransportTransientData(SteamTransport transport)
		{
			this.transport = transport;
		}

		public ISteamTransport transport;
		public int newSteam;
		public int newCondensation;
		public List<SteamTransportTransientData> WaterFlowSourceUnits { get; set; }
		public List<SteamTransportTransientData> SteamFlowSourceUnits { get; set; }
	}
}