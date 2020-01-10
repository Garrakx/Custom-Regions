using CustomRegions.Mod;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CustomRegions
{
    public static class PlayerProgressionHook
    {
        public static void ApplyHooks()
        {
            On.PlayerProgression.ctor += PlayerProgression_ctor;
        }

        private static void PlayerProgression_ctor(On.PlayerProgression.orig_ctor orig, PlayerProgression self, RainWorld rainWorld, bool tryLoad)
        {
            orig(self, rainWorld, tryLoad);

            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
            {
                Debug.Log($"Custom Regions: PlayerProgression, loading new regions");

                string regionToAdd = keyValues.Key;
                bool shouldAdd = true;

                for (int i = 0; i < self.regionNames.Length; i++)
                {
                    if (regionToAdd.Equals(self.regionNames[i]))
                    {
                        shouldAdd = false;
                    }
                }
                if (shouldAdd)
                {
                    Array.Resize(ref self.regionNames, self.regionNames.Length + 1);
                    self.regionNames[self.regionNames.Length - 1] = keyValues.Key;
                    Debug.Log($"Custom Regions: Added new region [{regionToAdd}] from [{keyValues.Value}].");
                }
            }
            if(self.regionNames.Length != self.mapDiscoveryTextures.Length)
            {
                Array.Resize(ref self.mapDiscoveryTextures, self.regionNames.Length);
                Debug.Log($"Custom Regions: Resizing mapDiscovery in PlayerProgression.");
            }
        }
    }
}
