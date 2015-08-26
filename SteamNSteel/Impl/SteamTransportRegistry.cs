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

        private readonly List<SteamTransportTopology> ActiveTopologies = new List<SteamTransportTopology>();
        private readonly List<SteamTransportLocation> PendingTopologyChanges = new List<SteamTransportLocation>();

        public ISteamTransport RegisterSteamTransport(int x, int y, ForgeDirection[] initialAllowedDirections)
        {
            SteamTransportLocation steamTransportLocation = new SteamTransportLocation(x, y);
            SteamTransport result = SteamUnits.GetOrAdd(steamTransportLocation, new SteamTransport(steamTransportLocation));
            
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
                
                ActiveTopologies.Remove(foundTransport.GetTopology());
                PendingTopologyChanges.Add(foundTransport.GetTransportLocation());

                result.SetAdjacentTransport(direction, foundTransport);
                foundTransport.SetAdjacentTransport(oppositeDirection, result);
            }

            this.PendingTopologyChanges.Add(steamTransportLocation);

            return result;
        }

        public void DestroySteamTransport(int x, int y)
        {
            SteamTransport transport;
            var steamTransportLocation = new SteamTransportLocation(x, y);
            SteamUnits.TryRemove(steamTransportLocation, out transport);

            ActiveTopologies.Remove(transport.GetTopology());

            foreach (ForgeDirection direction in ForgeDirection.VALID_DIRECTIONS)
            {
                SteamTransport adjacentTransport = (SteamTransport)transport.GetAdjacentTransport(direction);
                if (adjacentTransport == null) continue;

                ActiveTopologies.Remove(adjacentTransport.GetTopology());
                PendingTopologyChanges.Add(adjacentTransport.GetTransportLocation());
                adjacentTransport.SetAdjacentTransport(direction.getOpposite(), null);
            }
        }
    }
}