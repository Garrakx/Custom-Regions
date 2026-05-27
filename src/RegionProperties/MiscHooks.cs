using CustomRegions.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace CustomRegions.RegionProperties
{

internal static class MiscHooks
    {
        public static void ApplyHooks()
        {
            On.VoidSpawnWorldAI.DirectionFinder.ctor += DirectionFinder_ctor;
            On.VoidSpawnWorldAI.Update += VoidSpawnWorldAI_Update;
            On.VoidSpawnEgg.AIMapReady += VoidSpawnEgg_AIMapReady;
            IL.Room.Loaded += Room_Loaded;

            On.Fly.Update += Fly_Update;
            On.FlyGraphics.ctor += FlyGraphics_ctor;
            On.FlyGraphics.ApplyPalette += FlyGraphics_ApplyPalette;

            On.DropBugAbstractAI.ctor += DropBugAbstractAI_ctor;
            On.DaddyLongLegs.ctor += DaddyLongLegs_ctor;
            On.FireFly.ctor += FireFly_ctor;
            On.TinyDragonfly.DrawSprites += TinyDragonfly_DrawSprites;
            On.SSLightRod.UpdateLightAmount += SSLightRod_UpdateLightAmount;

            On.SuperStructureFuses.ctor += SuperStructureFuses_ctor;
        }

        private static void SuperStructureFuses_ctor(On.SuperStructureFuses.orig_ctor orig, SuperStructureFuses self, PlacedObject placedObject, RWCustom.IntRect rect, Room room)
        {
            orig(self, placedObject, rect, room);
            if (self.room?.world.region?.GetCRSProperties().superStructureFusesBroken is float f)
            {
                self.broken = f;
            }
        }

        private static void SSLightRod_UpdateLightAmount(On.SSLightRod.orig_UpdateLightAmount orig, SSLightRod self)
        {
            //should be in SSLightRod.ctor, but has to happen before this method's orig is called
            if (self.room.game.world.region?.GetCRSProperties().lightRodColor is Color color)
            {
                self.color = color;
            }
            orig(self);

        }

        private static void FireFly_ctor(On.FireFly.orig_ctor orig, FireFly self, Room room, Vector2 pos)
        {
            orig(self, room, pos);
            if (room.world.region?.GetCRSProperties().fireflyColor is HSLColor color)
            {
                self.col = RWCustom.Custom.HSL2RGB(Mathf.Lerp(color.hue - 0.025f, color.hue + 0.025f, UnityEngine.Random.value), color.saturation, color.lightness);
            }
        }

        private static void TinyDragonfly_DrawSprites(On.TinyDragonfly.orig_DrawSprites orig, TinyDragonfly self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (self.room?.world.region?.GetCRSProperties().dragonflyColor is HSLColor color)
            {
                Vector2 vector2 = Vector3.Slerp(self.lastDir, self.dir, timeStacker);
                float num2 = Mathf.Pow(Mathf.InverseLerp(0.6f, 1f, Vector2.Dot(vector2, RWCustom.Custom.DegToVec(47f))), 2f) * Mathf.Pow(UnityEngine.Random.value, 0.5f);
                float hue = (Mathf.Lerp(color.hue - 0.05f, color.hue + 0.05f, self.hue) + 1f) % 1f;
                float sat = color.saturation + RWCustom.Custom.LerpMap(1f - color.saturation, 0.2f, 1f, 0.2f, 0.4f) * Mathf.Pow(num2, 0.3f);
                float lightness = Mathf.Min(color.lightness + 0.4f * num2, 1f);
                sLeaser.sprites[0].color = Color.Lerp(RWCustom.Custom.HSL2RGB(hue, sat, lightness), self.paletteBlack, 0.3f * (1f - num2) + 0.5f * self.paletteDarkness);
            }
        }


        private static void DaddyLongLegs_ctor(On.DaddyLongLegs.orig_ctor orig, DaddyLongLegs self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);

            var dict = world.region?.GetCRSProperties().rotEffectColor;
            if (dict != null && dict.TryGetValue(self.Template.type, out Color color))
            {
                self.effectColor = color;
            }
            dict = world.region?.GetCRSProperties().rotEyeColor;
            if (dict != null && dict.TryGetValue(self.Template.type, out Color color2))
            {
                self.eyeColor = color2;
            }
        }

        private static void DropBugAbstractAI_ctor(On.DropBugAbstractAI.orig_ctor orig, DropBugAbstractAI self, World world, AbstractCreature parent)
        {
            orig(self, world, parent);
            if (world.singleRoomWorld || world.GetAbstractRoom(parent.pos) == null || world.GetAbstractRoom(parent.pos).shelter) return;

            var items = world.region.GetCRSProperties().dropwigBaitItems;
            if (items != null)
            {
                parent.LoseAllStuckObjects();
                foreach (AbstractPhysicalObject.AbstractObjectType type in items.Keys)
                {
                    if (UnityEngine.Random.value < items[type])
                    {
                        var abstractPhysicalObject = ScavengerHooks.GenerateDefaultObject(world, type, parent.pos);
                        new AbstractPhysicalObject.CreatureGripStick(parent, abstractPhysicalObject, 0, true);
                        break;
                    }
                }
            }
        }

        #region batflies
        private static void FlyGraphics_ctor(On.FlyGraphics.orig_ctor orig, FlyGraphics self, PhysicalObject ow)
        {
            orig(self, ow);

            if (self.fly.abstractCreature.world.region?.GetCRSProperties().batGlowColor is Color c)
            {
                FlyFields.GetField(self.fly).color = c;
            }
        }


        private static void FlyGraphics_ApplyPalette(On.FlyGraphics.orig_ApplyPalette orig, FlyGraphics self, RoomCamera.SpriteLeaser sLeaser,
            RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);

            if (FlyFields.GetField(self.fly).color is Color c)
            {
                sLeaser.sprites[3].color = c;
            }
        }

        private static void Fly_Update(On.Fly.orig_Update orig, Fly self, bool eu)
        {
            orig(self, eu);

            if (FlyFields.GetField(self).color is not Color color)
            {
                return;
            }
            try
            {

                if (!self.dead)
                {
                    if (UnityEngine.Random.value < 0.1f)
                    {
                        FlyFields.GetField(self).flicker = Mathf.Max(FlyFields.GetField(self).flicker, UnityEngine.Random.value);
                    }
                }

                if (FlyFields.GetField(self).light != null)
                {
                    if (FlyFields.GetField(self).light.slatedForDeletetion || self.room.Darkness(self.mainBodyChunk.pos) == 0f || self.dead || self.Stunned)
                    {
                        FlyFields.GetField(self).light = null;
                    }
                    else
                    {
                        FlyFields.GetField(self).sin += 1f / Mathf.Lerp(20f, 80f, UnityEngine.Random.value);
                        float sin = FlyFields.GetField(self).sin;
                        FlyFields.GetField(self).light.stayAlive = true;
                        FlyFields.GetField(self).light.setPos = new UnityEngine.Vector2?(self.bodyChunks[0].pos);
                        FlyFields.GetField(self).light.setRad = new float?(60f + 20f * UnityEngine.Mathf.Sin(sin * 3.14159274f * 2f));
                        FlyFields.GetField(self).light.setAlpha = new float?(0.55f - 0.1f * UnityEngine.Mathf.Sin(sin * 3.14159274f * 2f));
                        // float customColorHue = customColor == null ? 0.6f : CRExtras.RGB2HSL(customColor ?? UnityEngine.Color.white).hue;

                        FlyFields.GetField(self).light.color = Color.Lerp(color, Color.black, 0.2f * FlyFields.GetField(self).flicker);
                    }
                }
                else if (self.room.Darkness(self.bodyChunks[0].pos) > 0f && !self.dead)
                {
                    FlyFields.GetField(self).light = new LightSource(self.bodyChunks[0].pos, false, UnityEngine.Color.yellow, self);
                    FlyFields.GetField(self).light.requireUpKeep = true;
                    self.room.AddObject(FlyFields.GetField(self).light);
                }
            }
            catch { /* I am lazy, sorry in advance */ }
        }
        public class FlyFields
        {
            private static ConditionalWeakTable<Fly, FlyFields> _crsFlyData = new();
            public static FlyFields GetField(Fly self) => _crsFlyData.GetValue(self, _ => new());

            public Color? color;

            // Lightsource
            public LightSource light = null;
            public float sin;
            internal float flicker;
        }
        #endregion

        #region voidspawn
        private static void Room_Loaded(ILContext il)
        {
            var c = new ILCursor(il);

            for (int i = 0; i < 2; i++)
            {
                bool first = i == 0;
                int index = first ? 16 : 42;
                ILLabel label = null;

                if (c.TryGotoNext(MoveType.After,
                x => x.MatchBr(out label), //end of fake switch case
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<Room>(nameof(Room.roomSettings)),
                x => x.MatchLdfld<RoomSettings>(first ? nameof(RoomSettings.effects) : nameof(RoomSettings.placedObjects)),
                x => x.MatchLdloc(out index),
                x => x.MatchCallvirt(out _), //List<RoomSettings.RoomEffect>.get_Item
                x => first ? x.MatchLdfld<RoomSettings.RoomEffect>(nameof(RoomSettings.RoomEffect.type)) : x.MatchLdfld<PlacedObject>(nameof(PlacedObject.type)),
                x => first ? x.MatchLdsfld<RoomSettings.RoomEffect.Type>(nameof(RoomSettings.RoomEffect.Type.VoidSpawn)) : x.MatchLdsfld<PlacedObject.Type>(nameof(PlacedObject.Type.VoidSpawnEgg)),
                x => x.MatchCall(out _), //RoomEffect.op_Inequality
                x => x.MatchBrfalse(out _)
                ))
                {
                    c.Emit(OpCodes.Ldarg_0);
                    c.Emit(OpCodes.Ldloc, index);

                    if (first)
                    {
                        c.EmitDelegate((Room self, int i) =>
                        {
                            if (self.world.region?.GetCRSProperties().hideVoidSpawn is bool b)
                            {
                                if (!b) return false;
                                bool shouldShow = (self.game.session is StoryGameSession session && session.saveState.CanSeeVoidSpawn) || self.game.setupValues.playerGlowing;
                                bool hunterCondition = self.game.StoryCharacter != SlugcatStats.Name.Red || (self.world.region.name == "SB");
                                if (!hunterCondition && shouldShow)
                                {
                                    self.AddObject(new VoidSpawnKeeper(self, self.roomSettings.effects[i]));
                                }
                            }

                            return true;
                        });
                    }
                    else
                    {
                        c.EmitDelegate((Room self, int i) =>
                        {
                            if (self.world.region?.GetCRSProperties().hideVoidSpawn is bool b)
                            {
                                if (!b) return false;
                                if (self.game.session is not StoryGameSession session) return true;

                                var state = UnityEngine.Random.state;
                                bool consumed = session.saveState.ItemConsumed(self.world, false, self.abstractRoom.index, i);
                                bool shouldShow = self.game.setupValues.playerGlowing || session.saveState.theGlow || self.world.region.name == "SL";
                                bool hunterCondition = self.game.StoryCharacter != SlugcatStats.Name.Red && UnityEngine.Random.value < 0.05882353f;

                                if (!hunterCondition && shouldShow && !consumed)
                                {
                                    self.AddObject(new VoidSpawnKeeper(self, self.roomSettings.effects[i]));
                                }
                                UnityEngine.Random.state = state; //keep the random check consistent
                            }

                            return true;
                        });
                    }
                    c.Emit(OpCodes.Brfalse, label); //skip the spawn if returned false from before
                }
                else
                {
                    CustomRegionsMod.BepLogError($"CustomRegions.RegionProperties.MiscHooks.Room_Loaded: IL Hook part {i+1} failed.");
                }
            }
        }

        private static void VoidSpawnEgg_AIMapReady(On.VoidSpawnEgg.orig_AIMapReady orig, VoidSpawnEgg self)
        {
            orig(self);
            if (self.room.game.StoryCharacter == SlugcatStats.Name.Red && self.room.world.region?.GetCRSProperties().hideVoidSpawn is bool b && !b)
            {
                self.spawn.behavior = new VoidSpawn.EggToExit(self.spawn, (self.placedObject.data as PlacedObject.VoidSpawnEggData).exit, self.room, self.spawn.dayLightMode);
            }
        }

        private static void VoidSpawnWorldAI_Update(On.VoidSpawnWorldAI.orig_Update orig, VoidSpawnWorldAI self)
        {
            if (self.directionFinder == null && !self.triedAddProgressionFinder && self.world.region.GetCRSProperties().voidSpawnTarget != null)
            {
                self.directionFinder = new VoidSpawnWorldAI.DirectionFinder(self.world);
                if (self.directionFinder.destroy) self.directionFinder = null;
            }

            orig(self);
        }

        private static void DirectionFinder_ctor(On.VoidSpawnWorldAI.DirectionFinder.orig_ctor orig, VoidSpawnWorldAI.DirectionFinder self, World world)
        {
            orig(self, world);
            if (world.region?.GetCRSProperties().voidSpawnTarget is string s && world.GetAbstractRoom(s) is AbstractRoom room)
            {
                if (self.showToRoom != -1)
                {
                    self.checkNext.Clear();
                    AbstractRoom abstractRoom = world.GetAbstractRoom(self.showToRoom);
                    for (int k = 0; k < abstractRoom.connections.Length; k++)
                    {
                        self.matrix[abstractRoom.index - world.firstRoomIndex][k] = -1f;
                    }
                }

                self.showToRoom = room.index;
                for (int k = 0; k < room.connections.Length; k++)
                {
                    self.checkNext.Add(new RWCustom.IntVector2(room.index - world.firstRoomIndex, k));
                    self.matrix[room.index - world.firstRoomIndex][k] = 0f;
                }
            }
        }
        #endregion
    }
}
