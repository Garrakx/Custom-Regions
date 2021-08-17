using Partiality.Modloader;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Security;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Linq;
using System.Text;
using Menu;
using static CustomRegions.Mod.CustomWorldStructs;
using CustomRegions.CustomPearls;
using CustomRegions.Creatures;
using CustomRegions.Music;
using CustomRegions.DevInterface;
using CustomRegions.CustomMenu;
using PastebinMachine.EnumExtender;
using System.Net;

// Delete Publicity Stunt requirement by pastebee
[assembly: IgnoresAccessChecksTo("Assembly-CSharp")]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[module: UnverifiableCode]

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class IgnoresAccessChecksToAttribute : Attribute
    {
        public IgnoresAccessChecksToAttribute(string assemblyName)
        {
            AssemblyName = assemblyName;
        }
        public string AssemblyName { get; }
    }
}

namespace CustomRegions.Mod
{

    public class CustomWorldMod : PartialityMod
    {
        //public static CustomWorldScript script;
        //public static CustomWorldConfig config;
        public static CustomWorldOption customWorldOption;
        public static CustomWorldMod mod;
        public static string versionCR = "";

        public CustomWorldMod()
        {
            mod = this;
            ModID = "Custom Regions Mod";
            Version = "0.9." + version + "-experimental.3";
            author = "Garrakx";
            versionCR = $"v{Version}";
        }

        // Code for AutoUpdate support

        // Update URL - don't touch!
        public string updateURL = "http://beestuff.pythonanywhere.com/audb/api/mods/3/0";
        public int version = 41;//NEEDS UPDATE, should be 42

        // Public key in base64 - don't touch!
        public string keyE = "AQAB";
        public string keyN = "13Mr+YOzb1iLnJvzkuP4NEZEWwOtWKWvWAN0HdsQ5SF2+RG7k8FbtmQut+2+69ideiJHDW66jWBcGGvfiQ0+5yLAUBpGSckC7V79yZgFQT39lvgU0ykAjonkA+ZTODFnehubyCkrrrzwno4boZghEZmDS2YsSyDJ6RLJyD2/WeCokcTj1vIHZhY9DzkooFtejz9yI/PCZtq8tfq2AzSiQPS+0xGQs3fnAkOGoV1WZ/inW5/rRyjD5HICr8t79UmcopfRK383YBrf2G96HeVYvY2vwSS/BW/m32rTLOZHr+XX7SIZshz7BLK6xEssy4qXjskvAUshqNudxtQnIkShGJuKWF1V2vvwqgY/IZiAbDXdBOUaSd09ldHBlTz9EfzBcgqffVRaUTzS71yGLISyrLriezozlK1YZW9vvijpbD0rmDaJ4aq9s6EzhdgVkTEuChtm/Fj9pgsswjvkbgHw1t9QZWqu4pweNd3IE/Lktst8HBKLiw1aRaffbZIhh1apbyjF8iflD8sNzbIHEfEvc35MEwIFqibJVnVxppBa15HpOxeXOzwuTjFaLSURRvbOEFPmpyd1Nm4nMzZZHHPjQXT7oYQAxjSCfqnLAdYsEnNo/2172jJGLfBWWGFTavqiCYqLhjtYkPfRgpcdw4FldgjX4w7RGMD/Ra5VXvmDMTE=";
        // ------------------------------------------------


        public override void OnEnable()
        {
            base.OnEnable();

            try
            {
                if (File.Exists(Custom.RootFolderDirectory() + "debugCRS.txt"))
                {
                    string debugLevel = File.ReadAllText(Custom.RootFolderDirectory() + "debugCRS.txt");
                    if (debugLevel.Contains("FULL"))
                    {
                        CustomWorldMod.debugLevel = DebugLevel.FULL;
                    }
                    else if (debugLevel.Contains("MEDIUM"))
                    {
                        CustomWorldMod.debugLevel = DebugLevel.MEDIUM;
                    }
                    else
                    {
                        CustomWorldMod.debugLevel = DebugLevel.FULL;
                    }
                }
            } catch (Exception e) { CustomWorldMod.Log($"Could not read debug level file \n{e}", true); }

            CustomWorldMod.debugLevel = DebugLevel.FULL;
            CustomWorldMod.Log("Forcing DEBUG, don't forget to remove for release", true);

            bool usingBepinex = false;
            try
            {
                foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.GetName().Name.Equals("BepInEx.MonoMod.Loader") || assembly.GetName().Name.Contains("BepInEx"))
                    {
                        usingBepinex = true;
                        break;
                    }
                }
                CustomWorldMod.usingBepinex = usingBepinex;
                CustomWorldMod.Log($"Using BepInEx [{usingBepinex}]");
            }
            catch (Exception e)
            {
                CustomWorldMod.Log($"Error checking the modloaer \n{e}", true);
            }


            // Initialize scripts
            scripts = new List<CustomWorldScript>();

            CustomWorldMod.CreateCustomWorldLog();
            CustomWorldMod.CreateCustomWorldFolders();

            // Load from file
            CustomWorldMod.analyzingLog = string.Empty;

            if (!usingBepinex)
            {
                CustomWorldMod.Log("You are using Partiality, which is no longer worked on and has issues. " +
                        "Consider upgrading to BepInEx. Instructions for switching: [https://youtu.be/brDN_8uN6-U]", true);
            }

            Hooks.ApplyAllHooks();

            // Create exe updater
            if (scripts.FindAll(x => x is ExeUpdater).Count == 0 && !OfflineMode) 
            { 
               CustomWorldMod.scripts.Add(new ExeUpdater(CustomWorldMod.hashOnlineUrl, CustomWorldMod.executableUrl));
               CustomWorldMod.Log($"Creating pack downloader...");
            }

            // Grab news
            if (scripts.FindAll(x=> x is NewsFetcher).Count == 0 && !OfflineMode)
            {
                CustomWorldMod.scripts.Add(new NewsFetcher(CustomWorldMod.newsUrl));
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            Hooks.RemoveAllHooks();
        }



        public static CustomWorldOption LoadOI()
        {
            customWorldOption = new CustomWorldOption();
            return customWorldOption;
        }


        /// <summary>
        /// Returns the vanilla regions ID.
        /// </summary>
        public static string[] VanillaRegions()
        {
            return new string[] { "CC", "DS", "HI", "GW", "SI", "SU", "SH", "SL", "LF", "UW", "SB", "SS" };
        }


        /// <summary>
        /// Dictionary with all installed region packs, where the Key is the region pack name and the value is a struct with its information.
        /// </summary>
        public static Dictionary<string, RegionPack> installedPacks;

        /// <summary>
        /// Dictionary with region packs available on raindb, where the Key is the region pack name and the value is a struct with its information.
        /// </summary>
        public static Dictionary<string, RegionPack> rainDbPacks;


        /// <summary>
        /// Dictionary with activated region packs, where the Key is the region pack name and the value is the folder.
        /// </summary>
        public static Dictionary<string, string> activatedPacks;

        /// <summary>
        /// Dictionary with custom pearls loaded from activated regions. Key is hash and value is Pearl Information
        /// </summary>
        public static Dictionary<int, CustomPearl> customPearls;

        /// <summary>
        /// Dictionary with arena unlocks. Key is level name, and Value is where it should be unlocked.
        /// </summary>
        public static Dictionary<string, string> levelUnlocks;


        /// <summary>
        /// List containing activated custom regions
        /// </summary>
        public static List<string> activeModdedRegions;

        /// <summary>
        /// Dictionary containing the thumbnails. Key is the pack name and value is the thumb in byte array.
        /// </summary>
        public static Dictionary<string, byte[]> downloadedThumbnails;


        /// <summary>
        /// List containing Custom Regions scripts
        /// </summary>
        public static List<CustomWorldScript> scripts;

        /// <summary>
        /// Dictionary with all installed regions, where the Key is the region ID and the value is a struct with its information.
        /// </summary>
        //public static Dictionary<string, RegionPack> availableRegions;


        /// <summary>
        /// Dictionary with all installed regions, where the Key is the region ID and the value is a struct with its configuration.
        /// </summary>
        //public static Dictionary<string, RegionConfiguration> configurationRegions;

