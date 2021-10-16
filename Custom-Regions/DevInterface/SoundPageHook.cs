using CustomRegions.Mod;
using DevInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        private static void SoundPage_ctor(On.DevInterface.SoundPage.orig_ctor orig, SoundPage self, 
            DevUI owner, string IDstring, DevUINode parentNode, string name)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo("./SoundEffects/Ambient/");
            if (!directoryInfo.Exists)
            {
                Directory.CreateDirectory(directoryInfo.FullName);
                CustomWorldMod.Log($"Creating directory at [{directoryInfo.FullName}] to avoid a crash.", true);
            }

            try
            {
                orig(self, owner, IDstring, parentNode, name);
            } catch (Exception e) { CustomWorldMod.Log($"SoundPage crashed: \n{e}", true); }

            List<FileInfo> list = new List<FileInfo>();

            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
            {
                string customPath = CRExtras.BuildPath(keyValues.Value, CRExtras.CustomFolder.Ambient);
                directoryInfo = new DirectoryInfo(customPath);

                if (directoryInfo.Exists)
                {
                    FileInfo[] array = directoryInfo.GetFiles();
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (!list.Contains(array[i]) && !array[i].Name.Contains("meta"))
                        {
                            CustomWorldMod.Log($"[DevLoadedSoundEffects] Adding ambient sounds {array[i]} from [{keyValues.Key}]", 
                                false, CustomWorldMod.DebugLevel.FULL);
                            list.Add(array[i]);
                        }
                    }
                }
                else
                {
                    CustomWorldMod.Log($"[DevLoadedSoundEffects] {keyValues.Key} does not have loaded ambient sounds at {directoryInfo.FullName}",
                        false, CustomWorldMod.DebugLevel.FULL);
                }

                directoryInfo = new DirectoryInfo(customPath + "SoundEffects/Ambient/");
                if (directoryInfo.Exists)
                {
                    FileInfo[] array = directoryInfo.GetFiles();
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
                            CustomWorldMod.Log($"[DevLoadedSoundEffects/Ambient] Adding ambient sounds {array[j]} from [{keyValues.Key}]", 
                                false, CustomWorldMod.DebugLevel.FULL);
                            list.Add(array[j]);
                        }
                    }
     
                }
                else
                {
                    CustomWorldMod.Log($"{keyValues.Key} does not have ambient sounds at {directoryInfo.FullName}", false, CustomWorldMod.DebugLevel.FULL);
                }

            }

            List<FileInfo> currentFiles = new List<FileInfo>(self.files);
            for (int a = list.Count-1; a >= 0; a--)
            {
                currentFiles.Insert(0, list[a]);
            }
            self.files = currentFiles.ToArray();

            CustomWorldMod.Log($"[DevLoadedSoundEffects] Loaded ambient sounds: [{string.Join(", ", self.files.Select(l => l.Name).ToArray())}]", 
                false, CustomWorldMod.DebugLevel.FULL);

			self.totalFilePages = 1 + (int)((float)self.files.Length / (float)self.maxFilesPerPage + 0.5f);
			self.RefreshFilesPage();

		}
    }
}
