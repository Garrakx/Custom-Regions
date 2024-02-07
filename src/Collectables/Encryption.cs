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
using System.Reflection;
using System.Diagnostics;

namespace CustomRegions.Collectables
{
    internal static class Encryption
    {
        public static void ApplyHooks()
        {
            On.MoreSlugcats.ChatlogData.DecryptResult += ChatlogData_DecryptResult;
        }

        private static string ChatlogData_DecryptResult(On.MoreSlugcats.ChatlogData.orig_DecryptResult orig, string result, string path)
        {
            if (result[0] == '0') return result;
            return orig(result, path);
        }

        public static void EncryptAllCustomPearls()
        {
            CustomLog($"\nEncrypting pearls", false, DebugLevel.FULL);

            var normalizedSlugcatNames = SlugcatStats.Name.values.entries
                .Select(name => name.ToLower())
                .ToList();
            var normalizedPearlNames = PearlData.CustomDataPearlsList
                .Select(entry => entry.Value.filePath.ToLower())
                .ToList();
            
            foreach (ModManager.Mod mod in ModManager.ActiveMods)
            {
                if (mod.id == "moreslugcats" || mod.id == "expedition" || mod.id == "rwremix" || mod.id == "jollycoop") continue;
                if (File.Exists(Path.Combine(mod.path, $"Text{Path.DirectorySeparatorChar}NoEncryption.txt"))) continue;

                for (int i = 0; i < ExtEnum<InGameTranslator.LanguageID>.values.Count; i++)
                {
                    InGameTranslator.LanguageID languageID = InGameTranslator.LanguageID.Parse(i);
                    string directory = Path.Combine(mod.path, $"Text{Path.DirectorySeparatorChar}Text_{LocalizationTranslator.LangShort(languageID)}");

                    if (!Directory.Exists(directory)) continue;

                    string[] files = Directory.GetFiles(directory, "*.txt", SearchOption.AllDirectories);
                    foreach (string file in files)
                        EncryptIfDialogFile(file, ref normalizedSlugcatNames, ref normalizedPearlNames);
                }
            }
        }

        private static void EncryptIfDialogFile(string file, ref List<string> normalizedSlugcatNames, ref List<string> normalizedPearlNames)
        {
            string normalizedFileName = Path.GetFileNameWithoutExtension(file).ToLower();
            foreach (string pearlName in normalizedPearlNames)
            {   
                if (!normalizedFileName.StartsWith(pearlName)) continue;
                if (normalizedFileName == pearlName)
                {
                    EncryptCustomDialogue(file);
                    return;
                }
                foreach (string slugcat in normalizedSlugcatNames)
                {
                    if (normalizedFileName != pearlName + "-" + slugcat) continue;
                    EncryptCustomDialogue(file);
                    return;
                }
            }
        }

        public static void EncryptCustomDialogue(string path)
        {
            string filename = Path.GetFileNameWithoutExtension(path);
            string[] lines = File.ReadAllLines(path, Encoding.UTF8);
            if (lines.Length > 0 && lines[0].ToLower() == $"0-{filename}".ToLower())
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
                CustomLog($"Skipping encryption for {filename} as the first line doesn't match encryption requirements.", false, DebugLevel.MEDIUM);
                CustomLog($"Make the first line [0-{filename}] in order to encrypt", false, DebugLevel.MEDIUM);
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
                if (array.Length > 0 && array[0].Length > 0 && array[0].Substring(1).ToLower() == $"-{Path.GetFileNameWithoutExtension(path).ToLower()}")
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
