using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using RWCustom;
using System.Text.RegularExpressions;
using System.Globalization;

namespace CustomRegionsMod.Menu
{
    internal static class RegionLandscapes
    {
		public static void ApplyHooks()
		{
			On.Region.GetRegionLandscapeScene += Region_GetRegionLandscapeScene;
			On.Menu.MenuScene.BuildScene += MenuScene_BuildScene;
		}

		public static List<global::Menu.MenuScene.SceneID> customLandscapes = new List<global::Menu.MenuScene.SceneID>();

		public static void RefreshLandscapes()
		{
			UnregisterLandscapes();
			RegisterNewLandscapes();
		}

		public static void UnregisterLandscapes()
		{
			try
			{
				foreach (global::Menu.MenuScene.SceneID landscape in customLandscapes)
				{ if (landscape != null) { landscape.Unregister(); } }

                customLandscapes = new List<global::Menu.MenuScene.SceneID>();
			}
			catch (Exception e) { throw e; }
		}

		public static void RegisterNewLandscapes()
		{
			string path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar.ToString() + "regions.txt");
			if (File.Exists(path))
			{
				foreach (string text in File.ReadAllLines(path))
				{
					CustomRegionsMod.Log("text " + text);
					global::Menu.MenuScene.SceneID local = RegisterMenuScenes(text);
					if (local != null)
					{
						CustomRegionsMod.Log("Adding");
						customLandscapes.Add(local);
					}
				}
			}
		}

		public static global::Menu.MenuScene.SceneID RegisterMenuScenes(string name)
		{
			string sceneName = "Landscape - " + name;
			name = "Landscape_" + name;
			CustomRegionsMod.Log("new enum for " + name);
			if (ExtEnumBase.TryParse(typeof(global::Menu.MenuScene.SceneID), name, false, out _))
			{
                CustomRegionsMod.Log("already exists");
				return null;
			}
			else if (Directory.Exists(AssetManager.ResolveDirectory("Scenes" + Path.DirectorySeparatorChar.ToString() + sceneName)))
			{
                CustomRegionsMod.Log("success");
				return new global::Menu.MenuScene.SceneID(name, true);
			}
			else
			{ return null; }
		}


		private static void MenuScene_BuildScene(On.Menu.MenuScene.orig_BuildScene orig, global::Menu.MenuScene self)
		{
			orig(self);

			Progression.StoryRegionsMod.slugcatStoryStruct.CheckIfRegen();

			if ((self.sceneFolder == "" || self.sceneFolder == null) && customLandscapes.Contains(self.sceneID))
			{ BuildCustomScene2(self); }
		}

