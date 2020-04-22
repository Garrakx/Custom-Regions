using Partiality.Modloader;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Security;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Linq;


// Delete Publicity Stunt requirement by pastebee
[assembly: IgnoresAccessChecksTo("Assembly-CSharp")]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[module: UnverifiableCode]

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class IgnoresAccessChecksToAttribute : Attribute
    {
        public IgnoresAccessChecksToAttribute(string assemblyName)
        {
            AssemblyName = assemblyName;
        }

        public string AssemblyName { get; }
    }
}

namespace CustomRegions.Mod
{

    public class CustomWorldMod : PartialityMod
    {
        public static CustomWorldScript script;
        public static CustomWorldConfig config;

        public CustomWorldMod()
        {
            ModID = "Custom Regions Mod";
            Version = "0.1";
            author = "Garrakx & topicular";
        }


        /*
        public static CustomWorldOption LoadOI()
        {
            return new CustomWorldOption();
        }
        */


        // Code for AutoUpdate support
        // Should be put in the main PartialityMod class.
        // Comments are optional.

        // Update URL - don't touch!
        // You can go to this in a browser (it's safe), but you might not understand the result.
        // This URL is specific to this mod, and identifies it on AUDB.
        public string updateURL = "http://beestuff.pythonanywhere.com/audb/api/mods/3/0";
        // Version - increase this by 1 when you upload a new version of the mod.
        // The first upload should be with version 0, the next version 1, the next version 2, etc.
        // If you ever lose track of the version you're meant to be using, ask Pastebin.
        public int version = 6;
        // Public key in base64 - don't touch!
        public string keyE = "AQAB";
        public string keyN = "13Mr+YOzb1iLnJvzkuP4NEZEWwOtWKWvWAN0HdsQ5SF2+RG7k8FbtmQut+2+69ideiJHDW66jWBcGGvfiQ0+5yLAUBpGSckC7V79yZgFQT39lvgU0ykAjonkA+ZTODFnehubyCkrrrzwno4boZghEZmDS2YsSyDJ6RLJyD2/WeCokcTj1vIHZhY9DzkooFtejz9yI/PCZtq8tfq2AzSiQPS+0xGQs3fnAkOGoV1WZ/inW5/rRyjD5HICr8t79UmcopfRK383YBrf2G96HeVYvY2vwSS/BW/m32rTLOZHr+XX7SIZshz7BLK6xEssy4qXjskvAUshqNudxtQnIkShGJuKWF1V2vvwqgY/IZiAbDXdBOUaSd09ldHBlTz9EfzBcgqffVRaUTzS71yGLISyrLriezozlK1YZW9vvijpbD0rmDaJ4aq9s6EzhdgVkTEuChtm/Fj9pgsswjvkbgHw1t9QZWqu4pweNd3IE/Lktst8HBKLiw1aRaffbZIhh1apbyjF8iflD8sNzbIHEfEvc35MEwIFqibJVnVxppBa15HpOxeXOzwuTjFaLSURRvbOEFPmpyd1Nm4nMzZZHHPjQXT7oYQAxjSCfqnLAdYsEnNo/2172jJGLfBWWGFTavqiCYqLhjtYkPfRgpcdw4FldgjX4w7RGMD/Ra5VXvmDMTE=";
        // ------------------------------------------------


        public override void OnEnable()
        {
            base.OnEnable();

            GameObject gameObject = new GameObject();
            script = gameObject.AddComponent<CustomWorldScript>();
            CustomWorldScript.mod = this;

            config = default(CustomWorldConfig);



            script.Initialize();

        }

        public struct CustomWorldConfig
        {

        }

        /// <summary>
        /// Enum used in the mergin process when loading the world_XX.txt file.
        /// </summary>
        public enum MergeStatus
        {
            ROOMS,
            CREATURES,
            BATS
        }


        public struct WorldDataLine
        {
            public string data;
            public bool vanilla;
            public WorldDataLine(string data, bool vanilla)
            {
                this.data = data;
                this.vanilla = vanilla;
            }
        }

        public struct CreatureLine
        {
            public bool lineage;
            public string room;
            public string[] connectedDens;
            public string dens;
            public int denNumber;

            public CreatureLine(bool lineage, string room, string[] connectedDens)
            {
                this.lineage = lineage;
                this.room = room;
                this.connectedDens = connectedDens;

                this.dens = null;
                this.denNumber = -1;
            }

