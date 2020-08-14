using CustomRegions.Mod;
using Menu;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using HUD;
using Menu;

namespace CustomRegions.CustomMenu
{
    static class FastTravelScreenHook
    {
        public static void ApplyHooks()
        {

            // This method cannot be hooked due to limitations of MonoMod.exe
            //On.Menu.FastTravelScreen.GetRegionOrder += FastTravelScreen_GetRegionOrder;


            On.Menu.FastTravelScreen.TitleSceneID += FastTravelScreen_TitleSceneID;
            On.Menu.FastTravelScreen.ctor += FastTravelScreen_ctor;

            // Debug
            //On.Menu.FastTravelScreen.GetAccessibleShelterNamesOfRegion += FastTravelScreen_GetAccessibleShelterNamesOfRegion;
        }


        /// <summary>
        /// TODO description
        /// </summary>
        private static void FastTravelScreen_ctor(On.Menu.FastTravelScreen.orig_ctor orig, Menu.FastTravelScreen self, ProcessManager manager, ProcessManager.ProcessID ID)
        {
            orig(self, manager, ID);

            self.blackFade = 1f;
            self.lastBlackFade = 1f;
            self.accessibleRegions = new List<int>();
            self.discoveredSheltersInRegion = new List<int>();
            //self.pages.Add(new Page(self, null, "main", 0));
            self.playerShelters = new string[3];

            List<string> regionOrder = FastTravelScreen_GetRegionOrder(FastTravelScreen.GetRegionOrder); //FastTravelScreen.GetRegionOrder();
            CustomWorldMod.CustomWorldLog($"Custom Regions: FastTravelScreen. Manager.regionNames.Length [{manager.rainWorld.progression.regionNames.Length}]. RegionOrder.Count [{regionOrder.Count}]");
            for (int k = 0; k < regionOrder.Count; k++)
            {
                for (int l = 0; l < manager.rainWorld.progression.regionNames.Length; l++)
                {
                    if (regionOrder[k] == manager.rainWorld.progression.regionNames[l] && self.GetAccessibleShelterNamesOfRegion(manager.rainWorld.progression.regionNames[l]) != null)
                    {
                        self.accessibleRegions.Add(l);
                    }
                }
            }
            if (self.accessibleRegions.Count != 0)
            {
                try
                {
                    foreach (SimpleButton button in self.pages[0].subObjects)
                    {
                        if (button.signalText.Equals("BACK"))
                        {
                            self.pages[0].subObjects.Remove(button);
                        }
                    }
                }
                catch (Exception e)
                {
                    CustomWorldMod.CustomWorldLog($"Custom Regions: Failed to remove back button at FastTraveLScreen [{e}]");
                }


                self.noRegions = false;
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
                for (int n = 0; n < self.hudContainers.Length; n++)
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
                string[] array = File.ReadAllLines(string.Concat(new object[]
                {
                    Custom.RootFolderDirectory(),
                    "World",
                    Path.DirectorySeparatorChar,
                    "Regions",
                    Path.DirectorySeparatorChar,
                    "regions.txt"
                }));

                array = CustomWorldMod.AddModdedRegions(array);

                self.allRegions = new Region[array.Length];
                for (int num2 = 0; num2 < self.allRegions.Length; num2++)
                {
                    self.allRegions[num2] = new Region(array[num2], num, num2);
                    num += self.allRegions[num2].numberOfRooms;
                }
                self.loadedWorlds = new World[self.accessibleRegions.Count];
                self.loadedMapData = new Map.MapData[self.accessibleRegions.Count];
                if (self.currentShelter != null)
                {
                    for (int num3 = 0; num3 < self.accessibleRegions.Count; num3++)
                    {
                        if (self.allRegions[self.accessibleRegions[num3]].name == self.currentShelter.Substring(0, 2))
                        {
                            CustomWorldMod.CustomWorldLog(self.currentShelter);
                            CustomWorldMod.CustomWorldLog(string.Concat(new object[]
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
            /*else
			{
				CustomWorldMod.CustomWorldLog("NO ACCESSIBLE REGIONS!");
				self.pages[0].subObjects.Add(new SimpleButton(self, self.pages[0], self.Translate("BACK"), "BACK", new Vector2(200f, 100f), new Vector2(100f, 30f)));
				self.noRegions = true;
			}
			self.mySoundLoopID = ((ID != ProcessManager.ProcessID.RegionsOverviewScreen) ? SoundID.MENU_Fast_Travel_Screen_LOOP : SoundID.MENU_Main_Menu_LOOP);
			*/
        }

        /// <summary>
        /// Used for Debug purposes.
        /// </summary>
        private static List<string> FastTravelScreen_GetAccessibleShelterNamesOfRegion(On.Menu.FastTravelScreen.orig_GetAccessibleShelterNamesOfRegion orig, Menu.FastTravelScreen self, string regionAcronym)
        {
            List<string> ori = orig(self, regionAcronym);
            string debug = string.Empty;
            if (ori != null)
            {
                foreach (string s in ori)
                {
                    debug += s + "/";
                }
            }
            CustomWorldMod.CustomWorldLog($"Custom Regions: GetAccesibleShelter. RegionAcronym [{regionAcronym}]. List:[{debug}]");
            return ori;
        }


        public static List<string> FastTravelScreen_GetRegionOrder(On.Menu.FastTravelScreen.orig_GetRegionOrder orig)
        {
            List<string> list = orig();
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
            {
                list.Add(keyValues.Key);
            }
            return list;
        }

        /// <summary>
        /// in FastTravelScreen - Fills CustomWorldMod.sceneCustomID with the ID of the region to load, and sets the extendedSceneID enum to CustomSceneID.
        /// </summary>
        private static Menu.MenuScene.SceneID FastTravelScreen_TitleSceneID(On.Menu.FastTravelScreen.orig_TitleSceneID orig, Menu.FastTravelScreen self, string regionName)
        {
            CustomWorldMod.sceneCustomID = string.Empty;
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
            {
                if (keyValues.Key.Equals(regionName))
                {
                    CustomWorldMod.CustomWorldLog($"Custom Regions: TitleSceneID {regionName}");
                    CustomWorldMod.sceneCustomID = regionName;
                }

            }

            if (orig(self, regionName) == Menu.MenuScene.SceneID.Empty && CustomWorldMod.sceneCustomID != string.Empty)
            {
                CustomWorldMod.CustomWorldLog($"Custom Regions: TitleSceneID. Using custom Scene ID [{CustomWorldMod.sceneCustomID}]");
                return EnumExt_extendedSceneID.CustomSceneID;
            }

            return orig(self, regionName);
        }
    }
}
