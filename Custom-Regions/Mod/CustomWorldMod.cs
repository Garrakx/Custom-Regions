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
using System.Security.Cryptography;
using System.Collections.Specialized;

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
        public static CustomWorldConfig config;
        public static CustomWorldOption customWorldOption;
        public static CustomWorldMod mod;
        public static string versionCR = "";

        public CustomWorldMod()
        {
            ModID = "Custom Regions Mod";
            Version = "0.7." + version;
            author = "Garrakx";
            versionCR = $"v0.7.{version}";
        }

        // Code for AutoUpdate support

        // Update URL - don't touch!
        public string updateURL = "http://beestuff.pythonanywhere.com/audb/api/mods/3/0";
        public int version = 38;

        // Public key in base64 - don't touch!
        public string keyE = "AQAB";
        public string keyN = "13Mr+YOzb1iLnJvzkuP4NEZEWwOtWKWvWAN0HdsQ5SF2+RG7k8FbtmQut+2+69ideiJHDW66jWBcGGvfiQ0+5yLAUBpGSckC7V79yZgFQT39lvgU0ykAjonkA+ZTODFnehubyCkrrrzwno4boZghEZmDS2YsSyDJ6RLJyD2/WeCokcTj1vIHZhY9DzkooFtejz9yI/PCZtq8tfq2AzSiQPS+0xGQs3fnAkOGoV1WZ/inW5/rRyjD5HICr8t79UmcopfRK383YBrf2G96HeVYvY2vwSS/BW/m32rTLOZHr+XX7SIZshz7BLK6xEssy4qXjskvAUshqNudxtQnIkShGJuKWF1V2vvwqgY/IZiAbDXdBOUaSd09ldHBlTz9EfzBcgqffVRaUTzS71yGLISyrLriezozlK1YZW9vvijpbD0rmDaJ4aq9s6EzhdgVkTEuChtm/Fj9pgsswjvkbgHw1t9QZWqu4pweNd3IE/Lktst8HBKLiw1aRaffbZIhh1apbyjF8iflD8sNzbIHEfEvc35MEwIFqibJVnVxppBa15HpOxeXOzwuTjFaLSURRvbOEFPmpyd1Nm4nMzZZHHPjQXT7oYQAxjSCfqnLAdYsEnNo/2172jJGLfBWWGFTavqiCYqLhjtYkPfRgpcdw4FldgjX4w7RGMD/Ra5VXvmDMTE=";
        // ------------------------------------------------


        public override void OnEnable()
        {
            base.OnEnable();

            config = default;
            /*
            GameObject gameObject = new GameObject();
            script = gameObject.AddComponent<CustomWorldScript>();
            CustomWorldScript.mod = this;
            */

            CustomWorldMod.CreateCustomWorldLog();
            CustomWorldMod.CreateCustomWorldFolders();

            // Load from file
            CustomWorldMod.analyzingLog = string.Empty;

            MapHook.ApplyHook();
            RegionGateHook.ApplyHooks();
            RegionHook.ApplyHook();
            RoomSettingsHook.ApplyHook();
            WorldHook.ApplyHook();
            WorldLoaderHook.ApplyHooks();
            OverWorldHook.ApplyHooks();
            PlayerProgressionHook.ApplyHooks();

            // Pearl
            DataPearlHook.ApplyHooks();
            SLOracleBehaviorHasMarkHook.ApplyHooks();

            // Rain world instance
            RainWorldHook.ApplyHooks();

            // Custom Palette
            RoomCameraHook.ApplyHook();

            // Electric gate
            RoomHook.ApplyHooks();
            WaterGateHook.ApplyHooks();

            // Custom Decal
            CustomDecalHook.ApplyHook();

            // Scene
            FastTravelScreenHook.ApplyHooks();
            MainMenuHook.ApplyHooks();
            MenuSceneHook.ApplyHook();
            MenuIllustrationHook.ApplyHook();
            SlugcatSelectMenuHook.ApplyHooks();

            // DevInterface
            MapPageHook.ApplyHooks();
            MapRenderOutputHook.ApplyHooks();

            // Arena
            MultiplayerMenuHook.ApplyHook();
            ArenaCreatureSpawnerHook.ApplyHook();

            // WinState - achievement
            WinStateHook.ApplyHook();

            SaveStateHook.ApplyHook();

            // MusicPiece
            MusicPieceHook.ApplyHooks();
            ProceduralMusicInstructionsHook.ApplyHooks();

            BigEelHook.ApplyHooks();
            TentaclePlantGraphicsHook.ApplyHooks();
            DaddyLongLegsHook.ApplyHooks();
            LizardGraphicsHook.ApplyHooks();

            //script.Initialize();

        }

        public static CustomWorldOption LoadOI()
        {
            customWorldOption = new CustomWorldOption();
            return customWorldOption;
        }

        public struct CustomWorldConfig
        {

        }


        /// <summary>
        /// Holds the value of the sceneID in use.
        /// </summary>
        public static string sceneCustomID = string.Empty;


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
        public static Dictionary<string, RegionPack> installedRegionPacks;

        /// <summary>
        /// List containing activated custom region
        /// </summary>
        public static List<string> activeModdedRegions;

        // public static List<string>

        /// <summary>
        /// Dictionary with activated region packs, where the Key is the region pack name and the value is the folder.
        /// </summary>
        public static Dictionary<string, string> loadedRegionPacks;

        /// <summary>
        /// Dictionary with all installed regions, where the Key is the region ID and the value is a struct with its information.
        /// </summary>
        //public static Dictionary<string, RegionPack> availableRegions;


        /// <summary>
        /// Dictionary with all installed regions, where the Key is the region ID and the value is a struct with its configuration.
        /// </summary>
        //public static Dictionary<string, RegionConfiguration> configurationRegions;

        /// <summary>
        /// Dictionary with custom pearls lodaded from activated regions. Key is region and value is Pearl Information
        /// </summary>
        public static Dictionary<string, CustomPearl> customPearls;

        /// <summary>
        /// path of the CustomResources folder (Mods\CustomResources\)
        /// </summary>
        public static string resourcePath = "Mods" + Path.DirectorySeparatorChar + "CustomResources" + Path.DirectorySeparatorChar;

        /// <summary>
        /// path of the CustomResources save folder (UserData\CustomRegionSaveData\)
        /// </summary>
        public static string regionSavePath = "UserData" + Path.DirectorySeparatorChar + "CustomRegionSaveData" + Path.DirectorySeparatorChar;

        /// <summary>
        /// Divider A used for CR save
        /// </summary>
        public static string saveDividerA = "<CRdivA>";
        /// <summary>
        /// Divider B used for CR save
        /// </summary>
        public static string saveDividerB = "<CRdivB>";

        /// <summary>
        /// Rain world game instance
        /// </summary>
        internal static RainWorld rainWorldInstance = null;

        protected static int numberOfVanillaRegions = 11;

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
        /// Monobehaviour that downloads thumbnails
        /// </summary>
        public static ThumbnailDownloader thumbnailDownloader;


        /// <summary>
        /// Method used for translating with Config Machine
        /// </summary>
        public static string Translate(string orig)
        {
            if (customWorldOption != null)
            {
                //return customWorldOption.Translate(orig);
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
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            Dictionary<string, RegionPack> installedPacksUpdated = new Dictionary<string, RegionPack>();

            foreach (KeyValuePair<string, RegionPack> regionPack in CustomWorldMod.installedRegionPacks)
            {
                //int regionNumber = 11;
                int packNumber = 0;
                try
                {
                    if (regionPack.Value.activated)
                    {
                        RegionPack regionPackUpdate = regionPack.Value;
                        //foreach (KeyValuePair<string, CustomRegion> newRegion in infoRegionUpdated.newRegions)
                        foreach (string newRegion in regionPackUpdate.regions)
                        {
                            //infoRegionUpdated.newRegions[newRegion.Key].regionNumber = regionNumber;
                            //regionNumber++;
                            CustomWorldMod.activeModdedRegions.Add(newRegion);
                            EnumExtender.AddDeclaration(typeof(MenuScene.SceneID), "Landscape_" + newRegion);
                        }
                        regionPackUpdate.loadNumber = packNumber;
                        dictionary.Add(regionPack.Key, regionPack.Value.folderName);
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

            try
            {
                EnumExtender.ExtendEnumsAgain();
                Array array = Enum.GetNames(typeof(MenuScene.SceneID));
                List<string> debug = new List<string>((string[])array);
                Log($"Extending SceneID enum ... [{string.Join(", ", debug.ToArray())}]");

            }
            catch (Exception e)
            {
                Log("Error extending SceneID enum " + e, true);
            }

            CustomWorldMod.installedRegionPacks = installedPacksUpdated;
            CustomWorldMod.loadedRegionPacks = dictionary;

            CustomWorldMod.Log($"Activated region packs [{string.Join(", ", new List<string>(CustomWorldMod.loadedRegionPacks.Keys).ToArray())}]");
            CustomWorldMod.Log($"New Custom Regions added by Region Packs [{string.Join(", ", activeModdedRegions.ToArray())}]");
        }

        /// <summary>
        /// Creaters the custom world log file
        /// </summary>
        public static void CreateCustomWorldLog()
        {
            using (StreamWriter sw = File.CreateText(Custom.RootFolderDirectory() + "customWorldLog.txt"))
            {
                sw.WriteLine($"############################################\n Custom World Log {versionCR} \n");
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
        public static void Log(string test)
        {
            if (!File.Exists(Custom.RootFolderDirectory() + "customWorldLog.txt"))
            {
                CreateCustomWorldLog();
            }

            using (StreamWriter file =
            new StreamWriter(Custom.RootFolderDirectory() + "customWorldLog.txt", true))
            {
                file.WriteLine(test);
            }

        }

        /// <summary>
        /// Appends the provided string to the log. If log file doesn't exist, create it. Bool indicates if you want to log into exceptionlog as well
        /// </summary>
        public static void Log(string test, bool throwException)
        {
            if (throwException)
            {
                Debug.LogError("[CustomRegions] " + test);
            }
            Log("[ERROR] " + test);
        }

        /// <summary>
        /// Builds available regions, loaded regions and save analyzer
        /// </summary>
        public static void LoadCustomWorldResources()
        {
            try
            {
                string[] array = File.ReadAllLines(Custom.RootFolderDirectory() + "World" + Path.DirectorySeparatorChar + "Regions" + Path.DirectorySeparatorChar + "regions.txt");
                numberOfVanillaRegions = array.Length;
                Log($"Number of regions installed in vanilla folder [{numberOfVanillaRegions}]");
                if (numberOfVanillaRegions != 12)
                {
                    Log($"ERROR! You have [{numberOfVanillaRegions - 12}] region(s) merged with vanilla files, CR might not work correctly", true);
                }
            }
            catch (Exception) { }

            CustomWorldMod.LoadInstalledPacks();

            CustomWorldMod.BuildModRegionsDictionary();

            CustomWorldMod.ReadSaveAnalyzerFiles();

            CustomWorldMod.AnalyzeSave();
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
                        if (!savedsum.Equals(installedRegionPacks[info.name].checksum))
                        {
                            saveProblems[saveSlot].checkSum.Add(info.name);
                        }

                        if (info.loadNumber != installedRegionPacks[info.name].loadNumber)
                        {
                            saveProblems[saveSlot].loadOrder = true;
                        }
                    }



                    // Compare installed regions
                    saveProblems[saveSlot].missingRegions = savedPacks.Except(loadedRegionPacks.Keys).ToList();
                    saveProblems[saveSlot].extraRegions = loadedRegionPacks.Keys.Except(savedPacks).ToList();


                    if (savedPacks.Count != loadedRegionPacks.Count ||
                        (saveProblems[saveSlot].missingRegions != null && saveProblems[saveSlot].missingRegions.Count != 0) ||
                        (saveProblems[saveSlot].extraRegions != null && saveProblems[saveSlot].extraRegions.Count != 0))
                    {
                        saveProblems[saveSlot].installedRegions = true;
                    }

                }
                catch (Exception e)
                {
                    Debug.Log(e + "SaveSlot " + saveSlot);
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

                string saveFileName = Custom.RootFolderDirectory() + CustomWorldMod.regionSavePath + $"CRsav_{saveSlot + 1}.txt";


                if (!File.Exists(Custom.RootFolderDirectory() + "UserData" + Path.DirectorySeparatorChar + ((saveSlot != 0) ? ("sav_" + (saveSlot + 1)) : "sav") + ".txt"))
                {
                    File.Delete(saveFileName);
                    Log($"Deleting {saveFileName} since vanilla save is empty");
                    return;
                }

                if (File.Exists(saveFileName))
                {
                    packInfoInSaveSlot[saveSlot] = new List<RegionPack>();
                    string allText = File.ReadAllText(saveFileName);
                    string sum = allText.Substring(0, 32);
                    allText = allText.Substring(32, allText.Length - 32);

                    if (Custom.Md5Sum(allText).Equals(sum))
                    {
                        Log($"SaveSlot [{saveSlot + 1}] found! Correct checksum");
                    }
                    else
                    {
                        File.Delete(saveFileName);
                        Log("CR Save was tinkered! Why did you touch this?");
                        Debug.LogError("CUSTOM REGIONS ERROR! CR save was tinkered. Why did you touch this?");
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

                                packInfoInSaveSlot[saveSlot].Add(new RegionPack(name, null, true, checkSum, null, null, null, null, null, -20, packNumber)); 
                                    
                                    
                                    //(regionID, null, null, true, -20, checkSum, regionNumber, null, null, null, null));

                            }
                        }
                    }
                    catch (Exception e) { Debug.Log(e); }


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
            foreach (KeyValuePair<string, string> regionPack in CustomWorldMod.loadedRegionPacks)
            {
                //CustomWorldMod.CustomWorldLog($"Custom Regions: PlayerProgression, loading new regions");

                foreach (string customRegion in CustomWorldMod.installedRegionPacks[regionPack.Key].regions)
                {
                    string regionToAdd = customRegion;
                    /*
                    bool shouldAdd = true;
                    for (int i = 0; i < regionNames.Length; i++)
                    {
                        if (regionToAdd.Equals(regionNames[i]))
                        {
                            shouldAdd = false;
                        }
                    }
                    */

                    //if (shouldAdd)
                    if (!regionNames.Contains(regionToAdd))
                    {
                        Array.Resize(ref regionNames, regionNames.Length + 1);
                        regionNames[regionNames.Length - 1] = regionToAdd;
                        CustomWorldMod.Log($"Custom Regions: Added new region to regionNames [{regionToAdd}] from [{regionToAdd}].");
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
                updatedList.Add(data.data);
            }

            return updatedList;
        }

        /// <summary>
        /// Builds available regions and manages json files
        /// </summary>
        public static void LoadInstalledPacks()
        {
            CustomWorldMod.customPearls = new Dictionary<string, CustomPearl>();

            string path = Custom.RootFolderDirectory() + CustomWorldMod.resourcePath;

            Dictionary<string, RegionPack> unsortedPacks = new Dictionary<string, RegionPack>();

            // For each Region Mod Installed
            foreach (string dir in Directory.GetDirectories(path))
            {
                Log($"#Loading [{dir}]");

                // For upgrading to regionpack
                string pathOfRegionInfo = dir + Path.DirectorySeparatorChar + "regionInfo.json";

                // JSON with all information
                string pathOfPackInfo = dir + Path.DirectorySeparatorChar + "packInfo.json";

                // Region Pack
                RegionPack pack = new RegionPack("", "", false, "", new DirectoryInfo(dir).Name, "",
                    new Dictionary<string, float>(),
                    new Dictionary<string, RegionConfiguration>(),
                    new List<string>(), -1, -1);

                // Creating pack info ...
                if (!File.Exists(pathOfPackInfo))
                {
                    // Region Name
                    pack.name = new DirectoryInfo(dir).Name;
                    string regionID = string.Empty;

                    // Upgrade to packInfo.json
                    if (File.Exists(pathOfRegionInfo))
                    {
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
                        regionID = File.ReadAllText(dir + Path.DirectorySeparatorChar + "regionID.txt");
                        regionID = regionID.ToUpper();

                        File.Delete(dir + Path.DirectorySeparatorChar + "regionID.txt");
                        CustomWorldMod.Log($"Custom Regions: Updating regionID from old CR version... Obtained regionID [{regionID}]");
                    }
                    else
                    {
                        pack.activated = true;
                        pack.checksum = CustomWorldMod.GenerateRegionCheckSum(dir);
                    }

                    SerializePackInfoJSON(pathOfPackInfo, ref pack);
                }

                Dictionary<string, object> dictionary = null;
                try
                {
                    dictionary = File.ReadAllText(pathOfPackInfo).dictionaryFromJson();
                }
                catch (Exception e)
                {
                    Log($"CORRUPTED JSON FILE -- DELETING [{pathOfPackInfo}] - [{e}]", true);
                    File.Delete(pathOfPackInfo);
                    SerializePackInfoJSON(pathOfPackInfo, ref pack);
                }

                if (dictionary != null)
                {
                    FromDictionaryToPackInfo(dictionary, ref pack);
                }

                if (pack.name.Equals(string.Empty))
                {
                    pack.name = new DirectoryInfo(dir).Name;
                }

                Log($"Description for ({pack.name}) is: [{pack.description}]");

                if (pack.description.Equals("N / A") || pack.description.Equals(string.Empty))
                {
                    pack.description = "No description";
                }

                string newDescr = string.Empty;
                string newUrl = string.Empty;

                if (pack.name.ToLower().Contains("aether"))
                {
                    newDescr = "Aether Ridge is a derelict desalination rig to the north of Sky Islands. Includes over 200 new rooms, six new arenas, and more.";
                    newUrl = "http://www.raindb.net/previews/aether.png";
                }
                else if (pack.name.ToLower().Contains("badlands"))
                {
                    newDescr = "The Badlands is a region connecting Farm Arrays and Garbage Wastes. It features many secrets and unlockables, including three new arenas.";
                    newUrl = "http://www.raindb.net/previews/badlands.png";
                }
                else if (pack.name.ToLower().Contains("root"))
                {
                    newDescr = "A new region expanding on Subterranean, and The Exterior, with all new rooms. Made to give exploration focused players more Rain World to discover.";
                    newUrl = "http://www.raindb.net/previews/root2.png";
                }
                else if (pack.name.ToLower().Contains("side house"))
                {
                    newDescr = "Adds a new region connecting Shoreline, 5P, and Depths. An amalgamation of many of the game's unused rooms. Also includes a couple custom unlockable maps for arena mode.";
                    newUrl = "http://www.raindb.net/previews/sidehouse_preview.png";
                }
                else if (pack.name.ToLower().Contains("swamplands"))
                {
                    newDescr = "A new swampy region that connects Garbage Wastes and Shoreline.";
                    newUrl = "http://www.raindb.net/previews/swamp.png";
                }
                else if (pack.name.ToLower().Contains("master quest"))
                {
                    newDescr = "A new game+ style mod that reorganizes the game's regions, trying to rekindle the feelings of when you first got lost in Rain World.";
                    newUrl = "http://www.raindb.net/previews/master.png";
                }
                else if (pack.name.ToLower().Contains("underbelly"))
                {
                    newDescr = "A dark and damp region connecting Outskirts, Shoreline and Farm arrays.";
                    newUrl = "http://www.raindb.net/previews/underbelly.png";
                }


                // Checksum handler
                string newChecksum = CustomWorldMod.GenerateRegionCheckSum(dir);
                if (!newChecksum.Equals(string.Empty) && !newChecksum.Equals(pack.checksum))
                {
                    Log($"New checksum for {pack.name} [{newChecksum}]");
                    pack.checksum = newChecksum;
                }
                if (!newDescr.Equals(string.Empty) && !newDescr.Equals(pack.description))
                {
                    Log($"New description for {pack.name} [{newDescr}]");
                    pack.description = newDescr;
                }
                if (!newUrl.Equals(string.Empty) && !newUrl.Equals(pack.url))
                {
                    Log($"New url for {pack.name} [{newUrl}]");
                    pack.url = newUrl;
                }

                // Write new info
                if ((!newDescr.Equals(string.Empty) && pack.description.Equals("No description")) || !newChecksum.Equals(string.Empty) || !newUrl.Equals(string.Empty))
                {
                    Log($"Updating packInfo for {pack.name}");
                    File.Delete(pathOfPackInfo);
                    SerializePackInfoJSON(pathOfPackInfo, ref pack);

                }

                /*
                // Load region information
                CustomWorldMod.Log($"Adding available region [{pack.regionID}]. Activated [{pack.activated}]. Folder name [{pack.folderName}]");
                if (pack.regionID != string.Empty)
                {
                    try
                    {
                        notSortedDictionary.Add(pack.regionID, pack);
                    }
                    catch (Exception dic) { CustomWorldMod.Log($"Custom Regions: Error in adding [{pack.regionID}] => {dic}"); };
                }
                */

                // Load region pack
                CustomWorldMod.Log($"Adding available region pack [{pack.name}]. Activated [{pack.activated}]. Folder name [{pack.folderName}]");
                if (pack.name != string.Empty)
                {
                    try
                    {
                        unsortedPacks.Add(pack.name, pack);
                    }
                    catch (Exception dic) { CustomWorldMod.Log($"Custom Regions: Error in adding [{pack.name}] => {dic}"); };
                }
                else
                {
                    Log($"Pack name ({pack.name}) or folder ({pack.folderName}) was empty! Fatal Error", true);
                }

                if (!Directory.Exists(Custom.RootFolderDirectory() + CustomWorldMod.resourcePath + pack.folderName))
                {
                    CustomWorldMod.Log($"CR could not find folder [{pack.folderName}] from region [{pack.name}]. Try removing any dots from the folder names and reloading.", true);
                }


                if (pack.activated)
                {
                    LoadCustomPearls(dir, pack.name);
                    LoadElectricGates(dir, pack);
                    LoadVariations(dir, pack);
                }
                else
                {
                    Log("Won't load configuration / pearls / electric gates until it is enabled");
                }

                Log("-------");
            }

            //installedRegionPacks
            CustomWorldMod.installedRegionPacks = new Dictionary<string, RegionPack>();
            foreach (KeyValuePair<string, RegionPack> pack in unsortedPacks.OrderBy(x => x.Value.loadOrder))
            {
                if(!pack.Key.Equals(""))
                {
                    CustomWorldMod.installedRegionPacks.Add(pack.Key, pack.Value);
                }
                else
                {
                    Log($"Error loading region", true);
                }
            }

            Log($"Ordered installed region packs [{string.Join(", ", installedRegionPacks.Keys.ToArray())}]");
        }


        private static void FromDictionaryToPackInfo(Dictionary<string, object> json, ref RegionPack pack)
        {
            //   regionInformation = JsonConvert.DeserializeObject <Dictionary<string, string>>(dictionary);

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
                //pack.activated = bool.Parse((string)value);
                //pack.activated = (bool)value;
            }
            if (packDictionary.TryGetValue("checksum", out value) && value != null)
            {
                pack.checksum = value.ToString();
            }
            if (packDictionary.TryGetValue("url", out value) && value != null)
            {
                pack.url = value.ToString();
            }
            if (packDictionary.TryGetValue("loadOrder", out value) && value != null)
            {
                pack.loadOrder = int.Parse(value.ToString());
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
                            Log($"Duplicate region loaded from regionInfo.json [{pack.name}]", true);
                        }
                    }
                }
            }


        }


        public static void FromDictionaryToRegionConfig(Dictionary<string, object> dictionary, ref RegionConfiguration regionConfiguration)
        {
            if (dictionary.TryGetValue("albino_leviathan", out object value) && value != null)
            {
                regionConfiguration.albinoLevi = bool.Parse(value.ToString());
            }

            if (dictionary.TryGetValue("albino_jetfish", out value) && value != null)
            {
                regionConfiguration.albinoJet = bool.Parse(value.ToString());
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
                regionConfiguration.blackSalamanderChance = int.Parse(value.ToString());
            }
        }

        public static void LoadElectricGates(string dir, RegionPack pack)
        {
            // Add electric gates
            string pathToElectricGates = dir + Path.DirectorySeparatorChar + "World" + Path.DirectorySeparatorChar + "Gates" + Path.DirectorySeparatorChar + "electricGates.txt";
            if (File.Exists(pathToElectricGates))
            {
                string[] electricGates = File.ReadAllLines(pathToElectricGates);
                for (int i = 0; i < electricGates.Length; i++)
                {
                    string gateName = Regex.Split(electricGates[i], " : ")[0];
                    float meterHeigh = float.Parse(Regex.Split(electricGates[i], " : ")[1]);

                    Log($"Added new gate electric gate [{gateName}] from [{pack.name}]. Meter height [{meterHeigh}]");
                    pack.electricGates.Add(gateName, meterHeigh);
                }
            }

        }

        public static void LoadCustomPearls(string dir, string regionName)
        {
            // Add Custom Pearls
            string pathToPearls = dir + Path.DirectorySeparatorChar + "Assets" + Path.DirectorySeparatorChar + "pearlData.txt";
            if (File.Exists(pathToPearls))
            {
                Log($"Loading pearl data for {regionName}");
                string[] customPearlsLines = File.ReadAllLines(pathToPearls);
                for (int i = 0; i < customPearlsLines.Length; i++)
                {
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
                        pearlName = $"{regionName}_{lineDivided[1]}";
                    }
                    catch (Exception e)
                    {
                        Log("Error loading pearl information, missing pearl ID or pearl Name in pearlData.txt" + e, true);
                        continue;
                    }

                    try
                    {
                        pearlColor = OptionalUI.OpColorPicker.HexToColor(lineDivided[2]);
                    }
                    catch (Exception) { Log($"Pearl missing color from {regionName}", true); }

                    try
                    {
                        secondaryColor = OptionalUI.OpColorPicker.HexToColor(lineDivided[3]);
                    }
                    catch (Exception) { Log($"Pearl missing highlighted color from {regionName}"); }

                    CustomWorldMod.Log($"Added new pearl [{pearlName} / {fileNumber} / {pearlColor}]");

                    CustomWorldMod.customPearls.Add(pearlName, new CustomPearl(pearlName, fileNumber, pearlColor, secondaryColor, regionName));

                    // Extend PearlTypeEnum
                    EnumExtender.AddDeclaration(typeof(DataPearl.AbstractDataPearl.DataPearlType), pearlName);

                    // Extend ConvoID
                    EnumExtender.AddDeclaration(typeof(Conversation.ID), "Moon_" + pearlName);
                }
                EnumExtender.ExtendEnumsAgain();
            }

            // Encrypt text files
            Log($"Creating conversation files for {regionName}...");
            for (int j = 0; j < Enum.GetNames(typeof(InGameTranslator.LanguageID)).Length; j++)
            {
                for (int k = 1; k <= 57; k++)
                {
                    string pathToConvo = dir + Path.DirectorySeparatorChar + "Assets" + Path.DirectorySeparatorChar + "Text" +
                        Path.DirectorySeparatorChar + "Text_" + LocalizationTranslator.LangShort((InGameTranslator.LanguageID)j) + Path.DirectorySeparatorChar + k + ".txt";

                    if (File.Exists(pathToConvo))
                    {
                        string convoLines = File.ReadAllText(pathToConvo, Encoding.Default);
                        if (convoLines[0] == '0' && Regex.Split(convoLines, Environment.NewLine).Length > 1)
                        {
                            Log($"Encrypting file [{Path.GetFileNameWithoutExtension(pathToConvo)}] from [{regionName}]. [{Regex.Split(convoLines, Environment.NewLine).Length}]");
                            string text4 = Custom.xorEncrypt(convoLines, 54 + k + j * 7);
                            text4 = '1' + text4.Remove(0, 1);
                            File.WriteAllText(pathToConvo, text4);
                        }
                    }
                }
            }
        }

        public static void LoadVariations(string dir, RegionPack packInfo)
        {
            string pathToRegionsDir = dir + Path.DirectorySeparatorChar + "World" + Path.DirectorySeparatorChar + "Regions" + Path.DirectorySeparatorChar;
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
                    RegionConfiguration regionConfiguration = new RegionConfiguration(null, false, false, false, null, false, null, -1);

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
                        } catch (Exception e) { Log($"Exception loading variation [{e}]", true); }
                        regionConfiguration.regionID = new DirectoryInfo(regionDir).Name;

                        // Load region information
                        CustomWorldMod.Log($"Adding configuration for region [{regionConfiguration.regionID}] from [{packInfo.name}]");/* - " +
                        $"Albino Leviathan [{regionConfiguration.albinoLevi}] Albino JetFish [{regionConfiguration.albinoJet}] " +
                        $"Kelp Color [{regionConfiguration.kelpColor.Value.r},{regionConfiguration.kelpColor.Value.g}, {regionConfiguration.kelpColor.Value.b}]" +
                        $" BLL color [{regionConfiguration.bllColor.Value.r},{regionConfiguration.bllColor.Value.g}, {regionConfiguration.bllColor.Value.b}] " +
                        $"Black Salamander chance [{regionConfiguration.blackSalamanderChance * 100f}%]");
                        */
                        if (packInfo.name != string.Empty)
                        {
                            try
                            {
                                packInfo.regionConfig.Add(regionConfiguration.regionID, regionConfiguration);
                            }
                            catch (Exception dic) { CustomWorldMod.Log($"Custom Regions: Error in adding config [{regionConfiguration.regionID}] => {dic}"); };
                        }

                    }
                }
            }
        }

        private static string GenerateRegionCheckSum(string path)
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
                Log($"[{Path.GetFileNameWithoutExtension(path)}] text file doesn't exist: [{path}]");
                return string.Empty;
            }
            return File.ReadAllText(path);
        }

        public static string GetSaveInformation()
        {
            string dictionaryString = "Custom Regions: New save, Custom Regions Information \n";
            dictionaryString += "<progCRdivA>";
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegionPacks)
            {
                dictionaryString += $"<progCRdivB>{keyValues.Key}" +
                    $"<progCRdivB>{CustomWorldMod.installedRegionPacks[keyValues.Key].loadNumber}" +
                    $"<progCRdivB>{CustomWorldMod.installedRegionPacks[keyValues.Key].checksum}";
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

        private static void SerializePackInfoJSON(string dir, ref RegionPack pack)
        {
            using (StreamWriter sw = File.CreateText(dir))
            {
                string json = "{\n" +
                     "   \"regionPackName\": \"" + pack.name + "\", \n" +
                     "   \"description\": \"" + pack.description + "\", \n" +
                     "   \"activated\": " + pack.activated.ToString().ToLower() + ", \n" +
                     "   \"loadOrder\": " + pack.loadOrder + ", \n" +
                     "   \"regions\": \"" + string.Join(", ", pack.regions.ToArray()) + "\", \n" +
                     "   \"url\": \"" + pack.url + "\", \n" +
                     "   \"checksum\": \"" + pack.checksum + "\" \n" +
                     "}";
                /*
                                int i = 1;
                                foreach (KeyValuePair<string, CustomRegion> keyValues in pack.newRegions)
                                {
                                    CustomRegion region = keyValues.Value;
                                    json += ",\n " +
                                        $"\"region{i}\": " + "{\n" +
                                        "   \"regionID\": \"" + region.regionID + "\", \n" +
                                        "   \"loadOrder\": \"" + region.loadOrder + "\" \n" +
                                         "}";
                                    i++;
                                }
                                json += "\n}";
                */
                sw.WriteLine(json);
            }
        }




        public static void WriteRegionConfigJSONFile(string dirPath, bool leviAlbino, bool jetfishAlbino,
            string shortcutColor, string kelpColor, string bllColor)
        {
            using (StreamWriter sw = File.CreateText(dirPath + Path.DirectorySeparatorChar + "regionConfiguration.json"))
            {
                sw.WriteLine("{\n"
                    + $"   \"albino_leviathan\":  \"{leviAlbino.ToString().ToLower()}\", \n"
                    + $"   \"albino_jetfish\":  \"{jetfishAlbino.ToString().ToLower()}\", \n"

                    + $"   \"shortcut_color\":  \"{shortcutColor}\", \n"

                    + $"   \"monster_kelp_color\":  \"{kelpColor}\", \n"

                    + $"   \"brother_color\":  \"{bllColor}\", \n"
                );
            }
        }



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
                obtainedInfo.Add($"Description: {pack.description.Substring(0, 15)}...");
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
                pack.url = value.ToString();
                obtainedInfo.Add($"URL: {pack.url}");
            }
            Log($"Upgrade to regionPack.json. Information parsed: [{string.Join(", ", obtainedInfo.ToArray())}]");
        }

        internal static void DownloadThumbs()
        {
            Dictionary<string, string> thumbInfo = new Dictionary<string, string>();
            foreach (KeyValuePair<string, RegionPack> entry in CustomWorldMod.installedRegionPacks)
            {
                if (entry.Value.url != string.Empty && !File.Exists(Custom.RootFolderDirectory() + CustomWorldMod.resourcePath + entry.Value.folderName + Path.DirectorySeparatorChar + "thumb.png"))
                {
                    try
                    {
                        Log($"new thumb {entry.Value.folderName} - {entry.Value.url}");
                        thumbInfo.Add(entry.Value.folderName, entry.Value.url);
                    } catch (Exception e) {Log($"Error queuing thumbs [{e}] [{entry.Value.url}]", true); }
                }
            }

            if (ThumbnailDownloader.instance == null)
            {
                ThumbnailDownloader.Create(thumbInfo);
            }

            //thumbnailDownloader = new ThumbnailDownloader(thumbInfo);


            /*
            string filePath = Custom.RootFolderDirectory() + CustomWorldMod.resourcePath +
                 "Badlands" + Path.DirectorySeparatorChar;
            string url = "http://www.raindb.net/previews/badlands.png";

            Debug.Log("Try downloadin to " + filePath);
            //DownloadAndSaveThumb(url, filePath);
            www = new WWW(url);
            */
        }
    }
}
