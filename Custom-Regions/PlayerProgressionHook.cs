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
            orig(self, saveCurrentState, saveMaps, saveMiscProg);

            bool flag = false;
            bool[] array = new bool[self.mapDiscoveryTextures.Length];
            bool flag2 = false;
           /* if (saveMaps)
            {
                self.miscProgressionData.SaveDiscoveredShelters(ref self.tempSheltersDiscovered);
            }
            self.tempSheltersDiscovered.Clear();*/
            string[] progLines = self.GetProgLines();
            string text = string.Empty;
            for (int i = 0; i < progLines.Length; i++)
            {
                bool flag3 = false;
                string[] array2 = Regex.Split(progLines[i], "<progDivB>");
                if (array2[0] == "SAVE STATE")
                {
                    if (saveCurrentState && self.currentSaveState != null && int.Parse(array2[1][21].ToString()) == self.currentSaveState.saveStateNumber)
                    {
                        text = text + "SAVE STATE<progDivB>" + self.currentSaveState.SaveToString();
                        CustomWorldMod.CustomWorldLog("successfully saved state " + self.currentSaveState.saveStateNumber + " to disc");
                        flag = true;
                    }
                    else
                    {
                        text += progLines[i];
                    }
                    flag3 = true;
                }
                else if (array2[0] == "MAP")
                {
                    int num = -1;
                    int num2 = 0;
                    while (num2 < self.regionNames.Length && num < 0)
                    {
                        if (self.regionNames[num2] == array2[1])
                        {
                            num = num2;
                        }
                        num2++;
                    }
                    if (!saveMaps || num < 0 || self.mapDiscoveryTextures[num] == null)
                    {
                        text += progLines[i];
                    }
                    else
                    {
                        string text2 = text;
                        text = string.Concat(new string[]
                        {
                    text2,
                    "MAP<progDivB>",
                    self.regionNames[num],
                    "<progDivB>",
                    Convert.ToBase64String(self.mapDiscoveryTextures[num].EncodeToPNG())
                        });
                    }
                    flag3 = true;
                    array[num] = true;
                }
                else if (array2[0] == "MISCPROG")
                {
                    if (!saveMiscProg)
                    {
                        text += progLines[i];
                    }
                    else
                    {
                        text = text + "MISCPROG<progDivB>" + self.miscProgressionData.ToString();
                    }
                    flag3 = true;
                    flag2 = true;
                }
                if (flag3)
                {
                    text += "<progDivA>";
                }
            }
            if (saveCurrentState && !flag && self.currentSaveState != null)
            {
                text = text + "SAVE STATE<progDivB>" + self.currentSaveState.SaveToString() + "<progDivA>";
                CustomWorldMod.CustomWorldLog("successfully saved state " + self.currentSaveState.saveStateNumber + " to disc (fresh)");
            }
            if (saveMaps)
            {
                for (int j = 0; j < array.Length; j++)
                {
                    if (!array[j] && self.mapDiscoveryTextures[j] != null)
                    {
                        string text2 = text;
                        text = string.Concat(new string[]
                        {
                    text2,
                    "MAP<progDivB>",
                    self.regionNames[j],
                    "<progDivB>",
                    Convert.ToBase64String(self.mapDiscoveryTextures[j].EncodeToPNG()),
                    "<progDivA>"
                        });
                    }
                }
            }
            if (saveMiscProg && !flag2)
            {
                text = text + "MISCPROG<progDivB>" + self.miscProgressionData.ToString() + "<progDivA>";
            }

           // CustomWorldMod.CustomWorldLog(text);
            
            using (StreamWriter streamWriter = File.CreateText(CustomWorldMod.resourcePath + "saveDebug.txt"))
            {
                streamWriter.Write(text);
            }
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
