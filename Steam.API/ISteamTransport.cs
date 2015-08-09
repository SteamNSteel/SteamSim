namespace Steam.API
{
    public interface ISteamTransport
    {
        void AddSteam(int unitsOfSteam);

        void AddCondensate(int unitsOfWater);

        int TakeSteam(int desiredUnitsOfSteam);

        int TakeCondensate(int desiredUnitsOfWater);

        void SetMaximumSteam(int maximumUnitsOfSteam);

        void SetMaximumCondensate(int maxumimUnitsOfWater);
    }
}