using CompletelyOptional;
using DevInterface;
using OptionalUI;
using RWCustom;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using static CustomRegions.Mod.CustomWorldStructs;

namespace CustomRegions.Mod
{

    public abstract class CustomRegionScript
    {
        public bool readyToDelete;
        public string name;
        public int ID;
        public CustomRegionScript()
        {
            this.ID = (int)(UnityEngine.Random.value * 1000f);
        }

        public static void Create() { }

        public void Init() { CustomWorldMod.Log($"Init [{this.name}] [{this.ID}]"); }

        public virtual void Clear() { CustomWorldMod.Log($"Clearing [{this.name}] [{this.ID}]"); }

        public virtual void Log(string log, bool error) { CustomWorldMod.Log($"[{this.name}] {log}", error); }

        public virtual void Log(string log) { Log(log, false); }

        public virtual void Update() { }
    }


    /// <summary>
    /// Script to launch Console that downloads regions 
    /// </summary>
    public class PackDownloader : CustomRegionScript
    {
        Process process;
        private StringBuilder captureOutput;
        public string stringStatus;
        internal static readonly string progressDivider = "<progDivider>";
        internal static readonly string downloadDivider = "<downStat>";
        internal static readonly string unzipDivider = "<zipStat>";
        internal static readonly string finishedDivider = "<finStat>";
        public string packName;
        public float progress;
        private bool movedDependencies;
        List<string> dependenciesName;
        private bool errorGrabbingPack;
        internal OpSimpleButton downloadButton;
        public const int OK = 0;
        public const int ERROR = -1;
        // File to unzip does not exist
        public const int FILENOTFOUND = 2;
        // Directory already exists
        public const int DIREALREADYEXIST = 3;
        // File already exists
        public const int FILEALREADYEXIST = 4;

        //LogStatus status;
        enum LogStatus
        {
            OK,
            FileNotFound,
            DirectoryAlreadyExists,
            FileAlreadyExists,
            Downloaded,
            Unzipped,
            Loading,
            ExitOK,
            Error
        }



        /// <summary>
        /// Arguments: {url}{divider}\{packName}\ where divider is <div>
        /// </summary>
        public PackDownloader(string arguments, string packName)
        {
            this.name = "PackDownloader";
            errorGrabbingPack = true;
            this.dependenciesName = new List<string>();
            movedDependencies = false;
            stringStatus = "Loading";
            string executableName = CustomWorldMod.exeDownloaderLocation;
            this.packName = packName;
            if (!File.Exists(executableName))
            {
                CustomWorldMod.Log($"Missing RegionDownloader.exe [{executableName}]", true);
                return;
            }
            this.Init(arguments, executableName);
            //CustomWorldMod.scripts.Add(this);
        }

        public override void Clear()
        {
            base.Clear();

            if (this.process != null)
            {
                this.process.Close();
            }
        }

        public override void Update()
        {
            if (process == null || process.HasExited && !readyToDelete)
            {
                Log("RegionPackDownloader console has exited");
                CheckForDependencies();
                if (movedDependencies)
                {
                    stringStatus = "Done";
                    PopUp(false);
                }
                else if(errorGrabbingPack)
                {
                    stringStatus = "Retry";
                    PopUp(true);
                }
                else
                {
                    stringStatus = "Finished";
                    CustomWorldMod.LoadCustomWorldResources();
                }
                readyToDelete = true;
            }
            if (downloadButton != null)
            {
                downloadButton.text = stringStatus;
            }


        }


