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
        public static string textLoadWorld = string.Empty;
        public static bool singleWorld = false;


        public static void ApplyHooks()
        {
            On.OverWorld.LoadFirstWorld += OverWorld_LoadFirstWorld;

            On.OverWorld.GetRegion_1 += OverWorld_GetRegion_1;
            On.OverWorld.LoadWorld += OverWorld_LoadWorld;
        }

        private static void OverWorld_LoadWorld(On.OverWorld.orig_LoadWorld orig, OverWorld self, string worldName, int playerCharacterNumber, bool singleRoomWorld)
        {
            CustomWorldMod.Log($"Custom Regions: Loading world. Worldname [{worldName}], using [{textLoadWorld}]. SingleWorld [{singleWorld}]");
            orig(self, textLoadWorld, playerCharacterNumber, singleWorld);
        }


        private static void OverWorld_LoadFirstWorld(On.OverWorld.orig_LoadFirstWorld orig, OverWorld self)
        {
            textLoadWorld = string.Empty;
            singleWorld = false;

            AddMissingRegions(self);

            bool flag = false;
            string text = string.Empty;

            if (self.game.IsArenaSession)
            {
                flag = true;
                text = self.game.GetArenaGameSession.arenaSitting.GetCurrentLevel;
            }
            else if (self.game.manager.menuSetup.startGameCondition == ProcessManager.MenuSetup.StoryGameInitCondition.Dev)
            {
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

                // Check vanilla first
                string directory = Custom.RootFolderDirectory() + "World" + Path.DirectorySeparatorChar + "Regions" + Path.DirectorySeparatorChar + text2;
                string directory2 = Custom.RootFolderDirectory() + "World" + Path.DirectorySeparatorChar + "Regions" + Path.DirectorySeparatorChar + Regex.Split(text, "_")[1];

                if (Directory.Exists(directory))
                {
                    flag2 = true;
                }
                else if (Regex.Split(text, "_").Length > 2 && Directory.Exists(directory2))
                {
                    text2 = Regex.Split(text, "_")[1];
                    flag2 = true;
                }

                // Check custom regions
                if (!flag2)
                {
                    text2 = Regex.Split(text, "_")[0];
                    foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
                    {
                        string customDirectory = Custom.RootFolderDirectory() + CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar + "World" + Path.DirectorySeparatorChar + "Regions" + Path.DirectorySeparatorChar + text2;
                        //string customDirectory2 = Custom.RootFolderDirectory() + CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar + "World" + Path.DirectorySeparatorChar + "Regions" + Path.DirectorySeparatorChar + Regex.Split(text, "_")[1];
                        CustomWorldMod.Log($"Custom Regions: CustomDirectory [{customDirectory}]");
                        //CustomWorldMod.Log($"Custom Regions: CustomDirectory2 [{customDirectory2}]");
                        if (Directory.Exists(customDirectory))
                        {
                            flag2 = true;
                            break;
                        }
                        else if (Regex.Split(text, "_").Length > 2 && Directory.Exists(directory2))
                        {
                            text2 = Regex.Split(text, "_")[1];
                            flag2 = true;
                            break;
                        }
                    }
                }


                if (!flag2)
                {
                    flag = true;
                }
            }


            // If arena or SetupWorld
            if (flag)
            {
                textLoadWorld = text;
                singleWorld = true;
            }
            else
            {
                textLoadWorld = text2;
                singleWorld = false;
            }

            self.FIRSTROOM = text;

            orig(self);
        }



        /// <summary>
        /// Used for debugging purposes
        /// </summary>
        private static Region OverWorld_GetRegion_1(On.OverWorld.orig_GetRegion_1 orig, OverWorld self, string rName)
        {
            CustomWorldMod.Log($"Custom Region: Getting region. AbstractRoom [{rName}]).");

            string debug = "Custom Region: All regions: {";
            for (int i = 0; i < self.regions.Length; i++)
            {
                debug += $" {self.regions[i].name},";
            }
            debug += "}";

            CustomWorldMod.Log(debug);

            return orig(self, rName);
        }

        public static void AddMissingRegions(OverWorld self)
        {
            int num = self.regions[self.regions.Length - 1].firstRoomIndex;
            int regionNumber = self.regions[self.regions.Length - 1].regionNumber + 1;

            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
            {
                string path = CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar + "World" + Path.DirectorySeparatorChar + "Regions";

                if (Directory.Exists(path))
                {
                    string[] regions = Directory.GetDirectories(path);
                    foreach (string regionEach in regions)
                    {
                        string regionToAdd = Path.GetFileNameWithoutExtension(regionEach);
                        if (regionToAdd != keyValues.Key)
                        {
                            continue;
                        }

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
                            CustomWorldMod.Log($"Custom Regions: Added new region [{regionToAdd}] from [{keyValues.Value}]. Number of rooms [{self.regions[self.regions.Length - 1].numberOfRooms}]. Region number [{regionNumber}]");
                            num += self.regions[self.regions.Length - 1].numberOfRooms;
                            regionNumber++;
                        }
                    }
                }
            }
        }





        /* private static void OverWorld_LoadWorld(On.OverWorld.orig_LoadWorld orig, OverWorld self, string worldName, int playerCharacterNumber, bool singleRoomWorld)
         {
             if (worldName.Length > 2)
             {
                 CustomWorldMod.CustomWorldLog($"Custom World: ERROR! splitting worldName {worldName}");
                 string text2 = Regex.Split(worldName, "_")[0];

                 foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
                 {
                     CustomWorldMod.CustomWorldLog($"Custom Regions: Searching room in {keyValues.Value}");
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
                         CustomWorldMod.CustomWorldLog($"Custom Regions: Loading world [{text2}] from {keyValues.Value}");
                         flag2 = true;
                         break;
                     }
                 }
                 worldName = text2;
             }

             CustomWorldMod.CustomWorldLog($"Custom Regions: Loading world - Worldname [{worldName}]");
             orig(self, worldName, playerCharacterNumber, singleRoomWorld);
         }*/
    }
}
