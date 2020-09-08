namespace Steam.Machines
{
    public class FurnaceTileEntity : ModTileEntity
    {
        public FurnaceTileEntity()
        {
            
        }

        public override void onTick()
        {
            getSteamTransport().takeSteam(5);
            base.onTick();
        }
    }
}
