using CustomRegions.Mod;

namespace CustomRegions
{
    static class RainWorldHook
    {
        public static void ApplyHooks()
        {
            On.RainWorld.Start += RainWorld_Start;
            On.RainWorldGame.RestartGame += RainWorldGame_RestartGame;
        }

        public static void RemoveHooks()
        {
            On.RainWorld.Start -= RainWorld_Start;
            On.RainWorldGame.RestartGame -= RainWorldGame_RestartGame;
        }

        private static void RainWorld_Start(On.RainWorld.orig_Start orig, RainWorld self)
        {
            CustomWorldMod.LoadCustomWorldResources();
            CustomWorldMod.rainWorldInstance = self;
            CustomWorldMod.Log($"Assigning RW instance. Null [{CustomWorldMod.rainWorldInstance == null}]", false, CustomWorldMod.DebugLevel.MEDIUM);

            orig(self);
        }

        // Only called from Dev-tools R
        private static void RainWorldGame_RestartGame(On.RainWorldGame.orig_RestartGame orig, RainWorldGame self)
        {
            orig(self);

            if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftShift))
            {
                CustomWorldMod.Log("\nResetting CRS ... \n");
                CustomWorldMod.LoadCustomWorldResources();

                // Line above resets progression, including map discovery textures...
                // Fix nullref on map discovery in the frame of the reset lol
                for (int num = 0; num < self.cameras.Length; num++)
                {
                    self.cameras[num].hud.ResetMap(new HUD.Map.MapData(self.world, self.rainWorld));
                    if (self.cameras[num].hud.textPrompt.subregionTracker != null)
                    {
                        self.cameras[num].hud.textPrompt.subregionTracker.lastShownRegion = 0;
                    }
                }
            }
            else
            {
                CustomWorldMod.Log("Hold [shift]+[r] to also reset CRS packs and configuration");
            }
        }

    }
}
