using CustomRegions.Mod;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using static CustomRegions.Mod.CustomWorldStructs;

namespace CustomRegions
{
    public static class PlayerProgressionHook
    {
     
        public static void ApplyHooks()
        {
            On.PlayerProgression.LoadProgression += PlayerProgression_LoadProgression;
            On.PlayerProgression.InitiateProgression += PlayerProgression_InitiateProgression;

            On.PlayerProgression.MiscProgressionData.SaveDiscoveredShelter += MiscProgressionData_SaveDiscoveredShelter;
            On.PlayerProgression.MiscProgressionData.ToString += MiscProgressionData_ToString;
            On.PlayerProgression.MiscProgressionData.FromString += MiscProgressionData_FromString;

            On.PlayerProgression.SaveToDisk += PlayerProgression_SaveToDisk;
            On.PlayerProgression.WipeAll += PlayerProgression_WipeAll;
        }

        public static void RemoveHooks()
        {
            On.PlayerProgression.LoadProgression -= PlayerProgression_LoadProgression;
            On.PlayerProgression.InitiateProgression -= PlayerProgression_InitiateProgression;

            // Debug
            On.PlayerProgression.MiscProgressionData.SaveDiscoveredShelter -= MiscProgressionData_SaveDiscoveredShelter;
            On.PlayerProgression.MiscProgressionData.ToString -= MiscProgressionData_ToString;
            On.PlayerProgression.MiscProgressionData.FromString -= MiscProgressionData_FromString;

            On.PlayerProgression.SaveToDisk -= PlayerProgression_SaveToDisk;
            On.PlayerProgression.WipeAll -= PlayerProgression_WipeAll;
        }

        private static void PlayerProgression_WipeAll(On.PlayerProgression.orig_WipeAll orig, PlayerProgression self)
        {
            orig(self);
            string saveFileName = Custom.RootFolderDirectory() + CustomWorldMod.regionSavePath + $"CRsav_{self.rainWorld.options.saveSlot + 1}.txt";
            CustomWorldMod.Log($"Clearing CR save (data path [{saveFileName}])");
            if (File.Exists(saveFileName))
            {
                File.Delete(saveFileName);
                try
                {
                    CustomWorldMod.packInfoInSaveSlot[self.rainWorld.options.saveSlot].Clear();
                } catch (Exception) { }

                CustomWorldMod.Log("Deleted CR save");
                CustomWorldMod.ReadSaveAnalyzerFiles();
                CustomWorldMod.AnalyzeSave();
            }
        }

        private static void PlayerProgression_SaveToDisk(On.PlayerProgression.orig_SaveToDisk orig, PlayerProgression self, bool saveCurrentState, bool saveMaps, bool saveMiscProg)
        {
            // Check if first time saved
            string saveFileName = Custom.RootFolderDirectory() + CustomWorldMod.regionSavePath + $"CRsav_{self.rainWorld.options.saveSlot + 1}.txt";
            CustomWorldMod.Log($"CR save data path [{saveFileName}]");
            if (!File.Exists(saveFileName))
            {
                string saveRegionData = string.Empty;
                //dictionaryString += $"{ string.Join(", ", new List<string>(CustomWorldMod.loadedRegions.Values).ToArray())}" + "}";
                //saveRegionData += $"{CustomWorldMod.saveDividerA}REGIONLIST{string.Join(",",CustomWorldMod.loadedRegions.Keys.ToArray())}{CustomWorldMod.saveDividerA}";

                foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
                {
                    if (CustomWorldMod.installedPacks.TryGetValue(keyValues.Key, out RegionPack regionInfo))
                    {
                        saveRegionData += CustomWorldMod.SerializeRegionInfo(regionInfo);
                    }    
                }

                // WRITE FILE
                using (StreamWriter streamWriter = File.CreateText(saveFileName))
                {
                    CustomWorldMod.Log($"Creating save log [{saveRegionData}]");
                    streamWriter.Write(Custom.Md5Sum(saveRegionData) + saveRegionData);
                }
            }

            orig(self, saveCurrentState, saveMaps, saveMiscProg);
        }

