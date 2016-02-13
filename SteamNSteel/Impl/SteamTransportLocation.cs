using Steam.API;

namespace SteamNSteel.Impl
{
    public class SteamTransportLocation
    {
        private SteamTransportLocation(int x, int y, int z, int worldId)
        {
            X = x;
            Y = y;
            Z = z;
            WorldId = worldId;
        }

		public static SteamTransportLocation create(int x, int y)
		{
			return new SteamTransportLocation(x, y, 0, 0);
		}

		public static SteamTransportLocation create(int x, int y, int z, int worldId)
	    {
		    return new SteamTransportLocation(x, y, z, worldId);
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

	    public SteamTransportLocation offset(EnumFacing direction)
	    {
			//Fixme: Use a pool?
		    return new SteamTransportLocation(X + direction.offsetX, Y + direction.offsetY, Z + direction.offsetZ, WorldId);
	    }

	    public override string ToString()
	    {
		    return $"STL [{X}, {Y}, {Z}]";
	    }
    }
}