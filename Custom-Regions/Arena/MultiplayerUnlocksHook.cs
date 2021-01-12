using CustomRegions.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CustomRegions.Arena
{
    static class MultiplayerUnlocksHook
    {
        public static void ApplyHooks()
        {
            On.MultiplayerUnlocks.LevelLockID += MultiplayerUnlocks_LevelLockID;
        }

        private static MultiplayerUnlocks.LevelUnlockID MultiplayerUnlocks_LevelLockID(On.MultiplayerUnlocks.orig_LevelLockID orig, string levelName)
        {
            if (CustomWorldMod.levelUnlocks.ContainsKey(levelName))
            {
                MultiplayerUnlocks.LevelUnlockID unlockID = (MultiplayerUnlocks.LevelUnlockID)(-1);
                try
                {
                    unlockID = (MultiplayerUnlocks.LevelUnlockID)
                                            Enum.Parse(typeof(MultiplayerUnlocks.LevelUnlockID), CustomWorldMod.levelUnlocks[levelName]);
                    CustomWorldMod.Log($"Unlocked custom arena [{levelName}] [{unlockID}]");

                } catch (Exception e)
                {
                    CustomWorldMod.Log($"Error parsing levelUnlockID enum [{levelName}] - [{e}]", true);
                }
                if (unlockID >= 0)
                {
                    return unlockID;
                }
            }

           return orig(levelName);
        }
    }
}
