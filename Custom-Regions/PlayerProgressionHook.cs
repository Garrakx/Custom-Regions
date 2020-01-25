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
        public static void ApplyHooks()
        {
            On.PlayerProgression.LoadProgression += PlayerProgression_LoadProgression;
            On.PlayerProgression.InitiateProgression += PlayerProgression_InitiateProgression;

            // Debug
            On.PlayerProgression.MiscProgressionData.SaveDiscoveredShelter += MiscProgressionData_SaveDiscoveredShelter;
            On.PlayerProgression.MiscProgressionData.ToString += MiscProgressionData_ToString;
            On.PlayerProgression.MiscProgressionData.FromString += MiscProgressionData_FromString;
        }

        public static void AddModdedRegions(PlayerProgression self)
        {
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
            {
                Debug.Log($"Custom Regions: PlayerProgression, loading new regions");
                string regionToAdd = keyValues.Key;
                bool shouldAdd = true;

                for (int i = 0; i < self.regionNames.Length; i++)
                {
                    if (regionToAdd.Equals(self.regionNames[i]))
                    {
                        shouldAdd = false;
                    }
                }
                if (shouldAdd)
                {
                    Array.Resize(ref self.regionNames, self.regionNames.Length + 1);
                    self.regionNames[self.regionNames.Length - 1] = keyValues.Key;
                    Debug.Log($"Custom Regions: Added new region [{regionToAdd}] from [{keyValues.Value}].");
                }
            }
        }

        private static void PlayerProgression_InitiateProgression(On.PlayerProgression.orig_InitiateProgression orig, PlayerProgression self)
        {
            AddModdedRegions(self);
            if (self.regionNames.Length != self.mapDiscoveryTextures.Length)
            {
                Array.Resize(ref self.mapDiscoveryTextures, self.regionNames.Length);
                Debug.Log($"Custom Regions: Resizing mapDiscovery in PlayerProgression.");
            }
            self.miscProgressionData.discoveredShelters = new List<string>[self.regionNames.Length];
            orig(self);
        }

        private static void PlayerProgression_LoadProgression(On.PlayerProgression.orig_LoadProgression orig, PlayerProgression self)
        {
            AddModdedRegions(self);
            if (self.regionNames.Length != self.mapDiscoveryTextures.Length)
            {
                Array.Resize(ref self.mapDiscoveryTextures, self.regionNames.Length);
                Debug.Log($"Custom Regions: Resizing mapDiscovery in PlayerProgression.");
            }
            self.miscProgressionData.discoveredShelters = new List<string>[self.regionNames.Length];
            orig(self);
        }

        private static void MiscProgressionData_FromString(On.PlayerProgression.MiscProgressionData.orig_FromString orig, PlayerProgression.MiscProgressionData self, string s)
        {
            string debug2 = "Custom Regions: RegionNames { ";
            for (int i = 0; i < self.owner.regionNames.Length; i++)
            {
                debug2 += self.owner.regionNames[i] + " , ";
            }
            Debug.Log(debug2);

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
                                            }
                                        }
                                    }
                                    //myStringOutput = String.Join(",", array2[2].Select(p => p.ToString()).ToArray());
                                    break;
                                }
                        }
                    }
                }


                for (int a = 0; a < array2.Length; a++)
                {
                    for (int b = 0; b < array2[a].Length; b++)
                    {
                        debug += array2[a][b] + " ";
                    }
                }
            }
            debug += " ]";
            Debug.Log(debug);
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
            Debug.Log(debug2+ "} ");
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
            Debug.Log(text + "] ");
            return orig(self);

        }

        private static void MiscProgressionData_SaveDiscoveredShelter(On.PlayerProgression.MiscProgressionData.orig_SaveDiscoveredShelter orig, PlayerProgression.MiscProgressionData self, string roomName)
        {
            Debug.Log($"Custom Regions: Save Discovered Shelter [{roomName}]. ");
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
            Debug.Log(debug);
            if (self.discoveredShelters[num] == null)
            {
                self.discoveredShelters[num] = new List<string>();
            }
            for (int j = 0; j < self.discoveredShelters[num].Count; j++)
            {
                if (self.discoveredShelters[num][j] == roomName)
                {
                    Debug.Log("Custom Regions: Save shelter ERROR, already saved");
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
            Debug.Log("} " + debug2);
        }
    }
}
