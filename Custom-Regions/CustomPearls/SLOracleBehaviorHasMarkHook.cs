using CustomRegions.Mod;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CustomRegions.CustomPearls
{
    public class EnumExt_ConversationID
    {
        public static Conversation.ID CustomConversationID;
    }



	static class SLOracleBehaviorHasMarkHook
    {
        public static void ApplyHooks()
        {
            On.SLOracleBehaviorHasMark.GrabObject += SLOracleBehaviorHasMark_GrabObject;
            On.SLOracleBehaviorHasMark.MoonConversation.AddEvents += MoonConversation_AddEvents;

        }


        private static void SLOracleBehaviorHasMark_GrabObject(On.SLOracleBehaviorHasMark.orig_GrabObject orig, SLOracleBehaviorHasMark self, PhysicalObject item)
        {
            bool foundPearl = false;
            if (!(item is SSOracleSwarmer))
            {
                if (!self.State.HaveIAlreadyDescribedThisItem(item.abstractPhysicalObject.ID))
                {
                    if (item is DataPearl)
                    {
                        CustomWorldMod.CustomWorldLog($"Moon grabbed pearl: {(item as DataPearl).AbstractPearl.dataPearlType}");
                        if ((item as DataPearl).AbstractPearl.dataPearlType != DataPearl.AbstractDataPearl.DataPearlType.Misc)
                        {
                            if ((item as DataPearl).AbstractPearl.dataPearlType != DataPearl.AbstractDataPearl.DataPearlType.Misc2)
                            {
                                if ((item as DataPearl).AbstractPearl.dataPearlType != DataPearl.AbstractDataPearl.DataPearlType.PebblesPearl)
                                {
                                    if (!self.State.significantPearls[(int)(item as DataPearl).AbstractPearl.dataPearlType])
                                    {
                                        foreach (KeyValuePair<string, CustomWorldMod.CustomPearl> pearls in CustomWorldMod.customPearls)
                                        {
                                            if (foundPearl) { break; }

                                            DataPearl.AbstractDataPearl.DataPearlType dataPearlType = (DataPearl.AbstractDataPearl.DataPearlType)
                                                        Enum.Parse(typeof(DataPearl.AbstractDataPearl.DataPearlType), pearls.Key);

                                            if((item as DataPearl).AbstractPearl.dataPearlType == dataPearlType)
                                            {
                                                CustomWorldMod.CustomWorldLog($"Loading custom pearl...[{pearls.Key}] from [{pearls.Value.regionID}]");
                                                foundPearl = true;
                                                if (self.currentConversation != null)
                                                {
                                                    self.currentConversation.Interrupt("...", 0);
                                                    self.currentConversation.Destroy();
                                                    self.currentConversation = null;
                                                }
                                                Conversation.ID id = Conversation.ID.None;
                                                try
                                                {
                                                    id = (Conversation.ID)Enum.Parse(typeof(Conversation.ID), "Moon_" + pearls.Key);
                                                } 
                                                catch (Exception e)
                                                {
                                                    CustomWorldMod.CustomWorldLog($"Conversation not found for [{pearls.Key}] + {e}");
                                                }

                                                self.currentConversation = new SLOracleBehaviorHasMark.MoonConversation(id, self, SLOracleBehaviorHasMark.MiscItemType.NA);
                                                self.State.significantPearls[(int)(item as DataPearl).AbstractPearl.dataPearlType] = true;
                                                self.State.totalPearlsBrought++;

                                                Debug.Log("pearls brought up: " + self.State.totalPearlsBrought);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if( foundPearl)
                                        {
                                            self.AlreadyDiscussedItem(true);
                                        }
                                    }
                                }

                            }

                        }

                    }
                    if (foundPearl)
                    {
                        self.State.totalItemsBrought++;
                        self.State.AddItemToAlreadyTalkedAbout(item.abstractPhysicalObject.ID);
                    }
                }
            }

            orig(self, item);
        }

        private static void MoonConversation_AddEvents(On.SLOracleBehaviorHasMark.MoonConversation.orig_AddEvents orig, SLOracleBehaviorHasMark.MoonConversation self)
        {
            bool foundPearl = false;
            foreach (KeyValuePair<string, CustomWorldMod.CustomPearl> pearls in CustomWorldMod.customPearls)
            {
                if (foundPearl) { break; }

                Conversation.ID id = (Conversation.ID)Enum.Parse(typeof(Conversation.ID), "Moon_" + pearls.Value.name);
                if (self.id == id)
                {
                    foundPearl = true;
                    self.PearlIntro();
                    LoadCustomEventsFromFile(pearls.Value.ID, CustomWorldMod.loadedRegions[pearls.Value.regionID], self);
                }

            }
            orig(self);
        }

        private static void LoadCustomEventsFromFile(int fileName, string customRegion, Conversation self)
        {
            CustomWorldMod.CustomWorldLog("~~~LOAD CONVO " + fileName);

            char div = Path.DirectorySeparatorChar;
            string convoPath = Custom.RootFolderDirectory() + CustomWorldMod.resourcePath + customRegion + div +
                "Assets" + div + "Text" + div + "Text_" + LocalizationTranslator.LangShort(CustomWorldMod.rainWorldInstance.inGameTranslator.currentLanguage)
                + div + fileName + ".txt";

            if (!File.Exists(convoPath))
            {
                CustomWorldMod.CustomWorldLog("NOT FOUND " + convoPath);
                return;
            }
            string text2 = File.ReadAllText(convoPath, Encoding.UTF8);
            if (text2[0] == '0')
            {
                Debug.LogError("Tried to encrypt custom text");
                //Conversation.EncryptAllDialogue();
            }
            else
            {
                text2 = Custom.xorEncrypt(text2, (int)(54 + fileName + (int)CustomWorldMod.rainWorldInstance.inGameTranslator.currentLanguage * 7));
            }
            string[] array = Regex.Split(text2, Environment.NewLine);
            try
            {
                if (Regex.Split(array[0], "-")[1] == fileName.ToString())
                {

                    for (int j = 1; j < array.Length; j++)
                    {
                        string[] array3 = Regex.Split(array[j], " : ");
                        if (array3.Length == 3)
                        {
                            self.events.Add(new Conversation.TextEvent(self, int.Parse(array3[0]), array3[2], int.Parse(array3[1])));
                        }
                        else if (array3.Length == 2)
                        {
                            if (array3[0] == "SPECEVENT")
                            {
                                self.events.Add(new Conversation.SpecialEvent(self, 0, array3[1]));
                            }
                            else if (array3[0] == "PEBBLESWAIT")
                            {
                                //self.events.Add(new SSOracleBehavior.PebblesConversation.PauseAndWaitForStillEvent(self, null, int.Parse(array3[1])));
                            }
                        }
                        else if (array3.Length == 1 && array3[0].Length > 0)
                        {
                            self.events.Add(new Conversation.TextEvent(self, 0, array3[0], 0));
                        }
                    }

                }
            }
            catch
            {
                CustomWorldMod.CustomWorldLog("TEXT ERROR");
                self.events.Add(new Conversation.TextEvent(self, 0, "TEXT ERROR", 100));
            }
        }

    }
}
