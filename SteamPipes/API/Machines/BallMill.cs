using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamPipes.API
{
	public class BallMill : SteamUnit, ISteamConsumer
	{
		public int AmountPerTick
		{
			get { return 2; } // 40 mb/tick
		}
	}
}
