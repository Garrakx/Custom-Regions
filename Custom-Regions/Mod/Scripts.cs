using OptionalUI;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using static CustomRegions.Mod.CustomWorldStructs;

namespace CustomRegions.Mod
{
    public abstract class CustomWorldScript
    {
        public bool readyToDelete;
        public string name;
        public int ID;
        public string action;
        public Stopwatch stopwatch;

        public CustomWorldScript()
        {
            this.ID = (int)(UnityEngine.Random.Range(1, int.MaxValue));
        }

        public virtual void Init()
        {
            CustomWorldMod.Log($"Init [{this.name}] [{this.ID}]");
            stopwatch = new Stopwatch();
            stopwatch.Start();
        }

        public virtual void Clear()
        {
            stopwatch.Stop();
            DateTime date2 = new DateTime(stopwatch.ElapsedTicks);
            CustomWorldMod.Log($"[{this.name}] Clearing [{this.ID}]. Total time Elapsed [{date2.ToString("s.ffff")}s]", false, CustomWorldMod.DebugLevel.RELEASE);
        }

        public virtual void Log(string log, bool error) { CustomWorldMod.Log($"[{this.name}] {log}", error, CustomWorldMod.DebugLevel.MEDIUM); }

        public virtual void Log(string log) { Log(log, false); }

        public virtual void Update() { }
    }


    /// <summary>
    /// Script to launch Console that downloads regions 
    /// </summary>
    public class PackDownloader : CustomWorldScript
    {
        public string stringStatus;
        public string packName;
        public float progress;

        private Process process;
        private StringBuilder captureOutput;
        private static readonly string progressDivider = "<progDivider>";
        private static readonly string downloadDivider = "<downStat>";
        private static readonly string unzipDivider = "<zipStat>";
        //private static readonly string finishedDivider = "<finStat>";
        private bool movedDependencies;
        private bool errorGrabbingPack;
        private List<string> dependenciesName;

        public OpSimpleButton downloadButton;

        public const int OK = 0;
        public const int ERROR = -1;
        // File to unzip does not exist
        public const int FILENOTFOUND = 2;
        // Directory already exists
        public const int DIREALREADYEXIST = 3;
        // File already exists
        public const int FILEALREADYEXIST = 4;

        string errorLogged;

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
        /// Arguments: {url}{divider}\{packName}\{divider}\{RW process ID} where divider is <div>
        /// </summary>
        public PackDownloader(string arguments, string packName)
        {
            this.errorLogged = string.Empty;
            this.name = "PackDownloader";
            errorGrabbingPack = true;
            this.dependenciesName = new List<string>();
            movedDependencies = false;
            stringStatus = "Loading";
            string executableName = CustomWorldMod.exeDownloaderLocation;
            this.packName = packName;
            this.action = "Downloading a pack";
            if (!File.Exists(executableName))
            {
                CustomWorldMod.Log($"Missing RegionDownloader.exe [{executableName}]", true);
            }
            else
            {
                this.Init(arguments, executableName);
            }
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
                if (!errorGrabbingPack)
                {
                    LogPackDownload(CustomWorldMod.rainDbPacks[this.packName], CustomWorldMod.exeDownloaderLocation);
                }

                CheckForDependencies();
                if (movedDependencies)
                {
                    stringStatus = "Done";
                    PopUp(false);
                }
                else if (errorGrabbingPack)
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

        public void LogPackDownload(CustomWorldStructs.RegionPack pack, string executableName)
        {
            string divider = "<div>";

            string url = $"{CustomWorldMod.crsDBUrl}{pack.name.Replace(" ", "_")}";

            Dictionary<string, object> postData = new Dictionary<string, object>();
            postData.Add("crs_version", CustomWorldMod.mod.Version);
            postData.Add("bepinex", CustomWorldMod.usingBepinex);
            postData.Add("pack_version", pack.version);
            postData.Add("pack_checksum", pack.checksum);

            string postDataString = postData.toJson();
            //string arguments = $"{url}{divider}\"{packName}\"{divider}{ID}{divider}\" + CustomWorldMod.resourcePath + (signal.Contains("update") ? $"{divider}update" : "");

            string arguments = $"<logD>{divider}{url}{divider}{postDataString}";

            Log($"Logging succesfuly download, arguments [{arguments}");

            try
            {
                var request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                request.ContentType = "application/json";
                request.Method = "POST";
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    /*
                    string json = "{\"crsVersion\": \"" + CustomWorldMod.mod.version.ToString() + "\", \n" +
                             "\"bepinex\": \"" + CustomWorldMod.usingBepinex.ToString() + "\"}";
                    CustomWorldMod.Log(json);
                    */

                    streamWriter.Write(postData.toJson());
                }
            }
            catch (Exception e)
            {
                CustomWorldMod.Log("Mono crashed after request, this is fine :) " + e);
            }
        }