		public static void LoadPositions(global::Menu.MenuScene scene)
		{

			string path2 = AssetManager.ResolveFilePath(scene.sceneFolder + Path.DirectorySeparatorChar.ToString() + "positions_ims.txt");
			if (!File.Exists(path2) || !(scene is global::Menu.InteractiveMenuScene))
			{
				path2 = AssetManager.ResolveFilePath(scene.sceneFolder + Path.DirectorySeparatorChar.ToString() + "positions.txt");
			}
			if (File.Exists(path2))
			{
				string[] array3 = File.ReadAllLines(path2);
				int num3 = 0;
				while (num3 < array3.Length && num3 < scene.depthIllustrations.Count)
				{
					scene.depthIllustrations[num3].pos.x = float.Parse(Regex.Split(RWCustom.Custom.ValidateSpacedDelimiter(array3[num3], ","), ", ")[0], NumberStyles.Any, CultureInfo.InvariantCulture);
					scene.depthIllustrations[num3].pos.y = float.Parse(Regex.Split(RWCustom.Custom.ValidateSpacedDelimiter(array3[num3], ","), ", ")[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					scene.depthIllustrations[num3].lastPos = scene.depthIllustrations[num3].pos;
					num3++;
				}
			}

			path2 = AssetManager.ResolveFilePath(scene.sceneFolder + Path.DirectorySeparatorChar.ToString() + "depths.txt");

			if (File.Exists(path2))
			{
				string[] array = File.ReadAllLines(path2);
				int num2 = 0;
				while (num2 < array.Length && num2 < scene.depthIllustrations.Count)
				{
					scene.depthIllustrations[num2].depth = float.Parse(array[num2]);
					num2++;
				}
			}


		}

		public static void BuildCustomScene2(global::Menu.MenuScene scene)
		{
			string[] array = scene.sceneID.ToString().Split('_');

			if (array.Length != 2)
				return;

			string fileName = $"{array[0]} - {array[1]}";
			string regionAcronym = array[1];
			scene.blurMin = -0.1f;
			scene.blurMax = 0.5f;

			scene.sceneFolder = "Scenes" + Path.DirectorySeparatorChar.ToString() + fileName;

			if (!Directory.Exists(AssetManager.ResolveDirectory(scene.sceneFolder)) || Directory.GetFiles(AssetManager.ResolveDirectory(scene.sceneFolder)).Length == 0)
			{ goto LandscapeTitle; }


			if (scene.flatMode)
			{
				scene.AddIllustration(new global::Menu.MenuIllustration(scene.menu, scene, scene.sceneFolder, fileName + " - Flat", new Vector2(683f, 384f), false, true));
				goto LandscapeTitle;
			}

			string path = scene.sceneFolder + Path.DirectorySeparatorChar.ToString() + fileName + ".txt";

			if (!File.Exists(AssetManager.ResolveFilePath(path)))
			{ goto LandscapeTitle; }

			foreach (string line in File.ReadAllLines(AssetManager.ResolveFilePath(path)))
			{
				string[] array2 = Regex.Split(line, " : ");

				if (array2.Length == 0 || array2[0].Length == 0)
				{ continue; }

				if (array2[0] == "blurMin" && array2.Length >= 2)
				{ scene.blurMin = float.Parse(array2[1]); }

				else if (array2[0] == "blurMax" && array2.Length >= 2)
				{ scene.blurMax = float.Parse(array2[1]); }

				else if (array2[0] == "idleDepths" && array2.Length >= 2 && float.TryParse(array2[1], out float idleResult))
				{ (scene as global::Menu.InteractiveMenuScene)?.idleDepths.Add(idleResult); }

				else
				{
					if (File.Exists(AssetManager.ResolveFilePath(scene.sceneFolder + Path.DirectorySeparatorChar.ToString() + array2[0] + ".png")))
					{
						scene.AddIllustration(new global::Menu.MenuDepthIllustration(
							scene.menu, scene, scene.sceneFolder, array2[0], new Vector2(0f, 0f),
							(array2.Length >= 2 && int.TryParse(array2[1], out int r) ? r : 1),
							(array2.Length >= 3 && ExtEnumBase.TryParse(typeof(global::Menu.MenuDepthIllustration.MenuShader), array2[2], false, out ExtEnumBase result) ? (global::Menu.MenuDepthIllustration.MenuShader)result : global::Menu.MenuDepthIllustration.MenuShader.Normal)
							));
					}
				}
			}

			LoadPositions(scene);


		LandscapeTitle:;
			if (scene.menu.ID == ProcessManager.ProcessID.FastTravelScreen || scene.menu.ID == ProcessManager.ProcessID.RegionsOverviewScreen)
			{
				scene.AddIllustration(new global::Menu.MenuIllustration(scene.menu, scene, string.Empty, $"Title_{regionAcronym}_Shadow", new Vector2(0.01f, 0.01f), true, false));
				scene.AddIllustration(new global::Menu.MenuIllustration(scene.menu, scene, string.Empty, $"Title_{regionAcronym}", new Vector2(0.01f, 0.01f), true, false));
				scene.flatIllustrations[scene.flatIllustrations.Count - 1].sprite.shader = scene.menu.manager.rainWorld.Shaders["MenuText"];
			}

		}

		private static global::Menu.MenuScene.SceneID Region_GetRegionLandscapeScene(On.Region.orig_GetRegionLandscapeScene orig, string regionAcro)
		{
			Progression.StoryRegionsMod.slugcatStoryStruct.CheckIfRegen();

			CustomRegionsMod.Log("trying to load Landscape_" + regionAcro);
			if (ExtEnumBase.TryParse(typeof(global::Menu.MenuScene.SceneID), "Landscape_" + regionAcro, false, out ExtEnumBase result))
			{
				return (global::Menu.MenuScene.SceneID)result;
			}
			return orig(regionAcro);
		}

	}
}
