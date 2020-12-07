using CustomRegions.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CustomRegions
{
    static class RainWorldHook
    {
        public static void ApplyHooks()
        {
            On.RainWorld.Start += RainWorld_Start;
        }

        private static void RainWorld_Start(On.RainWorld.orig_Start orig, RainWorld self)
        {
            CustomWorldMod.LoadCustomWorldResources();
            CustomWorldMod.DownloadThumbs();

            orig(self);

            CustomWorldMod.rainWorldInstance = self;

            if (ThumbnailDownloader.instance != null && ThumbnailDownloader.instance.readyToDelete)
            {
                ThumbnailDownloader.instance.Clear();
                ThumbnailDownloader.instance = null;
            }
        }
    }
}
