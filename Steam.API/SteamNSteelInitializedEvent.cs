using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Steam.API
{
    public class SteamNSteelInitializedEvent
    {
        private readonly ISteamTransportRegistry _steamTransportRegistry;

        public SteamNSteelInitializedEvent(ISteamTransportRegistry steamTransportRegistry)
        {
            _steamTransportRegistry = steamTransportRegistry;
        }

        public ISteamTransportRegistry GetSteamTransportRegistry()
        {
            return _steamTransportRegistry;
        }
    }
}
