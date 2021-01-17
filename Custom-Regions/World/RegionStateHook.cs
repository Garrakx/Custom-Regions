using CustomRegions.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CustomRegions.CWorld
{
    static class RegionStateHook
    {
        public static void ApplyHooks()
        {
            // DEBUG
            On.RegionState.ctor += RegionState_ctor;
        }

        private static void RegionState_ctor(On.RegionState.orig_ctor orig, RegionState self, SaveState saveState, World world)
        {
            orig(self, saveState, world);

            CustomWorldMod.Log($"DEBUG: regionLoadString [{saveState.regionLoadStrings[world.region.regionNumber]}]. ");

        }
    }
}
