using CustomRegions.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections.Generic;
using System.IO;
using static CustomRegions.Mod.Structs;

namespace CustomRegions.CustomPearls
{
    internal static class CustomConvo
    {
        public static void ApplyHooks()
        {
            On.Conversation.DataPearlToConversation += Conversation_DataPearlToConversation;
            On.SLOracleBehaviorHasMark.MoonConversation.AddEvents += MoonConversation_AddEvents;
            IL.Conversation.LoadEventsFromFile_int_Name_bool_int += Conversation_LoadEventsFromFileIL;
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
                c.EmitDelegate((Conversation self, int fileName, SlugcatStats.Name saveFile, InGameTranslator.LanguageID languageID, string original) => SearchConvoFile(self, fileName.ToString(), saveFile, languageID) ?? original);
                c.Emit(OpCodes.Stloc_1);
            }
            else 
            {
                CustomRegionsMod.BepLogError("failed to il hook Conversation.LoadEventsFromFile!");
            }
        }

        public static string SearchConvoFile(Conversation self, string fileName, SlugcatStats.Name saveFile, InGameTranslator.LanguageID languageID)
        {
            string langDirectory = self.interfaceOwner.rainWorld.inGameTranslator.SpecificTextFolderDirectory(languageID) + Path.DirectorySeparatorChar;

            string oracleName = "";

            if (self.interfaceOwner is OracleBehavior behavior)
            {
                oracleName = behavior.oracle.ID.value + Path.DirectorySeparatorChar;
            }

            string slugName = "";
            if (saveFile != null)
            {
                slugName = "-" + saveFile.value;
            }

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
                
                if (File.Exists(AssetManager.ResolveFilePath(text))) return AssetManager.ResolveFilePath(text);
            }
            return null;
        }

        static string Check(string path) => File.Exists(AssetManager.ResolveFilePath(path)) ? AssetManager.ResolveFilePath(path) : null;

        private static void MoonConversation_AddEvents(On.SLOracleBehaviorHasMark.MoonConversation.orig_AddEvents orig, SLOracleBehaviorHasMark.MoonConversation self)
        {
            orig(self);

            foreach (KeyValuePair<DataPearl.AbstractDataPearl.DataPearlType, CustomPearl> customPearl in Data.CustomDataPearlsList)
            {
                if (self.id == customPearl.Value.conversationID)
                {
                    self.PearlIntro();
                    Encryption.LoadEventsFromFile(self, customPearl.Value.filePath);
                    return;
                }
            }
        }

        private static Conversation.ID Conversation_DataPearlToConversation(On.Conversation.orig_DataPearlToConversation orig, DataPearl.AbstractDataPearl.DataPearlType type)
        {
            if (Data.CustomDataPearlsList.TryGetValue(type, out CustomPearl customPearl))
            {
                CustomRegionsMod.CustomLog($"Found custom pearl conversation {customPearl.conversationID}");
                return customPearl.conversationID;
            }

            { return orig(type); }
        }
    }
}
