using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using static System.Net.Mime.MediaTypeNames;

namespace CustomRegions.Mod
{
    public class Structs
    {

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
                    CustomRegionsMod.CustomLog($"Corrupted vanilla line [{line}]", true);
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

        public struct RoomLine2
        {
            public string room;
            public List<string> connections;
            public List<string> tags;

            public RoomLine2(string room, List<string> connections, List<string> tags)
            {
                this.room = room;
                this.connections = connections;
                this.tags = tags;
            }

            public static bool TryParse(string line, out RoomLine2 result)
            {
                result = new RoomLine2();
                result.connections = new();
                result.tags = new();

                if (!line.Contains(" : "))
                 return false;
                string[] array = Regex.Split(line, " : ");
                if (array.Length < 2)
                 return false;

                result.room = array[0];
                result.connections = Regex.Split(RWCustom.Custom.ValidateSpacedDelimiter(array[1], ","), ", ").ToList();

                if (array.Length > 2)
                {
                    for (int i = 2; i < array.Length; i++)
                    {
                        result.tags.Add(array[i]);
                    }
                }

                return true;
            }

            public override string ToString()
            {
                string tag = tags.Count > 0 ? " : " + string.Join(" : ", tags) : "";
                return string.Concat(new string[] 
                { 
                room,
                " : ",
                string.Join(", ", connections),
                tag
                });
            }
        }

        public interface ICreatureType
        {
            public string creature { get; set; }
            public string tags { get; set; }

        }

        public struct LoneCreature : ICreatureType
        {
            public string creature { get; set; }
            public string tags { get; set; }

            int den;
            int quantity;


            public override string ToString()
            {
                return string.Join("-", new string[]
                {
                den.ToString(),
                creature,
                tags,
                quantity.ToString()
                });
            }

            public static bool TryParse(string line, out LoneCreature result)
            {
                result = new();

                if (!line.Contains("-"))
                    return false;

                string[] array = Regex.Split(line, "-");
                if (array.Length < 2 || int.TryParse(array[0], out result.den))
                    return false;

                result.creature = array[1];
                result.quantity = 1;

                bool firstTag = false;

                for (int j = 2; j < array.Length; j++)
                {
                    if (array[j].Length > 0 && array[j][0] == '{')
                    {
                        result.tags = array[j];
                        firstTag = true;
                    }
                    else if (firstTag)
                    {
                        result.tags = result.tags + "-" + array[j];
                    }
                    else
                    {
                        try
                        {
                            result.quantity = Convert.ToInt32(array[j], CultureInfo.InvariantCulture);
                        }
                        catch
                        {
                            result.quantity = 1;
                        }
                    }
                    if (array[j].Contains("}"))
                    {
                        firstTag = false;
                    }

                }

                return true;
            }
        }

        public struct Lineage : ICreatureType
        {
            public string creature { get; set; }
            public string tags { get; set; }

            float moveOn;

            public override string ToString()
            {
                return string.Join("-", new string[]
                {
                creature,
                tags,
                moveOn.ToString()
                });
            }

            public static bool TryParse(string line, out Lineage result)
            {
                result = new();

                if (!line.Contains("-"))
                    return false;

                string[] array = Regex.Split(line, "-");
                if (array.Length < 2)
                    return false;

                result.creature = array[1];
                result.moveOn = 0f;

                bool firstTag = false;

                for (int j = 1; j < array.Length; j++)
                {
                    if (array[j].Length > 0 && array[j][0] == '{')
                    {
                        result.tags = array[j];
                        firstTag = true;
                    }
                    else if (firstTag)
                    {
                        result.tags = result.tags + "-" + array[j];
                    }
                    else
                    {
                        try
                        {
                            result.moveOn = float.Parse(array[j], NumberStyles.Any, CultureInfo.InvariantCulture);
                        }
                        catch
                        {
                            result.moveOn = 0f;
                        }
                    }
                    if (array[j].Contains("}"))
                    {
                        firstTag = false;
                    }

                }

                return true;
            }
        }

        public struct CreatureLine2
        {
            public bool lineage;
            public string room;
            public List<ICreatureType> creatures;
            public int lineageDen;

            public static bool TryParse(string line, out CreatureLine2 result)
            {
                result = new();
                result.creatures = new List<ICreatureType>();
                result.room = "";

                if (!line.Contains(" : "))
                    return false;

                string[] array = Regex.Split(line, " : ");
                if (array.Length < 2)
                    return false;

                if (array[0] == "LINEAGE")
                {
                    if (array.Length < 4 || int.TryParse(array[2], out result.lineageDen))
                        return false;

                    result.room = array[1];


                    result.lineage = true;

                    string[] array3 = Regex.Split(RWCustom.Custom.ValidateSpacedDelimiter(array[3], ","), ", ");
                    foreach (string str in array3)
                    {
                        if (Lineage.TryParse(str, out Lineage lineage))
                        { result.creatures.Add(lineage); }
                    }
                    return true;
                }

                result.room = array[0];

                string[] array2 = Regex.Split(RWCustom.Custom.ValidateSpacedDelimiter(array[1], ","), ", ");
                foreach (string str in array2)
                {
                    if (LoneCreature.TryParse(str, out LoneCreature loneCreature))
                    { result.creatures.Add(loneCreature); }
                }
                return true;
            }

            public override string ToString()
            {
                if (lineage)
                    return string.Join(" : ", new string[] {
                    "LINEAGE",
                    room,
                    lineageDen.ToString(),
                    string.Join(", ", creatures)
                });
                else
                    return string.Join(" : ", new string[] {
                    room,
                    string.Join(", ", creatures)
                });
            }
        }

        /// <summary>
        /// Struct with information about custom pearls
        /// </summary>
        public struct CustomPearl
        {
            public Conversation.ID conversationID;
            public DataPearl.AbstractDataPearl.DataPearlType type;
            public Color color;
            public Color highlightColor;
            public string filePath;

            public CustomPearl(DataPearl.AbstractDataPearl.DataPearlType type, Color color, Color highlightColor, string filePath, Conversation.ID conversationID)
            {
                this.type = type;
                this.color = color;
                this.highlightColor = highlightColor;
                this.filePath = filePath;
                this.conversationID = conversationID;
            }

            public override string ToString()
            {
                return string.Join(", ", new string[] {
                    this.type.ToString(),
                    this.color.ToString(),
                    this.highlightColor.ToString(),
                    this.conversationID.ToString(),
                    this.filePath
                }) ;
            }
        }
    }
}
