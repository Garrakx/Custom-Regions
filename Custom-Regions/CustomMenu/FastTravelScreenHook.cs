using CustomRegions.Mod;
using Menu;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;


namespace CustomRegions.CustomMenu
{
    static class FastTravelScreenHook
    {
        public static string temporalShelter;

        public static void ApplyHooks()
        {

            On.Menu.FastTravelScreen.GetRegionOrder += FastTravelScreen_GetRegionOrder;
            On.Menu.FastTravelScreen.TitleSceneID += FastTravelScreen_TitleSceneID;
            On.Menu.FastTravelScreen.InitiateRegionSwitch += FastTravelScreen_InitiateRegionSwitch;
        }

        private static void FastTravelScreen_InitiateRegionSwitch(On.Menu.FastTravelScreen.orig_InitiateRegionSwitch orig, FastTravelScreen self, int switchToRegion)
        {
            if (switchToRegion == 0 && self.currentRegion == 0)
            {
                if (temporalShelter != null)
                {
                    self.currentShelter = temporalShelter;
                    temporalShelter = null;
                }
                string shelter = self.currentShelter ?? string.Empty;
                CustomWorldMod.Log($"Initiate Region switch, called from Fast Travel ctor... [{shelter}]");
                int num = 0;
                string[] array = File.ReadAllLines(string.Concat(new object[]
                {
                    Custom.RootFolderDirectory(),
                    "World",
                    Path.DirectorySeparatorChar,
                    "Regions",
                    Path.DirectorySeparatorChar,
                    "regions.txt"
                }));

                array = CustomWorldMod.AddModdedRegions(array);

                self.allRegions = new Region[array.Length];
                for (int num2 = 0; num2 < self.allRegions.Length; num2++)
                {
                    self.allRegions[num2] = new Region(array[num2], num, num2);
                    num += self.allRegions[num2].numberOfRooms;
                }
                self.loadedWorlds = new World[self.accessibleRegions.Count];
                self.loadedMapData = new HUD.Map.MapData[self.accessibleRegions.Count];
                if (self.currentShelter != null)
                {
                    for (int num3 = 0; num3 < self.accessibleRegions.Count; num3++)
                    {
                        if (self.allRegions[self.accessibleRegions[num3]].name == self.currentShelter.Substring(0, 2))
                        {
                            CustomWorldMod.Log(self.currentShelter);
                            CustomWorldMod.Log(string.Concat(new object[]
                            {
                                "found start region: ",
                                num3,
                                " ",
                                self.allRegions[self.accessibleRegions[num3]].name
                            }));
                            self.currentRegion = num3;
                            break;
                        }
                    }
                }
                switchToRegion = self.currentRegion;
            }

            orig(self, switchToRegion);
        }



        public static List<string> FastTravelScreen_GetRegionOrder(On.Menu.FastTravelScreen.orig_GetRegionOrder orig)
        {
            /* <3 SLIME CUBED <3 */
            List<string> order = (List<string>)orig.Method.Invoke(orig.Target, new object[] { });
            /* <3 SLIME CUBED <3 */

            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
            {
                if (!order.Contains(keyValues.Key))
                {
                    order.Add(keyValues.Key);
                }
            }
            CustomWorldMod.Log($"GETREGIONORDER SANITY CHECK ~ [{string.Join(", ", order.ToArray())}]");
            return order;
        }

        /// <summary>
        /// in FastTravelScreen - Searchs for custom SceneID, sets the the currentShelter to null to avoid nullref and stores it in a static var
        /// </summary>
        private static Menu.MenuScene.SceneID FastTravelScreen_TitleSceneID(On.Menu.FastTravelScreen.orig_TitleSceneID orig, Menu.FastTravelScreen self, string regionName)
        {
            // Debug
            CustomWorldMod.Log($"Accesible regions count [{self.accessibleRegions.Count}] out of [{FastTravelScreen.GetRegionOrder().Count}]");

            //CustomWorldMod.sceneCustomID = string.Empty;
            MenuScene.SceneID ID = MenuScene.SceneID.Empty;
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
            {
                if (keyValues.Key.Equals(regionName))
                {
                    CustomWorldMod.Log($"Custom Regions: TitleSceneID {regionName}");
                    try
                    {
                        ID = (MenuScene.SceneID)Enum.Parse(typeof(MenuScene.SceneID), $"Landscape_{regionName}");

                    }
                    catch (Exception e)
                    {
                        CustomWorldMod.Log($"Enum not found [{e}]");
                    }
                    break;
                }

            }

            if (orig(self, regionName) == Menu.MenuScene.SceneID.Empty && ID != MenuScene.SceneID.Empty)
            {
                CustomWorldMod.Log($"Custom Regions: TitleSceneID. Using custom SceneID [{ID}]");

                // removing the current shelter to avoid Array index out of range in Fastravelscreen ctor. 
                temporalShelter = self.currentShelter;
                self.currentShelter = null;

                return ID;
            }

            return orig(self, regionName);
        }
    }
}
