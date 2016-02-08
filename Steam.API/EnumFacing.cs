namespace Steam.API
{
    public struct EnumFacing
    {
		//Y has been reversed for this program because XAML.
		/** -Y */
		//public static readonly EnumFacing DOWN = new EnumFacing(0, -1, 0, 0);
		public static readonly EnumFacing DOWN = new EnumFacing(0, 1, 0, 0, "DOWN");
		/** +Y */
		//public static readonly EnumFacing UP = new EnumFacing(0, 1, 0, 1);
		public static readonly EnumFacing UP = new EnumFacing(0, -1, 0, 1, "UP");
		/** -Z */
		public static readonly EnumFacing NORTH = new EnumFacing(0, 0, -1, 2, "NORTH");
        /** +Z */
        public static readonly EnumFacing SOUTH = new EnumFacing(0, 0, 1, 3, "SOUTH");
        /** -X */
        public static readonly EnumFacing WEST = new EnumFacing(-1, 0, 0, 4, "WEST");
        /** +X */
        public static readonly EnumFacing EAST = new EnumFacing(1, 0, 0, 5, "EAST");
        /**
         * Used only by getOrientation, for invalid inputs
         */
        public static readonly EnumFacing UNKNOWN = new EnumFacing(0, 0, 0, 6, "UNKNOWN");
        public static readonly EnumFacing[] VALID_DIRECTIONS = {DOWN, UP, NORTH, SOUTH, WEST, EAST};
        public static readonly int[] OPPOSITES = {1, 0, 3, 2, 5, 4, 6};
        // Left hand rule rotation matrix for all possible axes of rotation
        public static readonly int[,] ROTATION_MATRIX =
        {
            {0, 1, 4, 5, 3, 2, 6},
            {0, 1, 5, 4, 2, 3, 6},
            {5, 4, 2, 3, 0, 1, 6},
            {4, 5, 2, 3, 1, 0, 6},
            {2, 3, 1, 0, 4, 5, 6},
            {3, 2, 0, 1, 4, 5, 6},
            {0, 1, 2, 3, 4, 5, 6}
        };

        public readonly int _ordinal;
	    private readonly string _name;
	    public readonly int flag;
        public readonly int offsetX;
        public readonly int offsetY;
        public readonly int offsetZ;

        private EnumFacing(int x, int y, int z, int ordinal, string name)
        {
            offsetX = x;
            offsetY = y;
            offsetZ = z;
            _ordinal = ordinal;
	        _name = name;
	        flag = 1 << ordinal;
        }

        public static EnumFacing getOrientation(int id)
        {
            if (id >= 0 && id < VALID_DIRECTIONS.Length)
            {
                return VALID_DIRECTIONS[id];
            }
            return UNKNOWN;
        }

        public EnumFacing getOpposite()
        {
            return getOrientation(OPPOSITES[_ordinal]);
        }

        public EnumFacing getRotation(EnumFacing axis)
        {
            return getOrientation(ROTATION_MATRIX[axis._ordinal, _ordinal]);
        }

        public static explicit operator int(EnumFacing v)
        {
            return v._ordinal;
        }

	    public override string ToString()
	    {
		    return $"ForgeDirection ({_name})";
	    }
    }
}