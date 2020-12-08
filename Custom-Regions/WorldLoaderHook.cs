using CustomRegions.Mod;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using static CustomRegions.Mod.CustomWorldStructs;

namespace CustomRegions
{

    // This code comes from EasyModPack by topicular
    // Adapted to work with any region by Garrakx

    static class WorldLoaderHook
    {
        /// <summary>
        /// Enum used in the mergin process when loading the world_XX.txt file.
        /// </summary>
        public enum MergeStatus
        {
            ROOMS,
            CREATURES,
            BATS
        }


        public static void ApplyHooks()
        {
            On.WorldLoader.FindRoomFileDirectory += WorldLoader_FindRoomFileDirectory;
            On.WorldLoader.NextActivity += WorldLoader_NextActivity;
            On.WorldLoader.ctor += WorldLoader_ctor;

            // DEBUG
            //On.WorldLoader.MappingRooms += WorldLoader_MappingRooms;

        }

        /// <summary>
        /// Compares and merges a room-connection in the existing room list
        /// </summary>
        public static List<WorldDataLine> AddNewRoom(string newRoom, List<WorldDataLine> oldList, string modID)
        {
            // Check if this connection already exists
            bool sameConnections = false;

            // Holds the old line which needs to be merged or replaced
            string roomConnectionsToBeReplaced = string.Empty;

            foreach (WorldDataLine oldRoom in oldList)
            {
                if (oldRoom.data.Equals(newRoom))
                {
                    // The room connetions are exactly the same, skipped
                    sameConnections = true;
                    roomConnectionsToBeReplaced = string.Empty;
                    break;
                }
                else
                {
                    string oldBaseRoom = oldRoom.data.Substring(0, oldRoom.data.IndexOf(" "));
                    string newBaseRoom = newRoom.Substring(0, newRoom.IndexOf(" "));

                    if (oldBaseRoom.Equals(newBaseRoom))
                    {
                        // The room is the same but different connections
                        roomConnectionsToBeReplaced = oldRoom.data;

                        CustomWorldMod.Log($"Custom Regions: Found conflict. Existing room[{roomConnectionsToBeReplaced}] with room to be added [{newRoom}]");
                    }
                }
            }

            // Found a connection that needs to be replaced or merged
            if (roomConnectionsToBeReplaced != string.Empty)
            {

                CustomWorldMod.Log($"Custom Regions: Trying to merge [{roomConnectionsToBeReplaced}]");

                // Extract room name from line to be merged / replaced
                string roomToBeReplacedName = roomConnectionsToBeReplaced.Substring(0, roomConnectionsToBeReplaced.IndexOf(" "));

                // Check if the room to be merged / replaced is vanilla
                bool isVanilla = oldList.Find(x => x.data.Equals(roomConnectionsToBeReplaced)).vanilla;

                // Check if containts GATE/SHELTER/SWARMROOM at the end
                string endingSetting = string.Empty;
                string temp = roomConnectionsToBeReplaced.Substring(roomConnectionsToBeReplaced.IndexOf(": ") + 2);
                if (temp.IndexOf(": ") > 0)
                {
                    endingSetting = temp.Substring(temp.IndexOf(": ") + 2);
                }


                //----------------------
                // Build new connections
                //----------------------

                // Convert the line strings into Lists so it is easier to manipulate and read
                List<string> oldConnections = FromConnectionsToList(roomConnectionsToBeReplaced);
                List<string> newConnections = FromConnectionsToList(newRoom);

                // if isRoomBeingReplace is false, it means it got merged.
                bool isRoomBeingReplaced = false;

                // if allEmpty is true, there are not disconnnected pipes in the room
                bool noDisconnectedPipes = true;

                // Check if any operation took place
                bool performedOperation = false;


                ///------------------------------------------------------------------------------------------------------------
                /// HOW IT WORKS (subject to change)
                /// 
                /// If the mod is modifying a vanilla room, it will replace it completly. 
                /// Modders should make sure they don't occuppy all the pipes for that room.
                /// 
                /// If the mod is modifying a room that is either modded or modified by another mod, CR will try to merge both.
                /// 
                /// If this room does not have any empty exits, the two mods will be incompatible.
                /// 
                ///------------------------------------------------------------------------------------------------------------


                if (isVanilla)
                {
                    // A mod omitted to include DISCONNECTED in their lines, adding it to increase compability
                    if (newConnections.Count < oldConnections.Count)
                    {
                        for (int i = 0; i < (oldConnections.Count - newConnections.Count); i++)
                        {
                            newConnections.Add("DISCONNECTED");
                            performedOperation = true;
                        }
                        CustomWorldMod.Log($"Custom Regions: ERROR! Added compability to [{newRoom}]! It has less connections than vanilla! [{roomConnectionsToBeReplaced}]");
                        CustomWorldMod.Log($"Custom Regions: Converted to [{string.Join(", ", newConnections.ToArray())}]");
                    }

                    // The mod will replace the old lines since they are vanilla.
                    isRoomBeingReplaced = true;
                }

                else // Only merge if it is between two mods
                {
                    for (int j = 0; j < newConnections.Count; j++)
                    {
                        for (int i = 0; i < oldConnections.Count; i++)
                        {
                            if (!oldConnections[i].Equals(newConnections[j]))
                            {
                                // old connection has a DISCONNECTED / DISCONNECT pipe
                                bool oldConnectionDisconnected = oldConnections[i].Equals("DISCONNECTED") || oldConnections[i].Equals("DISCONNECT");

                                // new connection has a DISCONNECTED / DISCONNECT pipe
                                bool newConnectionDisconnected = newConnections[j].Equals("DISCONNECTED") || newConnections[j].Equals("DISCONNECT");

                                // The room to be merged has empty exits, so new mod will use those
                                if (oldConnectionDisconnected && !newConnectionDisconnected)
                                {
                                    if (!oldConnections.Contains(newConnections[j]))
                                    {
                                        CustomWorldMod.Log($"Custom Regions: Replaced disconnected [{oldConnections[i]}] with [{newConnections[j]}]");
                                        oldConnections[i] = newConnections[j];
                                        noDisconnectedPipes = false;
                                        performedOperation = true;
                                        break;
                                    }
                                }

                                // If the room is Vanilla, mod can replace pipes with DISCONNECTED
                                /*else if (isVanilla && newConnectionDisconnected)
                                {
                                    CustomWorldMod.CustomWorldLog($"Custom Regions: Replaced vanilla [{oldConnections[i]}] with disconnected [{newConnections[j]}]");
                                    oldConnections[i] = newConnections[j];
                                    noDisconnectedPipes = false;
                                }*/

                                /* else
                                 {
                                     // room will be completly replaced
                                     isRoomBeingReplaced = true;
                                 }*/
                            }
                        }
                    }
                }


                CustomWorldMod.Log($"Custom Regions: Analized old room [{roomConnectionsToBeReplaced}]. Added by a mod? [{isVanilla}]. NewRoomConnections [{string.Join(", ", newConnections.ToArray())}]. IsBeingReplaced [{isRoomBeingReplaced}]. No Empty Pipes [{noDisconnectedPipes}]");

                if (roomConnectionsToBeReplaced.ToUpper().Contains("DISCONNECTED") && roomConnectionsToBeReplaced.ToUpper().Contains("DISCONNECT"))
                {
                    noDisconnectedPipes = true;
                }

                // No empty pipes but room needs to be replaced. Whole line will be replaced
                if (isVanilla)
                {
                    if (isRoomBeingReplaced && noDisconnectedPipes)
                    {
                        CustomWorldMod.Log($"Custom Regions: Comparing two rooms without disconnected pipes. [{roomConnectionsToBeReplaced}] is vanilla: [{isVanilla}]. with [{string.Join(", ", newConnections.ToArray())}]");
                        oldConnections = newConnections;
                        performedOperation = true;
                    }
                }
                else if (!performedOperation)
                {

                    if (noDisconnectedPipes/*!errorLog.ToUpper().Contains("DISCONNECTED") && !errorLog.ToUpper().Contains("DISCONNECT")*/)
                    {
                        string errorLog = $"#Found possible incompatible room [{roomToBeReplacedName} : {string.Join(", ", newConnections.ToArray())}] from [{modID}] and [{roomConnectionsToBeReplaced}] from [{oldList.Find(x => x.data.Equals(roomConnectionsToBeReplaced)).modID}]. \\n If [{roomConnectionsToBeReplaced}] is from the vanilla game everything is fine. Otherwise you might be missing compatibility patch.";
                        CustomWorldMod.analyzingLog += errorLog + "\n\n";
                        CustomWorldMod.Log("Custom Regions: ERROR! " + errorLog);
                        UnityEngine.Debug.LogError(errorLog);
                        //UnityEngine.Debug.LogError($"Found two incompatible region mods: {modID} <-> {oldList.Find(x => x.data.Equals(roomConnectionsToBeReplaced)).modID}");
                    }
                }

                // A merging / replacement got place, so add changes to world lines.
                if (isRoomBeingReplaced || !noDisconnectedPipes || performedOperation)
                {
                    endingSetting = endingSetting != string.Empty ? " : " + endingSetting : "";

                    // Convert from list to connections
                    string updatedConnections = FromListToConnectionsString(roomConnectionsToBeReplaced.Substring(0, roomConnectionsToBeReplaced.IndexOf(" ")), oldConnections);

                    int index = oldList.IndexOf(oldList.Find(x => x.data.Equals(roomConnectionsToBeReplaced)));
                    if (index != -1)
                    {
                        if (oldList[index].data != (updatedConnections + endingSetting))
                        {
                            CustomWorldMod.Log($"Custom Regions: Replaced [{oldList[index].data}] with [{updatedConnections + endingSetting}]");
                            oldList[index] = new WorldDataLine(updatedConnections + endingSetting, false, modID);
                        }
                    }
                }

            }
            // Mod added a completly new connection
            else
            {
                if (!sameConnections)
                {
                    //CustomWorldMod.Log($"Custom Regions: Added new room [{newRoom}]");
                    oldList.Add(new WorldDataLine(newRoom, false));
                }
            }

            return oldList;
        }

