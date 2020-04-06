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
    static class RoomCameraHook
    {
        public static void ApplyHook()
        {
            On.RoomCamera.LoadPalette += RoomCamera_LoadPalette;
        }

        private static void RoomCamera_LoadPalette(On.RoomCamera.orig_LoadPalette orig, RoomCamera self, int pal, ref UnityEngine.Texture2D texture)
        {
            if (pal > 35)
            {
                string regionName = self.room.world.region.name;
                Debug.Log($"Custom Regions: Loading custom palette [{pal}] from [{regionName}]");

                foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
                {
                    string path = CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar;

                    string paletteFolder = string.Concat(new object[] { "file:///", Custom.RootFolderDirectory(), path, keyValues.Value, Path.DirectorySeparatorChar, "Assets", Path.DirectorySeparatorChar, "Futile", Path.DirectorySeparatorChar, "Resources", Path.DirectorySeparatorChar, "Palettes", Path.DirectorySeparatorChar, "palette", pal, ".png" });
                    Debug.Log($"Custom Regions: Searching palette at {paletteFolder}");

                    if (Directory.Exists(paletteFolder))
                    {
                        Debug.Log($"Custom Regions: Found custom palette [{paletteFolder}]");
                        //notFound = false;

                        texture = new Texture2D(32, 16, TextureFormat.ARGB32, false);
                        texture.anisoLevel = 0;
                        texture.filterMode = FilterMode.Point;
                        self.www = new WWW(paletteFolder);
                        self.www.LoadImageIntoTexture(texture);
                        if (self.room != null)
                        {
                            self.ApplyEffectColorsToPaletteTexture(ref texture, self.room.roomSettings.EffectColorA, self.room.roomSettings.EffectColorB);
                        }
                        else
                        {
                            self.ApplyEffectColorsToPaletteTexture(ref texture, -1, -1);
                        }
                        texture.Apply(false);
                    }


                }

            }
            else
            {
                orig(self, pal, ref texture);
            }
        }
    }
}
