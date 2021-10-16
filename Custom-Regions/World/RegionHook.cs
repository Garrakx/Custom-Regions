using CustomRegions.Mod;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace CustomRegions.CWorld
{
    static class RegionHook
    {
        public static void ApplyHooks()
        {
            On.Region.NumberOfRoomsInRegion += Region_NumberOfRoomsInRegion;
            On.Region.ctor += Region_ctor;
        }

        public static string GetSubRegionName(string packName, string regID)
        {
            string propertiesPath = CRExtras.BuildPath(packName, CRExtras.CustomFolder.RegionID, regionID: regID, file: "Properties.txt");
            if (File.Exists(propertiesPath))
            {
                string[] array = File.ReadAllLines(propertiesPath);
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
        /// Loads new Properties for Region. Templates and subregions are merged
        /// </summary>
        private static void Region_ctor(On.Region.orig_ctor orig, Region self, string name, int firstRoomIndex, int regionNumber)
        {
            CustomWorldMod.Log($"Creating region {name}", false, CustomWorldMod.DebugLevel.FULL);
            orig(self, name, firstRoomIndex, regionNumber);
            string[] vanillaTemplateNames = (string[])self.roomSettingTemplateNames.Clone();

            self.roomSettingTemplateNames = null;
            self.roomSettingsTemplates = null;
            List<string> currentTemplateNames = null;

            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
            {
                CustomWorldMod.Log($"Loading custom Properties for {keyValues.Key}", false, CustomWorldMod.DebugLevel.FULL);

                string propertiesFilePath = CRExtras.BuildPath(keyValues.Value, CRExtras.CustomFolder.RegionID, regionID: name, file: "Properties.txt");

                if (File.Exists(propertiesFilePath))
                {
                    CustomWorldMod.Log($"Found custom properties for {keyValues.Key}", false, CustomWorldMod.DebugLevel.MEDIUM);
                    string[] array = File.ReadAllLines(propertiesFilePath);
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
                                    // Merging room template
                                    case 0:
                                        {
                                            string[] array2 = Regex.Split(Regex.Split(array[i], ": ")[1], ", ");
                                            if (array2 == null)
                                            {
                                                CustomWorldMod.Log($"Corrupted properties file [{propertiesFilePath}]", true);
                                                break;
                                            }
                                            int previousIndex = 0;

                                            // First region pack, init arrays
                                            if (self.roomSettingsTemplates == null)
                                            {
                                                CustomWorldMod.Log($"Creating templates for [{keyValues.Key}]", false, CustomWorldMod.DebugLevel.FULL);
                                                self.roomSettingsTemplates = new RoomSettings[array2.Length];
                                                self.roomSettingTemplateNames = new string[array2.Length];
                                            }
                                            else
                                            {
                                                CustomWorldMod.Log($"Extending templates for [{keyValues.Key}]. " +
                                                    $"Previous index [{previousIndex}], " +
                                                    $"new size [{self.roomSettingsTemplates.Length + array2.Length}]", false, CustomWorldMod.DebugLevel.FULL);

                                                previousIndex = self.roomSettingTemplateNames.Length - 1;
                                                System.Array.Resize(ref self.roomSettingTemplateNames,
                                                    self.roomSettingTemplateNames.Length + array2.Length);

                                                System.Array.Resize(ref self.roomSettingsTemplates,
                                                    self.roomSettingsTemplates.Length + array2.Length);
                                            }

                                            currentTemplateNames = new List<string>(self.roomSettingTemplateNames);
                                         
                                            for (int j = 0; j < array2.Length; j++)
                                            {
                                                string newTemplate = array2[j];
                                                if (!currentTemplateNames.Contains(newTemplate))
                                                {
                                                    CustomWorldMod.Log($"Adding new custom templates [{newTemplate}] at ({j + previousIndex}) for [{keyValues.Key}]",
                                                        false, CustomWorldMod.DebugLevel.FULL);

                                                    self.roomSettingTemplateNames[previousIndex + j] = newTemplate;
                                                    self.ReloadRoomSettingsTemplate(newTemplate);
                                                }
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


            // Add vanilla templates
            if (self.roomSettingsTemplates != null)
            {
                // merge
                currentTemplateNames = new List<string>(self.roomSettingTemplateNames);

            }
            else
            {
                CustomWorldMod.Log($"Loading vanilla templates for [{self.name}]...", false, CustomWorldMod.DebugLevel.MEDIUM);
                // load vanilla
                currentTemplateNames = new List<string>();
                self.roomSettingsTemplates = new RoomSettings[vanillaTemplateNames.Length];
                self.roomSettingTemplateNames = new string[vanillaTemplateNames.Length];
            }

            for (int j = 0; j < vanillaTemplateNames.Length; j++)
            {
                string newTemplate = vanillaTemplateNames[j];
                if (!currentTemplateNames.Contains(newTemplate))
                {
                    self.roomSettingTemplateNames[j] = newTemplate;
                    self.ReloadRoomSettingsTemplate(newTemplate);
                }
            }
            CustomWorldMod.Log($"Loaded setting templates: [{string.Join(", ", self.roomSettingTemplateNames)}]", false, CustomWorldMod.DebugLevel.MEDIUM);
        }

        /// <summary>
        /// How many new rooms in region, read from world file
        /// </summary>
        private static int Region_NumberOfRoomsInRegion(On.Region.orig_NumberOfRoomsInRegion orig, string name)
        {
            bool customRegion = false;
            int totalRooms = 0;

            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
            {
                string worldFilePath = CRExtras.BuildPath(keyValues.Value, CRExtras.CustomFolder.RegionID, regionID: name, file: "world_" + name + ".txt");
                if (File.Exists(worldFilePath))
                {
                    customRegion = true;
                    string[] array = File.ReadAllLines(worldFilePath);
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
                    totalRooms += num;
                }
            }

            if (customRegion)
            {
                return totalRooms;
            }

            return orig(name);

        }
    }
}
