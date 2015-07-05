using SteamPipes.API;

namespace SteamPipes.Machines
{
	public class Boiler : SteamUnit, ISteamProvider
	{
		public int AmountPerTick
		{
			get { return 10; } // 40 mb/tick
		}
	}
}