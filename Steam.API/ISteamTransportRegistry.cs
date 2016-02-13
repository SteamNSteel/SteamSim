namespace Steam.API
{
	//FIXME: Add reference to int z, int, worldId
    public interface ISteamTransportRegistry
    {
        ISteamTransport registerSteamTransport(int x, int y, EnumFacing[] enumFacing);

        void destroySteamTransport(int x, int y);
	}
}