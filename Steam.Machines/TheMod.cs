using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Steam.API;

namespace Steam.Machines
{
    class TheMod
    {
        internal static ISteamTransportRegistry SteamTransportRegistry;

        public void OnSteamNSteelInitialized(SteamNSteelInitializedEvent evt)
        {
            SteamTransportRegistry = evt.GetSteamTransportRegistry();
        }
    }
}