        /// <summary>
        /// Returns a string from a connectionList
        /// </summary>
        public static string FromListToConnectionsString(string oldRoom, List<string> connections)
        {
            oldRoom += " : ";
            int a = 0;
            foreach (string room in connections)
            {
                if (a == 0)
                {
                    oldRoom += room;
                }
                else
                {
                    oldRoom += ", " + room;
                }
                a++;
            }
            //CustomWorldMod.CustomWorldLog($"Custom Regions: FromListToConnection: [{oldRoom}]");
            return oldRoom;
        }

        /// <summary>
        /// Compares and merges a bat blockage in the existing bat blockage list
        /// </summary>
        internal static List<WorldDataLine> AddNewBatBlockage(string newBlockage, List<WorldDataLine> oldBatsLine)
        {
            bool sameBlockage = false;

            foreach (WorldDataLine oldBlockage in oldBatsLine)
            {
                if (oldBlockage.data.Equals(newBlockage))
                {
                    // The blockages are exactly the same, skipped
                    sameBlockage = true;
                    break;
                }
            }

            if (!sameBlockage)
            {
                CustomWorldMod.Log($"Custom Regions: Added new blockage [{newBlockage}]");
                oldBatsLine.Add(new WorldDataLine(newBlockage, false));

            }
            return oldBatsLine;
        }

