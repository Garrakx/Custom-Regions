using CustomRegions.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CustomRegions.Creatures
{
    static class FlyGraphicsHook
    {
        public static void ApplyHooks()
        {
            On.Fly.Update += Fly_Update;

            On.FlyGraphics.ctor += FlyGraphics_ctor;
            On.FlyGraphics.ApplyPalette += FlyGraphics_ApplyPalette;

            On.FlyGraphics.InitiateSprites += FlyGraphics_InitiateSprites;
            //On.FlyGraphics.DrawSprites += FlyGraphics_DrawSprites;
        }


        static UnityEngine.Color? customColor;
        private static void FlyGraphics_ctor(On.FlyGraphics.orig_ctor orig, FlyGraphics self, PhysicalObject ow)
        {
            orig(self, ow);

            customColor = null;
            World world = ow.abstractPhysicalObject.world;
            if (world != null && !world.singleRoomWorld && world.region != null)
            {
                foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
                {
                    if (CustomWorldMod.installedPacks[keyValues.Key].regionConfig.TryGetValue(world.region.name,
                        out CustomWorldStructs.RegionConfiguration config))
                    {
                        if (!config.batVanilla)
                        {
                            CustomWorldMod.Log($"Glowing batfly in [{world.region.name}] from [{CustomWorldMod.installedPacks[keyValues.Key].name}]",
                               false, CustomWorldMod.DebugLevel.FULL);
                            customColor = config.batFlyColor;
                            break;
                        }
                    }
                }
            }
        }


        private static void FlyGraphics_InitiateSprites(On.FlyGraphics.orig_InitiateSprites orig, FlyGraphics self, RoomCamera.SpriteLeaser sLeaser,
            RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);
            if (customColor == null)
            {
                return;
            }

            FlyFields.GetField(self.owner as Fly).numberOfOrigSprites = sLeaser.sprites.Count();
            
            // scrapped
            return;

            try
            {
                Fly fly = self.owner as Fly;
                Array.Resize(ref sLeaser.sprites, FlyFields.GetField(fly).numberOfOrigSprites + 1);

                //LizardJaw3.0
                int extra = 0;
                FlyFields.GetField(fly).bodySprite = FlyFields.GetField(self.owner as Fly).numberOfOrigSprites + extra;
                sLeaser.sprites[FlyFields.GetField(fly).bodySprite] = new FSprite("LizardJaw3.2", true);

                sLeaser.sprites[FlyFields.GetField(fly).bodySprite].anchorX = 0.5f;
                sLeaser.sprites[FlyFields.GetField(fly).bodySprite].anchorY = 0.5f;
                sLeaser.sprites[FlyFields.GetField(fly).bodySprite].scaleX *= 0.25f;
                sLeaser.sprites[FlyFields.GetField(fly).bodySprite].scaleY *= -0.3f;
                extra++;

                /* Cant be bothered
                //LizardBubble6
                FlyFields.GetField(fly).wingEyeL = FlyFields.GetField(self.owner as Fly).numberOfOrigSprites + extra;
                sLeaser.sprites[FlyFields.GetField(fly).wingEyeL] = new FSprite("LizardBubble6", true);
                sLeaser.sprites[FlyFields.GetField(fly).wingEyeL].scaleX *= 0.25f;
                sLeaser.sprites[FlyFields.GetField(fly).wingEyeL].scaleY *= 0.25f;
                sLeaser.sprites[FlyFields.GetField(fly).wingEyeL].anchorX -= 0.5f;
                sLeaser.sprites[FlyFields.GetField(fly).wingEyeL].anchorY -= 0.5f;
                extra++;

                FlyFields.GetField(fly).wingEyeR = FlyFields.GetField(self.owner as Fly).numberOfOrigSprites + extra;
                sLeaser.sprites[FlyFields.GetField(fly).wingEyeR] = new FSprite("LizardBubble6", true);
                sLeaser.sprites[FlyFields.GetField(fly).wingEyeR].scaleX *= 0.25f;
                sLeaser.sprites[FlyFields.GetField(fly).wingEyeR].scaleY *= 0.25f;
                sLeaser.sprites[FlyFields.GetField(fly).wingEyeR].anchorX -= 0.5f;
                sLeaser.sprites[FlyFields.GetField(fly).wingEyeR].anchorY -= 0.5f;
                extra++;
                */
                self.AddToContainer(sLeaser, rCam, null);
            }
            catch (Exception e)
            {
                CustomWorldMod.Log("Batfly crash " + e, true);
            }
        }


        private static void FlyGraphics_ApplyPalette(On.FlyGraphics.orig_ApplyPalette orig, FlyGraphics self, RoomCamera.SpriteLeaser sLeaser,
            RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);

            if ((self.owner as Creature).dead || customColor == null)
            {
                return;
            }

            // Eye color
            sLeaser.sprites[3].color = customColor ?? palette.blackColor;

            // Scrapped
            for (int i = FlyFields.GetField(self.owner as Fly).numberOfOrigSprites - 1; i < sLeaser.sprites.Count(); i++)
            {
                sLeaser.sprites[i].color = customColor ?? palette.blackColor;
            }
        }

        private static void Fly_Update(On.Fly.orig_Update orig, Fly self, bool eu)
        {
            orig(self, eu);

            if (customColor == null)
            {
                return;
            }

            if (!self.dead)
            {
                FlyFields.GetField(self).flickeringFac = 1f;
                FlyFields.GetField(self).flickerDuration = Mathf.Lerp(10f, 30f, UnityEngine.Random.value);
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
                    HSLColor color = CRExtras.RGB2HSL(customColor ?? UnityEngine.Color.white);
                    FlyFields.GetField(self).light.color = RWCustom.Custom.HSL2RGB(color.hue, color.saturation, color.lightness - 0.2f * FlyFields.GetField(self).flicker);
                }
            }
            else if (self.room.Darkness(self.bodyChunks[0].pos) > 0f && !self.dead)
            {
                Mod.CustomWorldMod.Log($"Creating light for [{self.abstractCreature.creatureTemplate}-{self.abstractPhysicalObject.ID.number}]", 
                    false, CustomWorldMod.DebugLevel.FULL);

                FlyFields.GetField(self).light = new LightSource(self.bodyChunks[0].pos, false, UnityEngine.Color.yellow, self);
                FlyFields.GetField(self).light.requireUpKeep = true;
                self.room.AddObject(FlyFields.GetField(self).light);
            }
        }

        // scrapped
        private static void FlyGraphics_DrawSprites(On.FlyGraphics.orig_DrawSprites orig, FlyGraphics self, RoomCamera.SpriteLeaser sLeaser,
    RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            Fly fly = self.owner as Fly;

            sLeaser.sprites[FlyFields.GetField(fly).bodySprite].x = sLeaser.sprites[0].x;
            sLeaser.sprites[FlyFields.GetField(fly).bodySprite].y = sLeaser.sprites[0].y;
            sLeaser.sprites[FlyFields.GetField(fly).bodySprite].rotation = sLeaser.sprites[0].rotation;

            sLeaser.sprites[FlyFields.GetField(fly).wingEyeL].x = sLeaser.sprites[1].x;
            sLeaser.sprites[FlyFields.GetField(fly).wingEyeL].y = sLeaser.sprites[1].y;
            sLeaser.sprites[FlyFields.GetField(fly).wingEyeL].rotation = sLeaser.sprites[1].rotation;

            sLeaser.sprites[FlyFields.GetField(fly).wingEyeR].x = sLeaser.sprites[2].x;
            sLeaser.sprites[FlyFields.GetField(fly).wingEyeR].y = sLeaser.sprites[2].y;
            sLeaser.sprites[FlyFields.GetField(fly).wingEyeR].rotation = sLeaser.sprites[2].rotation;

        }



    }

    public class FlyFields
    {
        // Might be prone to memleak?
        private readonly Fly self;
        private static Dictionary<Fly, FlyFields> fields;

        // Lightsource
        public LightSource light;
        public float sin;
        internal float flicker;
        internal float flickerDuration;
        internal float flickeringFac;
        internal int numberOfOrigSprites;

        public int bodySprite;
        public int wingEyeL;
        public int wingEyeR;

        /// <summary>Returns hook fields, or generate new entry if missing.
        ///</summary>
        ///

        public static FlyFields GetField(Fly self)
        {
            if (fields == null)
            {
                Mod.CustomWorldMod.Log($"Initiating field dictionary [{self.abstractPhysicalObject.ID.number}]");
                fields = new Dictionary<Fly, FlyFields>();
            }

            if (fields.TryGetValue(self, out FlyFields field))
            {
                return field;
            }

            Mod.CustomWorldMod.Log($"Adding field to dictionary [{self.abstractPhysicalObject.ID.number}]");
            Mod.CustomWorldMod.Log($"Number of elements in dictionary [{fields.Count()}]");
            field = new FlyFields(self);
            fields.Add(self, field);
            return field;
        }

        /*
        public static void AddField(Fly self)
        {
            if (fields == null)
            {
                Mod.CustomWorldMod.Log($"Initiating field dictionary [{self.abstractPhysicalObject.ID.number}]");
                fields = new Dictionary<Fly, FlyFields>();
            }

            if (self != null)
            {
                FlyFields field;
                if (fields.TryGetValue(self, out field))
                {
                    fields.Remove(self);
                }
                field = new FlyFields(self);
                fields.Add(self, field);
            }
        }
        
        public static FlyFields GetField(Fly self)
        {
            if (fields.TryGetValue(self, out FlyFields field))
            {
                Mod.CustomWorldMod.Log($"Returning field dictionary [{self.abstractPhysicalObject.ID.number}]. Null [{field.light == null}]");
                return field;
            }

            return null;
        }
        */
        public void ClearFields()
        {
            fields.Clear();
            fields = null;
        }

        public FlyFields(Fly self)
        {
            this.self = self;
            light = null;
        }
    }
}
