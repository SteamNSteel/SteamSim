using System;
using System.Collections.Generic;

namespace SteamNSteel.Impl
{
    internal class SteamTransportTopology
    {
	    private readonly int _topologyGeneration;
		private readonly Object _lockObject = new object();
		List<SteamTransport> _locatedTransports = new List<SteamTransport>(); 

	    protected bool Equals(SteamTransportTopology other)
        {
            return _id.Equals(other._id);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        private readonly Guid _id;

        public SteamTransportTopology(int topologyGeneration)
        {
	        _topologyGeneration = topologyGeneration;
	        _id = Guid.NewGuid();
        }

	    public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }


	    public bool IsSupercededBy(SteamTransportTopology otherTopology)
	    {
		    return _topologyGeneration < otherTopology._topologyGeneration;
	    }

	    public bool IsSameGenerationAs(SteamTransportTopology otherTopology)
	    {
		    return _topologyGeneration == otherTopology._topologyGeneration;
	    }

	    public bool HasPriorityOver(SteamTransportTopology otherTopology)
	    {
		    return _id.CompareTo(otherTopology) > 0;
	    }

	    public void AddTransport(SteamTransport steamTransport)
	    {
			_locatedTransports.Add(steamTransport);
        }

	    public IEnumerable<SteamTransport> GetTransports()
	    {
		    return _locatedTransports;
	    }

	    public void MakeObsolete()
	    {
		    _locatedTransports.Clear();
	    }

	    public object GetLockObject()
	    {
		    return _lockObject; 
	    }
    }
}