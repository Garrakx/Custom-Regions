using RWCustom;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using CustomRegions.CustomMenu;

namespace CustomRegions.Mod
{
    public class CustomWorldScript : MonoBehaviour
    {

        public void Initialize()
        {
            CustomWorldMod.script = this;

            CustomWorldMod.CreateCustomWorldLog();
            CustomWorldMod.CreateCustomResourceFolder();

            CustomWorldMod.loadedRegions = CustomWorldMod.BuildModRegionsDictionary();
            string dictionaryString = "Custom Regions: Loading \n{";
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
            {
                dictionaryString += keyValues.Key + " : " + keyValues.Value + ", ";
            }
            Debug.Log(dictionaryString.TrimEnd(',', ' ') + "}");
            CustomWorldMod.CustomWorldLog(dictionaryString.TrimEnd(',', ' ') + "}");

            MapHook.ApplyHook();
            RegionGateHook.ApplyHooks();
            RegionHook.ApplyHook();
            RoomSettingsHook.ApplyHook();
            WorldHook.ApplyHook();
            WorldLoaderHook.ApplyHooks();
            OverWorldHook.ApplyHooks();
            PlayerProgressionHook.ApplyHooks();

            // Custom Palette
            RoomCameraHook.ApplyHook();

            // Custom Decal
            CustomDecalHook.ApplyHook();

            // Scene
            FastTravelScreenHook.ApplyHooks();
            MainMenuHook.ApplyHooks();
            MenuSceneHook.ApplyHook();
            MenuIllustrationHook.ApplyHook();
            SlugcatSelectMenuHook.ApplyHooks();
            //LevelSelectorHook.ApplyHook();

            // Arena
            MultiplayerMenuHook.ApplyHook();
            ArenaCreatureSpawnerHook.ApplyHook();

            // WinState - achievement
            WinStateHook.ApplyHook();

        }

        public RainWorld rw;
        public static CustomWorldMod mod;
        public static ProcessManager pm;

        public void Update()
        {
            if (rw == null)
            {
                rw = FindObjectOfType<RainWorld>();
                pm = rw.processManager;
            }
        }

    }

}
