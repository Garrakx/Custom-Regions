using CustomRegions.Mod;
using RWCustom;
using System.Collections.Generic;
using System.IO;
using DevInterface;

namespace CustomRegions.DevInterface
{
    static class MapRenderOutputHook
    {
        public static void ApplyHooks()
        {
            On.DevInterface.MapRenderOutput.Signal += MapRenderOutput_Signal;
        }

        public static void RemoveHooks()
        {
            On.DevInterface.MapRenderOutput.Signal -= MapRenderOutput_Signal;
        }

        private static void MapRenderOutput_Signal(On.DevInterface.MapRenderOutput.orig_Signal orig, global::DevInterface.MapRenderOutput self, global::DevInterface.DevUISignalType type, global::DevInterface.DevUINode sender, string message)
        {
            string pathToMapFile = string.Empty;

            // From a Custom Region
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
            {
                string customFilePath = CRExtras.BuildPath(keyValues.Value, CRExtras.CustomFolder.RegionID, regionID: self.owner.game.world.name);

                if (Directory.Exists(customFilePath))
                {
                    pathToMapFile = customFilePath + "map_" + self.owner.game.world.name + ".png";

                    CustomWorldMod.Log($"[DEV] Saving custom Map Config to map_XX.png from [{keyValues.Value}] to [{pathToMapFile}]");

                    PNGSaver.SaveTextureToFile(self.texture, pathToMapFile);
                    self.ClearSprites();
                    (self.parentNode as MapPage).renderOutput = null;
                    (self.parentNode as MapPage).modeSpecificNodes.Remove(self);
                    self.parentNode.subNodes.Remove(self);
                    return;
                }

            }

            CustomWorldMod.Log($"[DEV] No custom region folder found for [{self.owner.game.world.name}], using vanilla...");

            orig(self, type, sender, message);
        }


    }
}
