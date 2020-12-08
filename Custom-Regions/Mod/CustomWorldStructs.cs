using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CustomRegions.Mod
{
    public class CustomWorldStructs
    {
        /// <summary>
        /// Struct with information of available regions 
        /// [regionID, regionName, description, activated, checksum, loadOrder(Default is 100)]
        /// </summary>
        public struct RegionPack
        {
            public string name;
            public string description;
            public bool activated;
            public string checksum;
            public string folderName;
            public string url;
            public Dictionary<string, float> electricGates;
            public Dictionary<string, RegionConfiguration> regionConfig;
            public List<string> regions;
            public int loadOrder;
            public int loadNumber;

            public RegionPack(string name, string description, bool activated, string checksum, string folderName, string url, 
                Dictionary<string, float> electricGates, Dictionary<string, RegionConfiguration> regionConfig, List<string> regions, int loadOrder, int packNumber)
            {
                this.name = name;
                this.description = description;
                this.activated = activated;
                this.checksum = checksum;
                this.folderName = folderName;
                this.url = url;
                this.electricGates = electricGates;
                this.regionConfig = regionConfig;
                this.regions = regions;
                this.loadOrder = loadOrder;
                this.loadNumber = packNumber;
            }
        }

        /*
        public struct CustomRegion
        {
            public string regionID;
            public int loadOrder;
            public int regionNumber;

            public CustomRegion(string regionID, int loadOrder, int regionNumber)
            {
                this.regionID = regionID;
                this.loadOrder = loadOrder;
                this.regionNumber = regionNumber;
            }
        }
        */

        public struct RegionConfiguration
        {
            public string regionID;
            public bool albinoLevi;
            public bool albinoJet;
            public bool kelpVanilla;
            public Color? kelpColor;
            public bool bllVanilla;
            public Color? bllColor;
            public float blackSalamanderChance;

            public RegionConfiguration(string regionID, bool albinoLevi, bool albinoJet, 
                bool kelpVanilla, Color? kelpColor, bool bllVanilla, Color? bllColor, float blackSalamanderChance)
            {
                this.regionID = regionID;
                this.albinoLevi = albinoLevi;
                this.albinoJet = albinoJet;
                this.kelpVanilla = kelpVanilla;
                this.kelpColor = kelpColor;
                this.bllVanilla = bllVanilla;
                this.bllColor = bllColor;
                this.blackSalamanderChance = blackSalamanderChance;
            }
        }

        /// <summary>
        /// Struct with information of world lines, used in region merging and loading.
        /// [Data: holds the line itself, Vanilla: comes from vanilla or is it modified, modID: last mod which loaded or modified the line (empty if vanilla)]
        /// </summary>
        public struct WorldDataLine
        {
            public string data;
            public bool vanilla;
            public string modID;
            public WorldDataLine(string data, bool vanilla)
            {
                this.data = data;
                this.vanilla = vanilla;
                this.modID = string.Empty;
            }
            public WorldDataLine(string data, bool vanilla, string modID)
            {
                this.data = data;
                this.vanilla = vanilla;
                this.modID = modID;
            }
        }

        /// <summary>
        /// Struct with information of creature lines, used in region merging and loading.
        /// </summary>
        public struct CreatureLine
        {
            public bool lineage;
            public string room;
            public string[] connectedDens;
            public string dens;
            public int denNumber;

            public CreatureLine(bool lineage, string room, string[] connectedDens)
            {
                this.lineage = lineage;
                this.room = room;
                this.connectedDens = connectedDens;

                this.dens = null;
                this.denNumber = -1;
            }

            public CreatureLine(bool lineage, string room, string dens, int denNumber)
            {
                this.lineage = lineage;
                this.room = room;
                this.dens = dens;
                this.denNumber = denNumber;

                this.connectedDens = null;
            }
        }

        /// <summary>
        /// Struct with information about problems with save.
        /// </summary>
        public struct SaveProblems
        {
            public bool loadOrder;
            public bool installedRegions;
            public List<string> checkSum;
            public List<string> extraRegions;
            public List<string> missingRegions;

            public SaveProblems(bool differntLoadOrder, bool differentInstalledRegion, List<string> problematicCheckSum, List<string> extraReg, List<string> misReg)
            {
                this.loadOrder = differntLoadOrder;
                this.installedRegions = differentInstalledRegion;
                this.checkSum = problematicCheckSum;
                this.extraRegions = extraReg;
                this.missingRegions = misReg;
            }

            public bool AnyProblems => this.loadOrder || this.installedRegions || (this.checkSum != null && this.checkSum.Count > 0);

        }

        /// <summary>
        /// Struct with information about custom pearls
        /// </summary>
        public struct CustomPearl
        {
            public string name;
            public int ID;
            public Color color;
            public Color? secondaryColor;
            public string packName;

            public CustomPearl(string name, int iD, Color color, Color? secondaryColor, string packName)
            {
                this.name = name;
                this.ID = iD;
                this.color = color;
                this.secondaryColor = secondaryColor;
                this.packName = packName;
            }
        }
    }
}
