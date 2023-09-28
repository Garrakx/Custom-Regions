using CustomRegions.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using static CustomRegions.Mod.Structs;
using Random = UnityEngine.Random;

namespace CustomRegions.Collectables
{
    public static class CustomConvo
    {
        internal static void ApplyHooks()
        {
            On.Conversation.DataPearlToConversation += Conversation_DataPearlToConversation;
            On.SLOracleBehaviorHasMark.MoonConversation.AddEvents += MoonConversation_AddEvents;
            IL.Conversation.LoadEventsFromFile_int_Name_bool_int += Conversation_LoadEventsFromFileIL;
            IL.SLOracleBehaviorHasMark.GrabObject += SLOracleBehaviorHasMark_GrabObject;
        }

        /// <summary>
        /// Lets Saint pearls be read if Saint-specific txts exist
        /// </summary>
        private static void SLOracleBehaviorHasMark_GrabObject(ILContext il)
        {
            var c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdsfld<DataPearl.AbstractDataPearl.DataPearlType>(nameof(DataPearl.AbstractDataPearl.DataPearlType.LF_west)),
                x => x.MatchCall(out _)))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate((bool flag, SLOracleBehaviorHasMark self, PhysicalObject item) => {
                    string pearl = (item as DataPearl).abstractPhysicalObject.type.value;
                    SlugcatStats.Name name = self.oracle.room.game.StoryCharacter;

                    return flag || SearchConvoFile(self, pearl, name, out _, slugExclusive: true) != null; 
                });
            }
        }

        private static void Conversation_LoadEventsFromFileIL(MonoMod.Cil.ILContext il)
        {
            var c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.AfterLabel,
                x => x.MatchLdloc(1),
                x => x.MatchCall(typeof(File), nameof(File.Exists)),
                x => x.MatchBrtrue(out _),
                x => x.MatchLdstr("NOT FOUND ") //lol
                ))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldarg, 1);
                c.Emit(OpCodes.Ldarg_2);
                c.Emit(OpCodes.Ldloc_0);
                c.Emit(OpCodes.Ldloc_1);
                c.EmitDelegate((Conversation self, int fileName, SlugcatStats.Name saveFile, InGameTranslator.LanguageID languageID, string original) => SearchConvoFile(self.interfaceOwner, fileName.ToString(), saveFile, out _) ?? original);
                c.Emit(OpCodes.Stloc_1);
            }
            else
            {
                CustomRegionsMod.BepLogError("failed to il hook Conversation.LoadEventsFromFile!");
            }
        }

        private static void MoonConversation_AddEvents(On.SLOracleBehaviorHasMark.MoonConversation.orig_AddEvents orig, SLOracleBehaviorHasMark.MoonConversation self)
        {
            orig(self);

            foreach (KeyValuePair<DataPearl.AbstractDataPearl.DataPearlType, CustomPearl> customPearl in PearlData.CustomDataPearlsList)
            {
                if (self.id == customPearl.Value.conversationID)
                {
                    self.PearlIntro();
                    LoadEventsFromFile(self, customPearl.Value.filePath);
                    return;
                }
            }
        }

        private static Conversation.ID Conversation_DataPearlToConversation(On.Conversation.orig_DataPearlToConversation orig, DataPearl.AbstractDataPearl.DataPearlType type)
        {
            if (PearlData.CustomDataPearlsList.TryGetValue(type, out CustomPearl customPearl))
            {
                CustomRegionsMod.CustomLog($"Found custom pearl conversation {customPearl.conversationID}");
                return customPearl.conversationID;
            }

            { return orig(type); }
        }

        public static void LoadEventsFromFile(Conversation self, string fileName, SlugcatStats.Name saveFile = null, bool oneRandomLine = false, int randomSeed = 0)
        {
            LoadEventsFromFile(self, fileName, null, saveFile, oneRandomLine, randomSeed);
        }

        public static void LoadEventsFromFile(Conversation self, string fileName, Oracle.OracleID oracleID, SlugcatStats.Name saveFile = null, bool oneRandomLine = false, int randomSeed = 0)
        {
            if (saveFile == null) { saveFile = self.currentSaveFile; }

            CustomRegionsMod.CustomLog("~~~LOAD CONVO " + fileName);

            string path = CustomConvo.SearchConvoFile(self.interfaceOwner, fileName, saveFile, out var languageID, oracleID);
            if (!File.Exists(path)) return;

            string fileText = Encryption.DecryptCustomText(path, languageID, fileName);

            CustomRegionsMod.CustomLog(fileText, false, CustomRegionsMod.DebugLevel.FULL);

            string[] array = Regex.Split(fileText, "\r\n");
            ParseConvoText(self, array, oneRandomLine, randomSeed);
        }

        public static string SearchConvoFile(Conversation.IOwnAConversation interfaceOwner, string fileName, SlugcatStats.Name saveFile, out InGameTranslator.LanguageID languageID, Oracle.OracleID oracleID = null, bool iteratorExclusive = false, bool slugExclusive = false)
        {
            languageID = interfaceOwner.rainWorld.inGameTranslator.currentLanguage;

            string oracleName = "";
            if (oracleID != null) oracleName = oracleID.value + Path.DirectorySeparatorChar;
            else if (interfaceOwner is OracleBehavior behavior) oracleName = behavior.oracle.ID.value + Path.DirectorySeparatorChar;

            string slugName = saveFile != null ? "-" + saveFile.value : "";

            for (; ; )
            {
                string langDirectory = interfaceOwner.rainWorld.inGameTranslator.SpecificTextFolderDirectory(languageID) + Path.DirectorySeparatorChar;

                for (int i = 0; i < 4; i++)
                {
                    string text = i switch
                    {
                        0 => langDirectory + oracleName + fileName + slugName + ".txt",
                        1 => langDirectory + oracleName + fileName + ".txt",
                        2 => langDirectory + fileName + slugName + ".txt",
                        _ => langDirectory + fileName + ".txt"
                    };
                    CustomRegionsMod.CustomLog($"Searching for pearl convo at path [{text}]: {(File.Exists(AssetManager.ResolveFilePath(text)) ? "Found!" : "Not Found")}", false, CustomRegionsMod.DebugLevel.FULL);

                    if (iteratorExclusive && (i != 0 || i != 1)) continue;
                    if (slugExclusive && (i != 0 || i != 2)) continue;

                    if (File.Exists(AssetManager.ResolveFilePath(text))) return AssetManager.ResolveFilePath(text);
                }

                CustomRegionsMod.CustomLog("NOT FOUND " + fileName);
                if (languageID == InGameTranslator.LanguageID.English) break;

                CustomRegionsMod.CustomLog("RETRY WITH ENGLISH");
                languageID = InGameTranslator.LanguageID.English;
            }
            return null;
        }

        public static void ParseConvoText(Conversation self, string[] array, bool oneRandomLine = false, int randomSeed = 0)
        {
            try
            {
                if (oneRandomLine)
                {
                    List<Conversation.TextEvent> list = new List<Conversation.TextEvent>();
                    for (int i = 1; i < array.Length; i++)
                    {
                        string[] array2 = LocalizationTranslator.ConsolidateLineInstructions(array[i]);
                        if (array2.Length == 3)
                        {
                            list.Add(new Conversation.TextEvent(self, int.Parse(array2[0], NumberStyles.Any, CultureInfo.InvariantCulture), array2[2], int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture)));
                        }
                        else if (array2.Length == 1 && array2[0].Length > 0)
                        {
                            list.Add(new Conversation.TextEvent(self, 0, array2[0], 0));
                        }
                    }
                    if (list.Count > 0)
                    {
                        Random.State state = Random.state;
                        Random.InitState(randomSeed);
                        Conversation.TextEvent item = list[Random.Range(0, list.Count)];
                        Random.state = state;
                        self.events.Add(item);
                    }
                }
                else
                {
                    for (int j = 1; j < array.Length; j++)
                    {
                        string[] array3 = LocalizationTranslator.ConsolidateLineInstructions(array[j]);
                        if (array3.Length == 3)
                        {
                            if (ModManager.MSC && !int.TryParse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture, out int num) && int.TryParse(array3[2], NumberStyles.Any, CultureInfo.InvariantCulture, out int num2))
                            {
                                self.events.Add(new Conversation.TextEvent(self, int.Parse(array3[0], NumberStyles.Any, CultureInfo.InvariantCulture), array3[1], int.Parse(array3[2], NumberStyles.Any, CultureInfo.InvariantCulture)));
                            }
                            else
                            {
                                self.events.Add(new Conversation.TextEvent(self, int.Parse(array3[0], NumberStyles.Any, CultureInfo.InvariantCulture), array3[2], int.Parse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture)));
                            }
                        }
                        else if (array3.Length == 2)
                        {
                            if (array3[0] == "SPECEVENT")
                            {
                                self.events.Add(new Conversation.SpecialEvent(self, 0, array3[1]));
                            }
                            else if (array3[0] == "PEBBLESWAIT")
                            {
                                self.events.Add(new SSOracleBehavior.PebblesConversation.PauseAndWaitForStillEvent(self, null, int.Parse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture)));
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
                CustomRegionsMod.CustomLog("TEXT ERROR");
                self.events.Add(new Conversation.TextEvent(self, 0, "TEXT ERROR", 100));
            }
        }

    }
}
