using CustomRegions.Mod;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CustomRegions.Creatures
{
    static class TentaclePlantGraphicsHook
    {
        public static void ApplyHooks()
        {
            On.TentaclePlantGraphics.ctor += TentaclePlantGraphics_ctor; ;
            On.TentaclePlantGraphics.ApplyPalette += TentaclePlantGraphics_ApplyPalette;
        }

        private static void TentaclePlantGraphics_ctor(On.TentaclePlantGraphics.orig_ctor orig, TentaclePlantGraphics self, PhysicalObject ow)
        {
            orig(self, ow);
            customColor = null;
            World world = ow.abstractPhysicalObject.world;
            if (world != null)
            {
                //CustomWorldMod.Log($"Region Name [{self.region.name}]");
                foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
                {
                    //CustomWorldMod.Log($"Checking in [{CustomWorldMod.availableRegions[keyValues.Key].regionName}]");
                    if (CustomWorldMod.availableRegions[keyValues.Key].regionConfig.TryGetValue(world.region.name, out CustomWorldStructs.RegionConfiguration config))
                    {
                        if (!config.kelpVanilla)
                        {
                            customColor = new Color[self.danglers.Length];
                            
                            CustomWorldMod.Log($"Spawning tentacle plant with custom color in [{world.region.name}] from [{CustomWorldMod.availableRegions[keyValues.Key].regionName}]");
                            for (int i = 0; i < customColor.Length; i++)
                            {
                                HSLColor hsl = CRExtras.RGB2HSL(config.kelpColor ?? new Color());
                                customColor[i] = Custom.HSL2RGB(Mathf.Lerp(hsl.hue*0.9f, hsl.hue * 1.1f, Mathf.Pow(self.danglerProps[i, 0], 1.6f)), Mathf.Lerp(hsl.saturation, 0.35f, self.danglerProps[i, 0]), Mathf.Lerp(hsl.lightness, 0.35f, self.danglerProps[i, 0]));
                            }
                        }
                            break;
                    }
                }
            }
        }

        private static Color[] customColor;

        private static void TentaclePlantGraphics_ApplyPalette(On.TentaclePlantGraphics.orig_ApplyPalette orig, TentaclePlantGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);
            if (customColor != null)
            {
                for (int i = 0; i < self.danglers.Length; i++)
                {
                    sLeaser.sprites[i + 1].color = Color.Lerp(customColor[i], palette.blackColor, rCam.room.Darkness(self.plant.rootPos));
                }
            }
        }
    }
}
