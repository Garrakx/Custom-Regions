using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using CustomRegions.Mod;

namespace CustomRegions.Progression
{
    internal static class StoryRegionsMod
    {
        public static void ApplyHooks()
        {
            On.SlugcatStats.SlugcatStoryRegions += SlugcatStats_getSlugcatStoryRegions;
            On.SlugcatStats.SlugcatOptionalRegions += SlugcatStats_getSlugcatOptionalRegions;
            On.Region.RegionColor += Region_RegionColor;
        }

        private static UnityEngine.Color Region_RegionColor(On.Region.orig_RegionColor orig, string regionName)
        {
            return orig(regionName.ToUpper());
        }

        private static List<string> SlugcatStats_getSlugcatOptionalRegions(On.SlugcatStats.orig_SlugcatOptionalRegions orig, SlugcatStats.Name i)
        {
            List<string> regions = orig(i);
            CustomStaticCache.CheckForRefresh();
            if (CustomStaticCache.CustomOptionalRegions.ContainsKey(i))
                return regions.Union(CustomStaticCache.CustomOptionalRegions[i].Where(x => !regions.Contains(x))).ToList();

            else return regions;
        }

        private static List<string> SlugcatStats_getSlugcatStoryRegions(On.SlugcatStats.orig_SlugcatStoryRegions orig, SlugcatStats.Name i)
        {
            List<string> regions = orig(i);
            CustomStaticCache.CheckForRefresh();
            if (CustomStaticCache.CustomStoryRegions.ContainsKey(i))
                return regions.Union(CustomStaticCache.CustomStoryRegions[i].Where(x => !regions.Contains(x))).ToList();

            else return regions;
        }

    }



    
}