        /// <summary>
        /// Returns a List from a room-connection string
        /// This method should be heavily optimized and cleaned up.
        /// </summary>
        public static List<string> FromConnectionsToList(string oldConnections)
        {
            // CustomWorldMod.CustomWorldLog($"Custom Regions: Trying to split [{oldConnections}]");
            List<string> connections = new List<string>();

            string[] split = Regex.Split(oldConnections, " : ");
            int position = 0;

            if (split.Length >= 2)
            {
                // Remove base room
                position = 1;
            }

            string[] split_rooms = Regex.Split(split[position], ", ");

            foreach (string s in split_rooms)
            {
                if (s.Trim() != "")
                {
                    connections.Add(s);
                }
            }
            // updatedConnections = updatedConnections.Where(x => !string.IsNullOrEmpty(x)).ToArray();

            //CustomWorldMod.CustomWorldLog($"Custom Regions: FromConnectionToList [{string.Join(",", connections.ToArray())}]");

            return connections;
        }

        /// <summary>
        /// Compares and merges a creatre-line in the existing creature list
        /// </summary>
        internal static List<WorldDataLine> AddNewCreature(string newCreatureLine, List<WorldDataLine> oldCreaturesSpawns)
        {
            bool sameCreatureLine = false;
            string creatureLineBeReplaced = string.Empty;


            bool lineage = false;
            string roomNameNewLine = string.Empty;

            //CustomWorldMod.Log($"Custom Regions: Adding new creature spawn [{newCreatureLine}]]");

            if (newCreatureLine.Contains("OFFSCREEN"))
            {
                //creatureLineBeReplaced = newCreatureLine;
                creatureLineBeReplaced = string.Empty;
                sameCreatureLine = false;
            }
            else
            {

                if (newCreatureLine.Contains("LINEAGE"))
                {
                    lineage = true;
                    roomNameNewLine = Regex.Split(newCreatureLine, " : ")[1];
                }
                else
                {
                    roomNameNewLine = Regex.Split(newCreatureLine, " : ")[0];
                }


                foreach (WorldDataLine oldSpawnLine in oldCreaturesSpawns)
                {

                    if (oldSpawnLine.data.Contains(newCreatureLine))
                    {
                        // The spawn is exactly the same, skipped
                        sameCreatureLine = true;
                        creatureLineBeReplaced = string.Empty;
                        break;
                    }
                    else
                    {

                        //CustomWorldMod.CustomWorldLog($"Custom Regions: Splitting [{newCreatureLine}]");
                        //CustomWorldMod.CustomWorldLog($"Custom regions: Testing Creature listing [{string.Join(",", oldLinesList.ToArray())}]");

                        string[] array = Regex.Split(oldSpawnLine.data, " : ");
                        string oldRoomName = array[0];

                        if (oldSpawnLine.data.Contains("OFFSCREEN"))
                        {
                            continue;
                        }

                        if (lineage)
                        {
                            if (!oldSpawnLine.data.Contains("LINEAGE"))
                            {
                                continue;
                            }
                            else
                            {
                                oldRoomName = array[1];
                            }
                        }
                        // CustomWorldMod.CustomWorldLog($"Custom Regions: Comparing [{oldLinesList[roomIndex]}] with [{roomNameNewLine}]");


                        /* if (lineage)
                         {
                             if(oldRoomName.Equals(roomNameNewLine))
                             {
                                 if (array[2] == Regex.Split(newCreatureLine, " : ")[2])
                                 {
                                     // Found same LINEAGE
                                     creatureLineBeReplaced = string.Empty;
                                     sameCreatureLine = true;
                                     break;
                                 }
                                 creatureLineBeReplaced = oldSpawnLine.data;
                             }
                         }
                         else*/
                        {
                            // Adding creatures to new room, check for collision
                            if (oldRoomName.Equals(roomNameNewLine))
                            {
                                creatureLineBeReplaced = oldSpawnLine.data;
                            }

                        }




                        //CustomWorldMod.CustomWorldLog($"Custom Regions: Analyzing [{newCreatureLine}]. Room Name [{roomName}]");


                    }


                }
            }
            if (creatureLineBeReplaced == string.Empty)
            {
                if (!sameCreatureLine)
                {
                    oldCreaturesSpawns.Add(new WorldDataLine(newCreatureLine, false));
                }
            }
            else
            {

                CreatureLine newLines = FillCreatureLine(newCreatureLine);
                CreatureLine oldLines = FillCreatureLine(creatureLineBeReplaced);

                bool isVanilla = oldCreaturesSpawns.Find(x => x.data.Equals(creatureLineBeReplaced)).vanilla;
                int index = oldCreaturesSpawns.IndexOf(oldCreaturesSpawns.Find(x => x.data.Equals(creatureLineBeReplaced)));

                CustomWorldMod.Log($"Custom Regions: Trying to merge creature [{newCreatureLine}] with [{creatureLineBeReplaced}] (vanilla [{isVanilla}])");

                // This bit might be redundant
                if (lineage && oldLines.lineage && newLines.lineage)
                {
                    if (oldLines.denNumber != newLines.denNumber)
                    {
                        oldCreaturesSpawns.Add(new WorldDataLine(newCreatureLine, false));
                    }
                    else if (isVanilla)
                    {
                        if (index > -1)
                        {
                            oldCreaturesSpawns[index] = new WorldDataLine(newCreatureLine, false);
                        }
                    }

                }
                else
                {
                    string[] updatedConnections = new string[1];
                    string updatedCreatureLine;

                    bool empty = true;
                    for (int i = 0; i < newLines.connectedDens.Length; i++)
                    {
                        bool shouldAdd = false;
                        if (newLines.connectedDens[i] != null)
                        {
                            try
                            {
                                if (oldLines.connectedDens.Length <= i || oldLines.connectedDens[i] == null)
                                {
                                    CustomWorldMod.Log($"Custom Regions: Empty pipe, filling with [{newLines.connectedDens[i]}]"); ;
                                    shouldAdd = true;
                                }
                                else if (isVanilla)
                                {
                                    CustomWorldMod.Log($"Custom Regions: replacing vanilla pipe [{oldLines.connectedDens[i]}] with [{newLines.connectedDens[i]}]");
                                    empty = false;
                                    shouldAdd = true;
                                }
                            }
                            catch (Exception) { shouldAdd = true; }
                        }

                        if (shouldAdd)
                        {
                            if (updatedConnections.Length <= i)
                            {
                                Array.Resize(ref updatedConnections, i + 1);
                            }
                            updatedConnections[i] = newLines.connectedDens[i];
                        }
                    }

                    for (int a = 0; a < oldLines.connectedDens.Length; a++)
                    {
                        try
                        {
                            if (updatedConnections.Length <= a)
                            {
                                Array.Resize(ref updatedConnections, a + 1);
                            }

                            if (updatedConnections[a] == null && (oldLines.connectedDens[a] != null))
                            {
                                updatedConnections[a] = oldLines.connectedDens[a];
                            }
                        }
                        catch (Exception) { }
                    }

                    // Remove empty slots
                    updatedConnections = updatedConnections.Where(x => !string.IsNullOrEmpty(x)).ToArray();

                    updatedCreatureLine = $"{oldLines.room} : {string.Join(", ", updatedConnections)}";

                    if (index > -1)
                    {
                        oldCreaturesSpawns[index] = new WorldDataLine(updatedCreatureLine, empty);
                    }
                }


            }


            return oldCreaturesSpawns;
        }

