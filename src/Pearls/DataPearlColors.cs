using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static CustomRegions.Mod.Structs;
using static CustomRegions.CustomPearls.Data;
using CustomRegions.Mod;

namespace CustomRegions.CustomPearls
{
    internal static class DataPearlColors
{

        public static void ApplyHooks()
        {
            On.DataPearl.ApplyPalette += DataPearl_ApplyPalette;
            On.DataPearl.UniquePearlMainColor += DataPearl_UniquePearlMainColor;
            On.DataPearl.UniquePearlHighLightColor += DataPearl_UniquePearlHighLightColor;
        }

        private static Color DataPearl_UniquePearlMainColor(On.DataPearl.orig_UniquePearlMainColor orig, DataPearl.AbstractDataPearl.DataPearlType pearlType)
        {
            try
            {
                if (CustomDataPearlsList.TryGetValue(pearlType, out CustomPearl customPearl))
                { return customPearl.color; }
                else
                { return orig(pearlType); }
            }
            catch (Exception e) { throw e; }
        }

        private static Color? DataPearl_UniquePearlHighLightColor(On.DataPearl.orig_UniquePearlHighLightColor orig, DataPearl.AbstractDataPearl.DataPearlType pearlType)
        {
            try
            {
                if (CustomDataPearlsList.TryGetValue(pearlType, out CustomPearl customPearl))
                { return customPearl.highlightColor; }
                else
                { return orig(pearlType); }
            }
            catch (Exception e) { throw e; }
        }

        private static void DataPearl_ApplyPalette(On.DataPearl.orig_ApplyPalette orig, DataPearl self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            try
            {
                orig(self, sLeaser, rCam, palette);

                DataPearl.AbstractDataPearl.DataPearlType pearlType = (self.abstractPhysicalObject as DataPearl.AbstractDataPearl).dataPearlType;

                if (CustomDataPearlsList.TryGetValue(pearlType, out CustomPearl customPearl))
                {
                    self.color = customPearl.color;
                    self.highlightColor = customPearl.highlightColor;
                    return;
                }
            }
            catch (Exception e) { throw e; }
        }
    }
}
