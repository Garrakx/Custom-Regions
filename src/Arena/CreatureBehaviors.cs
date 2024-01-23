using CustomRegions.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CustomRegions.Arena
{
    internal static class CreatureBehaviors
    {
        public static void ApplyHooks()
        {
            IL.MirosBirdAbstractAI.Raid += MirosBirdAbstractAI_Raid;
            On.ArenaGameSession.SpawnCreatures += ArenaGameSession_SpawnCreatures;
            On.DeerAbstractAI.ctor += DeerAbstractAI_ctor;
        }

        private static void DeerAbstractAI_ctor(On.DeerAbstractAI.orig_ctor orig, DeerAbstractAI self, World world, AbstractCreature parent)
        {
            orig(self, world, parent);
            if (world.singleRoomWorld)
            {
                for (int i = 0; i < world.GetAbstractRoom(0).nodes.Length; i++)
                {
                    if (world.GetAbstractRoom(0).nodes[i].type == AbstractRoomNode.Type.SideExit && world.GetAbstractRoom(0).nodes[i].entranceWidth >= 3)
                    {
                        self.allowedNodes.Add(new WorldCoordinate(0, -1, -1, i));
                    }
                }
                return;
            }
        }

        private static void ArenaGameSession_SpawnCreatures(On.ArenaGameSession.orig_SpawnCreatures orig, ArenaGameSession self)
        {
            orig(self);

            if (self is SandboxGameSession sandbox && !sandbox.PlayMode) return;

            var abstractRoom = self.game.world.GetAbstractRoom(0);
            AbstractRoomNode node = abstractRoom.nodes.Where(x => x.type == AbstractRoomNode.Type.GarbageHoles).FirstOrDefault();

            int wormsToSpawn = 0;

            string path = AssetManager.ResolveFilePath("Levels" + Path.DirectorySeparatorChar.ToString() + abstractRoom.name + "_Arena.txt");
            if (!File.Exists(path) || node.type != AbstractRoomNode.Type.GarbageHoles)
            { return; }

            foreach (string line in File.ReadAllLines(path))
            {
                if (line.Length > 2 && line.Substring(0, 2) != "//")
                {
                    string[] array2 = Regex.Split(line, " - ");
                    if (array2[0].Length >= 2 && array2[0] == "GarbageWorms" && int.TryParse(array2[1], out var result))
                    { wormsToSpawn = result; break; }
                }
            }

            for (int i = 0; i < wormsToSpawn; i++)
            {
                AbstractCreature abstractCreature = new AbstractCreature(abstractRoom.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.GarbageWorm), null, new WorldCoordinate(abstractRoom.index, -1, -1, abstractRoom.nodes.ToList().IndexOf(node)), self.game.GetNewID());
                abstractRoom.MoveEntityToDen(abstractCreature);
            }
        }

        private static void MirosBirdAbstractAI_Raid(MonoMod.Cil.ILContext il)
        {
            var c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdsfld<CreatureTemplate.Type>(nameof(CreatureTemplate.Type.MirosBird)),
                x => x.MatchCall(typeof(ExtEnum<CreatureTemplate.Type>).GetMethod("op_Equality"))
                ))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldloc_3);
                c.EmitDelegate((bool orig, MirosBirdAbstractAI self, int i) => { return orig && self.parent.Room.creatures[i].state.alive; });
            }
            else
            {
                CustomRegionsMod.BepLogError("failed to il hook MirosBirdAbstractAI.Raid");
            }

            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdsfld<CreatureTemplate.Type>(nameof(CreatureTemplate.Type.MirosBird)),
                x => x.MatchCall(typeof(ExtEnum<CreatureTemplate.Type>).GetMethod("op_Equality"))
                ))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldloc, 5);
                c.Emit(OpCodes.Ldloc, 6);
                c.EmitDelegate((bool orig, MirosBirdAbstractAI self, int i, int j) => { return orig && self.parent.world.GetAbstractRoom(self.parent.world.firstRoomIndex + i).creatures[j].state.alive; });
            }
        }
    }
}
