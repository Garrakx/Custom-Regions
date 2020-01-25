using CustomRegions.Mod;
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
    static class WorldHook
    {
        // This code comes from EasyModPack by topicular
        // Adapted to work with any region by Garrakx


        public static void ApplyHook()
        {
            On.World.LoadMapConfig += World_LoadMapConfig;

            // Debug
            On.World.GetNode += World_GetNode;
        }
         
        private static AbstractRoomNode World_GetNode(On.World.orig_GetNode orig, World self, WorldCoordinate c)
        {
            // this.GetAbstractRoom(c.room).nodes[c.abstractNode];

            if(self.GetAbstractRoom(c.room).nodes == null)
            {
                Debug.Log("Custom Regions: ERROR at GetNode !!! abstractRoomNodes is null"); 
            }
            if(self.GetAbstractRoom(c.room).nodes.Length < 1)
            {
                Debug.Log("Custom Regions: ERROR at GetNode !!! abstractRoomNodes is empty");
            }

            /*
            string debug = $"Custom Regions: Nodes in [{self.GetAbstractRoom(c.room).name}]"+" {";
            for (int i = 0; i < self.GetAbstractRoom(c.room).nodes.Length; i++)
            {
                try
                {
                    debug += self.GetAbstractRoom(c.room).nodes[i] + "/";
                }
                catch (Exception e) { }
            }
            Debug.Log(debug + "}");
            */

            return orig(self, c);
        }

        /// <summary>
        /// Loads MapConfig and Properties from new World in World.
        /// </summary>
        private static void World_LoadMapConfig(On.World.orig_LoadMapConfig orig, World self, int slugcatNumber)
        {
            //if (!enabled) { orig.Invoke(world, slugcatNumber); return; }
            //Debug.Log("-- mapconfig as player: " + slugcatNumber);

            orig(self, slugcatNumber);

            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
            {
                Debug.Log($"Custom Regions: Loading room configurations and properties for {keyValues.Key}");
                string path = CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar;
                string[] array;

                //Mapconfig
                string text = string.Concat(new object[]
                {
                    Custom.RootFolderDirectory(),
                    path.Replace('/', Path.DirectorySeparatorChar),
                    "World",
                    Path.DirectorySeparatorChar,
                    "Regions",
                    Path.DirectorySeparatorChar,
                    self.name,
                    Path.DirectorySeparatorChar,
                    "map_",
                    self.name,
                    ".txt"
                });

                if (File.Exists(text))
                {
                    Debug.Log($"Custom Regions: Loaded custom mapconfig for room {keyValues.Value} as player: {slugcatNumber}");
                    array = File.ReadAllLines(text);
                    for (int i = 0; i < array.Length; i++)
                    {
                        string[] array2 = Regex.Split(array[i], ": ");
                        if (array2.Length == 2)
                        {
                            for (int j = 0; j < self.NumberOfRooms; j++)
                            {
                                if (self.abstractRooms[j].name == array2[0])
                                {
                                    self.abstractRooms[j].mapPos.x = float.Parse(Regex.Split(array2[1], ",")[0]);
                                    self.abstractRooms[j].mapPos.y = float.Parse(Regex.Split(array2[1], ",")[1]);
                                    if (Regex.Split(array2[1], ",").Length >= 5)
                                    {
                                        self.abstractRooms[j].layer = int.Parse(Regex.Split(array2[1], ",")[4]);
                                    }
                                    if (Regex.Split(array2[1], ",").Length >= 6)
                                    {
                                        self.abstractRooms[j].subRegion = int.Parse(Regex.Split(array2[1], ",")[5]);
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
                /*
                else
                {
                    // orig(self, slugcatNumber);
                    // goto properties;
                    
                }
                */


                // Properties.
                text = string.Concat(new object[]
                {
                    Custom.RootFolderDirectory(),
                    path.Replace('/', Path.DirectorySeparatorChar),
                    "World",
                    Path.DirectorySeparatorChar,
                    "Regions",
                    Path.DirectorySeparatorChar,
                    self.name,
                    Path.DirectorySeparatorChar,
                    "Properties.txt"
                });

                if (!File.Exists(text))
                {
                    text = string.Concat(new object[]
                    {
                    Custom.RootFolderDirectory(),
                    "World",
                    Path.DirectorySeparatorChar,
                    "Regions",
                    Path.DirectorySeparatorChar,
                    self.name,
                    Path.DirectorySeparatorChar,
                    "Properties.txt"
                    });

                    if (!File.Exists(text))
                    {
                        return;
                    }
                }

                Debug.Log($"Custom Regions: Loaded custom properties for room {keyValues.Value} as player: {slugcatNumber}");
                array = File.ReadAllLines(text);
                for (int k = 0; k < array.Length; k++)
                {
                    string[] array3 = Regex.Split(array[k], ": ");
                    if (array3.Length == 3)
                    {
                        if (array3[0] == "Room_Attr")
                        {
                            for (int l = 0; l < self.NumberOfRooms; l++)
                            {
                                if (self.abstractRooms[l].name == array3[1])
                                {
                                    string[] array4 = Regex.Split(array3[2], ",");
                                    for (int m = 0; m < array4.Length; m++)
                                    {
                                        if (array4[m] != string.Empty)
                                        {
                                            string[] array5 = Regex.Split(array4[m], "-");
                                            self.abstractRooms[l].roomAttractions[(int)Custom.ParseEnum<CreatureTemplate.Type>(array5[0])] = Custom.ParseEnum<AbstractRoom.CreatureRoomAttraction>(array5[1]);
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        else if (array3[0] == "Broken Shelters" && slugcatNumber == int.Parse(array3[1]))
                        {
                            string[] array4 = Regex.Split(array3[2], ", ");
                            for (int n = 0; n < array4.Length; n++)
                            {
                                if (self.GetAbstractRoom(array4[n]) != null && self.GetAbstractRoom(array4[n]).shelter)
                                {
                                    Debug.Log(string.Concat(new object[]
                                    {
                                "--slugcat ",
                                slugcatNumber,
                                " has a broken shelter at : ",
                                array4[n],
                                " (shelter index ",
                                self.GetAbstractRoom(array4[n]).shelterIndex,
                                ")"
                                    }));
                                    self.brokenShelters[self.GetAbstractRoom(array4[n]).shelterIndex] = true;
                                }
                            }
                        }
                    }
                }

                Vector2 b = new Vector2(float.MaxValue, float.MaxValue);
                for (int num = 0; num < self.NumberOfRooms; num++)
                {
                    if (self.abstractRooms[num].mapPos.x - (float)self.abstractRooms[num].size.x * 3f * 0.5f < b.x)
                    {
                        b.x = self.abstractRooms[num].mapPos.x - (float)self.abstractRooms[num].size.x * 3f * 0.5f;
                    }
                    if (self.abstractRooms[num].mapPos.y - (float)self.abstractRooms[num].size.y * 3f * 0.5f < b.y)
                    {
                        b.y = self.abstractRooms[num].mapPos.y - (float)self.abstractRooms[num].size.y * 3f * 0.5f;
                    }
                }
                for (int num2 = 0; num2 < self.NumberOfRooms; num2++)
                {
                    self.abstractRooms[num2].mapPos -= b;
                }
            }
        }
    }
}
