namespace SteamPipes.Impl
{
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