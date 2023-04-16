using CustomRegions.Mod;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static CustomRegions.CustomWorld.RegionPreprocessors;
using static CustomRegions.Mod.Structs;

namespace CustomRegions.CustomWorld
{
    internal class WorldLoaderHook
    {
        /// <summary> Enum used in the mergin process when loading the world_XX.txt file.
        /// </summary>
        public enum MergeStatus
        {
            ROOMS,
            CREATURES,
            BATS
        }

        /// <summary>
        /// Used for mergin algorithm
        /// </summary>
        private static MergeStatus status = (MergeStatus) 5;

        #region Loading times debug
        /*
        private static System.Diagnostics.Stopwatch absRoomLoadWatch = null;
        private static void WorldLoader_LoadAbstractRoom(On.WorldLoader.orig_LoadAbstractRoom orig, World world, string roomName, AbstractRoom room, RainWorldGame.SetupValues setupValues)
        {
            absRoomLoadWatch.Reset();
            absRoomLoadWatch.Start();
            orig(world, roomName, room, setupValues);
            absRoomLoadWatch.Stop();

            try {
                string[] levelText = File.ReadAllLines(WorldLoader.FindRoomFile(roomName, false, ".txt"));
                int baked = int.Parse(levelText[9].Split(new char[] { '|' })[0]);
                //CustomRegionsMod.CustomLog($"[WORLD LOADER] Room baked: {]", false, CustomRegionsMod.DebugLevel.MEDIUM);
                if (baked == 0) {
                    CustomRegionsMod.unbakedRooms.Add($"{roomName}");
                }
            } catch { }
        }

        private static void WorldLoader_CreatingAbstractRooms(On.WorldLoader.orig_CreatingAbstractRooms orig, WorldLoaderHook self)
        {
            absRoomLoadWatch.Start();
            orig(self);
            absRoomLoadWatch.Stop();
            DateTime date = new DateTime(absRoomLoadWatch.ElapsedTicks);
            CustomRegionsMod.CustomLog($"[WorldLoader]: AbstractRoom [{self.roomAdder[self.cntr][0]}]. Time Elapsed [{date.ToString("s.ffff")}s]", false, CustomRegionsMod.DebugLevel.FULL);
        }

        private static WorldLoaderHook.Activity activity = WorldLoaderHook.Activity.Finished;
        private static System.Diagnostics.Stopwatch activityWatch = null;
        private static System.Diagnostics.Stopwatch worldLoaderWatch = null;
        private static void WorldLoader_Update(On.WorldLoader.orig_Update orig, WorldLoaderHook self)
        {
            worldLoaderWatch.Start();
            if (activity != self.activity) {
                if (activityWatch != null) {
                    DateTime date = new DateTime(activityWatch.ElapsedTicks);
                    CustomRegionsMod.CustomLog($"[WorldLoader]: World [{self.worldName}] Activity [{activity}]. Time Elapsed [{date.ToString("s.ffff")}s]", false, CustomRegionsMod.DebugLevel.FULL);
                }
                activity = self.activity;
                activityWatch = new System.Diagnostics.Stopwatch();
            }
            activityWatch.Start();
            orig(self);
            activityWatch.Stop();
            worldLoaderWatch.Stop();
            if (self.Finished) {
                DateTime date2 = new DateTime(worldLoaderWatch.ElapsedTicks);
                CustomRegionsMod.CustomLog($"[WorldLoader]: Finished loading world [{self.worldName}]. Total time Elapsed [{date2.ToString("s.ffff")}s]", false, CustomRegionsMod.DebugLevel.RELEASE);

                if (CustomRegionsMod.unbakedRooms.Count > 0) {
                    string unbakedRooms = string.Join(", ", CustomRegionsMod.unbakedRooms.ToArray());
                    CustomRegionsMod.CustomLog($"Found unbaked rooms from [{self.worldName}]. \n[{unbakedRooms}]", true);
                }
            }
        }
        */
        #endregion


        public static void ApplyHooks()
        {
            On.WorldLoader.ctor_RainWorldGame_Name_bool_string_Region_SetupValues += WorldLoader_ctor_RainWorldGame_Name_bool_string_Region_SetupValues;
        }