        private void PopUp(bool error)
        {
            OpTab tab = CompletelyOptional.ConfigMenu.currentInterface.Tabs.First(x => x.name.Equals("Browse RainDB"));
            if (OptionInterface.IsConfigScreen && (tab != null) && !tab.isHidden)
            {
                string labelText = "N/A";
                string buttonText = "ERROR";
                string buttonText2 = null;
                CustomWorldOption.OptionSignal signal = CustomWorldOption.OptionSignal.Empty;
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
                    signal = CustomWorldOption.OptionSignal.CloseGame;
                    buttonText2 = "Later";
                }
                else
                {
                    labelText = $"Error while downloading [{this.packName}]\n\n";
                    if (File.Exists(CustomWorldMod.exeDownloaderLocation))
                    {
                        if (!this.errorLogged.Equals(string.Empty))
                        {
                            labelText += $"{this.errorLogged}\n";
                        }
                        labelText += "Please try again.\n";
                    }
                    else
                    {
                        labelText += "Missing executable.\n";
                    }
                    buttonText = "Close";
                    signal = CustomWorldOption.OptionSignal.CloseWindow;
                }
                CustomWorldOption.CreateWindowPopUp(tab, labelText, signal, buttonText, error, buttonText2: buttonText2);
            }
        }

        private void CheckForDependencies()
        {

            string pathToDependencies = Custom.RootFolderDirectory() + CustomWorldMod.resourcePath + this.packName + @"/PackDependencies";
            if (!Directory.Exists(pathToDependencies))
            {
                CustomWorldMod.Log($"Pack doesn't have dependencies [{this.packName}]");
                return;
            }

            string[] dependenciesFullPath = Directory.GetFiles(pathToDependencies);
            Log($"Found dependencies [{string.Join(", ", dependenciesFullPath)}] for [{this.packName}]");
            string pathToMoveDependencies;
            if (CustomWorldMod.usingBepinex)
            {
                pathToMoveDependencies = Custom.RootFolderDirectory() + @"BepInEx/plugins/";
            }
            else
            {
                pathToMoveDependencies = Custom.RootFolderDirectory() + @"Mods/";
            }

            foreach (string dependencyPath in dependenciesFullPath)
            {
                string dependencyName = new FileInfo(dependencyPath).Name;

                try
                {
                    if (File.Exists(pathToMoveDependencies + dependencyName))
                    {

                        PackDependency downloadedDependency = new PackDependency();
                        downloadedDependency.LoadDependency(dependencyPath);

                        bool shouldDelete = true;
                        bool shouldSkip = false;

                        // Installed dependency
                        var installedDependencies = CustomWorldMod.installedDependencies.FindAll(x => x.assemblyName.Equals(downloadedDependency.assemblyName)).ToList();

                        if (installedDependencies.Count != 0)
                        {
                            // found dependency with same name
                            // check if installed version is greater than downloaded 

                            foreach (var installedDep in installedDependencies)
                            {
                                if (installedDep.audbVersion < downloadedDependency.audbVersion)
                                {
                                    // needs upgrade
                                    shouldDelete = true;
                                }
                                else if (!installedDep.hash.Equals(downloadedDependency.hash))
                                {
                                    // hash is different, using downloaded version
                                    shouldDelete = true;
                                }
                                else
                                {
                                    CustomWorldMod.Log($"Dependency [{installedDep.assemblyName}] already installed and up-to-date. " +
                                        $"AUDB Ver [{installedDep.audbVersion}] vs.[{downloadedDependency.audbVersion}]");
                                    shouldSkip = true;
                                    break;
                                }
                            }
                            if (shouldSkip) { continue; }

                        }
                        else
                        {
                            // no installed depency with same name
                            // should copy new one
                            CustomWorldMod.Log($"Dependency [{downloadedDependency.assemblyName}], [{dependencyName}] not found, installing...");
                            shouldDelete = true;
                        }

                        if (shouldDelete)
                        {
                            Log($"Deleting old [{dependencyName}]...");
                            File.Delete(pathToMoveDependencies + dependencyName);
                        }

                    }
                    else
                    {
                        // new dependency
                    }

                    // Copy dependencies
                    Log($"Saving [{dependencyName}], from [{dependencyPath}] to [{pathToMoveDependencies + dependencyName}]...");
                    File.Copy(dependencyPath, pathToMoveDependencies + dependencyName);
                    movedDependencies = true;
                    this.dependenciesName.Add(dependencyName);
                }
                catch (Exception e)
                {
                    CustomWorldMod.Log($"Error moving dependency [{dependencyName}] {e}");
                }
            }
        }

