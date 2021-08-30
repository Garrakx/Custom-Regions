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
            //TO-DO ScavengerAbstractAIHook.ApplyHooks();
            TentaclePlantGraphicsHook.ApplyHooks();

            /// CUSTOM MENU ///
            FastTravelScreenHook.ApplyHooks();
            MenuIllustrationHook.ApplyHooks();
            MenuSceneHook.ApplyHooks();
            MultiplayerMenuHook.ApplyHooks();
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

        public static void RemoveAllHooks()
        {
            if (!CustomWorldMod.usingBepinex)
            {
                CustomWorldMod.Log($"Could not disable hooks since you are using Partiality! Consider upgrading to BepInEx", true);
                // Partiality crashes with -=
                return;
                // :(
            }

            /// ARENAS ///
            ArenaCreatureSpawnerHook.RemoveHooks();
            MultiplayerUnlocksHook.RemoveHooks();

            /// CREATURES ///
            BigEelHook.RemoveHooks();
            DaddyLongLegsHook.RemoveHooks();
            DaddyCorruptionHook.RemoveHooks();
            FlyGraphicsHook.RemoveHooks();
            LizardGraphicsHook.RemoveHooks();
            //TO-DO ScavengerAbstractAIHook.RemoveHooks();
            TentaclePlantGraphicsHook.RemoveHooks();

            /// CUSTOM MENU ///
            FastTravelScreenHook.RemoveHooks();
            MenuIllustrationHook.RemoveHooks();
            MenuSceneHook.RemoveHooks();
            MultiplayerMenuHook.RemoveHooks();
            SlugcatSelectMenuHook.RemoveHooks();

            /// CUSTOM PEARLS ///
            DataPearlHook.RemoveHooks();
            PlacedObjectHook.RemoveHooks();
            SLOracleBehaviorHasMarkHook.RemoveHooks();

            /// DEV INTERFACE ///
            MapPageHook.RemoveHooks();
            MapRenderOutputHook.RemoveHooks();
            CustomDecalRepresentationHook.RemoveHooks();
            SoundPageHook.RemoveHooks();
            TriggersPageHook.RemoveHooks();

            /// HUD ///
            MapHook.RemoveHooks();

            /// MUSIC ///
            MultiplayerDJHook.RemoveHooks();
            MusicPieceHook.RemoveHooks();
            ProceduralMusicInstructionsHook.RemoveHooks();
            SoundLoaderHook.RemoveHooks();


            /// WORLD ///
            OverWorldHook.RemoveHooks();
            RegionGateHook.RemoveHooks();
            RegionHook.RemoveHooks();
            WorldHook.RemoveHooks();
            WorldLoaderHook.RemoveHooks();


            MainLoopProcessHook.RemoveHooks();
            PlayerProgressionHook.RemoveHooks();
            // Rain world instance
            RainWorldHook.RemoveHooks();
            // Custom Palette
            RoomCameraHook.RemoveHooks();
            RoomHook.RemoveHooks();
            RoomSettingsHook.RemoveHooks();
            SaveStateHook.RemoveHooks();
            // WinState - achievement
            WinStateHook.RemoveHooks();
            // WWW
            WWWHook.RemoveHooks();
        }
    }
}