        public static void UpdateProgresionCRS(PlayerProgression self)
        {
            self.regionNames = CustomWorldMod.AddModdedRegions(self.regionNames);

            if (self.regionNames.Length != self.mapDiscoveryTextures.Length)
            {
                Array.Resize(ref self.mapDiscoveryTextures, self.regionNames.Length);
                CustomWorldMod.Log($"Custom Regions: Resizing mapDiscovery in PlayerProgression.");
            }
            self.miscProgressionData.discoveredShelters = new List<string>[self.regionNames.Length];

            // Karma locks
            List<string> tempLocks = new List<string>(self.karmaLocks);
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
            {
                CustomWorldMod.Log($"Custom Regions: Loading karmaGate requirement for {keyValues.Key}", false, CustomWorldMod.DebugLevel.FULL);
                string path = CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar;
                string path2 = path + "World" + Path.DirectorySeparatorChar + "Gates" + Path.DirectorySeparatorChar + "locks.txt";

                if (File.Exists(path2))
                {
                    foreach (string line in File.ReadAllLines(path2))
                    {
                        if (!tempLocks.Contains(line))
                            tempLocks.Insert(0, line);
                    }
                }
                else
                {
                    CustomWorldMod.Log($"Custom Regions: {keyValues.Key} does not contain a locks.txt file", false, CustomWorldMod.DebugLevel.MEDIUM);
                }
            }

            self.karmaLocks = tempLocks.ToArray();
            CustomWorldMod.Log($"Loaded karmaGate requirements [{string.Join(", ", self.karmaLocks)}]", false, CustomWorldMod.DebugLevel.MEDIUM);
            
        }

        private static void PlayerProgression_InitiateProgression(On.PlayerProgression.orig_InitiateProgression orig, PlayerProgression self)
        {
            UpdateProgresionCRS(self);
            orig(self);
        }

        private static void PlayerProgression_LoadProgression(On.PlayerProgression.orig_LoadProgression orig, PlayerProgression self)
        {
            UpdateProgresionCRS(self);
            orig(self);
        }

        private static void MiscProgressionData_FromString(On.PlayerProgression.MiscProgressionData.orig_FromString orig, PlayerProgression.MiscProgressionData self, string s)
        {
            CustomWorldMod.Log($"MISC PROGRESSION FROM STRING - Region Names [{string.Join(", ", self.owner.regionNames)}]");

            Dictionary<string, int> dictionaryTemp = new Dictionary<string, int>(7);
            string[] array = Regex.Split(s, "<mpdA>");

            string debug = "Loaded shelters from string: [ ";
            for (int i = 0; i < array.Length; i++)
            {
                string[] array2 = Regex.Split(array[i], "<mpdB>");
                string text = array2[0];
                if (text != null)
                {
                    if (dictionaryTemp == null)
                    {
                        Dictionary<string, int> dictionary = new Dictionary<string, int>(7)
                        {
                            { "SHELTERLIST", 0 },
                            { "MENUREGION", 1 },
                            { "LEVELTOKENS", 2 },
                            { "SANDBOXTOKENS", 3 },
                            { "INTEGERS", 4 },
                            { "PLAYEDARENAS", 5 },
                            { "REDSFLOWER", 6 }
                        };
                        dictionaryTemp = dictionary;
                    }
                    int num;
                    if (dictionaryTemp.TryGetValue(text, out num))
                    {
                        switch (num)
                        {
                            case 0:
                                {
                                    int num2 = -1;
                                    for (int j = 0; j < self.owner.regionNames.Length; j++)
                                    {
                                        if (self.owner.regionNames[j] == array2[1])
                                        {
                                            num2 = j;
                                            break;
                                        }
                                    }
                                    if (num2 > -1)
                                    {
                                        // self.discoveredShelters[num2] = new List<string>();
                                        string[] array3 = Regex.Split(array2[2], "<mpdC>");
                                        for (int k = 0; k < array3.Length; k++)
                                        {
                                            if (array3[k].Length > 0)
                                            {
                                                // self.discoveredShelters[num2].Add(array3[k]);
                                                //shelters.Add(array3[k]);
                                                for (int h = 0; h < array3[k].Length; h++)
                                                {
                                                    debug += array3[h] + " ";
                                                }
                                            }
                                        }
                                    }
                                    //myStringOutput = String.Join(",", array2[2].Select(p => p.ToString()).ToArray());
                                    break;
                                }
                        }
                    }
                }
            }
            debug += " ]";
            CustomWorldMod.Log(debug);
            orig(self, s);

            string debug2 = "Discovered Shelters { ";
            for (int i = 0; i < self.discoveredShelters.Length; i++)
            {
                if (self.discoveredShelters[i] != null)
                {
                    for (int x = 0; x < self.discoveredShelters[i].Count; x++)
                    {

                        if (self.discoveredShelters[i][x] != null)
                        {
                            debug += self.discoveredShelters[i][x] + " , ";
                        }
                    }
                }
            }
            CustomWorldMod.Log(debug2 + "} ");
        }

