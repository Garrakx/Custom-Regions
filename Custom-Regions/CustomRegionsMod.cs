using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using UnityEngine;
using System.IO;
using RWCustom;

using System.Security;
using System.Security.Permissions;
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace CustomRegionsMod
{
    [BepInPlugin("bro.crs", "Custom Regions Support", "0.1.0")]
    public class CustomRegionsMod : BaseUnityPlugin
    {
        public void OnEnable()
        {
            Logger.LogInfo("CRS Running");
            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
        }

        public static string versionCR
        {
        get =>  $"v0.1.0";
        }

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            Progression.StoryRegionsMod.ApplyHooks();
            Menu.RegionLandscapes.ApplyHooks();
            CMusic.ProceduralMusicHooks.ApplyHooks();
            ArenaUnlocks.UnlockEnum.ApplyHooks();
            CustomRegionsMod.Log("hooks done");
        }

        

        public static void Log(string logText)
        {
            if (!File.Exists(Custom.RootFolderDirectory() + "customWorldLog.txt"))
            {
                CreateCustomWorldLog();
            }

            Debug.Log(logText);

            try
            {
                using (StreamWriter file = new StreamWriter(Custom.RootFolderDirectory() + Path.DirectorySeparatorChar.ToString() + "customWorldLog.txt", true))
                {
                    file.WriteLine(logText);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// Appends the provided string to the log. If log file doesn't exist, create it. Bool indicates if you want to log into exceptionlog as well
        /// </summary>
        public static void Log(string logText, bool throwException)
        {
            if (throwException)
            {
                Debug.LogError("[CustomRegions] " + logText);
                logText = "[ERROR] " + logText + "\n";
            }
            Log(logText);
        }

        public static void Log(string logText, bool throwException, DebugLevel minDebugLevel)
        {
            if (minDebugLevel <= CustomRegionsMod.debugLevel)
            {
                Log(logText, throwException);
            }
        }
        public static void CreateCustomWorldLog()
        {
            using (StreamWriter sw = File.CreateText(Custom.RootFolderDirectory() + Path.DirectorySeparatorChar.ToString() + "customWorldLog.txt"))
            {
                sw.WriteLine($"############################################\n Custom World Log {versionCR} [DEBUG LEVEL: {CustomRegionsMod.debugLevel}]\n");
            }
        }

        public enum DebugLevel { RELEASE, MEDIUM, FULL }

        public static DebugLevel debugLevel = DebugLevel.FULL;

    }
}
