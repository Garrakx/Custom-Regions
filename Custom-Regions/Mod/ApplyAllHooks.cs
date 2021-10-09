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
            // Ordered by folders in alphabetic


            /// ARENAS ///
            ArenaCreatureSpawnerHook.ApplyHooks();
            MultiplayerUnlocksHook.ApplyHooks();

            /// CREATURES ///
            BigEelHook.ApplyHooks();
            DaddyCorruptionHook.ApplyHooks();
            DaddyLongLegsHook.ApplyHooks();
            FlyGraphicsHook.ApplyHooks();
            LizardGraphicsHook.ApplyHooks();
            ScavengerAbstractAIHook.ApplyHooks();
            TentaclePlantGraphicsHook.ApplyHooks();

            /// CUSTOM MENU ///
            FastTravelScreenHook.ApplyHooks();
            MenuIllustrationHook.ApplyHooks();
            MenuSceneHook.ApplyHooks();
            MultiplayerMenuHook.ApplyHooks();
            PauseMenuHook.ApplyHooks();
            SlugcatSelectMenuHook.ApplyHooks();

            /// CUSTOM PEARLS ///
            DataPearlHook.ApplyHooks();
            PlacedObjectHook.ApplyHooks();
            SLOracleBehaviorHasMarkHook.ApplyHooks();

            /// DEV INTERFACE ///
            MapPageHook.ApplyHooks();
            MapRenderOutputHook.ApplyHooks();
            CustomDecalRepresentationHook.ApplyHooks();
            SoundPageHook.ApplyHooks();
            TriggersPageHook.ApplyHooks();

            /// HUD ///
            MapHook.ApplyHooks();

            /// MUSIC ///
            MultiplayerDJHook.ApplyHooks();
            MusicPieceHook.ApplyHooks();
            ProceduralMusicInstructionsHook.ApplyHooks();
            SoundLoaderHook.ApplyHooks();


            /// WORLD ///
            OverWorldHook.ApplyHooks();
            RegionGateHook.ApplyHooks();
            RegionHook.ApplyHooks();
            WorldHook.ApplyHooks();
            WorldLoaderHook.ApplyHooks();


            MainLoopProcessHook.ApplyHooks();
            PlayerProgressionHook.ApplyHooks();
            // Rain world instance
            RainWorldHook.ApplyHooks();
            // Custom Palette
            RoomCameraHook.ApplyHooks();
            RoomHook.ApplyHooks();
            RoomSettingsHook.ApplyHooks();
            SaveStateHook.ApplyHooks();
            // WinState - achievement
            WinStateHook.ApplyHooks();
            // WWW
            WWWHook.ApplyHooks();
        }
    }
}