        private static void WorldLoader_ctor_RainWorldGame_Name_bool_string_Region_SetupValues(On.WorldLoader.orig_ctor_RainWorldGame_Name_bool_string_Region_SetupValues orig, WorldLoader self, RainWorldGame game, SlugcatStats.Name playerCharacter, bool singleRoomWorld, string worldName, Region region, RainWorldGame.SetupValues setupValues)
        {
            orig(self, game, playerCharacter, singleRoomWorld, worldName, region, setupValues);

            RegionInfo regionInfo = new RegionInfo();
            regionInfo.RegionID = region.name;
            regionInfo.Lines = self.lines;

            foreach (RegionPreprocessor filter in regionPreprocessors)
            { filter(regionInfo); }

            self.lines = regionInfo.Lines;

            var analyzedLines = GetWorldLines(self);

            CustomRegionsMod.CustomLog("## ANALYZED LINES ##");
            foreach(var line in analyzedLines) {
                CustomRegionsMod.CustomLog(line.line);
            }
        }

        // IT MIGHT DO REDUNDANT OPERATIONS, NEEDS OPTIMIZATION
        private static List<WorldDataLine> AnalyzeMergedRooms(List<WorldDataLine> lines)
        {
            List<WorldDataLine> brokenLines = new List<WorldDataLine>();

            //  List<WorldDataLine> linesProcessed = new List<WorldDataLine>(lines);

            List<string> currentConnections;
            List<WorldDataLine> otherConnectedLines;
            List<WorldDataLine> fixedLines = new List<WorldDataLine>(lines);
            WorldDataLine currentLine;
            WorldDataLine otherConnectedLine;

            //foreach (var currentLine in lines)
            for (int i = 0; i < lines.Count(); i++) {
                currentLine = lines[i];
                CustomRegionsMod.CustomLog($"Analyzing line [{currentLine.line}]", false, CustomRegionsMod.DebugLevel.FULL);

                // All lines that contain current room in their connections
                otherConnectedLines = lines.FindAll(x => !x.roomName.Equals(currentLine.roomName)
                && FromConnectionsToList(x.connections).Contains(currentLine.roomName));

                // Check if current room is connected to any other room
                if (otherConnectedLines.Count == 0) {

                    currentConnections = Regex.Split(currentLine.connections, ", ").ToList();
                    if (currentConnections.FindAll(x => x.Contains("DISCONNECTED")).Count == currentConnections.Count) {
                        // room is disconnected from everything
                    } else {
                        // room is broken
                        // room is not connected
                        CustomRegionsMod.CustomLog("     Room does not appear elsewhere", false, CustomRegionsMod.DebugLevel.FULL);
                        brokenLines.Add(currentLine);


                        CustomRegionsMod.CustomLog($"          Broken connection. Current room does not appear in any other connection " +
                                    $"Disconnecting...");

                        // Disconnect current broken connection
                        WorldDataLine temp1;
                        int currentRoom = fixedLines.FindIndex(x => x.roomName.Equals(currentLine.roomName));
                        brokenLines.Add(currentLine);
                        temp1 = currentLine;
                        for (int n = 0; n < currentConnections.Count(); n++) {
                            currentConnections[n] = "DISCONNECTED";
                        }
                        string endingString = currentLine.endingString != string.Empty ? (" : " + currentLine.endingString) : "";
                        temp1.line = FromListToConnectionsString(currentLine.roomName, currentConnections) + endingString;
                        temp1.BuildRoomFromWholeLine(temp1.line);
                        fixedLines[currentRoom] = temp1;


                        CustomRegionsMod.CustomLog($"               Fixed current line [{fixedLines[currentRoom].line}]", false, CustomRegionsMod.DebugLevel.FULL);
                    }

                    //CustomRegionsMod.CustomLog("     ", false, CustomRegionsMod.DebugLevel.FULL);

                    /* REDUNDANT
                    connections = Regex.Split(currentLine.connections, ", ").ToList();
                    if (connections.FindAll(x => x.Contains("DISCONNECTED")).Count == connections.Count)
                    {
                        // room is disconnected from everything
                    }
                    else
                    {
                        // room is broken
                        brokenLines.Add(currentLine);
                        CustomRegionsMod.CustomLog($"Line is broken [{currentLine.line}]", false, CustomRegionsMod.DebugLevel.MEDIUM);
                    }
                    */
                }
                // Room is connected to another room
                else {
                    CustomRegionsMod.CustomLog($"     Room appears in other connections:\n     " +
                        $"{string.Join("\n     ", otherConnectedLines.Select(x => x.line).ToArray())}", false, CustomRegionsMod.DebugLevel.FULL);

                    //foreach (var otherConnectedLine in otherConnectedLines)

                    // All lines that have our room in their connections
                    for (int j = 0; j < otherConnectedLines.Count(); j++) {
                        /*List<WorldDataLine> linesToModify = linesNotProcessed.FindAll(x => !x.roomName.Equals(currentLine.roomName)).
                            FindAll(x => x.connections.Contains(currentLine.roomName));*/

                        otherConnectedLine = otherConnectedLines[j];
                        //connections = Regex.Split(otherConnectedLine.connections, ", ").ToList();

                        // Check if connection is reciprocal
                        if (!FromConnectionsToList(currentLine.connections).Contains(otherConnectedLine.roomName)) {
                            // Disconnect both lines between each other
                            CustomRegionsMod.CustomLog($"          Broken connection. Current line does not have room [{otherConnectedLine.roomName}]. Disconnecting...");

                            WorldDataLine temp1;
                            // Disconnect current room from other rooms
                            int indexOtherRoom = fixedLines.FindIndex(x => x.roomName.Equals(otherConnectedLine.roomName));
                            brokenLines.Add(fixedLines[indexOtherRoom]);
                            temp1 = fixedLines[indexOtherRoom];
                            temp1.line = fixedLines[indexOtherRoom].line.Replace(currentLine.roomName, "DISCONNECTED");
                            temp1.BuildRoomFromWholeLine(temp1.line);
                            fixedLines[indexOtherRoom] = temp1;

                            /*CustomRegionsMod.CustomLog($"Sanity check 1. Trying to replace [{currentLine.roomName}] at [{temp1.line}]. " +
                                $"Contains? [{temp1.line.Contains(currentLine.roomName)}].");*/

                            /*
                            int indexCurrentRoom = fixedLines.FindIndex(x => x.roomName.Equals(currentLine.roomName));
                            // Disconnect current room from other lines
                            temp1 = fixedLines[indexCurrentRoom];
                            temp1.line = fixedLines[indexCurrentRoom].line.Replace(otherConnectedLine.roomName, "DISCONNECT");
                            fixedLines[indexCurrentRoom] = temp1;
                            */

                            CustomRegionsMod.CustomLog($"               Fixed other line [{fixedLines[indexOtherRoom].line}]", false, CustomRegionsMod.DebugLevel.FULL);
                        }

                        /*
                         List<WorldDataLine> linesToModify = linesProcessed.FindAll(x => !x.roomName.Equals(currentLine.roomName)).
                         FindAll(x => x.connections.Contains(currentLine.roomName));
                         // Remove analyzed connections
                         foreach (var lineToModify in linesToModify)
                         {
                             string otherRoomName = lineToModify.roomName;
                             connections = Regex.Split(lineToModify.connections, ", ").ToList();
                             WorldDataLine updatedLine = lineToModify;
                             // Remove all connections that match the current room
                             connections.RemoveAll(x => x.Equals(currentLine.roomName));
                             connections.RemoveAll(x => x.Equals("DISCONNECTED"));
                             int indexB = linesProcessed.FindIndex(x => x.Equals(lineToModify));
                             if (indexB < 0 || indexB >= linesProcessed.Count())
                             {
                                 CustomRegionsMod.CustomLog($"Index out of range ???? [{indexB}] ");
                             }
                             else
                             {
                                 // Update not processed lines
                                 updatedLine.connections = string.Join(", ", connections.ToArray());
                                 linesProcessed[indexB] = updatedLine;
                             }
                         }
                        */

                    }
                    // check if all our connections appear elsewhere
                    currentConnections = FromConnectionsToList(currentLine.connections);
                    for (int l = 0; l < currentConnections.Count(); l++) {
                        if (currentConnections[l].Equals("DISCONNECTED")) {
                            continue;
                        }
                        CustomRegionsMod.CustomLog($"          Does [{currentConnections[l]}] appear elsewhere ...", false, CustomRegionsMod.DebugLevel.FULL);
                        /*
                        bool found = false;
                        for (int c = 0; c < otherConnectedLines.Count; c++)
                        {
                            List<string> temp123 = FromConnectionsToList(otherConnectedLines[c].connections);
                            found = temp123.Contains(currentConnections[l]);
                            CustomRegionsMod.CustomLog($"Comparing to [{otherConnectedLines[c].connections}]. Found [{found}]");
                            if (found) break;
                        }*/
                        if (otherConnectedLines.FindAll(x => x.roomName.Equals(currentConnections[l])).Count() == 0)
                        //if (!found)
                        {
                            // current line is connected to nowwhere
                            CustomRegionsMod.CustomLog($"          Broken connection. Current line has a broken connection [{currentConnections[l]}]. " +
                                $"Disconnecting...");

                            // Disconnect current broken connection
                            WorldDataLine temp1;
                            int currentRoom = fixedLines.FindIndex(x => x.roomName.Equals(currentLine.roomName));
                            brokenLines.Add(currentLine);
                            temp1 = currentLine;
                            temp1.line = fixedLines[currentRoom].line.Replace(currentConnections[l], "DISCONNECTED");
                            temp1.BuildRoomFromWholeLine(temp1.line);
                            fixedLines[currentRoom] = temp1;


                            CustomRegionsMod.CustomLog($"               Fixed current line [{fixedLines[currentRoom].line}]", false, CustomRegionsMod.DebugLevel.FULL);

                        }
                    }
                }

                /*
                // Check if room has broken connections
                connections = Regex.Split(currentLine.connections, ", ").ToList();
                foreach (var item in connections)
                {
                    if (!item.Equals("DISCONNECTED"))
                    {
                        if (lines.FindAll(x => x.connections.Contains(item)).Count() == 0)
                        {
                            // Connection is broken
                            linesNotProcessed.Add(currentLine);
                        }
                        else
                        {
                            // Connection is not broken
                        }
                    }
                }
                */
            }

            /*
            linesProcessed.RemoveAll(x => x.connections.Equals(string.Empty));
            //linesNotProcessed.Concat(brokenLines);
            if (linesProcessed.Count != 0)
            {
                CustomRegionsMod.CustomLog($"Found broken connections in world file! Read customWorldCustomLog.txt for more information", true);
                CustomRegionsMod.CustomLog($"Broken connections:");
                foreach (var item in linesProcessed)
                {
                    CustomRegionsMod.CustomLog($"Room: [{item.roomName}]. Connections: [{item.connections}]");
                }
            }
            */
            brokenLines = brokenLines.Distinct().ToList();
            if (brokenLines.Count != 0) {
                CustomRegionsMod.CustomLog($"\nThese lines were disconnected:");
                foreach (var item in brokenLines) {
                    CustomRegionsMod.CustomLog($"Room: [{item.roomName}]. Connections: [{item.connections}] " +
                        $"replaced with -> [{Regex.Split(fixedLines.Find(x => x.roomName.Equals(item.roomName)).line, " : ")[1]}]");
                }
                CustomRegionsMod.CustomLog($"Found broken connections in world file! Read {CustomRegionsMod.logFileName} for more information", true);
            }

            return fixedLines;
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

            if (split.Length >= 2) {
                // Remove base room
                position = 1;
            }

            string[] split_rooms = Regex.Split(split[position], ", ");

            foreach (string s in split_rooms) {
                if (s.Trim() != "") {
                    connections.Add(s);
                }
            }
            // updatedConnections = updatedConnections.Where(x => !string.IsNullOrEmpty(x)).ToArray();

            //CustomWorldMod.CustomWorldLog($"Custom Regions: FromConnectionToList [{string.Join(",", connections.ToArray())}]");

            return connections;
        }

