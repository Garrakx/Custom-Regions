using CustomRegions.Mod;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CustomRegions.CustomMenu
{
    static class MultiplayerMenuHook
    {
        public static void ApplyHook()
        {
            On.Menu.MultiplayerMenu.ctor += MultiplayerMenu_ctor;
        }

        private static void MultiplayerMenu_ctor(On.Menu.MultiplayerMenu.orig_ctor orig, Menu.MultiplayerMenu self, ProcessManager manager)
        {
            orig(self, manager);

            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
            {
                string path = Custom.RootFolderDirectory() + CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar + "Levels";
                if (Directory.Exists(path))
                {
                    Debug.Log($"Custom Regions: Loading arena(s) from [{keyValues.Value}]");
                    string debug = $"Loaded arenas [";
                    string[] files = Directory.GetFiles(path);
                    for (int i = 0; i < files.Length; i++)
                    {
                        if (files[i].Substring(files[i].Length - 4, 4) == ".txt" && files[i].Substring(files[i].Length - 13, 13) != "_Settings.txt" && files[i].Substring(files[i].Length - 10, 10) != "_Arena.txt")
                        {
                            string[] array = files[i].Substring(0, files[i].Length - 4).Split(new char[]
                            {
                        Path.DirectorySeparatorChar
                            });
                            self.allLevels.Add(array[array.Length - 1]);
                            debug += array[array.Length - 1] + ", ";
                        }
                        for (int j = self.allLevels.Count - 1; j >= 0; j--)
                        {
                            if (!self.multiplayerUnlocks.IsLevelUnlocked(self.allLevels[j]))
                            {
                                self.allLevels.RemoveAt(j);
                            }
                        }
                        self.allLevels.Sort((string A, string B) => self.multiplayerUnlocks.LevelListSortString(A).CompareTo(self.multiplayerUnlocks.LevelListSortString(B)));
                        for (int k = 0; k < self.allLevels.Count; k++)
                        {
                            self.thumbsToBeLoaded.Add(self.allLevels[k]);
                        }
                    }
                    Debug.Log(debug + "]");
                }

            }

        }
    }
}
