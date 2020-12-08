using CustomRegions.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CustomRegions.Creatures
{
    static class LizardGraphicsHook
    {
        public static void ApplyHooks()
        {
            On.LizardGraphics.ctor += LizardGraphics_ctor;
        }

        private static void LizardGraphics_ctor(On.LizardGraphics.orig_ctor orig, LizardGraphics self, PhysicalObject ow)
        {
            orig(self, ow);

            World world = ow.abstractPhysicalObject.world;
            if (world != null)
            {
                foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegionPacks)
                {
                    if (CustomWorldMod.installedRegionPacks[keyValues.Key].regionConfig.TryGetValue(world.region.name, out CustomWorldStructs.RegionConfiguration config))
                    {
                        if (config.blackSalamanderChance >= 0)
                        {
                            //CustomWorldMod.Log($"Spawning tentacle plant with custom color in [{world.region.name}] from [{CustomWorldMod.availableRegions[keyValues.Key].regionName}]");
                            int seed = UnityEngine.Random.seed;
                            UnityEngine.Random.seed = self.lizard.abstractCreature.ID.RandomSeed;
                            self.blackSalamander = (UnityEngine.Random.value < config.blackSalamanderChance);
                            UnityEngine.Random.seed = seed;
                        }
                        break;
                    }
                }
            }
        }
    }
}
