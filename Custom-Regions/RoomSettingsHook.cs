using CustomRegions.Mod;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CustomRegions
{
    static class RoomSettingsHook
    {

        // This code comes from EasyModPack by topicular
        // Adapted to work with any region by Garrakx


        public static void ApplyHook()
        {
            On.RoomSettings.Load += RoomSettings_Load;
        }

        /// <summary>
        /// Loads vanilla room setting if there is no custom one.
        /// </summary>
        private static void RoomSettings_Load(On.RoomSettings.orig_Load orig, RoomSettings self, int playerChar)
        {
            // CHECK IF THIS WORKS
            if (/*!enabled || */(!self.isTemplate && !File.Exists(self.filePath)))
            {
                Debug.Log($"Custom Regions: Loading room settings");
                self.filePath = CustomWorldMod.FindVanillaRoom(self.name, false) + "_Settings.txt";
            }
            orig(self, playerChar);
        }
    }
}