            public CreatureLine(bool lineage, string room, string dens, int denNumber)
            {
                this.lineage = lineage;
                this.room = room;
                this.dens = dens;
                this.denNumber = denNumber;

                this.connectedDens = null;
            }
        }

        /// <summary>
        /// Providing an array with vanilla region IDs, returns this array but with the new regionsID added from the CustomWorldMod.lodadedRegions dictionary.
        /// </summary>
        /// <returns>returns string[] regionsID</returns>
        public static string[] AddModdedRegions(string[] regionNames)
        {
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
            {
                //Debug.Log($"Custom Regions: PlayerProgression, loading new regions");
                string regionToAdd = keyValues.Key;
                bool shouldAdd = true;

                for (int i = 0; i < regionNames.Length; i++)
                {
                    if (regionToAdd.Equals(regionNames[i]))
                    {
                        shouldAdd = false;
                    }
                }
                if (shouldAdd)
                {
                    Array.Resize(ref regionNames, regionNames.Length + 1);
                    regionNames[regionNames.Length - 1] = keyValues.Key;
                    Debug.Log($"Custom Regions: Added new region [{regionToAdd}] from [{keyValues.Value}].");
                }
            }
            return regionNames;
        }


        /// <summary>
        /// Compares and merges a room-connection in the existing room list
        /// This method should be heavily optimized and cleaned up.
        /// </summary>
        public static List<WorldDataLine> AddNewRoom(string newRoom, List<WorldDataLine> oldList)
        {
            bool sameConnections = false;
            string roomToBeReplaced = string.Empty;

            foreach (WorldDataLine oldRoom in oldList)
            {
                if (oldRoom.data.Equals(newRoom))
                {
                    // The room is exactly the same, skipped
                    sameConnections = true;
                    roomToBeReplaced = string.Empty;
                    break;
                }
                else
                {
                    try
                    {
                        string oldBaseRoom = oldRoom.data.Substring(0, oldRoom.data.IndexOf(" "));
                        string newBaseRoom = newRoom.Substring(0, newRoom.IndexOf(" "));

                        if (oldBaseRoom.Equals(newBaseRoom))
                        {
                            // The room is the same but different connections
                            roomToBeReplaced = oldRoom.data;

                            if (!sameConnections)
                            {
                                Debug.Log($"Custom Regions: Found conflict [{oldRoom.data}] with [{newRoom}]");
                                //roomToBeReplaced = string.Empty;
                            }
                        }
                    }
                    catch (Exception e) { }

                }
            }

            if (roomToBeReplaced == string.Empty)
            {
                if (!sameConnections)
                {
                    //Debug.Log($"Custom Regions: Added new room [{newRoom}]");
                    oldList.Add(new WorldDataLine(newRoom, false));
                }
            }
            else
            {
                //Debug.Log($"Custom Regions: Found conflict [{newRoom}]");

                //Debug.Log($"Custom Regions: Trying to merge {roomToBeReplaced}");

                // Check if containts GATE/SHELTER/SWARMROOM at the end
                string endingSetting = string.Empty;
                string temp = roomToBeReplaced.Substring(roomToBeReplaced.IndexOf(": ") + 2);
                if (temp.IndexOf(": ") > 0)
                {
                    endingSetting = temp.Substring(temp.IndexOf(": ") + 2);
                }

                List<string> oldConnections = FromConnectionsToList(roomToBeReplaced);
                List<string> newConnections = FromConnectionsToList(newRoom);

                bool isRoomBeingReplaced = false;
                bool allEmpty = true;
                // Build new connections
                for (int i = 0; i < oldConnections.Count; i++)
                {
                    for (int j = 0; j < newConnections.Count; j++)
                    {
                        if (!oldConnections[i].Equals(newConnections[j]))
                        {
                            if (oldConnections[i].Equals("DISCONNECTED") && !newConnections[j].Equals("DISCONNECTED") && !newConnections[j].Equals("DISCONNECT"))
                            {
                                oldConnections[i] = newConnections[j];
                                allEmpty = false;
                                //Debug.Log($"Custom Regions: Added [{newConnections[j]}] to [{conflictingRoom}]");
                            }
                            else if (oldConnections[i].Equals("DISCONNECT") && !newConnections[j].Equals("DISCONNECTED") && !newConnections[j].Equals("DISCONNECT"))
                            {
                                oldConnections[i] = newConnections[j];
                                allEmpty = false;
                                //Debug.Log($"Custom Regions: Added [{newConnections[j]}] to [{conflictingRoom}]");
                            }
                            else
                            {
                                isRoomBeingReplaced = true;
                            }
                            /*
                            else if(!oldConnections[i].Equals("DISCONNECTED") && newConnections[j].Equals("DISCONNECTED"))
                            {
                                newConnections[j] = oldConnections[i];
                                Debug.Log($"Custom Regions: Added [{oldConnections[i]}] to [{conflictingRoom}]");
                            }
                            else if(!oldConnections[i].Equals("DISCONNECTED") && !newConnections[j].Equals("DISCONNECTED"))
                            {
                                Debug.Log("Custom Regions: ERROR!!! Regions incompatible!!!");
                                break;
                            }
                            */
                        }
                    }
                }
                //bool performedOperations = false;
                if (isRoomBeingReplaced && allEmpty)
                {
                    bool isVanilla = oldList.Find(x => x.data.Equals(roomToBeReplaced)).vanilla;

                    Debug.Log($"Custom Regions: Comparing two rooms without disconnected pipes. [{roomToBeReplaced}] is vanilla: [{isVanilla}].");
                    if (newConnections.Count > oldConnections.Count || isVanilla)
                    {
                        oldConnections = newConnections;
                        //performedOperations = true;
                    }

                    if (oldConnections.Contains(roomToBeReplaced))
                    {
                        Debug.Log($"Custom Regions: Connections has conflict still. [{string.Join(", ", oldConnections.ToArray())}]");
                    }

                    endingSetting = endingSetting != string.Empty ? ": " + endingSetting : "";

                    // Convert from list to connections
                    string updatedConnections = FromListToConnections(roomToBeReplaced.Substring(0, roomToBeReplaced.IndexOf(" ")), oldConnections);
                    //int index = oldList.IndexOf(conflictingRoom);
                    int index = oldList.IndexOf(oldList.Find(x => x.data.Equals(roomToBeReplaced)));
                    if (index != -1)
                    {
                        Debug.Log($"Custom Regions: Replaced [{oldList[index].data}] with [{updatedConnections + endingSetting}]");
                        oldList[index] = new WorldDataLine(updatedConnections + endingSetting, false);
                    }
                }

            }
            /*List<string> updatedList = new List<string>();
            foreach(worldData data in oldList)
            {
                updatedList.Add(data.data);
            }*/
            return oldList;
        }

