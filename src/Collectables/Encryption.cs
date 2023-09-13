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
using static CustomRegions.Mod.CustomRegionsMod;
using static CustomRegions.Mod.Structs;

namespace CustomRegions.Collectables
{
    internal static class Encryption
    {
        public static void EncryptAllCustomPearls()
        {
            CustomLog($"Encrypting pearls", false, DebugLevel.FULL);
            foreach (ModManager.Mod mod in ModManager.ActiveMods)
            {
                for (int i = 0; i < ExtEnum<InGameTranslator.LanguageID>.values.Count; i++)
                {
                    InGameTranslator.LanguageID languageID = InGameTranslator.LanguageID.Parse(i);
                    string directory = Path.Combine(mod.path, $"Text{Path.DirectorySeparatorChar}Text_{LocalizationTranslator.LangShort(languageID)}");

                    if (!Directory.Exists(directory)) continue;

                    string[] files = Directory.GetFiles(directory, "*.txt", SearchOption.AllDirectories);
                    foreach (string file in files)
                    {
                        foreach (KeyValuePair<DataPearl.AbstractDataPearl.DataPearlType, CustomPearl> pearl in PearlData.CustomDataPearlsList)
                        {
                            for (int j = -1; j <ExtEnum<SlugcatStats.Name>.values.Count; j++)
                            {
                                string name = pearl.Value.filePath;
                                if (j > -1)  name += "-" + SlugcatStats.Name.values.entries[j];
                                
                                if (Path.GetFileNameWithoutExtension(file).ToLower() == name.ToLower())
                                {
                                    EncryptCustomDialogue(file);
                                }
                            }
                        }
                    }

                }
            }
        }

        public static void EncryptCustomDialogue(string path)
        {
            string filename = Path.GetFileNameWithoutExtension(path);
            string[] lines = File.ReadAllLines(path, Encoding.UTF8);
            if (lines.Length > 0 && lines[0] == $"0-{filename}")
            {
                CustomLog($"Encrypting convo [{filename}]");
                InGameTranslator.EncryptDecryptFile(path, true);
            }
            else if (lines[0].Length > 0 && lines[0][0] == '1')
            {
                CustomLog($"Skipping encryption for {filename} as it is already encrypted", false, DebugLevel.FULL);
            }
            else
            {
                CustomLog($"Skipping encryption for {filename} as the first line doesn't match encryption requirements.");
                CustomLog($"Make the first line [0-{filename}] in order to encrypt");
            }
        }


        //DecryptKey(fileName + slugName)
        public static int DecryptKey(string fileName) => fileName.Select(x => x - '0').Sum();
        //OldDecryptKey(fileName);
        public static int OldDecryptKey(string fileName) => fileName.GetHashCode();

        public static string DecryptCustomText(string path, InGameTranslator.LanguageID languageID, string pearlName)
        {
            string fileText = InGameTranslator.EncryptDecryptFile(path, false, true);
            if (fileText == null)
            {
                CustomLog("Text was not encrypted! Using unencrypted text");
                return File.ReadAllText(path);
            }
            else 
            {
                string[] array = Regex.Split(fileText, "\r\n");
                if (array.Length > 0 && array[0].Length > 0 && array[0].Substring(1) == $"-{Path.GetFileNameWithoutExtension(path)}")
                {
                    return fileText;
                }
                else
                {
                    CustomLog("Decryption failed, assuming legacy encryption");
                    return Custom.xorEncrypt(File.ReadAllText(path, Encoding.UTF8), 54 + pearlName.GetHashCode() + (int)languageID * 7);
                }
            }
        }
    }
}
