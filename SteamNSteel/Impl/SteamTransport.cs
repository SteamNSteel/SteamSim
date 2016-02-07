using System;
using Steam.API;

namespace SteamNSteel.Impl
{
    class SteamTransport : ISteamTransport
    {
        private readonly SteamTransportLocation _steamTransportLocation;
        private SteamTransportTopology _topology;

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
	    public readonly object _syncObj = new object();
	    public bool StructureChanged;

	    public void AddSteam(double unitsOfSteam)
        {
            if (_steamStored + unitsOfSteam >= _maximumSteam)
            {   
                _steamStored = _maximumSteam;
                //return (_maximumWater - _waterStored);
                return;
            }

            _steamStored += unitsOfSteam;

            //return 0;
        }

        public void AddCondensate(double unitsOfWater)
        {
            if (_waterStored + unitsOfWater >= _maximumWater)
            {
                _waterStored = _maximumWater;
                //return (_maximumWater - _waterStored);
                return;
            }

            _waterStored += unitsOfWater;

            //return 0;
        }

        public double TakeSteam(double desiredUnitsOfSteam)
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

        public double TakeCondensate(double desiredUnitsOfWater)
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

        public void SetMaximumSteam(double maximumUnitsOfSteam)
        {
            _maximumSteam = maximumUnitsOfSteam;
        }

        public void SetMaximumCondensate(double maximimUnitsOfWater)
        {
            _maximumWater = maximimUnitsOfWater;
        }

        public void ToggleDebug()
        {
            _debug = !_debug;
        }

        public bool GetShouldDebug()
        {
            return _debug;
        }

        public double GetSteamStored()
        {
            return _steamStored;
        }

        public double GetWaterStored()
        {
            return _waterStored;
        }

        public double GetMaximumWater()
        {
            return _maximumWater;
        }

        public double GetMaximumSteam()
        {
            return _maximumSteam;
        }

        public double GetTemperature()
        {
            return _temperature;
        }

		public void SetTemperature(double temperature)
		{
			_temperature = temperature;
		}

		public double GetHeatConductivity()
		{
			return _heatConductivity;
		}

		



        public void SetCanConnect(ForgeDirection direction, bool canConnect)
        {
            _canConnect[(int)direction] = canConnect;
        }

        public bool CanConnect(ForgeDirection direction)
        {
            return _canConnect[(int) direction];
        }

        public void SetAdjacentTransport(ForgeDirection direction, ISteamTransport transport)
        {
            if (CanConnect(direction))

            _adjacentTransports[(int)direction] = transport;
	        StructureChanged = true;
        }

        public ISteamTransport GetAdjacentTransport(ForgeDirection direction)
        {
            return _adjacentTransports[(int)direction];
        }

        public bool CanTransportAbove()
        {
            return _adjacentTransports[(int) ForgeDirection.UP] != null;
        }

        public bool CanTransportBelow()
        {
            return _adjacentTransports[(int)ForgeDirection.DOWN] != null;
        }

        public bool CanTransportWest()
        {
            return _adjacentTransports[(int)ForgeDirection.WEST] != null;
        }

        public bool CanTransportEast()
        {
            return _adjacentTransports[(int)ForgeDirection.EAST] != null;
        }

        internal SteamTransportTopology GetTopology()
        {
            return _topology;
        }

        internal void SetTopology(SteamTransportTopology topology)
        {
            _topology = topology;
	        topology.AddTransport(this);
        }

        internal SteamTransportLocation GetTransportLocation()
        {
            return _steamTransportLocation;
        }
    }
}