using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Steam.API;

namespace Steam.Machines
{
    public static class ChildMod
    {
        internal static ISteamTransportRegistry SteamTransportRegistry;
        public static readonly List<ModTileEntity> TileEntities = new List<ModTileEntity>();

        public static void OnSteamNSteelInitialized(SteamNSteelInitializedEvent evt)
        {
            SteamTransportRegistry = evt.GetSteamTransportRegistry();
        }

        public static void AddTileEntity(ModTileEntity tileEntity)
        {
            lock (TileEntities)
            {
                TileEntities.Add(tileEntity);
            }
        }

        public static void RemoveTileEntity(ModTileEntity tileEntity)
        {
            lock (TileEntities)
            {
                TileEntities.Remove(tileEntity);
            }

        }
    }
}