        readonly static string[] ResourceFolders = {
            "Atlases", "Audio", "Decals", "Illustrations", "LoadedSoundEffects", "Music", "Palettes", "Projections"
        };

        public static string[] AvailableResourceFolders;

        public static readonly string assemblyLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        public static readonly string exeDownloaderLocation = CustomWorldMod.assemblyLocation + @"\RegionPackDownloader.exe";

        /// <summary>
        /// path of the CustomResources folder (Mods\CustomResources\)
        /// </summary>
        public readonly static string resourcePath = "Mods" + Path.DirectorySeparatorChar + "CustomResources" + Path.DirectorySeparatorChar;

        /// <summary>
        /// path of the CustomResources save folder (UserData\CustomRegionSaveData\)
        /// </summary>
        public readonly static string regionSavePath = "UserData" + Path.DirectorySeparatorChar + "CustomRegionSaveData" + Path.DirectorySeparatorChar;

        /// <summary>
        /// URL to grab region pack information
        /// </summary>
        public readonly static string packFetcherUrl = @"http://garrakx.pythonanywhere.com/raindb.json";
        public readonly static string hashOnlineUrl = @"http://garrakx.pythonanywhere.com/executable_hash.txt";
        public readonly static string executableUrl = @"http://garrakx.pythonanywhere.com/RegionPackDownloader.exe";
        public readonly static string newsUrl = @"http://garrakx.pythonanywhere.com/news.txt";

        /// <summary>
        /// Divider A used for CR save
        /// </summary>
        public readonly static string saveDividerA = "<CRdivA>";
        /// <summary>
        /// Divider B used for CR save
        /// </summary>
        public readonly static string saveDividerB = "<CRdivB>";

        public readonly static string removeWorldLineDiv = "_REMOVECRS";

        /// <summary>
        /// Rain world game instance
        /// </summary>
        internal static RainWorld rainWorldInstance = null;


        public readonly static int numberOfVanillaRegions = VanillaRegions().Length;

        /// <summary>
        /// Array of lists containing loaded packs info for each saveslot
        /// </summary>
        public static List<RegionPack>[] packInfoInSaveSlot;

        /// <summary>
        /// Array of SaveProblems for each saveslot
        /// </summary>
        public static SaveProblems[] saveProblems;

        /// <summary>
        /// Strings that stores installation problems
        /// </summary>
        public static string analyzingLog;

        /// <summary>
        /// Bool that displays if the user is using BepInEx modloader or no
        /// </summary>
        internal static bool usingBepinex;


        public static readonly string customUnlocksFileName = "customUnlocks";

        public static bool OfflineMode { get; set; } = File.Exists(Custom.RootFolderDirectory() + resourcePath + "offline.txt");

        
        public enum DebugLevel {RELEASE, MEDIUM, FULL}

        public static DebugLevel debugLevel = DebugLevel.RELEASE;
        

        /// <summary>
        /// Method used for translating with Config Machine
        /// </summary>
        public static string Translate(string orig)
        {
            if (customWorldOption != null) 
            {
                return customWorldOption.Translate(orig);
            }
            return orig;
        }


        /// <summary>
        /// Builds a dictionary where the Key is the region ID and the value is the region name folder.
        /// </summary>
        public static void BuildModRegionsDictionary()
        {
            CustomWorldMod.activeModdedRegions = new List<string>();

            // Only load activate regions from CustomWorldMod.availableRegions
            Dictionary<string, string> activatedPacksTemp = new Dictionary<string, string>();
            Dictionary<string, RegionPack> installedPacksUpdated = new Dictionary<string, RegionPack>();

            foreach (KeyValuePair<string, RegionPack> regionPack in CustomWorldMod.installedPacks)
            {
                int packNumber = 0;
                try
                {
                    if (regionPack.Value.activated)
                    {
                        RegionPack regionPackUpdate = regionPack.Value;
                        foreach (string newRegion in regionPackUpdate.regions)
                        {
                            //infoRegionUpdated.newRegions[newRegion.Key].regionNumber = regionNumber;
                            //regionNumber++;

                            // Add new region
                            if (!CustomWorldMod.activeModdedRegions.Contains(newRegion))
                            {
                                CustomWorldMod.activeModdedRegions.Add(newRegion);
                            }
                            else
                            {
                                CustomWorldMod.Log($"The region pack [{regionPack.Key}] is adding a duplicate region!. " +
                                    $"The field (regions) in packInfo.json should *only* include new regions added by the pack.", false, DebugLevel.MEDIUM);
                            } 

                            try{EnumExtender.AddDeclaration(typeof(MenuScene.SceneID), "Landscape_" + newRegion);}
                            catch (Exception e){CustomWorldMod.Log("Error extending enums " + e, true);}
                        }

                        regionPackUpdate.loadNumber = packNumber;
                        activatedPacksTemp.Add(regionPack.Key, regionPack.Value.folderName);
                        installedPacksUpdated.Add(regionPack.Key, regionPackUpdate);
                        packNumber++;
                    }
                    else
                    {
                        installedPacksUpdated.Add(regionPack.Key, regionPack.Value);
                    }

                }
                catch (Exception e) { CustomWorldMod.Log($"Error while trying to add customRegion: {e}", true); }
            }

            // Add declaration of LevelUnlockIDs
            var disctinctIDs = CustomWorldMod.levelUnlocks.Values.Distinct();
            foreach (string unlockID in disctinctIDs)
            {
                EnumExtender.AddDeclaration(typeof(MultiplayerUnlocks.LevelUnlockID), unlockID);
                CustomWorldMod.Log($"Adding declaration LevelUnlockID [{unlockID}]", false, DebugLevel.FULL);
            }

            try
            {
                EnumExtender.ExtendEnumsAgain();

                string[] names = Enum.GetNames(typeof(MenuScene.SceneID));
                names = names.Skip(names.Length - CustomWorldMod.activeModdedRegions.Count).ToArray();
                List<string> debug = new List<string>(names);
                Log($"Extending SceneID enum ... [{string.Join(", ", debug.ToArray())}]");

                string[] names2 = Enum.GetNames(typeof(MultiplayerUnlocks.LevelUnlockID));
                names2 = names2.Skip(names2.Length - disctinctIDs.ToList().Count).ToArray();
                List<string> debug2 = new List<string>(names2);
                Log($"Extending LevelUnlockID enum ... [{string.Join(", ", debug2.ToArray())}]");

            }
            catch (Exception e)
            {
                Log("Error extending SceneID enum " + e, true);
            }

            CustomWorldMod.installedPacks = installedPacksUpdated;
            CustomWorldMod.activatedPacks = activatedPacksTemp;

            CustomWorldMod.Log($"Activated region packs [{string.Join(", ", new List<string>(CustomWorldMod.activatedPacks.Keys).ToArray())}]");
            CustomWorldMod.Log($"New Custom Regions added by Region Packs [{string.Join(", ", activeModdedRegions.ToArray())}]");

            List<string> availableResourceFoldersTemp = null;
            foreach (KeyValuePair<string, string> regionPack in CustomWorldMod.activatedPacks)
            {
                //string folder = Custom.RootFolderDirectory() + resourcePath + regionPack.Value + @"\Assets\Futile\Resources";
                string folder = CRExtras.BuildPath(regionPack.Value, CRExtras.CustomFolder.Resources);

                if (Directory.Exists(folder))
                {
                    foreach (string dir in Directory.GetDirectories(folder))
                    {
                        string folderName = new DirectoryInfo(dir).Name;
                        //Log($"FolderName [{folderName}]");
                        if (availableResourceFoldersTemp == null) { availableResourceFoldersTemp = new List<string>(); }
                        if (!availableResourceFoldersTemp.Contains(folderName) && ResourceFolders.Contains(folderName))
                        {
                            availableResourceFoldersTemp.Add(folderName);
                        }
                    }
                }
            }
            if (availableResourceFoldersTemp != null)
            {
                AvailableResourceFolders = availableResourceFoldersTemp.ToArray();
                CustomWorldMod.Log($"CR will load resources from the following folders: [{string.Join(", ", AvailableResourceFolders)}]");
            }
        }

