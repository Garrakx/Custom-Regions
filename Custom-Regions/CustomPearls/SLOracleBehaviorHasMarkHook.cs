using CustomRegions.Mod;
using RWCustom;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using static CustomRegions.Mod.CustomWorldStructs;

namespace CustomRegions.CustomPearls
{

    static class SLOracleBehaviorHasMarkHook
    {
        public static void ApplyHooks()
        {
            On.SLOracleBehaviorHasMark.GrabObject += SLOracleBehaviorHasMark_GrabObject;
            On.SLOracleBehaviorHasMark.MoonConversation.AddEvents += MoonConversation_AddEvents;
        }
        public static void RemoveHooks()
        {
            On.SLOracleBehaviorHasMark.GrabObject -= SLOracleBehaviorHasMark_GrabObject;
            On.SLOracleBehaviorHasMark.MoonConversation.AddEvents -= MoonConversation_AddEvents;
        }

        private static void SLOracleBehaviorHasMark_GrabObject(On.SLOracleBehaviorHasMark.orig_GrabObject orig, 
            SLOracleBehaviorHasMark self, PhysicalObject item)
        {
            if (item is DataPearl dataPearl)
            {
                DataPearl.AbstractDataPearl.DataPearlType pearlType = dataPearl.AbstractPearl.dataPearlType;
                KeyValuePair<int, CustomWorldStructs.CustomPearl> foundPearl = 
                    CustomWorldMod.customPearls.FirstOrDefault(x => x.Value.name.Equals(pearlType.ToString()));

                CustomWorldMod.Log($"Moon grabbed pearl: {pearlType}");

                // Pearl is not vanilla
                if (!foundPearl.Equals(default(KeyValuePair<int, CustomWorldStructs.CustomPearl>)))
                {
                    CustomWorldMod.Log($"Loading custom pearl...[{foundPearl.Value.name}] from [{foundPearl.Value.packName}]");
                    if (!self.State.HaveIAlreadyDescribedThisItem(item.abstractPhysicalObject.ID))
                    {
                        if (self.currentConversation != null)
                        {
                            self.currentConversation.Interrupt("...", 0);
                            self.currentConversation.Destroy();
                            self.currentConversation = null;
                        }
                        Conversation.ID id = Conversation.ID.None;
                        try
                        {
                            id = (Conversation.ID)Enum.Parse(typeof(Conversation.ID), "Moon_" + foundPearl.Value.name);
                        }
                        catch (Exception e)
                        {
                            CustomWorldMod.Log($"Conversation not found for [{foundPearl.Value.name}] + {e}");
                        }

                        self.currentConversation = new SLOracleBehaviorHasMark.MoonConversation(id, self, SLOracleBehaviorHasMark.MiscItemType.NA);
                        self.State.totalPearlsBrought++;
                        CustomWorldMod.Log("pearls brought up: " + self.State.totalPearlsBrought);
                        self.State.totalItemsBrought++;
                        self.State.AddItemToAlreadyTalkedAbout(item.abstractPhysicalObject.ID);

                        // <3 bee <3 ~ base.GrabObject(item)
                        var method = typeof(SLOracleBehavior).GetMethod("GrabObject");
                        var ftn = method.MethodHandle.GetFunctionPointer();
                        var func = (Action<PhysicalObject>)Activator.CreateInstance(typeof(Action<PhysicalObject>), self, ftn);
                        func(item);
                        // <3 bee <3

                        return;
                    }
                }
            }
            orig(self, item);
        }

        private static void MoonConversation_AddEvents(On.SLOracleBehaviorHasMark.MoonConversation.orig_AddEvents orig, 
            SLOracleBehaviorHasMark.MoonConversation self)
        {
            bool foundPearl = false;
            foreach (KeyValuePair<int, CustomPearl> pearls in CustomWorldMod.customPearls)
            {
                if (foundPearl) { break; }

                Conversation.ID id = (Conversation.ID)Enum.Parse(typeof(Conversation.ID), "Moon_" + pearls.Value.name);
                if (self.id == id)
                {
                    foundPearl = true;
                    self.PearlIntro();
                    LoadCustomEventsFromFile(pearls.Value.ID, CustomWorldMod.activatedPacks[pearls.Value.packName], self);
                }

            }
            orig(self);
        }

        private static void LoadCustomEventsFromFile(int fileName, string regionPackFolder, Conversation self)
        {
            CustomWorldMod.Log("~~~LOAD CONVO " + fileName);

            string file = "Text_" + LocalizationTranslator.LangShort(CustomWorldMod.rainWorldInstance.inGameTranslator.currentLanguage)
                + Path.DirectorySeparatorChar + fileName + ".txt";
            string convoPath = CRExtras.BuildPath(regionPackFolder, CRExtras.CustomFolder.Text, file: file);

            if (!File.Exists(convoPath))
            {
                CustomWorldMod.Log("NOT FOUND " + convoPath);
                return;
            }
            string text2 = File.ReadAllText(convoPath, Encoding.Default);
            if (text2[0] == '0')
            {
                CustomWorldMod.EncryptCustomDialogue(CRExtras.BuildPath(regionPackFolder, CRExtras.CustomFolder.Text), regionPackFolder);
            }
            else
            {
                CustomWorldMod.Log($"Decrypting file [{fileName}] at [{regionPackFolder}] in [{CustomWorldMod.rainWorldInstance.inGameTranslator.currentLanguage}]");
                text2 = Custom.xorEncrypt(text2, (int)(54 + fileName + (int)CustomWorldMod.rainWorldInstance.inGameTranslator.currentLanguage * 7));
            }
            string[] array = Regex.Split(text2, Environment.NewLine);
            if (array.Length < 2)
            {
                CustomWorldMod.Log($"Corrupted conversation [{array}]", true);
            }
            try
            {
                if (Regex.Split(array[0], "-")[1] == fileName.ToString())
                {
                    CustomWorldMod.Log($"Moon conversation... [{array[1].Substring(0, Math.Min(array[1].Length, 15))}]");
                    for (int j = 1; j < array.Length; j++)
                    {
                        string[] array3 = Regex.Split(array[j], " : ");

                        if (array3.Length == 1 && array3[0].Length > 0)
                        {
                            self.events.Add(new Conversation.TextEvent(self, 0, array3[0], 0));
                        }
   
                    }

                }
                else
                {
                    CustomWorldMod.Log($"Corrupted dialogue file...[{Regex.Split(array[0], "-")[1]}]", true);
                }
            }
            catch
            {
                CustomWorldMod.Log("TEXT ERROR");
                self.events.Add(new Conversation.TextEvent(self, 0, "TEXT ERROR", 100));
            }
        }

    }
}
