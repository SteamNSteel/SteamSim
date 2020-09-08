namespace Steam.API
{
    public interface ISteamTransport
    {
        void addSteam(double unitsOfSteam);

        void addCondensate(double unitsOfWater);

		double takeSteam(double desiredUnitsOfSteam);

		double takeCondensate(double desiredUnitsOfWater);

        void setMaximumSteam(double maximumUnitsOfSteam);

        void setMaximumCondensate(double maximumUnitsOfWater);

        void toggleDebug();

        bool getShouldDebug();
		double getSteamStored();
		double getWaterStored();
		double getMaximumWater();

		double getMaximumSteam();
        
		//double GetCalculatedMaximumSteam();
        double getTemperature();
        

        bool canTransportAbove();
        bool canTransportBelow();
        bool canTransportWest();
        bool canTransportEast();

    }
}   