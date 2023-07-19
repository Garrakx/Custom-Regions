using CustomRegions.Progression;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CustomRegions.Mod
{
    internal static class CustomStaticCache
{

        public static Dictionary<SlugcatStats.Name, List<string>> CustomStoryRegions = new();

        public static Dictionary<SlugcatStats.Name, List<string>> CustomOptionalRegions = new();

        public static List<string> SafariRegions = new();

        static List<string> currentRegionOrder = new();

        static List<string> currentSlugcats = new();

        public static void CheckForRefresh(bool forceRefresh = false)
        {
            //CustomRegionsMod.CustomLog("Checking if story regions need refresh", false, CustomRegionsMod.DebugLevel.FULL);

            //why does SequenceEquals throw an exception the first time, the list should be initialized
            if (!forceRefresh)
            {
                try
                {
                    forceRefresh = !currentRegionOrder.SequenceEqual(Region.GetFullRegionOrder()) || !currentSlugcats.SequenceEqual(ExtEnumBase.GetNames(typeof(SlugcatStats.Name)).ToList());
                }
                catch (Exception e) { forceRefresh = true; CustomRegionsMod.CustomLog($"Exception while refreshing!\n{e}"); }
            }
             
            if (forceRefresh)
            {
                Refresh();
            }
        }

        public static void Refresh()
        {
            CustomRegionsMod.CustomLog("--- Refreshing CRS ---", false, CustomRegionsMod.DebugLevel.MEDIUM);

            currentRegionOrder = Region.GetFullRegionOrder();
            currentSlugcats = ExtEnumBase.GetNames(typeof(SlugcatStats.Name)).ToList();
            try { RegenerateLists(); } catch(Exception e) { CustomRegionsMod.CustomLog("Failed to regenerate story lists\n" + e, true); }
            SafariEnums.Refresh();
            CustomMenu.RegionLandscapes.RefreshLandscapes();
        }

        public static void RegenerateLists()
        {
            if (Region.GetFullRegionOrder() == null) { return; }

            CustomStoryRegions = new Dictionary<SlugcatStats.Name, List<string>>();
            CustomOptionalRegions = new Dictionary<SlugcatStats.Name, List<string>>();
            SafariRegions = new List<string>();

            foreach (string slugString in SlugcatStats.Name.values.entries)
            {
                SlugcatStats.Name slug = new(slugString, false);
                if (SlugcatStats.HiddenOrUnplayableSlugcat(slug)) continue;

                CustomStoryRegions.Add(slug, new List<string>());
                CustomOptionalRegions.Add(slug, new List<string>());
            }

            foreach (string regionName in Region.GetFullRegionOrder())
            {
                string path = AssetManager.ResolveFilePath(string.Concat(new string[]
                {
                        "World",
                        Path.DirectorySeparatorChar.ToString(),
                        regionName,
                        Path.DirectorySeparatorChar.ToString(),
                        "metaproperties.txt"
                }));
                if (!File.Exists(path)) { continue; }

                CustomRegionsMod.CustomLog("Found MetaProperties.txt for region " + regionName, false, CustomRegionsMod.DebugLevel.MEDIUM);

                foreach (string line in File.ReadAllLines(path))
                {
                    if (line.ToLower().Trim() == "safari")
                    {
                        CustomRegionsMod.CustomLog("safari unlock for this region", false, CustomRegionsMod.DebugLevel.FULL);
                        SafariRegions.Add(regionName);
                        continue;
                    }

                    else if (line.ToLower().Trim() == "story")
                    {
                        CustomRegionsMod.CustomLog($"Story region for [ALL]", false, CustomRegionsMod.DebugLevel.FULL);
                        foreach (SlugcatStats.Name slugName in CustomStoryRegions.Keys)
                        {
                            if (CustomStoryRegions[slugName].Contains(regionName) || CustomOptionalRegions[slugName].Contains(regionName))
                            { continue; }

                            CustomStoryRegions[slugName].Add(regionName);
                        }
                    }

                    else if (line.ToLower().Trim() == "optional")
                    {
                        CustomRegionsMod.CustomLog($"Optional region for [ALL]", false, CustomRegionsMod.DebugLevel.FULL);
                        foreach (SlugcatStats.Name slugName in CustomOptionalRegions.Keys)
                        {
                            if (CustomStoryRegions[slugName].Contains(regionName) || CustomOptionalRegions[slugName].Contains(regionName))
                            { continue; }

                            CustomOptionalRegions[slugName].Add(regionName);
                        }
                    }

                    bool inverted = false;
                    string[] array = Regex.Split(line, " : ");

                    if (array.Length < 2) { continue; }

                    array[1] = array[1].Trim().ToLower();

                    if (array[0].StartsWith("X-"))
                    {
                        array[0] = array[0].Substring(2);
                        inverted = true;
                    }

                    string[] array2 = Regex.Split(array[0], ",");

                    string debug = "";

                    foreach (string str in array2)
                    {
                        foreach (string slugString in SlugcatStats.Name.values.entries)
                        {
                            SlugcatStats.Name slugName = new(slugString, false);
                            if (!CustomStoryRegions.ContainsKey(slugName) || CustomStoryRegions[slugName].Contains(regionName) || CustomOptionalRegions[slugName].Contains(regionName))
                            { continue; }

                            if ((str == slugString) == !inverted)
                            {
                                if (array[1] == "story")
                                {
                                    debug += slugName + ",";
                                    CustomStoryRegions[slugName].Add(regionName);
                                }
                                else if (array[1] == "optional")
                                {
                                    debug += slugName + ",";
                                    CustomOptionalRegions[slugName].Add(regionName);
                                }
                            }
                        }
                    }
                    if (debug != "" && (array[1] == "story" || array[1] == "optional"))
                    { CustomRegionsMod.CustomLog($"{array[1]} region for [{debug}]", false, CustomRegionsMod.DebugLevel.FULL); }
                }
            }
            LogCache();
        }

        public static void LogCache()
        {
            CustomRegionsMod.CustomLog($"\nCUSTOM STORY REGIONS FOR EACH SLUGCAT");
            foreach (KeyValuePair<SlugcatStats.Name, List<string>> slug in CustomStoryRegions)
            {
                CustomRegionsMod.CustomLog($"{slug.Key}: [{string.Join(", ", slug.Value.ToArray())}]");
            }

            CustomRegionsMod.CustomLog($"\nCUSTOM OPTIONAL REGIONS FOR EACH SLUGCAT");
            foreach (KeyValuePair<SlugcatStats.Name, List<string>> slug in CustomOptionalRegions)
            {
                CustomRegionsMod.CustomLog($"{slug.Key}: [{string.Join(", ", slug.Value.ToArray())}]");
            }
            CustomRegionsMod.CustomLog("");
        }
    }
}
