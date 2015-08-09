using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steam.Machines
{
    public class BallMillTileEntity : ModTileEntity
    {
        public override void OnTick()
        {
            GetSteamTransport().TakeSteam(2);

        }
    }
}
