using System.Collections.Generic;
using CustomRegions.Mod;

namespace CustomRegions
{
    static class WinStateHook
    {
        public static void ApplyHook()
        {
            On.WinState.CreateAndAddTracker += WinState_CreateAndAddTracker;

           // On.WinState.CycleCompleted += WinState_CycleCompleted;
        }

        // NOT NEEDED ANYMORE
        /*
        private static void WinState_CycleCompleted(On.WinState.orig_CycleCompleted orig, WinState self, RainWorldGame game)
        {
            orig(self, game);

			int num2 = -1;

			for (int i = 0; i < game.GetStorySession.playerSessionRecords.Length; i++)
			{
				if (game.GetStorySession.playerSessionRecords[i] != null)
				{
					PlayerSessionRecord playerSessionRecord = game.GetStorySession.playerSessionRecords[i];

					if (playerSessionRecord.wentToSleepInRegion != playerSessionRecord.wokeUpInRegion)
					{
						List<string> regionOrder = FastTravelScreenHook.FastTravelScreen_GetRegionOrder(Menu.FastTravelScreen.GetRegionOrder);
						num2 = regionOrder.IndexOf(game.rainWorld.progression.regionNames[playerSessionRecord.wentToSleepInRegion]);
						game.rainWorld.progression.miscProgressionData.menuRegion = game.rainWorld.progression.regionNames[playerSessionRecord.wentToSleepInRegion];
					}
				}
			}
            CustomWorldMod.Log($"Custom Regions: Cycle completed. num2 [{num2}].");
			if (num2 >= 0)
			{
				WinState.BoolArrayTracker boolArrayTracker = self.GetTracker(WinState.EndgameID.Traveller, true) as WinState.BoolArrayTracker;
                CustomWorldMod.Log($"Custom Regions: Cycle completed. boolArrayTracker length [{boolArrayTracker.progress.Length}].");
                if (num2 < boolArrayTracker.progress.Length)
				{
					boolArrayTracker.progress[num2] = true;
				}
			}
		}
        */
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
