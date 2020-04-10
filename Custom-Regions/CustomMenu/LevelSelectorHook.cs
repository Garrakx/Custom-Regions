using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CustomRegions.Mod;
using Menu;
using RWCustom;
using UnityEngine;

namespace CustomRegions.CustomMenu
{
    static class LevelSelectorHook
    {
        public static void ApplyHook()
        {
            //On.Menu.LevelSelector.Update += LevelSelector_Update;
        }

        private static void LevelSelector_Update(On.Menu.LevelSelector.orig_Update orig, Menu.LevelSelector self)
        {
            string debug2 = $"Custom Regions: Pending thumbnails [";

            foreach (string thumb in self.GetMultiplayerMenu.thumbsToBeLoaded)
            {
                debug2 += thumb + ", ";

            }
            //Debug.Log(debug2 + "]");

            if (self.thumbLoadDelay > 0)
            {
                self.thumbLoadDelay--;
            }
            if (self.GetMultiplayerMenu.thumbsToBeLoaded.Count > 0)
            {
                string text = self.GetMultiplayerMenu.thumbsToBeLoaded[0];
                string url = string.Empty;
                Debug.Log($"Custom regions: Trying to load thumbnail [{text}]");
                foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
                {
                    string path = Custom.RootFolderDirectory() + CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar + "Levels" + Path.DirectorySeparatorChar + text + "_Thumb.png";
                    if (File.Exists(path))
                    {
                        Debug.Log($"Custom regions: Found thumbnail [{path}]");
                        url = path;
                        break;
                    }

                }
                if (!url.Equals(string.Empty))
                {
                    vanillaLevelSelectorUpdate(self, url, true);
                    return;
                }
            }
            //orig(self);
        }

        public static void vanillaLevelSelectorUpdate(LevelSelector self, string url, bool flag)
        {
            self.Update();
            if (self.bumpVisible)
            {
                self.bumpVisible = false;
                self.allLevelsList.BumpVisible();
                if (self.levelsPlaylist != null)
                {
                    self.levelsPlaylist.BumpVisible();
                }
            }
            if (self.thumbLoadDelay < 1)
            {
                string text = self.GetMultiplayerMenu.thumbsToBeLoaded[0];
                self.GetMultiplayerMenu.thumbsToBeLoaded.RemoveAt(0);

                WWW www = new WWW(url);
                Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                texture2D.wrapMode = TextureWrapMode.Clamp;
                www.LoadImageIntoTexture(texture2D);
                if (!flag)
                {
                    TextureScale.Bilinear(texture2D, LevelSelector.ThumbWidth, LevelSelector.ThumbHeight);
                }
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        texture2D.SetPixel((i <= 1) ? (LevelSelector.ThumbWidth - 1) : 0, (i % 2 != 0) ? (LevelSelector.ThumbHeight - 1 - j) : j, new Color(0f, 0f, 0f, 0f));
                    }
                    for (int k = 0; k < 3; k++)
                    {
                        texture2D.SetPixel((i <= 1) ? (LevelSelector.ThumbWidth - 2) : 1, (i % 2 != 0) ? (LevelSelector.ThumbHeight - 1 - k) : k, new Color(0f, 0f, 0f, 0f));
                    }
                    for (int l = 0; l < 2; l++)
                    {
                        texture2D.SetPixel((i <= 1) ? (LevelSelector.ThumbWidth - 3) : 2, (i % 2 != 0) ? (LevelSelector.ThumbHeight - 1 - l) : l, new Color(0f, 0f, 0f, 0f));
                    }
                    texture2D.SetPixel((i <= 1) ? (LevelSelector.ThumbWidth - 4) : 3, (i % 2 != 0) ? (LevelSelector.ThumbHeight - 1) : 0, new Color(0f, 0f, 0f, 0f));
                    texture2D.SetPixel((i <= 1) ? (LevelSelector.ThumbWidth - 5) : 4, (i % 2 != 0) ? (LevelSelector.ThumbHeight - 1) : 0, new Color(0f, 0f, 0f, 0f));
                }
                texture2D.filterMode = FilterMode.Point;
                texture2D.Apply();
                self.GetMultiplayerMenu.loadedThumbTextures.Add(text);
                HeavyTexturesCache.LoadAndCacheAtlasFromTexture(text + "_Thumb", texture2D);
                for (int m = 0; m < self.subObjects.Count; m++)
                {
                    if (self.subObjects[m] is LevelSelector.LevelsList)
                    {
                        for (int n = 0; n < (self.subObjects[m] as LevelSelector.LevelsList).levelItems.Count; n++)
                        {
                            if ((self.subObjects[m] as LevelSelector.LevelsList).levelItems[n].name == text)
                            {
                                (self.subObjects[m] as LevelSelector.LevelsList).levelItems[n].ThumbnailHasBeenLoaded();
                            }
                        }
                    }
                    else if (self.subObjects[m] is LevelSelector.SingleLevelDisplay && (self.subObjects[m] as LevelSelector.SingleLevelDisplay).currentLevelItem.name == text)
                    {
                        (self.subObjects[m] as LevelSelector.SingleLevelDisplay).currentLevelItem.ThumbnailHasBeenLoaded();
                    }
                }
                self.thumbLoadDelay = 2;
            }
        }
    }
}
