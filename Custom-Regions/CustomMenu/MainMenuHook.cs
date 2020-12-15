using CustomRegions.Mod;
using System.Collections.Generic;

namespace CustomRegions.CustomMenu
{
    static class MainMenuHook
    {
        public static void ApplyHooks()
        {
            //On.Menu.MainMenu.BackgroundScene += MainMenu_BackgroundScene;
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
                foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
                {
                    if (keyValues.Key.Equals(menuRegion))
                    {
                        CustomWorldMod.Log($"Custom Regions: TitleSceneID {menuRegion}");
                    }

                }
            }
            return orig(self);
        }
    }
}
