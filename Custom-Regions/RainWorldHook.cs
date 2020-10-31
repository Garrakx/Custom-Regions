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
            orig(self);
            CustomWorldMod.rainWorldInstance = self;
        }
    }
}
