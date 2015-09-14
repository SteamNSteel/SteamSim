namespace SteamNSteel.Impl.Jobs
{
	public class SteamNSteelConfiguration
	{
		public double TransferRatio = 0.8;
		public double SteamToWaterRatio = 0.8;
		public double CondensationRatePerTick = 0.01;
		public double TickLength = 1 / 20.0;

		public double EQUILIBRIUM = 75;
	}
}