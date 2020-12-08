using CustomRegions.Mod;
using MonoMod.RuntimeDetour;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CustomRegions
{
    static class RoomCameraHook
    {
        public delegate void orig_WWW_ctor(WWW self, string url);

        public static void ApplyHook()
        {
            // Palette
            //On.RoomCamera.LoadPalette += RoomCamera_LoadPalette;
            IDetour hookWWWctor = new Hook(typeof(WWW).GetConstructor(new Type[] { typeof(string) }), typeof(RoomCameraHook).GetMethod("WWW_ctor"));

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
        /// 
        public static void WWW_ctor(orig_WWW_ctor orig, WWW self, string url)
        {
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
            "palette"
            });
            if (url.Contains(vanillaPalettePath) && url.Contains(".png"))
            {
                //CustomWorldMod.Log($"Loading palette [{url}]");
                bool foundPalette = false;
                int pal = -1;
                //string[] split = Regex.Split(url, "Palettes");

                //Remove all path
                int found = url.IndexOf(vanillaPalettePath);
                if (found > 0)
                {
                    string trimmedUrl = url.Substring(found + vanillaPalettePath.Length);
                    pal = int.Parse(Regex.Split(trimmedUrl, ".png")[0]);
                    string regionName = string.Empty;
                    //CustomWorldMod.Log($"WWW trimmed path [{trimmedUrl}] Searching for palette [{pal}]");
                    foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegionPacks)
                    {

                        regionName = keyValues.Value;
                        string path = CustomWorldMod.resourcePath + regionName;

                        string paletteFolder = string.Concat(new object[] { Custom.RootFolderDirectory(), path, Path.DirectorySeparatorChar, "Assets", Path.DirectorySeparatorChar, "Futile", Path.DirectorySeparatorChar, "Resources", Path.DirectorySeparatorChar, "Palettes" });
                        //CustomWorldMod.CustomWorldLog($"Custom Regions: Searching palette at {paletteFolder}");

                        if (Directory.Exists(paletteFolder))
                        {
                            string palettePath = paletteFolder + Path.DirectorySeparatorChar + "palette" + pal + ".png";
                            //CustomWorldMod.Log($"Found custom palette directory. Searching palette [{palettePath}]");
                            if (File.Exists(palettePath))
                            {
                                foundPalette = true;
                                //CustomWorldMod.Log($"Loading custom palette [{palettePath}]");
                                url = "file:///" + palettePath;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    CustomWorldMod.Log($"Error loading pallete [{url}]", true);
                }

                if (!foundPalette)
                {
                    if (!File.Exists(vanillaPalettePath+pal+".png"))
                    {
                        CustomWorldMod.Log($"ERROR! Missing pallete [{pal}]", true);
                    }
                }
            }
            orig(self, url);
        }

        /*
        private static void RoomCamera_LoadPalette(On.RoomCamera.orig_LoadPalette orig, RoomCamera self, int pal, ref UnityEngine.Texture2D texture)
        {
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

            // if (pal > 35 && !File.Exists(vanillaPalettePath))
            // {

            string regionName = string.Empty;

            bool foundPalette = false;
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
            {

                regionName = keyValues.Value;
                string path = CustomWorldMod.resourcePath + regionName;

                string paletteFolder = string.Concat(new object[] { Custom.RootFolderDirectory(), path, Path.DirectorySeparatorChar, "Assets", Path.DirectorySeparatorChar, "Futile", Path.DirectorySeparatorChar, "Resources", Path.DirectorySeparatorChar, "Palettes" });
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
                }
            }

            if (!foundPalette)
            {
                CustomWorldMod.Log($"Trying to load vanilla palette [{pal}]");
                if (!File.Exists(vanillaPalettePath))
                {
                    CustomWorldMod.Log($"ERROR! Missing pallete [{pal}]", true);
                }
                orig(self, pal, ref texture);
            }

        }
        */
    }
}
