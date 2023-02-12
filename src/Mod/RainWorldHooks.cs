using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomRegions.Mod
{
    internal static class RainWorldHooks
    {
        public static void ApplyHooks()
        {
            On.RainWorldGame.RestartGame += RainWorldGame_RestartGame;
        }
        private static void RainWorldGame_RestartGame(On.RainWorldGame.orig_RestartGame orig, RainWorldGame self)
        {
            orig(self);

            if (UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftShift))
            {
                CustomRegionsMod.CustomLog("\nResetting CRS ... \n");
                CustomRegionsMod.CRSRefresh(true);

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
                CustomRegionsMod.CustomLog("Hold [shift]+[r] to also reset CRS packs and configuration");
            }
        }
    }
}
