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
        /// Dictionary with activated regions, where the Key is the region ID and the value is the name.
        /// </summary>
        public static Dictionary<string, string> loadedRegions;

        /// <summary>
        /// Dictionary with all installed regions, where the Key is the region ID and the value is a struct with its information.
        /// </summary>
        public static Dictionary<string, RegionInformation> availableRegions;

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
        /// Array of lists containing loaded regions info for each saveslot
        /// </summary>
        public static List<RegionInformation>[] regionInfoInSaveSlot;

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
            // Only load activate regions from CustomWorldMod.availableRegions
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            Dictionary<string, RegionInformation> updatedEntry = new Dictionary<string, RegionInformation>();
            foreach (KeyValuePair<string, RegionInformation> entry in CustomWorldMod.availableRegions)
            {
                int regionNumber = 11;

                try
                {
                    if (entry.Value.activated)
                    {
                        RegionInformation infoRegionUpdated = entry.Value;
                        infoRegionUpdated.regionNumber = regionNumber;

                        updatedEntry.Add(infoRegionUpdated.regionID, infoRegionUpdated);
                        regionNumber++;

                        dictionary.Add(entry.Value.regionID, entry.Value.folderName);
                        EnumExtender.AddDeclaration(typeof(MenuScene.SceneID), "Landscape_" + entry.Value.regionID);
                    }
                    else
                    {
                        updatedEntry.Add(entry.Key, entry.Value);
                    }
                }
                catch (Exception e) { CustomWorldMod.Log($"Custom Regions: Error while trying to add customRegion: {e}"); }
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

            CustomWorldMod.availableRegions = updatedEntry;
            CustomWorldMod.loadedRegions = dictionary;

            CustomWorldMod.Log($"Activated regions [{string.Join(", ", new List<string>(CustomWorldMod.loadedRegions.Values).ToArray())}]");
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

            CustomWorldMod.LoadAvailableRegions();

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
            if (regionInfoInSaveSlot == null)
            {
                return;
            }

            for (int saveSlot = 0; saveSlot < regionInfoInSaveSlot.Length; saveSlot++)
            {
                try
                {
                    saveProblems[saveSlot] = new SaveProblems(false, false, new List<string>(), new List<string>(), new List<string>());

                    if (regionInfoInSaveSlot[saveSlot] == null)
                    {
                        CustomWorldMod.Log($"No installed info for save slot [{saveSlot}]");
                        continue;
                    }

                    List<string> savedRegions = new List<string>();
                    foreach (RegionInformation info in regionInfoInSaveSlot[saveSlot])
                    {
                        savedRegions.Add(info.regionID);
                        string savedsum = info.checksum;
                        if (!savedsum.Equals(availableRegions[info.regionID].checksum))
                        {
                            saveProblems[saveSlot].checkSum.Add(info.regionID);
                        }

                        if (info.regionNumber != availableRegions[info.regionID].regionNumber)
                        {
                            saveProblems[saveSlot].loadOrder = true;
                        }
                    }



                    // Compare installed regions
                    saveProblems[saveSlot].missingRegions = savedRegions.Except(loadedRegions.Keys).ToList();
                    saveProblems[saveSlot].extraRegions = loadedRegions.Keys.Except(savedRegions).ToList();


                    if (savedRegions.Count != loadedRegions.Count ||
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
            regionInfoInSaveSlot = new List<RegionInformation>[3];
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
                    regionInfoInSaveSlot[saveSlot] = new List<RegionInformation>();
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

                                string regionID = Regex.Split(minedLines.Find(x => x.Contains("REGID")), "<REGID>")[1];
                                string checkSum = Regex.Split(minedLines.Find(x => x.Contains("SUM")), "<SUM>")[1];
                                int regionNumber = int.Parse(Regex.Split(minedLines.Find(x => x.Contains("ORDER")), "<ORDER>")[1]);

                                regionInfoInSaveSlot[saveSlot].Add(new RegionInformation(regionID, null, null, true, -20, checkSum, regionNumber, null, null, null, null));

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
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
            {
                //CustomWorldMod.CustomWorldLog($"Custom Regions: PlayerProgression, loading new regions");
                string regionToAdd = keyValues.Key;
                bool shouldAdd = true;

                for (int i = 0; i < regionNames.Length; i++)
                {
                    if (regionToAdd.Equals(regionNames[i]))
                    {
                        shouldAdd = false;
                    }
                }
                if (shouldAdd)
                {
                    Array.Resize(ref regionNames, regionNames.Length + 1);
                    regionNames[regionNames.Length - 1] = keyValues.Key;
                    CustomWorldMod.Log($"Custom Regions: Added new region to regionNames [{regionToAdd}] from [{keyValues.Value}].");
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


        public static object GetValueDictionary(string key, Dictionary<string, object> dictionary)
        {
            object value = null;
            if (dictionary.ContainsKey(key))
            {
                value = dictionary[key];
            }
            return value;
        }

        /// <summary>
        /// Builds available regions and manages json files
        /// </summary>
        public static void LoadAvailableRegions()
        {
            CustomWorldMod.availableRegions = new Dictionary<string, RegionInformation>();
            CustomWorldMod.customPearls = new Dictionary<string, CustomPearl>();
            //CustomWorldMod.configurationRegions = new Dictionary<string, RegionConfiguration>();

            string path = Custom.RootFolderDirectory() + CustomWorldMod.resourcePath;
            Dictionary<string, RegionInformation> notSortedDictionary = new Dictionary<string, RegionInformation>();
            // For each Region Mod Installed
            foreach (string dir in Directory.GetDirectories(path))
            {
                Log($"#Loading [{dir}]");

                string pathOfRegionInfo = dir + Path.DirectorySeparatorChar + "regionInfo.json";

                // Region Information
                string regionID = string.Empty;
                string regionName = string.Empty;
                string description = "No description";
                bool activated = true;
                string checksum = string.Empty;
                string url = string.Empty;
                int loadOrder = 100;
                // File does not exist, generate regionInfo.json
                if (!File.Exists(pathOfRegionInfo))
                {
                    // Region Name
                    regionName = new DirectoryInfo(dir).Name;

                    // If upgrading from old CR version
                    if (File.Exists(dir + Path.DirectorySeparatorChar + "regionID.txt"))
                    {
                        regionID = File.ReadAllText(dir + Path.DirectorySeparatorChar + "regionID.txt");
                        regionID = regionID.ToUpper();

                        activated = true;

                        File.Delete(dir + Path.DirectorySeparatorChar + "regionID.txt");
                        CustomWorldMod.Log($"Custom Regions: Updating regionID from old CR version... Obtained regionID [{regionID}]");
                    }

                    // regionID.txt did not exist or was empty
                    if (regionID == string.Empty)
                    {
                        string regionsPath = dir + Path.DirectorySeparatorChar + "World" + Path.DirectorySeparatorChar + "Regions";
                        CustomWorldMod.Log($"Custom Regions: Empty regionID, obtaining from [{regionsPath}]. Valid [{Directory.Exists(regionsPath)}]");

                        if (Directory.Exists(regionsPath))
                        {
                            // Try to get regionID
                            foreach (string regionsDir in Directory.GetDirectories(regionsPath))
                            {
                                regionID = Path.GetFileNameWithoutExtension(regionsDir);
                                foreach (string vanillaRegion in CustomWorldMod.VanillaRegions())
                                {
                                    //CustomWorldMod.CustomWorldLog($"Custom Regions: Comparing [{regionID}] with [{vanillaRegion}]");
                                    if (regionsDir.Contains(vanillaRegion))
                                    {
                                        regionID = string.Empty;
                                        break;
                                    }
                                }

                                if (regionID != string.Empty)
                                {
                                    break;
                                }
                            }

                        }
                    }

                    // If no ID found, generate one...
                    if (regionID == string.Empty)
                    {
                        // If a customRegion does not add new regions, obtain regionID from capital letters.
                        foreach (char letters in regionName)
                        {
                            if (char.IsUpper(letters))
                            {
                                regionID += letters;
                            }
                            if(regionID.Length == 2) { break; }
                        }
                        CustomWorldMod.Log($"Generated regionID since it was empty... [{regionID}]");
                    }

                    checksum = CustomWorldMod.GenerateRegionCheckSum(dir);

                    WriteRegionInfoJSONFile(dir, regionID, description, regionName, activated, loadOrder, url, checksum);

                }

                RegionInformation regionInformation = new RegionInformation(string.Empty, string.Empty, "No description",
                    true, loadOrder, string.Empty, -1, new DirectoryInfo(dir).Name, string.Empty, new Dictionary<string, float>(), new Dictionary<string, RegionConfiguration>());


                Dictionary<string, object> dictionary = null;
                try
                {
                    dictionary = File.ReadAllText(pathOfRegionInfo).dictionaryFromJson();
                }
                catch (Exception e)
                {
                    Log($"CORRUPTED JSON FILE -- DELETING [{pathOfRegionInfo}] - [{e}]", true);
                    File.Delete(dir + Path.DirectorySeparatorChar + "regionInfo.json");
                    WriteRegionInfoJSONFile(dir, regionInformation.regionID, regionInformation.description, regionInformation.regionName, regionInformation.activated, regionInformation.loadOrder, regionInformation.url, regionInformation.checksum);
                }

                //List<string> jsonFields = new List<string>(){ "regionID", "description", "regionName", "activated","loadOrder", "checksum", "url"};
                if (dictionary != null)
                {

                    FromDictionaryToRegionInfo(dictionary, ref regionInformation);

                    Log($"Description for ({regionInformation.regionName}) is: [{regionInformation.description}]");
                    //  string oldDescription = regionInformation.description;
                    if (regionInformation.description.Equals("N / A") || regionInformation.description.Equals(string.Empty))
                    {
                        regionInformation.description = "No description";
                    }

                    string newDescr = string.Empty;
                    string newUrl = string.Empty;

                    if (regionInformation.regionName.ToLower().Contains("aether ridge") || regionInformation.regionID.Equals("AR"))
                    {
                        newDescr = "Aether Ridge is derelict desalination rig to the north of Sky Islands. Includes over 200 new rooms, six new arenas, and more.";
                        newUrl = "http://www.raindb.net/previews/aether.png";
                    }
                    else if (regionInformation.regionName.ToLower().Contains("badlands") || regionInformation.regionID.Equals("BL"))
                    {
                        newDescr = "The Badlands is a region connecting Farm Arrays and Garbage Wastes. It features many secrets and unlockables, including three new arenas.";
                        newUrl = "http://www.raindb.net/previews/badlands.png";
                    }
                    else if (regionInformation.regionName.ToLower().Contains("root") || regionInformation.regionID.Equals("TR"))
                    {
                        newDescr = "A new region expanding on Subterranean, and The Exterior, with all new rooms. Made to give exploration focused players more Rain World to discover.";
                        newUrl = "http://www.raindb.net/previews/root2.png";
                    }
                    else if (regionInformation.regionName.ToLower().Contains("side house"))
                    {
                        newDescr = "Adds a new region connecting Shoreline, 5P, and Depths. An amalgamation of many of the game's unused rooms. Also includes a couple custom unlockable maps for arena mode.";
                        newUrl = "http://www.raindb.net/previews/sidehouse_preview.png";
                    }
                    else if (regionInformation.regionName.ToLower().Contains("swamplands"))
                    {
                        newDescr = "A new swampy region that connects Garbage Wastes and Shoreline.";
                        newUrl = "http://www.raindb.net/previews/swamp.png";
                    }
                    else if (regionInformation.regionName.ToLower().Contains("master quest"))
                    {
                        newDescr = "A new game+ style mod that reorganizes the game's regions, trying to rekindle the feelings of when you first got lost in Rain World.";
                        newUrl = "http://www.raindb.net/previews/master.png";
                    }


                    // Checksum handler
                    string newChecksum = CustomWorldMod.GenerateRegionCheckSum(dir);
                    if (!newChecksum.Equals(string.Empty) && !newChecksum.Equals(regionInformation.checksum))
                    {
                        Log($"New checksum for {regionInformation.regionName} [{newChecksum}]");
                        regionInformation.checksum = newChecksum;
                    }
                    if (!newDescr.Equals(string.Empty) && !newDescr.Equals(regionInformation.description))
                    {
                        Log($"New description for {regionInformation.regionName} [{newDescr}]");
                        regionInformation.description = newDescr;
                    }
                    if (!newUrl.Equals(string.Empty) && !newUrl.Equals(regionInformation.url))
                    {
                        Log($"New url for {regionInformation.regionName} [{newUrl}]");
                        regionInformation.url = newUrl;
                    }

                    // Write new info
                    if ((!newDescr.Equals(string.Empty) && regionInformation.description.Equals("No description")) || !newChecksum.Equals(string.Empty) || !newUrl.Equals(string.Empty))
                    {
                        Log($"Updating regionInfo for {regionInformation.regionName}");
                        File.Delete(dir + Path.DirectorySeparatorChar + "regionInfo.json");
                        WriteRegionInfoJSONFile(dir, regionInformation.regionID, regionInformation.description, regionInformation.regionName, regionInformation.activated, regionInformation.loadOrder, regionInformation.url, regionInformation.checksum);

                    }

                    // Load region information
                    CustomWorldMod.Log($"Adding available region [{regionInformation.regionID}]. Activated [{regionInformation.activated}]. Folder name [{regionInformation.folderName}]");
                    if (regionInformation.regionID != string.Empty)
                    {
                        try
                        {
                            notSortedDictionary.Add(regionInformation.regionID, regionInformation);
                        }
                        catch (Exception dic) { CustomWorldMod.Log($"Custom Regions: Error in adding [{regionInformation.regionID}] => {dic}"); };
                    }

                    if (!Directory.Exists(Custom.RootFolderDirectory() + CustomWorldMod.resourcePath + regionInformation.folderName))
                    {
                        CustomWorldMod.Log($"CR could not find folder [{regionInformation.folderName}] from region [{regionInformation.regionID}]. Try removing any dots from the folder names and reloading.", true);
                    }
                }

                if (regionInformation.activated)
                {
                    LoadCustomPearls(dir, regionInformation.regionID);
                    LoadElectricGates(dir, regionInformation);
                    LoadVariations(dir, regionInformation);
                }
                else
                {
                    Log("Won't load configuration / pearls / electric gates until it is enabled");
                }

                Log("-------");
            }

            foreach (KeyValuePair<string, RegionInformation> element in notSortedDictionary.OrderBy(d => d.Value.loadOrder))
            {
                //element.Value.regionNumber = regionNumber;
                CustomWorldMod.availableRegions.Add(element.Key, element.Value);
            }
        }

        private static void FromDictionaryToRegionInfo(Dictionary<string, object> dictionary, ref RegionInformation regionInformation)
        {
            if (GetValueDictionary("regionID", dictionary) != null)
            {
                regionInformation.regionID = (string)GetValueDictionary("regionID", dictionary);
            }

            if (GetValueDictionary("description", dictionary) != null)
            {

                regionInformation.description = (string)GetValueDictionary("description", dictionary);
            }

            if (GetValueDictionary("regionName", dictionary) != null)
            {
                regionInformation.regionName = (string)GetValueDictionary("regionName", dictionary);
            }

            // WHAT THE FRICC IS THIS
            if (dictionary.ContainsKey("activated"))
            {
                regionInformation.activated = dictionary["activated"].ToString().ToLower().Contains("true");
            }

            if (GetValueDictionary("loadOrder", dictionary) != null)
            {
                regionInformation.loadOrder = int.Parse(GetValueDictionary("loadOrder", dictionary).ToString());
            }

            if (GetValueDictionary("checksum", dictionary) != null)
            {
                regionInformation.checksum = (string)GetValueDictionary("checksum", dictionary);
            }

            if (GetValueDictionary("url", dictionary) != null)
            {
                regionInformation.url = (string)GetValueDictionary("url", dictionary);
            }
        }

        public static void FromDictionaryToRegionConfig(Dictionary<string, object> dictionary, ref RegionConfiguration regionConfiguration)
        {
            if (GetValueDictionary("albino_leviathan", dictionary) != null)
            {
                regionConfiguration.albinoLevi = bool.Parse((string)GetValueDictionary("albino_leviathan", dictionary));
            }

            if (GetValueDictionary("albino_jetfish", dictionary) != null)
            {
                regionConfiguration.albinoJet = bool.Parse((string)GetValueDictionary("albino_jetfish", dictionary));
            }

            if (GetValueDictionary("monster_kelp_color", dictionary) != null)
            {
                try
                {
                    string color = (string)GetValueDictionary("monster_kelp_color", dictionary);
                    if(color.Contains("#"))
                    {
                        color = color.Replace("#", "");
                        CustomWorldMod.Log($"Removed # from color [{color}]");
                    }
                    regionConfiguration.kelpColor = OptionalUI.OpColorPicker.HexToColor(color);
                }
                catch (Exception) { regionConfiguration.kelpColor = null; }
            }

            if (GetValueDictionary("brother_color", dictionary) != null)
            {
                try
                {
                    string color = (string)GetValueDictionary("brother_color", dictionary);
                    if (color.Contains("#"))
                    {
                        color = color.Replace("#", "");
                        CustomWorldMod.Log($"Removed # from color [{color}]");
                    }
                    regionConfiguration.bllColor = OptionalUI.OpColorPicker.HexToColor(color);
                }
                catch (Exception) { regionConfiguration.bllColor = null; }
            }
        }

        public static void LoadElectricGates(string dir, RegionInformation regionInfo)
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

                    Log($"Added new gate electric gate [{gateName}] from [{regionInfo.regionID}]. Meter height [{meterHeigh}]");
                    regionInfo.electricGates.Add(gateName, meterHeigh);
                }
            }

        }

        public static void LoadCustomPearls(string dir, string regionID)
        {
            // Add Custom Pearls
            string pathToPearls = dir + Path.DirectorySeparatorChar + "Assets" + Path.DirectorySeparatorChar + "pearlData.txt";
            if (File.Exists(pathToPearls))
            {
                Log($"Loading pearl data for {regionID}");
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
                        pearlName = $"{regionID}_{lineDivided[1]}";
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
                    catch (Exception) { Log($"Pearl missing color from {regionID}", true); }

                    try
                    {
                        secondaryColor = OptionalUI.OpColorPicker.HexToColor(lineDivided[3]);
                    }
                    catch (Exception) { Log($"Pearl missing highlighted color from {regionID}"); }

                    CustomWorldMod.Log($"Added new pearl [{pearlName} / {fileNumber} / {pearlColor}]");

                    CustomWorldMod.customPearls.Add(pearlName, new CustomPearl(pearlName, fileNumber, pearlColor, secondaryColor, regionID));

                    // Extend PearlTypeEnum
                    EnumExtender.AddDeclaration(typeof(DataPearl.AbstractDataPearl.DataPearlType), pearlName);

                    // Extend ConvoID
                    EnumExtender.AddDeclaration(typeof(Conversation.ID), "Moon_" + pearlName);
                }
                EnumExtender.ExtendEnumsAgain();
            }

            // Encrypt text files
            for (int j = 0; j < Enum.GetNames(typeof(InGameTranslator.LanguageID)).Length; j++)
            {
                for (int k = 1; k <= 57; k++)
                {
                    string pathToConvo = dir + Path.DirectorySeparatorChar + "Assets" + Path.DirectorySeparatorChar + "Text" +
                        Path.DirectorySeparatorChar + "Text_" + LocalizationTranslator.LangShort((InGameTranslator.LanguageID)j) + Path.DirectorySeparatorChar + k + ".txt";

                    if (File.Exists(pathToConvo))
                    {
                        Log($"Creating conversation files for {regionID}...");
                        string convoLines = File.ReadAllText(pathToConvo, Encoding.Default);
                        if (convoLines[0] == '0' && Regex.Split(convoLines, Environment.NewLine).Length > 1)
                        {
                            Log($"Encrypting file [{Path.GetFileNameWithoutExtension(pathToConvo)}] from [{regionID}]. [{Regex.Split(convoLines, Environment.NewLine).Length}]");
                            string text4 = Custom.xorEncrypt(convoLines, 54 + k + j * 7);
                            text4 = '1' + text4.Remove(0, 1);
                            File.WriteAllText(pathToConvo, text4);
                        }
                    }
                }
            }
        }

        public static void LoadVariations(string dir, RegionInformation regionInfo)
        {
            string pathToRegionsDir = dir + Path.DirectorySeparatorChar + "World" + Path.DirectorySeparatorChar + "Regions" + Path.DirectorySeparatorChar;
            if (!Directory.Exists(pathToRegionsDir))
            {
                Log($"Region [{regionInfo.regionName}] doesn't have Regions folder");
                return;
            }
            foreach (string regionDir in Directory.GetDirectories(pathToRegionsDir))
            {
                string pathConfig = regionDir + Path.DirectorySeparatorChar + "CustomConfig.json";
                // Load configuration
                if (File.Exists(pathConfig))
                {
                    Log($"Loading variation config for region [{new DirectoryInfo(regionDir).Name}] from [{regionInfo.regionName}]");
                    RegionConfiguration regionConfiguration = new RegionConfiguration(null, false, false, false, null, false, null);

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
                        regionConfiguration.kelpVanilla = regionConfiguration.kelpColor == null;
                        regionConfiguration.bllVanilla = regionConfiguration.bllColor == null;
                        regionConfiguration.regionID = new DirectoryInfo(regionDir).Name;

                        // Load region information
                        CustomWorldMod.Log($"Adding configuration for region [{regionConfiguration.regionID}] from [{regionInfo.regionName}] - " +
                        $"Albino Leviathan [{regionConfiguration.albinoLevi}] Albino JetFish [{regionConfiguration.albinoJet}] " +
                        $"Kelp Color [{regionConfiguration.kelpColor}] BLL color [{regionConfiguration.bllColor}]");

                        if (regionInfo.regionID != string.Empty)
                        {
                            try
                            {
                                regionInfo.regionConfig.Add(regionConfiguration.regionID, regionConfiguration);
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
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
            {
                dictionaryString += $"<progCRdivB>{keyValues.Key}" +
                    $"<progCRdivB>{CustomWorldMod.availableRegions[keyValues.Key].regionNumber}" +
                    $"<progCRdivB>{CustomWorldMod.availableRegions[keyValues.Key].checksum}";
            }
            dictionaryString += "<progCRdivA>";
            dictionaryString = dictionaryString.TrimEnd(',', ' ') + "";

            return dictionaryString;
        }


        public static string SerializeRegionInfo(RegionInformation regionInfo)
        {

            string infoSerial = string.Empty;

            // Start Region ID
            infoSerial += $"{saveDividerA}<REGID>{regionInfo.regionID}";

            /*------ CHECKSUM --------*/
            infoSerial += $"{saveDividerB}<SUM>{regionInfo.checksum}{saveDividerB}";
            /*------ CHECKSUM --------*/

            /*------ ORDER --------*/
            infoSerial += $"{saveDividerB}<ORDER>{regionInfo.regionNumber}{saveDividerB}";
            /*------ ORDER --------*/

            // End Region ID
            infoSerial += $"{saveDividerA}";

            return infoSerial;
        }



        public static void WriteRegionInfoJSONFile(string dirPath, string regionID, string description, string regionName, bool activated, int loadOrder, string url, string checksum)
        {
            // Create a file to write to.
            using (StreamWriter sw = File.CreateText(dirPath + Path.DirectorySeparatorChar + "regionInfo.json"))
            {
                sw.WriteLine("{\n"
                    + "   \"regionID\":  \"" + regionID + "\", \n"
                    + "   \"description\":  \"" + description + "\", \n"

                    + "   \"regionName\":  \"" + regionName + "\", \n"
                    + "   \"activated\":  " + activated.ToString().ToLower() + ", \n"
                    + "   \"loadOrder\": " + loadOrder + ", \n"

                    + "   \"url\":  \"" + url + "\", \n"
                    + "   \"checksum\":  \"" + checksum + "\" \n"
                    + "}");
                /*
                sw.WriteLine(new Dictionary<string, object>() { 
                    { "regionID", regionID },
                    { "description", description },
                    { "regionName", regionName },
                    { "activated", activated.ToString().ToLower() },
                    { "loadOrder", loadOrder },
                    { "url", url },
                    { "checksum", checksum }

                }.toJson());*/
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

        internal static void DownloadThumbs()
        {

            Dictionary<string, string> thumbInfo = new Dictionary<string, string>();
            foreach (KeyValuePair<string, RegionInformation> entry in CustomWorldMod.availableRegions)
            {
                if (entry.Value.url != string.Empty && !File.Exists(Custom.RootFolderDirectory() + CustomWorldMod.resourcePath + entry.Value.folderName + Path.DirectorySeparatorChar + "thumb.png"))
                {
                    Log($"new thumb {entry.Value.folderName} - {entry.Value.url}");
                    thumbInfo.Add(entry.Value.folderName, entry.Value.url);
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
