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
        /// <summary> Enum used in the mergin process when loading the world_XX.txt file.
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


        #region Loading times debug
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
        #endregion

        #region ROOM MERGING
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
                CustomWorldMod.Log($"Connection already existed, skipping... [{newLine}]", false, CustomWorldMod.DebugLevel.FULL);
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
                if (newConnections.Contains("DISCONNECT"))
                {
                    // necessary to avoid overwritting a vanilla room just because it
                    // used disconnect instead of disconnected or vice-versa
                    newConnections.Replace("DISCONNECT", "DISCONNECTED");
                }

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
            else if (oldLine.connections.Equals(newConnections))
            {
                replaceOrMerge = true;
                if (oldLine.endingString.Equals(string.Empty) && (!newEndingString.Equals(string.Empty)))
                {
                    oldLine.endingString = newEndingString;
                    modifiedOldLine = true;
                    CustomWorldMod.Log($"[WorldMerging]: Added new ending [{newEndingString}] ([{newPackName}]) to [{oldLine.roomName}] ([{oldLine.packName}])");
                }
                else
                {
                    // do nothing
                }

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
                        CustomWorldMod.Log($"[WorldMerging]: Pack [{newPackName}] omitted DISCONNECT/DISCONNECTED in their lines, added to increase compatibility -> [{string.Join(", ", newConnList.ToArray())}]");
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
                        updatedNewLine = newRoomName + " : " + updatedNewConnections + newEndingString;

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
                    List<string> connectionsMissing = oldConnList.Except(newConnList).ToList();

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
                        string updatedOldLine = oldLine.roomName + " : " + updatedOldConnections + newEndingString;
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
        #endregion

        #region CREATURE MERGING
        /// <summary> Compares and merges a creatre-line in the existing creature list
        /// </summary>
        /// 
        internal static List<WorldDataLine> AddNewCreature(string newSpawn, List<WorldDataLine> oldSpawnLines, string newPackName)
        {
            //CustomWorldMod.Log($"Analyzing line [{newLine}]");

            ///-------------------------------------------------------------------------------------///
            // Check if oldSpawn contains new line or is removing a spawn
            bool removingLine = false;
            if (newSpawn.Contains(CustomWorldMod.removeWorldLineDiv))
            {
                CustomWorldMod.Log($"[CreatureMerging] Removing spawn [{newSpawn}] ...");
                newSpawn = newSpawn.Replace(CustomWorldMod.removeWorldLineDiv, "");
                removingLine = true;
            }
            WorldDataLine oldSpawnLine = oldSpawnLines.Find(x => x.line.Equals(newSpawn));
            if (!oldSpawnLine.Equals(default(WorldDataLine)))
            {
                if (removingLine)
                {
                    oldSpawnLines.Remove(oldSpawnLine);
                    CustomWorldMod.Log($"[CreatureMerging] Removed spawn [{oldSpawnLine.line}] ...");
                }
                else
                {
                    // Spawn existed already, skipping...
                    CustomWorldMod.Log($"Spawn already existed, skipping... [{newSpawn}]", false, CustomWorldMod.DebugLevel.FULL);
                }
                return oldSpawnLines;
            }
            ///-------------------------------------------------------------------------------------///

            bool modifiedNewSpawn = false;
            bool modifiedOldSpawn = false;

            // If true, lane will be replaced/merged
            // If false, lane will be added
            bool replaceOrMerge = false;

            ///-------------------------------------------------------------------------------------///
            // Pre-processing
            string[] split = Regex.Split(newSpawn, " : ");
            string newRoomName = string.Empty;
            string newSpawns = string.Empty;
            ///-------------------------------------------------------------------------------------///


            // DataLine to add to oldLSpawnlist
            WorldDataLine? newWorldDataLine = null;
            bool lineage = false;

            // Corrupted line
            if (split.Length < 2)
            {
                CustomWorldMod.Log($"[CreatureMerging] Corrupted spawn [{newSpawn}] from [{newPackName}]", true);
                return oldSpawnLines;
            }
            else
            {
                if (split.Length > 2 && newSpawn.Contains("LINEAGE"))
                {
                    CustomWorldMod.Log($"[CreatureMerging] Lineage [{newSpawn}] from [{newPackName}]", false);
                    newRoomName = $"{split[1]} : {split[2]}";
                    newSpawns = split[3];
                    lineage = true;
                }
                else
                {
                    newRoomName = split[0];
                    newSpawns = split[1];
                }
            }

            // Line that has same room name, but different spawn
            oldSpawnLine = oldSpawnLines.Find(x => x.roomName.Equals(newRoomName));
            ///-------------------------------------------------------------------------------------///
            // pack added a new spawn
            ///-------------------------------------------------------------------------------------///
            if (oldSpawnLine.Equals(default(WorldDataLine)))
            {
                replaceOrMerge = false;
                newWorldDataLine = new WorldDataLine(newSpawn, newRoomName, newSpawns, "", lineage, false, newPackName);
            }
            ///-------------------------------------------------------------------------------------///
            // connections are the same, different ending
            ///-------------------------------------------------------------------------------------///
            else if (oldSpawnLine.connections.Equals(newSpawns))
            {
                // SHOULD NEVER HAPPEN
            }
            else if (oldSpawnLine.lineage != lineage)
            {
                // One of them is lineage and the other is not
            }
            ///-------------------------------------------------------------------------------------///
            // Spawn needs merging, since the roomName is the same, but the connections are different
            ///-------------------------------------------------------------------------------------///
            else
            {
                replaceOrMerge = true;
                CustomWorldMod.Log($"[CreatureMerging] Merging/replacing oldSpawn " +
                    $"[{oldSpawnLine.line}] ({oldSpawnLine.packName}) with [{newSpawn}] ({newPackName})");

                //-----------------------------------------//
                //          Build new connections
                //-----------------------------------------//

                // Convert the line strings into Lists so it is easier to manipulate and read
                List<string> oldSpawnList = FromConnectionsToList(oldSpawnLine.connections);
                List<string> newSpawnList = FromConnectionsToList(newSpawns);

                ///------------------------------------------------------------------------------------------------------------///
                /// HOW IT WORKS (subject to change)                                
                /// 
                /// If the mod is modifying a vanilla spawn, it will replace it completly. 
                /// 
                /// If the mod is modifying a spawn that is either modded or modified by another mod, CR will try to merge both.
                /// 
                ///------------------------------------------------------------------------------------------------------------///

                // -------------------------------------- //
                // Replace old spawn since it was vanilla  //
                // -------------------------------------- //
                if (oldSpawnLine.vanilla)
                {
                    // The mod will replace the old spawn lines since they are vanilla
                    string updatedNewSpawn = newSpawn;
                    string updatedNewConnections = newSpawns;
                    if (modifiedNewSpawn)
                    {
                        // From list to string separated with comma
                        updatedNewConnections = string.Join(", ", newSpawnList.ToArray());
                        // Rebuild line
                        updatedNewSpawn = newRoomName + " : " + updatedNewConnections;

                    }
                    newWorldDataLine = new WorldDataLine(updatedNewSpawn, newRoomName, updatedNewConnections, "", lineage, false, newPackName);
                }
                // ------------------------ //
                // Merge between two packs  //
                // ------------------------ //
                else
                {
                    List<string> spawnsToAdd = new List<string>();
                    for (int j = 0; j < newSpawnList.Count; j++)
                    {
                        for (int i = 0; i < oldSpawnList.Count; i++)
                        {
                            // Connection is the same, skip
                            if (oldSpawnList[i].Equals(newSpawnList[j]))
                            {
                                continue;
                            }

                            if (!oldSpawnList.Contains(newSpawnList[j]))
                            {
                                CustomWorldMod.Log($"[CreatureMerging] Added new creature to existing line [{newSpawnList[j]}]");
                                spawnsToAdd.Add(newSpawnList[j]);
                                modifiedOldSpawn = true;
                                break;
                            }

                        }
                    }

                    oldSpawnList = oldSpawnList.Union(spawnsToAdd).ToList();

                    if (modifiedOldSpawn)
                    {
                        // From list to string separated with comma
                        string updatedOldConnections = string.Join(", ", oldSpawnList.ToArray());
                        string lineageString = lineage ? "LINEAGE : " : "";

                        // Rebuild line
                        string updatedOldLine = lineageString + oldSpawnLine.roomName + " : " + updatedOldConnections;
                        newWorldDataLine = new WorldDataLine(updatedOldLine, oldSpawnLine.roomName, updatedOldConnections, "", false, "Merged");
                    }
                    // OUCH! TWO PACKS ARE INCOMPATIBLE D:
                    else
                    {
                        // [TODO] Add connections missing to a list to remove any disconnected rooms to avoid crash...
                        string errorLog = $"#Found possible incompatibility! Existing line: " +
                            $"[{oldSpawnLine.line}] from [{oldSpawnLine.packName}] and new line [{newSpawn}] from [{newPackName}].\n   " +
                            $"You might be missing compatibility patch or the two packs are incompatible.";
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
                    int index = oldSpawnLines.IndexOf(oldSpawnLines.Find(x => x.roomName.Equals(newWorldDataLine.Value.roomName)));
                    if (index == -1)
                    {
                        oldSpawnLines.Add(newWorldDataLine.Value);
                        CustomWorldMod.Log($"[CreatureMerging] Final action: add new line [{newWorldDataLine.Value.line}]", false, CustomWorldMod.DebugLevel.MEDIUM);
                    }
                    else
                    {
                        // FATAL ERROR
                        CustomWorldMod.Log($"[CreatureMerging] Fatal error when merging, line to be added already existed " +
                            $"[{newWorldDataLine.Value.line}]", true);
                    }
                }
                ///------------------------------------------------------------------------------------- ///
                // Modified existing connection (replace or merge)
                ///------------------------------------------------------------------------------------- ///
                else
                {
                    int index = oldSpawnLines.IndexOf(oldSpawnLines.Find(x => x.roomName.Equals(newRoomName)));
                    if (index != -1)
                    {
                        CustomWorldMod.Log($"[CreatureMerging] Updated line [{oldSpawnLines[index].line}] --> [{newWorldDataLine.Value.line}]");
                        oldSpawnLines[index] = newWorldDataLine.Value;
                    }
                    else
                    {
                        // FATAL ERROR
                        CustomWorldMod.Log($"[CreatureMerging] Fatal error when merging, line to be merged didn't exist " +
                            $"[{newWorldDataLine.Value.line}]", true);
                    }
                }
            }
            else
            {
                // do nothing
            }

            return oldSpawnLines;
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

        #endregion

        #region BAT MERGING
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
        #endregion

        /// <summary> Returns a string from a connectionList - CURSED
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

        /// <summary> Returns a List from a room-connection string
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
        /// Used for mergin algorithm
        /// </summary>
        private static MergeStatus status = (MergeStatus)5;

        /// <summary>
        /// Reads and loads all the world_XX.txt files found in all the custom worlds.
        /// </summary>
        /// 
        public static List<string> GetWorldLines(WorldLoader self)
        {
            return GetWorldLines(self.lines, self.worldName, self.playerCharacter);
        }

        public static List<string> GetWorldLines(List<string> selfLines, string selfWorldName, int selfPlayerCharacter)
        {
            List<string> lines = new List<string>();

            // Reset
            CustomWorldMod.analyzingLog = string.Empty;

            // Bool indicates whether it is vanilla or not
            List<WorldDataLine> ROOMS = new List<WorldDataLine>();
            List<WorldDataLine> CREATURES = new List<WorldDataLine>();
            List<WorldDataLine> BATS = new List<WorldDataLine>();

            if (selfLines.Count > 0)
            {
                // Fill ROOMS with vanilla rooms
                CustomWorldMod.Log("Custom Regions: Found vanilla room, filling lines");
                bool startRooms = false;
                bool startCreatures = false;
                bool startBats = false;

                foreach (string s in selfLines)
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
                            if (connections.Contains("DISCONNECT"))
                            {
                                connections.Replace("DISCONNECT", "DISCONNECTED");
                            }

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
                        // Pre-processing
                        string[] split = Regex.Split(s, " : ");
                        string roomName = string.Empty;
                        string spawns = string.Empty;
                        bool lineage = false;

                        List<string> logOuput = new List<string>();

                        // Corrupted line (this should not happen)
                        if (split.Length < 2)
                        {
                            CustomWorldMod.Log($"Corrupted vanilla line [{s}]", true);
                        }
                        else
                        {
                            if (split.Length > 2 && spawns.Contains("LINEAGE"))
                            {
                                roomName = split[1];
                                spawns = split[2];
                                lineage = true;
                            }
                            else
                            {
                                roomName = split[0];
                                spawns = split[1];
                            }
                        }
                        CREATURES.Add(new WorldDataLine(s, roomName, spawns, "", lineage, true, "Vanilla"));
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
                    selfWorldName,
                    Path.DirectorySeparatorChar,
                    "world_",
                    selfWorldName,
                    ".txt"
                });

                if (File.Exists(test))
                {
                    CustomWorldMod.Log($"Custom Regions: Found world_{selfWorldName}.txt from {keyValues.Value}");
                    foundAnyCustomRegion = true;
                    //self.lines = new List<string>();
                    string[] array = File.ReadAllLines(test);
                    if (!array.Contains("ROOMS") && !array.Contains("CREATURES") && !array.Contains("BLOCKAGES"))
                    {
                        CustomWorldMod.Log($"RegionPack [{keyValues.Key}] has corrupted world_{selfWorldName}.txt file: " +
                            $"Missing any ROOMS/CREATURES/BLOCKAGES delimiters", true);
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
                                            if (array2[k] == selfPlayerCharacter.ToString())
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
                                    CustomWorldMod.Log($"[{keyValues.Key}]: Merging rooms...", false, CustomWorldMod.DebugLevel.MEDIUM);
                                    status = MergeStatus.ROOMS;
                                }
                                else if (array[i] == "CREATURES")
                                {
                                    CustomWorldMod.Log($"[{keyValues.Key}]: Merging creatures...", false, CustomWorldMod.DebugLevel.MEDIUM);
                                    status = MergeStatus.CREATURES;
                                }
                                else if (array[i] == "BAT MIGRATION BLOCKAGES")
                                {
                                    CustomWorldMod.Log($"[{keyValues.Key}]: Merging bats...", false, CustomWorldMod.DebugLevel.MEDIUM);
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
                                            CREATURES = AddNewCreature(array[i], CREATURES, keyValues.Key);
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
                return selfLines;
            }

            if (!foundAnyCustomRegion)
            {
                CustomWorldMod.Log("Custom regions did not find any custom world_XX.txt files, so it will load vanilla. " +
                    "(if you were not expecting this it means you have something installed incorrectly)");
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

        // TO DO
        private static void AnalyzeWorldConnections()
        {
            List<string> testLines = null; // fill with vanilla
            string worldName = null;
            int playerCharacter = 0;


            testLines = GetWorldLines(testLines, worldName, playerCharacter);
        }

        #region Hooks
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
                else if (File.Exists(Custom.RootFolderDirectory() + arenaPath + ".txt"))
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

        #endregion

        
    }
}