        private void PopUp(bool error)
        {
            OpTab tab = ConfigMenu.currentInterface.Tabs.First(x => x.name.Equals("Browse RainDB"));
            if (OptionInterface.IsConfigScreen && (tab != null) && !tab.isHidden)
            {
                /*
                int spacing = 30;
                Vector2 buttonSize = new Vector2(70, 35);
                Vector2 rectSize = new Vector2(420, 135 + buttonSize.y);
                Vector2 rectPos = new Vector2(300- rectSize.x/2f, 300- rectSize.y/2);
                Vector2 labelSize = rectSize - new Vector2(spacing, spacing + buttonSize.y + spacing);
                OpRect restartPop = new OpRect(rectPos, rectSize, 0.9f);
                string labelText = $"[{this.packName}] requires additional mods to function:\n\n";
                labelText += $"The required files [{string.Join(", ", dependenciesName.ToArray())}] have been downloaded";
                if (CustomWorldMod.usingBepinex)
                {
                    labelText += $" to BepInEx's plugins folder. \nPlease restart the game to apply the changes.";
                }
                else
                {
                    labelText += $"to Partiality's Mods folder. \nPlease close the game and apply them using the Partiality Launcher.";
                }
                //text += $" \nDependencies installed: [{string.Join(", ", dependenciesName.ToArray())}]";

                OpLabelLong label = new OpLabelLong(new Vector2(rectPos.x + spacing/2, rectPos.y + buttonSize.y+spacing), labelSize, "", true, FLabelAlignment.Center)
                {
                    text = labelText,
                    verticalAlignment = OpLabel.LabelVAlignment.Top
                };

                OpSimpleButton closeGameButton = new OpSimpleButton(new Vector2(rectPos.x + (rectSize.x - buttonSize.x)/2f, rectPos.y + spacing/2f), buttonSize, "close_game", "Exit game");
                tab.AddItems(restartPop, label, closeGameButton);
                */
                string labelText = "N/A";
                string buttonText = "ERROR";
                string signal = "";
                if (!error)
                {
                    labelText = $"[{this.packName}] requires additional mods to function:\n\n";
                    labelText += $"The required files [{string.Join(", ", dependenciesName.ToArray())}] have been downloaded";
                    if (CustomWorldMod.usingBepinex)
                    {
                        labelText += $" to BepInEx's plugins folder. \nPlease restart the game to apply the changes.";
                    }
                    else
                    {
                        labelText += $"to Partiality's Mods folder. \nPlease close the game and apply them using the Partiality Launcher.";
                    }
                    buttonText = "Exit game";
                    signal = "close_game";
                }
                else
                {
                    labelText = $"Error while downloading [{this.packName}]\n\n";
                    if(File.Exists(CustomWorldMod.exeDownloaderLocation))
                    {
                        labelText += "Please try again.\n";
                    }
                    else
                    {
                        labelText += "Missing executable.\n";
                    }
                    buttonText = "Close";
                    signal = "close_window";
                }
                CustomWorldOption.CreateWindowPopUp(labelText, tab, signal, buttonText, error);
            }
        }

        private void CheckForDependencies()
        {
            string pathToDependencies = Custom.RootFolderDirectory() + CustomWorldMod.resourcePath + this.packName + @"/PackDependencies";
            if (Directory.Exists(pathToDependencies))
            {
                string[] dependenciesFullPath = Directory.GetFiles(pathToDependencies);
                Log($"Found dependencies [{string.Join(", ", dependenciesFullPath)}] for [{this.packName}]");
                if(CustomWorldMod.usingBepinex)
                {
                    string pathToPlugins = Custom.RootFolderDirectory() + @"BepInEx/plugins/";
                    foreach(string dependency in dependenciesFullPath)
                    {
                        string dependencyName = new FileInfo(dependency).Name;
                        this.dependenciesName.Add(dependencyName);
                        try
                        {
                            File.Move(dependency, pathToPlugins + dependencyName);
                            movedDependencies = true;
                        }
                        catch (Exception e)
                        {
                            CustomWorldMod.Log($"Error moving dependency [{dependencyName}] {e}");
                        }
                    }
                }
                else
                {
                    string pathToMods = Custom.RootFolderDirectory() + @"Mods/";

                }
            }
        }

