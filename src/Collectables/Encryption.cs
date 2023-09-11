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

namespace CustomRegions.Collectables
{
    internal static class Encryption
    {
        public static void EncryptAllCustomPearls()
        {
            foreach (KeyValuePair<DataPearl.AbstractDataPearl.DataPearlType, CustomPearl> pearl in PearlData.CustomDataPearlsList)
            {
                EncryptCustomDialogue(pearl.Value.filePath);
            }
        }

        public static void EncryptCustomDialogue(string fileName)
        {
            for (int i = 0; i < ExtEnum<InGameTranslator.LanguageID>.values.Count; i++)
            {
                InGameTranslator.LanguageID languageID = InGameTranslator.LanguageID.Parse(i);

                for (int j = -1; j < ExtEnum<SlugcatStats.Name>.values.Count; j++)
                {
                    string slugName = "";
                    if (j >= 0)
                    {
                        slugName = "-" + SlugcatStats.Name.values.entries[j];
                    }

                    string pathToConvo = AssetManager.ResolveFilePath("Text" + Path.DirectorySeparatorChar + "Text_" + LocalizationTranslator.LangShort(languageID) +
                        Path.DirectorySeparatorChar + fileName + slugName + ".txt");

                    if (!File.Exists(pathToConvo)) continue;

                    string convoLines = File.ReadAllText(pathToConvo, Encoding.Default);
                    //Log($"Conversation file: [{convoLines}]");
                    if (convoLines[0] != '0')
                    {
                        convoLines = Regex.Replace(convoLines, @"\r\n|\r|\n", "\r\n");
                        string[] lines = Regex.Split(convoLines, Environment.NewLine);
                        CustomLog($"Encrypting file [{Path.GetFileNameWithoutExtension(pathToConvo)}.txt]. " +
                            $"Number of lines [{lines.Length}]");

                        if (lines.Length > 1)
                        {
                            string text4 = Custom.xorEncrypt(convoLines, 54 + OldDecryptKey(fileName) + InGameTranslator.LanguageID.EncryptIndex(languageID) * 7);
                            text4 = '1' + text4.Remove(0, 1);
                            File.WriteAllText(pathToConvo, text4);
                        }
                        else
                        {
                            CustomLog($"Failed encrypting. No newLine character found while encrypting. " +
                                $"Try removing all new lines and pressing enter to separate them.", true);
                        }
                    }

                    else
                    {
                        CustomLog($"Convo already encrypted: [{LocalizationTranslator.LangShort(languageID)}] ({fileName})", false, DebugLevel.FULL);
                    }

                }
            }
        }

        //DecryptKey(fileName + slugName)
        public static int DecryptKey(string fileName) => fileName.Select(x => x - '0').Sum();
        //OldDecryptKey(fileName);
        public static int OldDecryptKey(string fileName) => fileName.GetHashCode();

        public static string DecryptCustomText(string path, InGameTranslator.LanguageID languageID)
        {
            string fileText = File.ReadAllText(path, Encoding.UTF8);
            if (fileText[0] != '0')
            {
                string fileName = Path.GetFileNameWithoutExtension(fileText).Split('-')[0];
                fileText = Custom.xorEncrypt(fileText, 54 + OldDecryptKey(fileName) + (int)languageID * 7);
            }
            return fileText;
        }
    }
}
