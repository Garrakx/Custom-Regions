using CustomRegions.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CustomRegions
{
    static class ElectricGateHook
    {
        public static void ApplyHooks()
        {
            On.ElectricGate.ctor += ElectricGate_ctor;
        }

        private static void ElectricGate_ctor(On.ElectricGate.orig_ctor orig, ElectricGate self, Room room)
        {
            foreach (KeyValuePair<string, CustomWorldMod.RegionInformation> entries in CustomWorldMod.availableRegions)
            {
                if (!entries.Value.activated || entries.Value.electricGates == null || entries.Value.electricGates.Count == 0)
                {
                    continue;
                }

                if (entries.Value.electricGates.ContainsKey(self.room.abstractRoom.name))
                {
                    return;
                }
            }

            orig(self, room);
        }
    }
}
