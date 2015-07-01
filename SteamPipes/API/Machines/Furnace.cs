namespace SteamPipes.API
{
	public class Furnace : SteamUnit, ISteamConsumer
	{
		public int AmountPerTick
		{
			get { return 5; } // 40 mb/tick
		}
	}
}