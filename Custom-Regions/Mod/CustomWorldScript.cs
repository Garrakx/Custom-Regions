using RWCustom;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using CustomRegions.CustomMenu;
using CustomRegions.CustomPearls;
using CustomRegions.DevInterface;
using CustomRegions.Music;

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

            CustomWorldMod.DownloadThumbs();


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

            // Pearl
            DataPearlHook.ApplyHooks();
            SLOracleBehaviorHasMarkHook.ApplyHooks();

            // Rain world instance
            RainWorldHook.ApplyHooks();

            // Custom Palette
            RoomCameraHook.ApplyHook();


            // Electric gate
            RoomHook.ApplyHooks();
            WaterGateHook.ApplyHooks();

            // Custom Decal
            CustomDecalHook.ApplyHook();

            // Scene
            FastTravelScreenHook.ApplyHooks();
            MainMenuHook.ApplyHooks();
            MenuSceneHook.ApplyHook();
            MenuIllustrationHook.ApplyHook();
            SlugcatSelectMenuHook.ApplyHooks();

            // DevInterface
            MapPageHook.ApplyHooks();
            MapRenderOutputHook.ApplyHooks();

            // Arena
            MultiplayerMenuHook.ApplyHook();
            ArenaCreatureSpawnerHook.ApplyHook();

            // WinState - achievement
            WinStateHook.ApplyHook();

            SaveStateHook.ApplyHook();

            // MusicPiece
            MusicPieceHook.ApplyHooks();
            ProceduralMusicInstructionsHook.ApplyHooks();

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
