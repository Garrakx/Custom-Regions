using CustomRegions.Mod;
using RWCustom;
using System;
using System.IO;
using UnityEngine;

namespace CustomRegions.CustomMenu
{
    static class MenuIllustrationHook
    {
        public static void ApplyHooks()
        {
            On.Menu.MenuIllustration.LoadFile_1 += MenuIllustration_LoadFile_1;
        }

        public static void RemoveHooks()
        {
            On.Menu.MenuIllustration.LoadFile_1 -= MenuIllustration_LoadFile_1;
        }

        private static void MenuIllustration_LoadFile_1(On.Menu.MenuIllustration.orig_LoadFile_1 orig, Menu.MenuIllustration self, string folder)
        {
            if (folder.Contains("CustomResources"))
            {
                try
                {
                    CustomWorldMod.Log($"Custom Regions: Loading custom resources at MenuIllustration. Folder [{folder}] and fileName [{self.fileName}]");
                    /*string path = CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar;
                    string sceneFolder = path + "Assets" + Path.DirectorySeparatorChar + "Futile" + Path.DirectorySeparatorChar + "Resources" + Path.DirectorySeparatorChar + "Scenes" + Path.DirectorySeparatorChar + $"Landscape - {keyValues.Key}";
                    */

                    self.www = new WWW(string.Concat(new object[] { "file:///", Custom.RootFolderDirectory(), folder, Path.DirectorySeparatorChar, self.fileName, ".png" }));
                    self.texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                    self.texture.wrapMode = TextureWrapMode.Clamp;
                    if (self.crispPixels)
                    {
                        self.texture.anisoLevel = 0;
                        self.texture.filterMode = FilterMode.Point;
                    }
                    self.www.LoadImageIntoTexture(self.texture);
                    HeavyTexturesCache.LoadAndCacheAtlasFromTexture(self.fileName, self.texture);
                    self.www = null;
                }
                catch (Exception e)
                {
                    CustomWorldMod.Log($"Custom Regions: Failed loading textures for {folder} - {self.fileName} "+e);
                }

            }
            else
            {
                orig(self, folder);
            }


        }

    }
}
