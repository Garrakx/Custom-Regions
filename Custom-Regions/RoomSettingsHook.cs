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

        public static void ApplyHooks()
        {
            On.RoomSettings.LoadPlacedObjects += RoomSettings_LoadPlacedObjects;

            On.RoomSettings.FindParent += RoomSettings_FindParent;
            On.RoomSettings.Save += RoomSettings_Save;
            On.RoomSettings.Reset += RoomSettings_Reset;
            On.RoomSettings.ctor += RoomSettings_ctor;
        }

        private static string regionName = null;
        private static void RoomSettings_ctor(On.RoomSettings.orig_ctor orig, 
            RoomSettings self, string name, Region region, bool template, bool firstTemplate, int playerChar)
        {
            if (region != null) { regionName = region.name; }
            orig(self, name, region, template, firstTemplate, playerChar);
        }

        private static void RoomSettings_Reset(On.RoomSettings.orig_Reset orig, RoomSettings self)
        {
            if (regionName == null)
            {
                // Reset() was called not from RoomSettings ctor
                orig(self);
                return;
            }

            if (self.isTemplate)
            {
                string filePath = "World" + Path.DirectorySeparatorChar + "Regions" + Path.DirectorySeparatorChar + regionName +
                    Path.DirectorySeparatorChar + self.name + ".txt";

                bool foundTemplate = false;
                foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
                {
                    string newPath = Custom.RootFolderDirectory() + 
                        CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar + filePath;
                    CustomWorldMod.Log($"[RoomSettings] Finding custom room settings template [{newPath}]", false, CustomWorldMod.DebugLevel.FULL);

                    if (File.Exists(newPath))
                    {
                        foundTemplate = true;
                        self.filePath = newPath;
                        CustomWorldMod.Log($"[RoomSettings] Found template at [{newPath}]");
                        break;
                    }

                }

                if (!foundTemplate && File.Exists(Custom.RootFolderDirectory() + filePath))
                {
                    self.filePath = Custom.RootFolderDirectory() + filePath;
                    CustomWorldMod.Log($"[RoomSettings] Using vanilla template at [{self.filePath}] since custom was not found", false, CustomWorldMod.DebugLevel.FULL);
                }


            }
            else
            {

                // Mod didn't include Settings file
                if (!File.Exists(self.filePath))
                {
                    CustomWorldMod.Log($"[RoomSettings] Room is not template, but could not find settings file at " +
                        $"[{self.filePath}]", false, CustomWorldMod.DebugLevel.FULL);

                    string regularRoomPath = Custom.RootFolderDirectory() +
                        "World" + Path.DirectorySeparatorChar + "Regions" + Path.DirectorySeparatorChar + Regex.Split(self.name, "_")[0];

                    regularRoomPath += Path.DirectorySeparatorChar + "Rooms" + Path.DirectorySeparatorChar + self.name + "_Settings.txt";
                    if (File.Exists(regularRoomPath))
                    {
                        self.filePath = regularRoomPath;
                    }
                    else
                    {
                        CustomWorldMod.Log($"[RoomSettings] Missing settings file for [{self.name}] - [{regularRoomPath}]", false, CustomWorldMod.DebugLevel.FULL);
                    }
                }
            }
            regionName = null;
            orig(self);
        }

        private static void RoomSettings_LoadPlacedObjects(On.RoomSettings.orig_LoadPlacedObjects orig, RoomSettings self, string[] s, int playerChar)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            orig(self, s, playerChar);
            watch.Stop();
            DateTime date = new DateTime(watch.ElapsedTicks);
            CustomWorldMod.Log($"[RoomSettings] Loading placedObjects [{self.name}]. Elapsed time [{date.ToString("s.ffff")}s]");
        }

        private static void RoomSettings_Save(On.RoomSettings.orig_Save orig, RoomSettings self)
        {
            CustomWorldMod.Log($"[RoomSettings] Custom Regions: Saving room settings at [{self.filePath}]");
            orig(self);
        }

        private static void RoomSettings_FindParent(On.RoomSettings.orig_FindParent orig, RoomSettings self, Region region)
        {
            CustomWorldMod.Log($"[RoomSettings] Finding parent... filePath [{self.filePath}]", false, CustomWorldMod.DebugLevel.FULL);

            try
            {
                orig(self, region);
            }
            catch (Exception e)
            {
                CustomWorldMod.Log($"[RoomSettings] Found illegal characters in a room settings file [{self.name}]. \nException: {e}", true);
                throw e;
            }
        }


        /// <summary>
        /// Returns vanilla world file, or other CustomWorld mod file if there is one
        /// </summary>
        /// <returns>Vanilla World path</returns>
        public static string FindVanillaRoom(string roomName, bool includeRootDirectory)
        {
            string result = "";

            string gatePath = Custom.RootFolderDirectory() + 
                "World" + Path.DirectorySeparatorChar + "Gates" + Path.DirectorySeparatorChar + roomName;

            string gateShelterPath = Custom.RootFolderDirectory() + 
                "World" + Path.DirectorySeparatorChar + "Gates" + Path.DirectorySeparatorChar + "Gate shelters" + Path.DirectorySeparatorChar + roomName;

            string regularRoomPath = Custom.RootFolderDirectory() + 
                "World" + Path.DirectorySeparatorChar + "Regions" + Path.DirectorySeparatorChar + Regex.Split(roomName, "_")[0];

            string arenaPath = Custom.RootFolderDirectory() + "Levels" + Path.DirectorySeparatorChar + roomName;

            // room is regular room
            if (Directory.Exists(regularRoomPath) && 
                File.Exists(regularRoomPath + Path.DirectorySeparatorChar + "Rooms" + Path.DirectorySeparatorChar + roomName + ".txt"))
            {
                result = Custom.RootFolderDirectory() + 
                    "World" + Path.DirectorySeparatorChar + "Regions" + Path.DirectorySeparatorChar + Regex.Split(roomName, "_")[0] + 
                    Path.DirectorySeparatorChar + "Rooms" + Path.DirectorySeparatorChar + roomName;
                CustomWorldMod.Log($"Custom Regions: Found room {roomName}. Path: {result}", false, CustomWorldMod.DebugLevel.FULL);
            }
            // room is GATE
            else if (Regex.Split(roomName, "_")[0] == "GATE" && File.Exists(Custom.RootFolderDirectory() + gatePath + ".txt"))
            {
                result = gatePath;
                CustomWorldMod.Log($"Custom Regions: Found gate {roomName}. Path: {result}", false, CustomWorldMod.DebugLevel.FULL);
            }
            // room is Gate shelter
            else if (File.Exists(Custom.RootFolderDirectory() + gateShelterPath + ".txt"))
            {
                result = gateShelterPath;
                CustomWorldMod.Log($"Custom Regions: Found gate_shelter {roomName}. Path: {result}", false, CustomWorldMod.DebugLevel.FULL);
            }
            // room is Arena
            else if (File.Exists(Custom.RootFolderDirectory() + arenaPath + ".txt"))
            {
                result = arenaPath;
                CustomWorldMod.Log($"Custom Regions: Found arena {roomName}. Path: {result}", false, CustomWorldMod.DebugLevel.FULL);
            }

            if (includeRootDirectory)
            {
                result = "file:///" + Custom.RootFolderDirectory() + result;
            }
            return result;
        }
    }


}
