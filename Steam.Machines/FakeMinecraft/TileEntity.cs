namespace Steam.Machines.FakeMinecraft
{
    public class TileEntity
    {
        private int _x;
        private int _y;


        public virtual void SetLocation(int x, int y)
        {
            _x = x;
            _y = y;
        }

        public virtual void OnTick() { }
    }
}