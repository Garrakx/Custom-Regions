using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CustomRegions.Mod;
using Mono.Cecil;
using Mono.Cecil.Cil;
using static CustomRegions.RegionProperties.RegionProperties;

namespace CustomRegions.RegionProperties
{
    internal static class CycleRelatedHooks
    {
        public static void ApplyHooks()
        {
            On.HUD.Map.ctor += Map_ctor;
            On.RoomRain.ThrowAroundObjects += RoomRain_ThrowAroundObjects;
            On.WormGrass.ctor += WormGrass_ctor;

            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            new Hook(typeof(RainCycle).GetProperty(nameof(RainCycle.MusicAllowed), bindingFlags).GetGetMethod(), RainCycle_MusicAllowed);
            new Hook(typeof(RainCycle).GetProperty(nameof(RainCycle.RegionHidesTimer), bindingFlags).GetGetMethod(), RainCycle_RegionHidesTimer);
            On.HUD.RainMeter.ctor += RainMeter_ctor;
            On.HUD.RainMeter.Draw += RainMeter_Draw;
            On.HUD.RainMeter.Update += RainMeter_Update;
            On.RainCycle.GetDesiredCycleLength += RainCycle_GetDesiredCycleLength;
            On.OverseerAbstractAI.ctor += OverseerAbstractAI_ctor;
            IL.RainCycle.ctor += RainCycle_ctor;
            On.RainCycle.Update += RainCycle_Update;
        }

        private static void RainCycle_Update(On.RainCycle.orig_Update orig, RainCycle self)
        {
            orig(self);
            if (self.timer >= self.sunDownStartTime && self.dayNightCounter == 2640 &&
                self.world.region?.GetCRSProperties().sundownMusic is string song && 
                self.world.game.manager.musicPlayer != null && self.world.game.world.rainCycle.MusicAllowed)
            {
                self.world.game.manager.musicPlayer.GameRequestsSong(new()
                {
                    cyclesRest = 5,
                    stopAtDeath = false,
                    stopAtGate = true,
                    songName = song
                });
            }

        }

        private static void RainCycle_ctor(ILContext il)
        {
            var c = new ILCursor(il);
            int index = 0;
            if (c.TryGotoNext(MoveType.AfterLabel,
                x => x.MatchLdsfld<MoreSlugcats.MoreSlugcats>(nameof(MoreSlugcats.MoreSlugcats.cfgDisablePrecycles)),
                x => x.MatchCallvirt(out _),
                x => x.MatchBrfalse(out _),
                x => x.MatchLdcR4(0),
                x => x.MatchStloc(out index)
                ))
            {
                c.Emit(OpCodes.Ldloc, index);
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((float orig, RainCycle self) =>
                {
                    StoryGameSession session = !self.world.singleRoomWorld && self.world.game.IsStorySession ? self.world.game.GetStorySession : null;
                    if (session != null)
                    {
                        if (session.saveState.saveStateNumber == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Rivulet && !session.saveState.miscWorldSaveData.pebblesEnergyTaken)
                        {
                            if (self.world.region?.GetCRSProperties().rivStormyPreCycleChance is float f)
                            {
                                return f * 100f;
                            }
                        }
                    }
                    if (self.world.region?.GetCRSProperties().forcePreCycleChance is float f2) return f2 * 100f;
                    if (self.world.region?.regionParams.earlyCycleChance == 0f) return 0f;
                    return orig;
                });
                c.Emit(OpCodes.Stloc, index);
            }
            else
            {
                CustomRegionsMod.BepLogError("CustomRegions.RegionProperties.CycleRelatedHooks.RainCycle_ctor: IL Hook failed.");
            }
        }

        private static void OverseerAbstractAI_ctor(On.OverseerAbstractAI.orig_ctor orig, OverseerAbstractAI self, World world, AbstractCreature parent)
        {
            orig(self, world, parent);
            if (self.world.region?.GetCRSProperties().hideTimer is bool b && b)
            {
                self.parent.ignoreCycle = b;
            }
        }

