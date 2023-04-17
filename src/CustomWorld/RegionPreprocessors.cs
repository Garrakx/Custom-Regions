using CustomRegions.Mod;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static CustomRegions.Mod.Structs;

namespace CustomRegions.CustomWorld
{
    internal static class RegionPreprocessors
    {

        public class RegionInfo
        {
            public string RegionID { get; internal set; }

            public SlugcatStats.Name playerCharacter { get; internal set; }
            /// <summary>
            /// World lines from world_XX.txt file.
            /// </summary>

            private List<string> _lines;
            public List<string> Lines {
                get { return _lines; }

                internal set
                {
                    _lines = value;
                    InitiateSectionBounds();
                }
            }

            public List<string> LinesSection(string section)
            {
                if (!sectionBounds.ContainsKey(section) || sectionBounds[section][0] == -1 || sectionBounds[section][1] == -1 || sectionBounds[section][1] < sectionBounds[section][0]) return null;

                return Lines.GetRange(sectionBounds[section][0], sectionBounds[section][1] - sectionBounds[section][0] + 1);
            }

            internal void InitiateSectionBounds()
            {
                int[] defBounds = new int[2] { -1, -1 };
                sectionBounds = new();

                for (int i = 0; i < Lines.Count; i++)
                {
                    CustomRegionsMod.CustomLog(Lines[i]);
                    string line = Lines[i];
                    bool end = false;
                    if (line.Substring(0, 4) == "END ")
                    { end = true; line = line.Substring(4); }

                    if (sections.Contains(line))
                    {
                        if (!sectionBounds.ContainsKey(line))
                        { sectionBounds[line] = defBounds.ToArray(); }

                        if (sectionBounds.ContainsKey(line))
                        { sectionBounds[line][end ? 1 : 0] = end ? i - 1 : i + 1; }
                    }

                }
            }

            public Dictionary<string, int[]> sectionBounds
            {
                get;
                internal set;
            }

            private List<string> sections = new()
            {
            "CONDITIONAL LINKS",
            "ROOMS",
            "CREATURES",
            "BAT MIGRATION BLOCKAGES"
            };
        }

        public static List<RegionPreprocessor> regionPreprocessors;

        public static List<CustomCondition> customConditions;

        public delegate void RegionPreprocessor(RegionInfo info);

        public delegate bool CustomCondition(string condition);

        public static void ReplaceRoom(RegionInfo info)
        {
            Dictionary<string, string> replace = new();

            string CL = "CONDITIONAL LINKS";
            string RM = "ROOMS";
            string CR = "CREATURES";
            string BM = "BAT MIGRATION BLOCKAGES";

            CustomRegionsMod.CustomLog("conditional links");
            for (int i = 0; i < info.LinesSection(CL)?.Count; i++)
            {
                if (!string.IsNullOrEmpty(info.LinesSection(CL)[i]))
                {
                    string[] array = Regex.Split(info.LinesSection(CL)[i], " : ");
                    if (array.Length >= 4 && array[1] == "REPLACEROOM" && array[0] == info.playerCharacter.ToString())
                    {
                        CustomRegionsMod.CustomLog($"adding line [{String.Join(" : ", array)}]");
                        replace.Add(array[2], array[3]);
                        //info.LinesSection(CL)[i] = "//";
                        info.Lines[i+info.sectionBounds[CL][0]] = "//";
                    }
                }
            }



            CustomRegionsMod.CustomLog("rooms");
            for (int i = 0; i < info.LinesSection(RM)?.Count; i++)
            {
                if (RoomLine2.TryParse(info.LinesSection(RM)[i], out RoomLine2 roomLine))
                {
                    CustomRegionsMod.CustomLog($"roomname: [{roomLine.room}]");
                    bool modify = false;
                    if (replace.ContainsKey(roomLine.room))
                    {
                        CustomRegionsMod.CustomLog("found key");
                        roomLine.room = replace[roomLine.room];
                        modify = true;
                    }

                    for (int j = 0; j < roomLine.connections.Count; j++)
                    {
                        if (replace.ContainsKey(roomLine.connections[j]))
                        {
                            roomLine.connections[j] = replace[roomLine.connections[j]];
                            modify = true;
                        }

                    }

                    if(modify)
                    { info.Lines[i + info.sectionBounds[RM][0]] = roomLine.ToString(); }
                }
            }

            CustomRegionsMod.CustomLog("creatures");
            for (int i = 0; i < info.LinesSection(CR)?.Count; i++)
            {
                if (CreatureLine2.TryParse(info.LinesSection(CR)[i], out CreatureLine2 creatureLine))
                {
                    bool modify = false;
                    if (replace.ContainsKey(creatureLine.room))
                    {
                        creatureLine.room = replace[creatureLine.room];
                        modify = true;
                    }

                    if (modify)
                    { info.Lines[i + info.sectionBounds[CR][0]] = creatureLine.ToString(); }
                }
            }

            CustomRegionsMod.CustomLog("bat migrations");
            for (int i = 0; i < info.LinesSection(BM)?.Count; i++)
            {
                if (replace.ContainsKey(info.LinesSection(BM)[i]))
                { info.Lines[i + info.sectionBounds[BM][0]] = replace[info.LinesSection(BM)[i]]; }
            }

            info.Lines.RemoveAll(str => str == "//");
        }

