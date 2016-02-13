using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steam.Machines
{
    public class FurnaceTileEntity : ModTileEntity
    {
        public FurnaceTileEntity()
        {
            
        }

        public override void OnTick()
        {
            GetSteamTransport().takeSteam(5);
            base.OnTick();
        }
    }
}
