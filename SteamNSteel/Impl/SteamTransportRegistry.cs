using System.Collections.Concurrent;
using System.Collections.Generic;
using Steam.API;

namespace SteamNSteel.Impl
{
    // ReSharper disable SuggestVarOrType_Elsewhere
    // ReSharper disable SuggestVarOrType_SimpleTypes
    // ReSharper disable SuggestVarOrType_BuiltInTypes  
    public class SteamTransportRegistry : ISteamTransportRegistry
    {
        private readonly ConcurrentDictionary<SteamTransportLocation, SteamTransport> _steamTransports =
            new ConcurrentDictionary<SteamTransportLocation, SteamTransport>();

        public ISteamTransport registerSteamTransport(int x, int y, EnumFacing[] initialAllowedDirections)
        {
			SteamTransportLocation steamTransportLocation = SteamTransportLocation.create(x, y);
            SteamTransport result = _steamTransports.GetOrAdd(steamTransportLocation, new SteamTransport(steamTransportLocation));

			bool[] allowedDirections = new bool[6];

			foreach (EnumFacing initialAllowedDirection in initialAllowedDirections)
			{
				allowedDirections[(int)initialAllowedDirection] = true;
			}

	        foreach (EnumFacing direction in EnumFacing.VALID_DIRECTIONS)
	        {
		        bool canConnect = allowedDirections[(int) direction];
		        result.setCanConnect(direction, canConnect);
	        }

	        TheMod.SteamTransportStateMachine.AddTransport(result);
			return result;
        }

        public void destroySteamTransport(int x, int y)
        {
            SteamTransport transport;
            var steamTransportLocation = SteamTransportLocation.create(x, y);

	        if (_steamTransports.TryRemove(steamTransportLocation, out transport))
	        {
		        TheMod.SteamTransportStateMachine.RemoveTransport(transport);
	        }
        }

		public ISteamTransport getSteamTransportAtLocation(SteamTransportLocation steamTransportLocation)
		{
			SteamTransport value;
			if (_steamTransports.TryGetValue(steamTransportLocation, out value))
			{
				return value;
			}
			return null;
		}
	}
}