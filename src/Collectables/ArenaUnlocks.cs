using BepInEx;
using CustomRegions.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace CustomRegions.Collectables
{
    internal static class ArenaUnlocks
    {
        public static void ApplyHooks()
        {
            On.MultiplayerUnlocks.LevelLockID += MultiplayerUnlocks_LevelLockID;
            On.MultiplayerUnlocks.LevelDisplayName += MultiplayerUnlocks_LevelDisplayName;
            IL.MultiplayerUnlocks.ctor += MultiplayerUnlocks_ctor;
        }

        private static void MultiplayerUnlocks_ctor(MonoMod.Cil.ILContext il)
        {
            var c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.Before, 
                    x => x.MatchLdsfld(out _),
                    x => x.MatchCallvirt<ExtEnumType>("get_Count"),
                    x => x.MatchStloc(1)))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldloc_0);
                c.EmitDelegate((MultiplayerUnlocks self, List<MultiplayerUnlocks.Unlock> list) => {
                    if (!self.unlockAll) return;
                    list.AddRange(customLevelUnlocks.Select(x => new MultiplayerUnlocks.Unlock(x.Value)));
                });
            }
            else
            {
                CustomRegionsMod.BepLogError("CustomRegions.Collectables.ArenaUnlocks.MultiplayerUnlocks_ctor: IL Hook failed.");
            }
        }

        private static string MultiplayerUnlocks_LevelDisplayName(On.MultiplayerUnlocks.orig_LevelDisplayName orig, string s)
        {
            if (customLevelNames.ContainsKey(s)) return customLevelNames[s];
            return orig(s);
        }

        private static MultiplayerUnlocks.LevelUnlockID MultiplayerUnlocks_LevelLockID(On.MultiplayerUnlocks.orig_LevelLockID orig, string levelName)
        {
            if (customLevelUnlocks.ContainsKey(levelName.ToLower()))
            {
                try
                {
                    MultiplayerUnlocks.LevelUnlockID unlockID = customLevelUnlocks[levelName];
                    //CustomRegionsMod.CustomLog($"found custom arena unlock [{levelName}] [{unlockID}]");
                    return unlockID;

                }
                catch (Exception e)
                {
                    CustomRegionsMod.CustomLog($"Error parsing levelUnlockID enum [{levelName}] - [{e}]", true);
                }
            }

            return orig(levelName);
        }

        public static Dictionary<string, MultiplayerUnlocks.LevelUnlockID> customLevelUnlocks = new Dictionary<string, MultiplayerUnlocks.LevelUnlockID>();
        public static Dictionary<string, string> customLevelNames = new Dictionary<string, string>();


        public static void RefreshArenaUnlocks()
        {
            UnregisterArenaUnlocks();
            RegisterArenaUnlocks();
        }

        public static void UnregisterArenaUnlocks()
        {
            foreach (KeyValuePair<string, MultiplayerUnlocks.LevelUnlockID> unlock in customLevelUnlocks)
            { unlock.Value?.Unregister(); }

            customLevelUnlocks = new();
            customLevelNames = new();
        }

        public static void RegisterArenaUnlocks()
        {
            string filePath = AssetManager.ResolveFilePath("CustomUnlocks.txt");
            if (!File.Exists(filePath)) return;

            CustomRegionsMod.CustomLog("\nRegistering Custom Arena Unlocks");

            foreach (string line in File.ReadAllLines(filePath))
            {
                if (line.IsNullOrWhiteSpace())
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
                    levelNames = Regex.Split(lineDivided[1], ",");
                }
                catch (Exception e)
                {
                    CustomRegionsMod.CustomLog("Error loading levelUnlock ID" + e, true);
                    continue;
                }

                foreach (string level in levelNames)
                {
                    if (level.IsNullOrWhiteSpace())
                    { continue; }

                    string[] levelSplit = Regex.Split(level, "-");
                    string levelFile = levelSplit[0].Trim();
                    string levelName = levelSplit.Length >= 2 ? levelSplit[1].Trim() : levelFile;
                    levelFile = levelFile.ToLower();
                    try
                    {

                        if (!customLevelUnlocks.ContainsKey(levelFile))
                        {
                            customLevelUnlocks[levelFile] = unlockID;
                            customLevelNames[levelFile] = levelName;
                            CustomRegionsMod.CustomLog($"Added new level unlock: [{level}-{unlockID}]");
                        }
                        else
                        {
                            CustomRegionsMod.CustomLog($"Duplicated arena name from two packs! [{level}]", true);
                        }
                    }
                    catch (Exception e)
                    {
                        CustomRegionsMod.CustomLog($"Error adding level unlock ID [{level}] [{e}]", true);
                    }
                }
            }
        }
    }
}
