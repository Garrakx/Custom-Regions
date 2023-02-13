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

        public static Dictionary<SlugcatStats.Name, List<string>> CustomStoryRegions = new Dictionary<SlugcatStats.Name, List<string>>();

        public static Dictionary<SlugcatStats.Name, List<string>> CustomOptionalRegions = new Dictionary<SlugcatStats.Name, List<string>>();

        public static List<string> NoSafariRegions = new List<string>();

        static List<string> currentRegionOrder = new List<string>();

        public static void CheckForRefresh(bool forceRefresh = false)
        {
            //CustomRegionsMod.CustomLog("Checking if story regions need refresh", false, CustomRegionsMod.DebugLevel.FULL);

            //why does SequenceEquals throw an exception the first time, the list should be initialized
            if (!forceRefresh)
            {
                try
                {
                    forceRefresh = !currentRegionOrder.SequenceEqual(Region.GetFullRegionOrder());
                }
                catch (Exception e) { forceRefresh = true; CustomRegionsMod.CustomLog($"Exception while refreshing! {e}"); }
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
            try { RegenerateLists(); } catch { CustomRegionsMod.CustomLog("Failed to regenerate story lists", true); }
            SafariEnums.Refresh();
            CustomMenu.RegionLandscapes.RefreshLandscapes();
        }

        public static void RegenerateLists()
        {
            try
            {
                if (Region.GetFullRegionOrder() == null) { return; }

                CustomStoryRegions = new Dictionary<SlugcatStats.Name, List<string>>();
                CustomOptionalRegions = new Dictionary<SlugcatStats.Name, List<string>>();
                NoSafariRegions = new List<string>();

                foreach (string slugString in SlugcatStats.Name.values.entries)
                {
                    SlugcatStats.Name slug = (SlugcatStats.Name)ExtEnumBase.Parse(typeof(SlugcatStats.Name), slugString, false);
                    //if (SlugcatStats.HiddenOrUnplayableSlugcat(slug)) continue;

                    CustomStoryRegions.Add(slug, new List<string>());
                    CustomOptionalRegions.Add(slug, new List<string>());
                }

                NoSafariRegions.Add("HR");

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
                        if (line == "NoSafari")
                        {
                            CustomRegionsMod.CustomLog("No safari unlock for this region", false, CustomRegionsMod.DebugLevel.FULL);
                            NoSafariRegions.Add(regionName);
                            continue;
                        }

                        bool inverted = false;
                        string[] array = Regex.Split(line, " : ");

                        if (array.Length < 2) { continue; }

                        if (array[0].StartsWith("X-"))
                        {
                            array[0] = line.Substring(2);
                            inverted = true;
                        }

                        string[] array2 = Regex.Split(array[0], ",");

                        foreach (string str in array2)
                        {
                            foreach (string slugString in SlugcatStats.Name.values.entries)
                            {
                                if ((str == slugString) == !inverted)
                                {
                                    if (array[1] == "Story")
                                    {
                                        CustomRegionsMod.CustomLog($"Story region for [{slugString}]", false, CustomRegionsMod.DebugLevel.FULL);
                                        CustomStoryRegions[(SlugcatStats.Name)ExtEnumBase.Parse(typeof(SlugcatStats.Name), slugString, false)].Add(regionName);

                                    }
                                    else if (array[1] == "Optional")
                                    {
                                        CustomRegionsMod.CustomLog($"Optional region for [{slugString}]", false, CustomRegionsMod.DebugLevel.FULL);
                                        CustomStoryRegions[(SlugcatStats.Name)ExtEnumBase.Parse(typeof(SlugcatStats.Name), slugString, false)].Add(regionName);

                                    }

                                }
                            }
                        }

                    }
                }
                LogCache();
            }
            catch (Exception e) { throw e; }
        }

        public static void LogCache()
        {
            CustomRegionsMod.CustomLog($"\nCUSTOM STORY REGIONS FOR EACH SLUGCAT");
            foreach (KeyValuePair<SlugcatStats.Name, List<string>> slug in CustomStoryRegions)
            {
                CustomRegionsMod.CustomLog($"{slug.Key}: [{String.Join(", ", slug.Value.ToArray())}]");
            }

            CustomRegionsMod.CustomLog($"\nCUSTOM OPTIONAL REGIONS FOR EACH SLUGCAT");
            foreach (KeyValuePair<SlugcatStats.Name, List<string>> slug in CustomOptionalRegions)
            {
                CustomRegionsMod.CustomLog($"{slug.Key}: [{String.Join(", ", slug.Value.ToArray())}]");
            }
            CustomRegionsMod.CustomLog("");
        }
    }
}
