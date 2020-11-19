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
            //On.RoomCamera.ctor += RoomCamera_ctor;

            // If a custom room uses vanilla textures
             On.RoomCamera.MoveCamera2 += RoomCamera_MoveCamera2;

             On.RoomCamera.PreLoadTexture += RoomCamera_PreLoadTexture;
        }

        /*
        private static void RoomCamera_ctor(On.RoomCamera.orig_ctor orig, RoomCamera self, RainWorldGame game, int cameraNumber)
        {
            orig(self, game, cameraNumber);
        }
        */

        private static void RoomCamera_PreLoadTexture(On.RoomCamera.orig_PreLoadTexture orig, RoomCamera self, Room room, int camPos)
        {
            if (self.quenedTexture == string.Empty)
            {
                string requestedTexture = WorldLoader.FindRoomFileDirectory(room.abstractRoom.name, true) + "_" + camPos + 1 + ".png";
                string path = requestedTexture;

                string delimitator = "file:///";
                int index = path.IndexOf(delimitator) + delimitator.Length;
                path = path.Substring(index);

                //CustomWorldMod.CustomWorldLog($"Custom regions: PreloadTexture path [{path}] Exists [{File.Exists(path)}]");
                if (!File.Exists(path))
                {
                    self.quenedTexture = FindCameraTexturePath(requestedTexture);
                    self.www = new WWW(self.quenedTexture);
                }
            }

            orig(self, room, camPos);
        }

        private static void RoomCamera_MoveCamera2(On.RoomCamera.orig_MoveCamera2 orig, RoomCamera self, string requestedTexture)
        {
            string path = requestedTexture;
            string delimitator = "file:///";
            int index = path.IndexOf(delimitator) + delimitator.Length;
            path = path.Substring(index);

            if (!File.Exists(path))
            {
                requestedTexture = FindCameraTexturePath(requestedTexture);
            }
            //CustomWorldMod.CustomWorldLog($"Custom regions: MoveCamera path [{path}] Exists [{File.Exists(path)}]. Requested texture [{requestedTexture}]. Quened texture [{self.quenedTexture}]");



            orig(self, requestedTexture);
        }


        public static string FindCameraTexturePath(string requestedTexture)
        {
            string delimitator = "Regions\\";
            int index = requestedTexture.IndexOf(delimitator) + delimitator.Length;
            string roomPathWithRegion = requestedTexture.Substring(index);

            string fullRoomPathWithRegion = Custom.RootFolderDirectory() + "World" + Path.DirectorySeparatorChar + "Regions" + Path.DirectorySeparatorChar + roomPathWithRegion;
           // CustomWorldMod.CustomWorldLog($"Custom regions: Searching vanilla room textures at [{fullRoomPathWithRegion}]");
            if (File.Exists(fullRoomPathWithRegion))
            {
                requestedTexture = "file:///" + fullRoomPathWithRegion;
                //CustomWorldMod.CustomWorldLog($"Custom regions: used vanilla textures for room [{requestedTexture}]");
            }

            return requestedTexture;
        }

        /// <summary>
        /// Searchs the CustomResources folder for a custom palette if its name is greater than 35. 
        /// CAREFUL! If two mods use the same palette number it will pick the first one it loads.
        /// Also loads effectColor.png
        /// </summary>
        private static void RoomCamera_LoadPalette(On.RoomCamera.orig_LoadPalette orig, RoomCamera self, int pal, ref UnityEngine.Texture2D texture)
        {
            // effect Color loading
            /*
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
            {
                string regionName = keyValues.Value;
                string path = CustomWorldMod.resourcePath + regionName;

                string effectColorPath = string.Concat(new object[] {
                    Custom.RootFolderDirectory(), path, Path.DirectorySeparatorChar, "Assets", Path.DirectorySeparatorChar,
                    "Futile", Path.DirectorySeparatorChar, "Resources", Path.DirectorySeparatorChar, "Palettes", Path.DirectorySeparatorChar, "effectColors.png" });

                if (File.Exists(effectColorPath))
                {
                    CustomWorldMod.CustomWorldLog($"Custom Regions: loading custom effectColor from [{keyValues.Value}]");
                    self.allEffectColorsTexture = new Texture2D(40, 4, TextureFormat.ARGB32, false);
                    self.allEffectColorsTexture.anisoLevel = 0;
                    self.allEffectColorsTexture.filterMode = FilterMode.Point;
                    self.www = new WWW(effectColorPath);
                    self.www.LoadImageIntoTexture(self.allEffectColorsTexture);
                    break;
                }
            }
            */


            // Palette
            string vanillaPalettePath = string.Concat(new object[]
            {
            Custom.RootFolderDirectory(),
            "Assets",
            Path.DirectorySeparatorChar,
            "Futile",
            Path.DirectorySeparatorChar,
            "Resources",
            Path.DirectorySeparatorChar,
            "Palettes",
            Path.DirectorySeparatorChar,
            "palette",
            pal,
            ".png"
            });

            if (pal > 35 && !File.Exists(vanillaPalettePath))
            {
                string regionName = string.Empty;
                /* 
                try
                {
                    regionName = self.room.world.region.name;
                }
                catch (Exception e)
                {
                    CustomWorldMod.CustomWorldLog($"Custom Regions: Error loading regionName from palette, world / region is null [{e}]");
                }

                CustomWorldMod.CustomWorldLog($"Custom Regions: Loading custom palette [{pal}] from [{regionName}]");
                */

                bool foundPalette = false;
                foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
                {
    
                    regionName = keyValues.Value;
                    string path = CustomWorldMod.resourcePath + regionName;

                    string paletteFolder = string.Concat(new object[] { Custom.RootFolderDirectory(), path, Path.DirectorySeparatorChar, "Assets", Path.DirectorySeparatorChar, "Futile", Path.DirectorySeparatorChar, "Resources", Path.DirectorySeparatorChar, "Palettes"});
                    //CustomWorldMod.CustomWorldLog($"Custom Regions: Searching palette at {paletteFolder}");

                    if (Directory.Exists(paletteFolder))
                    {
                        CustomWorldMod.Log($"Custom Regions: Found custom palette directory [{paletteFolder}]");
                        string palettePath = paletteFolder + Path.DirectorySeparatorChar + "palette" + pal + ".png";
                        if (File.Exists(palettePath)) 
                        {
                            foundPalette = true;
                            CustomWorldMod.Log($"Custom Regions: loading custom palette [{palettePath}]");
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
                        /*
                        else
                        {
                            CustomWorldMod.CustomWorldLog($"Custom Regions: ERROR when loading custom palette [{palettePath}]");
                        }
                        */
                    }
                }

                if (!foundPalette)
                {
                    CustomWorldMod.Log($"Error loading palette: {pal}");
                }
            }
            else
            {
                orig(self, pal, ref texture);
            }
        }
    }
}
