using CustomRegions.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static CustomRegions.Mod.CustomWorldStructs;

namespace CustomRegions.CustomPearls
{
    static class DataPearlHook
    {

        public static void ApplyHooks()
        {
            On.DataPearl.ApplyPalette += DataPearl_ApplyPalette;
            On.DataPearl.AbstractDataPearl.ToString += AbstractDataPearl_ToString;

            On.DataPearl.UniquePearlMainColor += DataPearl_UniquePearlMainColor;
            On.DataPearl.UniquePearlHighLightColor += DataPearl_UniquePearlHighLightColor;
        }

        private static string AbstractDataPearl_ToString(On.DataPearl.AbstractDataPearl.orig_ToString orig, DataPearl.AbstractDataPearl self)
        {

            DataPearl.AbstractDataPearl.DataPearlType backUpType = self.dataPearlType;
            KeyValuePair<int, CustomWorldStructs.CustomPearl> entry = CustomWorldMod.customPearls.FirstOrDefault(x => x.Value.name.Equals(backUpType.ToString()));

            // Pearl is not vanilla
            if (!entry.Equals(default(KeyValuePair<int, CustomWorldStructs.CustomPearl>)))
            {
                self.dataPearlType = (DataPearl.AbstractDataPearl.DataPearlType)entry.Key;
            }
            CustomWorldMod.Log($"AbstractDataPearl to string. PearlType [{self.dataPearlType}] [{backUpType.ToString()}]");
            string toString = orig(self);
            self.dataPearlType = backUpType;
            return toString;
        }
        

        private static void DataPearl_ApplyPalette(On.DataPearl.orig_ApplyPalette orig, DataPearl self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);

            bool foundPearl = false;
            foreach (KeyValuePair<int, CustomPearl> pearls in CustomWorldMod.customPearls)
            {
                if (foundPearl) { break; }

                DataPearl.AbstractDataPearl.DataPearlType dataPearlType = (DataPearl.AbstractDataPearl.DataPearlType)
                            Enum.Parse(typeof(DataPearl.AbstractDataPearl.DataPearlType), pearls.Value.name);

                if ((self.abstractPhysicalObject as DataPearl.AbstractDataPearl).dataPearlType == dataPearlType)
                {
                    foundPearl = true;
                    self.color = pearls.Value.color;
                    self.highlightColor = pearls.Value.secondaryColor;
                }

            }

        }

        private static Color DataPearl_UniquePearlMainColor(On.DataPearl.orig_UniquePearlMainColor orig, DataPearl.AbstractDataPearl.DataPearlType pearlType)
        {
            foreach (KeyValuePair<int, CustomPearl> pearls in CustomWorldMod.customPearls)
            {
                DataPearl.AbstractDataPearl.DataPearlType dataPearlType = (DataPearl.AbstractDataPearl.DataPearlType)
                            Enum.Parse(typeof(DataPearl.AbstractDataPearl.DataPearlType), pearls.Value.name);

                if (pearlType == dataPearlType)
                {
                    return pearls.Value.color;
                }

            }

            return orig(pearlType);
        }

        private static Color? DataPearl_UniquePearlHighLightColor(On.DataPearl.orig_UniquePearlHighLightColor orig, DataPearl.AbstractDataPearl.DataPearlType pearlType)
        {
            foreach (KeyValuePair<int, CustomPearl> pearls in CustomWorldMod.customPearls)
            {
                DataPearl.AbstractDataPearl.DataPearlType dataPearlType = (DataPearl.AbstractDataPearl.DataPearlType)
                            Enum.Parse(typeof(DataPearl.AbstractDataPearl.DataPearlType), pearls.Value.name);

                if (pearlType == dataPearlType)
                {
                    return pearls.Value.secondaryColor;
                }

            }

            return orig(pearlType);
        }
    }
}
