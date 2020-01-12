using CustomRegions.Mod;
using Menu;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CustomRegions
{
    class FastTravelScreenHook
    {
        public static void ApplyHooks()
        {
            On.Menu.FastTravelScreen.TitleSceneID += FastTravelScreen_TitleSceneID;
            On.Menu.FastTravelScreen.GetRegionOrder += FastTravelScreen_GetRegionOrder;

            // Debug
            On.Menu.FastTravelScreen.ctor += FastTravelScreen_ctor;
        }

        private static void FastTravelScreen_ctor(On.Menu.FastTravelScreen.orig_ctor orig, Menu.FastTravelScreen self, ProcessManager manager, ProcessManager.ProcessID ID)
        {
            List<string> regionOrder = FastTravelScreen_GetRegionOrder(Menu.FastTravelScreen.GetRegionOrder);
            string debug = "Custom Regions: {";
            foreach(string region in regionOrder)
            {
                debug += region +", ";
            }
            debug += " }";
            Debug.Log(debug);

            orig(self, manager, ID);

            Debug.Log($"Custom Regions: Accesible region number {self.accessibleRegions.Count}");

        }

        private static List<string> FastTravelScreen_GetRegionOrder(On.Menu.FastTravelScreen.orig_GetRegionOrder orig)
        {
            List<string> list = orig();
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
            {
                list.Add(keyValues.Key);
            }
            return list;
        }

        private static Menu.MenuScene.SceneID FastTravelScreen_TitleSceneID(On.Menu.FastTravelScreen.orig_TitleSceneID orig, Menu.FastTravelScreen self, string regionName)
        {
            CustomWorldMod.sceneCustomID = string.Empty;
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
            {
                if (keyValues.Key.Equals(regionName))
                {
                    Debug.Log($"Custom Regions: TitleSceneID {regionName}");
                    CustomWorldMod.sceneCustomID = regionName;
                }

            }

            // should return string.empty
            return orig(self, regionName);
        }
    }
}
