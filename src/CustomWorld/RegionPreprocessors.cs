using BepInEx;
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
                    string line = Lines[i];
                    if (line.IsNullOrWhiteSpace() || line.Length < 4) continue;
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

        public static void CustomConditionsProcessing(RegionInfo info)
        {
            for (int i = 0; i < info.Lines.Count; i++)
            {
                if (info.Lines[i].IsNullOrWhiteSpace()) continue;
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
                condition = condition.Replace("!", "");
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
                condition = condition.Replace("!", "");
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
                condition = condition.Replace("!", "");
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

            regionPreprocessors.Add(ReplaceRoomPreprocessor.ReplaceRoom);
            regionPreprocessors.Add(IndexedEntranceClass.IndexedEntrance);

            customConditions.Add(MSCCondition);
            customConditions.Add(RegionExistsCondition);
            customConditions.Add(ModIDCondition);
        }
    }
}
