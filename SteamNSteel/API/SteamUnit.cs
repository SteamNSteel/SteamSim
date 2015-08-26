using System;
using System.Collections.Generic;

namespace SteamNSteel.API
{
    [Obsolete("Currently kept for reference")]
    public class SteamUnit
	{
        private decimal _waterStored;
        private decimal _steamStored;
		private decimal _temperature;
		private SteamUnit _unitAbove;
		private SteamUnit _unitBelow;
		private SteamUnit _unitLeft;
		private SteamUnit _unitRight;
		
		private readonly List<SteamUnit> _allAdjacentConnections = new List<SteamUnit>();
		private readonly List<SteamUnit> _horizontalAdjacentConnections = new List<SteamUnit>();

		public SteamUnit()
		{
			SteamStored = 0;
			MaxSteam = 1000;
			MaxWater = 800;
			HeatConductivity = 2;
			SteamFlowSourceUnits = new HashSet<SteamUnit>();
			WaterFlowSourceUnits = new HashSet<SteamUnit>();
		}

		public int X { get; internal set; }
		public int Y { get; internal set; }

		public SteamUnit UnitAbove
		{
			get { return _unitAbove; }
			set
			{
				if (_unitAbove == value) return;
				if ((this is ISteamConsumer && value is ISteamConsumer)) return;
				if ((this is ISteamProvider && value is ISteamProvider)) return;
				if (_unitAbove != null)
				{
					_allAdjacentConnections.Remove(_unitAbove);
				}
				if (value != null)
				{
					_allAdjacentConnections.Add(value);
				}
				InvokeConnectionsChanged();
				_unitAbove = value;
			}
		}

		public SteamUnit UnitBelow
		{
			get { return _unitBelow; }
			set
			{
				if (_unitBelow == value) return;
				if ((this is ISteamConsumer && value is ISteamConsumer)) return;
				if ((this is ISteamProvider && value is ISteamProvider)) return;
				if (_unitBelow != null)
				{
					_allAdjacentConnections.Remove(_unitBelow);
				}
				if (value != null)
				{
					_allAdjacentConnections.Add(value);
				}
				InvokeConnectionsChanged();

				_unitBelow = value;
			}
		}

		public SteamUnit UnitLeft
		{
			get { return _unitLeft; }
			set
			{
				if (_unitLeft == value) return;
				if ((this is ISteamConsumer && value is ISteamConsumer)) return;
				if ((this is ISteamProvider && value is ISteamProvider)) return;

				if (_unitLeft != null)
				{
					_allAdjacentConnections.Remove(_unitLeft);
					_horizontalAdjacentConnections.Remove(_unitLeft);
				}
				if (value != null)
				{
					_allAdjacentConnections.Add(value);
					_horizontalAdjacentConnections.Add(value);
				}
				InvokeConnectionsChanged();
				_unitLeft = value;
			}
		}

		public SteamUnit UnitRight
		{
			get { return _unitRight; }
			set
			{
				if (_unitRight == value) return;
				if ((this is ISteamConsumer && value is ISteamConsumer)) return;
				if ((this is ISteamProvider && value is ISteamProvider)) return;
				if (_unitRight != null)
				{
					_allAdjacentConnections.Remove(_unitRight);
					_horizontalAdjacentConnections.Remove(_unitRight);
				}
				if (value != null)
				{
					_allAdjacentConnections.Add(value);
					_horizontalAdjacentConnections.Add(value);
				}
				InvokeConnectionsChanged();

				_unitRight = value;
			}
		}

		public decimal SteamStored
		{
			get { return _steamStored; }
			set
			{
				if (_steamStored != value)
				{
					_steamStored = value;
					InvokeDataChanged();
				}
			}
		}

		public decimal MaxSteam { get; set; }

		public decimal ActualMaxSteam
		{
			get { return (1 - (WaterStored/MaxWater))*MaxSteam; }
		}

		public double SteamDensity
		{
			get
			{
				if (ActualMaxSteam <= 0)
				{
					return SteamStored > 0 ? 100 : 0;
				}

				var x = (double) SteamStored;
				var c = (double) ActualMaxSteam;
				var a = c/100;
				var b = 100/c;
				var y = Math.Log10((x + a)*b)*50;
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
		}

		public List<SteamUnit> AllAdjacentConnections
		{
			get { return _allAdjacentConnections; }
		}

		public int ProcessPass { get; set; }

		public List<SteamUnit> HorizontalAdjacentConnections
		{
			get { return _horizontalAdjacentConnections; }
		}

		public HashSet<SteamUnit> SteamFlowSourceUnits { get; private set; }
		public HashSet<SteamUnit> WaterFlowSourceUnits { get; private set; }
		public decimal NewSteam { get; set; }
		public decimal NewWater { get; set; }

		public decimal Temperature
		{
			get { return _temperature; }
			set
			{
				if (_temperature != value)
				{
					_temperature = value;
					InvokeDataChanged();
				}
			}
		}

		public decimal HeatConductivity { get; private set; }

		public decimal WaterStored
		{
			get { return _waterStored; }
			set
			{
				if (_waterStored != value)
				{
					_waterStored = value;
					InvokeDataChanged();
				}
			}
		}

		public decimal MaxWater { get; set; }
		public bool Debug { get; set; }

		private void InvokeConnectionsChanged()
		{
			var evt = ConnectionsChanged;
			if (evt != null)
			{
				evt(this, new EventArgs());
			}
		}

		private void InvokeDataChanged()
		{
			var evt = DataChanged;
			if (evt != null)
			{
				evt(this, new EventArgs());
			}
		}

		public event EventHandler ConnectionsChanged;
		public event EventHandler DataChanged;
	}
}