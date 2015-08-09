namespace Steam.API
{
    public interface ISteamTransportRegistry
    {
        ISteamTransport RegisterSteamTransport(int x, int y);

        void DestroySteamTransport(int x, int y);
    }
}