﻿using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CustomRegions.Mod
{
    public class CustomWorldStructs
    {

        public struct PackDependency
        {
            /// <summary>
            /// List containing which region packs use this dependency.
            /// </summary>
            public List<string> usedBy;

            public string assemblyName;
            public string location;

            public int audbVersion;
            public string hash;
            internal void SetHash()
            {
                using (var md5 = System.Security.Cryptography.MD5.Create())
                {
                    using (var stream = File.OpenRead(this.location))
                    {
                        hash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
                    }
                }

            }

            public void LoadDependency(string fileLocation)
            {
                if (!File.Exists(fileLocation))
                {
                    return;
                }
                usedBy = new List<string>();
                this.audbVersion = -1;
                CustomWorldMod.Log($"Loading dependency at [{fileLocation}]", false, CustomWorldMod.DebugLevel.MEDIUM);
                ModuleDefinition module = ModuleDefinition.ReadModule(fileLocation);
                var list = module.Types.Where(x => x != null && x.IsPublic && x.HasFields).ToList();
                try
                {
                    foreach (TypeDefinition type in list.Where(type => type.BaseType.FullName.Equals("Partiality.Modloader.PartialityMod") ||
                                                                        type.BaseType.FullName.Equals("BepInEx.BaseUnityPlugin")).ToList())
                    {

                        CustomWorldMod.Log($"Found mod or plugin [{type.FullName}]", false, CustomWorldMod.DebugLevel.FULL);
                        //CustomWorldMod.Log($"[{string.Join(", ", type.Methods.Select(x => x.FullName).ToArray())}]");

                        var method = type.Methods.FirstOrDefault(x => x != null && x.Name.Equals(".ctor"));

                        if (method.Equals(default))
                        {
                            CustomWorldMod.Log($"Dependency [{type.FullName}] does not support AutoUpdate");
                            this.audbVersion = -1;

                        }
                        else
                        {
                            var processor = method.Body.GetILProcessor();
                            var newInstruction = processor.Create(OpCodes.Call, method);
                            //var firstInstruction = method.Body.Instructions[0];

                            var instruction = method.Body.Instructions.FirstOrDefault(l => l.OpCode == OpCodes.Stfld && l.Operand.ToString().Contains("version"));

                            if (instruction != null && !instruction.Equals(default))
                            {
                                CustomWorldMod.Log($"Instruction: [{instruction}]", false, CustomWorldMod.DebugLevel.FULL);
                                var previous = instruction.Previous;
                                CustomWorldMod.Log($"previous instruction: [{previous}]", false, CustomWorldMod.DebugLevel.FULL);
                                //CustomWorldMod.Log($"{previous.OpCode}");
                                if (previous.Operand != null && int.TryParse(previous.Operand.ToString(), out int result))
                                {
                                    this.audbVersion = result;

                                }
                                else
                                {
                                    // cursed
                                    if (int.TryParse(previous.OpCode.ToString().Replace($"{OpCodes.Ldc_I4}.", ""), out int result2))
                                    {

                                        this.audbVersion = result2;
                                    }
                                }
                                CustomWorldMod.Log("Audb version: " + this.audbVersion, false, CustomWorldMod.DebugLevel.MEDIUM);

                            }
                            else
                            {
                                // no autoupdate
                            }
                        }


                    }


                    this.assemblyName = System.Reflection.AssemblyName.GetAssemblyName(fileLocation).Name;
                    /*
                    System.Reflection.Assembly loadedAssembly = System.Reflection.Assembly.LoadFile(fileLocation);
                    System.Reflection.FieldInfo version = loadedAssembly.GetType().GetField("version");
                    if (version != null && version.FieldType == typeof(int))
                    {
                        this.audbVersion = (int)version.GetValue(loadedAssembly);
                    }
                    */
                    this.location = fileLocation;
                    this.SetHash();
                    CustomWorldMod.Log($"Loaded assembly [{this.assemblyName}]. AUDB version {this.audbVersion}. Hash {this.hash}. Location [{this.assemblyName}]", false, CustomWorldMod.DebugLevel.MEDIUM);
                }
                catch (Exception e)
                {
                    CustomWorldMod.Log($"Failed loaded assembly, {e}", true);
                }
            }

        }

        /// <summary>
        /// Struct with information of available regions 
        /// [regionID, regionName, description, activated, checksum, loadOrder(Default is random)]
        /// </summary>
        public struct RegionPack
        {
            public string name;
            public string description;
            public string author;
            public bool activated;
            public string checksum;
            public string folderName;
            public string thumbUrl;
            public Dictionary<string, float> electricGates;
            public Dictionary<string, RegionConfiguration> regionConfig;
            public List<string> regions;
            public int loadOrder;
            public int loadNumber;
            public string version;
            public string packUrl;
            public string requirements;
            public bool expansion;
            public bool shownInBrowser;
            public int downloads;

            /// <summary>If true, region name will be used for slugcat page menu.
            ///</summary>
            public bool useRegionName;

            public RegionPack(string name, string description, string author, bool activated, string checksum, string folderName, string url,
                Dictionary<string, float> electricGates, Dictionary<string, RegionConfiguration> regionConfig, List<string> regions, int loadOrder,
                int packNumber, string version, string packUrl, string requirements, bool usePackName, bool expansion, bool shownInBrowser, int downloads)
            {
                this.name = name;
                this.description = description;
                this.author = author;
                this.activated = activated;
                this.checksum = checksum;
                this.folderName = folderName;
                this.thumbUrl = url;
                this.electricGates = electricGates;
                this.regionConfig = regionConfig;
                this.regions = regions;
                this.loadOrder = loadOrder;
                this.loadNumber = packNumber;
                this.version = version;
                this.packUrl = packUrl;
                this.requirements = requirements;
                this.useRegionName = usePackName;
                this.expansion = expansion;
                this.shownInBrowser = shownInBrowser;
                this.downloads = downloads;
            }
            /// <summary>Initializes everything.
            ///</summary>
            public RegionPack(string folderName)
            {
                this.name = "";
                this.description = "";
                this.author = "";
                this.activated = false;
                this.checksum = "";
                this.folderName = folderName;
                this.thumbUrl = "";
                this.electricGates = new Dictionary<string, float>();
                this.regionConfig = new Dictionary<string, RegionConfiguration>();
                this.regions = new List<string>();
                this.loadOrder = (int)(UnityEngine.Random.value * 500);
                this.loadNumber = this.loadOrder;
                this.version = "1.0";
                this.packUrl = "";
                this.requirements = "";
                this.useRegionName = false;
                this.expansion = false;
                this.shownInBrowser = true;
                this.downloads = 0;
            }

            /// <summary>Initializes everything to null except ctor arguments. Used for the save inof
            ///</summary>
            public RegionPack(string name, string checkSum, int packNumber)
            {
                this.name = name;
                this.description = null;
                this.author = null;
                this.activated = false;
                this.checksum = checkSum;
                this.folderName = null;
                this.thumbUrl = null;
                this.electricGates = null;
                this.regionConfig = null;
                this.regions = null;
                this.loadOrder = int.MaxValue;
                this.loadNumber = packNumber;
                this.version = null;
                this.packUrl = null;
                this.requirements = null;
                this.useRegionName = false;
                this.expansion = false;
                this.shownInBrowser = true;
                this.downloads = 0;
            }
        }

        public struct RegionConfiguration
        {
            public string regionID;
            public bool albinoLevi;
            public bool albinoJet;
            public bool kelpVanilla;
            public Color? kelpColor;
            public bool bllVanilla;
            public Color? bllColor;
            public Color? batFlyColor;
            public bool batVanilla;
            public float blackSalamanderChance;
            public string scavTradeItem;
            public float scavGearChance;

            public RegionConfiguration(string regionID, bool albinoLevi, bool albinoJet, bool kelpVanilla, Color? kelpColor, bool bllVanilla,
                Color? bllColor, float blackSalamanderChance, Color? batFlyColor, bool batVanilla, string scavTradeItems, float scavGearChance)
            {
                this.regionID = regionID;
                this.albinoLevi = albinoLevi;
                this.albinoJet = albinoJet;
                this.kelpVanilla = kelpVanilla;
                this.kelpColor = kelpColor;
                this.bllVanilla = bllVanilla;
                this.bllColor = bllColor;
                this.blackSalamanderChance = blackSalamanderChance;
                this.batFlyColor = batFlyColor;
                this.batVanilla = batVanilla;
                this.scavTradeItem = scavTradeItems;
                this.scavGearChance = scavGearChance;
            }
        }

        /// <summary>
        /// Struct with information of world lines, used in region merging and loading.
        /// </summary>
        public struct WorldDataLine
        {
            /// <summary>
            /// Holds the whole line (room + connections + ending string)
            /// </summary>
            public string line;
            /// <summary>
            /// Room of the connection
            /// </summary>
            public string roomName;
            /// <summary>
            /// Connections (everything after : without the ending)
            /// </summary>
            public string connections;
            /// <summary>
            /// If connection has ending string (GATE, SHELTER, etc)
            /// </summary>
            public string endingString;
            public bool lineage;
            public bool vanilla;
            /// <summary>
            /// Last packName that modified this connection
            /// </summary>
            public string packName;

            public WorldDataLine(string line, string roomName, string connections, string endingString, bool lineage, bool vanilla, string modID)
            {
                this.line = line;
                this.roomName = roomName;
                this.connections = connections;
                this.endingString = endingString;
                this.lineage = lineage;
                this.vanilla = vanilla;
                this.packName = modID;
            }

            public WorldDataLine(string line, string roomName, string connections, string endingString, bool vanilla, string modID)
            {
                this.line = line;
                this.roomName = roomName;
                this.connections = connections;
                this.endingString = endingString;
                this.lineage = false;
                this.vanilla = vanilla;
                this.packName = modID;
            }

            public WorldDataLine(string line, bool vanilla)
            {
                this.line = line;
                this.roomName = null;
                this.connections = null;
                this.endingString = null;
                this.lineage = false;
                this.vanilla = vanilla;
                this.packName = null;
            }

            public void BuildRoomFromWholeLine(string line)
            {
                //CustomWorldMod.Log($"Rebuilding WorldData from line [{this}]", false, CustomWorldMod.DebugLevel.FULL);
                WorldDataLine updatedLine = this;
                string[] split = System.Text.RegularExpressions.Regex.Split(line, " : ");
                string roomName = string.Empty;
                string connections = string.Empty;
                string endingString = string.Empty;

                // Corrupted line (this should not happen)
                if (split.Length < 2 || split.Length > 3)
                {
                    CustomWorldMod.Log($"Corrupted vanilla line [{line}]", true);
                }
                else
                {
                    roomName = split[0];
                    connections = split[1];
                    if (connections.Contains("DISCONNECT"))
                    {
                        connections.Replace("DISCONNECT", "DISCONNECTED");
                    }

                    // Line has ending
                    if (split.Length == 3)
                    {
                        endingString = split[2];
                    }
                }
                updatedLine.roomName = roomName;
                updatedLine.connections = connections;
                updatedLine.endingString = endingString;
                this = updatedLine;
                //CustomWorldMod.Log($"Result after rebuilding [{this}]", false, CustomWorldMod.DebugLevel.FULL);
            }


            public override string ToString()
            {
                string formatedName = $"LINE [{this.line}] ROOMNAME [{this.roomName}] CONNECTIONS [{this.connections}] ENDINGSTRING [{this.endingString}] " +
                    $"PACK [{this.packName}]";
                return formatedName;
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

        public struct News
        {
            public const string IGNORE = "[ignr]";
            public const string BIGTEXT = "[bgTxT]";
            public const string DATE = "[dte]";

            /*
            public DateTime date;
            public string text;
            public string type;

            public News(DateTime date, string text, string type)
            {
                this.date = date;
                this.text = text;
                this.type = type;
            }
            */
        }

        public struct ProcessedThumbnail
        {
            public DateTime dateDownloaded;
            public Color mainColor;
            public byte[] data;
        }
    }
}
