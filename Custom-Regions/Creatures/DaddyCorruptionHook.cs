using CustomRegions.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CustomRegions.Creatures
{
    static class DaddyCorruptionHook
    {
        internal static void ApplyHooks()
        {
            On.DaddyCorruption.ctor += DaddyCorruption_ctor;
        }

        internal static void RemoveHooks()
        {
            On.DaddyCorruption.ctor -= DaddyCorruption_ctor;
        }

        /// <summary>
        ///  Thank you Thrithralas.
        /// </summary>
        private static void DaddyCorruption_ctor(On.DaddyCorruption.orig_ctor orig, DaddyCorruption self, Room room)
        {
            orig(self, room);
            if (room != null && room.world != null && !room.world.singleRoomWorld)
            {
                foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
                {
                    if (CustomWorldMod.installedPacks[keyValues.Key].regionConfig.
                        TryGetValue(room.world.region.name, out CustomWorldStructs.RegionConfiguration config))
                    {
                        if (!config.bllVanilla)
                        {
                            CustomWorldMod.Log($"Spawning custom Daddy corruption in [{room.world.region.name}] from " +
                                $"[{CustomWorldMod.installedPacks[keyValues.Key].name}]", false, CustomWorldMod.DebugLevel.FULL);
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
