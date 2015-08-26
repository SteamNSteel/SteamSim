using System;

namespace SteamPipes.Impl
{
    internal class SteamTransportTopology
    {
        protected bool Equals(SteamTransportTopology other)
        {
            return _id.Equals(other._id);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        private readonly Guid _id;

        public SteamTransportTopology()
        {
            _id = Guid.NewGuid();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
    }
}