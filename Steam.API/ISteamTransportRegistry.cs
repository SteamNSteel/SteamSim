namespace Steam.API
{
	//FIXME: Add reference to int z, int, worldId
    public interface ISteamTransportRegistry
    {
        ISteamTransport RegisterSteamTransport(int x, int y, EnumFacing[] enumFacing);

        void DestroySteamTransport(int x, int y);
	}
}