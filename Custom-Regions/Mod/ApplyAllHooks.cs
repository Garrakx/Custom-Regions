using CustomRegions.Arena;
using CustomRegions.Creatures;
using CustomRegions.CustomMenu;
using CustomRegions.CustomPearls;
using CustomRegions.CWorld;
using CustomRegions.DevInterface;
using CustomRegions.HUDs;
using CustomRegions.Music;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CustomRegions.Mod
{
    static class Hooks
    {
        public static void ApplyAllHooks()
        {
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
            PlacedObjectHook.ApplyHooks();

            // Rain world instance
            RainWorldHook.ApplyHooks();

            // Custom Palette
            RoomCameraHook.ApplyHook();

            // Electric gate
            RoomHook.ApplyHooks();
            WaterGateHook.ApplyHooks();

            // Custom Decal
            //CustomDecalHook.ApplyHook();

            MainLoopProcessHook.ApplyHooks();

            // Scene
            FastTravelScreenHook.ApplyHooks();
            //MainMenuHook.ApplyHooks();
            MenuSceneHook.ApplyHook();
            MenuIllustrationHook.ApplyHook();
            SlugcatSelectMenuHook.ApplyHooks();

            // DevInterface
            MapPageHook.ApplyHooks();
            MapRenderOutputHook.ApplyHooks();
            CustomDecalRepresentationHook.ApplyHooks();

            // Arena
            MultiplayerMenuHook.ApplyHook();
            ArenaCreatureSpawnerHook.ApplyHook();

            // WinState - achievement
            WinStateHook.ApplyHook();

            SaveStateHook.ApplyHook();

            // MusicPiece
            MusicPieceHook.ApplyHooks();
            ProceduralMusicInstructionsHook.ApplyHooks();

            BigEelHook.ApplyHooks();
            TentaclePlantGraphicsHook.ApplyHooks();
            DaddyLongLegsHook.ApplyHooks();
            LizardGraphicsHook.ApplyHooks();

            // WWW
            WWWHook.ApplyHooks();

            RegionStateHook.ApplyHooks();

            // Unlocks ID
            MultiplayerUnlocksHook.ApplyHooks();
        }
    }
}
