using System.Collections.Generic;
using Steam.API;

namespace Steam.Machines
{
    public static class ChildMod
    {
        internal static ISteamTransportRegistry SteamTransportRegistry;
        public static readonly List<ModTileEntity> TileEntities = new List<ModTileEntity>();

        public static void OnSteamNSteelInitialized(SteamNSteelInitializedEvent evt)
        {
            SteamTransportRegistry = evt.getSteamTransportRegistry();
        }

        public static void addTileEntity(ModTileEntity tileEntity)
        {
            lock (TileEntities)
            {
                TileEntities.Add(tileEntity);
            }
        }

        public static void removeTileEntity(ModTileEntity tileEntity)
        {
            lock (TileEntities)
            {
                TileEntities.Remove(tileEntity);
            }

        }
    }
}
