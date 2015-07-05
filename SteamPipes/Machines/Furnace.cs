using SteamPipes.API;

namespace SteamPipes.Machines
{
	public class Furnace : SteamUnit, ISteamConsumer
	{
		public int AmountPerTick
		{
			get { return 5; } // 40 mb/tick
		}
	}
}