        private static int RainCycle_GetDesiredCycleLength(On.RainCycle.orig_GetDesiredCycleLength orig, RainCycle self)
        {
            int result = orig(self);
            StoryGameSession session = !self.world.singleRoomWorld && self.world.game.IsStorySession ? self.world.game.GetStorySession : null;
            if (session != null && session.saveState.saveStateNumber == MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Rivulet &&
                !session.saveState.miscWorldSaveData.pebblesEnergyTaken)

            {
                if (self.world.region?.GetCRSProperties().rivStormyCycleLength is float r)
                {
                    //undo the multiplication of orig
                    result *= self.world.region.name switch
                    {
                        "VS" or "UW" or "SH" or "SB" or "SL" => 2,
                        _ => 3
                    };

                    result = (int)(result * r);
                }
            }

            if (self.world.region?.GetCRSProperties().cycleLength is float f)
            {
                result = (int)(result * f);
            }

            return result;
        }


        #region map & end cycle changes

        private static void Map_ctor(On.HUD.Map.orig_ctor orig, HUD.Map self, HUD.HUD hud, HUD.Map.MapData mapData)
        {
            orig(self, hud, mapData);
            Region region = (self.hud.rainWorld.processManager.currentMainLoop as RainWorldGame)?.overWorld.regions.SingleOrDefault(x => x.name == self.RegionName);
            if (region?.GetCRSProperties().mapDefaultMatLayers != null)
            {
                self.STANDARDELEMENTLIST = region.GetCRSProperties().mapDefaultMatLayers;
            }
        }

        public static CRSProperties GetCRSProperties(this HUD.HUD hud) => (hud.owner as Player)?.abstractCreature.world.region?.GetCRSProperties();

        private static void RainMeter_Update(On.HUD.RainMeter.orig_Update orig, HUD.RainMeter self)
        {
            orig(self);
            if (self.halfTimeShown && self.hud.GetCRSProperties()?.hideTimer is bool b && !b)
            {
                self.halfTimeShown = (self.hud.owner as Player).room.world.rainCycle.AmountLeft < 0.5f;
            }
        }

        private static void RainMeter_Draw(On.HUD.RainMeter.orig_Draw orig, HUD.RainMeter self, float timeStacker)
        {
            orig(self, timeStacker);

            if (self.hud.GetCRSProperties()?.hideTimer is bool b && !b)
                foreach (HUD.HUDCircle circle in self.circles)
                    circle.Draw(timeStacker);
        }

        private static void RainMeter_ctor(On.HUD.RainMeter.orig_ctor orig, HUD.RainMeter self, HUD.HUD hud, FContainer fContainer)
        {
            orig(self, hud, fContainer);
            if (self.halfTimeShown && self.hud.GetCRSProperties()?.hideTimer is bool b && !b)
            {
                self.halfTimeShown = false;
            }
        }

        private static bool RainCycle_RegionHidesTimer(Func<RainCycle, bool> orig, RainCycle self)
        {
            if (self.world.region?.GetCRSProperties().hideTimer is bool b)
                return b;
            return orig(self);
        }

        private static bool RainCycle_MusicAllowed(Func<RainCycle, bool> orig, RainCycle self)
        {
            if (self.world.region?.GetCRSProperties().musicAfterCycle is bool b)
                return b;
            return orig(self);
        }

        private static void WormGrass_ctor(On.WormGrass.orig_ctor orig, WormGrass self, Room room, List<RWCustom.IntVector2> tiles)
        {
            orig(self, room, tiles);
            if (room.world.region?.GetCRSProperties().wormGrassLight is not bool b)
            { return; }
            foreach (WormGrass.WormGrassPatch patch in self.patches)
            { patch.InitRegionalLight(b); }
        }

        private static void RoomRain_ThrowAroundObjects(On.RoomRain.orig_ThrowAroundObjects orig, RoomRain self)
        {
            float? num = self.room.world.region?.GetCRSProperties().throwObjectsThreshold;
            if (num is float f && self.room.roomSettings.RainIntensity <= f)
            { return; }
            orig(self);
        }

        #endregion

    }
}