        private static string MiscProgressionData_ToString(On.PlayerProgression.MiscProgressionData.orig_ToString orig, PlayerProgression.MiscProgressionData self)
        {
            string text = "MiscProgdata [";
            for (int i = 0; i < self.discoveredShelters.Length; i++)
            {
                if (self.discoveredShelters[i] != null && self.discoveredShelters[i].Count > 0)
                {
                    text = text + "SHELTERLIST<mpdB>" + self.owner.regionNames[i] + "<mpdB>";
                    for (int j = 0; j < self.discoveredShelters[i].Count; j++)
                    {
                        text = text + self.discoveredShelters[i][j] + ((j >= self.discoveredShelters[i].Count - 1) ? string.Empty : "<mpdC>");
                    }
                    text += "<mpdA>";
                }
            }
            CustomWorldMod.Log(text + "] ");
            return orig(self);

        }

        private static void MiscProgressionData_SaveDiscoveredShelter(On.PlayerProgression.MiscProgressionData.orig_SaveDiscoveredShelter orig, PlayerProgression.MiscProgressionData self, string roomName)
        {
            CustomWorldMod.Log($"Custom Regions: Save Discovered Shelter [{roomName}]. ");
            string debug = "Custom Regions: RegionNames { ";
            int num = -1;
            for (int i = 0; i < self.owner.regionNames.Length; i++)
            {
                debug += self.owner.regionNames[i] + " , ";
                if (self.owner.regionNames[i] == roomName.Substring(0, 2))
                {
                    num = i;
                    break;
                }
            }
            if (num == -1)
            {
                debug += "\n ERROR! region not found";
            }
           // CustomWorldMod.Log(debug);
            if (self.discoveredShelters[num] == null)
            {
                self.discoveredShelters[num] = new List<string>();
            }
            for (int j = 0; j < self.discoveredShelters[num].Count; j++)
            {
                if (self.discoveredShelters[num][j] == roomName)
                {
                    CustomWorldMod.Log("Custom Regions: Save shelter ERROR, already saved");
                }
            }
            orig(self, roomName);

            string debug2 = "Custom Regions: Discovered Shelters { ";
            for (int i = 0; i < self.discoveredShelters.Length; i++)
            {
                if (self.discoveredShelters[i] != null)
                {
                    for (int x = 0; x < self.discoveredShelters[i].Count; x++)
                    {

                        if (self.discoveredShelters[i][x] != null)
                        {
                            debug += self.discoveredShelters[i][x] + " , ";
                        }
                    }
                }
            }
            CustomWorldMod.Log("} " + debug2);
        }
    }
}
