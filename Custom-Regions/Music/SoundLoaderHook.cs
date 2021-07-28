using CustomRegions.Mod;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CustomRegions.Music
{
    static class SoundLoaderHook
    {
        public static void RemoveHooks()
        {
            On.SoundLoader.RequestAmbientAudioClip -= SoundLoader_RequestAmbientAudioClip;
        }

        public static void ApplyHooks()
        {
            On.SoundLoader.RequestAmbientAudioClip += SoundLoader_RequestAmbientAudioClip;
        }

        private static UnityEngine.AudioClip SoundLoader_RequestAmbientAudioClip(On.SoundLoader.orig_RequestAmbientAudioClip orig,
            SoundLoader self, string clipName)
        {
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
            {
                /*
                string customPath = RWCustom.Custom.RootFolderDirectory() + CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar;
                string ambientPath = customPath + "Assets/Futile/Resources/LoadedSoundEffects/Ambient/";
                string clipPath = ambientPath + clipName;
                */
                string ambientPath = CRExtras.BuildPath(keyValues.Value, CRExtras.CustomFolder.Ambient);
                string clipPath = Path.Combine(ambientPath, clipName);

                CustomWorldMod.Log($"[SoundLoader] Loading new audioClip at [{clipPath}] from [{keyValues.Key}]", false, CustomWorldMod.DebugLevel.FULL);
                if (File.Exists(clipPath))
                {
                    for (int j = 0; j < self.ambientImporters.Count; j++)
                    {
                        if (self.ambientImporters[j].fileName == clipName)
                        {
                            CustomWorldMod.Log($"[SoundLoader] AudioClip already loaded [{clipPath}] from [{keyValues.Key}]", false, 
                                CustomWorldMod.DebugLevel.FULL);
                            return self.ambientImporters[j].loadedClip;
                        }
                    }
                    if (self.gameObject == null)
                    {
                        self.gameObject = new GameObject("CustomSoundLoader");
                    }
                    CustomWorldMod.Log($"[SoundLoader] Loaded new audioClip at [{clipPath}] from [{keyValues.Key}]", false, 
                        CustomWorldMod.DebugLevel.FULL);
                    SoundLoader.AmbientImporter ambientImporter = self.gameObject.AddComponent<SoundLoader.AmbientImporter>();
                    ambientImporter.fileName = clipName;
                    self.ambientImporters.Add(ambientImporter);
                    ambientImporter.absolutePath = ambientPath;
                    ambientImporter.Init(self);
                    return null;
                    /*
                    // File already loaded
                    for (int i = 0; i < self.ambientClipsThroughUnity.Count; i++)
                    {
                        if (self.ambientClipsThroughUnity[i].name == clipName)
                        {
                            CustomWorldMod.Log($"[SoundLoader] AudioClip already loaded [{clipPath}] from [{keyValues.Key}]", false, CustomWorldMod.DebugLevel.FULL);
                            return self.ambientClipsThroughUnity[i];
                        }
                    }
                    WWW www = new WWW("file://" + clipPath);
                    AudioClip audioClip = www.GetAudioClip(false, false, AudioType.OGGVORBIS);
                    if (audioClip == null)
                    {
                        CustomWorldMod.Log($"[SoundLoader] Error loading file [{"file://" + clipPath}]", true);
                        return null;
                    }
                    audioClip.name = clipName;
                    self.ambientClipsThroughUnity.Add(audioClip);
                    CustomWorldMod.Log($"[SoundLoader] Loaded new audioClip at [{clipPath}] from [{keyValues.Key}]", false, CustomWorldMod.DebugLevel.FULL);
                    return audioClip;
                    */
                }
            }

            return orig(self, clipName);
        }

        /*
        private static void SoundLoader_LoadAllAmbientSounds(On.SoundLoader.orig_LoadAllAmbientSounds orig, SoundLoader self)
        {
            orig(self);
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
            {
                string customPath = RWCustom.Custom.RootFolderDirectory() + CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar;
                CustomWorldMod.Log($"[SoundLoader] Loading AmbientSounds ... [{customPath}]", false, CustomWorldMod.DebugLevel.FULL);
                DirectoryInfo directoryInfo = new DirectoryInfo(customPath + "SoundEffects/Ambient/");
                FileInfo[] files = directoryInfo.GetFiles();
                for (int i = 0; i < files.Length; i++)
                {
                    self.RequestAmbientAudioClip(files[i].Name);
                }
            }
        }
        */
    }
}
