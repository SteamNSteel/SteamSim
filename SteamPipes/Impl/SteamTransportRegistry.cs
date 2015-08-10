using System.Collections.Concurrent;
using Steam.API;
using SteamPipes.API;

namespace SteamPipes.Impl
{
    // ReSharper disable SuggestVarOrType_Elsewhere
    // ReSharper disable SuggestVarOrType_SimpleTypes
    // ReSharper disable SuggestVarOrType_BuiltInTypes  
    public class SteamTransportRegistry : ISteamTransportRegistry
    {
        private static readonly ConcurrentDictionary<SteamTransportLocation, SteamTransport> SteamUnits =
            new ConcurrentDictionary<SteamTransportLocation, SteamTransport>();

        public ISteamTransport RegisterSteamTransport(int x, int y, ForgeDirection[] initialAllowedDirections)
        {
            SteamTransportLocation steamTransportLocation = new SteamTransportLocation(x, y);
            SteamTransport result = SteamUnits.GetOrAdd(steamTransportLocation, new SteamTransport());
            
            bool[] allowedDirections = new bool[6];
            
            foreach (ForgeDirection initialAllowedDirection in initialAllowedDirections)
            {
                allowedDirections[(int) initialAllowedDirection] = true;
            }

            foreach (ForgeDirection direction in ForgeDirection.VALID_DIRECTIONS)
            {
                bool canConnect = allowedDirections[(int) direction];
                result.SetCanConnect(direction, canConnect);

                if (!canConnect) continue;

                SteamTransportLocation altSteamTransportLocation = new SteamTransportLocation(
                    steamTransportLocation.X + direction.offsetX,
                    steamTransportLocation.Y - direction.offsetY,
                    steamTransportLocation.Z + direction.offsetZ,
                    steamTransportLocation.WorldId
                    );

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
            SteamUnits.TryRemove(new SteamTransportLocation(x, y), out transport);

            foreach (ForgeDirection direction in ForgeDirection.VALID_DIRECTIONS)
            {
                SteamTransport adjacentTransport = (SteamTransport)transport.GetAdjacentTransport(direction);
                adjacentTransport?.SetAdjacentTransport(direction.getOpposite(), null);
            }
        }
    }
}