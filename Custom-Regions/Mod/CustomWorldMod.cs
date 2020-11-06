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
using On.ScavTradeInstruction;
using Partiality;
using PastebinMachine.EnumExtender;
using System.Collections;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;


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
        public static CustomWorldScript script;
        public static CustomWorldConfig config;
        public static CustomWorldOption customWorldOption;

        public CustomWorldMod()
        {
            ModID = "Custom Regions Mod";
            Version = "0.6." + version;
            author = "Garrakx";
        }

        // Code for AutoUpdate support
        // Should be put in the main PartialityMod class.

        // Update URL - don't touch!
        public string updateURL = "http://beestuff.pythonanywhere.com/audb/api/mods/3/0";
        public int version = 31;

        // Public key in base64 - don't touch!
        public string keyE = "AQAB";
        public string keyN = "13Mr+YOzb1iLnJvzkuP4NEZEWwOtWKWvWAN0HdsQ5SF2+RG7k8FbtmQut+2+69ideiJHDW66jWBcGGvfiQ0+5yLAUBpGSckC7V79yZgFQT39lvgU0ykAjonkA+ZTODFnehubyCkrrrzwno4boZghEZmDS2YsSyDJ6RLJyD2/WeCokcTj1vIHZhY9DzkooFtejz9yI/PCZtq8tfq2AzSiQPS+0xGQs3fnAkOGoV1WZ/inW5/rRyjD5HICr8t79UmcopfRK383YBrf2G96HeVYvY2vwSS/BW/m32rTLOZHr+XX7SIZshz7BLK6xEssy4qXjskvAUshqNudxtQnIkShGJuKWF1V2vvwqgY/IZiAbDXdBOUaSd09ldHBlTz9EfzBcgqffVRaUTzS71yGLISyrLriezozlK1YZW9vvijpbD0rmDaJ4aq9s6EzhdgVkTEuChtm/Fj9pgsswjvkbgHw1t9QZWqu4pweNd3IE/Lktst8HBKLiw1aRaffbZIhh1apbyjF8iflD8sNzbIHEfEvc35MEwIFqibJVnVxppBa15HpOxeXOzwuTjFaLSURRvbOEFPmpyd1Nm4nMzZZHHPjQXT7oYQAxjSCfqnLAdYsEnNo/2172jJGLfBWWGFTavqiCYqLhjtYkPfRgpcdw4FldgjX4w7RGMD/Ra5VXvmDMTE=";
        // ------------------------------------------------


        public override void OnEnable()
        {
            base.OnEnable();

            GameObject gameObject = new GameObject();
            script = gameObject.AddComponent<CustomWorldScript>();
            CustomWorldScript.mod = this;

            config = default(CustomWorldConfig);

            script.Initialize();

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


        /// <summary>
        /// Array of lists containing loaded regions info for each saveslot
        /// </summary>
        public static List<RegionInformation>[] regionInfoInSaveSlot = new List<RegionInformation>[3];

        /// <summary>
        /// Array of SaveProblems for each saveslot
        /// </summary>
        public static SaveProblems[] saveProblems = new SaveProblems[3];

        // Used for config screen
        public static string analyzingLog;

        public static ThumbnailDownloader thumbnailDownloader;

        /// <summary>
        /// Enum used in the mergin process when loading the world_XX.txt file.
        /// </summary>
        public enum MergeStatus
        {
            ROOMS,
            CREATURES,
            BATS
        }

        /// <summary>
        /// Struct with information of available regions 
        /// [regionID, regionName, description, activated, checksum, loadOrder(Default is 100)]
        /// </summary>
        public struct RegionInformation
        {
            public string regionID;
            public string regionName;
            public int regionNumber;
            public string description;
            public bool activated;
            public string checksum;
            public int loadOrder;
            public string folderName;
            public string url;
            public Dictionary<string, float> electricGates;
            //public Dictionary<string, CustomPearl> customPearls;

            public RegionInformation(string regionID, string regionName, string description, bool activated, 
                int loadOrder, string checksum, int regionNumber, string folderName, string url,
                Dictionary<string, float> electricGates, Dictionary<string, CustomPearl> customPearls)
            {
                this.regionID = regionID;
                this.regionName = regionName;
                this.description = description;
                this.activated = activated;
                this.checksum = checksum;
                this.loadOrder = loadOrder;
                this.regionNumber = regionNumber;
                this.folderName = folderName;
                this.url = url;
                this.electricGates = electricGates;
                //this.customPearls = customPearls;
            }
        }

        /// <summary>
        /// Struct with information of world lines, used in region merging and loading.
        /// [Data: holds the line itself, Vanilla: comes from vanilla or is it modified, modID: last mod which loaded or modified the line (empty if vanilla)]
        /// </summary>
        public struct WorldDataLine
        {
            public string data;
            public bool vanilla;
            public string modID;
            public WorldDataLine(string data, bool vanilla)
            {
                this.data = data;
                this.vanilla = vanilla;
                this.modID = string.Empty;
            }
            public WorldDataLine(string data, bool vanilla, string modID)
            {
                this.data = data;
                this.vanilla = vanilla;
                this.modID = modID;
            }
        }

        /// <summary>
        /// Struct with information of creature lines, used in region merging and loading.
        /// </summary>
        public struct CreatureLine
        {
            public bool lineage;
            public string room;
            public string[] connectedDens;
            public string dens;
            public int denNumber;

            public CreatureLine(bool lineage, string room, string[] connectedDens)
            {
                this.lineage = lineage;
                this.room = room;
                this.connectedDens = connectedDens;

                this.dens = null;
                this.denNumber = -1;
            }

            public CreatureLine(bool lineage, string room, string dens, int denNumber)
            {
                this.lineage = lineage;
                this.room = room;
                this.dens = dens;
                this.denNumber = denNumber;

                this.connectedDens = null;
            }
        }

        /// <summary>
        /// Struct with information about problems with save.
        /// </summary>
        public struct SaveProblems
        {
            public bool loadOrder;
            public bool installedRegions;
            public List<string> checkSum;
            public List<string> extraRegions;
            public List<string> missingRegions;

            public SaveProblems(bool differntLoadOrder, bool differentInstalledRegion, List<string> problematicCheckSum, List<string> extraReg, List<string> misReg)
            {
                this.loadOrder = differntLoadOrder;
                this.installedRegions = differentInstalledRegion;
                this.checkSum = problematicCheckSum;
                this.extraRegions = extraReg;
                this.missingRegions = misReg;
            }

            public bool anyProblems => this.loadOrder || this.installedRegions || (this.checkSum != null && this.checkSum.Count > 0);

        }

        public struct CustomPearl
        {
            public string name;
            public int ID;
            public Color color;
            public Color? secondaryColor;
            public string regionID;

            public CustomPearl(string name, int iD, Color color, Color? secondaryColor, string regionID)
            {
                this.name = name;
                this.ID = iD;
                this.color = color;
                this.secondaryColor = secondaryColor;
                this.regionID = regionID;
            }
        }

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
        /// <returns>returns a Dictionary(string, string)</returns>
        public static Dictionary<string, string> BuildModRegionsDictionary()
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
                        CustomWorldMod.RegionInformation infoRegionUpdated = entry.Value;
                        infoRegionUpdated.regionNumber = regionNumber;

                        updatedEntry.Add(infoRegionUpdated.regionID, infoRegionUpdated);
                        regionNumber++;

                        dictionary.Add(entry.Value.regionID, entry.Value.folderName);
                    }
                    else
                    {
                        updatedEntry.Add(entry.Key, entry.Value);
                    }
                }

                catch (Exception e) { CustomWorldMod.CustomWorldLog($"Custom Regions: Error while trying to add customRegion: {e}"); }
            }

            CustomWorldMod.availableRegions = updatedEntry;
            return dictionary;
        }

        public static void CreateCustomWorldLog()
        {
            using (StreamWriter sw = File.CreateText(Custom.RootFolderDirectory() + "customWorldLog.txt"))
            {
                sw.WriteLine("############################################\n Custom World Log \n");
            }
        }

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

        public static void CustomWorldLog(string test)
        {
            if (!File.Exists(Custom.RootFolderDirectory() + "customWorldLog.txt"))
            {

                // Create a file to write to.
                /*
                using (StreamWriter sw = File.CreateText(Custom.RootFolderDirectory() + "customWorldLog.txt"))
                {
                    sw.WriteLine("######################\n Custom World Log\n");
                }
                */
                CreateCustomWorldLog();
            }

            using (StreamWriter file =
            new StreamWriter(Custom.RootFolderDirectory() + "customWorldLog.txt", true))
            {
                file.WriteLine(test);
            }

        }

        public static void CustomWorldLog(string test, bool throwException)
        {
            CustomWorldLog(test);
            Debug.LogError(test);
        }

        /// <summary>
        /// Builds available regions, loaded regions and SaveAnalyzer
        /// </summary>
        public static void LoadCustomWorldResources()
        {
            CustomWorldMod.LoadAvailableRegions();

            CustomWorldMod.loadedRegions = CustomWorldMod.BuildModRegionsDictionary();

            // Debug
            string dictionaryString = "Custom Regions: Loading {";
            dictionaryString += $"{ string.Join(", ", new List<string>(CustomWorldMod.loadedRegions.Values).ToArray())}" + "}";
            CustomWorldMod.CustomWorldLog(dictionaryString);


            CustomWorldMod.ReadSaveAnalyzer();
            CustomWorldMod.AnalyzeSave();
        }

        private static void AnalyzeSave()
        {
            // foreach(List<RegionInformation> infoSave in regionInfoInSaveSlot)
            if (regionInfoInSaveSlot == null)
            {
                return;
            }

            for (int saveSlot = 0; saveSlot < regionInfoInSaveSlot.Length; saveSlot++)
            {
                try
                {
                    if (regionInfoInSaveSlot[saveSlot] == null)
                    {
                        continue;
                    }

                    saveProblems[saveSlot] = new SaveProblems(false, false, new List<string>(), new List<string>(), new List<string>());

                    List<string> savedRegions = new List<string>();
                    foreach(RegionInformation info in regionInfoInSaveSlot[saveSlot])
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
                        (saveProblems[saveSlot].extraRegions != null && saveProblems[saveSlot].extraRegions.Count != 0) )
                    {
                        saveProblems[saveSlot].installedRegions = true;
                    }


                    /*
                    else
                    {
                        // Compare region order
                        for (int i = 0; i < savedRegions.Count; i++)
                        {
                            // Compare checksum
                            if (regionInfoInSaveSlot[saveSlot].Find(x => x.regionID == savedRegions[i]).checksum != availableRegions[loadedRegions[savedRegions[i]]].checksum)
                            {
                                saveProblems[saveSlot].checkSum.Add((regionInfoInSaveSlot[saveSlot].Find(x => x.regionID == savedRegions[i]).checksum));
                            }
                            string temp = string.Empty;

                            if (!loadedRegions.TryGetValue(savedRegions[i], out temp) || savedRegions[i] != temp)
                            {
                                saveProblems[saveSlot].loadOrder = true;
                            }
                        }
                    }*/

                }
                catch (Exception e)
                {
                    Debug.Log(e + "SaveSlot " + saveSlot);
                }
            }
        }

        public static void ReadSaveAnalyzer()
        {
            for (int saveSlot = 0; saveSlot < 3; saveSlot++)
            {

                string saveFileName = Custom.RootFolderDirectory() + CustomWorldMod.regionSavePath + $"CRsav_{saveSlot + 1}.txt";

                regionInfoInSaveSlot[saveSlot] = new List<RegionInformation>();

                if(!File.Exists(Custom.RootFolderDirectory() + "UserData"+Path.DirectorySeparatorChar+ ((saveSlot != 0) ? ("sav_" + (saveSlot + 1)) : "sav") + ".txt"))
                {
                    File.Delete(saveFileName);
                    CustomWorldLog($"Deleting {saveFileName} since vanilla save is empty");
                    return;
                }

                if (File.Exists(saveFileName))
                {
                    string allText = File.ReadAllText(saveFileName);
                    string sum = allText.Substring(0, 32);
                    allText = allText.Substring(32, allText.Length - 32);

                    if (Custom.Md5Sum(allText).Equals(sum))
                    {
                        CustomWorldLog($"SaveSlot [{saveSlot + 1}] found! Correct checksum");
                    }
                    else
                    {
                        File.Delete(saveFileName);
                        CustomWorldLog("CR Save was tinkered! Why did you touch this?");
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
                    CustomWorldLog($"SaveSlot [{saveSlot+1}] does not have CR information");
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
                    CustomWorldMod.CustomWorldLog($"Custom Regions: Added new region [{regionToAdd}] from [{keyValues.Value}].");
                }
            }
            return regionNames;
        }


        /// <summary>
        /// Compares and merges a room-connection in the existing room list
        /// </summary>
        public static List<WorldDataLine> AddNewRoom(string newRoom, List<WorldDataLine> oldList, string modID)
        {
            // Check if this connection already exists
            bool sameConnections = false;

            // Holds the old line which needs to be merged or replaced
            string roomConnectionsToBeReplaced = string.Empty;

            foreach (WorldDataLine oldRoom in oldList)
            {
                if (oldRoom.data.Equals(newRoom))
                {
                    // The room connetions are exactly the same, skipped
                    sameConnections = true;
                    roomConnectionsToBeReplaced = string.Empty;
                    break;
                }
                else
                {
                    string oldBaseRoom = oldRoom.data.Substring(0, oldRoom.data.IndexOf(" "));
                    string newBaseRoom = newRoom.Substring(0, newRoom.IndexOf(" "));

                    if (oldBaseRoom.Equals(newBaseRoom))
                    {
                        // The room is the same but different connections
                        roomConnectionsToBeReplaced = oldRoom.data;

                        CustomWorldMod.CustomWorldLog($"Custom Regions: Found conflict. Existing room[{roomConnectionsToBeReplaced}] with room to be added [{newRoom}]");
                    }
                }
            }

            // Found a connection that needs to be replaced or merged
            if (roomConnectionsToBeReplaced != string.Empty)
            {

                CustomWorldMod.CustomWorldLog($"Custom Regions: Trying to merge [{roomConnectionsToBeReplaced}]");

                // Extract room name from line to be merged / replaced
                string roomToBeReplacedName = roomConnectionsToBeReplaced.Substring(0, roomConnectionsToBeReplaced.IndexOf(" "));

                // Check if the room to be merged / replaced is vanilla
                bool isVanilla = oldList.Find(x => x.data.Equals(roomConnectionsToBeReplaced)).vanilla;

                // Check if containts GATE/SHELTER/SWARMROOM at the end
                string endingSetting = string.Empty;
                string temp = roomConnectionsToBeReplaced.Substring(roomConnectionsToBeReplaced.IndexOf(": ") + 2);
                if (temp.IndexOf(": ") > 0)
                {
                    endingSetting = temp.Substring(temp.IndexOf(": ") + 2);
                }


                //----------------------
                // Build new connections
                //----------------------

                // Convert the line strings into Lists so it is easier to manipulate and read
                List<string> oldConnections = FromConnectionsToList(roomConnectionsToBeReplaced);
                List<string> newConnections = FromConnectionsToList(newRoom);

                // if isRoomBeingReplace is false, it means it got merged.
                bool isRoomBeingReplaced = false;

                // if allEmpty is true, there are not disconnnected pipes in the room
                bool noDisconnectedPipes = true;

                // Check if any operation took place
                bool performedOperation = false;


                ///------------------------------------------------------------------------------------------------------------
                /// HOW IT WORKS (subject to change)
                /// 
                /// If the mod is modifying a vanilla room, it will replace it completly. 
                /// Modders should make sure they don't occuppy all the pipes for that room.
                /// 
                /// If the mod is modifying a room that is either modded or modified by another mod, CR will try to merge both.
                /// 
                /// If this room does not have any empty exits, the two mods will be incompatible.
                /// 
                ///------------------------------------------------------------------------------------------------------------


                if (isVanilla)
                {
                    // A mod omitted to include DISCONNECTED in their lines, adding it to increase compability
                    if (newConnections.Count < oldConnections.Count)
                    {
                        for (int i = 0; i < (oldConnections.Count - newConnections.Count); i++)
                        {
                            newConnections.Add("DISCONNECTED");
                            performedOperation = true;
                        }
                        CustomWorldMod.CustomWorldLog($"Custom Regions: ERROR! Added compability to [{newRoom}]! It has less connections than vanilla! [{roomConnectionsToBeReplaced}]");
                        CustomWorldMod.CustomWorldLog($"Custom Regions: Converted to [{string.Join(", ", newConnections.ToArray())}]");
                    }

                    // The mod will replace the old lines since they are vanilla.
                    isRoomBeingReplaced = true;
                }

                else // Only merge if it is between two mods
                {
                    for (int j = 0; j < newConnections.Count; j++)
                    {
                        for (int i = 0; i < oldConnections.Count; i++)
                        {
                            if (!oldConnections[i].Equals(newConnections[j]))
                            {
                                // old connection has a DISCONNECTED / DISCONNECT pipe
                                bool oldConnectionDisconnected = oldConnections[i].Equals("DISCONNECTED") || oldConnections[i].Equals("DISCONNECT");

                                // new connection has a DISCONNECTED / DISCONNECT pipe
                                bool newConnectionDisconnected = newConnections[j].Equals("DISCONNECTED") || newConnections[j].Equals("DISCONNECT");

                                // The room to be merged has empty exits, so new mod will use those
                                if (oldConnectionDisconnected && !newConnectionDisconnected)
                                {
                                    if (!oldConnections.Contains(newConnections[j]))
                                    {
                                        CustomWorldMod.CustomWorldLog($"Custom Regions: Replaced disconnected [{oldConnections[i]}] with [{newConnections[j]}]");
                                        oldConnections[i] = newConnections[j];
                                        noDisconnectedPipes = false;
                                        performedOperation = true;
                                        break;
                                    }
                                }

                                // If the room is Vanilla, mod can replace pipes with DISCONNECTED
                                /*else if (isVanilla && newConnectionDisconnected)
                                {
                                    CustomWorldMod.CustomWorldLog($"Custom Regions: Replaced vanilla [{oldConnections[i]}] with disconnected [{newConnections[j]}]");
                                    oldConnections[i] = newConnections[j];
                                    noDisconnectedPipes = false;
                                }*/

                                /* else
                                 {
                                     // room will be completly replaced
                                     isRoomBeingReplaced = true;
                                 }*/
                            }
                        }
                    }
                }


                CustomWorldMod.CustomWorldLog($"Custom Regions: Analized old room [{roomConnectionsToBeReplaced}]. Added by a mod? [{isVanilla}]. NewRoomConnections [{string.Join(", ", newConnections.ToArray())}]. IsBeingReplaced [{isRoomBeingReplaced}]. No Empty Pipes [{noDisconnectedPipes}]");

                if (roomConnectionsToBeReplaced.ToUpper().Contains("DISCONNECTED") && roomConnectionsToBeReplaced.ToUpper().Contains("DISCONNECT"))
                {
                    noDisconnectedPipes = true;
                }

                // No empty pipes but room needs to be replaced. Whole line will be replaced
                if (isVanilla)
                {
                    if (isRoomBeingReplaced && noDisconnectedPipes)
                    {
                        CustomWorldMod.CustomWorldLog($"Custom Regions: Comparing two rooms without disconnected pipes. [{roomConnectionsToBeReplaced}] is vanilla: [{isVanilla}]. with [{string.Join(", ", newConnections.ToArray())}]");
                        oldConnections = newConnections;
                        performedOperation = true;
                    }
                }
                else if (!performedOperation)
                {

                    if (noDisconnectedPipes/*!errorLog.ToUpper().Contains("DISCONNECTED") && !errorLog.ToUpper().Contains("DISCONNECT")*/)
                    {
                        string errorLog = $"#Found possible incompatible room [{roomToBeReplacedName} : {string.Join(", ", newConnections.ToArray())}] from [{modID}] and [{roomConnectionsToBeReplaced}] from [{oldList.Find(x => x.data.Equals(roomConnectionsToBeReplaced)).modID}]. \\n If [{roomConnectionsToBeReplaced}] is from the vanilla game everything is fine. Otherwise you might be missing compatibility patch.";
                        analyzingLog += errorLog + "\n\n";
                        CustomWorldMod.CustomWorldLog("Custom Regions: ERROR! " + errorLog);
                        UnityEngine.Debug.LogError(errorLog);
                        //UnityEngine.Debug.LogError($"Found two incompatible region mods: {modID} <-> {oldList.Find(x => x.data.Equals(roomConnectionsToBeReplaced)).modID}");
                    }
                }

                // A merging / replacement got place, so add changes to world lines.
                if (isRoomBeingReplaced || !noDisconnectedPipes || performedOperation)
                {
                    endingSetting = endingSetting != string.Empty ? " : " + endingSetting : "";

                    // Convert from list to connections
                    string updatedConnections = FromListToConnectionsString(roomConnectionsToBeReplaced.Substring(0, roomConnectionsToBeReplaced.IndexOf(" ")), oldConnections);

                    int index = oldList.IndexOf(oldList.Find(x => x.data.Equals(roomConnectionsToBeReplaced)));
                    if (index != -1)
                    {
                        if (oldList[index].data != (updatedConnections + endingSetting))
                        {
                            CustomWorldMod.CustomWorldLog($"Custom Regions: Replaced [{oldList[index].data}] with [{updatedConnections + endingSetting}]");
                            oldList[index] = new WorldDataLine(updatedConnections + endingSetting, false, modID);
                        }
                    }
                }

            }
            // Mod added a completly new connection
            else
            {
                if (!sameConnections)
                {
                    CustomWorldMod.CustomWorldLog($"Custom Regions: Added new room [{newRoom}]");
                    oldList.Add(new WorldDataLine(newRoom, false));
                }
            }

            return oldList;
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
        /// Returns a string from a connectionList
        /// </summary>
        public static string FromListToConnectionsString(string oldRoom, List<string> connections)
        {
            oldRoom += " : ";
            int a = 0;
            foreach (string room in connections)
            {
                if (a == 0)
                {
                    oldRoom += room;
                }
                else
                {
                    oldRoom += ", " + room;
                }
                a++;
            }
            //CustomWorldMod.CustomWorldLog($"Custom Regions: FromListToConnection: [{oldRoom}]");
            return oldRoom;
        }


        internal static List<WorldDataLine> AddNewBatBlockage(string newBlockage, List<WorldDataLine> oldBatsLine)
        {
            bool sameBlockage = false;

            foreach (WorldDataLine oldBlockage in oldBatsLine)
            {
                if (oldBlockage.data.Equals(newBlockage))
                {
                    // The blockages are exactly the same, skipped
                    sameBlockage = true;
                    break;
                }
            }

            if (!sameBlockage)
            {
                CustomWorldMod.CustomWorldLog($"Custom Regions: Added new blockage [{newBlockage}]");
                oldBatsLine.Add(new WorldDataLine(newBlockage, false));

            }
            return oldBatsLine;
        }

        /// <summary>
        /// Returns a List from a room-connection string
        /// This method should be heavily optimized and cleaned up.
        /// </summary>
        public static List<string> FromConnectionsToList(string oldConnections)
        {
            // CustomWorldMod.CustomWorldLog($"Custom Regions: Trying to split [{oldConnections}]");
            List<string> connections = new List<string>();

            string[] split = Regex.Split(oldConnections, " : ");
            int position = 0;

            if (split.Length >= 2)
            {
                // Remove base room
                position = 1;
            }

            string[] split_rooms = Regex.Split(split[position], ", ");

            foreach (string s in split_rooms)
            {
                if (s.Trim() != "")
                {
                    connections.Add(s);
                }
            }
            // updatedConnections = updatedConnections.Where(x => !string.IsNullOrEmpty(x)).ToArray();

            //CustomWorldMod.CustomWorldLog($"Custom Regions: FromConnectionToList [{string.Join(",", connections.ToArray())}]");

            return connections;
        }


        internal static List<WorldDataLine> AddNewCreature(string newCreatureLine, List<WorldDataLine> oldCreaturesSpawns)
        {
            bool sameCreatureLine = false;
            string creatureLineBeReplaced = string.Empty;


            bool lineage = false;
            string roomNameNewLine = string.Empty;

            CustomWorldMod.CustomWorldLog($"Custom Regions: Adding new creature spawn [{newCreatureLine}]]");

            if (newCreatureLine.Contains("OFFSCREEN"))
            {
                //creatureLineBeReplaced = newCreatureLine;
                creatureLineBeReplaced = string.Empty;
                sameCreatureLine = false;
            }
            else
            {

                if (newCreatureLine.Contains("LINEAGE"))
                {
                    lineage = true;
                    roomNameNewLine = Regex.Split(newCreatureLine, " : ")[1];
                }
                else
                {
                    roomNameNewLine = Regex.Split(newCreatureLine, " : ")[0];
                }


                foreach (WorldDataLine oldSpawnLine in oldCreaturesSpawns)
                {

                    if (oldSpawnLine.data.Contains(newCreatureLine))
                    {
                        // The spawn is exactly the same, skipped
                        sameCreatureLine = true;
                        creatureLineBeReplaced = string.Empty;
                        break;
                    }
                    else
                    {

                        //CustomWorldMod.CustomWorldLog($"Custom Regions: Splitting [{newCreatureLine}]");
                        //CustomWorldMod.CustomWorldLog($"Custom regions: Testing Creature listing [{string.Join(",", oldLinesList.ToArray())}]");

                        string[] array = Regex.Split(oldSpawnLine.data, " : ");
                        string oldRoomName = array[0];

                        if (oldSpawnLine.data.Contains("OFFSCREEN"))
                        {
                            continue;
                        }

                        if (lineage)
                        {
                            if (!oldSpawnLine.data.Contains("LINEAGE"))
                            {
                                continue;
                            }
                            else
                            {
                                oldRoomName = array[1];
                            }
                        }
                        // CustomWorldMod.CustomWorldLog($"Custom Regions: Comparing [{oldLinesList[roomIndex]}] with [{roomNameNewLine}]");


                        /* if (lineage)
                         {
                             if(oldRoomName.Equals(roomNameNewLine))
                             {
                                 if (array[2] == Regex.Split(newCreatureLine, " : ")[2])
                                 {
                                     // Found same LINEAGE
                                     creatureLineBeReplaced = string.Empty;
                                     sameCreatureLine = true;
                                     break;
                                 }
                                 creatureLineBeReplaced = oldSpawnLine.data;
                             }
                         }
                         else*/
                        {
                            // Adding creatures to new room, check for collision
                            if (oldRoomName.Equals(roomNameNewLine))
                            {
                                creatureLineBeReplaced = oldSpawnLine.data;
                            }

                        }




                        //CustomWorldMod.CustomWorldLog($"Custom Regions: Analyzing [{newCreatureLine}]. Room Name [{roomName}]");


                    }


                }
            }
            if (creatureLineBeReplaced == string.Empty)
            {
                if (!sameCreatureLine)
                {
                    oldCreaturesSpawns.Add(new WorldDataLine(newCreatureLine, false));
                }
            }
            else
            {

                CreatureLine newLines = FillCreatureLine(newCreatureLine);
                CreatureLine oldLines = FillCreatureLine(creatureLineBeReplaced);

                bool isVanilla = oldCreaturesSpawns.Find(x => x.data.Equals(creatureLineBeReplaced)).vanilla;
                int index = oldCreaturesSpawns.IndexOf(oldCreaturesSpawns.Find(x => x.data.Equals(creatureLineBeReplaced)));

                CustomWorldMod.CustomWorldLog($"Custom Regions: Trying to merge creature [{newCreatureLine}] with [{creatureLineBeReplaced}] (vanilla [{isVanilla}])");

                // This bit might be redundant
                if (lineage && oldLines.lineage && newLines.lineage)
                {
                    if (oldLines.denNumber != newLines.denNumber)
                    {
                        oldCreaturesSpawns.Add(new WorldDataLine(newCreatureLine, false));
                    }
                    else if (isVanilla)
                    {
                        if (index > -1)
                        {
                            oldCreaturesSpawns[index] = new WorldDataLine(newCreatureLine, false);
                        }
                    }

                }
                else
                {
                    string[] updatedConnections = new string[1];
                    string updatedCreatureLine;

                    bool empty = true;
                    for (int i = 0; i < newLines.connectedDens.Length; i++)
                    {
                        bool shouldAdd = false;
                        if (newLines.connectedDens[i] != null)
                        {
                            try
                            {
                                if (oldLines.connectedDens.Length <= i || oldLines.connectedDens[i] == null)
                                {
                                    CustomWorldMod.CustomWorldLog($"Custom Regions: Empty pipe, filling with [{newLines.connectedDens[i]}]"); ;
                                    shouldAdd = true;
                                }
                                else if (isVanilla)
                                {
                                    CustomWorldMod.CustomWorldLog($"Custom Regions: replacing vanilla pipe [{oldLines.connectedDens[i]}] with [{newLines.connectedDens[i]}]");
                                    empty = false;
                                    shouldAdd = true;
                                }
                            }
                            catch (Exception) { shouldAdd = true; }
                        }

                        if (shouldAdd)
                        {
                            if (updatedConnections.Length <= i)
                            {
                                Array.Resize(ref updatedConnections, i + 1);
                            }
                            updatedConnections[i] = newLines.connectedDens[i];
                        }
                    }

                    for (int a = 0; a < oldLines.connectedDens.Length; a++)
                    {
                        try
                        {
                            if (updatedConnections.Length <= a)
                            {
                                Array.Resize(ref updatedConnections, a + 1);
                            }

                            if (updatedConnections[a] == null && (oldLines.connectedDens[a] != null))
                            {
                                updatedConnections[a] = oldLines.connectedDens[a];
                            }
                        }
                        catch (Exception) { }
                    }

                    // Remove empty slots
                    updatedConnections = updatedConnections.Where(x => !string.IsNullOrEmpty(x)).ToArray();

                    updatedCreatureLine = $"{oldLines.room} : {string.Join(", ", updatedConnections)}";

                    if (index > -1)
                    {
                        oldCreaturesSpawns[index] = new WorldDataLine(updatedCreatureLine, empty);
                    }
                }


            }


            return oldCreaturesSpawns;
        }

        /// <summary>
        /// Create CreatureLine struct from string line. Used in Creature merging
        /// </summary>
        public static CreatureLine FillCreatureLine(string lines)
        {
            string roomName = string.Empty;
            string[] connectedDens = new string[0];

            string[] line = Regex.Split(lines, " : ");

            if (line[0] == "LINEAGE")
            {
                roomName = line[1];
                //CustomWorldMod.CustomWorldLog($"Custom Regions: Creating creature line. Lineage[{true}]. RoonName[{roomName}]. Spawners[{line[3]}]. DenNumber[{int.Parse(line[2])}]");
                return new CreatureLine(true, roomName, line[3], int.Parse(line[2]));

            }
            else
            {
                roomName = line[0];

                string[] array = Regex.Split(line[1], ", ");

                for (int i = 0; i < array.Length; i++)
                {
                    int denNumber = (int)char.GetNumericValue(array[i][0]);

                    if (denNumber >= connectedDens.Length)
                    {
                        Array.Resize(ref connectedDens, denNumber + 1);
                    }

                    connectedDens[denNumber] = array[i];
                }

                //CustomWorldMod.CustomWorldLog($"Custom Regions: Creating creature line. Lineage[{false}]. RoonName[{roomName}]. Spawners[{connectedDens.Length}]");
                return new CreatureLine(false, roomName, connectedDens);

            }

        }

        /// <summary>
        /// Returns vanilla world file, or other CustomWorld mod file if there is one
        /// </summary>
        /// <returns>Vanilla World path</returns>
        public static string FindVanillaRoom(string roomName, bool includeRootDirectory)
        {
            string result = "";

            string gatePath = Custom.RootFolderDirectory() + "World" + Path.DirectorySeparatorChar + "Gates" + Path.DirectorySeparatorChar + roomName;
            string gateShelterPath = Custom.RootFolderDirectory() + "World" + Path.DirectorySeparatorChar + "Gates" + Path.DirectorySeparatorChar + "Gate shelters" + Path.DirectorySeparatorChar + roomName;
            string regularRoomPath = Custom.RootFolderDirectory() + "World" + Path.DirectorySeparatorChar + "Regions" + Path.DirectorySeparatorChar + Regex.Split(roomName, "_")[0];
            string arenaPath = Custom.RootFolderDirectory() + "Levels" + Path.DirectorySeparatorChar + roomName;

            // room is regular room
            if (Directory.Exists(regularRoomPath) && File.Exists(regularRoomPath + Path.DirectorySeparatorChar + "Rooms" + Path.DirectorySeparatorChar + roomName + ".txt"))
            {
                result = Custom.RootFolderDirectory() + "World" + Path.DirectorySeparatorChar + "Regions" + Path.DirectorySeparatorChar + Regex.Split(roomName, "_")[0] + Path.DirectorySeparatorChar + "Rooms" + Path.DirectorySeparatorChar + roomName;
                //CustomWorldMod.CustomWorldLog($"Custom Regions: Found room {roomName} in {keyValues.Key}. Path: {result}");
            }
            // room is GATE
            else if (Regex.Split(roomName, "_")[0] == "GATE" && File.Exists(Custom.RootFolderDirectory() + gatePath + ".txt"))
            {
                result = gatePath;
                //CustomWorldMod.CustomWorldLog($"Custom Regions: Found gate {roomName} in {keyValues.Key}. Path: {result}");
            }
            // room is Gate shelter
            else if (File.Exists(Custom.RootFolderDirectory() + gateShelterPath + ".txt"))
            {
                result = gateShelterPath;
                //CustomWorldMod.CustomWorldLog($"Custom Regions: Found gate_shelter {roomName} in {keyValues.Key}. Path: {result}");
            }
            // room is Arena
            else if (File.Exists(Custom.RootFolderDirectory() + arenaPath + ".txt"))
            {
                result = arenaPath;
                //CustomWorldMod.CustomWorldLog($"Custom Regions: Found arena {roomName} in {keyValues.Key}. Path: {result}");
            }

            // CustomWorldMod.CustomWorldLog("Using Custom Worldfile: " + result);
            if (includeRootDirectory)
            {
                result = "file:///" + Custom.RootFolderDirectory() + result;
            }
            return result;
        }

        /// <summary>
        /// Builds the world from the merged world_XX.txt files.
        /// </summary>
        /// <returns>Returns a List<string> with room connections, creatures and bat migration blockages.</returns>
        internal static List<string> BuildWorldText(List<string> ROOMS, List<string> CREATURES, List<string> BATS)
        {
            List<string> list = new List<string>();

            string startRooms = "ROOMS";
            string endRooms = "END " + startRooms;

            string startCreatures = "CREATURES";
            string endCreatures = "END " + startCreatures;

            string startBats = "BAT MIGRATION BLOCKAGES";
            string endBats = "END " + startBats;

            // ROOMS
            list.Add(startRooms);
            foreach (string room in ROOMS)
            {
                list.Add(room);
            }
            list.Add(endRooms);

            // CREATURES
            list.Add(startCreatures);
            foreach (string creature in CREATURES)
            {
                list.Add(creature);
            }
            list.Add(endCreatures);

            // BATS
            list.Add(startBats);
            foreach (string bats in BATS)
            {
                list.Add(bats);
            }
            list.Add(endBats);

            return list;

        }

        public static object GetRegionInfoJson(string key, Dictionary<string, object> dictionary)
        {
            object value = null;
            if ((dictionary.ContainsKey(key)))
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
            CustomWorldMod.availableRegions = new Dictionary<string, CustomWorldMod.RegionInformation>();
            CustomWorldMod.customPearls = new Dictionary<string, CustomPearl>();

            string path = Custom.RootFolderDirectory() + CustomWorldMod.resourcePath;
            Dictionary<string, RegionInformation> notSortedDictionary = new Dictionary<string, RegionInformation>();
            // For each Region Mod Installed
            foreach (string dir in Directory.GetDirectories(path))
            {
                string pathOfRegionInfo = dir + Path.DirectorySeparatorChar + "regionInfo.json";

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
                    regionName = Path.GetFileNameWithoutExtension(dir);

                    // If upgrading from old CR version
                    if (File.Exists(dir + Path.DirectorySeparatorChar + "regionID.txt"))
                    {
                        regionID = File.ReadAllText(dir + Path.DirectorySeparatorChar + "regionID.txt");
                        regionID = regionID.ToUpper();

                        activated = true;

                        File.Delete(dir + Path.DirectorySeparatorChar + "regionID.txt");
                        CustomWorldMod.CustomWorldLog($"Custom Regions: Updating regionID from old CR version... Obtained regionID [{regionID}]");
                    }

                    // regionID.txt did not exist or was empty
                    if (regionID == string.Empty)
                    {
                        string regionsPath = dir + Path.DirectorySeparatorChar + "World" + Path.DirectorySeparatorChar + "Regions";
                        CustomWorldMod.CustomWorldLog($"Custom Regions: Empty regionID, obtaining from [{regionsPath}]. Valid [{Directory.Exists(regionsPath)}]");

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

                        if (regionID == string.Empty)
                        {
                            // If a customRegion does not add new regions, obtain regionID from capital letters.
                            foreach (char letters in regionName)
                            {
                                if (char.IsUpper(letters))
                                {
                                    regionID += letters;
                                }
                            }
                        }
                    }

                    checksum = CustomWorldMod.GenerateRegionCheckSum(dir);

                    WriteRegionInfoJSONFile(dir, regionID, description, regionName, activated, loadOrder, url, checksum);

                }

                

                RegionInformation regionInformation = new RegionInformation(string.Empty, string.Empty, "No description", 
                    true, loadOrder, string.Empty, -1, new DirectoryInfo(dir).Name, string.Empty, new Dictionary<string, float>(), new Dictionary<string, CustomPearl>());


                Dictionary<string, object> dictionary = File.ReadAllText(pathOfRegionInfo).dictionaryFromJson();
                if (dictionary != null)
                {
                    if (GetRegionInfoJson("regionID", dictionary) != null)
                    {
                        regionInformation.regionID = (string)GetRegionInfoJson("regionID", dictionary);
                    }

                    if (GetRegionInfoJson("description", dictionary) != null)
                    {

                        regionInformation.description = (string)GetRegionInfoJson("description", dictionary);
                    }

                    if (GetRegionInfoJson("regionName", dictionary) != null)
                    {
                        regionInformation.regionName = (string)GetRegionInfoJson("regionName", dictionary);
                    }

                    if (dictionary.ContainsKey("activated"))
                    {
                        regionInformation.activated = dictionary["activated"].ToString().ToLower().Contains("true");
                    }

                    if (GetRegionInfoJson("loadOrder", dictionary) != null)
                    {
                        regionInformation.loadOrder = int.Parse(GetRegionInfoJson("loadOrder", dictionary).ToString());
                    }

                    if (GetRegionInfoJson("checksum", dictionary) != null)
                    {
                        regionInformation.checksum = (string)GetRegionInfoJson("checksum", dictionary);
                    }
                    if (GetRegionInfoJson("checksum", dictionary) != null)
                    {
                        regionInformation.url = (string)GetRegionInfoJson("url", dictionary);
                    }


                    CustomWorldLog($"Description for ({regionInformation.regionName}) is: [{regionInformation.description}]");
                  //  string oldDescription = regionInformation.description;
                    if (regionInformation.description.Equals("N / A") || regionInformation.description.Equals(string.Empty))
                    {
                        regionInformation.description = "No description";
                    }

                    string newDescr = string.Empty;
                    string newUrl = string.Empty;

                    if (regionInformation.regionName.ToLower().Contains("aether ridge"))
                    {
                        newDescr = "Aether Ridge is derelict desalination rig to the north of Sky Islands. Includes over 200 new rooms, six new arenas, and more.";
                        newUrl = "http://www.raindb.net/previews/aether.png";
                    }
                    else if (regionInformation.regionName.ToLower().Contains("badlands"))
                    {
                        newDescr = "The Badlands is a region connecting Farm Arrays and Garbage Wastes. It features many secrets and unlockables, including three new arenas.";
                        newUrl = "http://www.raindb.net/previews/badlands.png";
                    }
                    else if (regionInformation.regionName.ToLower().Contains("root"))
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
                        CustomWorldLog($"New checksum for {regionInformation.regionName} [{newChecksum}]");
                        regionInformation.checksum = newChecksum;
                    }
                    if(!newDescr.Equals(string.Empty))
                    {
                        CustomWorldLog($"New description for {regionInformation.regionName} [{newDescr}]");
                        regionInformation.description = newDescr;
                    }
                    if(!newUrl.Equals(string.Empty))
                    {
                        CustomWorldLog($"New url for {regionInformation.regionName} [{url}]");
                        regionInformation.url = newUrl;
                    }

                    // Write new info
                    if ((!newDescr.Equals(string.Empty)&&regionInformation.description.Equals("No description")) || !newChecksum.Equals(string.Empty) || !newUrl.Equals(string.Empty))
                    {
                        CustomWorldLog($"Updating regionInfo for {regionInformation.regionName}");
                        File.Delete(dir + Path.DirectorySeparatorChar + "regionInfo.json");
                        WriteRegionInfoJSONFile(dir, regionInformation.regionID, regionInformation.description, regionInformation.regionName, regionInformation.activated, regionInformation.loadOrder, regionInformation.url, regionInformation.checksum);

                    }




                    // Load region information
                    CustomWorldMod.CustomWorldLog($"Custom Regions: Adding available region [{regionInformation.regionID}]. Activated [{regionInformation.activated}]");
                    if (regionInformation.regionID != string.Empty)
                    {
                        try
                        {
                            notSortedDictionary.Add(regionInformation.regionID, regionInformation);
                        }
                        catch (Exception dic) { CustomWorldMod.CustomWorldLog($"Custom Regions: Error in adding [{regionInformation.regionID}] => {dic}"); };
                    }

                    if (!Directory.Exists(Custom.RootFolderDirectory() + CustomWorldMod.resourcePath + regionInformation.folderName))
                    {
                        CustomWorldMod.CustomWorldLog($"CR could not find folder [{regionInformation.folderName}] from region [{regionInformation.regionID}]. Try removing any dots from the folder names and reloading.", true);
                    }
                }

                // Add electric gates (MOVE INSIDE LOAD CUSTOM EXTRAS
                string pathToElectricGates = dir + Path.DirectorySeparatorChar + "World" + Path.DirectorySeparatorChar + "Gates" + Path.DirectorySeparatorChar + "electricGates.txt";
                CustomWorldLog($"Loading electric gates for {regionID}");
                if (File.Exists(pathToElectricGates))
                {
                    string[] electricGates = File.ReadAllLines(pathToElectricGates);
                    for (int i = 0; i < electricGates.Length; i++)
                    {
                        string gateName = Regex.Split(electricGates[i], " : ")[0];
                        float meterHeigh = float.Parse(Regex.Split(electricGates[i], " : ")[1]);

                        CustomWorldLog($"Added new gate [{gateName}]. Meter height [{meterHeigh}]");
                        regionInformation.electricGates.Add(gateName, meterHeigh);
                    }
                }

                if (regionInformation.activated)
                {
                    LoadCustomExtras(dir, regionInformation.regionID);
                }

            }

            // Save each world data line in alphabetical order
            foreach (KeyValuePair<string, RegionInformation> element in notSortedDictionary.OrderBy(d => d.Value.loadOrder))
            {
                //element.Value.regionNumber = regionNumber;
                CustomWorldMod.availableRegions.Add(element.Key, element.Value);
            }
        }

        public static void LoadCustomExtras(string dir, string regionID)
        {
            // Add Custom Pearls
            string pathToPearls = dir + Path.DirectorySeparatorChar + "Assets" + Path.DirectorySeparatorChar + "pearlData.txt";
            CustomWorldLog($"Loading pearl data for {regionID}");
            if (File.Exists(pathToPearls))
            {
                string[] customPearlsLines = File.ReadAllLines(pathToPearls);
                for (int i = 0; i < customPearlsLines.Length; i++)
                {
                    string[] lineDivided = Regex.Split(customPearlsLines[i], " : ");
                    int fileNumber = 0; string pearlName = ""; Color pearlColor = new Color(0.7f, 0.7f, 0.7f); Color ?secondaryColor = new Color();

                    try
                    {
                      fileNumber = int.Parse(lineDivided[0]);
                      pearlName = $"{regionID}_{lineDivided[1]}";
                    } 
                    catch (Exception e)
                    {
                        CustomWorldLog("Error loading pearl information, missing pearl ID or pearl Name in pearlData.txt" + e, true);
                        continue;
                    }

                    try
                    {
                        pearlColor = OptionalUI.OpColorPicker.HexToColor(lineDivided[2]);
                    } catch (Exception) { CustomWorldLog($"Pearl missing color from {regionID}", true); }
                    try
                    {
                        secondaryColor = OptionalUI.OpColorPicker.HexToColor(lineDivided[3]);
                    }
                    catch (Exception) { CustomWorldLog($"Pearl missing highlighted from {regionID}"); }

                    CustomWorldMod.CustomWorldLog($"Added new pearl [{pearlName}/{fileNumber}/{pearlColor}]");
                    //regionInformation.customPearls.Add(pearlName, new CustomPearl(fileNumber, pearlColor));

                    CustomWorldMod.customPearls.Add(pearlName, new CustomPearl(pearlName, fileNumber, pearlColor, secondaryColor, regionID));

                    // Extend PearlTypeEnum
                    EnumExtender.AddDeclaration(typeof(DataPearl.AbstractDataPearl.DataPearlType), pearlName);

                    // Extend ConvoID
                    EnumExtender.AddDeclaration(typeof(Conversation.ID), "Moon_" + pearlName);
                }
                EnumExtender.ExtendEnumsAgain();
            }

            CustomWorldLog($"Creating conversatin files for {regionID}...");
            {
                
                /*
                foreach (string directory in Directory.GetDirectories(pathToConvo))
                {
                    foreach(string convoFile in Directory.GetFiles(directory))
                    {
                        string[] conversation = File.ReadAllLines(convoFile);
                        if (conversation.[0] == '0' && Regex.Split(conversation, Environment.NewLine).Length > 1)
                        {
                            string text4 = Custom.xorEncrypt(conversation, 54 + k + j * 7);
                            text4 = '1' + text4.Remove(0, 1);
                            File.WriteAllText(conversation, text4);
                        }
                    }
                    
                }*/

                for (int j = 0; j < Enum.GetNames(typeof(InGameTranslator.LanguageID)).Length; j++)
                {
                    for (int k = 1; k <= 57; k++)
                    {
                        string pathToConvo = dir + Path.DirectorySeparatorChar + "Assets" + Path.DirectorySeparatorChar + "Text" + 
                            Path.DirectorySeparatorChar + "Text_" + LocalizationTranslator.LangShort((InGameTranslator.LanguageID)j) + Path.DirectorySeparatorChar + k + ".txt";

                        if (File.Exists(pathToConvo))
                        {
                            CustomWorldLog($"Encrypting file [{Path.GetFileNameWithoutExtension(pathToConvo)}] from [{regionID}]");
                            string convoLines = File.ReadAllText(pathToConvo, Encoding.Default);
                            if (convoLines[0] == '0' && Regex.Split(convoLines, Environment.NewLine).Length > 1)
                            {
                                string text4 = Custom.xorEncrypt(convoLines, 54 + k + j * 7);
                                text4 = '1' + text4.Remove(0, 1);
                                File.WriteAllText(pathToConvo, text4);
                            }
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
                CustomWorldLog($"ERROR! World text file doesn't exist: [{path}]");
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
                    + " \"regionID\":  \"" + regionID + "\", \n"
                    + " \"description\":  \"" + description + "\", \n"

                    + " \"regionName\":  \"" + regionName + "\", \n"
                    + " \"activated\":  " + activated.ToString().ToLower() + ", \n"
                    + " \"loadOrder\": " + loadOrder + ", \n"

                    + " \"url\":  \"" + url + "\", \n"
                    + " \"checksum\":  \"" + checksum + "\" \n"
                    + "}");
            }
        }

        internal static void DownloadThumbs()
        {
            
            Dictionary<string, string> thumbInfo = new Dictionary<string, string>();
            foreach (KeyValuePair<string, RegionInformation> entry in CustomWorldMod.availableRegions)
            {
                if (entry.Value.url != string.Empty && !File.Exists(Custom.RootFolderDirectory() + CustomWorldMod.resourcePath + entry.Value.folderName + Path.DirectorySeparatorChar + "thumb.png"))
                {
                    CustomWorldLog($"new thumb {entry.Value.folderName} - {entry.Value.url}");
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

        static IEnumerator DownloadAndSaveThumb(string url, string destination)
        {
            Debug.Log("Dowloading thumb..");
            Texture2D tex;
            tex = new Texture2D(4, 4, TextureFormat.DXT1, false);
            using (WWW www = new WWW(url))
            {
                yield return www;
                www.LoadImageIntoTexture(tex);
                //GetComponent<Renderer>().material.mainTexture = tex;
                byte[] file = tex.EncodeToPNG();//Convert.FromBase64String(www.text);
                File.WriteAllBytes(destination, file);
                Debug.Log("Thumb downloaded");
            }
        }
    }
}
