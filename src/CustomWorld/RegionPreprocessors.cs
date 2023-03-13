using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomRegions.CustomWorld
{
    internal static class RegionPreprocessors
    {

        public class RegionInfo
        {
            public string RegionID { get; internal set; }
            /// <summary>
            /// World lines from world_XX.txt file.
            /// </summary>
            public List<string> Lines { get; internal set; }
        }

        public static List<RegionPreprocessor> regionPreprocessors;

        public static List<CustomCondition> customConditions;

        public delegate void RegionPreprocessor(RegionInfo info);

        public delegate bool CustomCondition(string condition);

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

        public static void InitializeBuiltinPreprocessors()
        {
            regionPreprocessors = new List<RegionPreprocessor>();
            customConditions = new List<CustomCondition>();

            regionPreprocessors.Add(CustomConditionsProcessing);

            customConditions.Add(MSCCondition);
            customConditions.Add(RegionExistsCondition);
        }
    }
}
