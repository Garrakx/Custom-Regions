using Partiality.Modloader;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

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

            loadedRegions = CustomWorldMod.BuildModRegionsDictionary();
            string dictionaryString = "Custom Regions: Loading \n{";
            foreach (KeyValuePair<string, string> keyValues in loadedRegions)
            {
                dictionaryString += keyValues.Key + " : " + keyValues.Value + ", ";
            }
            Debug.Log(dictionaryString.TrimEnd(',', ' ') + "}");


            script.Initialize();

        }

        public struct CustomWorldConfig
        {

        }


        public enum MergeStatus
        {
            ROOMS,
            CREATURES,
            BATS
        }


        /// <summary>
        /// Returns the vanilla regions ID. UNUSED
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
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
            {
                //Debug.Log($"Custom Regions: Loading custom properties for {keyValues.Key}");
                string path = CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar;

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
        /// Where new World Folder is located
        /// </summary>
        public const string resourcePath = "Mods/CustomResources/";

        /// <summary>
        /// Whether you enable loading your world or not
        /// </summary>
        //public static bool enabled = BallanceMod.option.customWorld;


        /// <summary>
        /// Builds a dictionary where the Key is the region ID and the value is the name.
        /// </summary>
        /// <returns>returns a Dictionary(string, string)</returns>
        private static Dictionary<string, string> BuildModRegionsDictionary()
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
                }

                regionID = regionID.ToUpper();
                dictionary.Add(regionID, Path.GetFileNameWithoutExtension(region));
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

    }
}
