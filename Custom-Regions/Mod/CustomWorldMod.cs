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
            Version = "0.4." + version;
            author = "Garrakx";
        }

        // Code for AutoUpdate support
        // Should be put in the main PartialityMod class.

        // Update URL - don't touch!
        public string updateURL = "http://beestuff.pythonanywhere.com/audb/api/mods/3/0";
        public int version = 15;

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

        public static CustomWorldOption LoadOI()
        {
            return new CustomWorldOption();
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

        /// <summary>
        /// Struct with information of available regions 
        /// [regionID, regionName, description, activated, checksum, loadOrder(Default is 100)]
        /// </summary>
        public struct RegionInformation
        {
            public string regionID;
            public string regionName;
            public string description;
            public bool activated;
            public string checksum;
            public int loadOrder;

            public RegionInformation(string regionID, string regionName, string description, bool activated, int loadOrder, string checksum)
            {
                this.regionID = regionID;
                this.regionName = regionName;
                this.description = description;
                this.activated = activated;
                this.checksum = checksum;
                this.loadOrder = loadOrder;
            }
        }

        /// <summary>
        /// Struct with information of world lines, used in region merging and loading.
        /// [Data: holds the line itself, Vanilla: comes from vanilla or is it modified, modID: last mod which loaded or modified the line (empty if vanilla)]
        /// </summary>
        public struct WorldDataLine
        {
            public string data;
            public bool vanilla;
            public string modID;
            public WorldDataLine(string data, bool vanilla)
            {
                this.data = data;
                this.vanilla = vanilla;
                this.modID = string.Empty;
            }
            public WorldDataLine(string data, bool vanilla, string modID)
            {
                this.data = data;
                this.vanilla = vanilla;
                this.modID = modID;
            }
        }

        /// <summary>
        /// Struct with information of creature lines, used in region merging and loading.
        /// </summary>
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

                        //Debug.Log($"Custom Regions: Found conflict [{roomConnectionsToBeReplaced}] with [{newRoom}]");
                    }
                }
            }

            // Found a connection that needs to be replaced or merged
            if (roomConnectionsToBeReplaced != string.Empty)
            {

                //Debug.Log($"Custom Regions: Trying to merge {roomConnectionsToBeReplaced}");

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
                        Debug.Log($"Custom Regions: ERROR! Added compability to [{newRoom}]! It has less connections than vanilla! [{roomConnectionsToBeReplaced}]");
                        Debug.Log($"Custom Regions: Converted to [{string.Join(", ", newConnections.ToArray())}]");
                    }

                    // The mod will replace the old lines since they are vanilla.
                    isRoomBeingReplaced = true;
                }

                else // Only merge if it is between to mods
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
                                    //Debug.Log($"Custom Regions: Replaced disconnected [{oldConnections[i]}] with [{newConnections[j]}]");
                                    oldConnections[i] = newConnections[j];
                                    noDisconnectedPipes = false;
                                    performedOperation = true;
                                    break;
                                }
                                // If the room is Vanilla, mod can replace pipes with DISCONNECTED
                                /*else if (isVanilla && newConnectionDisconnected)
                                {
                                    Debug.Log($"Custom Regions: Replaced vanilla [{oldConnections[i]}] with disconnected [{newConnections[j]}]");
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

                Debug.Log($"Custom Regions: Analized room [{roomConnectionsToBeReplaced}]. Vanilla [{isVanilla}]. NewRoomConnections [{string.Join(", ", newConnections.ToArray())}]. IsBeingReplaced [{isRoomBeingReplaced}]. No Empty Pipes [{noDisconnectedPipes}]");

                // No empty pipes but room needs to be replaced. Whole line will be replaced
                if (isRoomBeingReplaced && noDisconnectedPipes)
                {
                    Debug.Log($"Custom Regions: Comparing two rooms without disconnected pipes. [{roomConnectionsToBeReplaced}] is vanilla: [{isVanilla}]. with [{string.Join(", ", newConnections.ToArray())}]");
                    if (/*newConnections.Count > oldConnections.Count ||*/ isVanilla)
                    {
                        oldConnections = newConnections;
                        performedOperation = true;
                    }
                    else
                    {
                        Debug.Log($"Custom Regions: ERROR! Found incompatible room [{roomToBeReplacedName} : {string.Join(", ", newConnections.ToArray())}] from [{modID}] and [{roomConnectionsToBeReplaced}] from [{oldList.Find(x => x.data.Equals(roomConnectionsToBeReplaced)).modID}]");
                    }

                    /* if (oldConnections.Contains(roomConnectionsToBeReplaced))
                     {
                         Debug.Log($"Custom Regions: Connections has conflict still. [{string.Join(", ", oldConnections.ToArray())}]");
                     }*/
                }

                // A merging / replacement got place, so add changes to world lines.
                if (isRoomBeingReplaced || !noDisconnectedPipes || performedOperation)
                {
                    endingSetting = endingSetting != string.Empty ? " : " + endingSetting : "";

                    // Convert from list to connections
                    string updatedConnections = FromListToConnections(roomConnectionsToBeReplaced.Substring(0, roomConnectionsToBeReplaced.IndexOf(" ")), oldConnections);

                    int index = oldList.IndexOf(oldList.Find(x => x.data.Equals(roomConnectionsToBeReplaced)));
                    if (index != -1)
                    {
                        if (oldList[index].data != (updatedConnections + endingSetting))
                        {
                            Debug.Log($"Custom Regions: Replaced [{oldList[index].data}] with [{updatedConnections + endingSetting}]");
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
                    Debug.Log($"Custom Regions: Added new room [{newRoom}]");
                    oldList.Add(new WorldDataLine(newRoom, false));
                }
            }

            return oldList;
        }

        /// <summary>
        /// Returns a List from WorldData
        /// </summary>
        public static List<string> fromWorldDataToList(List<WorldDataLine> worldData)
        {
            List<string> updatedList = new List<string>();
            foreach (WorldDataLine data in worldData)
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
                Debug.Log($"Custom Regions: Added new blockage [{newBlockage}]");
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
            // Debug.Log($"Custom Regions: Trying to split [{oldConnections}]");
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

            //Debug.Log($"Custom Regions: FromConnectionToList [{string.Join(",", connections.ToArray())}]");

            return connections;
        }


        internal static List<WorldDataLine> AddNewCreature(string newCreatureLine, List<WorldDataLine> oldCreaturesSpawns)
        {
            bool sameCreatureLine = false;
            string creatureLineBeReplaced = string.Empty;


            bool lineage = false;
            string roomNameNewLine = string.Empty;

            // Debug.Log($"Custom Regions: Adding new creature spawn [{newCreatureLine}]]");

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

                    if (oldSpawnLine.data.Contains(newCreatureLine))
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
                                        Debug.Log($"Custom Regions: Empty pipe, filling with [{newLines.connectedDens[i]}]"); ;
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

        /// <summary>
        /// Create CreatureLine struct from string line. Used in Creature merging
        /// </summary>
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
        /// Dictionary where the Key is the region ID and the value is the name.
        /// </summary>
        public static Dictionary<string, string> loadedRegions;

        /// <summary>
        /// Dictionary where the Key is the region ID and the value is a struct with its information.
        /// </summary>
        public static Dictionary<string, RegionInformation> availableRegions;

        /// <summary>
        /// path of the CustomResources folder (Mods\CustomResources\)
        /// </summary>
        public static string resourcePath = "Mods" + Path.DirectorySeparatorChar + "CustomResources" + Path.DirectorySeparatorChar;

        /// <summary>
        /// Builds a dictionary where the Key is the region ID and the value is the region name.
        /// </summary>
        /// <returns>returns a Dictionary(string, string)</returns>
        public static Dictionary<string, string> BuildModRegionsDictionary()
        {
            // Only load activate regions from CustomWorldMod.availableRegions
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (KeyValuePair<string, RegionInformation> entry in CustomWorldMod.availableRegions)
            {
                try
                {
                    if (entry.Value.activated)
                    {
                        dictionary.Add(entry.Value.regionID, entry.Value.regionName);
                    }
                }
                catch (Exception e) { Debug.Log($"Custom Regions: Error while trying to add customRegion: {e}"); }
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
            if (!Directory.Exists(Custom.RootFolderDirectory() + resourcePath))
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

        public static object GetRegionInfoJson(string key, Dictionary<string, object> dictionary)
        {
            object value = null;
            if ((dictionary.ContainsKey(key)))
            {
                value = dictionary[key];
            }
            return value;
        }

        public static void LoadAvailableRegions()
        {
            CustomWorldMod.availableRegions = new Dictionary<string, CustomWorldMod.RegionInformation>();

            string path = Custom.RootFolderDirectory() + CustomWorldMod.resourcePath;
            Dictionary<string, RegionInformation> notSortedDictionary = new Dictionary<string, RegionInformation>();

            // For each Region Mod Installed
            foreach (string dir in Directory.GetDirectories(path))
            {
                string pathOfRegionInfo = dir + Path.DirectorySeparatorChar + "regionInfo.json";

                string regionID = string.Empty;
                string regionName = string.Empty;
                string description = "N / A";
                bool activated = false;
                string checksum = string.Empty;
                int loadOrder = 100;

                // File does not exist, generate regionInfo.json
                if (!File.Exists(pathOfRegionInfo))
                {
                    // Region Name
                    regionName = Path.GetFileNameWithoutExtension(dir);

                    // If upgrading from old CR version
                    if (File.Exists(dir + Path.DirectorySeparatorChar + "regionID.txt"))
                    {
                        regionID = File.ReadAllText(dir + Path.DirectorySeparatorChar + "regionID.txt");
                        regionID = regionID.ToUpper();

                        activated = true;

                        File.Delete(dir + Path.DirectorySeparatorChar + "regionID.txt");
                        Debug.Log($"Custom Regions: Updating regionID from old CR version... Obtained regionID [{regionID}]");
                    }

                    // regionID.txt did not exist or was empty
                    if (regionID == string.Empty)
                    {
                        string regionsPath = dir + Path.DirectorySeparatorChar + "World" + Path.DirectorySeparatorChar + "Regions";
                        Debug.Log($"Custom Regions: Empty regionID, obtaining from [{regionsPath}]. Valid [{Directory.Exists(regionsPath)}]");

                        if (Directory.Exists(regionsPath))
                        {
                            // Try to get regionID
                            foreach (string regionsDir in Directory.GetDirectories(regionsPath))
                            {
                                regionID = Path.GetFileNameWithoutExtension(regionsDir);
                                foreach (string vanillaRegion in CustomWorldMod.VanillaRegions())
                                {
                                    //Debug.Log($"Custom Regions: Comparing [{regionID}] with [{vanillaRegion}]");
                                    if (regionsDir.Contains(vanillaRegion))
                                    {
                                        regionID = string.Empty;
                                        break;
                                    }
                                }

                                if (regionID != string.Empty)
                                {
                                    break;
                                }
                            }

                        }

                        if (regionID == string.Empty)
                        {
                            // If a customRegion does not add new regions, obtain regionID from capital letters.
                            foreach (char letters in regionName)
                            {
                                if (char.IsUpper(letters))
                                {
                                    regionID += letters;
                                }
                            }
                        }
                    }

                    // Create a file to write to.
                    using (StreamWriter sw = File.CreateText(dir + Path.DirectorySeparatorChar + "regionInfo.json"))
                    {
                        sw.WriteLine("{\n"
                            + " \"regionID\":  \"" + regionID + "\", \n"
                            + " \"description\":  \"" + description + "\", \n"

                            + " \"regionName\":  \"" + regionName + "\", \n"
                            + " \"activated\":  " + activated.ToString().ToLower() + ", \n"
                            + " \"loadOrder\": " + loadOrder + ", \n"

                            // Checksum unused at the moment
                            + " \"checksum\":  \"" + checksum + "\" \n"
                            + "}");
                    }

                    Debug.Log($"Custom Regions: Adding available region [{regionID}]");
                    try
                    {
                        notSortedDictionary.Add(regionID, new CustomWorldMod.RegionInformation(regionID, regionName, description, activated, loadOrder, checksum));
                    }
                    catch (Exception dic) { Debug.Log($"Custom Regions: Error in adding [{regionID}] => {dic}"); };

                }
                // Read JSON file
                else
                {
                    Dictionary<string, object> dictionary = File.ReadAllText(pathOfRegionInfo).dictionaryFromJson();
                    RegionInformation regionInformation = new RegionInformation(string.Empty, string.Empty, "N / A", true, loadOrder, string.Empty);

                    if (dictionary != null)
                    {
                        if (GetRegionInfoJson("regionID", dictionary) != null)
                        {
                            regionInformation.regionID = (string)GetRegionInfoJson("regionID", dictionary);
                        }

                        if (GetRegionInfoJson("description", dictionary) != null)
                        {
                            regionInformation.description = (string)GetRegionInfoJson("description", dictionary);
                        }

                        if (GetRegionInfoJson("regionName", dictionary) != null)
                        {
                            regionInformation.regionName = (string)GetRegionInfoJson("regionName", dictionary);
                        }

                        if (dictionary.ContainsKey("activated"))
                        {
                            regionInformation.activated = dictionary["activated"].ToString().ToLower().Contains("true");
                        }

                        if (GetRegionInfoJson("loadOrder", dictionary) != null)
                        {
                            regionInformation.loadOrder = int.Parse(GetRegionInfoJson("loadOrder", dictionary).ToString());
                        }

                        if (GetRegionInfoJson("checksum", dictionary) != null)
                        {
                            regionInformation.checksum = (string)GetRegionInfoJson("checksum", dictionary);
                        }

                        Debug.Log($"Custom Regions: Adding available region [{regionInformation.regionID}]");
                        if (regionInformation.regionID != string.Empty)
                        {
                            try
                            {
                                notSortedDictionary.Add(regionInformation.regionID, regionInformation);
                            }
                            catch (Exception dic) { Debug.Log($"Custom Regions: Error in adding [{regionInformation.regionID}] => {dic}"); };
                        }
                    }

                }
            }

            // Save each world data line in alphabetical order
            foreach (KeyValuePair<string, RegionInformation> element in notSortedDictionary.OrderBy(d => d.Value.loadOrder))
            {
                CustomWorldMod.availableRegions.Add(element.Key, element.Value);
            }

            //CustomWorldMod.availableRegions = sortedAvailableRegions.OrderBy(d => d.Value.loadOrder);
        }

        public static string GetSaveInformation()
        {
            string dictionaryString = "Custom Regions: New save, Custom Regions Information \n{";
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
            {
                dictionaryString += $"{keyValues.Key} : {keyValues.Value}. LoadOrder[{CustomWorldMod.availableRegions[keyValues.Key].loadOrder}] Checksum [{CustomWorldMod.availableRegions[keyValues.Key].checksum}], ";
            }
            dictionaryString = dictionaryString.TrimEnd(',', ' ') + "}";

            return dictionaryString;
        }
    }
}
