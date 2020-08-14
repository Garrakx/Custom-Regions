using CustomRegions.Mod;
using Menu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CustomRegions.CustomMenu
{
    static class SlugcatSelectMenuHook
    {
        public static void ApplyHooks() 
        {
            On.Menu.SlugcatSelectMenu.SlugcatPageContinue.ctor += SlugcatPageContinue_ctor;
        }

        private static void SlugcatPageContinue_ctor(On.Menu.SlugcatSelectMenu.SlugcatPageContinue.orig_ctor orig, Menu.SlugcatSelectMenu.SlugcatPageContinue self, Menu.Menu menu, Menu.MenuObject owner, int pageIndex, int slugcatNumber)
        {
            orig(self, menu, owner, pageIndex, slugcatNumber);

            if (self.saveGameData.shelterName != null && self.saveGameData.shelterName.Length > 2)
            {
                string text2 = self.saveGameData.shelterName.Substring(0, 2);

                bool customRegion = true;
                List<string> vanillaRegions = CustomWorldMod.VanillaRegions().ToList();
                for (int i = 0; i < vanillaRegions.Count; i++)
                {
                    if (text2 == vanillaRegions[i])
                    {
                        customRegion = false;
                    }
                }
                if (customRegion)
                {
                    foreach (MenuObject label in self.subObjects)
                    {
                        if (label is MenuLabel && label == self.regionLabel && (label as MenuLabel).text.Length < 3)
                        {
                            string fullRegionName = "N / A";
                            CustomWorldMod.loadedRegions.TryGetValue(text2, out fullRegionName);
                            CustomWorldMod.CustomWorldLog("Custom Regions: text " + text2);
                            if (fullRegionName != null)
                            {
                                if (fullRegionName.Length > 0)
                                {
                                    text2 = fullRegionName;

                                    fullRegionName = string.Concat(new object[]
                                    {
                                text2,
                            " - ",
                            menu.Translate("Cycle"),
                            " ",
                            (slugcatNumber != 2) ? self.saveGameData.cycle : (RedsIllness.RedsCycles(self.saveGameData.redsExtraCycles) - self.saveGameData.cycle)
                                    });
                                }
                            (label as MenuLabel).text = fullRegionName;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
