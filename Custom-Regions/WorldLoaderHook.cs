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
            // On.WorldLoader.ctor += WorldLoader_ctor;
            On.WorldLoader.FindRoomFileDirectory += WorldLoader_FindRoomFileDirectory;
            On.WorldLoader.NextActivity += WorldLoader_NextActivity;
        }

        private static void WorldLoader_ctor(On.WorldLoader.orig_ctor orig, WorldLoader self, RainWorldGame game, int playerCharacter, bool singleRoomWorld, string worldName, Region region, RainWorldGame.SetupValues setupValues)
        {
            /*if (worldName.Length > 2)
            {
                Debug.Log($"Custom World: ERROR! splitting worldName {worldName}");
                string text2 = Regex.Split(worldName, "_")[0];

                foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
                {
                    Debug.Log($"Custom Regions: Searching room in {keyValues.Value}");
                    string path = CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar;
                    bool flag2 = false;
                    if (Directory.Exists(string.Concat(new object[]
                    {
                        Custom.RootFolderDirectory(),
                        path.Replace('/', Path.DirectorySeparatorChar),
                        Path.DirectorySeparatorChar,
                        "World",
                        Path.DirectorySeparatorChar,
                        "Regions",
                        Path.DirectorySeparatorChar,
                        text2
                    })))
                    {
                        Debug.Log($"Custom Regions: Loading world [{text2}] from [{keyValues.Value}]");
                        flag2 = true;
                        break;
                    }
                }
                worldName = text2;
            }

            Debug.Log($"Custom Regions: Loading world - Worldname [{worldName}] ");
            */

            orig(self, game, playerCharacter, singleRoomWorld, worldName, region, setupValues);
        }


        /// <summary>
        /// Returns new World path if there is any of new file is exist
        /// </summary>
        /// <returns>New World path first, then vanilla</returns>
        private static string WorldLoader_FindRoomFileDirectory(On.WorldLoader.orig_FindRoomFileDirectory orig, string roomName, bool includeRootDirectory)
        {
            //if (!enabled) { return orig(roomName, includeRootDirectory); }

            string result = "";

            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
            {
                string path = CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar;
                //Debug.Log($"Custom Regions: Finding room {roomName} in {keyValues.Key}. Path: {path}");

                string test = string.Concat(new object[]
                {
                Custom.RootFolderDirectory(),
                path.Replace('/', Path.DirectorySeparatorChar),
                "World",
                Path.DirectorySeparatorChar,
                "Regions",
                Path.DirectorySeparatorChar,
                Regex.Split(roomName, "_")[0]
                });
                if (Directory.Exists(test))
                {
                    bool file = false;
                    if (File.Exists(string.Concat(new object[]
                    {
                    test,
                    Path.DirectorySeparatorChar,
                    "Rooms",
                    Path.DirectorySeparatorChar,
                    roomName,
                    ".txt"
                    }))) { file = true; }
                    else
                    {
                        string n = Regex.Split(roomName, "_")[1].Substring(0, 1);
                        int num = char.Parse(n) - 64;
                        for (int i = 0; i < num; i++)
                        {
                            if (File.Exists(string.Concat(new object[]
                            {
                            test,
                            Path.DirectorySeparatorChar,
                            "Rooms",
                            Path.DirectorySeparatorChar,
                            roomName,
                            "_",
                            i,
                            ".png"
                            }))) { file = true; break; }
                        }
                    }


                    if (file)
                    {
                        if (includeRootDirectory)
                        {
                            result = string.Concat(new object[]
                            {
                            "file:///",
                            test,
                            Path.DirectorySeparatorChar,
                            "Rooms",
                            Path.DirectorySeparatorChar,
                            roomName
                            });
                        }
                        else
                        {
                            result = string.Concat(new object[]
                            {
                            path.Replace('/', Path.DirectorySeparatorChar),
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
                        Debug.Log($"Custom Regions: Found room {roomName} in {keyValues.Key}. Path: {result}");
                    }
                }
                // room is a GATE
                else if (Regex.Split(roomName, "_")[0] == "GATE" && File.Exists(string.Concat(new object[]
                {
                    Custom.RootFolderDirectory(),
                    path.Replace('/', Path.DirectorySeparatorChar),
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
                        result = string.Concat(new object[]
                        {
                        "file:///",
                        Custom.RootFolderDirectory(),
                        path.Replace('/', Path.DirectorySeparatorChar),
                        "World",
                        Path.DirectorySeparatorChar,
                        "Gates",
                        Path.DirectorySeparatorChar,
                        roomName
                        });
                    }
                    else
                    {
                        result = string.Concat(new object[]
                        {
                        path.Replace('/', Path.DirectorySeparatorChar),
                        "World",
                        Path.DirectorySeparatorChar,
                        "Gates",
                        Path.DirectorySeparatorChar,
                        roomName
                        });
                    }
                    Debug.Log($"Custom Regions: Found gate {roomName} in {keyValues.Key}. Path: {result}");
                }
                // Gate shelter
                else if (File.Exists(string.Concat(new object[]
                {
                    Custom.RootFolderDirectory(),
                    path.Replace('/', Path.DirectorySeparatorChar),
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
                        result = string.Concat(new object[]
                        {
                    "file:///",
                    Custom.RootFolderDirectory(),
                    path.Replace('/', Path.DirectorySeparatorChar),
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

                        result = string.Concat(new object[]
                        {
                    path.Replace('/', Path.DirectorySeparatorChar),
                    "World",
                    Path.DirectorySeparatorChar,
                    "Gates",
                    Path.DirectorySeparatorChar,
                    "Gate shelters",
                    Path.DirectorySeparatorChar,
                    roomName
                        });
                    }
                    Debug.Log($"Custom Regions: Found gate_shelter {roomName} in {keyValues.Key}. Path: {result}");
                }
            }

            if (result != "")
            {
                //Debug.Log("Using Custom Worldfile: " + result);
                return result;
            }
            else
            {
                return orig(roomName, includeRootDirectory);
            }
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
                    Debug.Log("Custom Regions: World was null, creating new lines");
                }

                //List<string >lines = new List<string>();

                foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
                {
                    Debug.Log($"Custom Regions: Reading world_{self.worldName}.txt from {keyValues.Value}");
                    string path = CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar;

                    string test = string.Concat(new object[]
                    {
                    Custom.RootFolderDirectory(),
                    path.Replace('/', Path.DirectorySeparatorChar),
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
                        Debug.Log($"Custom Regions: Found world_{self.worldName}.txt from {keyValues.Value}");
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
                                        CustomWorldMod.CustomWorldLog($"\n\nLoading resources from {keyValues.Value}");
                                        CustomWorldMod.CustomWorldLog(array[i]);
                                    }
                                    else if (array[i] == "END BAT MIGRATION BLOCKAGES")
                                    {
                                        CustomWorldMod.CustomWorldLog(array[i]);
                                        CustomWorldMod.CustomWorldLog($"### Finished loading resources from {keyValues.Value}\n\n");
                                    }
                                    else
                                    {
                                        CustomWorldMod.CustomWorldLog(array[i]);
                                    }
                                    //self.lines.Add(array[i]);
                                }
                            }
                        }
                    }
                }

                /*
                foreach(string stringLines in self.lines)
                {
                    if (stringLines == "ROOMS")
                    {
                        CustomWorldMod.CustomWorldLog($"\n\nLoading resources from Vanilla");
                        CustomWorldMod.CustomWorldLog(stringLines);
                    }
                    else if (stringLines == "END BAT MIGRATION BLOCKAGES")
                    {
                        CustomWorldMod.CustomWorldLog(stringLines);
                        CustomWorldMod.CustomWorldLog($"### Finished loading resources from Vanilla\n\n");
                    }
                    else
                    {
                        CustomWorldMod.CustomWorldLog(stringLines);
                    }
                    lines.Add(stringLines);
                }
                self.lines = lines;
                */
            }
            orig(self);
        }
    }
}
