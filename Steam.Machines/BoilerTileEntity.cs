using Steam.API;

namespace Steam.Machines
{
    public class BoilerTileEntity : ModTileEntity
    {
        public override void OnTick()
        {
            GetSteamTransport().AddSteam(20);
            base.OnTick();
        }

        protected override ForgeDirection[] GetValidSteamTransportDirections()
        {
            return new []
            {
                ForgeDirection.NORTH,
                ForgeDirection.SOUTH,
                ForgeDirection.EAST,
                ForgeDirection.WEST,
            };
        }
    }
}
