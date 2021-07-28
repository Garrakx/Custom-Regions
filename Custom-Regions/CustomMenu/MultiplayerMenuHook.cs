using CustomRegions.Mod;
using MonoMod.RuntimeDetour;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace CustomRegions.CustomMenu
{
    static class MultiplayerMenuHook
    {
        public static void ApplyHooks()
        {
            On.Menu.MultiplayerMenu.ctor += MultiplayerMenu_ctor;
        }

        public static void RemoveHooks()
        {
            On.Menu.MultiplayerMenu.ctor -= MultiplayerMenu_ctor;
        }

        // Thumbnail
        // THIS IS CALLED IN WWWCTOR
        // EXTREMELY CURSED
        public static void MultiplayerMenuUrl(ref string url)
        {
            if (url.Contains("file:///" + Custom.RootFolderDirectory() + "Levels") && url.Contains("_1.png"))
            {
                //"file:///", Custom.RootFolderDirectory(), "Levels", Path.DirectorySeparatorChar, text, "_1.png"
                string path = url;

                //Remove "file:///"
                string stringToRemove = "file:///";
                int found = path.IndexOf(stringToRemove);
                path = path.Substring(found + stringToRemove.Length);
                //Custom.RootFolderDirectory(), "Levels", Path.DirectorySeparatorChar, text, "_1.png"

                //Remove "_1.png"
                stringToRemove = "_1.png";
                found = path.IndexOf(stringToRemove);
                path = path.Substring(0, found);
                //Custom.RootFolderDirectory(), "Levels", Path.DirectorySeparatorChar, text
                path += "_Thumb.png";

                //CustomWorldMod.CustomWorldLog($"Custom Regions: WWW trimmed path [{path}]. File exists [{File.Exists(path)}]");

                if (!File.Exists(path))
                {
                    //CustomWorldMod.CustomWorldLog($"Custom Regions: File does not exist [{path}]");

                    //Remove Custom.RootFolderDirectory(), "Levels", Path.DirectorySeparatorChar,
                    stringToRemove = Custom.RootFolderDirectory() + "Levels" + Path.DirectorySeparatorChar;
                    found = path.IndexOf(stringToRemove);
                    path = path.Substring(found + stringToRemove.Length);
                    //text, "_Thumb.png"

                    //Remove after text
                    found = path.IndexOf("_");
                    if (found > 0)
                    { path = path.Substring(0, found); }

                    foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
                    {
                        CustomWorldMod.Log($"Custom Regions: WWWW trimmed path [{path}]");
                        /*
                        string updatedPath = Custom.RootFolderDirectory() + CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar + 
                            "Levels" + Path.DirectorySeparatorChar;
                        */
                        string updatedPath = CRExtras.BuildPath(keyValues.Value, CRExtras.CustomFolder.Levels);
                        if (File.Exists(updatedPath + path + "_Thumb.png"))
                        {
                            url = "file:///" + updatedPath + path + "_Thumb.png";
                            break;
                        }
                    }
                }

            }
        }


        /// <summary>
        /// CURSED
        /// </summary>
        private static void MultiplayerMenu_ctor(On.Menu.MultiplayerMenu.orig_ctor orig, Menu.MultiplayerMenu self, ProcessManager manager)
        {
            orig(self, manager);

            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
            {
                /*
                string path = Custom.RootFolderDirectory() + CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar + "Levels";
                */
                string path = CRExtras.BuildPath(keyValues.Value, CRExtras.CustomFolder.Levels);

                if (Directory.Exists(path))
                {
                    CustomWorldMod.Log($"Custom Regions: Loading arena(s) from [{keyValues.Value}]");

                    string[] files = Directory.GetFiles(path);

                    for (int i = 0; i < files.Length; i++)
                    {
                        if (files[i].Substring(files[i].Length - 4, 4) == ".txt" && files[i].Substring(files[i].Length - 13, 13) != "_Settings.txt" 
                            && files[i].Substring(files[i].Length - 10, 10) != "_Arena.txt" && !files[i].Contains(CustomWorldMod.customUnlocksFileName) )
                        {
                            string[] array = files[i].Substring(0, files[i].Length - 4).Split(new char[]
                            {
                                Path.DirectorySeparatorChar
                            });
                            self.allLevels.Add(array[array.Length - 1]);
                        }
                        for (int j = self.allLevels.Count - 1; j >= 0; j--)
                        {
                            if (!self.multiplayerUnlocks.IsLevelUnlocked(self.allLevels[j]))
                            {
                                self.allLevels.RemoveAt(j);
                            }
                        }
                        self.allLevels.Sort((string A, string B) => 
                        self.multiplayerUnlocks.LevelListSortString(A).CompareTo(self.multiplayerUnlocks.LevelListSortString(B)));

                        foreach (string level in self.allLevels)
                        {
                            if (!self.thumbsToBeLoaded.Contains(level))
                            {
                                self.thumbsToBeLoaded.Add(level);
                            }
                        }
                        /*for (int k = 0; k < self.allLevels.Count; k++)
                        {
                            self.thumbsToBeLoaded.Add(self.allLevels[k]);
                        }*/
                    }
                }
                string debug = $"Custom Regions: Loaded arenas [";
                string debug2 = $"Custom Regions: Pending thumbnails [";

                foreach (string level in self.allLevels)
                {
                    debug += level + ", ";

                }
                foreach (string thumb in self.thumbsToBeLoaded)
                {
                    debug2 += thumb + ", ";

                }

                CustomWorldMod.Log(debug + "]");
                //CustomWorldMod.CustomWorldLog(debug2 + "]");
                self.ClearGameTypeSpecificButtons();
                self.InitiateGameTypeSpecificButtons();

            }

        }
    }
}
