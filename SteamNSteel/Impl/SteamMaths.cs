using System;

namespace SteamNSteel.Impl
{
	public static class SteamMaths
	{
		public static double CalculateMaximumSteam(double waterStored, double maximumWater, double maximumSteam)
		{
			return (1 - (waterStored / maximumWater)) * maximumSteam;
		}

		public static double CalculateSteamDensity(double steamStored, double maximumSteam)
		{
			//var calculatedMaximumSteam = GetCalculatedMaximumSteam(waterStored);
			if (maximumSteam <= 0)
			{
				return steamStored > 0 ? 100 : 0;
			}

			var x = steamStored;
			var c = maximumSteam;
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
	}
}