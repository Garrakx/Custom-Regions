using CustomRegions.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CustomRegions.CustomMenu
{
    static class PauseMenuHook
    {
        public static void ApplyHooks()
        {
            On.Menu.PauseMenu.ctor += PauseMenu_ctor;
        }

        private static void PauseMenu_ctor(On.Menu.PauseMenu.orig_ctor orig, Menu.PauseMenu self, ProcessManager manager, RainWorldGame game)
        {
            orig(self, manager, game);

            // Log error
            if (CustomWorldMod.crashPlacedObjects)
            {
                string textError = "Error while loading placed objects, you might be missing dependencies";
                Menu.MenuLabel errorLabel = new Menu.MenuLabel(self, self.pages[0], textError, 
                    new Vector2(CustomWorldMod.rainWorldInstance.options.ScreenSize.x/2, ( 20f)), default(Vector2), true);
                errorLabel.label.color = Color.red;
                self.pages[0].subObjects.Add(errorLabel);
            }
        }
    }
}
