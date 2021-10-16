using CustomRegions.Mod;
using System.Collections.Generic;

namespace CustomRegions.Creatures
{
    static class DaddyLongLegsHook
    {
        public static void ApplyHooks()
        {
            On.DaddyLongLegs.ctor += DaddyLongLegs_ctor;
        }

        /// <summary>
        /// Checks if the region has colored BLLs/DLLs configured
        /// </summary>
        private static void DaddyLongLegs_ctor(On.DaddyLongLegs.orig_ctor orig, DaddyLongLegs self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            if (world != null && !world.singleRoomWorld)
            {
                foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
                {
                    if (CustomWorldMod.installedPacks[keyValues.Key].regionConfig.TryGetValue(world.region.name, 
                        out CustomWorldStructs.RegionConfiguration config))
                    {
                        if (!config.bllVanilla)
                        {
                            CustomWorldMod.Log($"Spawning custom DDL/BLL in [{world.region.name}] from " +
                                $"[{CustomWorldMod.installedPacks[keyValues.Key].name}]", false, CustomWorldMod.DebugLevel.FULL);
                            self.colorClass = true;
                            self.effectColor = config.bllColor ?? new UnityEngine.Color(0, 0, 1);
                            self.eyeColor = self.effectColor;
                            break;
                        }
                    }
                }
            }
        }
    }
}
