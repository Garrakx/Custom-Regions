using CustomRegions.Mod;
using Menu;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CustomRegions.CustomMenu
{
    public class EnumExt_extendedSceneID
    {
        public static MenuScene.SceneID CustomSceneID;
    }


    static class MenuSceneHook
    {
        public static void ApplyHook()
        {
            On.Menu.MenuScene.BuildScene += MenuScene_BuildScene;
            On.Menu.MenuScene.SaveToFile += MenuScene_SaveToFile;
            On.Menu.MenuScene.Update += MenuScene_Update;
        }

        private static void MenuScene_Update(On.Menu.MenuScene.orig_Update orig, MenuScene self)
        {
            orig(self);
            if(Input.GetKeyDown("r"))
            {
                string regionID = self.depthIllustrations[0].fileName.Substring(0, 2);
                LoadScenePositionSettings(self, self.sceneFolder, regionID);
            }
        }

        private static void MenuScene_SaveToFile(On.Menu.MenuScene.orig_SaveToFile orig, MenuScene self)
        {
            try
            {
                orig(self);
            } catch(Exception e)
            {
                CustomWorldMod.CustomWorldLog($"(Expected behaviour) Failed to save position.txt, using custom folder [Vanilla error - {e}]");
                string regionID = self.depthIllustrations[0].fileName.Substring(0, 2);
                Debug.Log("Saving : " + regionID);
                string text = string.Empty;
                for (int i = 0; i < self.depthIllustrations.Count; i++)
                {
                    Debug.Log(self.depthIllustrations[i].fileName + "   " + self.depthIllustrations[i].pos);
                    string text2 = text;
                    text = string.Concat(new object[]
                    {
                    text2,
                    self.depthIllustrations[i].pos.x,
                    ", ",
                    self.depthIllustrations[i].pos.y,
                    Environment.NewLine
                    });
                }
                //string regionValue;
                //CustomWorldMod.loadedRegions.TryGetValue(regionID, out regionValue);
                //string path = CustomWorldMod.resourcePath + regionValue + Path.DirectorySeparatorChar;
                string positionFiledPath = self.sceneFolder + Path.DirectorySeparatorChar + "positions.txt";
                if (File.Exists(positionFiledPath))
                {
                    using (StreamWriter streamWriter = File.CreateText(positionFiledPath))
                    {
                        streamWriter.Write(text);
                    }
                }
                else
                {
                    CustomWorldMod.CustomWorldLog($"ERROR! position.txt file not found! Looking at [{positionFiledPath}]");
                    // Create position txt file?
                }
            }
        }

        public static void BuildCustomRegionScene(MenuScene self, string regionID, string sceneFolder)
        {
            if (self.flatMode)
            {
                self.AddIllustration(new MenuIllustration(self.menu, self, self.sceneFolder, $"Landscape - {regionID} - Flat", new Vector2(683f, 384f), false, true));
            }
            else
            {
                Vector2 posVector = new Vector2(0, 0);

                string[] fileEntries = Directory.GetFiles(sceneFolder);
                Array.Sort(fileEntries);
                List<MenuDepthIllustration> illu = new List<MenuDepthIllustration>();
                foreach (string fileName in fileEntries)
                {
                    if (fileName.Contains(".png") && !fileName.ToLower().Contains("flat") && !fileName.ToLower().Contains("meta"))
                    {
                        string name = Path.GetFileNameWithoutExtension(fileName);
                        //CustomWorldMod.CustomWorldLog($"Custom Regions: Loading {name}");
                        /* int number = name[name.Length - 1];
                         if (number < positions.Length)
                         {
                             posVector.x = float.Parse(positions[number].Split(new string[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries)[0]);
                             posVector.y = float.Parse(positions[number].Split(new string[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries)[1]);
                         }*/
                        
                        
                       

                        //self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, $"{name}", posVector, 4 + number, MenuDepthIllustration.MenuShader.Normal));
                        CustomWorldMod.CustomWorldLog($"Custom Regions: Loading MenuDepthIllustration - Name [{name}] - Position[{posVector}]");
                        illu.Add(new MenuDepthIllustration(self.menu, self, self.sceneFolder, name, posVector, 1f, MenuDepthIllustration.MenuShader.Normal));
                    }
                }
                illu.Reverse();
                foreach (MenuDepthIllustration depthIllustration in illu)
                {
                    CustomWorldMod.CustomWorldLog($"Custom Regions: Adding MenuDepthIllustration - Name [{depthIllustration.fileName}]");
                    self.AddIllustration(depthIllustration);
                }

                // Load positions
                LoadScenePositionSettings(self, sceneFolder, regionID);
            }

            string regionValue;
            CustomWorldMod.loadedRegions.TryGetValue(regionID, out regionValue);
            string path = CustomWorldMod.resourcePath + regionValue + Path.DirectorySeparatorChar;
            string titleFolderName = path + "Assets" + Path.DirectorySeparatorChar + "Futile" + Path.DirectorySeparatorChar + "Resources" + Path.DirectorySeparatorChar + "Illustrations";
            if (self.menu.ID == ProcessManager.ProcessID.FastTravelScreen || self.menu.ID == ProcessManager.ProcessID.RegionsOverviewScreen)
            {
                CustomWorldMod.CustomWorldLog($"Custom Regions: Adding Title - Name [{$"Title_{regionID}"}], path [{titleFolderName}]");
                self.AddIllustration(new MenuIllustration(self.menu, self, titleFolderName, $"Title_{regionID}_Shadow", new Vector2(0.01f, 0.01f), true, false));
                self.AddIllustration(new MenuIllustration(self.menu, self, titleFolderName, $"Title_{regionID}", new Vector2(0.01f, 0.01f), true, false));
                self.flatIllustrations[self.flatIllustrations.Count - 1].sprite.shader = self.menu.manager.rainWorld.Shaders["MenuText"];
            }
        }

        public static void LoadScenePositionSettings(MenuScene self, string sceneFolder, string regionID)
        {
            CustomWorldMod.CustomWorldLog($"Custom Regions: Loading settings for Illustration at [{sceneFolder}]");
            string[] readingTextFile = new string[0];
            if (File.Exists(sceneFolder + "positions.txt"))
            {
                CustomWorldMod.CustomWorldLog($"Custom Regions: Reading positions.txt for {regionID}");
                readingTextFile = File.ReadAllLines(sceneFolder + "positions.txt");

                int num2 = 0;
                while (num2 < readingTextFile.Length && num2 < self.depthIllustrations.Count)
                {
                    self.depthIllustrations[num2].pos.x = float.Parse(Regex.Split(readingTextFile[num2], ", ")[0]);
                    self.depthIllustrations[num2].pos.y = float.Parse(Regex.Split(readingTextFile[num2], ", ")[1]);
                    self.depthIllustrations[num2].lastPos = self.depthIllustrations[num2].pos;
                    CustomWorldMod.CustomWorldLog($"Custom Regions: Position file Number [{num2 + 1}] position loaded [{self.depthIllustrations[num2].pos}]");
                    num2++;
                }
            }
            if (File.Exists(sceneFolder + "depths.txt"))
            {
                CustomWorldMod.CustomWorldLog($"Custom Regions: Reading depths.txt for {regionID}");
                readingTextFile = File.ReadAllLines(sceneFolder + "depths.txt");

                int num2 = 0;
                while (num2 < readingTextFile.Length && num2 < self.depthIllustrations.Count)
                {
                    self.depthIllustrations[num2].depth = float.Parse(readingTextFile[num2]);
                    CustomWorldMod.CustomWorldLog($"Custom Regions: Depths file Number [{num2 + 1}] position loaded [{self.depthIllustrations[num2].depth}]");
                    num2++;
                }
            }
        }

        private static void MenuScene_BuildScene(On.Menu.MenuScene.orig_BuildScene orig, Menu.MenuScene self)
        {
           /* if(self.sceneID == EnumExt_extendedSceneID.CustomSceneID)
            {
                self.sceneID = MenuScene.SceneID.MainMenu;
            }*/
            orig(self);

            if (self.sceneID == EnumExt_extendedSceneID.CustomSceneID && CustomWorldMod.sceneCustomID != string.Empty)
            {
                CustomWorldMod.CustomWorldLog($"Custom Regions: Building custom scene [{CustomWorldMod.sceneCustomID}]");
                Vector2 vector = new Vector2(0f, 0f);
                //bool notFound = true;
                foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
                {
                    string path = CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar;
                    string sceneFolder = path + "Assets" + Path.DirectorySeparatorChar + "Futile" + Path.DirectorySeparatorChar + "Resources"+ Path.DirectorySeparatorChar + "Scenes" + Path.DirectorySeparatorChar + $"Landscape - {keyValues.Key}" + Path.DirectorySeparatorChar;
                    //CustomWorldMod.CustomWorldLog($"Custom Regions: Searching assets at {sceneFolder}");
                    if (Directory.Exists(sceneFolder) && keyValues.Key.Equals(CustomWorldMod.sceneCustomID))
                    {
                        CustomWorldMod.CustomWorldLog($"Custom Regions: Found custom scene [{sceneFolder}]");
                        //notFound = false;
                        self.sceneFolder = sceneFolder;
                        BuildCustomRegionScene(self, keyValues.Key, sceneFolder);
                    }
                }

            }
        }

    }
}
