using SteamPipes.API;

namespace SteamPipes.Machines
{
	public class BallMill : SteamUnit, ISteamConsumer
	{
		public int AmountPerTick
		{
			get { return 2; } // 40 mb/tick
		}
	}
}
