using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CustomRegions.CustomMenu;
using CustomRegions.Mod;
using UnityEngine;

namespace CustomRegions
{
    static class WinStateHook
    {
        public static void ApplyHook()
        {
            On.WinState.CreateAndAddTracker += WinState_CreateAndAddTracker;
            On.WinState.CycleCompleted += WinState_CycleCompleted;
        }

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
            CustomWorldMod.CustomWorldLog($"Custom Regions: Cycle completed. num2 [{num2}].");
			if (num2 >= 0)
			{
				WinState.BoolArrayTracker boolArrayTracker = self.GetTracker(WinState.EndgameID.Traveller, true) as WinState.BoolArrayTracker;
                CustomWorldMod.CustomWorldLog($"Custom Regions: Cycle completed. boolArrayTracker length [{boolArrayTracker.progress.Length}].");
                if (num2 < boolArrayTracker.progress.Length)
				{
					boolArrayTracker.progress[num2] = true;
				}
			}
		}

        private static WinState.EndgameTracker WinState_CreateAndAddTracker(On.WinState.orig_CreateAndAddTracker orig, WinState.EndgameID ID, List<WinState.EndgameTracker> endgameTrackers)
        {
            WinState.EndgameTracker endgameTracker = null;
            if (ID == WinState.EndgameID.Traveller)
            {
                endgameTracker = new WinState.BoolArrayTracker(ID, 12 + CustomWorldMod.loadedRegions.Count);

                if (endgameTracker != null && endgameTrackers != null)
                {
                    endgameTrackers.Add(endgameTracker);
                }
                return endgameTracker;
            }


            return orig(ID, endgameTrackers);
        }
    }
}
