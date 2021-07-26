using CustomRegions.Mod;
using DevInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CustomRegions.DevInterface
{
    static class SoundPageHook
    {
        public static void ApplyHooks()
        {
            On.DevInterface.SoundPage.ctor += SoundPage_ctor;
        }

        public static void RemoveHooks()
        {
            On.DevInterface.SoundPage.ctor -= SoundPage_ctor;
        }

        private static void SoundPage_ctor(On.DevInterface.SoundPage.orig_ctor orig, global::DevInterface.SoundPage self, global::DevInterface.DevUI owner, string IDstring, global::DevInterface.DevUINode parentNode, string name)
        {
            try
            {
                orig(self, owner, IDstring, parentNode, name);
            } catch (Exception e) { CustomWorldMod.Log($"SoundPage crashed: \n{e}", true); }

            List<FileInfo> list = new List<FileInfo>();

            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
            {
                string customPath = RWCustom.Custom.RootFolderDirectory() + CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar;
                DirectoryInfo directoryInfo = new DirectoryInfo(customPath+"Assets/Futile/Resources/LoadedSoundEffects/Ambient/");
                if (!directoryInfo.Exists)
                {
                    CustomWorldMod.Log($"{keyValues.Key} does not have loaded ambient sounds at {directoryInfo.FullName}", 
                        false, CustomWorldMod.DebugLevel.FULL);
                    continue;
                }

                FileInfo[] array = directoryInfo.GetFiles();
                for (int i = 0; i < array.Length; i++)
                {
                    if (!list.Contains(array[i]))
                    {
                        CustomWorldMod.Log($"Adding loaded ambient sounds {array[i]} from [{keyValues.Key}]", false, CustomWorldMod.DebugLevel.FULL);
                        list.Add(array[i]);
                    }
                }
                directoryInfo = new DirectoryInfo(customPath+"SoundEffects/Ambient/");
                if (!directoryInfo.Exists)
                {
                    CustomWorldMod.Log($"{keyValues.Key} does not have ambient sounds at {directoryInfo.FullName}", false, CustomWorldMod.DebugLevel.FULL);
                    continue;
                }

                array = directoryInfo.GetFiles();
                for (int j = 0; j < array.Length; j++)
                {
                    bool flag = true;
                    for (int k = 0; k < list.Count; k++)
                    {
                        if (list[k].Name == array[k].Name)
                        {
                            flag = false;
                            break;
                        }
                    }
                    if (flag)
                    {
                        CustomWorldMod.Log($"Adding ambient sounds {array[j]} from [{keyValues.Key}]", false, CustomWorldMod.DebugLevel.FULL);
                        list.Add(array[j]);
                    }
                }
                // CURSED
                for (int l = list.Count - 1; l >= 0; l--)
                {
                    if (list[l].Name.Length > 5 && list[l].Name.Substring(list[l].Name.Length - 5, 5) == ".meta")
                    {
                        list.RemoveAt(l);
                    }
                }

            }

            List<FileInfo> currentFiles = new List<FileInfo>(self.files);
            for (int a = list.Count; a > 0; a--)
            {
                currentFiles.Insert(0, list[a]);
            }
            self.files = currentFiles.ToArray();

            CustomWorldMod.Log($"Loaded ambient sounds: [{string.Join(", ", self.files.Select(l => l.Name).ToArray())}]");

			self.totalFilePages = 1 + (int)((float)self.files.Length / (float)self.maxFilesPerPage + 0.5f);
			self.RefreshFilesPage();

		}
    }
}
