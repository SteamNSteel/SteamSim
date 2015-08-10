using System;
using Steam.API;

namespace SteamPipes.Impl
{
    public class SteamTransport : ISteamTransport
    {
        public SteamTransport()
        {
            _maximumSteam = 1000;
            _maximumWater = 800;
        }

        private int _waterStored = 0;
        private int _steamStored = 0;

        private double _temperature;

        private int _maximumWater;
        private int _maximumSteam;

        readonly ISteamTransport[] _adjacentTransports = new ISteamTransport[6];
        readonly bool[] _canConnect = new bool[6];

        private bool _debug;

        public void AddSteam(int unitsOfSteam)
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

        public void AddCondensate(int unitsOfWater)
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

        public int TakeSteam(int desiredUnitsOfSteam)
        {
            if (desiredUnitsOfSteam >= _steamStored)
            {
                _steamStored -= desiredUnitsOfSteam;
                return desiredUnitsOfSteam;
            }

            int actualUnitsOfSteam = _steamStored;
            _steamStored = 0;
            return actualUnitsOfSteam;
        }

        public int TakeCondensate(int desiredUnitsOfWater)
        {
            if (desiredUnitsOfWater > _waterStored)
            {
                _steamStored -= desiredUnitsOfWater;
                return desiredUnitsOfWater;
            }

            int actualUnitsOfSteam = _waterStored;
            _waterStored = 0;
            return actualUnitsOfSteam;
        }

        public void SetMaximumSteam(int maximumUnitsOfSteam)
        {
            _maximumSteam = maximumUnitsOfSteam;
        }

        public void SetMaximumCondensate(int maximimUnitsOfWater)
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

        public int GetSteamStored()
        {
            return _steamStored;
        }

        public int GetWaterStored()
        {
            return _waterStored;
        }

        public int GetMaximumWater()
        {
            return _maximumWater;
        }

        public int GetMaximumSteam()
        {
            return _maximumSteam;
        }

        public double GetCalculatedSteamDensity()
        {
            var calculatedMaximumSteam = GetCalculatedMaximumSteam();
            if (calculatedMaximumSteam <= 0)
            {
                return _steamStored > 0 ? 100 : 0;
            }

            var x = (double)_steamStored;
            var c = (double)calculatedMaximumSteam;
            var a = c / 100;
            var b = 100 / c;
            var y = Math.Log10((x + a) * b) * 50;
            if (y > 100)
            {
                return 100;
            }
            if (y < 0)
            {
                return 0;
            }
            return y;
        }

        public int GetCalculatedMaximumSteam()
        {
            return (1 - (_waterStored / _maximumWater)) * _maximumSteam;
        }

        public double GetTemperature()
        {
            return _temperature;
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

        
    }
}