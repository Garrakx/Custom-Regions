using CustomRegions.Mod;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace CustomRegions.ArenaUnlocks
{
    internal static class UnlockEnum
    {
        public static void ApplyHooks()
        {
            On.MultiplayerUnlocks.LevelLockID += MultiplayerUnlocks_LevelLockID;
        }

        private static MultiplayerUnlocks.LevelUnlockID MultiplayerUnlocks_LevelLockID(On.MultiplayerUnlocks.orig_LevelLockID orig, string levelName)
        {
            if (customLevelUnlocks.ContainsKey(levelName.ToLower())) {
                try {
                    MultiplayerUnlocks.LevelUnlockID unlockID = customLevelUnlocks[levelName];
                    CustomRegionsMod.CustomLog($"found custom arena unlock [{levelName}] [{unlockID}]");
                    return unlockID;

                } catch (Exception e) {
                    CustomRegionsMod.CustomLog($"Error parsing levelUnlockID enum [{levelName}] - [{e}]", true);
                }
            }

            return orig(levelName);
        }

        static Dictionary<string, MultiplayerUnlocks.LevelUnlockID> customLevelUnlocks = new Dictionary<string, MultiplayerUnlocks.LevelUnlockID>();


        public static void RefreshArenaUnlocks()
        {
            UnregisterArenaUnlocks();
            RegisterArenaUnlocks();
        }

        public static void UnregisterArenaUnlocks()
        {
            try {
                foreach (KeyValuePair<string, MultiplayerUnlocks.LevelUnlockID> unlock in customLevelUnlocks) 
                { unlock.Value?.Unregister(); }

                customLevelUnlocks = new Dictionary<string, MultiplayerUnlocks.LevelUnlockID>();
            } catch (Exception e) { throw e; }
        }

        public static void RegisterArenaUnlocks()
        {
            foreach (string line in File.ReadAllLines(AssetManager.ResolveFilePath("CustomUnlocks.txt")))
            {
                if (line.Equals(string.Empty))
                {
                    // Line empty, skip
                    continue;
                }
                string[] lineDivided = Regex.Split(line, " : ");
                MultiplayerUnlocks.LevelUnlockID unlockID;
                string[] levelNames;

                try
                {
                    if (ExtEnumBase.TryParse(typeof(MultiplayerUnlocks.LevelUnlockID), lineDivided[0], false, out ExtEnumBase result))
                    {
                        unlockID = (MultiplayerUnlocks.LevelUnlockID)result;
                    }
                    else
                    {
                        unlockID = new MultiplayerUnlocks.LevelUnlockID(lineDivided[0], true);
                    }
                }
                catch (Exception e)
                {
                    CustomRegionsMod.CustomLog("Error loading levelUnlock ID" + e, true);
                    continue;
                }

                try
                {
                    levelNames = Regex.Split(lineDivided[1].Replace(" ", ""), ",");
                }
                catch (Exception e)
                {
                    CustomRegionsMod.CustomLog("Error loading levelUnlock name" + e, true);
                    continue;
                }

                for (int j = 0; j < levelNames.Length; j++)
                {
                    if (levelNames[j].Equals(string.Empty))
                    {
                        continue;
                    }

                    try
                    {

                        if (!customLevelUnlocks.ContainsKey(levelNames[j]))
                        {
                            customLevelUnlocks.Add(levelNames[j].ToLower(), unlockID);
                            CustomRegionsMod.CustomLog($"Added new level unlock: [{levelNames[j]}-{unlockID}]");
                        }
                        else
                        {
                            CustomRegionsMod.CustomLog($"Duplicated arena name from two packs! [{levelNames[j]}]", true);
                        }
                    }
                    catch (Exception e)
                    {
                        CustomRegionsMod.CustomLog($"Error adding level unlock ID [{levelNames[j]}] [{e}]", true);
                    }
                }
            }
        }
    }
}
