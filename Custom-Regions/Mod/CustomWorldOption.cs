using CompletelyOptional;
using OptionalUI;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using static CustomRegions.Mod.CustomWorldStructs;

namespace CustomRegions.Mod
{
    public class CustomWorldOption : OptionInterface
    {

        public CustomWorldOption() : base(mod: CustomWorldMod.mod)
        {
        }

        public override bool Configuable()
        {
            return false;
        }

        public override void Initialize()
        {
            base.Initialize();

            updateAvailableTabWarning = false;
            errorTabWarning = false;

            Tabs = new OpTab[4];

            windows = new List<WindowCM>();

            Stopwatch watch = new Stopwatch();
            DateTime date;

            watch.Start();
            MainTabRedux(0, "Main Tab");

            watch.Stop();
            date = new DateTime(watch.ElapsedTicks);
            CustomWorldMod.Log($"[CM Menu] Finished loading Main tab [{date.ToString("s.ffff")}s]", false, CustomWorldMod.DebugLevel.MEDIUM);
            watch.Reset();

            watch.Start();
            AnalyserSaveTab(1, "Analyzer");
            watch.Stop();
            date = new DateTime(watch.ElapsedTicks);
            CustomWorldMod.Log($"[CM Menu] Finished loading AnalyserTab tab [{date.ToString("s.ffff")}s]", false, CustomWorldMod.DebugLevel.MEDIUM);
            watch.Reset();

            watch.Start();
            PackBrowser(2, "Browse RainDB");
            watch.Stop();
            date = new DateTime(watch.ElapsedTicks);
            CustomWorldMod.Log($"[CM Menu] Finished loading PackBrowser tab [{date.ToString("s.ffff")}s]", false, CustomWorldMod.DebugLevel.MEDIUM);
            watch.Reset();

            watch.Start();
            NewsTab(3, "News");
            watch.Stop();
            date = new DateTime(watch.ElapsedTicks);
            CustomWorldMod.Log($"[CM Menu] Finished loading NewsTab tab [{date.ToString("s.ffff")}s]", false, CustomWorldMod.DebugLevel.MEDIUM);
            watch.Reset();
        }

        private void NewsTab(int tab, string tabName)
        {
            Tabs[tab] = new OpTab(tabName);

            // Header
            OpLabel labelID = new OpLabel(new Vector2(100f, 560), new Vector2(400f, 40f), $"News Feed".ToUpper(), FLabelAlignment.Center, true);
            Tabs[tab].AddItems(labelID);

            OpLabel labelDsc = new OpLabel(new Vector2(100f, 540), new Vector2(400f, 20f), $"Latest news for CRS", FLabelAlignment.Center, false);
            Tabs[tab].AddItems(labelDsc);

            List<UIelement> news = new List<UIelement>();
            if (File.Exists(Custom.RootFolderDirectory() + "customNewsLog.txt"))
            {
                DateTime current = DateTime.UtcNow.Date;
                CustomWorldMod.Log($"Reading news feed, current time [{current.ToString("dd/MM/yyyy")}]");
                string[] lines = File.ReadAllLines(Custom.RootFolderDirectory() + "customNewsLog.txt");
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains(News.IGNORE) || lines[i].Equals(string.Empty)) { continue; }
                    bool bigText = false;
                    string lastUpdate = string.Empty;
                    TimeSpan diff;
                    if (lines[i].Contains(News.DATE))
                    {
                        if (!updatedNews)
                        {
                            try
                            {
                                DateTime newsDate = DateTime.ParseExact(lines[i].Replace(News.DATE, ""), "dd/MM/yyyy", null);
                                diff = current - newsDate;
                                lastUpdate = newsDate.ToShortDateString();
                                CustomWorldMod.Log($"News date [{lastUpdate}], difference [{diff.TotalDays}]");
                                if (Math.Abs(diff.TotalDays) < 7)
                                {
                                    updatedNews = true;
                                }

                            }
                            catch (Exception e) { CustomWorldMod.Log($"Error reading the date time in news feed [{lines[i].Replace(News.DATE, "")}] - [{e}]", 
                                true); }
                        }
                        continue;
                    }
                    if (lines[i].Contains(News.BIGTEXT)) { bigText = true; lines[i] = lines[i].Replace(News.BIGTEXT, ""); }

                    if (bigText)
                    {
                        news.Add(new OpLabel(default(Vector2), default(Vector2), lines[i], FLabelAlignment.Center, true));
                    }
                    else
                    {
                        news.Add(new OpLabelLong(default(Vector2), default(Vector2), lines[i], true, FLabelAlignment.Left));
                    }
                }

                //How Many Options
                int numberOfNews = news.Count;

                if (numberOfNews < 1)
                {
                    OpLabel label2 = new OpLabel(new Vector2(100f, 350), new Vector2(400f, 20f), "No news found.", FLabelAlignment.Center, false);
                    Tabs[tab].AddItems(label2);
                    return;
                }
                int spacing = 25;

                Vector2 rectSize = new Vector2(500 - spacing, 30);
                OpScrollBox mainScroll = new OpScrollBox(new Vector2(25, 25), new Vector2(550, 500), (int)(spacing + ((rectSize.y + spacing) * numberOfNews)));
                Vector2 rectPos = new Vector2(spacing / 2, mainScroll.GetContentSize() - rectSize.y - spacing);
                Vector2 labelSize = new Vector2(rectSize.x - spacing, rectSize.y - 2 * spacing);
                Tabs[tab].AddItems(mainScroll);

                for (int i = 0; i < numberOfNews; i++)
                {

                    UIelement label = news[i];
                    label.pos = rectPos + new Vector2(spacing, spacing);
                    label.size = labelSize;

                    mainScroll.AddItems(label);
                    rectPos.y -= rectSize.y + spacing;

                }
            }

