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

        public ISteamTransport RegisterSteamTransport(int x, int y, ForgeDirection[] initialAllowedDirections)
        {
			SteamTransportLocation steamTransportLocation = SteamTransportLocation.Create(x, y);
            SteamTransport result = _steamTransports.GetOrAdd(steamTransportLocation, new SteamTransport(steamTransportLocation));

			bool[] allowedDirections = new bool[6];

			foreach (ForgeDirection initialAllowedDirection in initialAllowedDirections)
			{
				allowedDirections[(int)initialAllowedDirection] = true;
			}

	        foreach (ForgeDirection direction in ForgeDirection.VALID_DIRECTIONS)
	        {
		        bool canConnect = allowedDirections[(int) direction];
		        result.SetCanConnect(direction, canConnect);
	        }

	        TheMod.SteamTransportStateMachine.AddTransport(result);
			return result;
        }

        public void DestroySteamTransport(int x, int y)
        {
            SteamTransport transport;
            var steamTransportLocation = SteamTransportLocation.Create(x, y);

	        if (_steamTransports.TryRemove(steamTransportLocation, out transport))
	        {
		        TheMod.SteamTransportStateMachine.RemoveTransport(transport);
	        }
        }

		public ISteamTransport GetSteamTransportAtLocation(SteamTransportLocation steamTransportLocation)
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