using Steam.API;
using Steam.Machines.FakeMinecraft;

namespace Steam.Machines
{
    public class ModTileEntity : TileEntity
    {
        private ISteamTransport _steamTransport;

        public override void SetLocation(int x, int y)
        {
            base.SetLocation(x, y);
            _steamTransport = TheMod.SteamTransportRegistry.RegisterSteamTransport(x, y);
        }

        protected ISteamTransport GetSteamTransport()
        {
            return _steamTransport;
        }
    }
}