            AddScriptIndicator(Tabs[tab]);
        }

        static List<WindowCM> windows = null;
        private bool updateAvailableTabWarning;
        private bool errorTabWarning;
        private bool updatedNews = false;
        Color updateBlinkColor = Color.white;
        float counter = 0;
        public float rotationClockCounter;

        public override void Update(float dt)
        {
            base.Update(dt);
            counter += 8f * dt;

            try
            {
                if (errorTabWarning)
                {
                    OpTab errorTab = Tabs.First(x => x.name.Equals("Analyzer"));
                    errorTab.color = Color.Lerp(Color.white, Color.red, 0.5f * (0.65f - Mathf.Sin(counter + Mathf.PI)));
                }

                OpTab raindbTab = Tabs.First(x => x.name.Equals("Browse RainDB"));
                if (updateAvailableTabWarning)
                {
                    //OpTab raindbTab = Tabs.First(x => x.name.Equals("Browse RainDB"));
                    updateBlinkColor = Color.Lerp(Color.white, Color.green, 0.5f * (0.65f - Mathf.Sin(counter + Mathf.PI)));
                    raindbTab.color = updateBlinkColor;
                }
                if (!raindbTab.isHidden && CustomWorldMod.scripts != null)
                {
                    PackDownloader script = CustomWorldMod.scripts.Find(x => x is PackDownloader) as PackDownloader;
                    if (script != null)
                    {
                        if (script.downloadButton == null)
                        {
                            OpSimpleButton downloadButton = (Tabs.First(x => x.name.Equals("Browse RainDB")).items.Find(
                                                                x => x is OpSimpleButton button && button.signal.Contains(script.packName)
                                                            ) as OpSimpleButton);
                            script.downloadButton = downloadButton;
                        }
                    }
                    List<UIelement> simpleButtons =
                        raindbTab.items.FindAll(x => x is OpSimpleButton button && button.text.ToLower().Contains("update"));

                    foreach (UIelement item in simpleButtons)
                    {
                        (item as OpSimpleButton).colorEdge = updateBlinkColor;
                    }

                }

                if (updatedNews)
                {
                    OpTab news = Tabs.First(x => x.name.ToLower().Contains("news"));
                    news.color = Color.Lerp(Color.white, Color.blue, 0.5f * (0.65f - Mathf.Sin(counter + Mathf.PI)));
                }
            }
            catch (Exception e) { CustomWorldMod.Log("Error getting downloadButton " + e, true); }

            if (windows != null)
            {
                foreach (WindowCM window in windows)
                {
                    OpTab tab = window.tab;
                    if (!tab.isHidden)
                    {
                        if (!window.added)
                        {
                            tab.AddItems(window.WindowContents.ToArray());
                            window.added = true;
                            window.ShowWindow();
                        }
                        if (window.opened && window.showLoading)
                        {
                            rotationClockCounter = Mathf.Lerp(rotationClockCounter, rotationClockCounter + 10, dt * 10);
                            window.UpdateLoadingRotation(dt, rotationClockCounter);
                            //window.loading.sprite.rotation = rotationClockCounter;
                        }
                    }
                }
            }

            OpTab currentTab = Tabs.First(x => !x.isHidden);
            if (!currentTab.Equals(default))
            {
                try
                {
                    OpImage loadingImage = currentTab.items.Find(x => x.description.ToLower().Contains("script") && x is OpImage) as OpImage;
                    OpLabel scriptLabel = currentTab.items.Find(x => x is OpLabel label && label.description.Equals("Script action")) as OpLabel;
                    if (!loadingImage.Equals(default))
                    {
                        int scriptsCount = CustomWorldMod.scripts.Count();
                        if (scriptsCount > 0)
                        {
                            loadingImage.Show();
                            scriptLabel.Show();
                            string currentDesc = loadingImage.description;
                            loadingImage.description = $"{currentDesc.Substring(0, currentDesc.IndexOf('[')+1)}{CustomWorldMod.scripts[scriptsCount - 1].action}]";
                            loadingImage.sprite.rotation = Mathf.Lerp(loadingImage.sprite.rotation, loadingImage.sprite.rotation + 10, dt * 5);
                            loadingImage.GrafUpdate(dt);
                            scriptLabel.text = CustomWorldMod.scripts[scriptsCount - 1].action;
                            //scriptLabel.GrafUpdate(dt);
                        }
                        else
                        {
                            loadingImage.Hide();
                            scriptLabel.Hide();
                        }
                    }

                }
                catch { }
            }

        }

        public override void Signal(UItrigger trigger, string signal)
        {
            base.Signal(trigger, signal);
            if (signal != null)
            {
                CustomWorldMod.Log($"Received menu signal [{signal}]");

                
                //
                // config menu list
                if (signal.Equals(OptionSignal.Refresh.ToString()))
                {
                    if (CustomWorldMod.scripts.Count == 0)
                    {
                        CRExtras.TryPlayMenuSound(SoundID.MENU_Player_Unjoin_Game);
                        ConfigMenu.ResetCurrentConfig();
                    }
                }
                // Reload pack list
                else if (signal.Equals(OptionSignal.ReloadRegions.ToString()))
                {
                    if (CustomWorldMod.scripts.Count == 0)
                    {
                        CRExtras.TryPlayMenuSound(SoundID.HUD_Exit_Game);
                        OpTab tab = CompletelyOptional.ConfigMenu.currentInterface.Tabs.First(x => !x.isHidden);
                        if (OptionInterface.IsConfigScreen && !tab.Equals(default(OpTab)))
                        {
                            CreateWindowPopUp(tab, $"Reloading regions...", CustomWorldOption.OptionSignal.Empty, "", true).ShouldShowLoading = true;
                        }
                        CustomWorldMod.LoadCustomWorldResources();
                    }
                }
                // Downnload a pack X
                else if (signal.Contains(OptionSignal.Download.ToString()) || signal.Contains(OptionSignal.Update.ToString()))
                {
                    // Process ID of Rain World
                    string ID = System.Diagnostics.Process.GetCurrentProcess().Id.ToString();
                    // Divider used
                    string divider = "<div>";
                    // Name of the pack to download
                    string packName = signal.Substring(signal.IndexOf("_") + 1);
                    string url = "";

                    CustomWorldMod.Log($"Download / update signal from [{packName}]");

                    if (CustomWorldMod.scripts.FindAll(x => x is PackDownloader).Count == 0)
                    {

                        if (CustomWorldMod.rainDbPacks.TryGetValue(packName, out RegionPack toDownload))
                        {
                            url = toDownload.packUrl;
                        }

                        if (url != null && url != string.Empty)
                        {
                            string arguments = $"{url}{divider}\"{packName}\"{divider}{ID}{divider}" +
                                @"\" + CustomWorldMod.resourcePath + (signal.Contains("update") ? $"{divider}update" : "");
                            CustomWorldMod.Log($"Creating pack downloader for [{arguments}]");

                            CustomWorldMod.scripts.Add(new PackDownloader(arguments, packName));
                            CRExtras.TryPlayMenuSound(SoundID.MENU_Player_Join_Game);
                        }
                        else
                        {
                            CustomWorldMod.Log($"Error loading pack [{packName}] from raindb pack list", true);
                        }
                    }
                    else
                    {
                        CustomWorldMod.Log("Pack downloader in process");

                        CRExtras.TryPlayMenuSound(SoundID.MENU_Player_Unjoin_Game);
                        OpTab tab = CompletelyOptional.ConfigMenu.currentInterface.Tabs.First(x => !x.isHidden);
                        if (OptionInterface.IsConfigScreen && !tab.Equals(default(OpTab)))
                        {
                            CreateWindowPopUp(tab, $"A pack is currently being downloaded.\n\nPlease wait until it finishes.",
                                CustomWorldOption.OptionSignal.CloseWindow, "OK", true);
                        }

                    }
                }
                // Close the game
                else if (signal.Equals(OptionSignal.CloseGame.ToString()))
                {
                    CRExtras.TryPlayMenuSound(SoundID.HUD_Exit_Game);
                    CustomWorldMod.Log("Exiting game...");
                    Application.Quit();
                }
                // Close(hide) pop-up window
                else if (signal.Equals(OptionSignal.CloseWindow.ToString()))
                {
                    if (windows != null)
                    {
                        foreach (WindowCM window in windows)
                        {
                            if (window != null && window.opened)
                            {
                                window.HideWindow();
                            }
                        }
                    }
                }
                else if (signal.Contains("Try") && (CustomWorldMod.scripts.Count > 0 && CustomWorldMod.scripts.FindAll(x => x is PackDownloader).Count != 0))
                {
                    CustomWorldMod.Log("Pack downloader in process");

                    CRExtras.TryPlayMenuSound(SoundID.MENU_Player_Unjoin_Game);
                    OpTab tab = CompletelyOptional.ConfigMenu.currentInterface.Tabs.First(x => !x.isHidden);
                    if (OptionInterface.IsConfigScreen && !tab.Equals(default(OpTab)))
                    {
                        CreateWindowPopUp(tab, $"A pack is currently being downloaded.\n\nPlease wait until it finishes.",
                            CustomWorldOption.OptionSignal.CloseWindow, "OK", true);
                    }
                }
                else if (signal.Contains(OptionSignal.TryUninstall.ToString()))
                {

                    try
                    {
                        string packName = Regex.Split(signal, "_")[1];
                        string text = $"Do you want to uninstall [{packName}]?\n\n Uninstalling will permanently delete the pack folder. ";
                        string safelyDeleteDependencies = 
                           $"You will be able to safely disable/uninstall the following dependencies:\n" +
                           $"[{string.Join(", ",CustomWorldMod.installedDependencies.FindAll(x => x.usedBy.Contains(packName) && x.usedBy.Count == 1).Select(x => x.assemblyName).ToArray())}]";
                        if (CustomWorldMod.activatedPacks.ContainsKey(packName))
                        {
                            text += safelyDeleteDependencies;
                        }

                        OpTab tab = CompletelyOptional.ConfigMenu.currentInterface.Tabs.First(x => !x.isHidden);

                        if (OptionInterface.IsConfigScreen && !tab.Equals(default(OpTab)))
                        {
                            CreateWindowPopUp(tab, text, $"{OptionSignal.Uninstall}_{packName}", "Uninstall", true, buttonText2: "Cancel");
                        }
                    }
                    catch (Exception e) { CustomWorldMod.Log("Uninstall failed: " + e); }
                }
                else if (signal.Contains(OptionSignal.Uninstall.ToString()))
                {
                    // Uninstall
                    try
                    {
                        CRExtras.TryPlayMenuSound(SoundID.HUD_Exit_Game);
                        WindowCM current = GetActiveWindow();
                        if (current != null)
                        {
                            current.ShowLoading();
                        }
                        string packName = Regex.Split(signal, "_")[1];
                        string folderName = CustomWorldMod.installedPacks[packName].folderName;

                        string pathFolder = CRExtras.BuildPath(folderName, CRExtras.CustomFolder.None);

                        CustomWorldMod.Log($"[WARNING] Deleting pack at [{pathFolder}]");
                        Directory.Delete(pathFolder, true);
                        CustomWorldMod.LoadCustomWorldResources();

                    }
                    catch (Exception e) { CustomWorldMod.Log($"Could not uninstall pack [{signal}] {e}", true); }
                }
                else if (signal.Contains(OptionSignal.TryDisableToggle.ToString()))
                {
                    // try disable
                    try
                    {
                        string packName = Regex.Split(signal, "_")[1];
                        RegionPack pack = CustomWorldMod.installedPacks[packName];
                        string action = pack.activated ? "Disable" : "Enable";
                        string text = $"Do you want to {action.ToLower()} [{packName}]?\n\n Enabling / disabling packs might corrupt your saves! " +
                            $"You can always enable / disable it again if problems arise.\n";
                        string safelyDeleteDependencies =
                           $"You will be able to safely disable/uninstall the following dependencies:\n" +
                           $"[{string.Join(", ", CustomWorldMod.installedDependencies.FindAll(x => x.usedBy.Contains(packName) && x.usedBy.Count == 1).Select(x => x.assemblyName).ToArray())}]";
                        text += safelyDeleteDependencies;

                        OpTab tab = CompletelyOptional.ConfigMenu.currentInterface.Tabs.First(x => !x.isHidden);
                        if (OptionInterface.IsConfigScreen && !tab.Equals(default(OpTab)))
                        {
                            CreateWindowPopUp(tab, text, $"{OptionSignal.DisableToggle}_{packName}", action, true, buttonText2: "Cancel");
                        }
                    }
                    catch (Exception e) { CustomWorldMod.Log("DisabelToggled failed: " + e); }
                }
                else if (signal.Contains(OptionSignal.DisableToggle.ToString()))
                {
                    try
                    {
                        CRExtras.TryPlayMenuSound(SoundID.HUD_Exit_Game);
                        WindowCM current = GetActiveWindow();
                        if (current != null)
                        {
                            current.ShowLoading();
                        }
                        OpTab tab = CompletelyOptional.ConfigMenu.currentInterface.Tabs.First(x => !x.isHidden);
                        string packName = Regex.Split(signal, "_")[1];
                        
                        CRExtras.DisableTogglePack(packName);
                    }
                    catch (Exception e) { CustomWorldMod.Log($"Could not disable pack [{signal}] {e}", true); }
                }
                else if (signal.Contains(OptionSignal.TryWipeProgress.ToString()))
                {

                    try
                    {
                        string text = $"Do you want to reset all progress?\n" +
                            $"WARNING!\n" +
                            $"This will reset all progress in the selected save slot,\n" +
                            $"including map exploration. Unlocked arenas and\n" +
                            $"sandbox items are retained.";
                        OpTab tab = CompletelyOptional.ConfigMenu.currentInterface.Tabs.First(x => !x.isHidden);

                        if (OptionInterface.IsConfigScreen && !tab.Equals(default(OpTab)))
                        {
                            CreateWindowPopUp(tab, text, $"{OptionSignal.WipeProgress}", "Wipe", true, buttonText2: "Cancel");
                        }
                    }
                    catch (Exception e) { CustomWorldMod.Log("Wipe failed: " + e); }
                }
                else if (signal.Contains(OptionSignal.WipeProgress.ToString()))
                {
                    // Wipe progress
                    try
                    {
                        CRExtras.TryPlayMenuSound(SoundID.MENU_Switch_Page_In);
                        WindowCM current = GetActiveWindow();
                        if (current != null)
                        {
                            current.ShowLoading();
                        }
                        CustomWorldMod.rainWorldInstance.progression.WipeAll();
                        this.Signal(trigger, OptionSignal.Refresh.ToString());
                    }
                    catch (Exception e) { CustomWorldMod.Log($"Could not wipe progress [{signal}] {e}", true); }
                }
                else
                {
                    CustomWorldMod.Log($"Unknown signal [{signal.ToString()}]", true);
                }
            }
        }

        private void PackBrowser(int tab, string tabName)
        {
            Tabs[tab] = new OpTab(tabName);

            // Header
            OpLabel labelID = new OpLabel(new Vector2(100f, 560), new Vector2(400f, 40f), $"Browse RainDB".ToUpper(), FLabelAlignment.Center, true);
            Tabs[tab].AddItems(labelID);

            OpLabel labelDsc = new OpLabel(new Vector2(100f, 540), new Vector2(400f, 20f), $"Download region packs from RainDB", FLabelAlignment.Center, false);
            Tabs[tab].AddItems(labelDsc);

            Tabs[tab].AddItems(new OpSimpleButton(new Vector2(525, 550), new Vector2(60, 30), OptionSignal.ReloadRegions.ToString(), "Refresh"));

            // Create pack list
            CreateRegionPackList(Tabs[tab], CustomWorldMod.rainDbPacks.Where(x => x.Value.shownInBrowser).ToDictionary(x => x.Key, x => x.Value),
                CustomWorldMod.processedThumbnails, true);

            AddScriptIndicator(Tabs[tab]);

        }

        public void MainTabRedux(int tab, string tabName)
        {
            Tabs[tab] = new OpTab(tabName);

            // MOD DESCRIPTION
            OpLabel labelID = new OpLabel(new Vector2(50, 560), new Vector2(500, 40f), rwMod.ModID.ToUpper(), FLabelAlignment.Center, true);
            Tabs[tab].AddItems(labelID);
            OpLabel labelDsc = new OpLabel(new Vector2(100f, 545), new Vector2(400f, 20f), "Support for custom regions.", FLabelAlignment.Center, false);
            Tabs[tab].AddItems(labelDsc);

            // VERSION AND AUTHOR
            OpLabel labelVersion = new OpLabel(new Vector2(50, 530), new Vector2(200f, 20f), "Version: " + rwMod.Version, FLabelAlignment.Left, false);
            Tabs[tab].AddItems(labelVersion);
            OpLabel labelAuthor = new OpLabel(new Vector2(430, 560), new Vector2(60, 20f), "by Garrakx", FLabelAlignment.Right, false);
            Tabs[tab].AddItems(labelAuthor);

            Tabs[tab].AddItems(new OpSimpleButton(new Vector2(525, 550), new Vector2(60, 30), OptionSignal.ReloadRegions.ToString(), "Reload"));

            OpLabelLong errorLabel = new OpLabelLong(new Vector2(25, 1), new Vector2(500, 15), "", true, FLabelAlignment.Center)
            {
                text = "Any changes made (load order, activating/deactivating) will corrupt the save"
            };

            Tabs[tab].AddItems(errorLabel);
            CreateRegionPackList(Tabs[tab], CustomWorldMod.installedPacks, CustomWorldMod.processedThumbnails, false);

            AddScriptIndicator(Tabs[tab]);

            if (CustomWorldMod.debugLevel == CustomWorldMod.DebugLevel.FULL)
            {
                // Wipe progress this.manager.rainWorld.progression.WipeAll();
                OpSimpleButton wipeProgress = new OpSimpleButton(new Vector2(0, 0), new Vector2(60, 25), OptionSignal.TryWipeProgress.ToString(), "Wipe");
                Tabs[tab].AddItems(wipeProgress);
            }
        }

        private void AddScriptIndicator(OpTab opTab)
        {
            int position = 15;
            OpImage loading = new OpImage(new Vector2(position, 600f - position), "Multiplayer_Time");
            loading.anchor = new Vector2(0.5f, 0.5f);
            loading.description = "Script []";

            OpLabel scriptLabel = new OpLabel(position + loading.sprite.width-5, 600f - position-10f, "", false)
            {
                description = "Script action"
            };

            opTab.AddItems(loading, scriptLabel);
        }

        private void CreateRegionPackList(OpTab tab, Dictionary<string, RegionPack> packs, Dictionary<string, ProcessedThumbnail> processedThumbs, bool raindb)
        {
            //How Many Options
            int numberOfOptions = packs.Count;
            int numberOfExpansions = packs.Values.ToList().FindAll(x => x.expansion).Count();

            CustomWorldMod.Log($"Number of packs [{numberOfOptions}]. Number of expansions [{numberOfExpansions}]", false, CustomWorldMod.DebugLevel.MEDIUM);

            if (numberOfOptions < 1)
            {
                OpLabel label2 = new OpLabel(new Vector2(100f, 450), new Vector2(400f, 20f), "No regions available.", FLabelAlignment.Center, false);
                if (raindb && CustomWorldMod.OfflineMode)
                {
                    label2.text = "Browsing RainDB is not available in offline mode";
                }
                tab.AddItems(label2);
                return;
            }

            // Constant spacing
            int spacing = 24;

            // SIZES AND POSITIONS OF ALL ELEMENTS //
            Vector2 thumbSize = new Vector2(225, 156);
            Vector2 rectSize = new Vector2(475, thumbSize.y + spacing / 2);

            /// calculates new vertical size by: scaling factor = (rectHor / thumbHor)
            /// verticalsize = scaling factor · thumbnailVer
            Vector2 rectBigSize = new Vector2(rectSize.x, rectSize.y * 0.75f + (rectSize.x - spacing / 2) / thumbSize.x * thumbSize.y);

            float contentSize = (spacing + (rectSize.y + spacing) * (numberOfOptions - numberOfExpansions) + (rectBigSize.y * numberOfExpansions + spacing));

            // ---------------------------------- //
            OpScrollBox mainScroll = new OpScrollBox(new Vector2(25, 25), new Vector2(550, 500), contentSize);
            tab.AddItems(mainScroll);

            // Bottom left
            Vector2 rectPos = new Vector2(spacing, mainScroll.GetContentSize());

            for (int i = 0; i < numberOfOptions; i++)
            {
                RegionPack pack = packs.ElementAt(i).Value;
                bool activated = pack.activated;
                bool update = false;
                bool big = pack.expansion;

                // Reset to defaults
                thumbSize = new Vector2(225, 156);
                rectSize = new Vector2(475, thumbSize.y + spacing / 2);

                // Use big size
                if (big) { rectSize = rectBigSize; thumbSize *= (rectSize.x - spacing / 2) / thumbSize.x; }

                rectPos.y -= rectSize.y + spacing;

                // Sizes
                Vector2 bigButtonSize = new Vector2(80, 30);
                Vector2 nameLabelSize = new Vector2(rectSize.x - thumbSize.x - 1.5f * spacing, 25);
                Vector2 descripLabelSize = new Vector2(rectSize.x - thumbSize.x - 1.75f * spacing, rectSize.y - nameLabelSize.y - bigButtonSize.y);
                if (big)
                {
                    // Sizes
                    nameLabelSize.x += thumbSize.x; // eliminate thumbnail size
                    descripLabelSize.x += thumbSize.x; // eliminate thumbnail size
                }

                // Positions
                Vector2 thumbPos = rectPos + new Vector2(spacing / 4f, rectSize.y - thumbSize.y - spacing / 4f);
                Vector2 nameLabelPos = rectPos + new Vector2(spacing * 0.75f + thumbSize.x, rectSize.y - nameLabelSize.y - 5f);
                if (big)
                {
                    nameLabelPos.x -= thumbSize.x;
                    nameLabelPos.y -= thumbSize.y + spacing / 2f;
                }
                Vector2 descLabelPos = nameLabelPos - new Vector2(0, descripLabelSize.y);
                Vector2 iconPosStart = rectPos + new Vector2(spacing / 2f, spacing / 2f);

                Vector2 downloadButtonPos = rectPos + new Vector2(rectSize.x - bigButtonSize.x - spacing / 2f, spacing / 2f);
                Vector2 disableButtonPos = downloadButtonPos - new Vector2(bigButtonSize.x * 0.5f + spacing / 3f, 0);
                Vector2 uninstallButtonPos = disableButtonPos - new Vector2(bigButtonSize.x + spacing / 3f, 0);

                try
                {
                    update = raindb && !activated && pack.checksum != null && pack.checksum != string.Empty &&
                        CustomWorldMod.installedPacks.ContainsKey(pack.name) && CustomWorldMod.installedPacks[pack.name].checksum != null &&
                        !pack.checksum.Equals(CustomWorldMod.installedPacks[pack.name].checksum);
                    /*
                    if (pack.checksum != null)
                    {
                        CustomWorldMod.Log($"[UPDATE] [{pack.name}] at [{pack.folderName}] needs update [{update}]. " +
                            $"Local [{pack.checksum}] <-> RainDB [{CustomWorldMod.installedPacks[pack.name].checksum}]");
                    }
                    */
                }
                catch (Exception e) { CustomWorldMod.Log($"Error checking the checksum for updates {e}"); }

                Color colorEdge = activated ? Color.white : new Color((108f / 255f), 0.001f, 0.001f);


                // RECTANGLE
                OpRect rectOption = new OpRect(rectPos, rectSize, 0.2f)
                {
                    doesBump = activated && !pack.packUrl.Equals(string.Empty)
                };
                CustomWorldMod.Log($"[{pack.name}] Rectpos [{rectPos}], contentSize [{contentSize}-{mainScroll.GetContentSize()}]",
                    false, CustomWorldMod.DebugLevel.FULL);
                mainScroll.AddItems(rectOption);
                // ---------------------------------- //


                //if (thumbnails.TryGetValue(pack.name, out byte[] fileData) && fileData.Length > 0)
                if (processedThumbs.TryGetValue(pack.name, out ProcessedThumbnail procThumb) )
                {
                    /*
                    Texture2D oldTex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    oldTex.LoadImage(fileData); //..this will auto-resize the texture dimensions.

                    Texture2D procTex = CRExtras.ProccessThumbnail(oldTex, pack, thumbSize, activated, raindb);
                    */

                    float alpha = 1f;
                    if (!activated && !raindb)
                    {
                        alpha = 0.75f;
                    }
                    else
                    {
                        colorEdge = procThumb.mainColor;
                    }

                    if (procThumb.data == null)
                    {
                        CustomWorldMod.Log($"[CM MENU] {pack.name} had null thumbnail texture", true);
                    }
                    else
                    {
                        Texture2D texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
                        texture.LoadImage(procThumb.data); //..this will auto-resize the texture dimensions.

                        //TextureScale.Point(texture, (int)(thumbSize.x), (int)(thumbSize.y));

                        //thumbnail.scale *= thumbnail.sprite.y / thumbSize.y;

                        OpImage thumbnail = new OpImage(thumbPos, texture);
                        thumbnail.scale = new Vector2(thumbSize.x / texture.width, thumbSize.y / texture.height);
                        thumbnail.alpha = alpha;

                        mainScroll.AddItems(thumbnail);
                    }
                }
                else
                {
                    // No thumbnail
                    OpImage thumbnail = new OpImage(rectPos + new Vector2((rectSize.y - thumbSize.y) / 2f, (rectSize.y - thumbSize.y) / 2f),
                        "gateSymbol0");
                    mainScroll.AddItems(thumbnail);
                    thumbnail.color = colorEdge;
                    thumbnail.sprite.x += thumbSize.x / 2f - thumbnail.sprite.width / 2f;
                }



                // REGION NAME LABEL
                string nameText = pack.name;
                if (!pack.author.Equals(string.Empty))
                {
                    nameText += " [by " + pack.author.ToUpper() + "]";
                }
                OpLabel labelRegionName = new OpLabel(nameLabelPos, nameLabelSize, "", FLabelAlignment.Left)
                {
                    description = nameText
                };

                // Add load order number if local pack
                if (!raindb)
                {
                    nameText = (i + 1).ToString() + "] " + nameText;
                }
                // Trim in case of overflow
                CRExtras.TrimString(ref nameText, nameLabelSize.x, "...");
                labelRegionName.text = nameText;
                mainScroll.AddItems(labelRegionName);
                // ---------------------------------- //


                // DESCRIPTION LABEL
                OpLabelLong labelDesc = new OpLabelLong(descLabelPos, descripLabelSize, "", true, FLabelAlignment.Left)
                {
                    text = pack.description,
                    verticalAlignment = OpLabel.LabelVAlignment.Top,
                    allowOverflow = false
                };
                mainScroll.AddItems(labelDesc);
                // ---------------------------------- //

                // DOWNLOADS
                if (raindb && pack.downloads > 50)
                {
                    int mag = (int)(Math.Floor(Math.Log10(pack.downloads)) / 3); // Truncates to 6, divides to 2
                    double divisor = Math.Pow(10, mag * 3);
                    double shortNumber = pack.downloads / divisor;
                    string result = shortNumber.ToString().Replace(".", ",");

                    OpLabel totalDownloads = new OpLabel(new Vector2(descLabelPos.x, downloadButtonPos.y), nameLabelSize, $"Downloads: {result}",FLabelAlignment.Left, false);
                    mainScroll.AddItems(totalDownloads);
                }


                rectOption.colorEdge = colorEdge;
                labelDesc.color = Color.Lerp(colorEdge, Color.gray, 0.6f);
                labelRegionName.color = colorEdge;

                if (big)
                {
                    Vector2 dividerPos = rectPos + new Vector2(spacing / 2f, rectSize.y - thumbSize.y - 7f);
                    //OpImage divider = new OpImage(rectPos + new Vector2(thumbSize.x + spacing, rectSize.y - spacing * 1.5f), "listDivider");
                    OpImage divider = new OpImage(dividerPos, "listDivider");
                    mainScroll.AddItems(divider);
                    divider.sprite.alpha = 0.1f;
                    divider.color = colorEdge;
                    divider.sprite.scaleX = 3.5f;
                    divider.sprite.width = rectSize.x - spacing;
                }
                // Add icons
                float iconOffset = 0f;
                float separation = spacing / 2.75f;

                // Custom pearls
                if (CustomWorldMod.customPearls.Values.Any(x => x.packName.Equals(pack.name)))
                {
                    OpImage requImage = new OpImage(iconPosStart, "ScholarB");
                    mainScroll.AddItems(requImage);
                    requImage.description = "Includes custom pearls";
                    requImage.sprite.color = Color.Lerp(new Color(1f, 0.843f, 0f), Color.white, 0.3f);
                    iconOffset += requImage.sprite.width + separation;
                }
                // Requeriments DLL
                if (!pack.requirements.Equals(string.Empty))
                {
                    OpImage requImage = new OpImage(iconPosStart + new Vector2(iconOffset, 0), "Kill_Daddy");
                    mainScroll.AddItems(requImage);
                    requImage.description = pack.requirements;
                    requImage.sprite.color = Color.Lerp(Color.blue, Color.white, 0.2f);
                    iconOffset += requImage.sprite.width + separation;
                }
                // Custom Colors
                if (pack.regionConfig != null && pack.regionConfig.Count > 0)
                {
                    OpImage requImage = new OpImage(iconPosStart + new Vector2(iconOffset, 0), "Kill_White_Lizard");
                    mainScroll.AddItems(requImage);
                    requImage.description = "Includes custom region variations";
                    requImage.sprite.color = Color.Lerp(Custom.HSL2RGB(UnityEngine.Random.Range(0f, 1f), 0.6f, 0.6f), Color.white, 0.2f);
                    iconOffset += requImage.sprite.width + separation;
                }

                // False button + text inside
                OpLabel installedLabel = new OpLabel(downloadButtonPos, new Vector2(80, 30), "", FLabelAlignment.Center, false);
                OpRect falseButtonRect = new OpRect(downloadButtonPos, new Vector2(80, 30))
                {
                    colorEdge = colorEdge
                };

                if (raindb)
                {
                    bool unav = false;
                    if ((activated || update) && !(unav = pack.packUrl.Equals(string.Empty)))
                    {
                        string text = update ? "Update" : "Download";
                        string signal = update ? $"{OptionSignal.Update}_{pack.name}" : $"{OptionSignal.Download}_{pack.name}";

                        // Download or Update
                        OpSimpleButton button = new OpSimpleButton(downloadButtonPos, new Vector2(80, 30), signal, text)
                        {
                            colorEdge = update ? Color.green : colorEdge
                        };
                        mainScroll.AddItems(button);
                    }
                    else
                    {
                        string text = unav ? "Unavailable" : "Installed";
                        // Installed
                        installedLabel.text = text;
                        installedLabel.color = Color.Lerp(colorEdge, unav ? Color.red : Color.green, 0.25f);
                        mainScroll.AddItems(installedLabel, falseButtonRect);
                    }

                }
                else
                {
                    // Version
                    if (activated)
                    {
                        installedLabel.text = $"v{pack.version}";
                        installedLabel.pos = downloadButtonPos + new Vector2(20, 0);
                        falseButtonRect = new OpRect(downloadButtonPos + new Vector2(40, 0), new Vector2(42, 30));
                        falseButtonRect.colorEdge = colorEdge;
                        mainScroll.AddItems(installedLabel, falseButtonRect);
                    }
                    /*
                    else
                    {
                        installedLabel.text = "Disabled";
                        installedLabel.color = colorEdge;
                    }
                    */

                    // Add buttons
                    OpSimpleButton disableButton = new OpSimpleButton(disableButtonPos, bigButtonSize,
                        $"{OptionSignal.TryDisableToggle}_{pack.name}", activated ? "Disable" : "Enable");

                    disableButton.colorEdge = colorEdge;
                    OpSimpleButton uninstallButton = new OpSimpleButton(uninstallButtonPos, bigButtonSize,
                        $"{OptionSignal.TryUninstall}_{pack.name}", "Uninstall");

                    uninstallButton.colorEdge = colorEdge;
                    mainScroll.AddItems(disableButton, uninstallButton);
                }

                if (update)
                {
                    updateAvailableTabWarning = true;
                }

            }
        }


        public enum OptionSignal
        {
            Empty,
            // Refresh config menu list
            Refresh,
            ReloadRegions,
            Download,
            Update,
            CloseGame,
            CloseWindow,
            TryDisableToggle,
            TryUninstall,
            DisableToggle,
            Uninstall,
            TryWipeProgress,
            WipeProgress
        }

        /// <summary>
        /// Creates a window popup in the CM menu.
        /// </summary>
        /// <param name="contentText"></param>
        /// <param name="tab"></param>
        /// <param name="signal"></param>
        /// <param name="buttonText1"></param>
        /// <param name="error"></param>
        public static WindowCM CreateWindowPopUp(OpTab tab, string contentText, string signalEnum1, string buttonText1, 
            bool error, string buttonText2 = null)
        {
            WindowCM foundWindow = null;
            foreach (WindowCM window in windows)
            {
                if (window.tab.Equals(tab))
                {
                    foundWindow = window;
                    break;
                }
            }

            if (foundWindow == null)
            {
                // Add new window
                foundWindow = new WindowCM(tab, contentText, signalEnum1, buttonText1, error, buttonCancelText: buttonText2);
                windows.Add(foundWindow);
            }
            else
            {
                // Update window contents
                foundWindow.UpdateWindow(contentText, signalEnum1, buttonText1, error, buttonCancelText: buttonText2);
            }

            return foundWindow;
        }

        public static WindowCM CreateWindowPopUp(OpTab tab, string contentText, CustomWorldOption.OptionSignal signalEnum1, string buttonText1, bool error,
           string buttonText2 = null)
        {
            return CreateWindowPopUp(tab, contentText, signalEnum1.ToString(), buttonText1, error, buttonText2: buttonText2);
        }

        public WindowCM GetActiveWindow()
        {
            if (windows != null)
            {
                foreach (WindowCM window in windows)
                {
                    OpTab tab = window.tab;
                    if (window.added && !tab.isHidden)
                    {
                        return window;
                    }
                }
            }

            return null;
        }


        private void AnalyserSaveTab(int tab, string tabName)
        {
            Tabs[tab] = new OpTab(tabName);

            OpLabel labelID = new OpLabel(new Vector2(100f, 560), new Vector2(400f, 40f), "Analyzer".ToUpper(), FLabelAlignment.Center, true);
            Tabs[tab].AddItems(labelID);

            string errorLog = CustomWorldMod.analyzingLog;

            if (errorLog.Equals(string.Empty))
            {
                errorLog = "After running loading the game once, any problems will show here.";
            }
            else
            {
                errorTabWarning = true;
            }

            OpLabel errorLabel = new OpLabelLong(new Vector2(25, 540), new Vector2(550, 20), "", true, FLabelAlignment.Center)
            {
                text = errorLog
            };

            Tabs[tab].AddItems(errorLabel);

            int saveSlot = 0;
            try
            {
                saveSlot = CustomWorldMod.rainWorldInstance.options.saveSlot;
            }
            catch (Exception e) { CustomWorldMod.Log("Crashed on config " + e, true); }

            // SAVE SLOT
            OpLabel labelID2 = new OpLabel(new Vector2(100f, 320), new Vector2(400f, 40f), $"Analyze Save Slot {saveSlot + 1}".ToUpper(), FLabelAlignment.Center, true);
            Tabs[tab].AddItems(labelID2);

            OpLabel labelDsc2 = new OpLabel(new Vector2(100f, 300), new Vector2(400f, 20f), $"Check problems in savelot {saveSlot + 1}", FLabelAlignment.Center, false);
            Tabs[tab].AddItems(labelDsc2);

            OpLabel errorLabel2 = new OpLabelLong(new Vector2(25, 200), new Vector2(550, 20), "", true, FLabelAlignment.Center)
            {
                text = "No problems found in your save :D"
            };

            Tabs[tab].AddItems(errorLabel);

            try
            {
                if (!CustomWorldMod.saveProblems[saveSlot].AnyProblems)
                {
                    return;
                }
            }
            catch (Exception e) { CustomWorldMod.Log("Crashed on config " + e, true); return; }

            errorLabel2.text = "If your save is working fine you can ignore these errors";

            List<string> problems = new List<string>();

            // problem with the installation
            if (CustomWorldMod.saveProblems[saveSlot].installedRegions)
            {
                string temp = string.Empty;
                if (CustomWorldMod.saveProblems[saveSlot].extraRegions != null && CustomWorldMod.saveProblems[saveSlot].extraRegions.Count > 0)
                {
                    temp = "EXTRA REGIONS\n";
                    temp += "You have installed / enabled new regions without clearing your save. You will need to uninstall / disable the following regions:\n";
                    temp += $"\nExtra Regions [{string.Join(", ", CustomWorldMod.saveProblems[saveSlot].extraRegions.ToArray())}]";
                    problems.Add(temp);
                }
                if (CustomWorldMod.saveProblems[saveSlot].missingRegions != null && CustomWorldMod.saveProblems[saveSlot].missingRegions.Count > 0)
                {
                    temp = "MISSING REGIONS\n";
                    temp += "You have uninstalled / disabled some regions without clearing your save. You will need to install / enable the following regions:\n";
                    temp += $"\nMissing Regions [{string.Join(", ", CustomWorldMod.saveProblems[saveSlot].missingRegions.ToArray())}]";
                    problems.Add(temp);
                }
            }
            // problem with load order
            else if (CustomWorldMod.saveProblems[saveSlot].loadOrder)
            {
                List<string> expectedOrder = new List<string>();
                foreach (RegionPack info in CustomWorldMod.packInfoInSaveSlot[saveSlot])
                {
                    expectedOrder.Add(info.name);
                }
                string temp2 = "INCORRECT ORDER\n";
                temp2 += "You have changed the order in which regions are loaded:\n";
                temp2 += $"Expected order [{string.Join(", ", expectedOrder.ToArray())}]\n";
                temp2 += $"\nInstalled order [{string.Join(", ", CustomWorldMod.activatedPacks.Keys.ToArray())}]";
                problems.Add(temp2);
            }

            // problem with check sum
            if (CustomWorldMod.saveProblems[saveSlot].checkSum != null && CustomWorldMod.saveProblems[saveSlot].checkSum.Count != 0)
            {
                string temp3 = "CORRUPTED FILES\n";
                temp3 += "\nYou have modified the world files of some regions:\n";
                temp3 += $"\nCorrupted Regions [{string.Join(", ", CustomWorldMod.saveProblems[saveSlot].checkSum.ToArray())}]";
                problems.Add(temp3);
            }



            //How Many Options
            int numberOfOptions = problems.Count;

            if (numberOfOptions < 1)
            {
                OpLabel label2 = new OpLabel(new Vector2(100f, 350), new Vector2(400f, 20f), "No regions problems found.", FLabelAlignment.Center, false);
                Tabs[tab].AddItems(label2);
                return;
            }
            errorTabWarning = true;
            int spacing = 25;

            Vector2 rectSize = new Vector2(475, 125);
            OpScrollBox mainScroll = new OpScrollBox(new Vector2(25, 25), new Vector2(550, 250), (int)(spacing + ((rectSize.y + spacing) * numberOfOptions)));
            Vector2 rectPos = new Vector2(spacing, mainScroll.GetContentSize() - rectSize.y - spacing);
            Vector2 labelSize = new Vector2(rectSize.x - 2 * spacing, rectSize.y - 2 * spacing);
            Tabs[tab].AddItems(mainScroll);

            for (int i = 0; i < numberOfOptions; i++)
            {
                Color colorEdge = new Color((108f / 255f), 0.001f, 0.001f);

                OpRect rectOption = new OpRect(rectPos, rectSize, 0.2f)
                {
                    colorEdge = colorEdge
                };

                mainScroll.AddItems(rectOption);

                OpLabelLong labelRegionName = new OpLabelLong(rectPos + new Vector2(spacing, spacing), labelSize, "", true, FLabelAlignment.Left)
                {
                    text = problems[i],
                    color = Color.white,
                    verticalAlignment = OpLabel.LabelVAlignment.Center
                };
                mainScroll.AddItems(labelRegionName);

                rectPos.y -= rectSize.y + spacing;

            }
        }

    }

    public class WindowCM
    {
        OpLabelLong label;
        OpSimpleButton button1;
        OpRect restartPop;
        OpImage cross;
        OpImage loading;
        OpSimpleButton buttonCancel;
        int spacing = 30;
        Vector2 buttonSize;
        Vector2 rectSize;
        Vector2 rectPos;
        Vector2 labelSize;
        Color color;

        Vector2 button1Pos;

        float doubleButtonOffset;

        public bool showLoading;
        public bool added;
        public bool opened;
        public OpTab tab;

        bool doubleButton;
        bool shouldShowLoading;

        public WindowCM(OpTab tab, string contentText, string signal1, string buttonText1, bool error, string buttonCancelText = null)
        {
            buttonSize = new Vector2(70, 35);
            rectSize = new Vector2(420, 135 + buttonSize.y);
            rectPos = new Vector2(300 - rectSize.x / 2f, 300 - rectSize.y / 2);
            labelSize = rectSize - new Vector2(spacing, spacing + buttonSize.y + spacing);
            color = !error ? Color.white : Color.red;

            button1Pos = new Vector2(rectPos.x + (rectSize.x - buttonSize.x) / 2f, rectPos.y + spacing / 2f);

            doubleButtonOffset = 0;
            doubleButton = buttonCancelText != null;

            if (doubleButton)
            {
                doubleButtonOffset = buttonSize.x / 2f + spacing / 2f;
            }

            string labelText = contentText;
            string symbol = error ? "Menu_Symbol_Clear_All" : "Menu_Symbol_CheckBox";

            added = false;

            CustomWorldMod.Log("[WINDOW] Called window", false, CustomWorldMod.DebugLevel.MEDIUM);


            restartPop = new OpRect(rectPos, rectSize, 0.9f);

            button1 = new OpSimpleButton(button1Pos - new Vector2(doubleButtonOffset, 0), buttonSize, signal1, buttonText1);

            buttonCancel = new OpSimpleButton(button1Pos + new Vector2(doubleButtonOffset, 0), buttonSize,
                CustomWorldOption.OptionSignal.CloseWindow.ToString(), buttonCancelText == null ? "NaN" : buttonCancelText);
            buttonCancel.description = "Cancel";

            loading = new OpImage(new Vector2(300f, 300f), "Multiplayer_Time");
            loading.anchor = new Vector2(0.5f, 0.5f);
            loading.description = "Waiting...";

            //loading.sprite.scale *= 2f;
            //loading.pos -= new Vector2(loading.sprite.width / 2f, loading.sprite.height / 2f);

            label = new OpLabelLong(new Vector2(rectPos.x + spacing / 2, rectPos.y + buttonSize.y + spacing * 1.5f), labelSize, "", true, FLabelAlignment.Center)
            {
                text = labelText,
                verticalAlignment = OpLabel.LabelVAlignment.Top
            };

            cross = new OpImage(new Vector2(rectPos.x + spacing / 2f, rectPos.y + rectSize.y - spacing), symbol);
            cross.sprite.color = color;
            this.tab = tab;
            showLoading = false;
        }

        public void UpdateWindow(string contentText, string signal1, string buttonText1, bool error, string buttonCancelText = null)
        {
            CustomWorldMod.Log("[WINDOW] Updating window...", false, CustomWorldMod.DebugLevel.MEDIUM);

            ShowWindow();

            color = !error ? Color.white : Color.red;

            doubleButtonOffset = 0;
            if (buttonCancelText != null)
            {
                doubleButtonOffset = buttonSize.x / 2f + spacing / 2f;
                this.buttonCancel.text = buttonCancelText;
                this.buttonCancel.Show();
            }
            else
            {
                this.buttonCancel.Hide();
            }

            if (signal1.Equals(CustomWorldOption.OptionSignal.Empty.ToString()))
            {
                button1.Hide();
            }

            label.text = contentText;
            string symbol = error ? "Menu_Symbol_Clear_All" : "Menu_Symbol_CheckBox";

            cross.ChangeElement(symbol);
            cross.sprite.color = color;

            button1.signal = signal1;
            button1.text = buttonText1;
            button1.pos = button1Pos - new Vector2(doubleButtonOffset, 0);
            buttonCancel.pos = button1Pos + new Vector2(doubleButtonOffset, 0);


        }

        public List<UIelement> WindowContents
        {
            get
            {
                return GetType().GetFields(
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).
                    Select(field => field.GetValue(this)).OfType<UIelement>().ToList(); // ignores null values
            }
        }

        public bool ShouldShowLoading { get => shouldShowLoading; set => shouldShowLoading = value; }

        public void HideWindow()
        {
            CustomWorldMod.Log("[WINDOW] Hiding window...", false, CustomWorldMod.DebugLevel.MEDIUM);
            var fieldValues = WindowContents;

            foreach (UIelement item in fieldValues.OfType<UIelement>())
            {
                item.Hide();
            }
            this.opened = false;
            showLoading = false;
        }

        public void ShowWindow()
        {
            CustomWorldMod.Log("[WINDOW] Showing window...", false, CustomWorldMod.DebugLevel.MEDIUM);
            var fieldValues = WindowContents;
            if (!ShouldShowLoading)
            {
                this.loading.Hide();
            }
            else
            {
                ShowLoading();
            }
            if (!doubleButton)
            {
                this.buttonCancel.Hide();
            }

            foreach (UIelement item in fieldValues.OfType<UIelement>())
            {
                if (item.description.Equals(loading.description) || item.description.Equals(buttonCancel.description))
                {
                    // Workaround for CM crashing, since Hide / Show will crash CM if called before adding to the tab
                    continue;
                }
                item.Show();
            }
            if (this.button1.signal.Equals(CustomWorldOption.OptionSignal.Empty.ToString()))
            {
                button1.Hide();
            }

            this.opened = true;
        }

        public void ShowLoading()
        {
            CustomWorldMod.Log("[WINDOW] Showing loading...", false, CustomWorldMod.DebugLevel.MEDIUM);
            loading.Show();
            showLoading = true;
        }

        public void AddElements()
        {
            var fieldValues = WindowContents;

            this.tab.AddItems(fieldValues.OfType<UIelement>().ToArray());

            ShowWindow();
        }

        internal void UpdateLoadingRotation(float dt, float rotation)
        {
            //CustomWorldMod.Log($"[WINDOW] Rotating loading... [{this.loading.sprite.rotation}]", false, CustomWorldMod.DebugLevel.FULL);
            //this.loading.sprite.rotation = Mathf.Lerp(this.loading.sprite.rotation, this.loading.sprite.rotation + 10, dt * 10);
            this.loading.sprite.rotation = rotation;
            this.loading.GrafUpdate(dt);
        }
    }
}
