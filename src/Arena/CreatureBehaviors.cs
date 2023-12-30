using CustomRegions.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomRegions.Arena
{
    internal static class CreatureBehaviors
    {
        public static void ApplyHooks()
        {
            IL.MirosBirdAbstractAI.Raid += MirosBirdAbstractAI_Raid;
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
