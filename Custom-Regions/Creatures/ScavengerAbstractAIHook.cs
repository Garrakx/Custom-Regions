using CustomRegions.Mod;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CustomRegions.Creatures
{
    static class ScavengerAbstractAIHook
    {
        public static void RemoveHooks()
        {
            On.ScavengerAbstractAI.UpdateMissionAppropriateGear -= ScavengerAbstractAI_UpdateMissionAppropriateGear;
            On.ScavengerAbstractAI.TradeItem -= ScavengerAbstractAI_TradeItem;
            On.ScavengerAbstractAI.InitGearUp -= ScavengerAbstractAI_InitGearUp;
            On.ScavengerAI.CollectScore_1 -= ScavengerAI_CollectScore_1;
        }

        public static void ApplyHooks()
        {
            On.ScavengerAbstractAI.UpdateMissionAppropriateGear += ScavengerAbstractAI_UpdateMissionAppropriateGear;
            On.ScavengerAbstractAI.TradeItem += ScavengerAbstractAI_TradeItem;
            On.ScavengerAbstractAI.InitGearUp += ScavengerAbstractAI_InitGearUp;
            On.ScavengerAI.CollectScore_1 += ScavengerAI_CollectScore_1; // Fixes: scav not holding item assigned for trade
        }

        // Helpers

        // Gets the configured regions special scavenger item. Affects init gearup and trader's specialty.
        private static bool TryGetSpecialScavItem(string regionName, out AbstractPhysicalObject.AbstractObjectType type)
        {
            type = default;
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
            {
                if (CustomWorldMod.installedPacks[keyValues.Key].regionConfig.TryGetValue(regionName,
                    out CustomWorldStructs.RegionConfiguration config))
                {
                    if (config.scavTradeItem != null)
                    {
                        if (!Enum.IsDefined(typeof(AbstractPhysicalObject.AbstractObjectType), config.scavTradeItem))
                        {
                            CustomWorldMod.Log($"Scavenger trade item misconfigured in {keyValues.Key}:{regionName}, got [{config.scavTradeItem}] which is not a valid AbstractObjectType", false, CustomWorldMod.DebugLevel.RELEASE);
                        }
                        else
                        {
                            CustomWorldMod.Log($"Found Scavenger trade item in {keyValues.Key}:{regionName}, got [{config.scavTradeItem}]", false, CustomWorldMod.DebugLevel.FULL);
                            type = (AbstractPhysicalObject.AbstractObjectType)Enum.Parse(typeof(AbstractPhysicalObject.AbstractObjectType), config.scavTradeItem);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        // Get the configured chance of a scav to equip the regions special on init gearup, not limited to traders
        private static float GetChanceOfSpecialItemOnGear(string regionName)
        {
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
            {
                if (CustomWorldMod.installedPacks[keyValues.Key].regionConfig.TryGetValue(regionName,
                    out CustomWorldStructs.RegionConfiguration config))
                {
                    if (config.scavGearChance >= 0f) // -1 == default
                    {
                        return config.scavGearChance;
                    }
                }
            }
            return 0.6f;
        }

        // Hook me :)
        // Returns null on failure to instantiate
        // Instantiates an item of type inside ~~mind of scav~~ probably in offscreen den where scav is
        private static AbstractPhysicalObject InstantiateCustomScavItemAbstract(AbstractPhysicalObject.AbstractObjectType type, ScavengerAbstractAI self)
        {
            EntityID id = self.world.game.GetNewID();
            switch (type)
            {
                // Commented out : safe default OR doesn't really work
                //case AbstractPhysicalObject.AbstractObjectType.Creature:
                //case AbstractPhysicalObject.AbstractObjectType.Rock:
                case AbstractPhysicalObject.AbstractObjectType.Spear:
                    return new AbstractSpear(self.world, null, self.parent.pos, id, self.IsSpearExplosive((!self.world.game.IsStorySession) ? 0 : (self.world.game.GetStorySession.saveState.cycleNumber + ((self.world.game.StoryCharacter != 2) ? 0 : 60))));
                //case AbstractPhysicalObject.AbstractObjectType.FlareBomb:
                case AbstractPhysicalObject.AbstractObjectType.VultureMask:
                    return new VultureMask.AbstractVultureMask(self.world, null, self.parent.pos, id, id.RandomSeed, false);
                //case AbstractPhysicalObject.AbstractObjectType.PuffBall:
                //case AbstractPhysicalObject.AbstractObjectType.DangleFruit:
                //case AbstractPhysicalObject.AbstractObjectType.Oracle:
                case AbstractPhysicalObject.AbstractObjectType.PebblesPearl:
                    return new PebblesPearl.AbstractPebblesPearl(self.world, null, self.parent.pos, id, -1, -1, null, id.RandomSeed % 3, id.RandomSeed % 3);
                //case AbstractPhysicalObject.AbstractObjectType.SLOracleSwarmer:
                //case AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer:
                case AbstractPhysicalObject.AbstractObjectType.DataPearl:
                    return new DataPearl.AbstractDataPearl(self.world, AbstractPhysicalObject.AbstractObjectType.DataPearl, null, self.parent.pos, id, -1, -1, null, DataPearl.AbstractDataPearl.DataPearlType.Misc);
                case AbstractPhysicalObject.AbstractObjectType.SeedCob: // Fails miserably
                    return new SeedCob.AbstractSeedCob(self.world, null, self.parent.pos, id, -1, -1, false, null);
                case AbstractPhysicalObject.AbstractObjectType.WaterNut:
                    return new WaterNut.AbstractWaterNut(self.world, null, self.parent.pos, id, -1, -1, null, false);
                //case AbstractPhysicalObject.AbstractObjectType.JellyFish:
                //case AbstractPhysicalObject.AbstractObjectType.Lantern:
                //case AbstractPhysicalObject.AbstractObjectType.KarmaFlower:
                //case AbstractPhysicalObject.AbstractObjectType.Mushroom:
                //case AbstractPhysicalObject.AbstractObjectType.VoidSpawn:
                //case AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant:
                //case AbstractPhysicalObject.AbstractObjectType.SlimeMold:
                //case AbstractPhysicalObject.AbstractObjectType.FlyLure:
                //case AbstractPhysicalObject.AbstractObjectType.ScavengerBomb:
                case AbstractPhysicalObject.AbstractObjectType.SporePlant:
                    return new SporePlant.AbstractSporePlant(self.world, null, self.parent.pos, id, -1, -1, null, false, true);
                //case AbstractPhysicalObject.AbstractObjectType.AttachedBee:
                case AbstractPhysicalObject.AbstractObjectType.EggBugEgg:
                    int seed = UnityEngine.Random.seed;
                    UnityEngine.Random.seed = id.RandomSeed;
                    float hue = Mathf.Lerp(-0.15f, 0.1f, Custom.ClampedRandomVariation(0.5f, 0.5f, 2f));
                    UnityEngine.Random.seed = seed;
                    return new EggBugEgg.AbstractBugEgg(self.world, null, self.parent.pos, id, hue);
                //case AbstractPhysicalObject.AbstractObjectType.NeedleEgg:
                //case AbstractPhysicalObject.AbstractObjectType.DartMaggot:
                case AbstractPhysicalObject.AbstractObjectType.BubbleGrass:
                    return new BubbleGrass.AbstractBubbleGrass(self.world, null, self.parent.pos, id, 1f, -1, -1, null);
                //case AbstractPhysicalObject.AbstractObjectType.NSHSwarmer:
                case AbstractPhysicalObject.AbstractObjectType.OverseerCarcass:
                    return new OverseerCarcass.AbstractOverseerCarcass(self.world, null, self.parent.pos, id, new Color(0.1f, 0.1f, 0.1f), 0);
                default:
                    try
                    {
                        if (AbstractConsumable.IsTypeConsumable(type))
                        {
                            return new AbstractConsumable(self.parent.world, type, null, self.parent.pos, self.world.game.GetNewID(), -1, -1, null);
                        }
                        else
                        {
                            return new AbstractPhysicalObject(self.parent.world, type, null, self.parent.pos, self.world.game.GetNewID());
                        }
                    }
                    catch (Exception)
                    {
                        return null; // This is nicer to manage if modders hook this method to spawn their own things.
                    }
            }
        }

        // Hook me :)
        // Returns 0 on unknown
        // Score of items that scavs wouldn't normally hold, for the region they're trade specials.
        // Picked some values myself rather than making it a setting -- hen
        private static int GetScoreOfMissingItem(AbstractPhysicalObject.AbstractObjectType specialItem)
        {
            switch (specialItem)
            {
                // Commented out: already handled by the game, OR non-spawnable
                //case AbstractPhysicalObject.AbstractObjectType.Creature:
                //case AbstractPhysicalObject.AbstractObjectType.Rock:
                //case AbstractPhysicalObject.AbstractObjectType.Spear:
                //case AbstractPhysicalObject.AbstractObjectType.FlareBomb:
                //case AbstractPhysicalObject.AbstractObjectType.VultureMask:
                //case AbstractPhysicalObject.AbstractObjectType.PuffBall:
                case AbstractPhysicalObject.AbstractObjectType.DangleFruit:
                    return 2;
                //case AbstractPhysicalObject.AbstractObjectType.Oracle:
                //case AbstractPhysicalObject.AbstractObjectType.PebblesPearl:
                case AbstractPhysicalObject.AbstractObjectType.SLOracleSwarmer:
                    return 4;
                case AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer:
                    return 3;
                //case AbstractPhysicalObject.AbstractObjectType.DataPearl:
                //case AbstractPhysicalObject.AbstractObjectType.SeedCob:
                //case AbstractPhysicalObject.AbstractObjectType.WaterNut:
                //case AbstractPhysicalObject.AbstractObjectType.JellyFish:
                //case AbstractPhysicalObject.AbstractObjectType.Lantern:
                //case AbstractPhysicalObject.AbstractObjectType.KarmaFlower:
                //case AbstractPhysicalObject.AbstractObjectType.Mushroom:
                //case AbstractPhysicalObject.AbstractObjectType.VoidSpawn:
                //case AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant:
                case AbstractPhysicalObject.AbstractObjectType.SlimeMold:
                    return 2;
                //case AbstractPhysicalObject.AbstractObjectType.FlyLure:
                //case AbstractPhysicalObject.AbstractObjectType.ScavengerBomb:
                //case AbstractPhysicalObject.AbstractObjectType.SporePlant:
                //case AbstractPhysicalObject.AbstractObjectType.AttachedBee:
                case AbstractPhysicalObject.AbstractObjectType.EggBugEgg:
                    return 1;
                case AbstractPhysicalObject.AbstractObjectType.NeedleEgg:
                    return 4;
                case AbstractPhysicalObject.AbstractObjectType.DartMaggot:
                    return 2;
                case AbstractPhysicalObject.AbstractObjectType.BubbleGrass:
                    return 3;
                case AbstractPhysicalObject.AbstractObjectType.NSHSwarmer:
                    return 7;
                //case AbstractPhysicalObject.AbstractObjectType.OverseerCarcass:
                default:
                    return 0;
            }
        }

        // Called on scav spawn while offscreen
        // If region has special && roll random chance of special on gerup: equip special
        // Causes scav to drop a rock or spear if inventory full. If inventory full of other stuff, skip special.
        private static void ScavengerAbstractAI_InitGearUp(On.ScavengerAbstractAI.orig_InitGearUp orig, ScavengerAbstractAI self)
        {
            orig(self);

            World world = self.world;
            if (world == null || world.singleRoomWorld || world.region == null || !TryGetSpecialScavItem(world.region.name, out AbstractPhysicalObject.AbstractObjectType specialItem))
            {
                return;
            }

            if (UnityEngine.Random.value < GetChanceOfSpecialItemOnGear(world.region.name))
            {
                // Find free grasp
                // Gearup normally fills 3 -> 2 -> 1 -> 0 'indexes'
                // Find min
                int grasp = int.MaxValue;
                for (int j = self.parent.stuckObjects.Count - 1; j >= 0; j--)
                {
                    if (self.parent.stuckObjects[j] is AbstractPhysicalObject.CreatureGripStick stick && self.parent.stuckObjects[j].A == self.parent)
                    {
                        if (stick.grasp < grasp) grasp = stick.grasp;
                    }
                }
                if (grasp == int.MaxValue) grasp = 3;
                else grasp--; // Next

                if (grasp < 0) // Full
                {
                    // going through objects last to first, drop a rock or spear
                    for (int k = self.parent.stuckObjects.Count - 1; k >= 0; k--)
                    {
                        if (self.parent.stuckObjects[k] is AbstractPhysicalObject.CreatureGripStick && self.parent.stuckObjects[k].A == self.parent && (self.parent.stuckObjects[k].B.type == AbstractPhysicalObject.AbstractObjectType.Rock || (self.parent.stuckObjects[k].B.type == AbstractPhysicalObject.AbstractObjectType.Spear && !(self.parent.stuckObjects[k].B as AbstractSpear).explosive)))
                        {
                            grasp = (self.parent.stuckObjects[k] as AbstractPhysicalObject.CreatureGripStick).grasp;
                            self.DropAndDestroy(self.parent.stuckObjects[k]);
                            break;
                        }
                    }
                }

                if (grasp >= 0)
                {
                    CustomWorldMod.Log($"Scavenger will spawn with [{specialItem}]", false, CustomWorldMod.DebugLevel.MEDIUM);
                    try
                    {
                        AbstractPhysicalObject abstractPhysicalObject = InstantiateCustomScavItemAbstract(specialItem, self);
                        if (abstractPhysicalObject == null) throw new Exception("Cannot instantiate item of type " + specialItem.ToString());
                        self.world.GetAbstractRoom(self.parent.pos).AddEntity(abstractPhysicalObject);
                        new AbstractPhysicalObject.CreatureGripStick(self.parent, abstractPhysicalObject, grasp, true);
                    }
                    catch (Exception e)
                    {
                        CustomWorldMod.Log($"Error at InitGearUp for [{specialItem}]. {e}", true);
                    }
                }
            }
        }

        // Called on re-gear for Trade-mission scavs. If the region has a special item assigned, re-gear with that.
        private static AbstractPhysicalObject ScavengerAbstractAI_TradeItem(On.ScavengerAbstractAI.orig_TradeItem orig, ScavengerAbstractAI self, bool main)
        {
            if (main)
            {
                World world = self.world;
                if (world != null && !world.singleRoomWorld && world.region != null && TryGetSpecialScavItem(world.region.name, out AbstractPhysicalObject.AbstractObjectType specialItem))
                {
                    try
                    {
                        AbstractPhysicalObject abstractPhysicalObject = InstantiateCustomScavItemAbstract(specialItem, self);
                        if (abstractPhysicalObject == null) throw new Exception("Cannot instantiate item of type " + specialItem.ToString());
                        return abstractPhysicalObject;
                    }
                    catch (Exception e)
                    {
                        CustomWorldMod.Log($"Error at TradeItem for [{specialItem}]. {e}", true);
                    }
                }
            }
            return orig(self, main);
        }

        // Determine if the scav is well-suited for its mission. In vanilla this will ensure the trader has the region's special.
        // We do the same for traders here.
        private static void ScavengerAbstractAI_UpdateMissionAppropriateGear(On.ScavengerAbstractAI.orig_UpdateMissionAppropriateGear orig,
            ScavengerAbstractAI self)
        {
            orig(self);

            if (self.squad == null || self.squad.missionType != ScavengerAbstractAI.ScavengerSquad.MissionID.Trade)
            {
                return;
            }
            World world = self.world;
            if (world == null || world.singleRoomWorld || world.region == null || !TryGetSpecialScavItem(world.region.name, out AbstractPhysicalObject.AbstractObjectType specialItem))
            {
                return;
            }
            self.missionAppropriateGear = false;
            for (int j = 0; j < self.parent.stuckObjects.Count; j++)
            {
                if (self.parent.stuckObjects[j] is AbstractPhysicalObject.CreatureGripStick &&
                    self.parent.stuckObjects[j].A == self.parent && self.parent.stuckObjects[j].B.type == specialItem)
                {
                    self.missionAppropriateGear = true;
                    return;
                }
            }
        }

        // Fixes: scavengers not holding onto the region's special
        private static int ScavengerAI_CollectScore_1(On.ScavengerAI.orig_CollectScore_1 orig, ScavengerAI self, PhysicalObject obj, bool weaponFiltered)
        {
            int val = orig(self, obj, weaponFiltered);
            if (val == 0 && !weaponFiltered)
            {
                if (self.scavenger.room != null)
                {
                    SocialEventRecognizer.OwnedItemOnGround ownedItemOnGround = self.scavenger.room.socialEventRecognizer.ItemOwnership(obj);
                    if (ownedItemOnGround != null && ownedItemOnGround.offeredTo != null && ownedItemOnGround.offeredTo != self.scavenger)
                    {
                        return 0;
                    }
                }
                World world = self.creature.world;
                if (world != null && !world.singleRoomWorld && world.region != null && TryGetSpecialScavItem(world.region.name, out AbstractPhysicalObject.AbstractObjectType specialItem))
                {
                    if (obj.abstractPhysicalObject.type == specialItem)
                    {
                        val = GetScoreOfMissingItem(specialItem);
                    }
                }
            }
            return val;
        }
    }
}
