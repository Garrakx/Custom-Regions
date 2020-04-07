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
        public static List<string> AddNewRoom(string newRoom, List<string> oldList)
        {
            bool sameConnections = false;
            string conflictingRoom = string.Empty;
            foreach (string oldRoom in oldList)
            {
                if (oldRoom.Equals(newRoom))
                {
                    // The room is exactly the same, skipped
                    sameConnections = true;
                    break;
                }
                else
                {
                    try
                    {
                        string oldBaseRoom = oldRoom.Substring(0, oldRoom.IndexOf(" "));
                        string newBaseRoom = newRoom.Substring(0, newRoom.IndexOf(" "));
                        if (oldBaseRoom.Equals(newBaseRoom))
                        {
                            // The room is the same but different connections
                            conflictingRoom = oldRoom;
                            break;
                        }
                    }
                    catch (Exception e) { }

                }
            }

            if (conflictingRoom == string.Empty && !sameConnections)
            {
                //Debug.Log($"Custom Regions: Added new room [{newRoom}]");
                oldList.Add(newRoom);
            }
            else
            {
                //Debug.Log($"Custom Regions: Found conflict [{newRoom}]");
                if (conflictingRoom != string.Empty)
                {
                    //Debug.Log($"Custom Regions: Trying to merge {conflictingRoom}");

                    // Check if containts GATE/SHELTER/SWARMROOM at the end
                    string endingSetting = string.Empty;
                    string temp = conflictingRoom.Substring(conflictingRoom.IndexOf(": ") + 2);
                    if (temp.IndexOf(": ") > 0)
                    {
                        endingSetting = temp.Substring(temp.IndexOf(": ") + 2);
                    }

                    List<string> oldConnections = FromConnectionsToList(conflictingRoom);
                    List<string> newConnections = FromConnectionsToList(newRoom);

                    // Build new connections
                    for (int i = 0; i < oldConnections.Count; i++)
                    {
                        for (int j = 0; j < newConnections.Count; j++)
                        {
                            if (!oldConnections[i].Equals(newConnections[j]))
                            {
                                if ((oldConnections[i].Equals("DISCONNECTED") || (oldConnections[i].Equals("DISCONNECT"))) && !(newConnections[j].Equals("DISCONNECTED") || newConnections[j].Equals("DISCONNECT")))
                                {
                                    oldConnections[i] = newConnections[j];
                                    //Debug.Log($"Custom Regions: Added [{newConnections[j]}] to [{conflictingRoom}]");
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

                    endingSetting = endingSetting != string.Empty ? ": " + endingSetting : "";

                    // Convert from list to connections
                    string updatedConnections = FromListToConnections(conflictingRoom.Substring(0, conflictingRoom.IndexOf(" ")), oldConnections);
                    int index = oldList.IndexOf(conflictingRoom);
                    if (index != -1)
                    {
                        Debug.Log($"Custom Regions: Replaced [{oldList[index]}] with [{updatedConnections + endingSetting}]");
                        oldList[index] = updatedConnections + endingSetting;
                    }
                }
            }
            return oldList;
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
        /// Path of the CustomResources folder
        /// </summary>
        public const string resourcePath = "Mods\\CustomResources\\";

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

                string regionIDpath = region + "\\regionID.txt";
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
            string text = string.Concat(new object[]
            {
                Custom.RootFolderDirectory(),
                "World",
                Path.DirectorySeparatorChar,
                "Regions",
                Path.DirectorySeparatorChar,
                Regex.Split(roomName, "_")[0]
            });
            if (Directory.Exists(text) && File.Exists(string.Concat(new object[]
                {
                text,
                Path.DirectorySeparatorChar,
                "Rooms",
                Path.DirectorySeparatorChar,
                roomName,
                ".txt"
                })))
            {
                if (includeRootDirectory)
                {
                    return string.Concat(new object[]
                    {
                    "file:///",
                    text,
                    Path.DirectorySeparatorChar,
                    "Rooms",
                    Path.DirectorySeparatorChar,
                    roomName
                    });
                }
                return string.Concat(new object[]
                {
                "World",
                Path.DirectorySeparatorChar,
                "Regions",
                Path.DirectorySeparatorChar,
                Regex.Split(roomName, "_")[0],
                Path.DirectorySeparatorChar,
                "Rooms",
                Path.DirectorySeparatorChar,
                roomName
                });
            }
            else if (Regex.Split(roomName, "_")[0] == "GATE" && File.Exists(string.Concat(new object[]
            {
            Custom.RootFolderDirectory(),
            "World",
            Path.DirectorySeparatorChar,
            "Gates",
            Path.DirectorySeparatorChar,
            roomName,
            ".txt"
            })))
            {
                if (includeRootDirectory)
                {
                    return string.Concat(new object[]
                    {
                    "file:///",
                    Custom.RootFolderDirectory(),
                    "World",
                    Path.DirectorySeparatorChar,
                    "Gates",
                    Path.DirectorySeparatorChar,
                    roomName
                    });
                }
                return string.Concat(new object[]
                {
                "World",
                Path.DirectorySeparatorChar,
                "Gates",
                Path.DirectorySeparatorChar,
                roomName
                });
            }
            else if (File.Exists(string.Concat(new object[]
            {
            Custom.RootFolderDirectory(),
            "World",
            Path.DirectorySeparatorChar,
            "Gates",
            Path.DirectorySeparatorChar,
            "Gate shelters",
            Path.DirectorySeparatorChar,
            roomName,
            ".txt"
            })))
            {
                if (includeRootDirectory)
                {
                    return string.Concat(new object[]
                    {
                    "file:///",
                    Custom.RootFolderDirectory(),
                    "World",
                    Path.DirectorySeparatorChar,
                    "Gates",
                    Path.DirectorySeparatorChar,
                    "Gate shelters",
                    Path.DirectorySeparatorChar,
                    roomName
                    });
                }
                return string.Concat(new object[]
                {
                "World",
                Path.DirectorySeparatorChar,
                "Gates",
                Path.DirectorySeparatorChar,
                "Gate shelters",
                Path.DirectorySeparatorChar,
                roomName
                });
            }
            else
            {
                if (includeRootDirectory)
                {
                    return string.Concat(new object[]
                    {
                    "file:///",
                    Custom.RootFolderDirectory(),
                    "Levels",
                    Path.DirectorySeparatorChar,
                    roomName
                    });
                }
                return "Levels" + Path.DirectorySeparatorChar + roomName;
            }
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
