using CustomRegions.Mod;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CustomRegions.DevInterface
{
    static class TriggersPageHook
    {
        public static void RemoveHooks()
        {
            On.DevInterface.TriggersPage.ctor -= TriggersPage_ctor;
        }

        public static void ApplyHooks()
        {
            On.DevInterface.TriggersPage.ctor += TriggersPage_ctor;
        }

        private static void TriggersPage_ctor(On.DevInterface.TriggersPage.orig_ctor orig, global::DevInterface.TriggersPage self, global::DevInterface.DevUI owner, string IDstring, global::DevInterface.DevUINode parentNode, string name)
        {
            orig(self, owner, IDstring, parentNode, name);
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
            {
                string songsPath = CRExtras.BuildPath(keyValues.Value, CRExtras.CustomFolder.Songs);
                DirectoryInfo directoryInfo = new DirectoryInfo(songsPath);
                CustomWorldMod.Log($"[TriggerPage] Loading custom triggers for [{keyValues.Key}]", false, CustomWorldMod.DebugLevel.FULL);
                if (directoryInfo.Exists)
                {
                    CustomWorldMod.Log($"[TriggerPage] Found custom triggers for [{keyValues.Key}]", false, CustomWorldMod.DebugLevel.MEDIUM);
                    FileInfo[] files = directoryInfo.GetFiles().Where(x => !x.Extension.Equals(".meta")).ToArray();
                    int previousIndex = 0;
                    if (self.songNames == null)
                    {
                        self.songNames = new string[files.Length];
                    }
                    else
                    {
                        previousIndex = self.songNames.Length;
                        Array.Resize(ref self.songNames, previousIndex + files.Length);
                    }
                    for (int j = previousIndex; j < self.songNames.Length; j++)
                    {
                        self.songNames[j] = Path.GetFileNameWithoutExtension(files[j].Name);
                    }
                    CustomWorldMod.Log($"[TriggerPage] Loaded ({self.songNames.Length-previousIndex}) sound triggers from [{keyValues.Key}]");
                }
            }

        }
    }
}
