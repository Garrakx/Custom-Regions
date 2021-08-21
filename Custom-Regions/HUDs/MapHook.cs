using CustomRegions.Mod;
using HUD;
using RWCustom;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CustomRegions.HUDs
{
    static class MapHook
    {
        public static void ApplyHooks()
        {
            On.HUD.Map.Update += Map_Update;
            On.HUD.Map.LoadConnectionPositions += Map_LoadConnectionPositions;
        }

        public static void RemoveHooks()
        {
            On.HUD.Map.Update -= Map_Update;
            On.HUD.Map.LoadConnectionPositions -= Map_LoadConnectionPositions;
        }

        /// <summary>
        /// Loads custom map texture if there is one.
        /// </summary>
        private static void Map_Update(On.HUD.Map.orig_Update orig, HUD.Map self)
        {
            if (self.www == null && self.mapTexture == null)
            {
                foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
                {
                    string pathToMapFile = CRExtras.BuildPath(keyValues.Value, CRExtras.CustomFolder.RegionID, regionID: self.RegionName,
                        file: "map_" + self.RegionName + ".png");

                    if (File.Exists(pathToMapFile))
                    {
                        CustomWorldMod.Log($"Loading map texture from {keyValues.Value} in region [{self.RegionName}]. Path [{pathToMapFile}]");
                        self.www = new WWW("file:///" + pathToMapFile);
                        break;
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

            orig(self);

            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
            {
                string pathToMapFile = CRExtras.BuildPath(keyValues.Value, CRExtras.CustomFolder.RegionID, regionID: self.RegionName,
                        file: "map_" + self.RegionName + ".png");

                if (File.Exists(pathToMapFile))
                {
                    CustomWorldMod.Log($"Loading map data from {keyValues.Key} in region [{self.RegionName}]. Path [{pathToMapFile}]");
                    //self.mapConnections = new List<Map.OnMapConnection>();
                    string[] array = File.ReadAllLines(pathToMapFile);
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
                                    self.mapConnections.Insert(0, (new Map.OnMapConnection(self, num, num2, 
                                        new IntVector2(int.Parse(array3[2]), int.Parse(array3[3])), 
                                        new IntVector2(int.Parse(array3[4]), int.Parse(array3[5])), int.Parse(array3[6]), int.Parse(array3[7]))) );
                                }
                            }
                        }
                    }
                    break;
                }
            }

            if (self.mapConnections == null)
            {
                CustomWorldMod.Log($"ERROR! No map found for {self.RegionName}");

            }

        }
    }
}
