using System.Collections.Generic;
using CustomRegions.Mod;

namespace CustomRegions.Creatures
{
    static class BigEelHook
    {
        public static void ApplyHooks()
        {
            On.BigEel.ctor += BigEel_ctor;
        }

        public static void RemoveHooks()
        {
            On.BigEel.ctor -= BigEel_ctor;
        }

        /// <summary>
        /// Checks if the region has albino leviathans configured
        /// </summary>
        private static void BigEel_ctor(On.BigEel.orig_ctor orig, BigEel self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);

            if (world != null && !world.singleRoomWorld && world.region != null)
            {
                foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
                {
                    if (CustomWorldMod.installedPacks[keyValues.Key].regionConfig.TryGetValue(world.region.name, out CustomWorldStructs.RegionConfiguration config))
                    {
                        //CustomWorldMod.Log($"Albino leviathan in [{world.region.name}]");
                        if (config.albinoLevi)
                        {
                            self.albino = true;
                            self.iVars.patternColorB = new HSLColor(0f, 0.6f, 0.75f);
                            self.iVars.patternColorA.hue = 0.5f;
                            self.iVars.patternColorA = HSLColor.Lerp(self.iVars.patternColorA, new HSLColor(0.97f, 0.8f, 0.75f), 0.9f);
                        }
                        break;
                    }
                }
            }
        }
    }
}
