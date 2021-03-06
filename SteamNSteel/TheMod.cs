﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Steam.API;
using SteamNSteel.Impl;
using SteamNSteel.Jobs;

namespace SteamNSteel
{
	public class TheMod
	{
		internal static SteamTransportRegistry SteamTransportRegistry;
		internal static SteamTransportStateMachine SteamTransportStateMachine;
		internal static JobManager JobManager;

		public static void OnSteamNSteelInitialized(SteamNSteelInitializedEvent evt)
		{
			SteamTransportRegistry = (SteamTransportRegistry) evt.GetSteamTransportRegistry();
			SteamTransportStateMachine = new SteamTransportStateMachine();
			JobManager = new JobManager();
			JobManager.Start();
        }

		public static void OnTick()
		{
			SteamTransportStateMachine.OnTick();
        }

		public static void PostTick()
		{
			SteamTransportStateMachine.PostTick();
		}
	}
}
