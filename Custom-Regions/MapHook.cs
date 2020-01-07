using CustomRegions.Mod;
using HUD;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CustomRegions
{
    static class MapHook
    {
        // This code comes from EasyModPack by topicular
        // Adapted to work with any region by Garrakx


        public static void ApplyHook()
        {
            On.HUD.Map.Update += Map_Update;
            On.HUD.Map.LoadConnectionPositions += Map_LoadConnectionPositions;
        }

        /// <summary>
        /// Loads custom map texture if there is one.
        /// </summary>
        private static void Map_Update(On.HUD.Map.orig_Update orig, HUD.Map self)
        {
            if (self.www == null)
            {
                if (self.mapTexture == null)
                {
                    foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
                    {
                        Debug.Log($"Custom Regions: Loading map texture {keyValues.Value} in {self.RegionName}");
                        string path = CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar;

                        string test = string.Concat(new object[]
                        {
                        Custom.RootFolderDirectory(),
                        path.Replace('/', Path.DirectorySeparatorChar),
                        "World",
                        Path.DirectorySeparatorChar,
                        "Regions",
                        Path.DirectorySeparatorChar,
                        self.RegionName,
                        Path.DirectorySeparatorChar,
                        "map_",
                        self.RegionName,
                        ".png"
                        });
                        if (File.Exists(test))
                        {
                            self.www = new WWW("file:///" + test);
                        }
                    }
                }
            }
            orig(self);
        }


        /// <summary>
        /// Loads custom map connections if there is one.
        /// </summary>
        private static void Map_LoadConnectionPositions(On.HUD.Map.orig_LoadConnectionPositions orig, HUD.Map self)
        {

            bool customMap = false;

            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
            {
                Debug.Log($"Custom Regions: Counting total rooms for {keyValues.Key} in {self.RegionName}");
                string path = CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar;
                string test = string.Concat(new object[]
                {
                Custom.RootFolderDirectory(),
                path.Replace('/', Path.DirectorySeparatorChar),
                "World",
                Path.DirectorySeparatorChar,
                "Regions",
                Path.DirectorySeparatorChar,
                self.RegionName,
                Path.DirectorySeparatorChar,
                "map_",
                self.RegionName,
                ".txt"
                });
                if (File.Exists(test))
                {
                    customMap = true;
                    self.mapConnections = new List<Map.OnMapConnection>();
                    string[] array = File.ReadAllLines(test);
                    for (int i = 0; i < array.Length; i++)
                    {
                        string[] array2 = Regex.Split(array[i], ": ");
                        if (array2.Length == 2 && array2[0] == "Connection")
                        {
                            string[] array3 = Regex.Split(array2[1], ",");
                            if (array3.Length == 8)
                            {
                                int num = -1;
                                int num2 = -1;
                                int num3 = self.mapData.firstRoomIndex;
                                while (num3 < self.mapData.firstRoomIndex + self.mapData.roomSizes.Length && (num < 0 || num2 < 0))
                                {
                                    if (self.mapData.NameOfRoom(num3) == array3[0])
                                    {
                                        num = num3;
                                    }
                                    else if (self.mapData.NameOfRoom(num3) == array3[1])
                                    {
                                        num2 = num3;
                                    }
                                    num3++;
                                }
                                if (num > 0 && num2 > 0)
                                {
                                    self.mapConnections.Add(new Map.OnMapConnection(self, num, num2, new IntVector2(int.Parse(array3[2]), int.Parse(array3[3])), new IntVector2(int.Parse(array3[4]), int.Parse(array3[5])), int.Parse(array3[6]), int.Parse(array3[7])));
                                }
                            }
                        }
                    }
                }
            }

            if (!customMap)
            {
                orig(self);
            }
        }
    }
}
