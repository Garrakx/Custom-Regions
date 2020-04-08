using CustomRegions.Mod;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CustomRegions
{
    static class SandboxEditor
    {
        public static void ApplyHook()
        {
            On.ArenaBehaviors.SandboxEditor.FilePath += SandboxEditor_FilePath;
        }

        private static string SandboxEditor_FilePath(On.ArenaBehaviors.SandboxEditor.orig_FilePath orig, string room)
        {
            string vanillaFilePath = Custom.RootFolderDirectory() + "UserData" + Path.DirectorySeparatorChar + "Sandbox" + Path.DirectorySeparatorChar + MultiplayerUnlocks.LevelDisplayName(room) + "_Sandbox.txt";
            string vanillaDirectoryPath = Custom.RootFolderDirectory() + "UserData" + Path.DirectorySeparatorChar + "Sandbox";
            if(!Directory.Exists(vanillaDirectoryPath))
            {
                Directory.CreateDirectory(vanillaDirectoryPath);
                Debug.Log("Avoided crash because Sandbox folder was not created");
            }

            return orig(room);
        }
    }
}
