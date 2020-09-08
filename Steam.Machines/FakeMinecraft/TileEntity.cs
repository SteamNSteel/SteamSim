namespace Steam.Machines.FakeMinecraft
{
    public class TileEntity
    {
        protected int _x;
        protected int _y;


        public virtual void setLocation(int x, int y)
        {
            _x = x;
            _y = y;
        }

        public virtual void onTick() { }
    }
}