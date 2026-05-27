using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CustomRegions.Mod
{
    internal static class Utils
    {
        public static IEnumerable<string> ProcessSlugcatConditions(IEnumerable<string> lines, SlugcatStats.Name slug)
        {
            foreach (string line in lines)
            {
                if (line.Length < 1) continue;
                else if (line[0] == '(' && line.Contains(')'))
                {
                    string text = line.Substring(1, line.IndexOf(")") - 1);
                    if (!StringMatchesSlugcat(text, slug)) continue;
                    else yield return line.Substring(line.IndexOf(")") + 1);
                }
                else yield return line;
            }
        }

        public static bool StringMatchesSlugcat(string text, SlugcatStats.Name slug)
        {
            bool include = false;
            bool inverted = false;

            if (text.StartsWith("X-"))
            {
                text = text.Substring(2);
                inverted = true;
            }

            if (slug == null)
            {
                return inverted;
            }

            foreach (string text2 in text.Split(','))
            {
                if (text2.Trim() == slug.ToString())
                {
                    include = true;
                    break;
                }
            }

            return inverted != include;
        }


        public static Color ParseColor(string s)
        {
            if (s.Contains(","))
            {
                float[] array = s.Split(',').Select(x => float.Parse(x.Trim())).ToArray();
                return new Color(array[0], array[1], array[2]);
            }
            return RWCustom.Custom.hexToColor(s);
        }
    }
}
