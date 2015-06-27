using System;
using System.Collections.Generic;
using System.Windows.Documents;

namespace SteamPipes
{
	internal class SteamUnit
	{
		private readonly List<SteamUnit> _allAdjacentConnections = new List<SteamUnit>();
		private readonly List<SteamUnit> _horizontalAdjacentConnections = new List<SteamUnit>();
		private SteamUnit _unitAbove;
		private SteamUnit _unitBelow;
		private SteamUnit _unitLeft;
		private SteamUnit _unitRight;
		private decimal _steamStored;
		public int X { get; private set; }
		public int Y { get; private set; }

		internal SteamUnit(int x, int y)
		{
			X = x;
			Y = y;

			SteamStored = 0;
			MaxSteam = 1000;

			FlowSourceUnits = new HashSet<SteamUnit>();
		}

		public SteamUnit UnitAbove
		{
			get { return _unitAbove; }
			set
			{
				if (_unitAbove != value)
				{
					if (_unitAbove != null)
					{
						_allAdjacentConnections.Remove(_unitAbove);
					}
					if (value != null)
					{
						_allAdjacentConnections.Add(value);
					}
					InvokeConnectionsChanged();	
				}
				_unitAbove = value;
			}
		}

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

		public SteamUnit UnitBelow
		{
			get { return _unitBelow; }
			set
			{
				if (_unitBelow != value)
				{
					if (_unitBelow != null)
					{
						_allAdjacentConnections.Remove(_unitBelow);
					}
					if (value != null)
					{
						_allAdjacentConnections.Add(value);
					}
					InvokeConnectionsChanged();	
				}
				_unitBelow = value;
			}
		}

		public SteamUnit UnitLeft
		{
			get { return _unitLeft; }
			set
			{
				if (_unitLeft != value)
				{
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
				}
				_unitLeft = value;
			}
		}

		public SteamUnit UnitRight
		{
			get { return _unitRight; }
			set
			{
				if (_unitRight != value)
				{
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
				}
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

		public List<SteamUnit> AllAdjacentConnections
		{
			get { return _allAdjacentConnections; }
		}

		public int ProcessPass { get; set; }

		public List<SteamUnit> HorizontalAdjacentConnections
		{
			get { return _horizontalAdjacentConnections; }
		}

		public HashSet<SteamUnit> FlowSourceUnits { get; private set; }

		public event EventHandler ConnectionsChanged;
		public event EventHandler DataChanged;
	}
}