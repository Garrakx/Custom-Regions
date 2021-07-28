using System;
using System.Collections.Generic;
using CustomRegions.Mod;
using UnityEngine;

namespace CustomRegions
{
    static class RoomHook
    {
        public static void ApplyHooks()
        {
            On.Room.AddObject += Room_AddObject;
            On.Room.Loaded += Room_Loaded;
        }
        public static void RemoveHooks()
        {
            On.Room.AddObject -= Room_AddObject;
            On.Room.Loaded -= Room_Loaded;
        }

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
                        PlacedObject placedObj = self.roomSettings.placedObjects[m];

                        if (placedObj.data is PlacedObject.MultiplayerItemData && UnityEngine.Random.value <= (placedObj.data as PlacedObject.MultiplayerItemData).chance)
                        {
                            PlacedObject.MultiplayerItemData.Type typeMulti = (placedObj.data as PlacedObject.MultiplayerItemData).type;
                            switch (typeMulti)
                            {
                                case PlacedObject.MultiplayerItemData.Type.Rock:
                                   // if ((self.game.session is StoryGameSession) && !(self.game.session as StoryGameSession).saveState.ItemConsumed(self.world, false, self.abstractRoom.index, m))
                                    {
                                        CustomWorldMod.Log("Added abstract reliable rock");
                                        AbstractPhysicalObject obj = new AbstractPhysicalObject(self.world, AbstractPhysicalObject.AbstractObjectType.Rock, null, self.GetWorldCoordinate(placedObj.pos), self.game.GetNewID());
                                        self.abstractRoom.entities.Add(obj);
                                    }
                                    break;
                                case PlacedObject.MultiplayerItemData.Type.ExplosiveSpear:
                                    //if ((self.game.session is StoryGameSession) && !(self.game.session as StoryGameSession).saveState.ItemConsumed(self.world, false, self.abstractRoom.index, m))
                                    {
                                        CustomWorldMod.Log("Added abstract explosive spear");
                                        AbstractPhysicalObject obj = new AbstractSpear(self.world, null, self.GetWorldCoordinate(placedObj.pos), self.game.GetNewID(), true);
                                        self.abstractRoom.entities.Add(obj);
                                    }
                                    break;
                                case PlacedObject.MultiplayerItemData.Type.Spear:
                                    //if ((self.game.session is StoryGameSession) && !(self.game.session as StoryGameSession).saveState.ItemConsumed(self.world, false, self.abstractRoom.index, m))
                                    {
                                        CustomWorldMod.Log("Added abstract spear");
                                        AbstractPhysicalObject obj = new AbstractSpear(self.world, null, self.GetWorldCoordinate(placedObj.pos), self.game.GetNewID(), false);
                                        self.abstractRoom.entities.Add(obj);
                                    }
                                    break;
                                case PlacedObject.MultiplayerItemData.Type.Bomb:
                                   // if ((self.game.session is StoryGameSession) && !(self.game.session as StoryGameSession).saveState.ItemConsumed(self.world, false, self.abstractRoom.index, m))
                                    {
                                        CustomWorldMod.Log("Added abstract scavenger bomb");
                                        AbstractPhysicalObject obj = new AbstractPhysicalObject(self.world, AbstractPhysicalObject.AbstractObjectType.ScavengerBomb, null, self.GetWorldCoordinate(placedObj.pos), self.game.GetNewID());
                                        self.abstractRoom.entities.Add(obj);
                                    }
                                    break;
                                case PlacedObject.MultiplayerItemData.Type.SporePlant:
                                    if ((self.game.session is StoryGameSession) && !(self.game.session as StoryGameSession).saveState.ItemConsumed(self.world, false, self.abstractRoom.index, m))
                                    {
                                        CustomWorldMod.Log("Added abstract scavenger bomb");
                                        AbstractPhysicalObject obj = new AbstractPhysicalObject(self.world, AbstractPhysicalObject.AbstractObjectType.SporePlant, null, self.GetWorldCoordinate(placedObj.pos), self.game.GetNewID());
                                        self.abstractRoom.entities.Add(obj);
                                    }
                                    break;
                                default:
                                    // Extended enums go here
                                    if (typeMulti == EnumExt_MultiplayerItemDataType.VultureMaskSpawn || typeMulti == EnumExt_MultiplayerItemDataType.KingVultureMaskSpawn)
                                    {
                                        CustomWorldMod.Log($"Added {(typeMulti == EnumExt_MultiplayerItemDataType.KingVultureMaskSpawn ? "king" : "")} vulture mask");
                                        EntityID newID = self.game.GetNewID();
                                        AbstractPhysicalObject obj = new VultureMask.AbstractVultureMask(self.world, null, self.GetWorldCoordinate(placedObj.pos),
                                        newID, newID.RandomSeed, typeMulti == EnumExt_MultiplayerItemDataType.KingVultureMaskSpawn);
                                        self.abstractRoom.entities.Add(obj);
                                    }
                                    else if (typeMulti == EnumExt_MultiplayerItemDataType.OverseerCarcassGreen)
                                    {
                                        CustomWorldMod.Log("Added abstract overseer green carcass");
                                        AbstractPhysicalObject item = new OverseerCarcass.AbstractOverseerCarcass(self.world, null, self.GetWorldCoordinate(placedObj.pos), self.game.GetNewID(), new Color(0.447058827f, 0.9019608f, 0.768627465f), 0);
                                        self.abstractRoom.entities.Add(item);

                                    }
                                    else if (typeMulti == EnumExt_MultiplayerItemDataType.OverseerCarcassBlue)
                                    {
                                        CustomWorldMod.Log("Added abstract overseer blue carcass");
                                        AbstractPhysicalObject item = new OverseerCarcass.AbstractOverseerCarcass(self.world, null, self.GetWorldCoordinate(placedObj.pos), self.game.GetNewID(), new Color(0f, 1f, 0f), 2);
                                        self.abstractRoom.entities.Add(item);

                                    }
                                    else if (typeMulti == EnumExt_MultiplayerItemDataType.OverseerCarcassYellow)
                                    {
                                        CustomWorldMod.Log("Added abstract overseer yellow carcass");
                                        AbstractPhysicalObject item = new OverseerCarcass.AbstractOverseerCarcass(self.world, null, self.GetWorldCoordinate(placedObj.pos), self.game.GetNewID(), new Color(1f, 0.8f, 0.3f), 3);
                                        self.abstractRoom.entities.Add(item);

                                    }
                                    break;
                            }
                        }
                    }
                }
            }
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
                    CustomWorldMod.Log("Water gate created, checking if it should be electric...");
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