        /// <summary>
        /// Creaters the custom world log file
        /// </summary>
        public static void CreateCustomWorldLog()
        {
            using (StreamWriter sw = File.CreateText(Custom.RootFolderDirectory() + "customWorldLog.txt"))
            {
                sw.WriteLine($"############################################\n Custom World Log {versionCR} [DEBUG LEVEL: {CustomWorldMod.debugLevel}]\n");
            }
        }

        /// <summary>
        /// Creaters the folders used by CR (CustomAssets and CustomWorldSaves)
        /// </summary>
        public static void CreateCustomWorldFolders()
        {
            if (!Directory.Exists(Custom.RootFolderDirectory() + resourcePath))
            {
                Directory.CreateDirectory(Custom.RootFolderDirectory() + resourcePath);
            }

            if (!Directory.Exists(Custom.RootFolderDirectory() + regionSavePath))
            {
                Directory.CreateDirectory(Custom.RootFolderDirectory() + regionSavePath);
            }
        }

        /// <summary>
        /// Appends the provided string to the log. If log file doesn't exist, create it.
        /// </summary>
        public static void Log(string logText)
        {
            if (!File.Exists(Custom.RootFolderDirectory() + "customWorldLog.txt"))
            {
                CreateCustomWorldLog();
            }

            try
            {
                using (StreamWriter file = new StreamWriter(Custom.RootFolderDirectory() + "customWorldLog.txt", true))
                {
                    file.WriteLine(logText);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        /// <summary>
        /// Appends the provided string to the log. If log file doesn't exist, create it. Bool indicates if you want to log into exceptionlog as well
        /// </summary>
        public static void Log(string logText, bool throwException)
        {
            if (throwException)
            {
                Debug.LogError("[CustomRegions] " + logText);
                logText = "[ERROR] " + logText + "\n";
            }
            Log(logText);
        }

        public static void Log(string logText, bool throwException, DebugLevel minDebugLevel)
        {
            if (minDebugLevel <= CustomWorldMod.debugLevel)
            {
                Log(logText, throwException);
            }
        }

        /// <summary>
        /// Builds available regions, loaded regions and save analyzer
        /// </summary>
        public static void LoadCustomWorldResources()
        {
            InitializeDictionaries();

            if (OfflineMode)
            {
                Log("RUNNING CUSTOM REGIONS IN OFFLINE MODE");
            }
            else
            {
                // Cascades all the loading scripts
                CustomWorldMod.FetchPackInfo();
            }


            try
            {
                string[] vanillaRegions = VanillaRegions();
                string[] installedRegions = File.ReadAllLines(Custom.RootFolderDirectory() + "World" +
                    Path.DirectorySeparatorChar + "Regions" + Path.DirectorySeparatorChar + "regions.txt");

                List<string> addedRegions = installedRegions.ToList().Except(vanillaRegions.ToList()).ToList();
                int numberOfInstalledRegions = installedRegions.Length;
                Log($"Number of regions installed in vanilla folder [{numberOfInstalledRegions}]");
                if (numberOfInstalledRegions != vanillaRegions.Length)
                {
                    Log($"You have [{numberOfInstalledRegions - numberOfVanillaRegions}] region(s) merged with vanilla files, " +
                        $"CR might not work correctly. Extra regions [{string.Join(", ", addedRegions.ToArray())}]", true);
                }
            }
            catch { }

            CustomWorldMod.LoadInstalledPacks();

            CustomWorldMod.BuildModRegionsDictionary();

            if (OfflineMode) { CustomWorldMod.LoadThumbnails(); }

            CustomWorldMod.ReadSaveAnalyzerFiles();

            CustomWorldMod.AnalyzeSave();

            try
            {
                CustomWorldMod.Log("Reloading player progresion...");
                CustomWorldMod.rainWorldInstance.progression =
                    new PlayerProgression(CustomWorldMod.rainWorldInstance, CustomWorldMod.rainWorldInstance.setup.loadProg);
            }
            catch (Exception e) { CustomWorldMod.Log($"Could not reload player progression [{e}]"); }

        }

        private static void InitializeDictionaries()
        {
            CustomWorldMod.installedPacks = new Dictionary<string, RegionPack>();
            CustomWorldMod.rainDbPacks = new Dictionary<string, RegionPack>();
            CustomWorldMod.activatedPacks = new Dictionary<string, string>();
            CustomWorldMod.downloadedThumbnails = new Dictionary<string, byte[]>();
            CustomWorldMod.levelUnlocks = new Dictionary<string, string>();
        }

        /// <summary>
        /// Fills the field regionInfo save slot which stores information about installed regions in each save.
        /// </summary>
        public static void AnalyzeSave()
        {
            saveProblems = new SaveProblems[3];
            // foreach(List<RegionInformation> infoSave in regionInfoInSaveSlot)
            if (packInfoInSaveSlot == null)
            {
                return;
            }

            for (int saveSlot = 0; saveSlot < packInfoInSaveSlot.Length; saveSlot++)
            {
                try
                {
                    saveProblems[saveSlot] = new SaveProblems(false, false, new List<string>(), new List<string>(), new List<string>());

                    if (packInfoInSaveSlot[saveSlot] == null)
                    {
                        CustomWorldMod.Log($"No installed info for save slot [{saveSlot}]");
                        continue;
                    }

                    List<string> savedPacks = new List<string>();
                    foreach (RegionPack info in packInfoInSaveSlot[saveSlot])
                    {
                        savedPacks.Add(info.name);
                        string savedsum = info.checksum;

                        if (installedPacks.TryGetValue(info.name, out RegionPack installedPack))
                        {
                            if (!savedsum.Equals(installedPacks[info.name].checksum))
                            {
                                saveProblems[saveSlot].checkSum.Add(info.name);
                            }

                            if (info.loadNumber != installedPacks[info.name].loadNumber)
                            {
                                saveProblems[saveSlot].loadOrder = true;
                            }
                        }
                    }

                    // Compare installed regions
                    saveProblems[saveSlot].missingRegions = savedPacks.Except(activatedPacks.Keys).ToList();
                    saveProblems[saveSlot].extraRegions = activatedPacks.Keys.Except(savedPacks).ToList();


                    if (savedPacks.Count != activatedPacks.Count ||
                        (saveProblems[saveSlot].missingRegions != null && saveProblems[saveSlot].missingRegions.Count != 0) ||
                        (saveProblems[saveSlot].extraRegions != null && saveProblems[saveSlot].extraRegions.Count != 0))
                    {
                        saveProblems[saveSlot].installedRegions = true;
                    }

                }
                catch (Exception e)
                {
                    CustomWorldMod.Log("SaveSlot: " + saveSlot + "\n" + e, true);
                }
            }
        }

        /// <summary>
        /// Loads the installed regions info from the save files
        /// </summary>
        public static void ReadSaveAnalyzerFiles()
        {
            packInfoInSaveSlot = new List<RegionPack>[3];
            for (int saveSlot = 0; saveSlot < 3; saveSlot++)
            {

                string CRSsaveFilePath = Custom.RootFolderDirectory() + CustomWorldMod.regionSavePath + $"CRsav_{saveSlot + 1}.txt";
                string vanilaFilePath = Custom.RootFolderDirectory() + 
                    "UserData" + Path.DirectorySeparatorChar + ((saveSlot != 0) ? ("sav_" + (saveSlot + 1)) : "sav") + ".txt";

                if (!File.Exists(vanilaFilePath))
                {
                    File.Delete(CRSsaveFilePath);
                    Log($"Deleting {CRSsaveFilePath} since vanilla save is empty");
                    return;
                }

                if (File.Exists(CRSsaveFilePath))
                {
                    packInfoInSaveSlot[saveSlot] = new List<RegionPack>();
                    string allText = File.ReadAllText(CRSsaveFilePath);
                    string sum = allText.Substring(0, 32);
                    allText = allText.Substring(32, allText.Length - 32);

                    if (Custom.Md5Sum(allText).Equals(sum))
                    {
                        Log($"SaveSlot [{saveSlot + 1}] found! Correct checksum");
                    }
                    else
                    {
                        File.Delete(CRSsaveFilePath);
                        Log("CR save was tinkered. Why did you touch this?", true);
                        throw new System.NullReferenceException("Deleting CR save and forcing restart");
                    }


                    string[] splitText = Regex.Split(allText, saveDividerA);
                    try
                    {
                        for (int j = 0; j < splitText.Length; j++)
                        {
                            if (!splitText[j].Equals(string.Empty))
                            {
                                List<string> minedLines = new List<string>();
                                string[] splitRegion = Regex.Split(splitText[j], saveDividerB);
                                string loadedRegion = string.Empty;
                                for (int k = 0; k < splitRegion.Length; k++)
                                {
                                    if (!splitRegion[k].Equals(string.Empty))
                                    {
                                        //loadedRegion += splitRegion[k];
                                        //Debug.Log(splitRegion[k]);
                                        minedLines.Add(splitRegion[k]);
                                    }
                                }

                                string name = Regex.Split(minedLines.Find(x => x.Contains("REGID")), "<REGID>")[1];
                                string checkSum = Regex.Split(minedLines.Find(x => x.Contains("SUM")), "<SUM>")[1];
                                int packNumber = int.Parse(Regex.Split(minedLines.Find(x => x.Contains("ORDER")), "<ORDER>")[1]);

                                packInfoInSaveSlot[saveSlot].Add(new RegionPack(name, checkSum, packNumber));


                                //(regionID, null, null, true, -20, checkSum, regionNumber, null, null, null, null));

                            }
                        }
                    }
                    catch (Exception e) { Log($"Error when reading CR save [{e}]", true); }


                    // DEBUG
                    /*
                    string regionsID = "";
                    foreach(RegionInformation info in regionInfoInSaveSlot[saveSlot])
                    {
                        regionsID += info.regionID + " ";
                    }
                    CustomWorldLog($"Save slot[{saveSlot}] is using [{regionsID}]");
                    */

                }
                else
                {
                    Log($"SaveSlot [{saveSlot + 1}] does not have CR information");
                }
            }
        }



        /// <summary>
        /// Providing an array with vanilla region IDs, returns this array but with the new regionsID added from the CustomWorldMod.lodadedRegions dictionary.
        /// </summary>
        /// <returns>returns string[] regionsID</returns>
        public static string[] AddModdedRegions(string[] regionNames)
        {
            foreach (KeyValuePair<string, string> regionPack in CustomWorldMod.activatedPacks)
            {
                //CustomWorldMod.CustomWorldLog($"Custom Regions: PlayerProgression, loading new regions");

                foreach (string customRegion in CustomWorldMod.installedPacks[regionPack.Key].regions)
                {
                    string regionToAdd = customRegion;

                    //if (shouldAdd)
                    if (!regionNames.Contains(regionToAdd))
                    {
                        Array.Resize(ref regionNames, regionNames.Length + 1);
                        regionNames[regionNames.Length - 1] = regionToAdd;
                        CustomWorldMod.Log($"Custom Regions: Added new region to regionNames [{regionToAdd}] from [{regionPack.Key}].");
                    }
                }
            }

            return regionNames;
        }


        /// <summary>
        /// Returns a List from WorldData
        /// </summary>
        public static List<string> FromWorldDataToListString(List<WorldDataLine> worldData)
        {
            List<string> updatedList = new List<string>();
            foreach (WorldDataLine data in worldData)
            {
                updatedList.Add(data.line);
            }

            return updatedList;
        }

        /// <summary>
        /// Builds available regions and manages json files
        /// </summary>
        public static void LoadInstalledPacks()
        {
            CustomWorldMod.customPearls = new Dictionary<int, CustomPearl>();

            Dictionary<string, RegionPack> unsortedPacks = new Dictionary<string, RegionPack>();
            string[] vanillaRegions = VanillaRegions();
            string path = Custom.RootFolderDirectory() + CustomWorldMod.resourcePath;

            // For each Region Mod Installed
            foreach (string dir in Directory.GetDirectories(path))
            {
                Log($"\n#Loading [{dir}]");

                // For upgrading to regionpack
                string pathOfRegionInfo = dir + Path.DirectorySeparatorChar + "regionInfo.json";

                // JSON with all information
                string pathOfPackInfo = dir + Path.DirectorySeparatorChar + "packInfo.json";

                // Region Pack
                RegionPack pack = new RegionPack(new DirectoryInfo(dir).Name);

                string[] insideDirectories = Directory.GetDirectories(dir);
                string[] expectedDirectories = { "Assets", "World", "Levels", "SoundEffects", "PackDependencies" };


                foreach (string directory in insideDirectories)
                {
                    string directoryName = new DirectoryInfo(directory).Name;
                    if (!expectedDirectories.Contains(directoryName))
                    {
                        if (!(directoryName.ToLower().Contains("patch") || directoryName.ToLower().Contains("patches")))
                        {
                            CustomWorldMod.Log($"[{pack.folderName}] is incorrectly installed. " +
                                $"Inside this folder ({dir}) there should be one or more of the following folder: " +
                                $"[{string.Join(", ", expectedDirectories)}] You currently have: {directoryName}", true);
                        }
                    }
                }


                // Creating pack info ...
                if (!File.Exists(pathOfPackInfo))
                {
                    Log($"Pack [{pack.name}] does not have packInfo.json");

                    // Region Name
                    pack.name = new DirectoryInfo(dir).Name;

                    // Upgrade to packInfo.json
                    if (File.Exists(pathOfRegionInfo))
                    {
                        Log("Upgrading to packInfo.json");
                        Dictionary<string, object> regionInfoDictionary = null;
                        try
                        {
                            regionInfoDictionary = File.ReadAllText(pathOfRegionInfo).dictionaryFromJson();
                        }
                        catch (Exception)
                        {
                            Log($"Corrupted regionInfo.json ... deleting", true);
                            File.Delete(pathOfRegionInfo);
                        }
                        if (regionInfoDictionary != null)
                        {
                            UpgradeToRegionPack(regionInfoDictionary, ref pack, dir);
                            File.Delete(pathOfRegionInfo);
                        }
                    }
                    // Upgrading from ancient CR version
                    else if (File.Exists(dir + Path.DirectorySeparatorChar + "regionID.txt"))
                    {
                        string regionID = File.ReadAllText(dir + Path.DirectorySeparatorChar + "regionID.txt");
                        regionID = regionID.ToUpper();

                        File.Delete(dir + Path.DirectorySeparatorChar + "regionID.txt");
                        CustomWorldMod.Log($"Custom Regions: Updating regionID from old CR version... Obtained regionID [{regionID}]");
                    }
                    else
                    {
                        pack.activated = true;
                        pack.checksum = CustomWorldMod.GeneratePackCheckSum(dir);
                    }

                    SerializePackInfoJSON(pathOfPackInfo, pack);
                }

                Dictionary<string, object> dictionary = null;
                try
                {
                    dictionary = File.ReadAllText(pathOfPackInfo).dictionaryFromJson();
                    FromDictionaryToPackInfo(dictionary, ref pack);
                }
                catch (Exception e)
                {
                    Log($"CORRUPTED JSON FILE -- DELETING [{pathOfPackInfo}] - [{e}]", true);
                    File.Delete(pathOfPackInfo);
                    SerializePackInfoJSON(pathOfPackInfo, pack);
                }

                // Pack name could not be found, using folder name
                if (pack.name.Equals(string.Empty))
                {
                    pack.name = new DirectoryInfo(dir).Name;
                }
                // No regions found, retreiving
                bool noRegions = pack.regions.Count == 0;
                if (pack.regions.Count == 0)
                {
                    string regionsFile = dir + Path.DirectorySeparatorChar + "World" + Path.DirectorySeparatorChar + 
                        "Regions" + Path.DirectorySeparatorChar + "regions.txt";

                    if (File.Exists(regionsFile))
                    {
                        foreach (string dirRegions in File.ReadAllLines(regionsFile))
                        {
                            if (!vanillaRegions.Contains(dirRegions))
                            {
                                // New custom region
                                pack.regions.Add(dirRegions);
                            }
                        }
                    }
                }
                // Added new regions
                if (pack.regions.Count != 0)
                {
                    Log($"Included regions [{string.Join(", ", pack.regions.ToArray())}]");
                }
                else
                {
                    // Nothing Added
                    Log($"[{pack.name}] does not add any new regions");
                }

                Log($"Description for [{pack.name}] is: [{pack.description.Substring(0, Mathf.Min(15, pack.description.Length))}...]");

                if (pack.description.Equals("N / A") || pack.description.Equals(string.Empty))
                {
                    pack.description = "No description";
                }


                // Load region pack 
                if (pack.name != string.Empty)
                {
                    CustomWorldMod.Log($"Adding available region pack [{pack.name}]. Activated [{pack.activated}]. Folder name [{pack.folderName}]");
                    try
                    {
                        unsortedPacks.Add(pack.name, pack);
                    }
                    catch (Exception dicErr) { CustomWorldMod.Log($"Custom Regions: Error in adding [{pack.name}] => {dicErr}"); };
                }
                else
                {
                    Log($"Pack name ({pack.name}) or folder ({pack.folderName}) was empty! Fatal Error", true);
                }

                if (!Directory.Exists(Custom.RootFolderDirectory() + CustomWorldMod.resourcePath + pack.folderName))
                {
                    CustomWorldMod.Log($"CR could not find folder [{pack.folderName}] from region [{pack.name}]. " +
                        $"Try removing any dots from the folder names and reloading.", true);
                }


                bool needSerialize = false;

                string newChecksum = GeneratePackCheckSum(dir);
                if (!pack.checksum.Equals(newChecksum))
                {
                    needSerialize = true;
                    Log($"Checksum: [{pack.name}] was modified, generating new checksum...");
                    pack.checksum = newChecksum;
                       
                }


                if (needSerialize)
                {
                    SerializePackInfoJSON(pathOfPackInfo, pack);
                }

                if (pack.activated)
                {
                    LoadCustomPearls(pack);
                    LoadElectricGates(pack);
                    LoadVariations(pack);
                    LoadArenaUnlocks(pack);
                }
                else
                {
                    Log("Won't load configuration / pearls / electric gates until it is enabled");
                }
                Log("-------");
            }

            foreach (KeyValuePair<string, RegionPack> pack in unsortedPacks.OrderBy(x => x.Value.loadOrder))
            {
                if (!pack.Key.Equals(""))
                {
                    CustomWorldMod.installedPacks.Add(pack.Key, pack.Value);
                }
                else
                {
                    Log($"Error loading region", true);
                }
            }

            Log($"Ordered installed region packs [{string.Join(", ", installedPacks.Keys.ToArray())}]");
        }


        public static void UpdateLocalPackWithOnline()
        {
            Dictionary<string, RegionPack> updatedInstalledPacks = null;
            foreach (KeyValuePair<string, RegionPack> entry in CustomWorldMod.installedPacks)
            {
                RegionPack pack = entry.Value;
                string dir = Custom.RootFolderDirectory() + resourcePath + pack.folderName;
                string pathOfPackInfo = dir + Path.DirectorySeparatorChar + "packInfo.json";
                string newDescr = string.Empty;
                string newUrl = string.Empty;
                string newAuthor = string.Empty;

                if (CustomWorldMod.rainDbPacks.ContainsKey(pack.name))
                {
                    // Deactivate RainDB pack since it is already installed
                    RegionPack temp = CustomWorldMod.rainDbPacks[pack.name];
                    temp.activated = false;
                    CustomWorldMod.rainDbPacks[pack.name] = temp;

                    newDescr = CustomWorldMod.rainDbPacks[pack.name].description;
                    newUrl = CustomWorldMod.rainDbPacks[pack.name].thumbUrl;
                    newAuthor = CustomWorldMod.rainDbPacks[pack.name].author;
                }

                bool needsUpdate = false;
                if (!newDescr.Equals(string.Empty) && !newDescr.Equals(pack.description))
                {
                    Log($"New description for {pack.description.Substring(0, Mathf.Min(15, pack.description.Length))}... [{newDescr}]");
                    pack.description = newDescr;
                    needsUpdate = true;
                }
                if (!newUrl.Equals(string.Empty) && !newUrl.Equals(pack.thumbUrl))
                {
                    Log($"New url for {pack.name} [{newUrl}]");
                    pack.thumbUrl = newUrl;
                    needsUpdate = true;
                }
                if (!newAuthor.Equals(string.Empty) && !newAuthor.Equals(pack.author))
                {
                    Log($"New author for {pack.name} [{newAuthor}]");
                    pack.author = newAuthor;
                    needsUpdate = true;
                }

                // Write new info
                if (needsUpdate)
                {
                    if (updatedInstalledPacks == null)
                    {
                        updatedInstalledPacks = new Dictionary<string, RegionPack>();
                    }
                    updatedInstalledPacks.Add(entry.Key, pack);
                    File.Delete(pathOfPackInfo);
                    SerializePackInfoJSON(pathOfPackInfo, pack);
                }
            }
            if (updatedInstalledPacks != null)
            {
                foreach (KeyValuePair<string, RegionPack> entry in updatedInstalledPacks)
                {
                    CustomWorldMod.installedPacks[entry.Key] = entry.Value;
                    Log($"Updated information from RainDB for pack [{entry.Key}]");
                }
            }
        }

        // Cursed
        public static void FromDictionaryToPackInfo(Dictionary<string, object> json, ref RegionPack pack, bool authorative = false)
        {

            Dictionary<string, object> packDictionary = json;

            if (packDictionary.TryGetValue("regionPackName", out object value) && value != null)
            {
                pack.name = value.ToString();
            }
            if (packDictionary.TryGetValue("description", out value) && value != null)//(GetValueDictionary("description", packDictionary) != null)
            {
                pack.description = value.ToString();
            }
            if (packDictionary.TryGetValue("activated", out value) && value != null)
            {
                pack.activated = bool.Parse(value.ToString());//value.ToString().ToLower().Contains("true");
            }
            if (packDictionary.TryGetValue("author", out value) && value != null)
            {
                pack.author = value.ToString();
            }
            if (packDictionary.TryGetValue("checksum", out value) && value != null)
            {
                pack.checksum = value.ToString();
            }
            if (packDictionary.TryGetValue("thumbURL", out value) && value != null)
            {
                pack.thumbUrl = value.ToString();
            }
            if (packDictionary.TryGetValue("loadOrder", out value) && value != null)
            {
                pack.loadOrder = int.Parse(value.ToString());
            }
            if (packDictionary.TryGetValue("version", out value) && value != null)
            {
                pack.version = value.ToString();
            }
            if (packDictionary.TryGetValue("packURL", out value) && value != null)
            {
                pack.packUrl = value.ToString();
            }
            if (packDictionary.TryGetValue("requirements", out value) && value != null)
            {
                pack.requirements = value.ToString();
            }
            if (packDictionary.TryGetValue("useRegionName", out value) && value != null)
            {
                pack.useRegionName = bool.Parse(value.ToString());
            }
            if (packDictionary.TryGetValue("regions", out value) && value != null)
            {
                string regions = value.ToString();
                string[] array = Regex.Split(regions, ",");
                foreach (string ID in array)
                {
                    if (!ID.Equals(string.Empty))
                    {
                        if (!pack.regions.Contains(ID.Replace(" ", "")))
                        {
                            pack.regions.Add(ID.Replace(" ", ""));
                        }
                        else
                        {
                            Log($"Duplicated region loaded from packInfo.json [{pack.name}]", true);
                        }
                    }
                }
            }
            // Only works when loading from the web
            if (authorative)
            {
                if (packDictionary.TryGetValue("expansion", out value) && value != null)
                {
                    pack.expansion = bool.Parse(value.ToString());
                }
                if (packDictionary.TryGetValue("shownInBrowser", out value) && value != null)
                {
                    pack.shownInBrowser = bool.Parse(value.ToString());
                }
            }

        }


        public static void FromDictionaryToRegionConfig(Dictionary<string, object> dictionary, ref RegionConfiguration regionConfiguration)
        {
            if (dictionary.TryGetValue("albino_leviathan", out object value) && value != null)
            {
                bool.TryParse(value.ToString(), out regionConfiguration.albinoLevi);
            }

            if (dictionary.TryGetValue("albino_jetfish", out value) && value != null)
            {
                bool.TryParse(value.ToString(), out regionConfiguration.albinoJet);
            }

            if (dictionary.TryGetValue("monster_kelp_color", out value) && value != null)
            {
                try
                {
                    string color = value.ToString();
                    if (color.Contains("#"))
                    {
                        color = color.Replace("#", "");
                        CustomWorldMod.Log($"Removed # from color [{color}]");
                    }
                    regionConfiguration.kelpColor = OptionalUI.OpColorPicker.HexToColor(color);
                }
                catch (Exception) { regionConfiguration.kelpColor = null; }
            }

            if (dictionary.TryGetValue("brother_color", out value) && value != null)
            {
                try
                {
                    string color = value.ToString();
                    if (color.Contains("#"))
                    {
                        color = color.Replace("#", "");
                        CustomWorldMod.Log($"Removed # from color [{color}]");
                    }
                    regionConfiguration.bllColor = OptionalUI.OpColorPicker.HexToColor(color);
                }
                catch (Exception) { regionConfiguration.bllColor = null; }
            }

            if (dictionary.TryGetValue("black_salamander_chance", out value) && value != null)
            {
                float.TryParse(value.ToString(), out regionConfiguration.blackSalamanderChance);
            }

            if (dictionary.TryGetValue("batfly_color", out value) && value != null)
            {
                try
                {
                    string color = value.ToString();
                    if (color.Contains("#"))
                    {
                        color = color.Replace("#", "");
                        CustomWorldMod.Log($"Removed # from color [{color}]");
                    }
                    regionConfiguration.batFlyColor = OptionalUI.OpColorPicker.HexToColor(color);
                }
                catch (Exception) { regionConfiguration.batFlyColor = null; }
            }

            if (dictionary.TryGetValue("scave_trade_item", out value) && value != null)
            {
                try
                {
                    string item = value.ToString();
                    regionConfiguration.scavTradeItem = item;
                }
                catch (Exception) { regionConfiguration.scavTradeItem = null; }
            }
        }

        public static void LoadElectricGates(RegionPack regionPack)
        {
            Log($"Loading electric gates...", false, DebugLevel.MEDIUM);
            // Add electric gates
            string pathToElectricGates = CRExtras.BuildPath(regionPack.folderName, CRExtras.CustomFolder.Gates, file: "electricGates.txt");
            if (File.Exists(pathToElectricGates))
            {
                string[] electricGates = File.ReadAllLines(pathToElectricGates);
                for (int i = 0; i < electricGates.Length; i++)
                {
                    string gateName = Regex.Split(electricGates[i], " : ")[0];
                    float meterHeigh = float.Parse(Regex.Split(electricGates[i], " : ")[1]);

                    Log($"Added new gate electric gate [{gateName}] from [{regionPack.name}]. Meter height [{meterHeigh}]");
                    regionPack.electricGates.Add(gateName, meterHeigh);
                }
            }

        }

        public static void LoadCustomPearls(RegionPack regionPack)
        {
            Log($"Loading custom pearls...", false, DebugLevel.MEDIUM);
            // Add Custom Pearls
            string pathToPearls = CRExtras.BuildPath(regionPack.folderName, CRExtras.CustomFolder.Assets, file: "pearlData.txt");
            if (File.Exists(pathToPearls))
            {
                Log($"Loading pearl data for {regionPack.name}");
                string[] customPearlsLines = File.ReadAllLines(pathToPearls);
                string[] newLines = customPearlsLines;
                for (int i = 0; i < customPearlsLines.Length; i++)
                {
                    int hash = 0;
                    if (customPearlsLines[i].Equals(string.Empty))
                    {
                        // Line empty, skip
                        continue;
                    }

                    string[] lineDivided = Regex.Split(customPearlsLines[i], " : ");
                    int fileNumber = 0; string pearlName = ""; Color pearlColor = new Color(0.7f, 0.7f, 0.7f); Color? secondaryColor = new Color();

                    try
                    {
                        fileNumber = int.Parse(lineDivided[0]);
                        pearlName = $"{regionPack.name.Replace(" ", "_")}_{lineDivided[1]}";
                    }
                    catch (Exception e)
                    {
                        Log("Error loading pearl information, missing pearl ID or pearl Name in pearlData.txt" + e, true);
                        continue;
                    }

                    try { pearlColor = OptionalUI.OpColorPicker.HexToColor(lineDivided[2]); }
                    catch (Exception e) { Log($"Pearl missing color from {regionPack.name} {e}", true); }

                    try { secondaryColor = OptionalUI.OpColorPicker.HexToColor(lineDivided[3]); }
                    catch (Exception e) { Log($"Pearl missing highlighted color from {regionPack.name} {e}"); }

                    try
                    {
                        if (lineDivided.Length < 5)
                        {
                            newLines[i] += $" : {hash = pearlName.GetHashCode()}";
                        }
                        else
                        {
                            hash = int.Parse(lineDivided[4]);
                        }
                    }
                    catch (Exception e) { Log($"Error loading hash {regionPack.name} {e}", true); }

                    CustomWorldMod.Log($"Added new pearl [{pearlName} / {fileNumber} / {pearlColor}]");
                    try
                    {
                        CustomWorldMod.customPearls.Add(hash, new CustomPearl(pearlName, fileNumber, pearlColor, secondaryColor, regionPack.name));
                    } catch (Exception e)
                    {
                        CustomWorldMod.Log($"Could not add pearl [{pearlName}] from [{regionPack.name}]. Make sure the hash is not duplicated. " +
                            $"You can try deleting the last long number after the color in [{pathToPearls}] and relaunch the game. \n {e}", true);
                    }

                    // Extend PearlTypeEnum
                    EnumExtender.AddDeclaration(typeof(DataPearl.AbstractDataPearl.DataPearlType), pearlName);

                    // Extend ConvoID
                    EnumExtender.AddDeclaration(typeof(Conversation.ID), "Moon_" + pearlName);
                }
                int lastPearl = Enum.GetNames(typeof(DataPearl.AbstractDataPearl.DataPearlType)).Length;
                int lastConvo = Enum.GetNames(typeof(Conversation.ID)).Length;

                try { EnumExtender.ExtendEnumsAgain(); }
                catch (Exception e) { Log($"Error extending pearl enum [{e}]", true); }

                string[] names = Enum.GetNames(typeof(DataPearl.AbstractDataPearl.DataPearlType));
                names = names.Skip(lastPearl).ToArray();
                List<string> debug = new List<string>(names);
                Log($"Extending DataPearlType enum ... [{string.Join(", ", debug.ToArray())}]");

                string[] names2 = Enum.GetNames(typeof(Conversation.ID));
                names2 = names2.Skip(lastConvo).ToArray();
                List<string> debug2 = new List<string>(names2);
                Log($"Extending ConversationID enum ... [{string.Join(", ", debug2.ToArray())}]");

                try { File.WriteAllLines(pathToPearls, newLines); }
                catch (Exception e) { Log($"Error creating pearl hash [{e}]", true); }
            }
            string directory = CRExtras.BuildPath(regionPack.name, CRExtras.CustomFolder.Text);
            if (Directory.Exists(directory))
            {
                // Encrypt text files
                Log($"Creating conversation files for {regionPack.name}...");
                EncryptCustomDialogue(directory, regionPack.name);
            }
        }

        public static void EncryptCustomDialogue(string dir, string regionPack)
        {
            for (int j = 0; j < Enum.GetNames(typeof(InGameTranslator.LanguageID)).Length; j++)
            {
                string pathToConvoDir = Path.Combine(dir, "Text_" + LocalizationTranslator.LangShort((InGameTranslator.LanguageID)j)+Path.DirectorySeparatorChar);

                if (Directory.Exists(pathToConvoDir))
                {
                    foreach (string pathToConvo in Directory.GetFiles(pathToConvoDir))
                    {
                        if (!int.TryParse(Path.GetFileNameWithoutExtension(pathToConvo), out int k))
                        {
                            Log($"Fatal error encrypting conversation files for [{regionPack}] " +
                                $"[Error parsing filename {Path.GetFileNameWithoutExtension(pathToConvo)}]", true);
                            return;
                        }

                        string convoLines = File.ReadAllText(pathToConvo, Encoding.Default);
                        //Log($"Conversation file: [{convoLines}]");
                        if (convoLines[0] == '0')
                        {
                            convoLines = Regex.Replace(convoLines, @"\r\n|\r|\n", "\r\n");
                            string[] lines = Regex.Split(convoLines, Environment.NewLine);
                            Log($"Encrypting file [{Path.GetFileNameWithoutExtension(pathToConvo)}.txt] from [{regionPack}]. " +
                                $"Number of lines [{lines.Length}]");

                            if (lines.Length > 1)
                            {
                                string text4 = Custom.xorEncrypt(convoLines, 54 + k + j * 7);
                                text4 = '1' + text4.Remove(0, 1);
                                File.WriteAllText(pathToConvo, text4);
                            }
                            else
                            {
                                Log($"Failed encrypting. No newLine character found while encrypting. " +
                                    $"Try removing all new lines and pressing enter to separate them.", true);
                            }
                        }
                        
                        else
                        {
                            Log($"Convo already encrypted: [{k}]", false, DebugLevel.FULL);
                        }
                        

                    }
                }
            }
        }

        public static void LoadVariations(RegionPack packInfo)
        {
            Log($"Loading custom colors...", false, DebugLevel.MEDIUM);
            // Add custom colors
            string pathToRegionsDir = CRExtras.BuildPath(packInfo.folderName, CRExtras.CustomFolder.Regions);
            if (!Directory.Exists(pathToRegionsDir))
            {
                Log($"Region [{packInfo.name}] doesn't have Regions folder");
                return;
            }
            foreach (string regionDir in Directory.GetDirectories(pathToRegionsDir))
            {
                string pathConfig = regionDir + Path.DirectorySeparatorChar + "CustomConfig.json";
                // Load configuration
                if (File.Exists(pathConfig))
                {
                    Log($"Loading variation config for region [{new DirectoryInfo(regionDir).Name}] from [{packInfo.name}]");
                    RegionConfiguration regionConfiguration = new RegionConfiguration(null, false, false, false, null, false, null, -1, null, false, null);

                    Dictionary<string, object> dictionary = null;
                    try
                    {
                        dictionary = File.ReadAllText(pathConfig).dictionaryFromJson();
                    }
                    catch (Exception e)
                    {
                        Log($"CORRUPTED JSON FILE [{pathConfig}] - [{e}]", true);
                        //File.Delete(pathConfig);
                        return;
                    }

                    //List<string> jsonFields = new List<string>(){ "regionID", "description", "regionName", "activated","loadOrder", "checksum", "url"};
                    if (dictionary != null)
                    {

                        FromDictionaryToRegionConfig(dictionary, ref regionConfiguration);
                        try
                        {
                            regionConfiguration.kelpVanilla = regionConfiguration.kelpColor == null;
                            regionConfiguration.bllVanilla = regionConfiguration.bllColor == null;
                            regionConfiguration.batVanilla = regionConfiguration.batFlyColor == null;
                        }
                        catch (Exception e) { Log($"Exception loading variation [{e}]", true); }
                        regionConfiguration.regionID = new DirectoryInfo(regionDir).Name;

                        // Load region information
                        CustomWorldMod.Log($"Adding configuration for region [{regionConfiguration.regionID}] from [{packInfo.name}]");
                        
                        if (packInfo.name != string.Empty)
                        {
                            try
                            {
                                packInfo.regionConfig.Add(regionConfiguration.regionID, regionConfiguration);
                            }
                            catch (Exception dic) { 
                                CustomWorldMod.Log($"Custom Regions: Error in adding config [{regionConfiguration.regionID}] => {dic}"); 
                            };
                        }

                    }
                }
            }
        }

        private static void LoadArenaUnlocks(RegionPack pack)
        {
            Log($"Loading arena unlocks...", false, DebugLevel.MEDIUM);
            
            string pathToRegionsDir = CRExtras.BuildPath(pack.folderName, CRExtras.CustomFolder.Levels);
            if (!Directory.Exists(pathToRegionsDir))
            {
                Log($"Pack [{pack.name}] doesn't have Levels folder");
                return;
            }

            string pathConfig = pathToRegionsDir + Path.DirectorySeparatorChar + "customUnlocks.txt";
            // Load configuration
            if (File.Exists(pathConfig))
            {
                Log($"Loading arena unlocks for pack [{pack.name}]");
                string[] customPearlsLines = File.ReadAllLines(pathConfig);
                for (int i = 0; i < customPearlsLines.Length; i++)
                {
                    if (customPearlsLines[i].Equals(string.Empty))
                    {
                        // Line empty, skip
                        continue;
                    }
                    string[] lineDivided = Regex.Split(customPearlsLines[i], " : ");
                    string unlockID = "";
                    string[] levelNames;

                    try
                    {
                        unlockID = lineDivided[0];
                    }
                    catch (Exception e)
                    {
                        Log("Error loading levelUnlock ID" + e, true);
                        continue;
                    }

                    try
                    {
                        levelNames = Regex.Split(lineDivided[1].Replace(" ", ""), ",");
                    }
                    catch (Exception e)
                    {
                        Log("Error loading levelUnlock name" + e, true);
                        continue;
                    }

                    for (int j = 0; j < levelNames.Length; j++)
                    {
                        if (levelNames[j].Equals(string.Empty))
                        {
                            continue;
                        }
                        if (CustomWorldMod.levelUnlocks == null)
                        {
                            CustomWorldMod.levelUnlocks = new Dictionary<string, string>();
                        }
                        try
                        {
                            if (!CustomWorldMod.levelUnlocks.ContainsKey(levelNames[j]))
                            {
                                CustomWorldMod.levelUnlocks.Add($"{levelNames[j]}", unlockID);
                                CustomWorldMod.Log($"Added new level unlock: [{levelNames[j]}-{unlockID}]");
                            }
                            else
                            {
                                CustomWorldMod.Log($"Duplicated arena name from two packs! [{levelNames[j]}]", true);
                            }
                        }
                        catch (Exception e)
                        {
                            CustomWorldMod.Log($"Error adding level unlock ID [{levelNames[j]}] [{e}]", true);
                        }
                    }

                }
            }
            else
            {
                Log($"Pack [{pack.name}] doesn't have customUnlocks.txt");
            }

        }

        private static string GeneratePackCheckSum(string path)
        {
            path += Path.DirectorySeparatorChar + "World" + Path.DirectorySeparatorChar;
            string checkSum = string.Empty;

            //checkSum += $"<gatesLock>{Custom.Md5Sum(ReadFileCheckSum(path + "Gates" + Path.DirectorySeparatorChar + "locks.txt"))}</gatesLock>";

            string regionsPath = path + "Regions";
            if (Directory.Exists(regionsPath))
            {
                // Try to get regionID
                foreach (string regionsDir in Directory.GetDirectories(regionsPath))
                {
                    checkSum += ReadFileCheckSum(regionsDir + Path.DirectorySeparatorChar + "Properties.txt");
                    checkSum += ReadFileCheckSum(regionsDir + Path.DirectorySeparatorChar + "world_" + Path.GetFileNameWithoutExtension(regionsDir) + ".txt");
                }
            }

            return Custom.Md5Sum(checkSum);
        }

        private static string ReadFileCheckSum(string path)
        {
            if (!File.Exists(path))
            {
                //Log($"[{Path.GetFileNameWithoutExtension(path)}] text file doesn't exist: [{path}]");
                return string.Empty;
            }
            return File.ReadAllText(path);
        }

        public static string GetSaveInformation()
        {
            string dictionaryString = "Custom Regions: New save, Custom Regions Information \n";
            dictionaryString += "<progCRdivA>";
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
            {
                dictionaryString += $"<progCRdivB>{keyValues.Key}" +
                    $"<progCRdivB>{CustomWorldMod.installedPacks[keyValues.Key].loadNumber}" +
                    $"<progCRdivB>{CustomWorldMod.installedPacks[keyValues.Key].checksum}";
            }
            dictionaryString += "<progCRdivA>";
            dictionaryString = dictionaryString.TrimEnd(',', ' ') + "";

            return dictionaryString;
        }


        public static string SerializeRegionInfo(RegionPack packInfo)
        {

            string infoSerial = string.Empty;

            // Start Region ID
            infoSerial += $"{saveDividerA}<REGID>{packInfo.name}";

            /*------ CHECKSUM --------*/
            infoSerial += $"{saveDividerB}<SUM>{packInfo.checksum}{saveDividerB}";
            /*------ CHECKSUM --------*/

            /*------ ORDER --------*/
            infoSerial += $"{saveDividerB}<ORDER>{packInfo.loadNumber}{saveDividerB}";
            /*------ ORDER --------*/

            // End Region ID
            infoSerial += $"{saveDividerA}";

            return infoSerial;
        }

        public static void SerializePackInfoJSON(string dir, RegionPack pack)
        {
            CustomWorldMod.Log($"Serializing packInfo json [{pack.name}]");
            using (StreamWriter sw = File.CreateText(dir))
            {
                string json = "{\n" +
                     "   \"regionPackName\": \"" + pack.name + "\", \n" +
                     "   \"description\": \"" + pack.description + "\", \n" +
                     "   \"author\": \"" + pack.author + "\", \n" +
                     "   \"activated\": " + pack.activated.ToString().ToLower() + ", \n" +
                     "   \"loadOrder\": " + pack.loadOrder + ", \n" +
                     "   \"regions\": \"" + string.Join(", ", pack.regions.ToArray()) + "\", \n" +
                     "   \"thumbURL\": \"" + pack.thumbUrl + "\", \n" +
                     "   \"version\": \"" + pack.version + "\", \n" +
                     "   \"requirements\": \"" + pack.requirements + "\", \n" +
                     "   \"useRegionName\": " + pack.useRegionName.ToString().ToLower() + ", \n";

                json += "   \"checksum\": \"" + pack.checksum + "\" \n" + "}";
                sw.WriteLine(json);
            }
        }

        // Unused
        public static void WriteRegionConfigJSONFile(string dirPath, bool leviAlbino, bool jetfishAlbino,
            string shortcutColor, string kelpColor, string bllColor, int blackSalChance)
        {
            using (StreamWriter sw = File.CreateText(dirPath + Path.DirectorySeparatorChar + "regionConfiguration.json"))
            {
                sw.WriteLine(
                    "{\n"
                    + $"   \"albino_leviathan\":  \"{leviAlbino.ToString().ToLower()}\", \n"
                    + $"   \"albino_jetfish\":  \"{jetfishAlbino.ToString().ToLower()}\", \n"
                    + $"   \"shortcut_color\":  \"{shortcutColor}\", \n"
                    + $"   \"monster_kelp_color\":  \"{kelpColor}\", \n"
                    + $"   \"black_salamander_chance\":  \"{blackSalChance}\", \n"
                    + $"   \"brother_color\":  \"{bllColor}\", \n"
                );
            }
        }


        /// <summary>
        /// Upgrades from old regionInfo.json to packInfo.json
        /// </summary>
        /// <param name="regionInfoDictionary"></param>
        /// <param name="pack"></param>
        /// <param name="dir"></param>
        public static void UpgradeToRegionPack(Dictionary<string, object> regionInfoDictionary, ref RegionPack pack, string dir)
        {
            List<string> obtainedInfo = new List<string>();

            if (regionInfoDictionary.TryGetValue("regionName", out object value) && value != null)
            {
                pack.name = value.ToString();
                obtainedInfo.Add($"PackName: {pack.name}");
            }
            if (regionInfoDictionary.TryGetValue("regionID", out value) && value != null)
            {
                string pathToWorldXX = dir + Path.DirectorySeparatorChar +
                    "World" + Path.DirectorySeparatorChar + "Regions" +
                    Path.DirectorySeparatorChar + value.ToString() + Path.DirectorySeparatorChar + "world_" + value.ToString() + ".txt";

                if (File.Exists(pathToWorldXX))
                {
                    pack.regions.Add(value.ToString());
                    //Log($"Trying to upgrade to regionPack.json, pack [{pack.name}] Adding custom region [{value.ToString()}]");
                    obtainedInfo.Add($"CustomRegion(s): {value.ToString()}");
                }
                else
                {
                    Log($"Trying to upgrade to regionPack.json, pack [{pack.name}] does not have custom region. Path [{pathToWorldXX}]");
                }
            }
            if (regionInfoDictionary.TryGetValue("loadOrder", out value) && value != null)
            {
                pack.loadOrder = int.Parse(value.ToString());
                obtainedInfo.Add($"LoadOrder: {pack.loadOrder}");
            }
            if (regionInfoDictionary.TryGetValue("description", out value) && value != null)
            {
                pack.description = value.ToString();
                obtainedInfo.Add($"Description: {pack.description.Substring(0, Mathf.Min(pack.description.Length, 15))}...");
            }
            if (regionInfoDictionary.TryGetValue("activated", out value) && value != null)
            {
                pack.activated = bool.Parse(value.ToString());
                obtainedInfo.Add($"Activated: {pack.activated}");
            }
            if (regionInfoDictionary.TryGetValue("checksum", out value) && value != null)
            {
                pack.checksum = value.ToString();
                obtainedInfo.Add($"Checksum: {pack.checksum}");
            }
            if (regionInfoDictionary.TryGetValue("url", out value) && value != null)
            {
                pack.thumbUrl = value.ToString();
                obtainedInfo.Add($"URL: {pack.thumbUrl}");
            }
            Log($"Upgrade to regionPack.json. Information parsed: [{string.Join(", ", obtainedInfo.ToArray())}]");
        }

        internal static void LoadThumbnails()
        {
            Dictionary<string, string> thumbInfo = new Dictionary<string, string>();
            Log($"Loading thumbnails. Installed regions [{string.Join(", ", CustomWorldMod.installedPacks.Keys.ToArray())}]");
            foreach (KeyValuePair<string, RegionPack> entry in CustomWorldMod.installedPacks)
            {
                string thumbPath = Custom.RootFolderDirectory() + CustomWorldMod.resourcePath + 
                    entry.Value.folderName + Path.DirectorySeparatorChar + "thumb.png";

                Log("Thumbnail path " + thumbPath, false, DebugLevel.FULL);
                if (File.Exists(thumbPath))
                {
                    Log($"Loading local thumb for [{entry.Value.name}] - {entry.Value.thumbUrl} (Folder [{thumbPath}])", false, DebugLevel.MEDIUM);
                    byte[] fileData;
                    fileData = File.ReadAllBytes(thumbPath);
                    downloadedThumbnails.Add(entry.Value.name, fileData);
                }
                else
                {
                    if (entry.Value.thumbUrl != string.Empty)
                    {
                        try
                        {
                            Log($"Queue thumb for [{entry.Value.name}] - {entry.Value.thumbUrl} (Folder [{thumbPath}])", false, DebugLevel.MEDIUM);
                            thumbInfo.Add(entry.Value.name, entry.Value.thumbUrl);
                        }
                        catch (Exception e) { Log($"Error queuing thumbs [{e}] [{entry.Value.thumbUrl}]", true); }
                    }
                    // No thumbnail to load, no url to grab
                    Log($"[{entry.Value.name}] does not have local thumbnail, nor URL to download");
                }
            }

            foreach (KeyValuePair<string, RegionPack> entry in CustomWorldMod.rainDbPacks)
            {
                if (entry.Value.thumbUrl != string.Empty && entry.Value.activated)
                {
                    try
                    {
                        Log($"Queue raindb thumb {entry.Value.name} - {entry.Value.thumbUrl}", false, DebugLevel.MEDIUM);
                        thumbInfo.Add(entry.Value.name, entry.Value.thumbUrl);
                    }
                    catch (Exception e) { Log($"Error queuing thumbs [{e}] [{entry.Value.thumbUrl}]", true); }
                }
            }

            // Create thumbnail downloader
            if (scripts.FindAll(x => x is ThumbnailDownloader).Count == 0 && !CustomWorldMod.OfflineMode)
            {
                scripts.Add(new ThumbnailDownloader(thumbInfo, ref downloadedThumbnails));
            }
        }


        internal static void FetchPackInfo()
        {
            if (scripts.FindAll(x => x is PackFetcher).Count == 0)
            {
                scripts.Add(new PackFetcher(packFetcherUrl));
            }
        }
    }
}
