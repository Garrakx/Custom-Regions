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
            On.SlugcatStats.getSlugcatStoryRegions += SlugcatStats_getSlugcatStoryRegions;
            On.SlugcatStats.getSlugcatOptionalRegions += SlugcatStats_getSlugcatOptionalRegions;
        }


        private static string[] SlugcatStats_getSlugcatOptionalRegions(On.SlugcatStats.orig_getSlugcatOptionalRegions orig, SlugcatStats.Name i)
        {
            CustomStaticCache.CheckForRefresh();
            return orig(i).Union(CustomStaticCache.CustomOptionalRegions[i].ToArray()).ToArray();
        }

        private static string[] SlugcatStats_getSlugcatStoryRegions(On.SlugcatStats.orig_getSlugcatStoryRegions orig, SlugcatStats.Name i)
        {
            CustomStaticCache.CheckForRefresh();
            return orig(i).Union(CustomStaticCache.CustomStoryRegions[i].ToArray()).ToArray();
        }

    }



    
}
