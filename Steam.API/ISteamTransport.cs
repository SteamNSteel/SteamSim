namespace Steam.API
{
    public interface ISteamTransport
    {
        void AddSteam(int unitsOfSteam);

        void AddCondensate(int unitsOfWater);

        int TakeSteam(int desiredUnitsOfSteam);

        int TakeCondensate(int desiredUnitsOfWater);

        void SetMaximumSteam(int maximumUnitsOfSteam);

        void SetMaximumCondensate(int maximimUnitsOfWater);

        void ToggleDebug();

        bool GetShouldDebug();
        int GetSteamStored();
        int GetWaterStored();
        int GetMaximumWater();

        int GetMaximumSteam();
        double GetCalculatedSteamDensity();
        int GetCalculatedMaximumSteam();
        double GetTemperature();
        

        bool CanTransportAbove();
        bool CanTransportBelow();
        bool CanTransportWest();
        bool CanTransportEast();

    }
}   