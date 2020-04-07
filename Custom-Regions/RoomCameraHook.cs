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


        /// <summary>
        /// Searchs the CustomResources folder for a custom palette if its name is greater than 35. 
        /// CAREFUL! If two mods use the same palette number it will pick the first one it loads.
        /// </summary>
        private static void RoomCamera_LoadPalette(On.RoomCamera.orig_LoadPalette orig, RoomCamera self, int pal, ref UnityEngine.Texture2D texture)
        {
            if (pal > 35)
            {
                string regionName = string.Empty;
                try
                {
                    regionName = self.room.world.region.name;
                }
                catch (Exception e)
                {
                    Debug.Log($"Custom Regions: Error loading regionName from palette, world / region is null [{e}]");
                }

                Debug.Log($"Custom Regions: Loading custom palette [{pal}] from [{regionName}]");

                foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
                {
                   /* if (regionName == string.Empty) 
                    {
                    }*/
                    regionName = keyValues.Value;
                    string path = CustomWorldMod.resourcePath + regionName;

                    string paletteFolder = string.Concat(new object[] { Custom.RootFolderDirectory(), path, Path.DirectorySeparatorChar, "Assets", Path.DirectorySeparatorChar, "Futile", Path.DirectorySeparatorChar, "Resources", Path.DirectorySeparatorChar, "Palettes"});
                    //Debug.Log($"Custom Regions: Searching palette at {paletteFolder}");

                    if (Directory.Exists(paletteFolder))
                    {
                        //Debug.Log($"Custom Regions: Found custom palette directory [{paletteFolder}]");
                        string palettePath = paletteFolder + Path.DirectorySeparatorChar + "palette" + pal + ".png";
                        if (File.Exists(palettePath)) 
                        {
                            Debug.Log($"Custom Regions: loading custom palette [{palettePath}]");
                            texture = new Texture2D(32, 16, TextureFormat.ARGB32, false);
                            texture.anisoLevel = 0;
                            texture.filterMode = FilterMode.Point;
                            self.www = new WWW("file:///" + palettePath);
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
                            break;
                        }
                        /*else
                        {
                            Debug.Log($"Custom Regions: ERROR !!! loading custom palette [{palettePath}]");
                        }*/
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
