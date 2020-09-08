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

		public static int CurrentTick { get; private set; }

		public static void onSteamNSteelInitialized(SteamNSteelInitializedEvent evt)
		{
			SteamTransportRegistry = (SteamTransportRegistry) evt.getSteamTransportRegistry();
			SteamTransportStateMachine = new SteamTransportStateMachine();
			JobManager = new JobManager();
			JobManager.start();
        }

		public static void onTick()
		{
			//TODO: Replace this with the world tick
			CurrentTick++;
			//TODO: Register JobManager for world tick events and handle this there.
			JobManager.doPretickJobs();
			SteamTransportStateMachine.onTick();
        }

		public static void postTick()
		{
			SteamTransportStateMachine.postTick();
		}

		public static void onSteamNSteelShuttingDown()
		{
			JobManager.stop();
		}
	}
}