        public void Init(string arguments, string executableName)
        {
            base.Init();
            Log($"Executing console app [{executableName}]");

            ProcessStartInfo processStartInfo = new ProcessStartInfo(executableName, arguments);
            processStartInfo.UseShellExecute = false;
            processStartInfo.ErrorDialog = false;
            processStartInfo.CreateNoWindow = false;
            //processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardInput = true;
            processStartInfo.RedirectStandardOutput = true;


            // Start the process
            process = new Process();
            process.StartInfo = processStartInfo;
            bool processStarted = process.Start();
            process.BeginOutputReadLine();

            captureOutput = new StringBuilder();
            process.OutputDataReceived += Process_OutputDataReceived;

            //process.WaitForExit();
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                string log = Regex.Replace(outLine.Data, @"\t|\n|\r", "");

                string usedDivider;
                string finDivier = "<finStat>";
                if (log.Contains(progressDivider))
                {
                    stringStatus = log.Substring(log.IndexOf(progressDivider) + progressDivider.Length);
                }
                else if (log.Contains(usedDivider = finDivier) || log.Contains(usedDivider = unzipDivider) || log.Contains(usedDivider = downloadDivider))
                {
                    string status = log.Replace(usedDivider, "");
                    if (int.TryParse(status, out int intStatus))
                    {
                        if (intStatus == 0)
                        {
                            errorGrabbingPack = false;
                        }
                    }
                }
                else if (!log.Equals(string.Empty))
                {
                    Log(log);
                }
                /*
                else if (log.Contains(downloadDivider))
                {
                    
                }
                else if(log.Contains(unzipDivider))
                {
                    if (log.Contains(OK.ToString()))
                    {
                        status = LogStatus.Unzipped;
                    }
                }
                */
            }
        }

