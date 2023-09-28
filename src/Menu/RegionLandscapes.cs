using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Globalization;
using CustomRegions.Mod;

namespace CustomRegions.CustomMenu
{
    internal static class RegionLandscapes
    {
        public static void ApplyHooks()
        {
            On.Region.GetRegionLandscapeScene += Region_GetRegionLandscapeScene;
            On.Menu.MenuScene.BuildScene += MenuScene_BuildScene;
        }

        public static List<Menu.MenuScene.SceneID> customLandscapes = new List<Menu.MenuScene.SceneID>();

        public static void RefreshLandscapes()
        {
            UnregisterLandscapes();
            RegisterNewLandscapes();
        }

        public static void UnregisterLandscapes()
        {
            foreach (Menu.MenuScene.SceneID landscape in customLandscapes) { landscape?.Unregister(); }

            customLandscapes = new List<Menu.MenuScene.SceneID>();
        }

        public static void RegisterNewLandscapes()
        {
            string path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar.ToString() + "regions.txt");
            if (File.Exists(path)) {
                foreach (string text in File.ReadAllLines(path)) {
                   // CustomRegionsMod.CustomLog("text " + text);
                    Menu.MenuScene.SceneID local = RegisterMenuScenes(text);
                    if (local != null) {
                        customLandscapes.Add(local);
                    }
                }
            }
        }

        public static Menu.MenuScene.SceneID RegisterMenuScenes(string name)
        {
            CustomRegionsMod.CustomLog("\n[MENU SCENE] Registering menu scenes...");
            string sceneName = "Landscape - " + name;
            name = "Landscape_" + name;
            CustomRegionsMod.CustomLog($"[MENU SCENE] checking if [{name}] needs new enum", CustomRegionsMod.DebugLevel.FULL);
            if (ExtEnumBase.TryParse(typeof(Menu.MenuScene.SceneID), name, false, out _)) {
                CustomRegionsMod.CustomLog($"[MENU SCENE] enum {name} already exists, skipping", CustomRegionsMod.DebugLevel.FULL);
                return null;
            } else if (Directory.Exists(AssetManager.ResolveDirectory("Scenes" + Path.DirectorySeparatorChar.ToString() + sceneName))) {
                CustomRegionsMod.CustomLog($"[MENU SCENE] success! Registered new enum {name}");
                return new Menu.MenuScene.SceneID(name, true);
            } else { return null; }
        }


        private static void MenuScene_BuildScene(On.Menu.MenuScene.orig_BuildScene orig, Menu.MenuScene self)
        {
            orig(self);

            CustomStaticCache.CheckForRefresh();

            if ((self.sceneFolder == "" || self.sceneFolder == null) && customLandscapes.Contains(self.sceneID)) { BuildCustomScene2(self); }
        }

