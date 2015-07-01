using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamPipes.API
{
	public class Boiler : SteamUnit, ISteamProvider
	{
		public int AmountPerTick
		{
			get { return 10; } // 40 mb/tick
		}
	}
}
