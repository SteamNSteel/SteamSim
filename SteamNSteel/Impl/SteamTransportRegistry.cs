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
        private readonly ConcurrentDictionary<SteamTransportLocation, SteamTransport> SteamUnits =
            new ConcurrentDictionary<SteamTransportLocation, SteamTransport>();

        public ISteamTransport RegisterSteamTransport(int x, int y, ForgeDirection[] initialAllowedDirections)
        {
            SteamTransportLocation steamTransportLocation = SteamTransportLocation.Create(x, y);
            SteamTransport result = SteamUnits.GetOrAdd(steamTransportLocation, new SteamTransport(steamTransportLocation));
            
            bool[] allowedDirections = new bool[6];
            
            foreach (ForgeDirection initialAllowedDirection in initialAllowedDirections)
            {
                allowedDirections[(int) initialAllowedDirection] = true;
            }

            //Should this be a job?
            foreach (ForgeDirection direction in ForgeDirection.VALID_DIRECTIONS)
            {
                bool canConnect = allowedDirections[(int) direction];
                result.SetCanConnect(direction, canConnect);

                if (!canConnect) continue;

	            SteamTransportLocation altSteamTransportLocation = steamTransportLocation.Offset(direction);

                SteamTransport foundTransport;
                if (!SteamUnits.TryGetValue(altSteamTransportLocation, out foundTransport)) continue;

                ForgeDirection oppositeDirection = direction.getOpposite();
                if (!foundTransport.CanConnect(oppositeDirection)) continue;
				
                result.SetAdjacentTransport(direction, foundTransport);
                foundTransport.SetAdjacentTransport(oppositeDirection, result);
            }

            return result;
        }

        public void DestroySteamTransport(int x, int y)
        {
            SteamTransport transport;
            var steamTransportLocation = SteamTransportLocation.Create(x, y);
            SteamUnits.TryRemove(steamTransportLocation, out transport);

            foreach (ForgeDirection direction in ForgeDirection.VALID_DIRECTIONS)
            {
                SteamTransport adjacentTransport = (SteamTransport)transport.GetAdjacentTransport(direction);
                if (adjacentTransport == null) continue;

                adjacentTransport.SetAdjacentTransport(direction.getOpposite(), null);
            }
        }

		public ISteamTransport GetSteamTransportAtLocation(SteamTransportLocation steamTransportLocation)
		{
			SteamTransport value;
			if (SteamUnits.TryGetValue(steamTransportLocation, out value))
			{
				return value;
			}
			return null;
		}
	}
}