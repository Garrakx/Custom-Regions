using CustomRegions.Mod;
using System;
using System.Linq;
using System.Runtime.Remoting.Messaging;

namespace CustomRegions
{
    static class RainWorldHook
    {
        public static void ApplyHooks()
        {
            On.RainWorld.Start += RainWorld_Start;
        }

        public static void RemoveHooks()
        {
            On.RainWorld.Start -= RainWorld_Start;
        }

        private static void RainWorld_Start(On.RainWorld.orig_Start orig, RainWorld self)
        {
            CustomWorldMod.LoadCustomWorldResources();
            CustomWorldMod.rainWorldInstance = self;

            orig(self);

        }
    }
}
