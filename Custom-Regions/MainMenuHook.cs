using CustomRegions.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CustomRegions
{
    static class MainMenuHook
    {
        public static void ApplyHooks()
        {
            On.Menu.MainMenu.BackgroundScene += MainMenu_BackgroundScene;
        }


        /// <summary>
		/// in MenuBackGroundScene - Fills CustomWorldMod.sceneCustomID with the ID of the region to load, and sets the extendedSceneID enum to CustomSceneID.
		/// </summary>
        /// <returns>Vanilla World path</returns>
        private static Menu.MenuScene.SceneID MainMenu_BackgroundScene(On.Menu.MainMenu.orig_BackgroundScene orig, Menu.MainMenu self)
        {
            if (self.manager.rainWorld.progression.miscProgressionData.menuRegion != null)
            {
                string menuRegion = self.manager.rainWorld.progression.miscProgressionData.menuRegion;
                CustomWorldMod.sceneCustomID = string.Empty;
                foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
                {
                    if (keyValues.Key.Equals(menuRegion))
                    {
                        Debug.Log($"Custom Regions: TitleSceneID {menuRegion}");
                        CustomWorldMod.sceneCustomID = menuRegion;
                    }

                }
            }
            return orig(self);
        }
    }
}
