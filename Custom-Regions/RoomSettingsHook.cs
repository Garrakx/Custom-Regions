using CustomRegions.Mod;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using RWCustom;

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

                    foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
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
            else
            {
                CustomWorldMod.Log($"Custom Regions: RoomSettings, room [{self.name}] is not template. FilePath [{self.filePath}]");
            }

            try
            {
                orig(self, region);
            } catch (Exception e)
            {
                CustomWorldMod.Log("Found illegal characters in a room settings file." + e);
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
                self.filePath = CustomWorldMod.FindVanillaRoom(self.name, false) + "_Settings.txt";
            }
            //}


            orig(self, playerChar);
        }
    }
}
