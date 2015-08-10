using System;
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
            _steamTransport = TheMod.SteamTransportRegistry.RegisterSteamTransport(x, y, GetValidSteamTransportDirections());
            TheMod.AddTileEntity(this);
        }

        protected virtual ForgeDirection[] GetValidSteamTransportDirections()
        {
            return new []
            {
                ForgeDirection.DOWN, 
                ForgeDirection.UP,
                ForgeDirection.NORTH,
                ForgeDirection.SOUTH,
                ForgeDirection.WEST,
                ForgeDirection.EAST
            };
        }

        public ISteamTransport GetSteamTransport()
        {
            return _steamTransport;
        }

        public event EventHandler<EventArgs> DataChanged;

        public void Destroy()
        {
            TheMod.SteamTransportRegistry.DestroySteamTransport(_x, _y);
            TheMod.RemoveTileEntity(this);
        }

        public override void OnTick()
        {
            DataChanged?.Invoke(this, new EventArgs());
        }
    }
}