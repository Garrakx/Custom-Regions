using System;
using BepInEx;
using UnityEngine;
using System.IO;
using RWCustom;
using System.Collections.Generic;

namespace CustomRegions.Mod
{

    [BepInPlugin(PLUGIN_ID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class CustomRegionsMod : BaseUnityPlugin
    {
        public const string PLUGIN_ID = "com.rainworldgame.garrakx.crs.mod";
        public const string PLUGIN_NAME = "Custom Regions Support";
        public const string PLUGIN_VERSION = "0.10.0.1";

         
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
                CustomMenu.RegionLandscapes.ApplyHooks();
                CustomMusic.ProceduralMusicHooks.ApplyHooks();
                ArenaUnlocks.UnlockEnum.ApplyHooks();
                Progression.StoryRegionsMod.ApplyHooks();
                CustomPearls.DataPearlColors.ApplyHooks();
                RainWorldHooks.ApplyHooks();
                CustomWorld.WorldLoaderHook.ApplyHooks();
            } catch (Exception ex) {
                BepLogError("Error while applying Hooks: " + ex.ToString());
            }
            BepLog("Finished applying hooks!");
        }

        private void RainWorld_OnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            if (init) return;
            init = true;
            //OptionInterface oi = MachineConnector.GetRegisteredOI("bubbleweedsaver");
            //cfgEven = oi.config.Bind<bool>("EvenUse", true, new ConfigurableInfo("Whether to use multiple BubbleGrasses evenly or not. Either use all BubbleGrasses in divided speed(true) or use one BubbleGrass at a time(false)."));
            CreateCustomWorldLog();
            CustomLog("Mod is Initialized.");
        }

        private static void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
        {
            orig(self);
            CRSRefresh();
        }

        public static void CRSRefresh(bool forceRefresh = false)
        {
            CustomStaticCache.CheckForRefresh(forceRefresh);
            ArenaUnlocks.UnlockEnum.RefreshArenaUnlocks();
            CustomPearls.Data.Refresh();
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

            //Debug.Log(logText);

            try {
                using (StreamWriter file = new StreamWriter(Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + logFileName, true)) {
                    file.WriteLine(logText);
                }
            } catch (Exception e) {
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// Appends the provided string to the log. If log file doesn't exist, create it. Bool indicates if you want to log into exceptionlog as well
        /// </summary>
        public static void CustomLog(string logText, bool throwException)
        {
            if (throwException) {
                Debug.LogError("[CRS] " + logText);
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

        public static void CreateCustomWorldLog()
        {
            //TODO: Add Date!
            using (StreamWriter sw = File.CreateText(Custom.RootFolderDirectory() + Path.DirectorySeparatorChar.ToString() + logFileName)) {
                sw.WriteLine($"############################################\n Custom World Log {versionCR} [DEBUG LEVEL: {debugLevel}]\n");
            }
        }

        public enum DebugLevel { RELEASE, MEDIUM, FULL }

        public static DebugLevel debugLevel = DebugLevel.FULL;
        internal static string analyzingLog;
        internal static IEnumerable<object> regionPreprocessors;
    }
}
