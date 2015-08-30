using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Steam.API;
using SteamNSteel.Impl;

namespace SteamNSteel
{
	public class TheMod
	{
		internal static SteamTransportRegistry SteamTransportRegistry;

		public static void OnSteamNSteelInitialized(SteamNSteelInitializedEvent evt)
		{
			SteamTransportRegistry = (SteamTransportRegistry) evt.GetSteamTransportRegistry();
		}
	}
}
