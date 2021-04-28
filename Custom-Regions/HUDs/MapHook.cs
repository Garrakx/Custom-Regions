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

        public static void ApplyHook()
        {
            On.HUD.Map.Update += Map_Update;
            On.HUD.Map.LoadConnectionPositions += Map_LoadConnectionPositions;

            On.HUD.Map.MapData.KarmaOfGate += MapData_KarmaOfGate;
        }


        private static int MapData_KarmaOfGate(On.HUD.Map.MapData.orig_KarmaOfGate orig, Map.MapData self, PlayerProgression progression, World initWorld, string roomName)
        {
            // Gotta scan it all, progression was loaded on game-start and doesnt account for enabled/disabled regions ?
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
            {
                CustomWorldMod.Log($"Custom Regions: Loading KarmaOfGate for {keyValues.Key}", false, CustomWorldMod.DebugLevel.FULL);
                string path = CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar;
                string path2 = path + "World" + Path.DirectorySeparatorChar + "Gates" + Path.DirectorySeparatorChar + "locks.txt";
                if (File.Exists(path2))
                {
                    string[] array = File.ReadAllLines(path2);

                    for (int i = 0; i < array.Length; i++)
                    {
                        string[] array2 = Regex.Split(array[i], " : ");
                        if (array2[0] == roomName)
                        {
                            string req1 = array2[1];
                            string req2 = array2[2];
                            int result;
                            int result2;
                            result = Custom.IntClamp(int.Parse(req1) - 1, 0, 4);
                            result2 = Custom.IntClamp(int.Parse(req2) - 1, 0, 4);

                            bool flipped = false;
                            if (roomName == "GATE_LF_SB" || roomName == "GATE_DS_SB" || roomName == "GATE_HI_CC" || roomName == "GATE_SS_UW")
                            {
                                flipped = true;
                            }

                            CustomWorldMod.Log($"Custom Regions: Found custom KarmaOfGate for {keyValues.Key}. Gate [{result}/{result2}]");

                            string[] namearray = Regex.Split(roomName, "_");

                            if (namearray[1] == initWorld.region.name != flipped)
                            {
                                return result;
                            }
                            return result2;
                        }
                    }
                }
            }
            return orig(self, progression, initWorld, roomName);
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
                    string pathToCustomFolder = Custom.RootFolderDirectory() + CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar;
                    string test = pathToCustomFolder + "World" + Path.DirectorySeparatorChar + "Regions" + Path.DirectorySeparatorChar +
                        self.RegionName + Path.DirectorySeparatorChar + "map_" + self.RegionName + ".png";

                    if (File.Exists(test))
                    {
                        CustomWorldMod.Log($"Loading map texture from {keyValues.Value} in region [{self.RegionName}]. Path [{test}]");
                        self.www = new WWW("file:///" + test);
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
            //List<Map.OnMapConnection> backUpConnections = self.mapConnections;

            orig(self);

            //self.mapConnections = new List<Map.OnMapConnection>();
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
            {
                string pathToCustomFolder = Custom.RootFolderDirectory() + CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar;
                string test = pathToCustomFolder + "World" + Path.DirectorySeparatorChar + "Regions" + Path.DirectorySeparatorChar +
                    self.RegionName + Path.DirectorySeparatorChar + "map_" + self.RegionName + ".txt";

                if (File.Exists(test))
                {
                    CustomWorldMod.Log($"Loading map data from {keyValues.Key} in region [{self.RegionName}]. Path [{test}]");
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
