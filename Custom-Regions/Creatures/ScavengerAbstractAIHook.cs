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
        }

        public static void ApplyHooks()
        {
            On.ScavengerAbstractAI.UpdateMissionAppropriateGear += ScavengerAbstractAI_UpdateMissionAppropriateGear;
            On.ScavengerAbstractAI.TradeItem += ScavengerAbstractAI_TradeItem;
            On.ScavengerAbstractAI.InitGearUp += ScavengerAbstractAI_InitGearUp;
        }

        private static void ScavengerAbstractAI_InitGearUp(On.ScavengerAbstractAI.orig_InitGearUp orig, ScavengerAbstractAI self)
        {
            List<AbstractWorldEntity> entities = new List<AbstractWorldEntity>(self.world.GetAbstractRoom(self.parent.pos).entities);
            orig(self);

            string tradeItem = null;
            World world = self.world;
            if (world != null && !world.singleRoomWorld && world.region != null)
            {
                foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
                {
                    if (CustomWorldMod.installedPacks[keyValues.Key].regionConfig.TryGetValue(world.region.name,
                        out CustomWorldStructs.RegionConfiguration config))
                    {
                        if (config.scavTradeItem != null)
                        {
                            tradeItem = config.scavTradeItem;
                        }
                        break;
                    }
                }
            }
            if (tradeItem == null || !Enum.IsDefined(typeof(AbstractPhysicalObject.AbstractObjectType), tradeItem))
            {
                return;
            }

            AbstractPhysicalObject.AbstractObjectType type = (AbstractPhysicalObject.AbstractObjectType)
                Enum.Parse(typeof(AbstractPhysicalObject.AbstractObjectType), tradeItem);

            int maxItems = 3;
            /*
            int num2 = 40;
            float num3 = 1f;
            if (self.world.game.IsStorySession)
            {
                num2 = (self.world.game.session as StoryGameSession).saveState.cycleNumber;
                if (self.world.game.StoryCharacter == 1)
                {
                    num2 = Mathf.FloorToInt((float)num2 * 0.75f);
                    num3 = 0.5f;
                }
                else if (self.world.game.StoryCharacter == 2)
                {
                    num2 += 60;
                    num3 = 1.5f;
                }
            }
            */
            entities = self.world.GetAbstractRoom(self.parent.pos).entities.Except(entities).ToList();
            CustomWorldMod.Log($"Scavenger spawned with [{entities.Count()}] items", false, CustomWorldMod.DebugLevel.FULL);
            if (UnityEngine.Random.value < 0.6f)
            {
                int freeHand = maxItems;
                if (entities.Count() >= maxItems)
                {
                    // already spawned with 3 items

                    /*
                    int num4 = Custom.IntClamp((int)(Mathf.Pow(UnityEngine.Random.value, 
                        Mathf.Lerp(1.5f, 0.5f, Mathf.Pow(self.parent.personality.dominance, 3f - num3))) * (3.5f + num3)), 0, 4);

                    if (num4 > 0)
                    {
                        // spawn with rock
                        freeHand--;
                    }
                    */
                }
                else
                {
                    freeHand = maxItems - entities.Count();

                    CustomWorldMod.Log($"Scavenger will spawn with [{type}]", false, CustomWorldMod.DebugLevel.MEDIUM);
                    try
                    {
                        AbstractPhysicalObject abstractPhysicalObject2 = new AbstractPhysicalObject(self.world, type, null, self.parent.pos, self.world.game.GetNewID());
                        self.world.GetAbstractRoom(self.parent.pos).AddEntity(abstractPhysicalObject2);
                        new AbstractPhysicalObject.CreatureGripStick(self.parent, abstractPhysicalObject2, freeHand, true);
                    }
                    catch (Exception e)
                    {
                        CustomWorldMod.Log($"Error at InitGearUp. [{tradeItem}] is not a valid scavenger item. {e}", true);
                    }
                }
            }


        }

        private static AbstractPhysicalObject ScavengerAbstractAI_TradeItem(On.ScavengerAbstractAI.orig_TradeItem orig, ScavengerAbstractAI self, bool main)
        {
            string tradeItem = null;
            World world = self.world;
            if (world != null && !world.singleRoomWorld && world.region != null)
            {
                foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
                {
                    if (CustomWorldMod.installedPacks[keyValues.Key].regionConfig.TryGetValue(world.region.name,
                        out CustomWorldStructs.RegionConfiguration config))
                    {
                        if (config.scavTradeItem != null)
                        {
                            tradeItem = config.scavTradeItem;
                        }
                        break;
                    }
                }
            }

            if (main)
            {
                if (tradeItem != null)
                {
                    if (Enum.IsDefined(typeof(AbstractPhysicalObject.AbstractObjectType), tradeItem))
                    {
                        AbstractPhysicalObject.AbstractObjectType type =
                            (AbstractPhysicalObject.AbstractObjectType)Enum.Parse(typeof(AbstractPhysicalObject.AbstractObjectType), tradeItem);
                        try
                        {
                            return new AbstractPhysicalObject(world, type, null, self.parent.pos, self.world.game.GetNewID());
                        } catch (Exception e)
                        {
                            CustomWorldMod.Log($"Error at TradeItem. [{tradeItem}] is not a valid scavenger item. {e}", true);
                        }
                    }
                    else
                    {
                        CustomWorldMod.Log($"[{tradeItem}] is not a valid scavenger item. Check for typos.", true);
                    }

                }
            }

            return orig(self, main);
        }

        private static void ScavengerAbstractAI_UpdateMissionAppropriateGear(On.ScavengerAbstractAI.orig_UpdateMissionAppropriateGear orig,
            ScavengerAbstractAI self)
        {
            orig(self);

            string tradeItem = null;
            if (self.squad == null || self.squad.missionType != ScavengerAbstractAI.ScavengerSquad.MissionID.Trade)
            {
                return;
            }
            World world = self.world;
            if (world != null && !world.singleRoomWorld && world.region != null)
            {
                foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
                {
                    if (CustomWorldMod.installedPacks[keyValues.Key].regionConfig.TryGetValue(world.region.name,
                        out CustomWorldStructs.RegionConfiguration config))
                    {
                        if (config.scavTradeItem != null)
                        {
                            tradeItem = config.scavTradeItem;
                        }
                        break;
                    }
                }
            }
            if (tradeItem == null)
            {
                // no items found to trade
                return;
            }
            for (int j = 0; j < self.parent.stuckObjects.Count; j++)
            {
                if (!Enum.IsDefined(typeof(AbstractPhysicalObject.AbstractObjectType), tradeItem))
                {

                    CustomWorldMod.Log($"[{tradeItem}] is not a valid scavenger item. Check for typos.", true);
                    continue;
                }
                AbstractPhysicalObject.AbstractObjectType type =
                    (AbstractPhysicalObject.AbstractObjectType)Enum.Parse(typeof(AbstractPhysicalObject.AbstractObjectType), tradeItem);

                if (self.parent.stuckObjects[j] is AbstractPhysicalObject.CreatureGripStick &&
                    self.parent.stuckObjects[j].A == self.parent && self.parent.stuckObjects[j].B.type == type)
                {
                    CustomWorldMod.Log($"Squad acquired mission: appropiate [{type}] at [{world.region.name}]",
                        false, CustomWorldMod.DebugLevel.MEDIUM);

                    self.missionAppropriateGear = true;
                    return;
                }
            }
        }
    }
}
