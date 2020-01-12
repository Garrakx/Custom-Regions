using CustomRegions.Mod;
using Menu;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CustomRegions
{
    static class MenuSceneHook
    {
        public static void ApplyHook()
        {
            On.Menu.MenuScene.BuildScene += MenuScene_BuildScene;
        }

        public static void BuildCustomRegionScene(MenuScene self, string regionID, string sceneFolder)
        {
            if (self.flatMode)
            {
                self.AddIllustration(new MenuIllustration(self.menu, self, self.sceneFolder, $"Landscape - {regionID} - Flat", new Vector2(683f, 384f), false, true));
            }
            else
            {
                string[] positions = new string[0];
                if(File.Exists(sceneFolder + "positions.txt"))
                {
                    Debug.Log($"Custom Regions: Reading positions.txt for {regionID}");
                    positions = File.ReadAllLines(sceneFolder + "positions.txt");
                }
                string[] fileEntries = Directory.GetFiles(sceneFolder);
                Array.Sort(fileEntries);
                fileEntries.Reverse();
                foreach (string fileName in fileEntries)
                {
                    if (fileName.Contains(".png") && !fileName.ToLower().Contains("flat"))
                    {
                        string name = Path.GetFileNameWithoutExtension(fileName);
                        int number = name[name.Length - 1];
                        Vector2 posVector = new Vector2(20, 20);
                        if (number < positions.Length)
                        {
                            posVector.x = float.Parse(positions[number].Split(new string[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries)[0]);
                            posVector.y = float.Parse(positions[number].Split(new string[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries)[1]);
                        }
                        Debug.Log($"Custom Regions: Adding MenuDepthIllustration - Name [{name}] - Number[{number}] - Position[{posVector}]");
                        self.AddIllustration(new MenuDepthIllustration(self.menu, self, self.sceneFolder, $"{name}", posVector, 4 + number, MenuDepthIllustration.MenuShader.Normal));
                    }
                }
            }

            if (self.menu.ID == ProcessManager.ProcessID.FastTravelScreen || self.menu.ID == ProcessManager.ProcessID.RegionsOverviewScreen)
            {
                self.AddIllustration(new MenuIllustration(self.menu, self, string.Empty, $"Title_{regionID}_Shadow", new Vector2(0.01f, 0.01f), true, false));
                self.AddIllustration(new MenuIllustration(self.menu, self, string.Empty, $"Title_{regionID}", new Vector2(0.01f, 0.01f), true, false));
                self.flatIllustrations[self.flatIllustrations.Count - 1].sprite.shader = self.menu.manager.rainWorld.Shaders["MenuText"];
            }
        }

        private static void MenuScene_BuildScene(On.Menu.MenuScene.orig_BuildScene orig, Menu.MenuScene self)
        {
            orig(self);
            if (self.sceneID == Menu.MenuScene.SceneID.Empty && CustomWorldMod.sceneCustomID != string.Empty)
            {
                Debug.Log($"Custom Regions: Building custom scene [{CustomWorldMod.sceneCustomID}]");
                Vector2 vector = new Vector2(0f, 0f);
                //bool notFound = true;
                foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
                {
                    string path = CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar;
                    string sceneFolder = path + "Assets" + Path.DirectorySeparatorChar + "Futile" + Path.DirectorySeparatorChar + "Resources"+ Path.DirectorySeparatorChar + "Scenes" + Path.DirectorySeparatorChar + $"Landscape - {keyValues.Key}";
                    if (File.Exists(sceneFolder))
                    {
                        Debug.Log($"Custom Regions: Found custom scene [{sceneFolder}]");
                        //notFound = false;
                        BuildCustomRegionScene(self, keyValues.Value, sceneFolder);
                    }
                }

            }
        }

    }
}
