using System;
using Steam.API;
using Steam.Machines.FakeMinecraft;

namespace Steam.Machines
{
    public class ModTileEntity : TileEntity
    {
        private ISteamTransport _steamTransport;

        public override void setLocation(int x, int y)
        {
            base.setLocation(x, y);
            _steamTransport = ChildMod.SteamTransportRegistry.registerSteamTransport(x, y, getValidSteamTransportDirections());
            ChildMod.addTileEntity(this);
        }

        protected virtual Direction[] getValidSteamTransportDirections()
        {
            return new []
            {
                Direction.DOWN, 
                Direction.UP,
                Direction.NORTH,
                Direction.SOUTH,
                Direction.WEST,
                Direction.EAST
            };
        }

        public ISteamTransport getSteamTransport()
        {
            return _steamTransport;
        }

        public event EventHandler<EventArgs> DataChanged;

        public void destroy()
        {
            ChildMod.SteamTransportRegistry.destroySteamTransport(_x, _y);
            ChildMod.removeTileEntity(this);
        }

        public override void onTick()
        {
            DataChanged?.Invoke(this, new EventArgs());
        }
    }
}