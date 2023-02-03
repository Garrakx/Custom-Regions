using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace CustomRegionsMod.Progression
{
    internal static class StoryRegionsMod
    {
        public static void ApplyHooks()
        {
            On.SlugcatStats.getSlugcatStoryRegions += SlugcatStats_getSlugcatStoryRegions;
            On.SlugcatStats.getSlugcatOptionalRegions += SlugcatStats_getSlugcatOptionalRegions;
            On.RainWorld.PostModsInit += RainWorld_PostModsInit;
        }


        private static void RainWorld_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
        {
            orig(self);
            slugcatStoryStruct.CheckIfRegen();
            ArenaUnlocks.UnlockEnum.RefreshArenaUnlocks();
        }



        private static string[] SlugcatStats_getSlugcatOptionalRegions(On.SlugcatStats.orig_getSlugcatOptionalRegions orig, SlugcatStats.Name i)
        {
            slugcatStoryStruct.CheckIfRegen();
            return orig(i).Union(slugcatStoryStruct.CustomOptionalRegions[i].ToArray()).ToArray();

        }

        private static string[] SlugcatStats_getSlugcatStoryRegions(On.SlugcatStats.orig_getSlugcatStoryRegions orig, SlugcatStats.Name i)
        {
            slugcatStoryStruct.CheckIfRegen();
            return orig(i).Union(slugcatStoryStruct.CustomStoryRegions[i].ToArray()).ToArray();
        }

        public static void resetStoryStruct()
        { slugcatStoryStruct = new SlugcatStoryStructs.SlugcatStoryStruct(); }

        public static SlugcatStoryStructs.SlugcatStoryStruct slugcatStoryStruct = new SlugcatStoryStructs.SlugcatStoryStruct();
    }

    

    static class SlugcatStoryStructs
    {
        public struct SlugcatStoryStruct
        {
            public Dictionary<SlugcatStats.Name, List<string>> CustomStoryRegions;

            public Dictionary<SlugcatStats.Name, List<string>> CustomOptionalRegions;

            public List<string> NoSafariRegions;

            List<string> currentRegionOrder;

            public void CheckIfRegen()
            {
                CustomRegionsMod.Log("Checking if story regions need refresh", false, CustomRegionsMod.DebugLevel.MEDIUM);

                //why does SequenceEquals throw an exception the first time, the list should be initialized
                bool update = false;
                try
                {
                    update = !currentRegionOrder.SequenceEqual(Region.GetFullRegionOrder());
                }
                catch { update = true; }


                if (update)
                {
                    CustomRegionsMod.Log("refreshing", false, CustomRegionsMod.DebugLevel.MEDIUM);

                    currentRegionOrder = Region.GetFullRegionOrder();
                    try { RegenerateLists(); }
                    catch { CustomRegionsMod.Log("Failed to regenerate story lists", true); }
                    SafariEnums.Refresh();
                    Menu.RegionLandscapes.RefreshLandscapes();
                }
            }

            public void RegenerateLists()
            {
                try
                {
                    if (Region.GetFullRegionOrder() == null)
                    { return; }

                    CustomStoryRegions = new Dictionary<SlugcatStats.Name, List<string>>();
                    CustomOptionalRegions = new Dictionary<SlugcatStats.Name, List<string>>();
                    NoSafariRegions = new List<string>();

                    foreach (string slugString in SlugcatStats.Name.values.entries)
                    {
                        SlugcatStats.Name slug = (SlugcatStats.Name)ExtEnumBase.Parse(typeof(SlugcatStats.Name), slugString, false);
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
                        if (!File.Exists(path))
                        { continue; }

                        CustomRegionsMod.Log("Found MetaProperties.txt for region " + regionName, false, CustomRegionsMod.DebugLevel.MEDIUM);

                        foreach (string line in File.ReadAllLines(path))
                        {
                            if (line == "NoSafari")
                            {
                                CustomRegionsMod.Log("No safari unlock for this region", false, CustomRegionsMod.DebugLevel.FULL);
                                NoSafariRegions.Add(regionName);
                                continue;
                            }

                            bool inverted = false;
                            string[] array = Regex.Split(line, " : ");

                            if (array.Length < 2)
                            { continue; }

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
                                            CustomRegionsMod.Log($"Story region for [{slugString}]", false, CustomRegionsMod.DebugLevel.FULL);
                                            CustomStoryRegions[(SlugcatStats.Name)ExtEnumBase.Parse(typeof(SlugcatStats.Name), slugString, false)].Add(regionName);

                                        }

                                        else if (array[1] == "Optional")
                                        {
                                            CustomRegionsMod.Log($"Optional region for [{slugString}]", false, CustomRegionsMod.DebugLevel.FULL);
                                            CustomStoryRegions[(SlugcatStats.Name)ExtEnumBase.Parse(typeof(SlugcatStats.Name), slugString, false)].Add(regionName);

                                        }

                                    }
                                }
                            }

                        }
                    }
                }
                catch (Exception e){ throw e; }
            }

            public SlugcatStoryStruct(bool f = false)
            {

                CustomStoryRegions = new Dictionary<SlugcatStats.Name, List<string>>();
                CustomOptionalRegions = new Dictionary<SlugcatStats.Name, List<string>>();
                NoSafariRegions = new List<string>();
                currentRegionOrder = new List<string>();

                CheckIfRegen();
            }
        }
    }
}
