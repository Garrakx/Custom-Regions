using System;
using BepInEx;
using UnityEngine;
using System.IO;
using RWCustom;
using System.Collections.Generic;

using System.Security;
using System.Security.Permissions;
using CustomRegions.CustomWorld;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]


namespace CustomRegions.Mod
{

    [BepInPlugin(PLUGIN_ID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class CustomRegionsMod : BaseUnityPlugin
    {
        public const string PLUGIN_ID = "com.rainworldgame.garrakx.crs.mod";
        public const string PLUGIN_NAME = "Custom Regions Support";
        public const string PLUGIN_VERSION = "0.10.5.1";
        public const string JSON_ID = "crs";

         
        private static bool init = false;
        public static CustomRegionsMod instance;

        public static BepInEx.Logging.ManualLogSource bepLog => instance.Logger;
        public static Configurable<bool> cfgEven;

        public const string logFileName = "crsLog.txt";

        public void Awake()
        {
            instance = this;

            // remove this
            //CreateCustomWorldLog(); this can't be called until Custom.RootFolderDirectory() is filled, wait for onmodsinit

            On.RainWorld.OnModsInit += RainWorld_OnModsInit;
            On.RainWorld.PostModsInit += RainWorld_PostModsInit;
            BepLog($"{PLUGIN_NAME} (v{PLUGIN_VERSION}) initialized, applying hooks...");

            try {
                IndexedEntranceClass.Apply();
                ReplaceRoomPreprocessor.Apply();
                Debugging.ApplyHooks();
                CustomMenu.RegionLandscapes.ApplyHooks();
                Arena.ChallengeMenu.ApplyHooks();
                Arena.ChallengeSupport.ApplyHooks();
                Arena.ChallengeToken.ApplyHooks();
                Arena.ChallengeTokenCache.ApplyHooks();
                Arena.ChallengeData.ApplyHooks();
                Arena.CreatureBehaviors.ApplyHooks();
                CustomMusic.ProceduralMusicHooks.ApplyHooks();
                Collectables.ArenaUnlocks.ApplyHooks();
                Progression.StoryRegionsMod.ApplyHooks();
                Collectables.PearlData.ApplyHooks();
                Collectables.CustomConvo.ApplyHooks();
                Collectables.Encryption.ApplyHooks();
                Collectables.Broadcasts.ApplyHooks();
                Arena.Properties.ApplyHooks();
                Arena.PreprocessorPatch.ApplyHooks();
                RainWorldHooks.ApplyHooks();
                WorldLoaderHook.ApplyHooks();
                RegionProperties.RegionProperties.ApplyHooks();
                RegionProperties.CycleRelatedHooks.ApplyHooks();
                RegionProperties.ScavengerHooks.ApplyHooks();
                RegionProperties.MiscHooks.ApplyHooks();
                RegionProperties.InvHooks.ApplyHooks();
            } catch (Exception ex) {
                BepLogError("Error while applying Hooks: " + ex.ToString());
            }
            BepLog("Finished applying hooks!");
        }

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            try
            {
                RemixMenu.RegisterOptionInterface();
                if (init) return;
                init = true;
                CreateCustomWorldLog();
                LoadDebugLevel();
                RegionPreprocessors.InitializeBuiltinPreprocessors();
                CustomLog("Mod is Initialized.");
            }
            catch (Exception e)
            {
                BepLogError("error in OnModsInit!\n" + e);
            }
        }

        private static void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
        {
            orig(self);
            CRSRefresh();
        }

        public static void CRSRefresh(bool forceRefresh = false)
        {
            try
            {
                CustomStaticCache.CheckForRefresh(forceRefresh);
                CustomMerge.MergeCustomFiles();
                Collectables.ArenaUnlocks.RefreshArenaUnlocks();
                Collectables.PearlData.Refresh();
                Collectables.Broadcasts.Refresh();
                Arena.ChallengeData.Refresh();
            }
            catch (Exception e) { CustomLog(e.ToString(), true); }
        }

        public static void BepLog(string message)
        {
            bepLog.LogMessage(message);
        }
        public static void BepLogError(string message)
        {
            bepLog.LogError("[CRS] " + message);
        }


        public static string versionCR {
            get => PLUGIN_VERSION;
        }

        public static void CustomLog(string logText)
        {
            if (!File.Exists(Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + logFileName)) {
                CreateCustomWorldLog();
            }

            if (debugLevel == DebugLevel.NONE) return;

            try {
                using (StreamWriter file = new StreamWriter(Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + logFileName, true)) {
                    file.WriteLine(logText);
                }
            } catch (Exception e) {
                Debug.LogError(e.ToString());
            }
        }

        /// <summary>
        /// Appends the provided string to the log. If log file doesn't exist, create it. Bool indicates if you want to log into exceptionlog as well
        /// </summary>
        public static void CustomLog(string logText, bool throwException)
        {
            if (throwException) {
                UnityEngine.Debug.LogError("[CRS] " + logText);
                logText = "[ERROR] " + logText + "\n";
            }
            CustomLog(logText);
        }

        public static void CustomLog(string logText, bool throwException, DebugLevel minDebugLevel)
        {
            if (minDebugLevel <= debugLevel) {
                CustomLog(logText, throwException);
            }
        }

        public static void CustomLog(string logText, DebugLevel minDebugLevel)
        {
            if (minDebugLevel <= debugLevel) {
                CustomLog(logText, false);
            }
        }

        private static void CreateCustomWorldLog()
        {
            using (StreamWriter sw = File.CreateText(Custom.RootFolderDirectory() + Path.DirectorySeparatorChar.ToString() + logFileName)) {
                sw.WriteLine($"############################################\n Custom World Log {versionCR} [DEBUG LEVEL: {debugLevel}]\n {DateTime.UtcNow:MM/dd/yyyy HH:mm:ss}\n");
                if (debugLevel == DebugLevel.NONE) sw.WriteLine($"CRSLog is disabled! It can be re-enabled in the Remix menu");
            }
        }

        private static void LoadDebugLevel()
        {
            string filePath = Custom.RootFolderDirectory() + Path.DirectorySeparatorChar.ToString() + "CRSDebugLevel.txt";
            if (File.Exists(filePath))
            {
                string debugString = File.ReadAllText(filePath);
                if (Enum.IsDefined(typeof(DebugLevel), debugString))
                {
                    debugLevel = (DebugLevel)Enum.Parse(typeof(DebugLevel), debugString);
                }
                else
                {
                    debugLevel = DebugLevel.FULL;
                }
                return;
            }
            SetDebugFromRemix();
        }

        public static void SetDebugFromRemix()
        {
            debugLevel = RemixMenu.DebugLevel.Value;
        }

        public enum DebugLevel { NONE, DEFAULT, MEDIUM, FULL }

        public static DebugLevel debugLevel = DebugLevel.DEFAULT;
        internal static string analyzingLog;
        internal static IEnumerable<object> regionPreprocessors;
    }
}
