using CustomRegions.Mod;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CustomRegions
{
    public static class OverWorldHook
    {
        public static void ApplyHooks()
        {
            On.OverWorld.LoadFirstWorld += OverWorld_LoadFirstWorld;
            On.OverWorld.GetRegion += OverWorld_GetRegion;
            On.OverWorld.GetRegion_1 += OverWorld_GetRegion_1;

        }

        /// <summary>
        /// Adds the new regions found in all region.txt files to the OverWorld.regions[]
        /// Will replace this method, reducing compability
        /// </summary>
        private static void OverWorld_LoadFirstWorld(On.OverWorld.orig_LoadFirstWorld orig, OverWorld self)
        {
            int num = self.regions[self.regions.Length - 1].firstRoomIndex;
            int regionNumber = self.regions[self.regions.Length - 1].regionNumber + 1;

            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
            {
                Debug.Log($"Custom Regions: Loading new regions");
                string path = CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar + "World" + Path.DirectorySeparatorChar + "Regions";

                // IF FILE EXIST
                string[] regions = Directory.GetDirectories(path);
                foreach (string regionEach in regions)
                {
                    string regionToAdd = Path.GetFileNameWithoutExtension(regionEach);
                    bool shouldAdd = true;

                    for (int i = 0; i < self.regions.Length; i++)
                    {
                        if (regionToAdd.Equals(self.regions[i].name))
                        {
                            shouldAdd = false;
                        }
                    }
                    if (shouldAdd)
                    {
                        Array.Resize(ref self.regions, self.regions.Length + 1);
                        self.regions[self.regions.Length - 1] = new Region(regionToAdd, num, regionNumber);
                        Debug.Log($"Custom Regions: Added new region [{regionToAdd}] from [{keyValues.Value}]. Number of rooms [{num}] - Region number [{regionNumber}]");
                        num += self.regions[self.regions.Length - 1].numberOfRooms;
                        regionNumber++;
                    }
                }

            }

            bool flag;
            string text;
            if (self.game.IsArenaSession)
            {
                flag = true;
                text = self.game.GetArenaGameSession.arenaSitting.GetCurrentLevel;
            }
            else if (self.game.manager.menuSetup.startGameCondition == ProcessManager.MenuSetup.StoryGameInitCondition.Dev)
            {
                //IF FILE EXIST!
                string[] array = File.ReadAllLines(Custom.RootFolderDirectory() + "setup.txt");
                text = Regex.Split(array[0], ": ")[1];
                flag = !self.game.setupValues.world;
            }
            else if (self.game.manager.menuSetup.startGameCondition == ProcessManager.MenuSetup.StoryGameInitCondition.RegionSelect || self.game.manager.menuSetup.FastTravelInitCondition)
            {
                text = self.game.manager.menuSetup.regionSelectRoom;
                flag = false;
            }
            else
            {
                text = (self.game.session as StoryGameSession).saveState.denPosition;
                flag = false;
            }
            if (self.game.startingRoom != string.Empty)
            {
                text = self.game.startingRoom;
            }
            string text2 = Regex.Split(text, "_")[0];

            if (!flag)
            {
                bool flag2 = false;
                foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
                {
                    string path = CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar;

                    if (Directory.Exists(string.Concat(new object[]
                    {
                        Custom.RootFolderDirectory(),
                        path.Replace('/', Path.DirectorySeparatorChar),
                        Path.DirectorySeparatorChar,
                        "World",
                        Path.DirectorySeparatorChar,
                        "Regions",
                        Path.DirectorySeparatorChar,
                        text2
                    })))
                    {
                        flag2 = true;
                        break;
                    }
                    else if (Regex.Split(text, "_").Length > 2 && Directory.Exists(string.Concat(new object[]
                    {
                        Custom.RootFolderDirectory(),
                        path.Replace('/', Path.DirectorySeparatorChar),
                        Path.DirectorySeparatorChar,
                        "World",
                        Path.DirectorySeparatorChar,
                        "Regions",
                        Path.DirectorySeparatorChar,
                        Regex.Split(text, "_")[1]
                    })))
                    {
                        text2 = Regex.Split(text, "_")[1];
                        flag2 = true;
                        break;
                    }
                }
                if (!flag2)
                {
                    flag = true;
                }
            }

            if (flag)
            {
                Debug.Log("Custom Regions: Using default LoadFirstWorld");
                orig(self);
                return;
                //self.LoadWorld(text, self.PlayerCharacterNumber, true);
            }
            else
            {
                self.LoadWorld(text2, self.PlayerCharacterNumber, false);
            }

            self.FIRSTROOM = text;
        }

        /// <summary>
        /// Used for debugging purposes
        /// </summary>
        private static Region OverWorld_GetRegion_1(On.OverWorld.orig_GetRegion_1 orig, OverWorld self, string rName)
        {
            Debug.Log($"Custom Region: Getting region. AbstractRoom [{rName}]).");

            string debug = "Custom Region: All regions: {";
            for (int i = 0; i < self.regions.Length; i++)
            {

                debug += $" {self.regions[i].name},";

            }
            debug += "}";

            Debug.Log(debug);

            return orig(self, rName);
        }

        /// <summary>
        /// Used for debugging purposes
        /// </summary>
        private static Region OverWorld_GetRegion(On.OverWorld.orig_GetRegion orig, OverWorld self, AbstractRoom room)
        {
            string[] array = Regex.Split(room.name, "_");
            Debug.Log($"Custom Region: Getting region. AbstractRoom [{room.name}] (splitted {array[0]}).");
            if (array.Length == 2)
            {
                Debug.Log($"Custom Region: Region obtained [{self.GetRegion(array[0])}]");
            }
            string debug = "Custom Region: All regions: {";
            for (int i = 0; i < self.regions.Length; i++)
            {

                    debug += $" {self.regions[i]},";

            }
            debug += "}";

            Debug.Log(debug);

            return orig(self, room);
        }





        // UNUSED
        /* private static void OverWorld_LoadWorld(On.OverWorld.orig_LoadWorld orig, OverWorld self, string worldName, int playerCharacterNumber, bool singleRoomWorld)
         {
             if (worldName.Length > 2)
             {
                 Debug.Log($"Custom World: ERROR! splitting worldName {worldName}");
                 string text2 = Regex.Split(worldName, "_")[0];

                 foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
                 {
                     Debug.Log($"Custom Regions: Searching room in {keyValues.Value}");
                     string path = CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar;
                     bool flag2 = false;
                     if (Directory.Exists(string.Concat(new object[]
                     {
                         Custom.RootFolderDirectory(),
                         path.Replace('/', Path.DirectorySeparatorChar),
                         Path.DirectorySeparatorChar,
                         "World",
                         Path.DirectorySeparatorChar,
                         "Regions",
                         Path.DirectorySeparatorChar,
                         text2
                     })))
                     {
                         Debug.Log($"Custom Regions: Loading world [{text2}] from {keyValues.Value}");
                         flag2 = true;
                         break;
                     }
                 }
                 worldName = text2;
             }

             Debug.Log($"Custom Regions: Loading world - Worldname [{worldName}]");
             orig(self, worldName, playerCharacterNumber, singleRoomWorld);
         }*/
    }
}
