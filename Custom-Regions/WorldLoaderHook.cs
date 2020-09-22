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

    // This code comes from EasyModPack by topicular
    // Adapted to work with any region by Garrakx

    static class WorldLoaderHook
    {
        public static void ApplyHooks()
        {
            On.WorldLoader.FindRoomFileDirectory += WorldLoader_FindRoomFileDirectory;
            On.WorldLoader.NextActivity += WorldLoader_NextActivity;
            On.WorldLoader.ctor += WorldLoader_ctor;

            // DEBUG
            //On.WorldLoader.MappingRooms += WorldLoader_MappingRooms;

        }

        /// <summary>
        /// All the lists are not getting initialized, so we have to initialize here.
        /// </summary>
        public static void InitializeWorldLoaderList(WorldLoader self)
        {
            if (self.roomAdder == null)
            {
                self.roomAdder = new List<string[]>();
            }
            if (self.roomTags == null)
            {
                self.roomTags = new List<List<string>>();
            }
            if (self.swarmRoomsList == null)
            {
                self.swarmRoomsList = new List<int>();
            }
            if (self.sheltersList == null)
            {
                self.sheltersList = new List<int>();
            }
            if (self.gatesList == null)
            {
                self.gatesList = new List<int>();
            }
            if (self.faultyExits == null)
            {
                self.faultyExits = new List<WorldCoordinate>();
            }
            if (self.abstractRooms == null)
            {
                self.abstractRooms = new List<AbstractRoom>();
            }
            if (self.spawners == null)
            {
                self.spawners = new List<World.CreatureSpawner>();
            }
            if (self.tempBatBlocks == null)
            {
                self.tempBatBlocks = new List<WorldLoader.BatMigrationBlockage>();
            }
        }

        /// <summary>
        /// Debugging purposes
        /// </summary>
        private static void WorldLoader_MappingRooms(On.WorldLoader.orig_MappingRooms orig, WorldLoader self)
        {
            try
            {
                string[] array = Regex.Split(self.lines[self.cntr], " : ");
                if (array.Length < 2)
                {
                    return;
                }
                string[] array2 = Regex.Split(array[1], ", ");
                string debug = $"Custom Regions: Mapping rooms: ";
                foreach (string lines in array)
                {
                    debug += $" {lines},";
                }
                CustomWorldMod.CustomWorldLog(debug);

            }
            catch (Exception e)
            {
                CustomWorldMod.CustomWorldLog($"Custom Regions: Mapping rooms failed, reason: {e}");
            }

            orig(self);
        }

        /// <summary>
        /// Vanilla RW does not check if the region about to load does exist. When we enter a custom region the game will try to look for the world files in the root folder.
        /// There should be a better way to do this, but if the region is custom I replace the ctor completly.
        /// </summary>
        private static void WorldLoader_ctor(On.WorldLoader.orig_ctor orig, WorldLoader self, RainWorldGame game, int playerCharacter, bool singleRoomWorld, string worldName, Region region, RainWorldGame.SetupValues setupValues)
        {
           /* try
            {
                CustomWorldMod.CustomWorldLog($"Custom Regions: Creating WorldLoader : Game [{game}]. PlayerCharacter [{playerCharacter}]. SingleRoomWorld [{singleRoomWorld}]. WorldName [{worldName}]");
            }
            catch (Exception e) { CustomWorldMod.CustomWorldLog($"Custom Reginons: Error ar WorldLoaderCtor [{e}]"); }
            */
            string pathRegion = string.Concat(new object[]
            {
                Custom.RootFolderDirectory(),
                Path.DirectorySeparatorChar,
                "World",
                Path.DirectorySeparatorChar,
                "Regions",
                Path.DirectorySeparatorChar,
                worldName,
                Path.DirectorySeparatorChar,
                "world_",
                worldName,
                ".txt"
            });


            if (!singleRoomWorld && File.Exists(pathRegion))
            {
                orig(self, game, playerCharacter, singleRoomWorld, worldName, region, setupValues);
            }
            else
            {
                // LOADING A CUSTOM REGION
                // THIS WILL REPLACE THE CTOR REDUCING COMPABILITY

                // INITIALIZING LISTS
                //CustomWorldMod.CustomWorldLog("Custom Worlds: Using custom WorldLoader ctor");
                try
                {
                    InitializeWorldLoaderList(self);
                }
                catch (Exception e)
                {
                    CustomWorldMod.CustomWorldLog($"Custom Worlds: something failed ERROR!!! [{e}]");
                }

                string path = CustomWorldMod.resourcePath + region + Path.DirectorySeparatorChar;

                self.game = game;
                self.playerCharacter = playerCharacter;
                self.world = new World(game, region, worldName, singleRoomWorld);
                self.singleRoomWorld = singleRoomWorld;
                self.worldName = worldName;
                self.setupValues = setupValues;
                self.lines = new List<string>();

                /*if (!singleRoomWorld)
                {
                    self.lines = getWorldLines(self);
                }*/
                if (!singleRoomWorld)
                {
                    self.simulateUpdateTicks = 100;
                    self.lines = getWorldLines(self);
                }
                self.NextActivity();
            }
        }


        /// <summary>
        /// If finds the room in the CustomAssets folder, returns that path (takes priority over vanilla)
        /// </summary>
        /// <returns>returns path to room</returns>
        private static string WorldLoader_FindRoomFileDirectory(On.WorldLoader.orig_FindRoomFileDirectory orig, string roomName, bool includeRootDirectory)
        {
            //if (!enabled) { return orig(roomName, includeRootDirectory); }

            string result = "";
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
            {
                string pathToCustomFolder = CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar;

                //string test = Custom.RootFolderDirectory() + pathToCustomFolder + "World" + Path.DirectorySeparatorChar + "Regions" + Path.DirectorySeparatorChar + Regex.Split(roomName, "_")[0];
                //CustomWorldMod.CustomWorldLog($"Custom Regions: Finding room {roomName} in {keyValues.Key}. Path: {test}");

                string gatePath = pathToCustomFolder + "World" + Path.DirectorySeparatorChar + "Gates" + Path.DirectorySeparatorChar + roomName;
                string gateShelterPath = pathToCustomFolder + "World" + Path.DirectorySeparatorChar + "Gates" + Path.DirectorySeparatorChar + "Gate shelters" + Path.DirectorySeparatorChar + roomName;
                string regularRoomPath = pathToCustomFolder + "World" + Path.DirectorySeparatorChar + "Regions" + Path.DirectorySeparatorChar + Regex.Split(roomName, "_")[0];
                string arenaPath = pathToCustomFolder + "Levels" + Path.DirectorySeparatorChar + roomName;

                // room is regular room
                if (Directory.Exists(regularRoomPath) && File.Exists(regularRoomPath + Path.DirectorySeparatorChar + "Rooms" + Path.DirectorySeparatorChar + roomName + ".txt"))
                {
                    result = pathToCustomFolder + "World" + Path.DirectorySeparatorChar + "Regions" + Path.DirectorySeparatorChar + Regex.Split(roomName, "_")[0] + Path.DirectorySeparatorChar + "Rooms" + Path.DirectorySeparatorChar + roomName;
                    //CustomWorldMod.CustomWorldLog($"Custom Regions: Found room {roomName} in {keyValues.Key}. Path: {result}");
                }
                // room is GATE
                else if (Regex.Split(roomName, "_")[0] == "GATE" && File.Exists(Custom.RootFolderDirectory() + gatePath + ".txt"))
                {
                    result = gatePath;
                    //CustomWorldMod.CustomWorldLog($"Custom Regions: Found gate {roomName} in {keyValues.Key}. Path: {result}");
                }
                // room is Gate shelter
                else if (File.Exists(Custom.RootFolderDirectory() + gateShelterPath + ".txt"))
                {
                    result = gateShelterPath;
                    //CustomWorldMod.CustomWorldLog($"Custom Regions: Found gate_shelter {roomName} in {keyValues.Key}. Path: {result}");
                }
                // room is Arena
                else if(File.Exists(Custom.RootFolderDirectory() + arenaPath + ".txt"))
                {
                    result = arenaPath;
                    CustomWorldMod.CustomWorldLog($"Custom Regions: Found arena {roomName} in {keyValues.Key}. Path: {result}");
                }

                if (result != "")
                {
                    break;
                }
            }

            if (result != "")
            {
                // CustomWorldMod.CustomWorldLog("Using Custom Worldfile: " + result);
                if (includeRootDirectory)
                {
                    result = "file:///" + Custom.RootFolderDirectory() + result;
                }
                return result;
            }
            else
            {
                return orig(roomName, includeRootDirectory);
            }
        }

        /// <summary>
        /// Could be used for merging algorithm
        /// </summary>
        private static CustomWorldMod.MergeStatus status;

        /// <summary>
        /// Reads and loads all the world_XX.txt files found in all the custom worlds.
        /// TODO: a) Implement an algorithm that merges all those files b) Just use the last one loaded.
        /// </summary>
        public static List<string> getWorldLines(WorldLoader self)
        {
            List<string> lines = new List<string>();

            // Reset
            CustomWorldMod.analyzingLog = string.Empty;

            // Bool indicates whether it is vanilla or not
            List<CustomWorldMod.WorldDataLine> ROOMS = new List<CustomWorldMod.WorldDataLine>();
            List<CustomWorldMod.WorldDataLine> CREATURES = new List<CustomWorldMod.WorldDataLine>();
            List<CustomWorldMod.WorldDataLine> BATS = new List<CustomWorldMod.WorldDataLine>();

            if (self.lines.Count > 0)
            {
                // Fill ROOMS with vanilla rooms
                //CustomWorldMod.CustomWorldLog("Custom Regions: Found vanilla rooms");
                bool startRooms = false;
                bool startCreatures = false;
                bool startBats = false;

                foreach (string s in self.lines)
                {
                    // ROOMS
                    if (s.Equals("END ROOMS"))
                    {
                        startRooms = false;
                    }
                    if (startRooms)
                    {
                        ROOMS.Add(new CustomWorldMod.WorldDataLine(s, true));
                    }
                    if (s.Equals("ROOMS"))
                    {
                        startRooms = true;
                    }

                    // CREATURES
                    if (s.Equals("END CREATURES"))
                    {
                        startCreatures = false;
                    }
                    if (startCreatures)
                    {
                        CREATURES.Add(new CustomWorldMod.WorldDataLine(s, true));
                    }
                    if (s.Equals("CREATURES"))
                    {
                        startCreatures = true;
                    }

                    // BAT MIGRATIONS
                    if (s.Equals("END BAT MIGRATION BLOCKAGES"))
                    {
                        startBats = false;
                    }
                    if (startBats)
                    {
                        BATS.Add(new CustomWorldMod.WorldDataLine(s, true));
                    }
                    if (s.Equals("BAT MIGRATION BLOCKAGES"))
                    {
                        startBats = true;
                    }
                }
            }

            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
            {
                //CustomWorldMod.CustomWorldLog($"Custom Regions: Reading world_{self.worldName}.txt from {keyValues.Value}");
                string path = CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar;

                string test = string.Concat(new object[]
                {
                    Custom.RootFolderDirectory(),
                    path,
                    Path.DirectorySeparatorChar,
                    "World",
                    Path.DirectorySeparatorChar,
                    "Regions",
                    Path.DirectorySeparatorChar,
                    self.worldName,
                    Path.DirectorySeparatorChar,
                    "world_",
                    self.worldName,
                    ".txt"
                });

                if (File.Exists(test))
                {
                    CustomWorldMod.CustomWorldLog($"Custom Regions: Found world_{self.worldName}.txt from {keyValues.Value}");
                    //self.lines = new List<string>();
                    string[] array = File.ReadAllLines(test);
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (array[i].Length > 1 && array[i].Substring(0, 2) != "//")
                        {
                            bool flag = true;
                            if (array[i][0] == '(')
                            {
                                flag = false;
                                for (int j = 1; j < 20; j++)
                                {
                                    if (array[i][j] == ')')
                                    {
                                        string[] array2 = Regex.Split(array[i].Substring(1, j - 1), ",");
                                        for (int k = 0; k < array2.Length; k++)
                                        {
                                            if (array2[k] == self.playerCharacter.ToString())
                                            {
                                                array[i] = array[i].Substring(j + 1, array[i].Length - j - 1);
                                                if (array[i][0] == ' ')
                                                {
                                                    array[i] = array[i].Substring(1, array[i].Length - 1);
                                                }
                                                flag = true;
                                                break;
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                            if (flag)
                            {

                                if (array[i] == "ROOMS")
                                {
                                    status = CustomWorldMod.MergeStatus.ROOMS;
                                }
                                else if (array[i] == "CREATURES")
                                {
                                    status++;
                                }
                                else if (array[i] == "BAT MIGRATION BLOCKAGES")
                                {
                                    status++;
                                }
                                else if (array[i] != "END ROOMS" && array[i] != "END CREATURES" && array[i] != "END BAT MIGRATION BLOCKAGES")
                                {
                                    switch (status)
                                    {
                                        case CustomWorldMod.MergeStatus.ROOMS:
                                            ROOMS = CustomWorldMod.AddNewRoom(array[i], ROOMS, keyValues.Key);
                                            break;
                                        case CustomWorldMod.MergeStatus.CREATURES:
                                            // MERGE CREATURES
                                            CREATURES = CustomWorldMod.AddNewCreature(array[i], CREATURES); //CREATURES.Add(array[i]);
                                            break;
                                        case CustomWorldMod.MergeStatus.BATS:
                                            // MERGE BATS
                                            BATS = CustomWorldMod.AddNewBatBlockage(array[i], BATS);
                                            break;
                                    }
                                }
                                //lines.Add(array[i]);
                                //CustomWorldMod.CustomWorldLog(array[i]);
                            }
                        }
                    }
                    //break;
                }

            }


            // Sort lists to increase readability 
            List<string> sortedRooms = CustomWorldMod.fromWorldDataToList(ROOMS);
            sortedRooms.Sort();

            List<string> sortedCreatures = CustomWorldMod.fromWorldDataToList(CREATURES);
            sortedCreatures.Sort();

            List<string> sortedBats = CustomWorldMod.fromWorldDataToList(BATS);
            sortedBats.Sort();

            lines = CustomWorldMod.BuildWorldText(sortedRooms, sortedCreatures, sortedBats);


            if (lines.Count < 2)
            {
                CustomWorldMod.CustomWorldLog("Custom Regions: ERROR! Lines.Count < 2");
                return self.lines;
            }

            foreach (string s in lines)
            {
                CustomWorldMod.CustomWorldLog(s);
            }
            return lines;
        }

        /// <summary>
        /// Use new world_##.txt file
        /// </summary>
        private static void WorldLoader_NextActivity(On.WorldLoader.orig_NextActivity orig, WorldLoader self)
        {
            /*
            if (//!enabled ||
            self.activity != WorldLoader.Activity.Init || self.singleRoomWorld)
            {
                orig(self);
                return;
            }
            */

            if (self.activity == WorldLoader.Activity.Init && !self.singleRoomWorld)
            {
                if (self.lines == null)
                {
                    CustomWorldMod.CustomWorldLog("Custom Regions: World was null, creating new lines");
                    self.lines = new List<string>();
                }

                self.lines = getWorldLines(self);
            }
            else
            {
                //CustomWorldMod.CustomWorldLog($"Custom Worlds: Next Activity was not init, was {self.activity}");
            }

            if (self.faultyExits == null)
            {
                CustomWorldMod.CustomWorldLog($"Custom Regions: NextActivity failed, faultyExits is null");
                self.faultyExits = new List<WorldCoordinate>();
            }
            orig(self);
        }
    }
}
