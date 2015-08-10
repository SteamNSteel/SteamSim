using System.Collections.Concurrent;
using Steam.API;
using SteamPipes.API;

namespace SteamPipes.Impl
{
    // ReSharper disable SuggestVarOrType_Elsewhere
    // ReSharper disable SuggestVarOrType_SimpleTypes
    // ReSharper disable SuggestVarOrType_BuiltInTypes
    public class SteamManager2 : ISteamManager, ISteamTransportRegistry
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

        internal class SteamTransportLocation
        {
            internal SteamTransportLocation(int x, int y) : this(x, y, 0, 0)
            {
            }

            internal SteamTransportLocation(int x, int y, int z, int worldId)
            {
                X = x;
                Y = y;
                Z = z;
                WorldId = worldId;
            }

            public int X { get; }
            public int Y { get; }
            public int Z { get; }
            public int WorldId { get; }

            protected bool Equals(SteamTransportLocation other)
            {
                return X == other.X && Y == other.Y && Z == other.Z && WorldId == other.WorldId;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = X;
                    hashCode = (hashCode*397) ^ Y;
                    hashCode = (hashCode*397) ^ Z;
                    hashCode = (hashCode*397) ^ WorldId;
                    return hashCode;
                }
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((SteamTransportLocation) obj);
            }
        }
    }
}