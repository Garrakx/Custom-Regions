using CustomRegions.Mod;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static CustomRegions.Mod.CustomWorldStructs;

namespace CustomRegions.CWorld
{

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
            On.WorldLoader.Update += WorldLoader_Update;
            On.WorldLoader.CreatingAbstractRooms += WorldLoader_CreatingAbstractRooms;
            On.WorldLoader.LoadAbstractRoom += WorldLoader_LoadAbstractRoom;
        }

        private static System.Diagnostics.Stopwatch absRoomLoadWatch = null;
        private static void WorldLoader_LoadAbstractRoom(On.WorldLoader.orig_LoadAbstractRoom orig, World world, string roomName, AbstractRoom room, RainWorldGame.SetupValues setupValues)
        {
            absRoomLoadWatch.Reset();
            absRoomLoadWatch.Start();
            orig(world, roomName, room, setupValues);
            absRoomLoadWatch.Stop();
            /*
            DateTime date = new DateTime(absRoomWatch.ElapsedTicks);
            CustomWorldMod.Log($"[WorldLoader]: Loading AbstractRoom [{roomName}]. Time Elapsed [{date.ToString("s.ffff")}s]", false, CustomWorldMod.DebugLevel.FULL);
            */
        }

        private static void WorldLoader_CreatingAbstractRooms(On.WorldLoader.orig_CreatingAbstractRooms orig, WorldLoader self)
        {
            absRoomLoadWatch.Start();
            orig(self);
            absRoomLoadWatch.Stop();
            DateTime date = new DateTime(absRoomLoadWatch.ElapsedTicks);
            CustomWorldMod.Log($"[WorldLoader]: AbstractRoom [{self.roomAdder[self.cntr][0]}]. Time Elapsed [{date.ToString("s.ffff")}s]", false, CustomWorldMod.DebugLevel.FULL);
        }

        private static WorldLoader.Activity activity = WorldLoader.Activity.Finished;
        private static System.Diagnostics.Stopwatch activityWatch = null;
        private static System.Diagnostics.Stopwatch worldLoaderWatch = null;
        private static void WorldLoader_Update(On.WorldLoader.orig_Update orig, WorldLoader self)
        {
            worldLoaderWatch.Start();
            if (activity != self.activity)
            {
                if (activityWatch != null)
                {
                    DateTime date = new DateTime(activityWatch.ElapsedTicks);
                    CustomWorldMod.Log($"[WorldLoader]: World [{self.worldName}] Activity [{activity}]. Time Elapsed [{date.ToString("s.ffff")}s]", false, CustomWorldMod.DebugLevel.FULL);
                }
                activity = self.activity;
                activityWatch = new System.Diagnostics.Stopwatch();
            }
            activityWatch.Start();
            orig(self);
            activityWatch.Stop();
            worldLoaderWatch.Stop();
            if (self.Finished)
            {
                DateTime date2 = new DateTime(worldLoaderWatch.ElapsedTicks);
                CustomWorldMod.Log($"[WorldLoader]: Finished loading world [{self.worldName}]. Total time Elapsed [{date2.ToString("s.ffff")}s]", false, CustomWorldMod.DebugLevel.RELEASE);
            }
        }

        /// <summary>
        /// Compares and merges a room-connection in the existing room list
        /// </summary>
        public static List<WorldDataLine> AddNewRoom(string newLine, List<WorldDataLine> oldLines, string newPackName)
        {
            //CustomWorldMod.Log($"Analyzing line [{newLine}]");

            ///-------------------------------------------------------------------------------------///
            // Check if oldLines contains new line
            WorldDataLine oldLine = oldLines.Find(x => x.line.Equals(newLine));
            if (!oldLine.Equals(default(WorldDataLine)))
            {
                // Connection exited already, skipping...
                //CustomWorldMod.Log("Connection already existed, skipping...", false, CustomWorldMod.DebugLevel.FULL);
                return oldLines;
            }
            ///-------------------------------------------------------------------------------------///

            bool modifiedNewLine = false;
            bool modifiedOldLine = false;

            // If true, lane will be replaced/merged
            // If false, lane will be added
            bool replaceOrMerge = false;

            ///-------------------------------------------------------------------------------------///
            // Pre-processing
            string[] split = Regex.Split(newLine, " : ");
            string newRoomName = string.Empty;
            string newConnections = string.Empty;
            string newEndingString = string.Empty;
            ///-------------------------------------------------------------------------------------///


            // DataLine to add to oldLlist
            WorldDataLine? newWorldDataLine = null;

            //List<string> logOuput = new List<string>();
            
            // Corrupted line
            if (split.Length < 2)
            {
                CustomWorldMod.Log($"[WorldMerging]: Corrupted line [{newLine}] from [{newPackName}]", true);
                return oldLines;
            }
            else
            {
                newRoomName = split[0];
                newConnections = split[1];
                try
                {
                    // necessary to avoid overwritting a vanilla room just because it
                    // used disconnect instead of disconnected or vice-versa
                    newConnections.Replace("DISCONNECT", "DISCONNECTED");
                } catch { }

                // Line has ending
                if (split.Length == 3)
                {
                    newEndingString = split[2];
                }
            }

            // Line that has same room name, but different connections or different ending
            oldLine = oldLines.Find(x => x.roomName.Equals(newRoomName));

            ///-------------------------------------------------------------------------------------///
            // pack added a new room connection
            ///-------------------------------------------------------------------------------------///
            if (oldLine.Equals(default(WorldDataLine)))
            {
                replaceOrMerge = false;
                newWorldDataLine = new WorldDataLine(newLine, newRoomName, newConnections, newEndingString, false, newPackName);
            }
            ///-------------------------------------------------------------------------------------///
            // connections are the same, different ending
            ///-------------------------------------------------------------------------------------///
            else if ( oldLine.connections.Equals(newConnections) )
            {
                replaceOrMerge = true;
                if (oldLine.endingString.Equals(string.Empty) && (!newEndingString.Equals(string.Empty)))
                {
                    oldLine.endingString = newEndingString;
                    modifiedOldLine = true;
                    CustomWorldMod.Log($"[WorldMerging]: Added new ending [{newEndingString}] ([{newPackName}]) to [{oldLine.roomName}] ([{oldLine.packName}])");
                }
                // do nothing
                
            }
            ///-------------------------------------------------------------------------------------///
            // Line needs merging, since the roomName is the same, but the connections are different
            ///-------------------------------------------------------------------------------------///
            else
            {
                replaceOrMerge = true;
                CustomWorldMod.Log($"[WorldMerging]: Merging/replacing oldline [{oldLine.line}] ({oldLine.packName}) with [{newLine}] ({newPackName})");

                //-----------------------------------------//
                //          Build new connections
                //-----------------------------------------//

                // Convert the line strings into Lists so it is easier to manipulate and read
                List<string> oldConnList = FromConnectionsToList(oldLine.connections);
                List<string> newConnList = FromConnectionsToList(newConnections);

                ///------------------------------------------------------------------------------------------------------------///
                /// HOW IT WORKS (subject to change)                                
                /// 
                /// If the mod is modifying a vanilla room, it will replace it completly. 
                /// Modders should make sure they don't occuppy all the pipes for that room.
                /// 
                /// If the mod is modifying a room that is either modded or modified by another mod, CR will try to merge both.
                /// 
                /// If this room does not have any empty exits, the two mods will be incompatible.
                /// 
                ///------------------------------------------------------------------------------------------------------------///

                // -------------------------------------- //
                // Replace old line since it was vanilla  //
                // -------------------------------------- //
                if (oldLine.vanilla)
                {
                    // A mod omitted to include DISCONNECTED in their lines, adding it to increase compability
                    if (newConnList.Count < oldConnList.Count && (oldConnList.Contains("DISCONNECTED") || oldConnList.Contains("DISCONNECT")))
                    {
                        for (int i = 0; i < (oldConnList.Count - newConnList.Count); i++)
                        {
                            modifiedNewLine = true;
                            newConnList.Add("DISCONNECTED");
                        }
                        CustomWorldMod.Log($"[WorldMerging]: Pack [{newPackName}] omitted DISCONNECT/DISCONNECTED in their lines, added to increase compatibility");
                    }

                    // The mod will replace the old lines since they are vanilla
                    string updatedNewLine = newLine;
                    string updatedNewConnections = newConnections;
                    if (modifiedNewLine)
                    {
                        // From list to string separated with comma
                        updatedNewConnections = string.Join(", ", newConnList.ToArray());
                        // Check if there is an ending setting
                        newEndingString = (!newEndingString.Equals(string.Empty)) ? (" : " + newEndingString) : string.Empty;
                        // Rebuild line
                        updatedNewLine = newRoomName + updatedNewConnections + newEndingString;
                        
                    }
                    newWorldDataLine = new WorldDataLine(updatedNewLine, newRoomName, updatedNewConnections, newEndingString, false, newPackName);
                }
                // ------------------------ //
                // Merge between two packs  //
                // ------------------------ //
                else
                {
                    for (int j = 0; j < newConnList.Count; j++)
                    {
                        for (int i = 0; i < oldConnList.Count; i++)
                        {
                            // Connection is the same, skip
                            if (oldConnList[i].Equals(newConnList[j]))
                            {
                                continue;
                            }

                            // old connection is a DISCONNECTED / DISCONNECT pipe
                            bool oldConnectionDisconnected = oldConnList[i].Equals("DISCONNECTED") || oldConnList[i].Equals("DISCONNECT");

                            // new connection is a DISCONNECTED / DISCONNECT pipe
                            bool newConnectionDisconnected = newConnList[j].Equals("DISCONNECTED") || newConnList[j].Equals("DISCONNECT");

                            // Old room is not disconnected, or the new one is
                            if (!oldConnectionDisconnected || newConnectionDisconnected)
                            {
                                continue;
                            }

                            // The room to be merged has empty exits, so new mod will use those
                            if (!oldConnList.Contains(newConnList[j]))
                            {
                                CustomWorldMod.Log($"[WorldMerging]: Replaced disconnected [{oldConnList[i]}] with [{newConnList[j]}]");
                                oldConnList[i] = newConnList[j];
                                modifiedOldLine = true;
                                break;
                            }
                        }
                    }

                    // Check if old line contains new line
                    List<string> connectionsMissing = newConnList.Except(newConnList).ToList();

                    if (modifiedOldLine)
                    {
                        if (!oldLine.endingString.Equals(string.Empty) || newEndingString.Equals(string.Empty))
                        {
                            newEndingString = oldLine.endingString;
                        }
                        else
                        {
                            newEndingString += (" : " + newEndingString);
                            CustomWorldMod.Log($"[WorldMerging]: Added new ending [{newEndingString}] ([{newPackName}]) to [{oldLine.roomName}] ([{oldLine.packName}])");
                        }
                        // From list to string separated with comma
                        string updatedOldConnections = string.Join(", ", oldConnList.ToArray());

                        // Check if there is an ending setting
                        newEndingString = (!newEndingString.Equals(string.Empty)) ? (" : " + newEndingString) : string.Empty;

                        // Rebuild line
                        string updatedOldLine = oldLine.roomName +" : " + updatedOldConnections + newEndingString;
                        newWorldDataLine = new WorldDataLine(updatedOldLine, oldLine.roomName, updatedOldConnections, newEndingString, false, "Merged");
                    }
                    else if (connectionsMissing.Count == 0)
                    {
                        CustomWorldMod.Log("[WorldMerging]: Skipping since old connections contained new connections");
                    }
                    // OUCH! TWO PACKS ARE INCOMPATIBLE D:
                    else
                    {
                        // [TODO] Add connections missing to a list to remove any disconnected rooms to avoid crash...
                        string errorLog = $"#Found possible incompatibility! Existing line: [{oldLine.line}] from [{oldLine.packName}] and new line [{newLine}] from [{newPackName}].\n   You might be missing compatibility patch or the two packs are incompatible.";
                        CustomWorldMod.analyzingLog += errorLog + "\n\n";
                        CustomWorldMod.Log(errorLog, true);
                    }
                }
            }

            if (newWorldDataLine != null)
            {
                ///------------------------------------------------------------------------------------- ///
                // Added new connection
                ///------------------------------------------------------------------------------------- ///
                if (!replaceOrMerge)
                {
                    // if (!oldLines.Find(x => x.roomName.Equals(newWorldDataLine.Value.roomName)).Equals(default(WorldDataLine)))
                    int index = oldLines.IndexOf(oldLines.Find(x => x.roomName.Equals(newWorldDataLine.Value.roomName)));
                    if (index == -1)
                    {
                        oldLines.Add(newWorldDataLine.Value);
                        CustomWorldMod.Log($"Final action: add new line [{newWorldDataLine.Value.line}]", false, CustomWorldMod.DebugLevel.MEDIUM);
                    }
                    else
                    {
                        // FATAL ERROR
                        CustomWorldMod.Log($"[WorldMerging]: Fatal error when merging, line to be added already existed [{newWorldDataLine.Value.line}]", true);
                    }
                }
                ///------------------------------------------------------------------------------------- ///
                // Modified existing connection (replace or merge)
                ///------------------------------------------------------------------------------------- ///
                else
                {
                    int index = oldLines.IndexOf(oldLines.Find(x => x.roomName.Equals(newRoomName)));
                    if (index != -1)
                    {
                        CustomWorldMod.Log($"[WorldMerging]: Updated line [{oldLines[index].line}] --> [{newWorldDataLine.Value.line}]");
                        oldLines[index] = newWorldDataLine.Value;
                    }
                    else
                    {
                        // FATAL ERROR
                        CustomWorldMod.Log($"[WorldMerging]: Fatal error when merging, line to be merged didn't exist [{newWorldDataLine.Value.line}]", true);
                    }
                }
            }
            else
            {
                // do nothing
            }

            return oldLines;
        }

        /// <summary>
        /// Returns a string from a connectionList - CURSED
        /// </summary>
        public static string FromListToConnectionsString(string roomName, List<string> connections)
        {
            roomName += " : ";
            int a = 0;
            foreach (string connection in connections)
            {
                if (a == 0)
                {
                    roomName += connection;
                }
                else
                {
                    roomName += ", " + connection;
                }
                a++;
            }
            //CustomWorldMod.CustomWorldLog($"Custom Regions: FromListToConnection: [{oldRoom}]");
            return roomName;
        }

        /// <summary>
        /// Compares and merges a bat blockage in the existing bat blockage list
        /// </summary>
        internal static List<WorldDataLine> AddNewBatBlockage(string newBlockage, List<WorldDataLine> oldBatsLine)
        {
            bool sameBlockage = false;

            foreach (WorldDataLine oldBlockage in oldBatsLine)
            {
                if (oldBlockage.line.Equals(newBlockage))
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
        /// TODO-REWRITE
        /// </summary>
        internal static List<WorldDataLine> AddNewCreature(string newCreatureLine, List<WorldDataLine> oldCreaturesSpawns)
        {
            bool sameCreatureLine = false;
            string creatureLineBeReplaced = string.Empty;


            bool lineage = false;
            string roomNameNewLine = string.Empty;

            CustomWorldMod.Log($"Custom Regions: Adding new creature spawn [{newCreatureLine}]]", false, CustomWorldMod.DebugLevel.FULL);

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

                    if (oldSpawnLine.line.Contains(newCreatureLine))
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

                        string[] array = Regex.Split(oldSpawnLine.line, " : ");
                        string oldRoomName = array[0];

                        if (oldSpawnLine.line.Contains("OFFSCREEN"))
                        {
                            continue;
                        }

                        if (lineage)
                        {
                            if (!oldSpawnLine.line.Contains("LINEAGE"))
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
                                creatureLineBeReplaced = oldSpawnLine.line;
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

                bool isVanilla = oldCreaturesSpawns.Find(x => x.line.Equals(creatureLineBeReplaced)).vanilla;
                int index = oldCreaturesSpawns.IndexOf(oldCreaturesSpawns.Find(x => x.line.Equals(creatureLineBeReplaced)));

                CustomWorldMod.Log($"[CREATURESMERGING]: Trying to merge creature [{newCreatureLine}] with [{creatureLineBeReplaced}] (vanilla [{isVanilla}])");

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
                                    CustomWorldMod.Log($"[CREATURESMERGING]: Empty pipe, filling with [{newLines.connectedDens[i]}]", false, CustomWorldMod.DebugLevel.FULL);
                                    shouldAdd = true;
                                }
                                else if (isVanilla)
                                {
                                    CustomWorldMod.Log($"[CREATURESMERGING]: replacing vanilla pipe [{oldLines.connectedDens[i]}] with [{newLines.connectedDens[i]}]");
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
            worldLoaderWatch = new Stopwatch();
            absRoomLoadWatch = new Stopwatch();

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
                CustomWorldMod.Log("Using custom WorldLoader ctor");
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

                self.NextActivity();
            }
        }


        /// <summary>
        /// If finds the room in the CustomAssets folder, returns that path (takes priority over vanilla)
        /// </summary>
        /// <returns>returns path to room</returns>
        private static string WorldLoader_FindRoomFileDirectory(On.WorldLoader.orig_FindRoomFileDirectory orig, string roomName, bool includeRootDirectory)
        {
            string result = "";
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
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
        /// Used for mergin algorithm
        /// </summary>
        private static MergeStatus status = (MergeStatus)5;

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
                CustomWorldMod.Log("Custom Regions: Found vanilla room, filling lines");
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
                        // Pre-processing
                        string[] split = Regex.Split(s, " : ");
                        string roomName = string.Empty;
                        string connections = string.Empty;
                        string endingString = string.Empty;

                        List<string> logOuput = new List<string>();

                        // Corrupted line (this should not happen)
                        if (split.Length < 2 || split.Length > 3)
                        {
                            CustomWorldMod.Log($"Corrupted vanilla line [{s}]", true);
                        }
                        else
                        {
                            roomName = split[0];
                            connections = split[1];
                            try
                            {
                                connections.Replace("DISCONNECT", "DISCONNECTED");
                            }
                            catch { }

                            // Line has ending
                            if (split.Length == 3)
                            {
                                endingString = split[2];
                            }
                        }
                        ROOMS.Add(new WorldDataLine(s, roomName, connections, endingString, true, "Vanilla"));
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
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
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
                    if(!array.Contains("ROOMS") && !array.Contains("CREATURES") && !array.Contains("BLOCKAGES"))
                    {
                        CustomWorldMod.Log($"RegionPack [{keyValues.Key}] has corrupted world_{self.worldName}.txt file: Missing any ROOMS/CREATURES/BLOCKAGES delimiters", true);
                    }
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
                                    status = MergeStatus.CREATURES;
                                }
                                else if (array[i] == "BAT MIGRATION BLOCKAGES")
                                {
                                    status = MergeStatus.BATS;
                                }
                                else if (array[i] != "END ROOMS" && array[i] != "END CREATURES" && array[i] != "END BAT MIGRATION BLOCKAGES")
                                {
                                    switch (status)
                                    {
                                        case MergeStatus.ROOMS:
                                            // MERGE ROOMS
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
                CustomWorldMod.Log("Lines.Count < 2", true);
                return self.lines;
            }

            if (!foundAnyCustomRegion)
            {
                CustomWorldMod.Log("Custom regions did not find any custom world_XX.txt files, so it will load vanilla. (if you were not expecting this it means you have something installed incorrectly)");
            }
            else
            {
                CustomWorldMod.Log("\nMerged world_XX.txt file");
                CustomWorldMod.Log(string.Join($"\n", lines.ToArray()));
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
            if (self.activity == WorldLoader.Activity.Init && !self.singleRoomWorld)
            {
                if (self.lines == null)
                {
                    CustomWorldMod.Log("Custom Regions: World was null, creating new lines");
                    self.lines = new List<string>();
                }
                self.lines = GetWorldLines(self);

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
