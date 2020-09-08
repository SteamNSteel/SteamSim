using Steam.API;

namespace Steam.Machines
{
    public class BoilerTileEntity : ModTileEntity
    {
        public override void onTick()
        {
            getSteamTransport().addSteam(20);
            base.onTick();
        }

        protected override Direction[] getValidSteamTransportDirections()
        {
            return new []
            {
                Direction.NORTH,
                Direction.SOUTH,
                Direction.EAST,
                Direction.WEST,
            };
        }
    }
}
