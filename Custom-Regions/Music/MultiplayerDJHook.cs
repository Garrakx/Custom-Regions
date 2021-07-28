using CustomRegions.Mod;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CustomRegions.Arena
{
    static class MultiplayerDJHook
    {
        public static void ApplyHooks()
        {
            On.Music.MultiplayerDJ.ctor += MultiplayerDJ_ctor;
        }

        public static void RemoveHooks()
        {
            On.Music.MultiplayerDJ.ctor -= MultiplayerDJ_ctor;
        }

        private static void MultiplayerDJ_ctor(On.Music.MultiplayerDJ.orig_ctor orig, global::Music.MultiplayerDJ self, global::Music.MusicPlayer musicPlayer)
        {
            orig(self, musicPlayer);

            string[] vanillaSongs = (string[])self.availableSongs.Clone();
            List<string> customMultiSongs = new List<string>();
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
            {
                /*
                string folderPath = RWCustom.Custom.RootFolderDirectory() + CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar;
                string multiMusicPath = folderPath + "Assets" + Path.DirectorySeparatorChar + "Futile" + Path.DirectorySeparatorChar + "Resources"
                    + Path.DirectorySeparatorChar + "Music" + Path.DirectorySeparatorChar + "MPMusic.txt";
                */
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
