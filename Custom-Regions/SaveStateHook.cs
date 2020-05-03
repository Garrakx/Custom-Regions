using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using CustomRegions.Mod;

namespace CustomRegions
{
    static class SaveStateHook
    {
        public static void ApplyHook()
        {
            On.SaveState.LoadGame += SaveState_LoadGame;
        }

        private static void SaveState_LoadGame(On.SaveState.orig_LoadGame orig, SaveState self, string str, RainWorldGame game)
        {
            // New game, generate regionsave checksum
            if (str == string.Empty)
            {
                Debug.Log(CustomWorldMod.GetSaveInformation());
            }
            // Existing game, validate regionsave checksum
            else
            {
                // Check if player playing on existing save before CR

            }

            orig(self, str, game);
        }
    }
}
