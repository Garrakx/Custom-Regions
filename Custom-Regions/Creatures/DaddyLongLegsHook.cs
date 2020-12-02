using CustomRegions.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CustomRegions.Creatures
{
    static class DaddyLongLegsHook
    {
        public static void ApplyHooks()
        {
            On.DaddyLongLegs.ctor += DaddyLongLegs_ctor;
        }

        private static void DaddyLongLegs_ctor(On.DaddyLongLegs.orig_ctor orig, DaddyLongLegs self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            if (world != null)
            {
                //CustomWorldMod.Log($"Region Name [{self.region.name}]");
                foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
                {
                    //CustomWorldMod.Log($"Checking in [{CustomWorldMod.availableRegions[keyValues.Key].regionName}]");
                    if (CustomWorldMod.availableRegions[keyValues.Key].regionConfig.TryGetValue(world.region.name, out CustomWorldStructs.RegionConfiguration config))
                    {
                        if (!config.bllVanilla)
                        {
                            CustomWorldMod.Log($"Spawning custom DDL/BLL in [{world.region.name}] from [{CustomWorldMod.availableRegions[keyValues.Key].regionName}]");
                            self.colorClass = true;
                            self.effectColor = config.bllColor ?? new UnityEngine.Color(0, 0, 1);
                            self.eyeColor = self.effectColor;
                        }
                        break;
                    }
                }
            }
        }
    }
}
