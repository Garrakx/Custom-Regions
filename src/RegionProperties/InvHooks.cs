using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Reflection;
using CustomRegions.Mod;

namespace CustomRegions.RegionProperties
{
    internal static class InvHooks
    {
        public static void ApplyHooks()
        {
            IL.Room.Loaded += (ILContext il) => SofanthielCondition<Room>(il, WormgrassSpamCondition, "LF", WormgrassAmountHook);
            IL.Room.Update += (ILContext il) => SofanthielCondition<Room>(il, ExplosiveSnailCondition, "DS");
            IL.Room.Update += (ILContext il) => SofanthielCondition<Room>(il, GrimeSpamCondition, "VS", GrimeAmountHook);
            IL.RoomCamera.Update += (ILContext il) => SofanthielCondition<RoomCamera>(il, BlackFadeCondition, "SB", BlackFadeHook);
            IL.Snail.Click += (ILContext il) => SofanthielCondition<Snail>(il, ExplosiveSnailCondition, "DS");
            var painJumpsHook = new Hook(typeof(Player).GetProperty("PainJumps", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetGetMethod(), PainJumps_Hook);
            
        }

        public static bool PainJumps_Hook(Func<Player, bool> orig, Player self)
        {
            return self.room?.world.region?.GetCRSProperties().invPainJumps is not bool b ? orig(self) : b && !self.room.abstractRoom.gate && !self.room.abstractRoom.shelter;
        }

        private delegate bool RoomCheck(bool orig, Room room);

        private static bool WormgrassSpamCondition(bool orig, Room room) => room?.world.region?.GetCRSProperties().invWormgrassSpam is float f ? f > 0 : orig;
        private static bool GrimeSpamCondition(bool orig, Room room) => room?.world.region?.GetCRSProperties().invGrimeSpam is float f ? f > 0 : orig;
        private static bool BlackFadeCondition(bool orig, Room room) => room?.world.region?.GetCRSProperties().invBlackFade is float f ? f > 0 : orig;
        private static bool ExplosiveSnailCondition(bool orig, Room room) => room?.world.region?.GetCRSProperties().invExplosiveSnails is bool b ? b : orig;

        private static void WormgrassAmountHook(ILCursor c)
        {
            for (int i = 0; i < 2; i++)
            {
                if (c.TryGotoNext(MoveType.After, x => x.MatchLdcR4(0.25f)))
                {
                    c.Emit(OpCodes.Ldarg_0);
                    c.EmitDelegate((float orig, Room self) => self.world.region?.GetCRSProperties().invWormgrassSpam is float f ? f : orig);
                }
                else
                { CustomRegionsMod.BepLogError($"failed to ilhook Sofanthiel Wormgrass Gimmick"); }
            }
        }
        private static void GrimeAmountHook(ILCursor c)
        {
            for (int i = 0; i < 2; i++)
            {
                if (c.TryGotoNext(MoveType.After, x => x.MatchLdcR4(0.25f)))
                {
                    c.Emit(OpCodes.Ldarg_0);
                    c.EmitDelegate((float orig, Room self) => self.world.region?.GetCRSProperties().invGrimeSpam is float f ? f : orig);
                }
                else
                { CustomRegionsMod.BepLogError($"failed to ilhook Sofanthiel Grime Gimmick"); }
            }
        }
        private static void BlackFadeHook(ILCursor c)
        {
            if (c.TryGotoNext(MoveType.After, x => x.MatchLdcR4(0.0025f)))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((float orig, RoomCamera self) => self.room?.world.region?.GetCRSProperties().invBlackFade is float f ? f : orig);
            }
            else
            { CustomRegionsMod.BepLogError($"failed to ilhook Sofanthiel Black Fade Gimmick"); }
        }

        private delegate void ILHook(ILCursor c);

        private static void SofanthielCondition<T>(ILContext il, RoomCheck del, string region, ILHook additionalHook = null) where T : class
        {
            var c = new ILCursor(il);
            bool success = false;
            while (true)
            {

                int sofanthielIndex;
                if (MatchSofanthielCheck(c)) sofanthielIndex = c.Index;
                else break;

                if (MatchRegionNameCheck(c, region))
                {
                    EmitRoom<T>(c);
                    c.EmitDelegate(del);

                    additionalHook?.Invoke(c);

                    c.Index = sofanthielIndex;

                    EmitRoom<T>(c);
                    c.EmitDelegate(del);

                    success = true;
                    break;
                }
            }
            if (!success) CustomRegionsMod.BepLogError($"failed to ilhook MatchSofanthiel: {region}"); 
        }

        private static void EmitRoom<T>(ILCursor c)
        {
            c.Emit(OpCodes.Ldarg_0);

            if (typeof(T) == typeof(RoomCamera)) c.Emit<RoomCamera>(OpCodes.Call, "get_room");
            if (typeof(T).IsSubclassOf(typeof(UpdatableAndDeletable))) c.Emit<UpdatableAndDeletable>(OpCodes.Ldfld, "room");
        }

        private static bool MatchSofanthielCheck(ILCursor c)
        {
            return c.TryGotoNext(MoveType.After,
                            x => x.MatchLdsfld<MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName>(nameof(MoreSlugcats.MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)),
                            x => x.MatchCall(typeof(ExtEnum<SlugcatStats.Name>).GetMethod("op_Equality"))
                            );
        }

        private static bool MatchRegionNameCheck(ILCursor c, string region)
        {
            string match = "";
            return c.TryGotoNext(MoveType.After,
                        x => x.MatchLdfld<Room>(nameof(Room.world)),
                        x => x.MatchLdfld<World>(nameof(World.region)),
                        x => x.MatchLdfld<Region>(nameof(Region.name)),
                        x => x.MatchLdstr(out match),
                        x => x.MatchCall<string>("op_Equality")
                        ) && match == region;
        }
    }
}