        public static List<string> fromWorldDataToList(List<WorldDataLine> worldData)
        {
            List<string> updatedList = new List<string>();
            foreach(WorldDataLine data in worldData)
            {
                updatedList.Add(data.data);
            }

            return updatedList;
        }

        /// <summary>
        /// Returns a string from a connectionList
        /// </summary>
        public static string FromListToConnections(string oldRoom, List<string> connections)
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
            //Debug.Log($"Custom Regions: FromListToConnection: [{oldRoom}]");
            return oldRoom;
        }

        /// <summary>
        /// Returns a List from a room-connection string
        /// This method should be heavily optimized and cleaned up.
        /// </summary>
        public static List<string> FromConnectionsToList(string oldConnections)
        {
            // Removing the base room
            string splittingOldRoom = oldConnections.Substring(oldConnections.IndexOf(": ") + 2);

            // Remove GATE/SHELTER/SWARMROOM at the end
            if (splittingOldRoom.IndexOf(": ") > 0)
            {
                splittingOldRoom = splittingOldRoom.Substring(0, splittingOldRoom.IndexOf(": "));
            }
            string debug = "Custom Regions: FromConnectionToList: [";

            // Contains each connection
            List<string> connections = new List<string>();

            // I am sure this can be optimized
            int position = splittingOldRoom.IndexOf(", ");
            if (position < 0)
            {
                // Only one connection
                connections.Add(splittingOldRoom);
                debug += splittingOldRoom + "; ";
            }
            while (position > 0)
            {
                connections.Add(splittingOldRoom.Substring(0, position));
                debug += splittingOldRoom.Substring(0, position) + "; ";
                splittingOldRoom = splittingOldRoom.Substring(position + 2);
                position = splittingOldRoom.IndexOf(", ");
            }
            if (splittingOldRoom != string.Empty)
            {
                connections.Add(splittingOldRoom);
                debug += splittingOldRoom + "; ";
            }
            //Debug.Log(debug + "]");
            return connections;
        }


