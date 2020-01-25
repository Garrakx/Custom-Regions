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
            On.Menu.FastTravelScreen.GetAccessibleShelterNamesOfRegion += FastTravelScreen_GetAccessibleShelterNamesOfRegion;
        }

        private static List<string> FastTravelScreen_GetAccessibleShelterNamesOfRegion(On.Menu.FastTravelScreen.orig_GetAccessibleShelterNamesOfRegion orig, FastTravelScreen self, string regionAcronym)
        {
            List<string> ori = orig(self, regionAcronym);
            string debug = string.Empty;
            if (ori != null)
            {
                foreach (string s in ori)
                {
                    debug += s + "/";
                }
            }
            Debug.Log($"Custom Regions: GetAccesibleShelter. RegionAcronym [{regionAcronym}]. List:[{debug}]");
            return ori;
        }

        private static void FastTravelScreen_ctor(On.Menu.FastTravelScreen.orig_ctor orig, Menu.FastTravelScreen self, ProcessManager manager, ProcessManager.ProcessID ID)
        {
            List<string> regionOrderHook = FastTravelScreen_GetRegionOrder(FastTravelScreen.GetRegionOrder);
            string debug = "Custom Regions: FastTravelHook {";
            foreach (string region in regionOrderHook)
            {
                debug += region + ", ";
            }
            debug += "}";
            Debug.Log(debug);


            // DEBUG
            string[] playerShelters = new string[3];
            for (int i = 0; i < playerShelters.Length; i++)
            {
                if (manager.rainWorld.progression.IsThereASavedGame(i))
                {
                    playerShelters[i] = manager.rainWorld.progression.ShelterOfSaveGame(i);
                }
            }
            string currentShelter = "SU_S01";
            if (manager.rainWorld.progression.PlayingAsSlugcat >= 0 && manager.rainWorld.progression.PlayingAsSlugcat < playerShelters.Length && playerShelters[manager.rainWorld.progression.PlayingAsSlugcat] != null)
            {
                currentShelter = playerShelters[manager.rainWorld.progression.PlayingAsSlugcat];
            }
            else
            {
                for (int j = 0; j < playerShelters.Length; j++)
                {
                    if (playerShelters[j] != null)
                    {
                        currentShelter = playerShelters[j];
                        break;
                    }
                }
            }
            /*
            List<string> regionOrder = FastTravelScreen_GetRegionOrder(FastTravelScreen.GetRegionOrder);
            for (int k = 0; k < regionOrder.Count; k++)
            {
                for (int l = 0; l < manager.rainWorld.progression.regionNames.Length; l++)
                {
                    if (regionOrder[k] == manager.rainWorld.progression.regionNames[l])
                    {
                        Debug.Log($"Custom Regions: Potential accesible regions [{regionOrder[k]}]");
                        int num = -1;
                        for (int i = 0; i < self.manager.rainWorld.progression.regionNames.Length; i++)
                        {
                            if (regionOrder[k] == self.manager.rainWorld.progression.regionNames[i])
                            {
                                num = i;
                                break;
                            }
                        }

                        if (self.manager.rainWorld.progression.miscProgressionData.discoveredShelters[num] == null)
                        {
                            Debug.Log("Custom Regions: ERROR! no discovered shelters");
                        }
                        else
                        {
                            debug = "Custom Regions: Discovered shelters [";
                            foreach(string s in self.manager.rainWorld.progression.miscProgressionData.discoveredShelters[num])
                            {
                                debug += s + "/"; 
                            }
                        }
                        debug += "]";
                        Debug.Log(debug);
                        //
                        //if (self.GetAccessibleShelterNamesOfRegion(manager.rainWorld.progression.regionNames[l]) != null)
                        //{
                         //   Debug.Log($"Custom Regions: Found accesible region [{regionOrder[k]}]");
                        //}
                        //
                    }
                }
            }
            */
            // DEBUG END

            orig(self, manager, ID);

            Debug.Log($"Custom Regions: Accesible region count {self.accessibleRegions.Count}");

            string debug2 = "Custom Regions: Player Shelters {";
            for (int j = 0; j < self.playerShelters.Length; j++)
            {
                if (self.playerShelters[j] != null)
                {
                    debug2 += self.playerShelters[j] + ", ";
                }
            }
            debug2 += " }";
            Debug.Log(debug2);
            Debug.Log($"Custom Regions: Current Shelter {self.currentShelter}");

            string debug3 = "Custom Regions: region Names {";
            for (int l = 0; l < manager.rainWorld.progression.regionNames.Length; l++)
            {
                debug3 += manager.rainWorld.progression.regionNames[l] + ", ";
            }
            debug3 += " }";
            Debug.Log(debug3);
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
