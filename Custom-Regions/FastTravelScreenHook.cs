using CustomRegions.Mod;
using Menu;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CustomRegions
{
    class FastTravelScreenHook
    {
        public static void ApplyHooks()
        {
            On.Menu.FastTravelScreen.TitleSceneID += FastTravelScreen_TitleSceneID;
            On.Menu.FastTravelScreen.GetRegionOrder += FastTravelScreen_GetRegionOrder;

            // Debug
            On.Menu.FastTravelScreen.ctor += FastTravelScreen_ctor;
        }

        private static void FastTravelScreen_ctor(On.Menu.FastTravelScreen.orig_ctor orig, Menu.FastTravelScreen self, ProcessManager manager, ProcessManager.ProcessID ID)
        {
            List<string> regionOrder = FastTravelScreen_GetRegionOrder(Menu.FastTravelScreen.GetRegionOrder);
            string debug = "Custom Regions: {";
            foreach(string region in regionOrder)
            {
                debug += region +", ";
            }
            debug += " }";
            Debug.Log(debug);

            orig(self, manager, ID);
            
            for (int k = 0; k < regionOrder.Count; k++)
            {
                for (int l = 0; l < manager.rainWorld.progression.regionNames.Length; l++)
                {
                    if (regionOrder[k] == manager.rainWorld.progression.regionNames[l] && self.GetAccessibleShelterNamesOfRegion(manager.rainWorld.progression.regionNames[l]) != null)
                    {
                        Debug.Log($"Custom Regions: Accesible region [{regionOrder[k]}]");
                        self.accessibleRegions.Add(l);
                    }
                }
            }
            if (self.accessibleRegions.Count != 0)
            {
                self.currentRegion = 0;
                self.upcomingRegion = -1;
                self.preloadedScenes = new InteractiveMenuScene[self.accessibleRegions.Count];
                for (int m = 0; m < self.accessibleRegions.Count; m++)
                {
                    self.pages.Add(new Page(self, null, manager.rainWorld.progression.regionNames[self.accessibleRegions[m]], m + 1));
                    self.pages[m + 1].Container = new FContainer();
                    self.container.AddChild(self.pages[m + 1].Container);
                    self.preloadedScenes[m] = new InteractiveMenuScene(self, self.pages[m + 1], self.TitleSceneID(manager.rainWorld.progression.regionNames[self.accessibleRegions[m]]));
                    self.pages[m + 1].subObjects.Add(self.preloadedScenes[m]);
                    if (m == 0)
                    {
                        self.scene = self.preloadedScenes[m];
                    }
                    else
                    {
                        self.preloadedScenes[m].Hide();
                    }
                }
                self.fadeSprite = new FSprite("Futile_White", true);
                self.fadeSprite.scaleX = 87.5f;
                self.fadeSprite.scaleY = 50f;
                self.fadeSprite.x = manager.rainWorld.screenSize.x / 2f;
                self.fadeSprite.y = manager.rainWorld.screenSize.y / 2f;
                self.fadeSprite.color = new Color(0f, 0f, 0f);
                self.container.AddChild(self.fadeSprite);
                self.gradientsContainer = new GradientsContainer(self, self.pages[0], new Vector2(0f, 0f), 0.5f);
                self.pages[0].subObjects.Add(self.gradientsContainer);
                self.mapButtonPrompt = new MenuLabel(self, self.pages[0], (!self.IsFastTravelScreen) ? self.Translate("Press the MAP button to view regional map") : self.Translate("Press the MAP button to select the shelter you wish to continue from"), new Vector2(583f, 5f), new Vector2(200f, 30f), false);
                self.mapButtonPrompt.label.color = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.MediumGrey);
                self.pages[0].subObjects.Add(self.mapButtonPrompt);
                self.prevButton = new BigArrowButton(self, self.pages[0], "PREVIOUS", new Vector2(200f, 90f), -1);
                self.pages[0].subObjects.Add(self.prevButton);
                self.nextButton = new BigArrowButton(self, self.pages[0], "NEXT", new Vector2(1116f, 90f), 1);
                self.pages[0].subObjects.Add(self.nextButton);
                if (self.IsFastTravelScreen)
                {
                    self.startButton = new HoldButton(self, self.pages[0], self.Translate("HOLD TO START"), "HOLD TO START", new Vector2(683f, 115f), 80f);
                    self.pages[0].subObjects.Add(self.startButton);
                }
                else if (self.IsRegionsScreen)
                {
                    if (manager.rainWorld.inGameTranslator.currentLanguage == InGameTranslator.LanguageID.Portuguese)
                    {
                        self.pages[0].subObjects.Add(new SimpleButton(self, self.pages[0], self.Translate("EXIT"), "BACK", new Vector2(self.prevButton.pos.x, 668f), new Vector2(100f, 30f)));
                    }
                    else
                    {
                        self.pages[0].subObjects.Add(new SimpleButton(self, self.pages[0], self.Translate("BACK"), "BACK", new Vector2(self.prevButton.pos.x, 668f), new Vector2(100f, 30f)));
                    }
                }
                self.hudContainers = new MenuContainer[2];
                for (int n = 0; n < 2; n++)
                {
                    self.hudContainers[n] = new MenuContainer(self, self.pages[0], new Vector2(0f, 0f));
                    self.pages[0].subObjects.Add(self.hudContainers[n]);
                }
                string text = "JUMP/THROW buttons - Switch layers";
                if (self.IsFastTravelScreen)
                {
                    text += "<LINE>PICK UP button - Select shelter";
                }
                text = self.Translate(text);
                text = text.Replace("<LINE>", "     ");
                self.buttonInstruction = new MenuLabel(self, self.pages[0], text, new Vector2(583f, 5f), new Vector2(200f, 30f), false);
                self.buttonInstruction.label.color = Menu.Menu.MenuRGB(Menu.Menu.MenuColors.DarkGrey);
                self.pages[0].subObjects.Add(self.buttonInstruction);
                self.selectedObject = null;
                int num = 0;

                List<string> IDs = new List<string>();
                foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
                {
                    IDs.Add(keyValues.Key);
                }
                string[] array = IDs.ToArray();

                self.allRegions = new Region[array.Length];
                for (int num2 = 0; num2 < self.allRegions.Length; num2++)
                {
                    self.allRegions[num2] = new Region(array[num2], num, num2);
                    num += self.allRegions[num2].numberOfRooms;
                }
                self.loadedWorlds = new World[self.accessibleRegions.Count];
                self.loadedMapData = new HUD.Map.MapData[self.accessibleRegions.Count];
                if (self.currentShelter != null)
                {
                    for (int num3 = 0; num3 < self.accessibleRegions.Count; num3++)
                    {
                        if (self.allRegions[self.accessibleRegions[num3]].name == self.currentShelter.Substring(0, 2))
                        {
                            Debug.Log(self.currentShelter);
                            Debug.Log(string.Concat(new object[]
                            {
                        "found start region: ",
                        num3,
                        " ",
                        self.allRegions[self.accessibleRegions[num3]].name
                            }));
                            self.currentRegion = num3;
                            break;
                        }
                    }
                }
                self.InitiateRegionSwitch(self.currentRegion);
                while (!self.worldLoader.Finished)
                {
                    self.worldLoader.Update();
                }
                self.AddWorldLoaderResultToLoadedWorlds(self.currentRegion);
                self.FinalizeRegionSwitch(self.currentRegion);
                self.worldLoader = null;
                self.hud = new HUD.HUD(new FContainer[]
                {
                    self.hudContainers[1].Container,
                    self.hudContainers[0].Container
                }, manager.rainWorld, self);
            }

            Debug.Log($"Custom Regions: Accesible region number {self.accessibleRegions.Count}");

        }

        private static List<string> FastTravelScreen_GetRegionOrder(On.Menu.FastTravelScreen.orig_GetRegionOrder orig)
        {
            List<string> list = orig();
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
            {
                list.Add(keyValues.Key);
            }
            return list;
        }

        private static Menu.MenuScene.SceneID FastTravelScreen_TitleSceneID(On.Menu.FastTravelScreen.orig_TitleSceneID orig, Menu.FastTravelScreen self, string regionName)
        {
            CustomWorldMod.sceneCustomID = string.Empty;
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
            {
                if (keyValues.Key.Equals(regionName))
                {
                    Debug.Log($"Custom Regions: TitleSceneID {regionName}");
                    CustomWorldMod.sceneCustomID = regionName;
                }

            }

            // should return string.empty
            return orig(self, regionName);
        }
    }
}