        internal static List<WorldDataLine> AddNewCreature(string newCreatureLine, List<WorldDataLine> oldCreaturesSpawns)
        {
            bool sameCreatureLine = false;
            string creatureLineBeReplaced = string.Empty;


            bool lineage = false;
            string roomNameNewLine = string.Empty;

            Debug.Log($"Custom Regions: Adding new creature spawn [{newCreatureLine}]]");

            if (newCreatureLine.Contains("OFFSCREEN"))
            {
                creatureLineBeReplaced = newCreatureLine;
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

                    if (oldSpawnLine.data.Equals(newCreatureLine))
                    {
                        // The spawn is exactly the same, skipped
                        sameCreatureLine = true;
                        creatureLineBeReplaced = string.Empty;
                        break;
                    }
                    else
                    {

                        //Debug.Log($"Custom Regions: Splitting [{newCreatureLine}]");
                        // Debug.Log($"Custom regions: Testing Creature listing [{string.Join(",", oldLinesList.ToArray())}]");

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
                        // Debug.Log($"Custom Regions: Comparing [{oldLinesList[roomIndex]}] with [{roomNameNewLine}]");


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




                        // Debug.Log($"Custom Regions: Analyzing [{newCreatureLine}]. Room Name [{roomName}]");


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

                    Debug.Log($"Custom Regions: Trying to merge creature [{newCreatureLine}] with [{creatureLineBeReplaced}] (vanilla [{isVanilla}])");

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
                                        Debug.Log($"Custom Regions: Empty pipe, filling with [{newLines.connectedDens[i]}]");;
                                        shouldAdd = true;
                                    }
                                    else if (isVanilla)
                                    {
                                        Debug.Log($"Custom Regions: replacing vanilla pipe [{oldLines.connectedDens[i]}] with [{newLines.connectedDens[i]}]");
                                        empty = false;
                                        shouldAdd = true;
                                    }
                                }
                                catch (Exception e) { shouldAdd = true; }
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
                            catch (Exception e) { }
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
            }

            return oldCreaturesSpawns;
        }


        public static List<string> FromSpawnLinesToList(string spawnLine)
        {
            string elementToAdd = string.Empty;
            string splittedLine = spawnLine;
            string delimitator = " : ";
            string delimitator2 = ", ";
            int index = 0;

            List<string> spawnList = new List<string>();

            // SEPARATE DIFFICULTY
            /*if (spawnLine.Contains("("))
            {
                index = splittedLine.IndexOf(")");
                spawnList.Add(splittedLine.Substring(0, index+1));
                splittedLine = splittedLine.Substring(index + 2);
            }

            // SEPARATE LINEAGE or OFFSCREEN
            if (splittedLine.Contains("LINEAGE") || splittedLine.Contains("OFFSCREEN"))
            {
                index = splittedLine.IndexOf(delimitator);
                spawnList.Add(splittedLine.Substring(0, index));
                splittedLine = splittedLine.Substring(index + delimitator.Length);
            }*/

            while (splittedLine.Contains(delimitator))
            {
                index = splittedLine.IndexOf(delimitator);
                spawnList.Add(splittedLine.Substring(0, index));
                splittedLine = splittedLine.Substring(index + delimitator.Length);
            }

            while(splittedLine.Contains(delimitator2))
            {
                index = splittedLine.IndexOf(delimitator2);
                spawnList.Add(splittedLine.Substring(0, index));
                splittedLine = splittedLine.Substring(index + delimitator2.Length);
            }
            spawnList.Add(splittedLine);

            return spawnList;

        }


        public static CreatureLine FillCreatureLine(string lines)
        {
            string roomName = string.Empty;
            string[] connectedDens = new string[0];

            string[] line = Regex.Split(lines, " : ");

            if (line[0] == "LINEAGE")
            {
                roomName = line[1];
                //Debug.Log($"Custom Regions: Creating creature line. Lineage[{true}]. RoonName[{roomName}]. Spawners[{line[3]}]. DenNumber[{int.Parse(line[2])}]");
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

                //Debug.Log($"Custom Regions: Creating creature line. Lineage[{false}]. RoonName[{roomName}]. Spawners[{connectedDens.Length}]");
                return new CreatureLine(false, roomName, connectedDens);

            }

        }