        /// <summary>
        /// Create CreatureLine struct from string line. Used in Creature merging
        /// </summary>
        public static CreatureLine FillCreatureLine(string lines)
        {
            string[] connectedDens = new string[0];

            string[] line = Regex.Split(lines, " : ");

            string roomName;
            if (line[0] == "LINEAGE")
            {
                roomName = line[1];
                //CustomWorldMod.CustomWorldLog($"Custom Regions: Creating creature line. Lineage[{true}]. RoonName[{roomName}]. Spawners[{line[3]}]. DenNumber[{int.Parse(line[2])}]");
                return new CreatureLine(true, roomName, line[3], int.Parse(line[2]));

            }
            else
            {
                roomName = line[0];

                string[] array = Regex.Split(line[1], ", ");

                for (int i = 0; i < array.Length; i++)
                {
                    int denNumber = (int)char.GetNumericValue(array[i][0]);

                    if (denNumber >= connectedDens.Length)
                    {
                        Array.Resize(ref connectedDens, denNumber + 1);
                    }

                    connectedDens[denNumber] = array[i];
                }

                //CustomWorldMod.CustomWorldLog($"Custom Regions: Creating creature line. Lineage[{false}]. RoonName[{roomName}]. Spawners[{connectedDens.Length}]");
                return new CreatureLine(false, roomName, connectedDens);

            }

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
                CustomWorldMod.Log(debug);

            }
            catch (Exception e)
            {
                CustomWorldMod.Log($"Custom Regions: Mapping rooms failed, reason: {e}");
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
                    CustomWorldMod.Log($"Custom Worlds: something failed ERROR!!! [{e}]");
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
                    CustomWorldMod.Log("World loader ctor...");
                    //self.lines = getWorldLines(self);
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
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegionPacks)
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
                    CustomWorldMod.Log($"Custom Regions: Found arena {roomName} in {keyValues.Key}. Path: {result}");
                }

