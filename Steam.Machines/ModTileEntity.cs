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
            _steamTransport = ChildMod.SteamTransportRegistry.RegisterSteamTransport(x, y, GetValidSteamTransportDirections());
            ChildMod.AddTileEntity(this);
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
            ChildMod.SteamTransportRegistry.DestroySteamTransport(_x, _y);
            ChildMod.RemoveTileEntity(this);
        }

        public override void OnTick()
        {
            DataChanged?.Invoke(this, new EventArgs());
        }
    }
}