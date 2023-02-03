using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace CustomRegionsMod.ArenaUnlocks
{
    internal static class UnlockEnum
    {
        public static void ApplyHooks()
        {
            On.MultiplayerUnlocks.LevelLockID += MultiplayerUnlocks_LevelLockID;
        }

        private static MultiplayerUnlocks.LevelUnlockID MultiplayerUnlocks_LevelLockID(On.MultiplayerUnlocks.orig_LevelLockID orig, string levelName)
        {
            if (customLevelUnlocks.ContainsKey(levelName.ToLower()))
            {
                try
                {
                    MultiplayerUnlocks.LevelUnlockID unlockID = customLevelUnlocks[levelName];
                    CustomRegionsMod.Log($"found custom arena unlock [{levelName}] [{unlockID}]");
                    return unlockID;

                }
                catch (Exception e)
                {
                    CustomRegionsMod.Log($"Error parsing levelUnlockID enum [{levelName}] - [{e}]", true);
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
			try
			{
				foreach (KeyValuePair<string, MultiplayerUnlocks.LevelUnlockID> unlock in customLevelUnlocks)
				{ if (unlock.Value != null) { unlock.Value.Unregister(); } }

				customLevelUnlocks = new Dictionary<string, MultiplayerUnlocks.LevelUnlockID>();
			}
			catch (Exception e) { throw e; }
		}

		public static void RegisterArenaUnlocks()
		{
			foreach (string path2 in AssetManager.ListDirectory("", false, true))
			{

				if (!File.Exists(path2) || Path.GetFileName(path2) != "customunlocks.txt")
				{ continue; }

				foreach (string line in File.ReadAllLines(path2))
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
                        CustomRegionsMod.Log("Error loading levelUnlock ID" + e, true);
                        continue;
                    }

                    try
                    {
                        levelNames = Regex.Split(lineDivided[1].Replace(" ", ""), ",");
                    }
                    catch (Exception e)
                    {
                        CustomRegionsMod.Log("Error loading levelUnlock name" + e, true);
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
                                CustomRegionsMod.Log($"Added new level unlock: [{levelNames[j]}-{unlockID}]");
                            }

                            else
                            {
                                CustomRegionsMod.Log($"Duplicated arena name from two packs! [{levelNames[j]}]", true);
                            }
                        }
                        catch (Exception e)
                        {
                            CustomRegionsMod.Log($"Error adding level unlock ID [{levelNames[j]}] [{e}]", true);
                        }
                    }
                }
            }
		}
	}
}
