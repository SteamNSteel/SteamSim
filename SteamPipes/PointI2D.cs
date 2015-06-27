﻿namespace SteamPipes
{
	internal class PointI2D
	{
		protected bool Equals(PointI2D other)
		{
			return X == other.X && Y == other.Y;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (X*397) ^ Y;
			}
		}

		public int X { get; private set; }
		public int Y { get; private set; }

		internal PointI2D(int x, int y)
		{
			X = x;
			Y = y;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((PointI2D) obj);
		}
	}
}