        /// <summary> Returns a string from a connectionList - CURSED
        /// </summary>
        public static string FromListToConnectionsString(string roomName, List<string> connections)
        {
            roomName += " : ";
            int a = 0;
            foreach (string connection in connections) {
                if (a == 0) {
                    roomName += connection;
                } else {
                    roomName += ", " + connection;
                }
                a++;
            }
            //CustomWorldMod.CustomWorldLog($"Custom Regions: FromListToConnection: [{oldRoom}]");
            return roomName;
        }


        /// <summary>
        /// Reads and loads all the world_XX.txt files found in all the custom worlds.
        /// </summary>
        /// 
        public static List<WorldDataLine> GetWorldLines(WorldLoader self)
        {
            return GetWorldLines(self.lines, self.worldName, self.playerCharacter);
        }

        public static List<WorldDataLine> GetWorldLines(List<string> selfLines, string selfWorldName, SlugcatStats.Name selfPlayerCharacter)
        {
            List<string> lines = new List<string>();

            // Reset
            CustomRegionsMod.analyzingLog = string.Empty;

            // Bool indicates whether it is vanilla or not
            List<WorldDataLine> ROOMS = new List<WorldDataLine>();
            List<WorldDataLine> CREATURES = new List<WorldDataLine>();
            List<WorldDataLine> BATS = new List<WorldDataLine>();

            // Game loaded a world_XX.txt file from vanilla
            if (selfLines.Count > 0) {
                // Fill ROOMS with vanilla rooms
                CustomRegionsMod.CustomLog("Custom Regions: Found vanilla room, filling lines");

                //API.RegionInfo worldInfo = new API.RegionInfo();

                //worldInfo.PackName = string.Empty;
                //worldInfo.RegionID = selfWorldName;
                //worldInfo.Lines = selfLines;
                //worldInfo.Vanilla = true;

                //CustomRegionsMod.CustomLog($"[Vanilla] Loading line filters by other mods... count [{CustomRegionsMod.regionPreprocessors.Count}]", false, CustomRegionsMod.DebugLevel.MEDIUM);
                //foreach (var filter in CustomRegionsMod.regionPreprocessors) {
                //    filter(worldInfo);
                //}


                bool startRooms = false;
                bool startCreatures = false;
                bool startBats = false;

                foreach (string s in selfLines) {
                    // ROOMS
                    if (s.Equals("END ROOMS")) {
                        startRooms = false;
                    }
                    if (startRooms) {
                        // Pre-processing
                        string[] split = Regex.Split(s, " : ");
                        string roomName = string.Empty;
                        string connections = string.Empty;
                        string endingString = string.Empty;

                        List<string> logOuput = new List<string>();

                        // Corrupted line (this should not happen)
                        if (split.Length < 2 || split.Length > 3) {
                            CustomRegionsMod.CustomLog($"Corrupted vanilla line [{s}]", true);
                        } else {
                            roomName = split[0];
                            connections = split[1];
                            if (connections.Contains("DISCONNECT")) {
                                connections.Replace("DISCONNECT", "DISCONNECTED");
                            }

                            // Line has ending
                            if (split.Length == 3) {
                                endingString = split[2];
                            }
                        }
                        ROOMS.Add(new WorldDataLine(s, roomName, connections, endingString, true, "Vanilla"));
                    }
                    if (s.Equals("ROOMS")) {
                        startRooms = true;
                    }

                    // CREATURES
                    if (s.Equals("END CREATURES")) {
                        startCreatures = false;
                    }
                    if (startCreatures) {
                        // Pre-processing
                        string[] split = Regex.Split(s, " : ");
                        string roomName = string.Empty;
                        string spawns = string.Empty;
                        bool lineage = false;

                        List<string> logOuput = new List<string>();

                        // Corrupted line (this should not happen)
                        if (split.Length < 2) {
                            CustomRegionsMod.CustomLog($"Corrupted vanilla line [{s}]", true);
                        } else {
                            if (split.Length > 2 && spawns.Contains("LINEAGE")) {
                                roomName = split[1];
                                spawns = split[2];
                                lineage = true;
                            } else {
                                roomName = split[0];
                                spawns = split[1];
                            }
                        }
                        CREATURES.Add(new WorldDataLine(s, roomName, spawns, "", lineage, true, "Vanilla"));
                    }
                    if (s.Equals("CREATURES")) {
                        startCreatures = true;
                    }

                    // BAT MIGRATIONS
                    if (s.Equals("END BAT MIGRATION BLOCKAGES")) {
                        startBats = false;
                    }
                    if (startBats) {
                        BATS.Add(new WorldDataLine(s, true));
                    }
                    if (s.Equals("BAT MIGRATION BLOCKAGES")) {
                        startBats = true;
                    }
                }
            }


            // Check for problems
            List<WorldDataLine> fixedROOMS = AnalyzeMergedRooms(ROOMS);
            ROOMS = fixedROOMS;

            if (CustomRegionsMod.analyzingLog != string.Empty) {
                CustomRegionsMod.CustomLog("Found possible incompatibilities! You might be missing compatibility patch or this two packs are incompatible. " +
                    "Please check customWorldLog or the Analyzer tab for more information.", true);
            }
            return ROOMS;


            //// Sort lists to increase readability 
            //List<string> sortedRooms = CustomRegionsMod.FromWorldDataToListString(ROOMS);
            //sortedRooms.Sort();

            //List<string> sortedCreatures = CustomRegionsMod.FromWorldDataToListString(CREATURES);
            //sortedCreatures.Sort();

            //List<string> sortedBats = CustomRegionsMod.FromWorldDataToListString(BATS);
            //sortedBats.Sort();

            //lines = BuildWorldText(sortedRooms, sortedCreatures, sortedBats);


            //if (lines.Count < 2) {
            //    CustomRegionsMod.CustomLog("Lines.Count < 2", true);
            //    return selfLines;
            //}

            //if (!foundAnyCustomRegion) {
            //    CustomRegionsMod.CustomLog($"Custom regions did not find any custom world_{selfWorldName}.txt files, so it will load vanilla. " +
            //        "(if you were not expecting this it means you have something installed incorrectly)");
            //} else {
            //    CustomRegionsMod.CustomLog($"\nMerged world_{selfWorldName}.txt file");
            //    CustomRegionsMod.CustomLog(string.Join($"\n", lines.ToArray()));
            //    CustomRegionsMod.CustomLog("\n");
            //}
            //return lines;
        }

    }
}
