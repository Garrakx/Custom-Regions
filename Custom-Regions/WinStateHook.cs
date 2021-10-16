using System.Collections.Generic;
using CustomRegions.Mod;

namespace CustomRegions
{
    static class WinStateHook
    {
        public static void ApplyHooks()
        {
            On.WinState.CreateAndAddTracker += WinState_CreateAndAddTracker;
        }

        private static WinState.EndgameTracker WinState_CreateAndAddTracker(On.WinState.orig_CreateAndAddTracker orig, WinState.EndgameID ID, List<WinState.EndgameTracker> endgameTrackers)
        {
            WinState.EndgameTracker endgameTracker = null;
            if (ID == WinState.EndgameID.Traveller)
            {
                int slots = CustomWorldMod.numberOfVanillaRegions + CustomWorldMod.activeModdedRegions.Count;
                endgameTracker = new WinState.BoolArrayTracker(ID, slots);

                if (endgameTracker != null && endgameTrackers != null)
                {
                    endgameTrackers.Add(endgameTracker);
                }
                CustomWorldMod.Log($"Expanded EndGameTracker to [{slots}]");
                return endgameTracker;
            }

            return orig(ID, endgameTrackers);
        }
    }
}
