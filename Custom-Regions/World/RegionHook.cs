using CustomRegions.Mod;
using RWCustom;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace CustomRegions.CWorld
{
    static class RegionHook
    {
        public static void ApplyHook()
        {
            On.Region.NumberOfRoomsInRegion += Region_NumberOfRoomsInRegion;
            On.Region.ctor += Region_ctor;
        }


        public static string GetSubRegionName(string packName, string regID)
        {
            //CustomWorldMod.CustomWorldLog($"Custom Regions: Loading custom properties for {keyValues.Key}");
            string path = CustomWorldMod.resourcePath + packName + Path.DirectorySeparatorChar;

            string test = string.Concat(new object[]
            {
                Custom.RootFolderDirectory(),
                path.Replace('/', Path.DirectorySeparatorChar),
                "World",
                Path.DirectorySeparatorChar,
                "Regions",
                Path.DirectorySeparatorChar,
                regID,
                Path.DirectorySeparatorChar,
                "properties.txt"
            });
            if (File.Exists(test))
            {
                string[] array = File.ReadAllLines(test);
                for (int i = 0; i < array.Length; i++)
                {
                    string text = Regex.Split(array[i], ": ")[0];
                    if (text != null)
                    {
                        if (text.Equals("Subregion"))
                        {
                            return Regex.Split(array[i], ": ")[1];
                        }
                    }
                }

            }

            return null;
        }

        /// <summary>
        /// Holds the properties of the region
        /// </summary>
        public static Dictionary<string, int> dictionaryProperties;

        /// <summary>
        /// Loads new Properties for Region.
        /// </summary>
        private static void Region_ctor(On.Region.orig_ctor orig, Region self, string name, int firstRoomIndex, int regionNumber)
        {
            orig(self, name, firstRoomIndex, regionNumber);

            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
            {
                //CustomWorldMod.CustomWorldLog($"Custom Regions: Loading custom properties for {keyValues.Key}");
                string path = CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar;

                string test = string.Concat(new object[]
                {
                Custom.RootFolderDirectory(),
                path.Replace('/', Path.DirectorySeparatorChar),
                "World",
                Path.DirectorySeparatorChar,
                "Regions",
                Path.DirectorySeparatorChar,
                name,
                Path.DirectorySeparatorChar,
                "properties.txt"
                });
                if (File.Exists(test))
                {
                    //CustomWorldMod.CustomWorldLog($"Custom Regions: Found custom properties for {keyValues.Key}");
                    string[] array = File.ReadAllLines(test);
                    for (int i = 0; i < array.Length; i++)
                    {
                        string text = Regex.Split(array[i], ": ")[0];
                        if (text != null)
                        {
                            if (RegionHook.dictionaryProperties == null)
                            {
                                Dictionary<string, int> dictionary = new Dictionary<string, int>(15);
                                dictionary.Add("Room Setting Templates", 0);
                                dictionary.Add("batDepleteCyclesMin", 1);
                                dictionary.Add("batDepleteCyclesMax", 2);
                                dictionary.Add("batDepleteCyclesMaxIfLessThanTwoLeft", 3);
                                dictionary.Add("batDepleteCyclesMaxIfLessThanFiveLeft", 4);
                                dictionary.Add("overseersSpawnChance", 5);
                                dictionary.Add("overseersMin", 6);
                                dictionary.Add("overseersMax", 7);
                                dictionary.Add("playerGuideOverseerSpawnChance", 8);
                                dictionary.Add("scavsMin", 9);
                                dictionary.Add("scavsMax", 10);
                                dictionary.Add("scavsSpawnChance", 11);
                                dictionary.Add("Subregion", 12);
                                dictionary.Add("batsPerActiveSwarmRoom", 13);
                                dictionary.Add("batsPerInactiveSwarmRoom", 14);
                                RegionHook.dictionaryProperties = dictionary;
                            }
                            if (RegionHook.dictionaryProperties.TryGetValue(text, out int num))
                            {
                                switch (num)
                                {
                                    case 0:
                                        {
                                            string[] array2 = Regex.Split(Regex.Split(array[i], ": ")[1], ", ");
                                            self.roomSettingsTemplates = new RoomSettings[array2.Length];
                                            self.roomSettingTemplateNames = new string[array2.Length];
                                            for (int j = 0; j < array2.Length; j++)
                                            {
                                                self.roomSettingTemplateNames[j] = array2[j];
                                                self.ReloadRoomSettingsTemplate(array2[j]);
                                                //CustomWorldMod.CustomWorldLog("Custom reload: " + self.roomSettingTemplateNames[j]);
                                                //CustomWorldMod.CustomWorldLog(self.roomSettingsTemplates[j].RandomItemDensity.ToString() + " " + self.roomSettingsTemplates[j].RandomItemSpearChance.ToString());
                                            }
                                            break;
                                        }
                                    case 1:
                                        self.regionParams.batDepleteCyclesMin = int.Parse(Regex.Split(array[i], ": ")[1]);
                                        break;
                                    case 2:
                                        self.regionParams.batDepleteCyclesMax = int.Parse(Regex.Split(array[i], ": ")[1]);
                                        break;
                                    case 3:
                                        self.regionParams.batDepleteCyclesMaxIfLessThanTwoLeft = int.Parse(Regex.Split(array[i], ": ")[1]);
                                        break;
                                    case 4:
                                        self.regionParams.batDepleteCyclesMaxIfLessThanFiveLeft = int.Parse(Regex.Split(array[i], ": ")[1]);
                                        break;
                                    case 5:
                                        self.regionParams.overseersSpawnChance = float.Parse(Regex.Split(array[i], ": ")[1]);
                                        break;
                                    case 6:
                                        self.regionParams.overseersMin = int.Parse(Regex.Split(array[i], ": ")[1]);
                                        break;
                                    case 7:
                                        self.regionParams.overseersMax = int.Parse(Regex.Split(array[i], ": ")[1]);
                                        break;
                                    case 8:
                                        self.regionParams.playerGuideOverseerSpawnChance = float.Parse(Regex.Split(array[i], ": ")[1]);
                                        break;
                                    case 9:
                                        self.regionParams.scavsMin = int.Parse(Regex.Split(array[i], ": ")[1]);
                                        break;
                                    case 10:
                                        self.regionParams.scavsMax = int.Parse(Regex.Split(array[i], ": ")[1]);
                                        break;
                                    case 11:
                                        self.regionParams.scavsSpawnChance = (float)int.Parse(Regex.Split(array[i], ": ")[1]);
                                        break;
                                    case 12:
                                        self.subRegions.Add(Regex.Split(array[i], ": ")[1]);
                                        break;
                                    case 13:
                                        self.regionParams.batsPerActiveSwarmRoom = int.Parse(Regex.Split(array[i], ": ")[1]);
                                        break;
                                    case 14:
                                        self.regionParams.batsPerInactiveSwarmRoom = int.Parse(Regex.Split(array[i], ": ")[1]);
                                        break;
                                }
                            }
                        }
                    }
                }
            }

        }

        /// <summary>
        /// How many new rooms in region, read from properties
        /// </summary>
        private static int Region_NumberOfRoomsInRegion(On.Region.orig_NumberOfRoomsInRegion orig, string name)
        {
            //if (!enabled) { return orig(self); }
            bool customRegion = false;
            int totalRooms = 0;
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
            {
                //CustomWorldMod.CustomWorldLog($"Custom Regions: Counting total rooms for {keyValues.Value} in {name}");
                string path = CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar;

                string test = string.Concat(new object[]
                {
                Custom.RootFolderDirectory(),
                path.Replace('/', Path.DirectorySeparatorChar),
                "World",
                Path.DirectorySeparatorChar,
                "Regions",
                Path.DirectorySeparatorChar,
                name,
                Path.DirectorySeparatorChar,
                "world_",
                name,
                ".txt"
                });
                if (File.Exists(test))
                {
                    customRegion = true;
                    string[] array = File.ReadAllLines(test);
                    bool flag = false;
                    int num = 1;
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (array[i] == "END ROOMS")
                        {
                            break;
                        }
                        if (flag)
                        {
                            num++;
                        }
                        if (array[i] == "ROOMS")
                        {
                            flag = true;
                        }
                    }
                    //CustomWorldMod.CustomWorldLog($"Custom Regions: {keyValues.Value} had {num} rooms-connections in region [{name}].");
                    totalRooms += num;
                }
            }

            if (customRegion)
            {
                return totalRooms;
            }

            return orig(name);
            /*
            else
            {
                return orig(name);
            }
            */

        }
    }
}
