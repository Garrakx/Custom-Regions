using CustomRegions.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.IO;
using System.Linq;
using static CustomRegions.CustomWorld.RegionPreprocessors;

namespace CustomRegions.Arena
{
    internal class PreprocessorPatch
    {
        public static void ApplyHooks()
        {
            IL.ArenaCreatureSpawner.SpawnArenaCreatures += ArenaCreatureSpawner_SpawnArenaCreatures;
        }

        private static void ArenaCreatureSpawner_SpawnArenaCreatures(MonoMod.Cil.ILContext il)
        {
            var c = new ILCursor(il);
            if (c.TryGotoNext(
                MoveType.After,
                x => x.MatchLdloc(0),
                x => x.MatchCall(typeof(File), nameof(File.ReadAllLines))
                ))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((string[] lines, RainWorldGame game) => 
                {
                    try
                    {
                        RegionInfo regionInfo = new()
                        {
                            RegionID = game.world.GetAbstractRoom(0).name,
                            Lines = lines.ToList().Where(x => !(string.IsNullOrEmpty(x) || x.StartsWith("\\\\"))).ToList()
                        };

                        foreach (RegionPreprocessor filter in regionPreprocessors)
                        {
                            try
                            {
                                filter(regionInfo);
                            }
                            catch (Exception e) { CustomRegionsMod.CustomLog($"Error when executing PreProcessor [{filter.Method.Name}]\n" + e.ToString(), true); }
                        }

                        CustomRegionsMod.CustomLog(string.Join(Environment.NewLine, regionInfo.Lines), false, CustomRegionsMod.DebugLevel.MEDIUM);
                        return regionInfo.Lines.ToArray();
                    }
                    catch (Exception e) 
                    {
                        CustomRegionsMod.CustomLog($"error when applying RegionPreprocessors to singleRoomWorld\n{e}");
                        return lines;
                    }
                });
            }
        }
    }
}
