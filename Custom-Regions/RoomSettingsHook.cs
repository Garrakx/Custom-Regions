using CustomRegions.Mod;
using System;
using System.Collections.Generic;
using System.IO;
using RWCustom;
using System.Text.RegularExpressions;

namespace CustomRegions
{
    static class RoomSettingsHook
    {

        public static void ApplyHook()
        {
           // On.RoomSettings.Load += RoomSettings_Load;
            On.RoomSettings.FindParent += RoomSettings_FindParent;

            // Debug
            On.RoomSettings.Save += RoomSettings_Save;
        }

        private static void RoomSettings_Save(On.RoomSettings.orig_Save orig, RoomSettings self)
        {
            CustomWorldMod.Log($"Custom Regions: Saving room settings at [{self.filePath}]");
            orig(self);
        }

        private static void RoomSettings_FindParent(On.RoomSettings.orig_FindParent orig, RoomSettings self, Region region)
        {
            if (self.isTemplate)
            {
                string filePath = "World" + Path.DirectorySeparatorChar + "Regions" + Path.DirectorySeparatorChar + region.name + Path.DirectorySeparatorChar + self.name + ".txt";

                if (!File.Exists(Custom.RootFolderDirectory() + filePath))
                {
                    //CustomWorldMod.CustomWorldLog($"Custom Regions: Finding custom room settings template [{filePath}]");

                    foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
                    {
                        string newPath = Custom.RootFolderDirectory() + CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar + filePath;

                        if (File.Exists(newPath))
                        {
                            self.filePath = newPath;
                            CustomWorldMod.Log($"Custom Regions: Found settings at [{newPath}]");
                            break;
                        }

                    }

                }

            }
            /*
            else
            {
                //CustomWorldMod.Log($"Custom Regions: RoomSettings, room [{self.name}] is not template. FilePath [{self.filePath}]");
                // Calling FindRoomFileDirectory will find the custom one
                //self.filePath = FindVanillaRoom(self.name, false) + "_Settings.txt";
            }
            */

            try
            {
                orig(self, region);
            } catch (Exception e)
            {
                CustomWorldMod.Log("Found illegal characters in a room settings file." + e, true);
                throw e;
            }
        }

        /// <summary>
        /// Loads vanilla room setting if there is no custom one.
        /// </summary>
        private static void RoomSettings_Load(On.RoomSettings.orig_Load orig, RoomSettings self, int playerChar)
        {
            // CHECK IF THIS WORKS
           // if (!self.isTemplate)
           // {
                string path = WorldLoader.FindRoomFileDirectory(self.name, false) + "_Settings.txt";
                CustomWorldMod.Log($"Custom Regions: Loading room settings for [{self.name}] at [{path}]");

            if (File.Exists(path))
            {
                self.filePath = path;
            }
            else
            {
                self.filePath = FindVanillaRoom(self.name, false) + "_Settings.txt";
            }
            //}


            orig(self, playerChar);
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
                //CustomWorldMod.CustomWorldLog($"Custom Regions: Found arena {roomName} in {keyValues.Key}. Path: {result}");
            }

            // CustomWorldMod.CustomWorldLog("Using Custom Worldfile: " + result);
            if (includeRootDirectory)
            {
                result = "file:///" + Custom.RootFolderDirectory() + result;
            }
            return result;
        }
    }


}
