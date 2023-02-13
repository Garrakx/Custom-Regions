using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using Random = UnityEngine.Random;
using static CustomRegions.Mod.CustomRegionsMod;
using CustomRegions.Mod;
using static CustomRegions.Mod.Structs;

namespace CustomRegions.CustomPearls
{
    internal static class Encryption
    {
        public static void EncryptAllCustomPearls()
        {
            foreach (KeyValuePair<DataPearl.AbstractDataPearl.DataPearlType, CustomPearl2> pearl in Data.CustomDataPearlsList)
            {
                EncryptCustomDialogue(pearl.Value.filePath);
            }
        }

        public static void EncryptCustomDialogue(string fileName)
        {
            for (int j = 0; j < ExtEnum<InGameTranslator.LanguageID>.values.Count; j++)
            {
                InGameTranslator.LanguageID languageID = InGameTranslator.LanguageID.Parse(j);
                string pathToConvo = AssetManager.ResolveFilePath("Text"+ Path.DirectorySeparatorChar + "Text_" + LocalizationTranslator.LangShort(languageID) +
                    Path.DirectorySeparatorChar + fileName + ".txt");
                int hash = fileName.GetHashCode();

                string convoLines = File.ReadAllText(pathToConvo, Encoding.Default);
                //Log($"Conversation file: [{convoLines}]");
                if (convoLines[0] == '0')
                {
                    convoLines = Regex.Replace(convoLines, @"\r\n|\r|\n", "\r\n");
                    string[] lines = Regex.Split(convoLines, Environment.NewLine);
                    CustomRegionsMod.CustomLog($"Encrypting file [{Path.GetFileNameWithoutExtension(pathToConvo)}.txt]. " +
                        $"Number of lines [{lines.Length}]");

                    if (lines.Length > 1)
                    {
                        string text4 = Custom.xorEncrypt(convoLines, 54 + hash + InGameTranslator.LanguageID.EncryptIndex(languageID) * 7);
                        text4 = '1' + text4.Remove(0, 1);
                        File.WriteAllText(pathToConvo, text4);
                    }
                    else
                    {
                        CustomRegionsMod.CustomLog($"Failed encrypting. No newLine character found while encrypting. " +
                            $"Try removing all new lines and pressing enter to separate them.", true);
                    }
                }

                else
                {
                    CustomRegionsMod.CustomLog($"Convo already encrypted: [{LocalizationTranslator.LangShort(languageID)}] ({fileName})", false, DebugLevel.FULL);
                }


            }
        }

        public static void LoadEventsFromFile(Conversation self, string fileName, SlugcatStats.Name saveFile = null, bool oneRandomLine = false, int randomSeed = 0)
        {
            if (saveFile == null) { saveFile = self.currentSaveFile; }

            CustomRegionsMod.CustomLog("~~~LOAD CONVO " + fileName);
            InGameTranslator.LanguageID languageID = self.interfaceOwner.rainWorld.inGameTranslator.currentLanguage;
            string text;
            for (; ; )
            {
                text = AssetManager.ResolveFilePath(self.interfaceOwner.rainWorld.inGameTranslator.SpecificTextFolderDirectory(languageID) + Path.DirectorySeparatorChar.ToString() + fileName + ".txt");
                if (saveFile != null)
                {
                    string text2 = text;
                    text = AssetManager.ResolveFilePath(string.Concat(new string[]
                    {
                    self.interfaceOwner.rainWorld.inGameTranslator.SpecificTextFolderDirectory(languageID),
                    Path.DirectorySeparatorChar.ToString(),
                    fileName,
                    "-",
                    saveFile.value,
                    ".txt"
                    }));
                    if (!File.Exists(text))
                    {
                        text = text2;
                    }
                }
                if (File.Exists(text))
                {
                    goto IL_117;
                }
                CustomRegionsMod.CustomLog("NOT FOUND " + text);
                if (!(languageID != InGameTranslator.LanguageID.English))
                {
                    break;
                }
                CustomRegionsMod.CustomLog("RETRY WITH ENGLISH");
                languageID = InGameTranslator.LanguageID.English;
            }
            return;
        IL_117:
            string text3 = File.ReadAllText(text, Encoding.UTF8);
            if (text3[0] != '0')
            {
                text3 = Custom.xorEncrypt(text3, 54 + fileName.GetHashCode() + (int)self.interfaceOwner.rainWorld.inGameTranslator.currentLanguage * 7);
            }

            string[] array = Regex.Split(text3, "\r\n");
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
                            int num;
                            int num2;
                            if (ModManager.MSC && !int.TryParse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture, out num) && int.TryParse(array3[2], NumberStyles.Any, CultureInfo.InvariantCulture, out num2))
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
