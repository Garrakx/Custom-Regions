using CustomRegions.Mod;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CustomRegions
{
    static class CustomDecalHook
    {
        public static void ApplyHook()
        {
            On.CustomDecal.LoadFile += CustomDecal_LoadFile;
        }

        /// <summary>
        /// Loads custom decal if it does not find it in the vanilla assets folder.
        /// </summary>
        private static void CustomDecal_LoadFile(On.CustomDecal.orig_LoadFile orig, CustomDecal self, string fileName)
        {
            if (Futile.atlasManager.GetAtlasWithName(fileName) != null)
            {
                return;
            }

            string vanillaPath = string.Concat(new object[] { Custom.RootFolderDirectory(), "Assets", Path.DirectorySeparatorChar, "Futile", Path.DirectorySeparatorChar, "Resources", Path.DirectorySeparatorChar, "Decals", Path.DirectorySeparatorChar, fileName, ".png" });

            if (File.Exists(vanillaPath))
            {
                orig(self, fileName);
            }
            else
            {
                // Rain World\Mods\CustomResources\Aether Ridge\Assets\Futile\Resources\Decals


                foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
                {
                    char dash = Path.DirectorySeparatorChar;
                    string customPath = $"{Custom.RootFolderDirectory()}{CustomWorldMod.resourcePath}{keyValues.Value}{dash}Assets{dash}Futile{dash}Resources{dash}Decals{dash}{fileName}.png";
                    //Debug.Log($"Custom Regions: Searching custom decal [{fileName}] at [{customPath}]");
                    if(File.Exists(customPath))
                    {
                        Debug.Log($"Custom Regions: Found custom decal [{fileName}] at [{customPath}]");
                        WWW www = new WWW("file:///"+customPath);
                        Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                        texture2D.wrapMode = TextureWrapMode.Clamp;
                        texture2D.anisoLevel = 0;
                        texture2D.filterMode = FilterMode.Point;
                        www.LoadImageIntoTexture(texture2D);
                        HeavyTexturesCache.LoadAndCacheAtlasFromTexture(fileName, texture2D);
                        break;
                    }
                }
            }
        }
    }
}
