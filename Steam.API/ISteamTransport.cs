namespace Steam.API
{
    public interface ISteamTransport
    {
        void AddSteam(double unitsOfSteam);

        void AddCondensate(double unitsOfWater);

		double TakeSteam(double desiredUnitsOfSteam);

		double TakeCondensate(double desiredUnitsOfWater);

        void SetMaximumSteam(double maximumUnitsOfSteam);

        void SetMaximumCondensate(double maximimUnitsOfWater);

        void ToggleDebug();

        bool GetShouldDebug();
		double GetSteamStored();
		double GetWaterStored();
		double GetMaximumWater();

		double GetMaximumSteam();
        
		//double GetCalculatedMaximumSteam();
        double GetTemperature();
        

        bool CanTransportAbove();
        bool CanTransportBelow();
        bool CanTransportWest();
        bool CanTransportEast();

    }
}   