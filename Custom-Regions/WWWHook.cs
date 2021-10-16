using CustomRegions.CustomMenu;
using CustomRegions.Mod;
using MonoMod.RuntimeDetour;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CustomRegions
{
    static class WWWHook
    {
        public delegate void orig_WWW_ctor(WWW self, string url);
        private static IDetour hookWWWctor;

        public static void ApplyHooks()
        {
            hookWWWctor = new Hook(typeof(WWW).GetConstructor(new Type[] { typeof(string) }), typeof(WWWHook).GetMethod("WWW_ctor"));
        }

        public static void WWW_ctor(orig_WWW_ctor orig, WWW self, string url)
        {
            MultiplayerMenuHook.MultiplayerMenuUrl(ref url);

            if (url.Contains("file:///") && url.Contains("Resources") && url.Contains(".png"))
            {
                //CustomWorldMod.Log("Transforming URL " + url);
                string firstPath = "file:///" + Custom.RootFolderDirectory();
                string trimmedURL = url.Substring(url.IndexOf(firstPath) + firstPath.Length);
                foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
                {
                    string path = Custom.RootFolderDirectory() + CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar + trimmedURL;
                    //CustomWorldMod.Log($"Loading effectPalette / palette [{path}]");
                    if (File.Exists(path))
                    {
                        CustomWorldMod.Log($"Loaded custom resource [{path}]");
                        url = "file:///" + path;
                        break;
                    }
                }
            }
            orig(self, url);
        }
    }
}
