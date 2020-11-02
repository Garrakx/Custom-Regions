using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using CustomRegions.Mod;

namespace CustomRegions
{
    static class RoomHook
    {
        public static void ApplyHooks()
        {
            On.Room.Loaded += Room_Loaded;
            On.Room.AddObject += Room_AddObject;
        }

        private static void Room_AddObject(On.Room.orig_AddObject orig, Room self, UpdatableAndDeletable obj)
        {
            if (self.game == null)
            {
                return;
            }

            if (obj is WaterGate)
            {
                // Add electric gate
                if (self.abstractRoom.gate)
                {
                    foreach (KeyValuePair<string, CustomWorldMod.RegionInformation> entries in CustomWorldMod.availableRegions)
                    {
                        if (!entries.Value.activated || entries.Value.electricGates == null || entries.Value.electricGates.Count == 0)
                        {
                            continue;
                        }

                        if (entries.Value.electricGates.ContainsKey(self.abstractRoom.name))
                        {
                            CustomWorldMod.CustomWorldLog($"Added electric gate [{self.abstractRoom.name}] from [{entries.Key}]");
                            self.regionGate = new ElectricGate(self);
                            (self.regionGate as ElectricGate).meterHeight = entries.Value.electricGates[self.abstractRoom.name];
                            obj = self.regionGate;
                            break;
                        }
                    }
                }
            }

            orig(self, obj);
        }

        private static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
        {
            orig(self);


        }
    }
}
