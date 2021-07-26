using CustomRegions.Mod;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CustomRegions.CWorld
{
    public static class OverWorldHook
    {
        public static string textLoadWorld = null;
        public static bool? singleWorld = null;


        public static void ApplyHooks()
        {
            On.OverWorld.LoadFirstWorld += OverWorld_LoadFirstWorld;

            On.OverWorld.GetRegion_1 += OverWorld_GetRegion_1;
            On.OverWorld.LoadWorld += OverWorld_LoadWorld;
        }

        public static void RemoveHooks()
        {
            On.OverWorld.LoadFirstWorld -= OverWorld_LoadFirstWorld;

            On.OverWorld.GetRegion_1 -= OverWorld_GetRegion_1;
            On.OverWorld.LoadWorld -= OverWorld_LoadWorld;
        }

        private static void OverWorld_LoadWorld(On.OverWorld.orig_LoadWorld orig, OverWorld self, string worldName, 
            int playerCharacterNumber, bool singleRoomWorld)
        {
            CustomWorldMod.Log($"Loading world. Worldname [{worldName}], using [{textLoadWorld ?? worldName}]. SingleWorld [{singleWorld ?? singleRoomWorld}]");
            orig(self, textLoadWorld ?? worldName, playerCharacterNumber, singleWorld ?? singleRoomWorld);
            // TEST THIS (I think it is no longer necessary)
            textLoadWorld = null;
            singleWorld = null;
        }


        private static void OverWorld_LoadFirstWorld(On.OverWorld.orig_LoadFirstWorld orig, OverWorld self)
        {
            textLoadWorld = null;
            singleWorld = null;

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
            else if (self.game.manager.menuSetup.startGameCondition == 
                ProcessManager.MenuSetup.StoryGameInitCondition.RegionSelect || self.game.manager.menuSetup.FastTravelInitCondition)
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
                string directory = Custom.RootFolderDirectory() + "World" + 
                    Path.DirectorySeparatorChar + "Regions" + Path.DirectorySeparatorChar + text2;

                string directory2 = Custom.RootFolderDirectory() + "World" + 
                    Path.DirectorySeparatorChar + "Regions" + Path.DirectorySeparatorChar + Regex.Split(text, "_")[1];

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
                    foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
                    {
                        string customDirectory = Custom.RootFolderDirectory() + CustomWorldMod.resourcePath + keyValues.Value + 
                            Path.DirectorySeparatorChar + "World" + Path.DirectorySeparatorChar + "Regions" + Path.DirectorySeparatorChar + text2;

                        CustomWorldMod.Log($"CustomDirectory [{customDirectory}]");
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
            CustomWorldMod.Log($"Getting region. AbstractRoom [{rName}]).");

            CustomWorldMod.Log($"Loaded regions [{string.Join(", ", self.regions.Select(x => x.name).ToArray())}]");

            return orig(self, rName);
        }

        public static void AddMissingRegions(OverWorld self)
        {
            int num = self.regions[self.regions.Length - 1].firstRoomIndex;
            int regionNumber = self.regions[self.regions.Length - 1].regionNumber + 1;
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
            {
                foreach(string regionToAdd in CustomWorldMod.installedPacks[keyValues.Key].regions)
                {
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
                        CustomWorldMod.Log($"Added new region [{regionToAdd}] from [{keyValues.Value}]. " +
                            $"Number of rooms [{self.regions[self.regions.Length - 1].numberOfRooms}]. Region number [{regionNumber}]");
                        num += self.regions[self.regions.Length - 1].numberOfRooms;
                        regionNumber++;
                    }
                }
                
            }
        }
    }
}
