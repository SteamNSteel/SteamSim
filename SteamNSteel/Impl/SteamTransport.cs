using System;
using Steam.API;

namespace SteamNSteel.Impl
{
    class SteamTransport : ISteamTransport
    {
        private readonly SteamTransportLocation _steamTransportLocation;

	    internal SteamTransport(SteamTransportLocation steamTransportLocation)
        {
            _steamTransportLocation = steamTransportLocation;
            _maximumSteam = 1000;
            _maximumWater = 800;
        }

        private double _waterStored = 0;
        private double _steamStored = 0;

        private double _temperature;
	    private double _heatConductivity;

        private double _maximumWater;
        private double _maximumSteam;

        readonly ISteamTransport[] _adjacentTransports = new ISteamTransport[6];
        readonly bool[] _canConnect = new bool[6];

        private bool _debug;
	    public bool StructureChanged;

	    public void addSteam(double unitsOfSteam)
        {
            if (_steamStored + unitsOfSteam >= _maximumSteam)
            {   
                _steamStored = _maximumSteam;
                return;
            }

            _steamStored += unitsOfSteam;
        }

        public void addCondensate(double unitsOfWater)
        {
            if (_waterStored + unitsOfWater >= _maximumWater)
            {
                _waterStored = _maximumWater;
                return;
            }

            _waterStored += unitsOfWater;
        }

        public double takeSteam(double desiredUnitsOfSteam)
        {
	        if (_steamStored <= 0)
	        {
		        _steamStored = 0;
		        return 0;
	        }
            if (desiredUnitsOfSteam <= _steamStored)
            {
                _steamStored -= desiredUnitsOfSteam;
                return desiredUnitsOfSteam;
            }

			double actualUnitsOfSteam = _steamStored;
            _steamStored = 0;
            return actualUnitsOfSteam;
        }

        public double takeCondensate(double desiredUnitsOfWater)
        {
	        if (_waterStored <= 0)
	        {
		        _waterStored = 0;
		        return 0;
	        }

            if (desiredUnitsOfWater <= _waterStored)
            {
                _waterStored -= desiredUnitsOfWater;
                return desiredUnitsOfWater;
            }

			double actualUnitsOfSteam = _waterStored;
            _waterStored = 0;
            return actualUnitsOfSteam;
        }

        public void setMaximumSteam(double maximumUnitsOfSteam)
        {
            _maximumSteam = maximumUnitsOfSteam;
        }

        public void setMaximumCondensate(double maximumUnitsOfWater)
        {
            _maximumWater = maximumUnitsOfWater;
        }

        public void toggleDebug()
        {
            _debug = !_debug;
        }

        public bool getShouldDebug()
        {
            return _debug;
        }

        public double getSteamStored()
        {
            return _steamStored;
        }

        public double getWaterStored()
        {
            return _waterStored;
        }

        public double getMaximumWater()
        {
            return _maximumWater;
        }

        public double getMaximumSteam()
        {
            return _maximumSteam;
        }

        public double getTemperature()
        {
            return _temperature;
        }

		public void setTemperature(double temperature)
		{
			_temperature = temperature;
		}

		public double getHeatConductivity()
		{
			return _heatConductivity;
		}
		
        public void setCanConnect(Direction direction, bool canConnect)
        {
            _canConnect[(int)direction] = canConnect;
        }

        public bool canConnect(Direction direction)
        {
            return _canConnect[(int) direction];
        }

        public void setAdjacentTransport(Direction direction, ISteamTransport transport)
        {
            if (canConnect(direction))

            _adjacentTransports[(int)direction] = transport;
	        StructureChanged = true;
        }

        public ISteamTransport getAdjacentTransport(Direction direction)
        {
            return _adjacentTransports[(int)direction];
        }

        public bool canTransportAbove()
        {
            return _adjacentTransports[(int) Direction.UP] != null;
        }

        public bool canTransportBelow()
        {
            return _adjacentTransports[(int)Direction.DOWN] != null;
        }

		[Obsolete]
        public bool canTransportWest()
        {
            return _adjacentTransports[(int)Direction.WEST] != null;
        }

		[Obsolete]
		public bool canTransportEast()
        {
            return _adjacentTransports[(int)Direction.EAST] != null;
        }

        internal SteamTransportLocation getTransportLocation()
        {
            return _steamTransportLocation;
        }
    }
}