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

        private static void RainWorld_Start(On.RainWorld.orig_Start orig, RainWorld self)
        {
            //CustomWorldMod.Log($"Loaded assemblies... [{string.Join(", ", .ToArray())}]"); 
            try
            {
                bool usingBepinex = false;
                foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.GetName().Name.Equals("BepInEx.MonoMod.Loader") || assembly.GetName().Name.Equals("BepInEx"))
                    {
                        usingBepinex = true;
                        break;
                    }
                }
                CustomWorldMod.usingBepinex = usingBepinex;
                CustomWorldMod.Log($"Using BepInEx [{usingBepinex}]");
            } 
            catch (Exception e)
            {
                CustomWorldMod.Log("Error checking the modloaer " + e, true);
            }

            CustomWorldMod.LoadCustomWorldResources();
            CustomWorldMod.rainWorldInstance = self;

            orig(self);

        }
    }
}
