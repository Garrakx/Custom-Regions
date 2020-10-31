using CustomRegions.Mod;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CustomRegions
{
    public static class PlayerProgressionHook
    {
        private static Dictionary<string, int> tempDictionary = null;

        public static void ApplyHooks()
        {
            On.PlayerProgression.LoadProgression += PlayerProgression_LoadProgression;
            On.PlayerProgression.InitiateProgression += PlayerProgression_InitiateProgression;

            // Fix Savefile
            // On.PlayerProgression.GetProgLines += PlayerProgression_GetProgLines;

            // Debug
            On.PlayerProgression.MiscProgressionData.SaveDiscoveredShelter += MiscProgressionData_SaveDiscoveredShelter;
            On.PlayerProgression.MiscProgressionData.ToString += MiscProgressionData_ToString;
            On.PlayerProgression.MiscProgressionData.FromString += MiscProgressionData_FromString;

            On.PlayerProgression.SaveToDisk += PlayerProgression_SaveToDisk;
        }

        private static void PlayerProgression_SaveToDisk(On.PlayerProgression.orig_SaveToDisk orig, PlayerProgression self, bool saveCurrentState, bool saveMaps, bool saveMiscProg)
        {
            // Check if first time saved
            string saveFileName = Custom.RootFolderDirectory() + CustomWorldMod.regionSavePath + $"CRsav_{self.rainWorld.options.saveSlot + 1}.txt";
            CustomWorldMod.CustomWorldLog($"CR save data path [{saveFileName}]");
            if (!File.Exists(saveFileName))
            {
                string saveRegionData = string.Empty;
                //dictionaryString += $"{ string.Join(", ", new List<string>(CustomWorldMod.loadedRegions.Values).ToArray())}" + "}";
                //saveRegionData += $"{CustomWorldMod.saveDividerA}REGIONLIST{string.Join(",",CustomWorldMod.loadedRegions.Keys.ToArray())}{CustomWorldMod.saveDividerA}";

                foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
                {
                    CustomWorldMod.RegionInformation regionInfo;
                    if (CustomWorldMod.availableRegions.TryGetValue(keyValues.Key, out regionInfo))
                    {
                        saveRegionData += CustomWorldMod.SerializeRegionInfo(regionInfo);
                    }    
                }

                // WRITE FILE
                using (StreamWriter streamWriter = File.CreateText(saveFileName))
                {
                    CustomWorldMod.CustomWorldLog($"Creating save log [{saveRegionData}]");
                    streamWriter.Write(Custom.Md5Sum(saveRegionData) + saveRegionData);
                }
            }

            orig(self, saveCurrentState, saveMaps, saveMiscProg);
        }

        // Debug
        private static string[] PlayerProgression_GetProgLines(On.PlayerProgression.orig_GetProgLines orig, PlayerProgression self)
        {
            tempDictionary = null;
            string[] progLines = orig(self);
            string path = Custom.RootFolderDirectory() + "SavedList.txt";
            for (int i = 0; i < progLines.Length; i++)
            {
                string[] array = Regex.Split(progLines[i], "<progDivB>");
                if (array.Length == 2 && array[0] == "SAVE STATE")
                {
                    List<string> saveDataList = array.ToList<string>();
                    List<string> updatedSaveDataList = saveDataList;

                    foreach (string s in saveDataList)
                    {
                        if (!File.Exists(path))
                        {
                            using (TextWriter tw = new StreamWriter(path))
                            {
                                tw.WriteLine(s);
                            }
                        }
                        else
                        {
                            using (StreamWriter tw = File.AppendText(path))
                            {
                                tw.WriteLine(s);
                            }
                        }

                        if (s.Equals("MAP"))
                        {
                            int index = saveDataList.IndexOf(s);
                            string regionName = string.Empty;
                            try
                            {
                                regionName = saveDataList[index++];
                            }
                            catch (Exception e) { CustomWorldMod.CustomWorldLog($"Custom Regions: Exception at fixing savefile {e}"); }

                            if (regionName == string.Empty)
                                continue;

                            if (!self.regionNames.ToList<string>().Contains(regionName))
                            {
                                CustomWorldMod.CustomWorldLog($"Custom Regions: fixing SAVE STATE file. Uninstalled region [{regionName}], clearing saveData...");
                            }
                        }
                    }
                }
            }
            return progLines;
        }



        private static void PlayerProgression_InitiateProgression(On.PlayerProgression.orig_InitiateProgression orig, PlayerProgression self)
        {
            self.regionNames = CustomWorldMod.AddModdedRegions(self.regionNames);
            if (self.regionNames.Length != self.mapDiscoveryTextures.Length)
            {
                Array.Resize(ref self.mapDiscoveryTextures, self.regionNames.Length);
                CustomWorldMod.CustomWorldLog($"Custom Regions: Resizing mapDiscovery in PlayerProgression.");
            }
            self.miscProgressionData.discoveredShelters = new List<string>[self.regionNames.Length];
            orig(self);
        }

        private static void PlayerProgression_LoadProgression(On.PlayerProgression.orig_LoadProgression orig, PlayerProgression self)
        {
            self.regionNames = CustomWorldMod.AddModdedRegions(self.regionNames);
            if (self.regionNames.Length != self.mapDiscoveryTextures.Length)
            {
                Array.Resize(ref self.mapDiscoveryTextures, self.regionNames.Length);
                CustomWorldMod.CustomWorldLog($"Custom Regions: Resizing mapDiscovery in PlayerProgression.");
            }
            self.miscProgressionData.discoveredShelters = new List<string>[self.regionNames.Length];
            orig(self);
        }

        private static void MiscProgressionData_FromString(On.PlayerProgression.MiscProgressionData.orig_FromString orig, PlayerProgression.MiscProgressionData self, string s)
        {
            string debug2 = "Custom Regions: MISC PROGRESION FROM STRING - RegionNames { ";
            for (int i = 0; i < self.owner.regionNames.Length; i++)
            {
                debug2 += self.owner.regionNames[i] + " , ";
            }
            CustomWorldMod.CustomWorldLog(debug2);

            Dictionary<string, int> dictionaryTemp = new Dictionary<string, int>(7);
            string[] array = Regex.Split(s, "<mpdA>");
            List<string> shelters = new List<string>();
            string myStringOutput = string.Empty;
            string debug = "Custom Regions: Loaded shelters from string: [ ";
            for (int i = 0; i < array.Length; i++)
            {
                string[] array2 = Regex.Split(array[i], "<mpdB>");
                string text = array2[0];
                if (text != null)
                {
                    if (dictionaryTemp == null)
                    {
                        Dictionary<string, int> dictionary = new Dictionary<string, int>(7);
                        dictionary.Add("SHELTERLIST", 0);
                        dictionary.Add("MENUREGION", 1);
                        dictionary.Add("LEVELTOKENS", 2);
                        dictionary.Add("SANDBOXTOKENS", 3);
                        dictionary.Add("INTEGERS", 4);
                        dictionary.Add("PLAYEDARENAS", 5);
                        dictionary.Add("REDSFLOWER", 6);
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


                /*for (int a = 0; a < array2.Length; a++)
                {
                    for (int b = 0; b < array2[a].Length; b++)
                    {
                        debug += array2[a][b] + " ";
                    }
                }*/
            }
            debug += " ]";
            CustomWorldMod.CustomWorldLog(debug);
            orig(self, s);

            debug2 = "Custom Regions: Discovered Shelters { ";
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
            CustomWorldMod.CustomWorldLog(debug2 + "} ");
        }

        private static string MiscProgressionData_ToString(On.PlayerProgression.MiscProgressionData.orig_ToString orig, PlayerProgression.MiscProgressionData self)
        {
            string text = "Custom Regions: MiscProgdata [";
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
            CustomWorldMod.CustomWorldLog(text + "] ");
            return orig(self);

        }

        private static void MiscProgressionData_SaveDiscoveredShelter(On.PlayerProgression.MiscProgressionData.orig_SaveDiscoveredShelter orig, PlayerProgression.MiscProgressionData self, string roomName)
        {
            CustomWorldMod.CustomWorldLog($"Custom Regions: Save Discovered Shelter [{roomName}]. ");
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
            CustomWorldMod.CustomWorldLog(debug);
            if (self.discoveredShelters[num] == null)
            {
                self.discoveredShelters[num] = new List<string>();
            }
            for (int j = 0; j < self.discoveredShelters[num].Count; j++)
            {
                if (self.discoveredShelters[num][j] == roomName)
                {
                    CustomWorldMod.CustomWorldLog("Custom Regions: Save shelter ERROR, already saved");
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
            CustomWorldMod.CustomWorldLog("} " + debug2);
        }
    }
}