/*
        public static void ClassifyTypeOfSpawnLine(string spawnLine, out string lineageLine, out string offscreenLine, out string regularLine)
        {
            lineageLine = string.Empty;
            offscreenLine = string.Empty;
            regularLine = string.Empty;

            string[] splitted = Regex.Split(spawnLine, " : ");
            if (splitted[0] == "LINEAGE")
            {
                lineageLine = splitted[1];
            }
            else if (splitted[0] == "OFFSCREEN")
            {
                offscreenLine = splitted[1];
            }
            else
            {
                regularLine = splitted[1];
            }
        }*/


        /// <summary>
        /// Holds the value of the sceneID in use.
        /// </summary>
        public static string sceneCustomID = string.Empty;


        /// <summary>
        /// Returns the vanilla regions ID.
        /// </summary>
        public static string[] VanillaRegions()
        {
            return new string[] { "CC", "DS", "HI", "GW", "SI", "SU", "SH", "SL", "LF", "UW", "SB", "SS" };
        }

        /// <summary>
        /// Fills the region array with the custom regions. UNUSED
        /// </summary>
        public static void FillCustomRegions(OverWorld self, RainWorldGame game)
        {

            string[] array = new string[]
            {
                string.Empty
            };

            string[] vanillaRegions = VanillaRegions();
            for (int i = 0; i < vanillaRegions.Length; i++)
            {
                using (StreamWriter file = new StreamWriter(string.Concat(new object[]
                        {
                    Custom.RootFolderDirectory(),
                    "World",
                    Path.DirectorySeparatorChar,
                    "Regions",
                    Path.DirectorySeparatorChar,
                    "regions.txt"
                        })))
                {
                    file.WriteLine(vanillaRegions[i]);
                }
            }

            foreach (KeyValuePair<string, string> keyValues in loadedRegions)
            {
                //Debug.Log($"Custom Regions: Loading custom properties for {keyValues.Key}");
                string path = resourcePath + keyValues.Value + Path.DirectorySeparatorChar;

                if (File.Exists(string.Concat(new object[]
                    {
                    Custom.RootFolderDirectory(),
                    "World",
                    Path.DirectorySeparatorChar,
                    "Regions",
                    Path.DirectorySeparatorChar,
                    "regions.txt"
                    })))
                {
                    array = File.ReadAllLines(string.Concat(new object[]
                    {
                        Custom.RootFolderDirectory(),
                        "World",
                        Path.DirectorySeparatorChar,
                        "Regions",
                        Path.DirectorySeparatorChar,
                        "regions.txt"
                    }));
                }
                int originalSize = self.regions.Length;
                int num = originalSize - 1;
                Array.Resize(ref self.regions, originalSize + array.Length);
                for (int i = originalSize - 1; i < array.Length; i++)
                {
                    Debug.Log($"Custom Regions: Added new region {array[i]} from {keyValues.Value}. Number of rooms {num}. Region position {i}");
                    using (StreamWriter file = new StreamWriter(string.Concat(new object[]
                    {
                    Custom.RootFolderDirectory(),
                    "World",
                    Path.DirectorySeparatorChar,
                    "Regions",
                    Path.DirectorySeparatorChar,
                    "regions.txt"
                    }), true))
                    {
                        file.WriteLine(keyValues.Value);
                    }
                    self.regions[i] = new Region(array[i], num, i);
                    num += self.regions[i].numberOfRooms;
                }
            }

            /*string regionDebugString = "Custom World: Total regions: {";
            foreach (Region regionFound in self.regions)
            {
                regionDebugString += regionFound.name + ", ";
            }
            Debug.Log(regionDebugString.TrimEnd(',', ' ') + "}");
            */
        }

        /// <summary>
        /// Dictionary where the Key is the region ID and the value is the name.
        /// </summary>
        public static Dictionary<string, string> loadedRegions;

        /// <summary>
        /// path of the CustomResources folder (Mods\CustomResources\)
        /// </summary>
        public static string resourcePath = "Mods" + Path.DirectorySeparatorChar + "CustomResources" + Path.DirectorySeparatorChar;

        /// <summary>
        /// Builds a dictionary where the Key is the region ID and the value is the name.
        /// </summary>
        /// <returns>returns a Dictionary(string, string)</returns>
        public static Dictionary<string, string> BuildModRegionsDictionary()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();

            // Each directory is a custom region mod
            string[] moddedRegions = Directory.GetDirectories(resourcePath);

            foreach (string region in moddedRegions)
            {
                string temp = Path.GetFileNameWithoutExtension(region);
                char[] tempChar = { temp[0], temp[1] };

                // Used in case region does not include \regionID.txt
                string regionID = new string(tempChar);

                string regionIDpath = region + Path.DirectorySeparatorChar + "regionID.txt";
                if (File.Exists(regionIDpath))
                {
                    regionID = File.ReadAllText(regionIDpath);

                    regionID = regionID.ToUpper();
                    dictionary.Add(regionID, Path.GetFileNameWithoutExtension(region));
                }
                else
                {
                    Debug.Log($"Custom Regions: Error! regionID.txt for {temp} does not exist");
                }
            }

            return dictionary;
        }

        public static void CreateCustomWorldLog()
        {
            using (StreamWriter sw = File.CreateText(Custom.RootFolderDirectory() + "customWorldLog.txt"))
            {
                sw.WriteLine("######################\n Custom World Log\n");
            }
        }

        public static void CreateCustomResourceFolder()
        {
            if(!Directory.Exists(Custom.RootFolderDirectory()+resourcePath)) 
            {
                Directory.CreateDirectory(Custom.RootFolderDirectory() + resourcePath);
            }
        }

        public static void CustomWorldLog(string test)
        {
            if (!File.Exists(Custom.RootFolderDirectory() + "customWorldLog.txt"))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(Custom.RootFolderDirectory() + "customWorldLog.txt"))
                {
                    sw.WriteLine("######################\n Custom World Log\n");
                }
            }

            using (StreamWriter file =
            new StreamWriter(Custom.RootFolderDirectory() + "customWorldLog.txt", true))
            {
                file.WriteLine(test);
            }

        }

        /// <summary>
        /// Holds the properties of the region
        /// </summary>
        public static Dictionary<string, int> dictionaryProperties;

        /// <summary>
        /// Returns vanilla world file, or other CustomWorld mod file if there is one
        /// </summary>
        /// <returns>Vanilla World path</returns>
        public static string FindVanillaRoom(string roomName, bool includeRootDirectory)
        {
            string result = "";

            string gatePath = Custom.RootFolderDirectory() + "World" + Path.DirectorySeparatorChar + "Gates" + Path.DirectorySeparatorChar + roomName;
            string gateShelterPath = Custom.RootFolderDirectory() + "World" + Path.DirectorySeparatorChar + "Gates" + Path.DirectorySeparatorChar + "Gate shelters" + Path.DirectorySeparatorChar + roomName;
            string regularRoomPath = Custom.RootFolderDirectory() + "World" + Path.DirectorySeparatorChar + "Regions" + Path.DirectorySeparatorChar + Regex.Split(roomName, "_")[0];
            string arenaPath = Custom.RootFolderDirectory() + "Levels" + Path.DirectorySeparatorChar + roomName;

            // room is regular room
            if (Directory.Exists(regularRoomPath) && File.Exists(regularRoomPath + Path.DirectorySeparatorChar + "Rooms" + Path.DirectorySeparatorChar + roomName + ".txt"))
            {
                result = Custom.RootFolderDirectory() + "World" + Path.DirectorySeparatorChar + "Regions" + Path.DirectorySeparatorChar + Regex.Split(roomName, "_")[0] + Path.DirectorySeparatorChar + "Rooms" + Path.DirectorySeparatorChar + roomName;
                //Debug.Log($"Custom Regions: Found room {roomName} in {keyValues.Key}. Path: {result}");
            }
            // room is GATE
            else if (Regex.Split(roomName, "_")[0] == "GATE" && File.Exists(Custom.RootFolderDirectory() + gatePath + ".txt"))
            {
                result = gatePath;
                //Debug.Log($"Custom Regions: Found gate {roomName} in {keyValues.Key}. Path: {result}");
            }
            // room is Gate shelter
            else if (File.Exists(Custom.RootFolderDirectory() + gateShelterPath + ".txt"))
            {
                result = gateShelterPath;
                //Debug.Log($"Custom Regions: Found gate_shelter {roomName} in {keyValues.Key}. Path: {result}");
            }
            // room is Arena
            else if (File.Exists(Custom.RootFolderDirectory() + arenaPath + ".txt"))
            {
                result = arenaPath;
                //Debug.Log($"Custom Regions: Found arena {roomName} in {keyValues.Key}. Path: {result}");
            }

            // Debug.Log("Using Custom Worldfile: " + result);
            if (includeRootDirectory)
            {
                result = "file:///" + Custom.RootFolderDirectory() + result;
            }
            return result;
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
    }
}