        /*
        public LogStatus ParseStatus(string s, string divider)
        {
            string status = s.Replace(divider, "");
            if (int.TryParse(status, out int intStatus))
            {
                switch (intStatus)
                {
                    case OK:
                        break;
                    case ERROR:
                        break;
                        case 
                }
            }
            return LogStatus.Error;
        }
        */
    }

    public class ExeUpdater : CustomRegionScript
    {
        bool needsUpdate;
        bool needsDownload;
        string hashUrl;
        string currentHash;
        string onlineHash;
        WWW www;
        bool hashing;
        bool downloading;
        byte[] fileBytes;
        private string fileURL;

        public ExeUpdater(string hashUrl, string fileURL)
        {
            downloading = false;
            hashing = false;
            this.name = "ExecutableUpdater";
            this.Init();
            this.hashUrl = hashUrl;
            this.fileURL = fileURL;
            this.needsDownload = false;
            this.needsUpdate = false;
            this.fileBytes = null;
            if (File.Exists(CustomWorldMod.exeDownloaderLocation))
            {
                using (var md5 = System.Security.Cryptography.MD5.Create())
                {
                    using (var stream = File.OpenRead(CustomWorldMod.exeDownloaderLocation))
                    {
                        currentHash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
                        Log($"Current exe hash [{currentHash}]");
                    }
                }
            }
            else
            {
                Log("Missing region downloader exe at " + CustomWorldMod.exeDownloaderLocation, true);
                needsDownload = true;
            }

            this.www = new WWW(this.hashUrl);
        }

        public override void Clear()
        {
            base.Clear();
            this.www.Dispose();
        }

        public override void Update()
        {
            base.Update();
            if(!string.IsNullOrEmpty(www.error)) 
            {
                Log(www.error, true);
                readyToDelete = true;
            }
            else if (www.isDone)
            {
                // Get online hash
                if (!hashing)
                {
                    hashing = true;
                    onlineHash = www.text;
                    Log($"Online hash [{onlineHash}]");
                    if (!onlineHash.Equals(currentHash))
                    {
                        needsDownload = true;
                        needsUpdate = true;
                    }

                    if (needsDownload)
                    {
                        this.www.Dispose();
                        this.www = new WWW(this.fileURL);
                    }
                    else
                    {
                        Log($"RegionPackDownloader.exe is up to date! :D");
                        downloading = true;
                        readyToDelete = true;
                    }
                   
                }
                // Get online file
                else if(!downloading)
                {
                    downloading = true;
                    this.fileBytes = www.bytes;
                    if (needsDownload)
                    {
                        string downloadedHash;
                        using (var md5 = System.Security.Cryptography.MD5.Create())
                        {
                            using (var stream = new MemoryStream(this.fileBytes))
                            {
                                downloadedHash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
                                Log($"Current exe hash [{currentHash}]");
                            }
                        }
                        if (downloadedHash.Equals(onlineHash))
                        {
                            if (needsUpdate && File.Exists(CustomWorldMod.exeDownloaderLocation))
                            {
                                try
                                {
                                    Log("Deleting old exe...");
                                    File.Delete(CustomWorldMod.exeDownloaderLocation);
                                }
                                catch (Exception e) { Log("Error deleting exe " + e, true); }
                            }
                            try
                            {
                                File.WriteAllBytes(CustomWorldMod.exeDownloaderLocation, fileBytes);
                                readyToDelete = true;
                            }
                            catch (Exception e) { Log("Error downloading exe " + e, true); }
                        }
                        else
                        {
                            Log("Fatal error updating/downloading executable", true);
                        }
                    }
                    else
                    {
                        readyToDelete = true;
                    }
                }
                else
                {
                    readyToDelete = true;
                }

            }

        }

    }

    /// <summary>
    /// Script to download missing thumbnails
    /// </summary>
    public class ThumbnailDownloader : CustomRegionScript
    {

        int currentThumb;
        WWW www;
        bool next;
        //string path;
        private List<string> packNames;
        private List<string> urls;
        private Dictionary<string, byte[]> thumbOutput;
        bool refreshedConfigScreen;
        public ThumbnailDownloader(Dictionary<string, string> thumbInfo, ref Dictionary<string, byte[]> thumbOutput)
        {
            this.refreshedConfigScreen = false;
            this.name = "ThumbnailDownloader";
            this.Init(thumbInfo, ref thumbOutput);
            //CustomWorldMod.scripts.Add(this);
        }
        
        public void Init(Dictionary<string, string> thumbInfo, ref Dictionary<string, byte[]> thumbOutput)
        {
            base.Init();
            if (thumbInfo == null || thumbInfo.Count < 1)
            {
                //CustomWorldMod.Log("Error creating thumbnail downloader, thumbnail not found", true);
                this.readyToDelete = true;
                return;
            }

            currentThumb = 0;
            this.packNames = thumbInfo.Keys.ToList();
            this.urls = thumbInfo.Values.ToList();
            this.www = new WWW(urls[currentThumb]);
            this.readyToDelete = false;
            this.next = false;
            this.thumbOutput = thumbOutput;
        }

        public override void Update()
        {
            if (urls == null || currentThumb >= this.urls.Count || packNames == null)
            {
                readyToDelete = true;
                return;
            }

            if (string.IsNullOrEmpty(www.error))
            {
                if (www.isDone)
                {
                    if (!next)
                    {
                        Log($"Downloaded thumb [{packNames[currentThumb]}]");
                        thumbOutput.Add(packNames[currentThumb], www.bytes);

                        if (CustomWorldMod.installedPacks.TryGetValue(packNames[currentThumb], out RegionPack value))
                        {
                            string path = Custom.RootFolderDirectory() + CustomWorldMod.resourcePath + value.folderName + Path.DirectorySeparatorChar + "thumb.png";
                            if (!File.Exists(path))
                            {
                                Log($"Saving thumb from [{packNames[currentThumb]}]... Path [{path}]");
                                Texture2D tex;
                                tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
                                tex.LoadImage(www.bytes);
                                tex.Apply();
                                byte[] file = tex.EncodeToPNG();
                                File.WriteAllBytes(path, file);
                            }
                        }
                        next = true;
                        currentThumb++;
                    }
                    else
                    {
                        this.www = new WWW(urls[currentThumb]);
                        next = false;
                    }
                }
            }
            else
            {
                readyToDelete = true;
                CustomWorldMod.Log(www.error, true);
            }
        }

        public override void Clear()
        {
            base.Clear();
            if (OptionInterface.IsConfigScreen && !refreshedConfigScreen)
            {
                refreshedConfigScreen = true;
                try
                {
                    ConfigMenu.ResetCurrentConfig();
                }
                catch (Exception e)
                {
                    Log("Error reloading config menu: " + e, true);
                }
            }
            try
            {
                if (this.packNames != null)
                { this.packNames.Clear(); }
                this.www.Dispose();
            }
            catch (Exception e) { Log(e.Message, true); }

            try
            {
                if (this.urls != null)
                    this.urls.Clear();
            }
            catch (Exception e) { Log(e.Message, true); }
        }
    }

    public class PackFetcher : CustomRegionScript
    {
        WWW www;
        //public Dictionary<string, RegionPack> packs;
        string url;
        bool ready;

        public PackFetcher(string url)
        {
            this.name = "PackFetcher";
            this.Init(url);
            //CustomWorldMod.scripts.Add(this);
        }

        public void Init(string url)
        {
            base.Init();
            this.url = url;
            this.readyToDelete = false;
            ready = true;
            this.www = new WWW(url);
        }

        public override void Update()
        {
            if (url == null || readyToDelete)
            {
                readyToDelete = true;
                return;
            }

            if (string.IsNullOrEmpty(www.error))
            {
                if (www.isDone && ready)
                {
                    Log($"Fetching RainDB data... path [{url}]");
                    string file = www.text;
                    List<object> json = file.listFromJson();

                    foreach (object jsonPack in json)
                    {
                        RegionPack regionPack = new RegionPack("");
                        //CustomWorldMod.Log($"Obtained data [{jsonPack.ToString()}]");
                        CustomWorldMod.FromDictionaryToPackInfo(jsonPack as Dictionary<string, object>, ref regionPack);
                        try
                        {
                            regionPack.activated = true;
                            //regionPack.activated = CustomWorldMod.installedRegionPacks.ContainsKey(regionPack.name);
                            CustomWorldMod.rainDbPacks.Add(regionPack.name, regionPack);
                        }
                        catch (Exception e) { Log($"Exception when adding fetched region [{e}]", true); }
                    }
                    // CustomWorldMod.rainDbPacks = CustomWorldMod.rainDbPacks.OrderBy(x => x.Value.activated == true);
                    //var sorted = from entry in CustomWorldMod.rainDbPacks orderby entry.Value.activated ascending select entry;
                    //CustomWorldMod.rainDbPacks = CustomWorldMod.rainDbPacks.OrderBy(x => x.Value.activated).ToDictionary(x => x.Key, x => x.Value);
                    Log($"Added fetched regions [{string.Join(", ", CustomWorldMod.rainDbPacks.Keys.ToArray())}]");
                    ready = false;
                    readyToDelete = true;
                    CustomWorldMod.UpdateLocalPackWithOnline();
                    CustomWorldMod.LoadThumbnails();
                }
            }
            else
            {
                readyToDelete = true;
                Log(www.error, true);
                CustomWorldMod.OfflineMode = true;
                if (CustomWorldMod.OfflineMode)
                {
                    Log("RUNNING CUSTOM REGIONS IN OFFLINE MODE");
                    CustomWorldMod.LoadThumbnails();
                }
            }
        }

        public override void Clear()
        {
            base.Clear();
            this.www.Dispose();
            /*
            try
            {
                //this.packs.Clear();
            }
            catch (Exception e) { Log(e.Message, true); }
            */
        }
    }

}
