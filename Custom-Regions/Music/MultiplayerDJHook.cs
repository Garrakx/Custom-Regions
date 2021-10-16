using CustomRegions.Mod;
using System.Collections.Generic;
using System.IO;

namespace CustomRegions.Arena
{
    static class MultiplayerDJHook
    {
        public static void ApplyHooks()
        {
            On.Music.MultiplayerDJ.ctor += MultiplayerDJ_ctor;
        }

        private static void MultiplayerDJ_ctor(On.Music.MultiplayerDJ.orig_ctor orig, global::Music.MultiplayerDJ self, global::Music.MusicPlayer musicPlayer)
        {
            orig(self, musicPlayer);

            string[] vanillaSongs = (string[])self.availableSongs.Clone();
            List<string> customMultiSongs = new List<string>();
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
            {
                string multiMusicPath = CRExtras.BuildPath(keyValues.Value, CRExtras.CustomFolder.Music, file: "MPMusic.txt");
                if (File.Exists(multiMusicPath) )
                {
                    CustomWorldMod.Log($"Loading Multiplayer music from [{keyValues.Key}]...", false, CustomWorldMod.DebugLevel.FULL);
                    customMultiSongs.AddRange(File.ReadAllLines(multiMusicPath));
                }
            }
            customMultiSongs.AddRange(vanillaSongs);
            self.availableSongs = customMultiSongs.ToArray();
            CustomWorldMod.Log($"Loaded Multiplayer songs: [{string.Join(", ", self.availableSongs)}]", false, CustomWorldMod.DebugLevel.MEDIUM);
        }
    }
}
