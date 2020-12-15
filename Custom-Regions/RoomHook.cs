using System.Collections.Generic;
using CustomRegions.Mod;

namespace CustomRegions
{
    static class RoomHook
    {
        public static void ApplyHooks()
        {
            On.Room.AddObject += Room_AddObject;

           // On.Room.Loaded += Room_Loaded;
        }

        
        // Load VultureMask placed Object
        /*
        private static void Room_Loaded(On.Room.orig_Loaded orig, Room self)
        {
            if (self.game == null)
            {
                return;
            }
            bool firstTimeRealized = self.abstractRoom.firstTimeRealized;
            orig(self);

            if (firstTimeRealized)
            {
                for (int m = 0; m < self.roomSettings.placedObjects.Count; m++)
                {
                    if (self.roomSettings.placedObjects[m].active)
                    {
                        PlacedObject.Type type = self.roomSettings.placedObjects[m].type;
                        CustomWorldMod.Log($"Checking if [{type}] is {PlacedObjectHook.EnumExt_PlacedObjectType.VultureMaskSpawn}");
                        if (type == EnumExt_PlacedObjectType.VultureMaskSpawn)
                        {
                            CustomWorldMod.Log("Added abstract vulture mask");
                            double kingVultChance = 0.01f;
                            if (self.game.session is StoryGameSession && (self.game.session as StoryGameSession).saveState.saveStateNumber == 2)
                                { kingVultChance = 0.1f; }
                            EntityID newID = self.game.GetNewID();
                            AbstractPhysicalObject item = new VultureMask.AbstractVultureMask(self.world, null, self.GetWorldCoordinate(self.roomSettings.placedObjects[m].pos), 
                                newID, newID.RandomSeed, UnityEngine.Random.value <= kingVultChance ? true : false);
                            self.abstractRoom.entities.Add(item);
                        }
                    }
                }
            }
        }
        */

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
                    CustomWorldMod.Log("Water gate created, checking if it should be electric...");
                    /*
                    foreach (KeyValuePair<string, CustomWorldMod.RegionInformation> entries in CustomWorldMod.availableRegions)
                    {
                        if (!entries.Value.activated || entries.Value.electricGates == null || entries.Value.electricGates.Count == 0)
                        {
                            continue;
                        }

                        if (entries.Value.electricGates.ContainsKey(self.abstractRoom.name))
                        {
                            (obj as WaterGate).Destroy();
                            CustomWorldMod.Log($"Added electric gate [{self.abstractRoom.name}] from [{entries.Key}]");
                            self.regionGate = new ElectricGate(self);
                            (self.regionGate as ElectricGate).meterHeight = entries.Value.electricGates[self.abstractRoom.name];
                            obj = self.regionGate;
                            break;
                        }
                    }
                    */
                    foreach (KeyValuePair<string, string> regions in CustomWorldMod.activatedPacks)
                    {
                        if (CustomWorldMod.installedPacks[regions.Key].electricGates != null)
                        {
                            if (CustomWorldMod.installedPacks[regions.Key].electricGates.ContainsKey(self.abstractRoom.name))
                            {
                                (obj as WaterGate).Destroy();
                                CustomWorldMod.Log($"Added electric gate [{self.abstractRoom.name}] from [{regions.Value}]");
                                self.regionGate = new ElectricGate(self);
                                (self.regionGate as ElectricGate).meterHeight = CustomWorldMod.installedPacks[regions.Key].electricGates[self.abstractRoom.name];
                                obj = self.regionGate;
                                break;
                            }
                        }
                    }
                }
            }

            orig(self, obj);
        }

    }
}
