namespace Steam.Machines
{
    public class BallMillTileEntity : ModTileEntity
    {
        public override void OnTick()
        {
            
            getSteamTransport().takeSteam(2);
            base.OnTick();

        }
    }
}
