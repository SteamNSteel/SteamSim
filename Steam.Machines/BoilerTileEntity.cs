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

        protected override EnumFacing[] GetValidSteamTransportDirections()
        {
            return new []
            {
                EnumFacing.NORTH,
                EnumFacing.SOUTH,
                EnumFacing.EAST,
                EnumFacing.WEST,
            };
        }
    }
}