                if (result != "")
                {
                    break;
                }
            }

            if (result != "")
            {
                //CustomWorldMod.CustomWorldLog("Using Custom Worldfile: " + result);
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
        private static MergeStatus status;

        /// <summary>
        /// Reads and loads all the world_XX.txt files found in all the custom worlds.
        /// TODO: a) Implement an algorithm that merges all those files b) Just use the last one loaded.
        /// </summary>
        public static List<string> GetWorldLines(WorldLoader self)
        {
            List<string> lines = new List<string>();

            // Reset
            CustomWorldMod.analyzingLog = string.Empty;

            // Bool indicates whether it is vanilla or not
            List<WorldDataLine> ROOMS = new List<WorldDataLine>();
            List<WorldDataLine> CREATURES = new List<WorldDataLine>();
            List<WorldDataLine> BATS = new List<WorldDataLine>();

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
                        ROOMS.Add(new WorldDataLine(s, true));
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
                        CREATURES.Add(new WorldDataLine(s, true));
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
                        BATS.Add(new WorldDataLine(s, true));
                    }
                    if (s.Equals("BAT MIGRATION BLOCKAGES"))
                    {
                        startBats = true;
                    }
                }
            }

            bool foundAnyCustomRegion = false;
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegionPacks)
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
                    CustomWorldMod.Log($"Custom Regions: Found world_{self.worldName}.txt from {keyValues.Value}");
                    foundAnyCustomRegion = true;
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
                                    status = MergeStatus.ROOMS;
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
                                        case MergeStatus.ROOMS:
                                            ROOMS = AddNewRoom(array[i], ROOMS, keyValues.Key);
                                            break;
                                        case MergeStatus.CREATURES:
                                            // MERGE CREATURES
                                            CREATURES = AddNewCreature(array[i], CREATURES); //CREATURES.Add(array[i]);
                                            break;
                                        case MergeStatus.BATS:
                                            // MERGE BATS
                                            BATS = AddNewBatBlockage(array[i], BATS);
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
            List<string> sortedRooms = CustomWorldMod.FromWorldDataToListString(ROOMS);
            sortedRooms.Sort();

            List<string> sortedCreatures = CustomWorldMod.FromWorldDataToListString(CREATURES);
            sortedCreatures.Sort();

            List<string> sortedBats = CustomWorldMod.FromWorldDataToListString(BATS);
            sortedBats.Sort();

            lines = BuildWorldText(sortedRooms, sortedCreatures, sortedBats);


            if (lines.Count < 2)
            {
                CustomWorldMod.Log("Custom Regions: ERROR! Lines.Count < 2");
                return self.lines;
            }

            /*
            foreach (string s in lines)
            {
            }
            */

            if (!foundAnyCustomRegion)
            {
                CustomWorldMod.Log("Custom regions did not find any custom world_XX.txt files, so it will load vanilla. (if you were not expecting this it means you have something installed incorrectly)");
            }
            else
            {
                CustomWorldMod.Log("\nMerged world_XX.txt file");
                CustomWorldMod.Log(string.Join("\n", lines.ToArray()));
                CustomWorldMod.Log("\n");
            }
            return lines;
        }



        /// <summary>
        /// Builds the world from the merged world_XX.txt files.
        /// </summary>
        /// <returns>Returns a List<string> with room connections, creatures and bat migration blockages.</returns>
        internal static List<string> BuildWorldText(List<string> ROOMS, List<string> CREATURES, List<string> BATS)
        {
            List<string> list = new List<string>();

            string startRooms = "ROOMS";
            string endRooms = "END " + startRooms;

            string startCreatures = "CREATURES";
            string endCreatures = "END " + startCreatures;

            string startBats = "BAT MIGRATION BLOCKAGES";
            string endBats = "END " + startBats;

            // ROOMS
            list.Add(startRooms);
            foreach (string room in ROOMS)
            {
                list.Add(room);
            }
            list.Add(endRooms);

            // CREATURES
            list.Add(startCreatures);
            foreach (string creature in CREATURES)
            {
                list.Add(creature);
            }
            list.Add(endCreatures);

            // BATS
            list.Add(startBats);
            foreach (string bats in BATS)
            {
                list.Add(bats);
            }
            list.Add(endBats);

            return list;

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
                    CustomWorldMod.Log("Custom Regions: World was null, creating new lines");
                    self.lines = new List<string>();
                }
                self.lines = GetWorldLines(self);

            }
            else
            {
                //CustomWorldMod.CustomWorldLog($"Custom Worlds: Next Activity was not init, was {self.activity}");
            }

            if (self.faultyExits == null)
            {
                CustomWorldMod.Log($"Custom Regions: NextActivity failed, faultyExits is null");
                self.faultyExits = new List<WorldCoordinate>();
            }
            orig(self);
        }
    }
}