        public void Init(string arguments, string executableName)
        {
            // Delete old folder
            if (CustomWorldMod.installedPacks.ContainsKey(this.packName))
            {
                string folderName = CustomWorldMod.installedPacks[this.packName].folderName;
                string pathToPackFolder = CRExtras.BuildPath(folderName, CRExtras.CustomFolder.None);
                CustomWorldMod.Log($"Updating pack, check if folder exists at: [{pathToPackFolder}]...", false, CustomWorldMod.DebugLevel.MEDIUM);
                if (Directory.Exists(pathToPackFolder))
                {
                    try
                    {
                        Directory.Delete(pathToPackFolder, true);
                    }
                    catch (Exception e)
                    {
                        CustomWorldMod.Log(e.ToString(), true);
                    }
                }
            }

            base.Init();
            Log($"Executing console app [{executableName}]");

            ProcessStartInfo processStartInfo = new ProcessStartInfo(executableName, arguments);
            processStartInfo.UseShellExecute = false;
            processStartInfo.ErrorDialog = false;
            processStartInfo.CreateNoWindow = true;
            //processStartInfo.RedirectStandardInput = true;
            processStartInfo.RedirectStandardOutput = true;
            //processStartInfo.RedirectStandardError = true;


            // Start the process
            process = new Process();
            process.StartInfo = processStartInfo;
            bool processStarted = process.Start();
            process.BeginOutputReadLine();

            captureOutput = new StringBuilder();
            process.OutputDataReceived += Process_OutputDataReceived;
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                string log = Regex.Replace(outLine.Data, @"\t|\n|\r", "");

                string usedDivider;
                string finDivier = "<finStat>";
                string errorDivider = "<err>";
                if (log.Contains(progressDivider))
                {
                    stringStatus = log.Substring(log.IndexOf(progressDivider) + progressDivider.Length);
                }
                else if (log.Contains(usedDivider = finDivier) || log.Contains(usedDivider = unzipDivider) || log.Contains(usedDivider = downloadDivider) || log.Contains(usedDivider = errorDivider))
                {
                    string status = log.Replace(usedDivider, "");
                    if (int.TryParse(status, out int intStatus))
                    {
                        if (intStatus == 0)
                        {
                            errorGrabbingPack = false;
                        }
                    }
                    // Something went wrong
                    else if (usedDivider == errorDivider)
                    {
                        CustomWorldMod.Log(status, true);
                        errorLogged = status;
                    }
                }
                else if (!log.Equals(string.Empty))
                {
                    Log(log);
                }
            }
        }

    }

    public class ExeUpdater : CustomWorldScript
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
            this.name = "ExecutableUpdater";
            this.Init();
            downloading = false;
            hashing = false;
            this.hashUrl = hashUrl;
            this.fileURL = fileURL;
            this.needsDownload = false;
            this.needsUpdate = false;
            this.fileBytes = null;
            this.action = "Checking for updates for RegionPackDownloader.exe";
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
                this.action = "Downloading RegionPackDownloader.exe";
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
            if (!string.IsNullOrEmpty(www.error))
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
                        this.action = "Updating RegionPackDownloader.exe";
                    }
                    else
                    {
                        Log($"RegionPackDownloader.exe is up to date! :D");
                        downloading = true;
                        readyToDelete = true;
                    }

                }
                // Get online file
                else if (!downloading)
                {
                    downloading = true;
                    this.fileBytes = www.bytes;

                    // File needs to be updated or downloaded
                    if (needsDownload)
                    {
                        string downloadedHash;
                        using (var md5 = System.Security.Cryptography.MD5.Create())
                        {
                            using (var stream = new MemoryStream(this.fileBytes))
                            {
                                downloadedHash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
                                Log($"Current exe hash [{downloadedHash}]");
                            }
                        }
                        // Download was correct
                        if (downloadedHash.Equals(onlineHash))
                        {
                            Log($"Exectuable download was correct :D");
                            // Delete old exectuable
                            if (needsUpdate && File.Exists(CustomWorldMod.exeDownloaderLocation))
                            {
                                try
                                {
                                    Log("Deleting old exe...");
                                    File.Delete(CustomWorldMod.exeDownloaderLocation);
                                }
                                catch (Exception e) { Log("Error deleting exe " + e, true); }
                            }
                            // Save downloaded executable
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
    public class ThumbnailDownloader : CustomWorldScript
    {
        int currentThumb;
        WWW www;
        bool next;
        bool refreshedConfigScreen;
        //string path;

        private List<string> packNames;
        private List<string> urls;
        private Dictionary<string, byte[]> thumbOutput;
        private Dictionary<string, ProcessedThumbnail> procThumbs;

        public ThumbnailDownloader(Dictionary<string, string> thumbInfo, ref Dictionary<string, byte[]> thumbOutput, ref Dictionary<string, ProcessedThumbnail> procThumbs)
        {
            this.refreshedConfigScreen = false;
            this.name = "ThumbnailDownloader";
            this.action = "Downloading thumbnails";
            this.Init(thumbInfo, ref thumbOutput, ref procThumbs);
        }

        public void Init(Dictionary<string, string> thumbInfo, ref Dictionary<string, byte[]> thumbOutput, ref Dictionary<string, ProcessedThumbnail> procThumbs)
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
            this.readyToDelete = false;
            this.next = false;
            this.thumbOutput = thumbOutput;
            this.procThumbs = procThumbs;


            bool shouldDownload = false;
            while (!shouldDownload && currentThumb < this.urls.Count)
            {
                string currentPackName = this.packNames[currentThumb];
                DateTime current = DateTime.UtcNow;
                if (CustomWorldMod.processedThumbnails.TryGetValue(currentPackName, out ProcessedThumbnail procThumb))
                {
                    TimeSpan diff = current - procThumb.dateDownloaded;
                    this.Log($"[{currentPackName}]'s thumb was downloaded [{diff.TotalMinutes}] mins ago");

                    if (Math.Abs(diff.TotalMinutes) > 5)
                    {
                        this.Log($"Downloading [{currentPackName}]'s thumb");
                        shouldDownload = true;
                        CustomWorldMod.processedThumbnails.Remove(currentPackName);
                        break;
                    }
                    else
                    {
                        this.Log($"Skipping [{currentPackName}]'s thumb");
                        currentThumb++;
                    }

                }
                else
                {
                    shouldDownload = true;
                    break;
                }
            }
            if (currentThumb < this.urls.Count)
            {
                this.www = new WWW(urls[currentThumb]);
            }
            else
            {
                readyToDelete = true;
            }

        }

        public override void Update()
        {
            if (urls == null || currentThumb >= this.urls.Count || packNames == null || www == null)
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
                        Texture2D tex = null;

                        if (CustomWorldMod.installedPacks.TryGetValue(packNames[currentThumb], out RegionPack value))
                        {
                            string path = Custom.RootFolderDirectory() + CustomWorldMod.resourcePath + value.folderName +
                                Path.DirectorySeparatorChar + "thumb.png";
                            if (!File.Exists(path))
                            {
                                Log($"Saving thumb from [{packNames[currentThumb]}]... Path [{path}]");
                                tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
                                tex.LoadImage(www.bytes);
                                tex.Apply();
                                byte[] file = tex.EncodeToPNG();
                                File.WriteAllBytes(path, file);
                            }
                        }
                        // Proccess thumbnail
                        if (tex == null)
                        {
                            tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
                            tex.LoadImage(www.bytes); //..this will auto-resize the texture dimensions.
                        }

                        ProcessedThumbnail procThumb = CRExtras.ProccessThumbnail(tex, www.bytes, packNames[currentThumb]);
                        Log($"Adding processed thumbnail [{packNames[currentThumb]}]");
                        if (CustomWorldMod.processedThumbnails.ContainsKey(packNames[currentThumb]))
                        {
                            CustomWorldMod.processedThumbnails[packNames[currentThumb]] = procThumb;
                        }
                        else
                        {
                            CustomWorldMod.processedThumbnails.Add(packNames[currentThumb], procThumb);
                        }


                        next = true;
                        currentThumb++;

                        bool shouldDownload = false;
                        while (!shouldDownload && currentThumb < this.urls.Count)
                        {
                            string currentPackName = this.packNames[currentThumb];
                            DateTime current = DateTime.UtcNow;
                            Log($"[{currentPackName}] Checking if thumbnail needs to be downloaded...");
                            if (CustomWorldMod.processedThumbnails.TryGetValue(currentPackName, out ProcessedThumbnail tempThumb))
                            {
                                TimeSpan diff = current - tempThumb.dateDownloaded;
                                this.Log($"[{currentPackName}]'s thumb was downloaded [{diff.TotalMinutes}] mins ago");
                                if (Math.Abs(diff.TotalMinutes) > 2)
                                {
                                    shouldDownload = true;
                                    break;
                                }
                                else
                                {
                                    currentThumb++;
                                }

                            }
                            else
                            {
                                shouldDownload = true;
                                break;
                            }
                        }
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
                    CompletelyOptional.ConfigMenu.ResetCurrentConfig();
                }
                catch (Exception e)
                {
                    Log("Error reloading config menu: " + e);
                }
            }
            try
            {
                if (this.packNames != null)
                { this.packNames.Clear(); }

                if (this.www != null)
                { this.www.Dispose(); }
            }
            catch (Exception e) { Log(e.Message, true); }

            try
            {
                if (this.urls != null)
                { this.urls.Clear(); }
            }
            catch (Exception e) { Log(e.Message, true); }
        }
    }

    public class PackFetcher : CustomWorldScript
    {
        WWW www;
        //public Dictionary<string, RegionPack> packs;
        string url;
        bool ready;

        public PackFetcher(string url)
        {
            this.name = "PackFetcher";
            this.Init(url);
            this.action = "Fetching RainDB information";
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
                    List<object> json;
                    try
                    {
                        json = file.listFromJson();

                        Dictionary<string, RegionPack> tempRainDb = new Dictionary<string, RegionPack>();
                        foreach (object jsonPack in json)
                        {
                            RegionPack regionPack = new RegionPack("");
                            //CustomWorldMod.Log($"Obtained data [{jsonPack.ToString()}]");
                            CustomWorldMod.FromDictionaryToPackInfo(jsonPack as Dictionary<string, object>, ref regionPack, authorative: true);
                            try
                            {
                                regionPack.activated = true;
                                //regionPack.activated = CustomWorldMod.installedRegionPacks.ContainsKey(regionPack.name);
                                tempRainDb.Add(regionPack.name, regionPack);
                            }
                            catch (Exception e) { Log($"Exception when adding fetched region [{e}]", true); }
                        }
                        var date = DateTime.UtcNow.Date;
                        var seed = date.Year * 1000 + date.DayOfYear;
                        var random1 = new System.Random(seed);

                        var seq = Enumerable.Range(0, tempRainDb.Count()).OrderBy(x => random1.Next()).Take(tempRainDb.Count()).ToList();
                        foreach (int item in seq)
                        {
                            KeyValuePair<string, RegionPack> tempItem = tempRainDb.ElementAt(item);
                            CustomWorldMod.rainDbPacks.Add(tempItem.Key, tempItem.Value);
                        }

                        Log($"Added fetched regions [{string.Join(", ", CustomWorldMod.rainDbPacks.Keys.ToArray())}]");
                    }
                    catch (Exception e)
                    {
                        Log("Error fetching regions " + e, true);
                    }
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

    public class NewsFetcher : CustomWorldScript
    {
        WWW www;
        bool ready;
        string url;

        public NewsFetcher(string url)
        {
            this.name = "NewsFetcher";
            this.Init();
            ready = false;
            this.url = url;
            this.www = new WWW(this.url);
            this.action = "Fetching news";
        }

        public override void Clear()
        {
            base.Clear();
            this.www.Dispose();
        }


        public override void Log(string log, bool error)
        {
            base.Log(log, error);
        }

        public override void Log(string log)
        {
            base.Log(log);
        }

        public override void Update()
        {
            base.Update();
            if (url == null || readyToDelete)
            {
                readyToDelete = true;
                return;
            }
            if (!string.IsNullOrEmpty(www.error))
            {
                Log(www.error, true);
                readyToDelete = true;
            }
            else if (www.isDone && !ready)
            {
                ready = true;
                using (StreamWriter sw = File.CreateText(Custom.RootFolderDirectory() + "customNewsLog.txt"))
                {
                    sw.WriteLine($"{CustomWorldStructs.News.IGNORE}Custom News Log {CustomWorldMod.versionCR} \n");
                    sw.WriteLine(www.text);
                    readyToDelete = true;
                }
            }
        }
    }

}
