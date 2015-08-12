namespace Steam.API
{
    public struct ForgeDirection
    {
        /** -Y */
        public static readonly ForgeDirection DOWN = new ForgeDirection(0, -1, 0, 0);
        /** +Y */
        public static readonly ForgeDirection UP = new ForgeDirection(0, 1, 0, 1);
        /** -Z */
        public static readonly ForgeDirection NORTH = new ForgeDirection(0, 0, -1, 2);
        /** +Z */
        public static readonly ForgeDirection SOUTH = new ForgeDirection(0, 0, 1, 3);
        /** -X */
        public static readonly ForgeDirection WEST = new ForgeDirection(-1, 0, 0, 4);
        /** +X */
        public static readonly ForgeDirection EAST = new ForgeDirection(1, 0, 0, 5);
        /**
         * Used only by getOrientation, for invalid inputs
         */
        public static readonly ForgeDirection UNKNOWN = new ForgeDirection(0, 0, 0, 6);
        public static readonly ForgeDirection[] VALID_DIRECTIONS = {DOWN, UP, NORTH, SOUTH, WEST, EAST};
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
        public readonly int flag;
        public readonly int offsetX;
        public readonly int offsetY;
        public readonly int offsetZ;

        private ForgeDirection(int x, int y, int z, int ordinal)
        {
            offsetX = x;
            offsetY = y;
            offsetZ = z;
            _ordinal = ordinal;
            flag = 1 << ordinal;
        }

        public static ForgeDirection getOrientation(int id)
        {
            if (id >= 0 && id < VALID_DIRECTIONS.Length)
            {
                return VALID_DIRECTIONS[id];
            }
            return UNKNOWN;
        }

        public ForgeDirection getOpposite()
        {
            return getOrientation(OPPOSITES[_ordinal]);
        }

        public ForgeDirection getRotation(ForgeDirection axis)
        {
            return getOrientation(ROTATION_MATRIX[axis._ordinal, _ordinal]);
        }

        public static explicit operator int(ForgeDirection v)
        {
            return v._ordinal;
        }
    }
}