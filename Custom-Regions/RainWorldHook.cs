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
            On.RainWorldGame.RestartGame += RainWorldGame_RestartGame;
        }

        public static void RemoveHooks()
        {
            On.RainWorld.Start -= RainWorld_Start;
            On.RainWorldGame.RestartGame -= RainWorldGame_RestartGame;
        }

        private static void RainWorld_Start(On.RainWorld.orig_Start orig, RainWorld self)
        {
            CustomWorldMod.LoadCustomWorldResources();
            CustomWorldMod.rainWorldInstance = self;
            CustomWorldMod.Log($"Assigning RW instance. Null [{CustomWorldMod.rainWorldInstance == null}]", false, CustomWorldMod.DebugLevel.MEDIUM);

            orig(self);
        }

        private static void RainWorldGame_RestartGame(On.RainWorldGame.orig_RestartGame orig, RainWorldGame self)
        {
            orig(self);
            CustomWorldMod.LoadCustomWorldResources();
        }

    }
}
