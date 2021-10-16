using CustomRegions.Mod;
using System.Collections.Generic;

namespace CustomRegions.Creatures
{
    static class LizardGraphicsHook
    {
        public static void ApplyHooks()
        {
            On.LizardGraphics.ctor += LizardGraphics_ctor;
        }

        /// <summary>
        /// Adjust chance of spawning black salamanders
        /// </summary>
        private static void LizardGraphics_ctor(On.LizardGraphics.orig_ctor orig, LizardGraphics self, PhysicalObject ow)
        {
            orig(self, ow);

            World world = ow.abstractPhysicalObject.world;
            if (world != null && !world.singleRoomWorld && world.region != null)
            {
                foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
                {
                    if (CustomWorldMod.installedPacks[keyValues.Key].regionConfig.TryGetValue(world.region.name, 
                        out CustomWorldStructs.RegionConfiguration config))
                    {
                        if (config.blackSalamanderChance >= 0)
                        {
                            CustomWorldMod.Log($"Custom salamander chance [{config.blackSalamanderChance}] in [{world.region.name}] from " +
                                $"[{CustomWorldMod.installedPacks[keyValues.Key].name}]", false, CustomWorldMod.DebugLevel.FULL);
                            int seed = UnityEngine.Random.seed;
                            UnityEngine.Random.seed = self.lizard.abstractCreature.ID.RandomSeed;
                            self.blackSalamander = (UnityEngine.Random.value < config.blackSalamanderChance);
                            UnityEngine.Random.seed = seed;
                            break;
                        }
                    }
                }
            }
        }
    }
}