        public static void CustomConditionsProcessing(RegionInfo info)
        {
            for (int i = 0; i < info.Lines.Count; i++)
            {
                if (info.Lines[i][0] == '{')
                {
                    bool remove = false;
                    string[] split = info.Lines[i].Substring(1, info.Lines[i].IndexOf("}") - 1).Split(',');
                    foreach (string str in split)
                    {
                        foreach (CustomCondition condition in customConditions)
                        {
                            if (condition(str) == false)
                            {
                                remove = true;
                                break;
                            }
                        }
                        if (remove)
                        { break; }
                    }
                    if (remove)
                    { info.Lines[i] = "//"; }

                    else
                    {
                        info.Lines[i] = info.Lines[i].Substring(info.Lines[i].IndexOf("}") + 1);
                    }
                }
            }

            info.Lines.RemoveAll(str => str == "//");
        }

        public static bool MSCCondition(string condition)
        {
            bool notInverted = true;
            if (condition.Contains('!'))
            {
                notInverted = false;
                condition.Remove('!');
            }

            if (condition != "MSC") return true;
            return ModManager.MSC == notInverted;
        }

        public static bool RegionExistsCondition(string condition)
        {
            bool notInverted = true;
            if (condition.Contains('!'))
            {
                notInverted = false;
                condition.Remove('!');
            }

            if (condition.Count() != 2) return true;
            return Region.GetFullRegionOrder().Contains(condition) == notInverted;
        }


        public static bool ModIDCondition(string condition)
        {
            bool notInverted = true;
            if (condition.Contains('!'))
            {
                notInverted = false;
                condition.Remove('!');
            }

            if (condition[0] != '#') return true;
            condition = condition.Substring(1);

            bool modIDExists = false;

            foreach (ModManager.Mod mod in ModManager.ActiveMods)
            {
                if (mod.id == condition)
                {
                    modIDExists = true;
                    break;
                }
            }

            return modIDExists == notInverted;
        }

        public static void InitializeBuiltinPreprocessors()
        {
            regionPreprocessors = new List<RegionPreprocessor>();
            customConditions = new List<CustomCondition>();

            regionPreprocessors.Add(CustomConditionsProcessing);

            regionPreprocessors.Add(ReplaceRoom);
            regionPreprocessors.Add(IndexedEntranceClass.IndexedEntrance);

            customConditions.Add(MSCCondition);
            customConditions.Add(RegionExistsCondition);
            customConditions.Add(ModIDCondition);
        }
    }
}
