namespace Steam.Machines
{
    public class BoilerTileEntity : ModTileEntity
    {
        public override void OnTick()
        {
            GetSteamTransport().AddSteam(20);
        }
    }
}