        public static void LoadPositions(Menu.MenuScene scene)
        {

            string path2 = AssetManager.ResolveFilePath(scene.sceneFolder + Path.DirectorySeparatorChar.ToString() + "positions_ims.txt");
            if (!File.Exists(path2) || scene is not Menu.InteractiveMenuScene) {
                path2 = AssetManager.ResolveFilePath(scene.sceneFolder + Path.DirectorySeparatorChar.ToString() + "positions.txt");
            }
            if (File.Exists(path2)) {
                string[] array3 = File.ReadAllLines(path2);
                int num3 = 0;
                while (num3 < array3.Length && num3 < scene.depthIllustrations.Count) {
                    scene.depthIllustrations[num3].pos.x = float.Parse(Regex.Split(RWCustom.Custom.ValidateSpacedDelimiter(array3[num3], ","), ", ")[0], NumberStyles.Any, CultureInfo.InvariantCulture);
                    scene.depthIllustrations[num3].pos.y = float.Parse(Regex.Split(RWCustom.Custom.ValidateSpacedDelimiter(array3[num3], ","), ", ")[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                    scene.depthIllustrations[num3].lastPos = scene.depthIllustrations[num3].pos;
                    num3++;
                }
            }

            path2 = AssetManager.ResolveFilePath(scene.sceneFolder + Path.DirectorySeparatorChar.ToString() + "depths.txt");

            if (File.Exists(path2)) {
                string[] array = File.ReadAllLines(path2);
                int num2 = 0;
                while (num2 < array.Length && num2 < scene.depthIllustrations.Count) {
                    scene.depthIllustrations[num2].depth = float.Parse(array[num2]);
                    num2++;
                }
            }


        }

        public static void BuildCustomScene2(Menu.MenuScene scene)
        {
            string[] array = scene.sceneID.ToString().Split('_');

            if (array.Length != 2)
                return;

            string fileName = $"{array[0]} - {array[1]}";
            string regionAcronym = array[1];
            scene.blurMin = -0.1f;
            scene.blurMax = 0.5f;

            scene.sceneFolder = "Scenes" + Path.DirectorySeparatorChar.ToString() + fileName;

            if (!Directory.Exists(AssetManager.ResolveDirectory(scene.sceneFolder)) || Directory.GetFiles(AssetManager.ResolveDirectory(scene.sceneFolder)).Length == 0) { goto LandscapeTitle; }


            if (scene.flatMode) {
                scene.AddIllustration(new Menu.MenuIllustration(scene.menu, scene, scene.sceneFolder, fileName + " - Flat", new Vector2(683f, 384f), false, true));
                goto LandscapeTitle;
            }

            string path = scene.sceneFolder + Path.DirectorySeparatorChar.ToString() + fileName + ".txt";

            if (!File.Exists(AssetManager.ResolveFilePath(path))) { goto LandscapeTitle; }

            foreach (string line in File.ReadAllLines(AssetManager.ResolveFilePath(path))) {
                string[] array2 = Regex.Split(line, " : ");

                if (array2.Length == 0 || array2[0].Length == 0) { continue; }

                bool layer = false;
                if (array2.Length >= 2 && float.TryParse(array2[1], out float result2))
                {
                    switch (array2[0])
                    {
                        case "blurMin":
                            scene.blurMin = result2;
                            break;
                        case "blurMax":
                            scene.blurMax = result2;
                            break;
                        case "idleDepths":
                            (scene as Menu.InteractiveMenuScene)?.idleDepths.Add(result2);
                            break;
                        default:
                            layer = true;
                            break;
                    }
                }
                else { layer = true; }
                if (layer && File.Exists(AssetManager.ResolveFilePath(scene.sceneFolder + Path.DirectorySeparatorChar.ToString() + array2[0] + ".png")))
                {
                    scene.AddIllustration(new Menu.MenuDepthIllustration(
                        scene.menu, scene, scene.sceneFolder, array2[0], new Vector2(0f, 0f),
                        (array2.Length >= 2 && int.TryParse(array2[1], out int r) ? r : 1),
                        (array2.Length >= 3 && ExtEnumBase.TryParse(typeof(Menu.MenuDepthIllustration.MenuShader), array2[2], false, out ExtEnumBase result) ? (Menu.MenuDepthIllustration.MenuShader)result : Menu.MenuDepthIllustration.MenuShader.Normal)
                        ));
                }
            }

            LoadPositions(scene);

        LandscapeTitle:
            if (scene.menu.ID == ProcessManager.ProcessID.FastTravelScreen || scene.menu.ID == ProcessManager.ProcessID.RegionsOverviewScreen) {
                scene.AddIllustration(new Menu.MenuIllustration(scene.menu, scene, string.Empty, $"Title_{regionAcronym}_Shadow", new Vector2(0.01f, 0.01f), true, false));
                scene.AddIllustration(new Menu.MenuIllustration(scene.menu, scene, string.Empty, $"Title_{regionAcronym}", new Vector2(0.01f, 0.01f), true, false));
                scene.flatIllustrations[scene.flatIllustrations.Count - 1].sprite.shader = scene.menu.manager.rainWorld.Shaders["MenuText"];
            }

        }

        private static Menu.MenuScene.SceneID Region_GetRegionLandscapeScene(On.Region.orig_GetRegionLandscapeScene orig, string regionAcro)
        {
            CustomStaticCache.CheckForRefresh();

            CustomRegionsMod.CustomLog("trying to load Landscape_" + regionAcro);
            if (ExtEnumBase.TryParse(typeof(Menu.MenuScene.SceneID), "Landscape_" + regionAcro, false, out ExtEnumBase result)) {
                return (Menu.MenuScene.SceneID) result;
            }
            return orig(regionAcro);
        }

    }
}
