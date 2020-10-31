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
            CustomWorldMod.CreateCustomWorldFolders();

            CustomWorldMod.LoadCustomWorldResources();

            // Load from file
            CustomWorldMod.analyzingLog = string.Empty;

            MapHook.ApplyHook();
            RegionGateHook.ApplyHooks();
            RegionHook.ApplyHook();
            RoomSettingsHook.ApplyHook();
            WorldHook.ApplyHook();
            WorldLoaderHook.ApplyHooks();
            OverWorldHook.ApplyHooks();
            PlayerProgressionHook.ApplyHooks();

            // Rain wolrd instance
            RainWorldHook.ApplyHooks();

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

            SaveStateHook.ApplyHook();

            // MusicPiece
            MusicPieceHook.ApplyHooks();

        }

        public static CustomWorldMod mod;
        
        public RainWorld rw;
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
