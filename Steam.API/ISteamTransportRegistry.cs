namespace Steam.API
{
    public interface ISteamTransportRegistry
    {
        ISteamTransport RegisterSteamTransport(int x, int y, ForgeDirection[] forgeDirection);

        void DestroySteamTransport(int x, int y);
    }
}