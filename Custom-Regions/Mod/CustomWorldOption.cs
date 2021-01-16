using CompletelyOptional;
using OptionalUI;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using static CustomRegions.Mod.CustomWorldStructs;

namespace CustomRegions.Mod
{
    public class CustomWorldOption : OptionInterface
    {

        public CustomWorldOption() : base(CustomWorldMod.mod)
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

            Tabs = new OpTab[3];
            Tabs[0] = new OpTab("Main Tab");
            MainTabRedux(0);

            Tabs[1] = new OpTab("Analyzer");
            AnalyserSaveTab(1);

            Tabs[2] = new OpTab("Browse RainDB");
            PackBrowser(2);
        }

        static List<UIelement> currentWindowPopUp = null;
        private bool updateAvailableTabWarning;
        private bool errorTabWarning;

        float counter = 0;
        public override void Update(float dt)
        {
            base.Update(dt);
            counter += 8f*dt;

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
                    raindbTab.color = Color.Lerp(Color.white, Color.green, 0.5f * (0.65f - Mathf.Sin(counter + Mathf.PI)));
                }
                if (!raindbTab.isHidden && CustomWorldMod.scripts != null)
                {
                    PackDownloader script = CustomWorldMod.scripts.Find(x => x is PackDownloader) as PackDownloader;
                    if (script != null)
                    {
                        if (script.downloadButton == null)
                        {
                            OpSimpleButton downloadButton = (Tabs.First(x => x.name.Equals("Browse RainDB")).items.Find(x => x is OpSimpleButton button && button.signal.Contains(script.packName)) as OpSimpleButton);
                            script.downloadButton = downloadButton;
                        }
                    }

                }
            }
            catch (Exception e) { CustomWorldMod.Log("Error getting downloadButton " + e, true); }
        }

        public override void Signal(UItrigger trigger, string signal)
        {
            base.Signal(trigger, signal);
            if (signal != null)
            {
                CustomWorldMod.Log($"Received menu signal [{signal}]");

                // Refresh config menu list
                if (signal.Equals("refresh"))
                {
                    ConfigMenu.ResetCurrentConfig();
                }
                // Reload pack list
                else if (signal.Equals("reloadRegions"))
                {
                    CustomWorldMod.LoadCustomWorldResources();
                }
                // Downnload a pack X
                else if (signal.Contains("download") || signal.Contains("update"))
                {
                    if (CustomWorldMod.scripts.FindAll(x => x is PackDownloader).Count == 0)
                    {
                        // Process ID of Rain World
                        string ID = System.Diagnostics.Process.GetCurrentProcess().Id.ToString();
                        // Divider used
                        string divider = "<div>";
                        // Name of the pack to download
                        string packName = signal.Substring(signal.IndexOf("_") + 1);
                        string url = "";

                        CustomWorldMod.Log($"Download / update signal from [{packName}]");

                        if (CustomWorldMod.rainDbPacks.TryGetValue(packName, out RegionPack toDownload))
                        {
                            url = toDownload.packUrl;
                        }

                        if (url != null && url != string.Empty)
                        {
                            string arguments = $"{url}{divider}\"{packName}\"{divider}{ID}{divider}" + @"\" + CustomWorldMod.resourcePath + (signal.Contains("update") ? $"{divider}update" : "");
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
                    }
                }
                // Close the game
                else if (signal.Equals("close_game"))
                {
                    CustomWorldMod.Log("Exiting game...");
                    Application.Quit();
                }
                // Close(hide) pop-up window
                else if (signal.Equals("close_window"))
                {
                    if (currentWindowPopUp != null)
                    {
                        OpTab tab = ConfigMenu.currentInterface.Tabs.First(x => x.name.Equals("Browse RainDB"));
                        if (tab != null)
                        {
                            foreach (UIelement item in currentWindowPopUp)
                            {
                                try
                                {
                                    item.Hide();
                                }
                                catch (Exception e) { CustomWorldMod.Log("option " + e, true); }
                            }
                        }
                    }
                }
                else
                {
                    CustomWorldMod.Log($"Unknown signal [{signal}]", true);
                }
            }
        }

        private void PackBrowser(int tab)
        {
            // Header
            OpLabel labelID = new OpLabel(new Vector2(100f, 560), new Vector2(400f, 40f), $"Browse RainDB".ToUpper(), FLabelAlignment.Center, true);
            Tabs[tab].AddItems(labelID);

            OpLabel labelDsc = new OpLabel(new Vector2(100f, 540), new Vector2(400f, 20f), $"Download region packs from RainDB", FLabelAlignment.Center, false);
            Tabs[tab].AddItems(labelDsc);

            Tabs[tab].AddItems(new OpSimpleButton(new Vector2(525, 550), new Vector2(60, 30), "reloadRegions", "Refresh"));

            // Create pack list
            CreateRegionPackList(Tabs[tab], CustomWorldMod.rainDbPacks, CustomWorldMod.downloadedThumbnails, true);
        }

        public void MainTabRedux(int tab)
        {
            // MOD DESCRIPTION
            OpLabel labelID = new OpLabel(new Vector2(50, 560), new Vector2(500, 40f), mod.ModID.ToUpper(), FLabelAlignment.Center, true);
            Tabs[tab].AddItems(labelID);
            OpLabel labelDsc = new OpLabel(new Vector2(100f, 545), new Vector2(400f, 20f), "Support for custom regions.", FLabelAlignment.Center, false);
            Tabs[tab].AddItems(labelDsc);

            // VERSION AND AUTHOR
            OpLabel labelVersion = new OpLabel(new Vector2(50, 530), new Vector2(200f, 20f), "Version: pre-release" /*+ mod.Version*/, FLabelAlignment.Left, false);
            Tabs[tab].AddItems(labelVersion);
            OpLabel labelAuthor = new OpLabel(new Vector2(430, 560), new Vector2(60, 20f), "by Garrakx", FLabelAlignment.Right, false);
            Tabs[tab].AddItems(labelAuthor);

            Tabs[tab].AddItems(new OpSimpleButton(new Vector2(525, 550), new Vector2(60, 30), "reloadRegions", "Reload"));

            OpLabelLong errorLabel = new OpLabelLong(new Vector2(25, 1), new Vector2(500, 15), "", true, FLabelAlignment.Center)
            {
                text = "Any changes made (load order, activating/deactivating) will corrupt the save"
            };

            Tabs[tab].AddItems(errorLabel);
            CreateRegionPackList(Tabs[tab], CustomWorldMod.installedPacks, CustomWorldMod.downloadedThumbnails, false);
        }

        private void CreateRegionPackList(OpTab tab, Dictionary<string, RegionPack> packs, Dictionary<string, byte[]> thumbnails, bool raindb)
        {
            //How Many Options
            int numberOfOptions = packs.Count;

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

            int spacing = 25;

            // SIZES AND POSITIONS OF ALL ELEMENTS //
            Vector2 rectSize = new Vector2(475, 175);
            Vector2 thumbSize = new Vector2(225, 156);
            Vector2 buttonDownloadSize = new Vector2(80, 30);
            Vector2 labelSize = new Vector2(rectSize.x - thumbSize.x - 1.5f * spacing, 25);
            Vector2 descripSize = new Vector2(rectSize.x - thumbSize.x - 1.5f * spacing, rectSize.y - labelSize.y - buttonDownloadSize.y);
            OpScrollBox mainScroll = new OpScrollBox(new Vector2(25, 25), new Vector2(550, 500), (int)(spacing + ((rectSize.y + spacing) * numberOfOptions)));
            Vector2 rectPos = new Vector2(spacing, mainScroll.contentSize - rectSize.y - spacing);
            // ---------------------------------- //

            tab.AddItems(mainScroll);

            for (int i = 0; i < numberOfOptions; i++)
            {
                RegionPack pack = packs.ElementAt(i).Value;
                bool activated = pack.activated;
                bool update = false;
                try
                {
                    update = raindb && !activated && pack.checksum != null && pack.checksum != string.Empty && !pack.checksum.Equals(CustomWorldMod.installedPacks[pack.name].checksum);
                }
                catch { CustomWorldMod.Log("Error checking the checksum for updates"); }
                Color colorEdge = activated ? Color.white : new Color((108f / 255f), 0.001f, 0.001f);
                Color colorInverse = Color.white;
                /*
                if (raindb)
                {
                    colorEdge = Color.white;
                    try
                    {
                        // Online checksum is different from local, needs to be updated.
                        if (!activated && pack.checksum != null && pack.checksum != string.Empty)
                        {
                            update = !pack.checksum.Equals(CustomWorldMod.installedPacks[pack.name].checksum);
                        }
                    }
                    catch { CustomWorldMod.Log("Error checking the checksum for updates"); }
                }
                */

                // RECTANGLE
                OpRect rectOption = new OpRect(rectPos, rectSize, 0.2f)
                {
                    doesBump = activated && !pack.packUrl.Equals(string.Empty)
                };
                mainScroll.AddItems(rectOption);
                // ---------------------------------- //


                // REGION NAME LABEL
                string nameText = pack.name;
                if (!pack.author.Equals(string.Empty))
                {
                    nameText += " [by " + pack.author.ToUpper() + "]";
                }
                OpLabel labelRegionName = new OpLabel(rectPos + new Vector2(thumbSize.x + spacing, 140), labelSize, "", FLabelAlignment.Left)
                {
                    description = nameText
                };

                // Add load order number if local pack
                if (!raindb)
                {
                    nameText = (i + 1).ToString() + "] " + nameText;
                }
                // Trim in case of overflow
                CRExtras.TrimString(ref nameText, labelSize.x, "...");
                labelRegionName.text = nameText;
                mainScroll.AddItems(labelRegionName);
                // ---------------------------------- //


                // DESCRIPTION LABEL
                OpLabelLong labelDesc = new OpLabelLong(rectPos + new Vector2(spacing + thumbSize.x, (rectSize.y - descripSize.y - labelSize.y - 0.5f * spacing)), descripSize, "", true, FLabelAlignment.Left)
                {
                    text = pack.description,
                    verticalAlignment = OpLabel.LabelVAlignment.Top,
                    allowOverflow = false
                };
                mainScroll.AddItems(labelDesc);
                // ---------------------------------- //

                if (thumbnails.TryGetValue(pack.name, out byte[] fileData) && fileData.Length > 0)
                {
                    Texture2D oldTex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    oldTex.LoadImage(fileData); //..this will auto-resize the texture dimensions.

                    Texture2D newTex = new Texture2D(oldTex.width, oldTex.height, TextureFormat.RGBA32, false);
                    Color[] convertedImage = oldTex.GetPixels();
                    List<HSLColor> hslColors = new List<HSLColor>();
                    int numberOfPixels = convertedImage.Length;
                    for (int c = 0; c < numberOfPixels; c++)
                    {
                        // Change opacity if not active
                        if (!activated && !raindb)
                        {
                            convertedImage[c].a *= 0.65f;
                        }
                        HSLColor hslColor = CRExtras.RGB2HSL(convertedImage[c]);
                        if (hslColor.saturation > 0.25 && hslColor.lightness > 0.25 && hslColor.lightness < 0.75f)
                        {
                            hslColors.Add(hslColor);
                        }
                    }
                    float averageLight = 0f;
                    float averageSat = 0f;
                    float medianHue = 0f;

                    // Calculate average light and sat
                    if (hslColors.Count > 0)
                    {
                        foreach (HSLColor color in hslColors)
                        {
                            averageLight += color.lightness / hslColors.Count;
                            averageSat += color.saturation / hslColors.Count;
                        }
                    }
                    // Calculate median hue
                    int half = hslColors.Count() / 2;
                    var sortedColors = hslColors.OrderBy(x => x.hue);
                    if (half != 0 && half < sortedColors.Count())
                    {
                        try
                        {
                            if ((hslColors.Count % 2) == 0)
                            {
                                medianHue = (sortedColors.ElementAt(half).hue + sortedColors.ElementAt(half - 1).hue) / 2;
                            }
                            else
                            {
                                medianHue = sortedColors.ElementAt(half).hue;
                            }
                        } catch (Exception e) { CustomWorldMod.Log($"Cannot calculate median hue [{e}] for [{pack.name}]", true); }
                    }

                    colorInverse = Color.Lerp(Custom.HSL2RGB((medianHue + 0.5f) % 1f, averageSat, averageLight), Color.white, 0.175f);
                    if ( (activated || raindb)  )
                    {
                        if (averageSat > 0.15f)
                        {
                            colorEdge = Color.Lerp(Custom.HSL2RGB(medianHue, averageSat, Mathf.Lerp(averageLight, 0.6f, 0.5f)), Color.white, 0.175f);
                        }
                        else
                        {
                            colorEdge = Custom.HSL2RGB(UnityEngine.Random.Range(0.1f, 0.75f), 0.4f, 0.75f);
                        }
                        CustomWorldMod.Log($"Color for [{pack.name}] - MedianHue [{medianHue}] averageSat [{averageSat}] averagelight [{averageLight}] - Number of pixels [{numberOfPixels}] " +
                                $"Colors [{hslColors.Count()}]", false, CustomWorldMod.DebugLevel.FULL);
                    }
                    hslColors.Clear();

                    newTex.SetPixels(convertedImage);
                    newTex.Apply();
                    TextureScale.Point(newTex, (int)thumbSize.x, (int)thumbSize.y);
                    oldTex = newTex;
                    OpImage thumbnail = new OpImage(rectPos + new Vector2((rectSize.y - thumbSize.y) / 2f, (rectSize.y - thumbSize.y) / 2f), oldTex);
                    mainScroll.AddItems(thumbnail);
                }
                else
                {
                    // No thumbnail
                    OpImage thumbnail = new OpImage(rectPos + new Vector2((rectSize.y - thumbSize.y) / 2f, (rectSize.y - thumbSize.y) / 2f), "gateSymbol0");
                    mainScroll.AddItems(thumbnail);
                    thumbnail.color = colorEdge;
                    thumbnail.sprite.x += thumbSize.x / 2 - thumbnail.sprite.width / 2;
                }

                rectOption.colorEdge = colorEdge;
                labelDesc.color = Color.Lerp(colorEdge, Color.gray, 0.6f);
                labelRegionName.color = colorEdge;

                /*
                // DIVIDER
                OpImage divider = new OpImage(rectPos + new Vector2(thumbSize.x + spacing, rectSize.y - spacing * 1.5f), "listDivider");
                mainScroll.AddItems(divider);
                divider.sprite.alpha = 0.1f;
                divider.color = colorEdge;
                divider.sprite.scaleX = 1.5f;
                //divider.color = Custom.HSL2RGB(averageHSL[0], averageHSL[1], averageHSL[2]);
                */

                float iconOffset = 0f;
                // Custom pearls
                if (CustomWorldMod.customPearls.Values.Any(x => x.packName.Equals(pack.name)))
                {
                    OpImage requImage = new OpImage(rectPos + new Vector2(thumbSize.x + spacing, spacing / 2f), "ScholarB");
                    mainScroll.AddItems(requImage);
                    requImage.description = "Includes custom pearls";
                    requImage.sprite.color = Color.Lerp(new Color(1f, 0.843f, 0f), Color.white, 0.3f);
                    iconOffset += requImage.sprite.width + spacing / 2f;
                }
                // Requeriments DLL
                if (!pack.requirements.Equals(string.Empty))
                {
                    OpImage requImage = new OpImage(rectPos + new Vector2(thumbSize.x + spacing + iconOffset, spacing / 2f), "Kill_Daddy");
                    mainScroll.AddItems(requImage);
                    requImage.description = pack.requirements;
                    requImage.sprite.color = Color.Lerp(Color.blue, Color.white, 0.2f);
                    iconOffset += requImage.sprite.width + spacing / 2f;
                }
                // Custom Colors
                if (pack.regionConfig != null && pack.regionConfig.Count > 0)
                {
                    OpImage requImage = new OpImage(rectPos + new Vector2(thumbSize.x + spacing + iconOffset, spacing / 2f), "Kill_White_Lizard");
                    mainScroll.AddItems(requImage);
                    requImage.description = "Includes custom region variations";
                    requImage.sprite.color = Color.Lerp(Custom.HSL2RGB(UnityEngine.Random.Range(0f, 1f), 0.6f, 0.6f), Color.white, 0.2f);
                    iconOffset += requImage.sprite.width + spacing / 2f;
                }

                Vector2 pos = rectPos + new Vector2(rectSize.x - spacing / 2f - buttonDownloadSize.x, spacing / 2f);
                OpLabel installed = new OpLabel(pos, new Vector2(80, 30), "", FLabelAlignment.Center, false);
                OpRect rect = new OpRect(pos, new Vector2(80, 30))
                {
                    colorEdge = colorEdge
                };

                if (raindb)
                {
                    bool unav = false;
                    if ((activated || update) && !(unav = pack.packUrl.Equals(string.Empty)))
                    {
                        string text = update ? "Update" : "Download";
                        string signal = update ? $"update_{pack.name}" : $"download_{pack.name}";

                        // Download or Update
                        OpSimpleButton button = new OpSimpleButton(pos, new Vector2(80, 30), signal, text)
                        {
                            colorEdge = update ? colorInverse : colorEdge
                        };
                        mainScroll.AddItems(button);
                    }
                    else
                    {
                        string text = unav ? "Unavailable" : "Installed";
                        // Installed
                        installed.text = text;
                        installed.color = Color.Lerp(colorEdge, unav ? Color.red : Color.green, 0.25f);
                        mainScroll.AddItems(installed, rect);
                    }
                }
                else
                {
                    // Version
                    if (activated)
                    {
                        installed.text = $"v{pack.version}";
                    }
                    else
                    {
                        installed.text = "Disabled";
                        installed.color = colorEdge;
                    }
                    mainScroll.AddItems(installed, rect);
                }

                /*
                // Warn about update
                if (update)
                {
                    CustomWorldMod.Log($"Update available for pack [{pack.name}]: Local: [{CustomWorldMod.installedPacks[pack.name].checksum}] vs online [{pack.checksum}]");
                    if (raindb)
                    {
                        // Warn the user
                        updateAvailableTabWarning = true;
                        //tab.color = Color.red;
                    }
                    else
                    {
                        OpLabelLong updateAvailable = new OpLabelLong(rectPos + new Vector2(spacing, spacing), thumbSize - new Vector2(spacing, spacing), "Update available", false, FLabelAlignment.Left)
                        {
                            color = Color.red//colorInverse
                        };
                        mainScroll.AddItems(updateAvailable);
                    }
                }
                */

                if (update)
                {
                    updateAvailableTabWarning = true;
                }
                /*
                OpLabelLong updateAvailable = new OpLabelLong(rectPos + new Vector2(spacing, spacing), thumbSize - new Vector2(spacing, spacing), "Update available", false, FLabelAlignment.Left)
                {
                    color = colorInverse
                };
                mainScroll.AddItems(updateAvailable);
                */

                rectPos.y -= rectSize.y + spacing;
            }
        }

        public static void CreateWindowPopUp(string contentText, OpTab tab, string signal, string buttonText, bool error)
        {
            OpLabelLong label;
            OpSimpleButton closeGameButton;
            OpRect restartPop;
            OpImage cross;
            CustomWorldMod.Log($"Number of items [{tab.items.Count}]");
            int spacing = 30;
            Vector2 buttonSize = new Vector2(70, 35);
            Vector2 rectSize = new Vector2(420, 135 + buttonSize.y);
            Vector2 rectPos = new Vector2(300 - rectSize.x / 2f, 300 - rectSize.y / 2);
            Vector2 labelSize = rectSize - new Vector2(spacing, spacing + buttonSize.y + spacing);
            string labelText = contentText;
            bool isNull = false;
            string symbol = error ? "Menu_Symbol_Clear_All" : "Menu_Symbol_CheckBox";
            Color color = error ? Color.white : Color.red;

            if (currentWindowPopUp == null)
            {
                isNull = true;
                currentWindowPopUp = new List<UIelement>();
            }
            else
            {
                foreach (UIelement item in currentWindowPopUp)
                {
                    item.Show();
                    if (item is OpLabelLong itemTab)
                    {
                        itemTab.text = labelText;
                    }
                    else if (item is OpImage itemTab4)
                    {
                        itemTab4.ChangeElement(symbol);
                        itemTab4.sprite.color = color;
                    }
                }
            }

            if (isNull)
            {
                restartPop = new OpRect(rectPos, rectSize, 0.9f);
                label = new OpLabelLong(new Vector2(rectPos.x + spacing / 2, rectPos.y + buttonSize.y + spacing), labelSize, "", true, FLabelAlignment.Center)
                {
                    text = labelText,
                    verticalAlignment = OpLabel.LabelVAlignment.Top
                };
                closeGameButton = new OpSimpleButton(new Vector2(rectPos.x + (rectSize.x - buttonSize.x) / 2f, rectPos.y + spacing / 2f), buttonSize, signal, buttonText);
                cross = new OpImage(new Vector2(rectPos.x + spacing / 2f, rectPos.y + rectSize.y - spacing), symbol);
                cross.sprite.color = Color.white;
                currentWindowPopUp.Add(cross);
                currentWindowPopUp.Add(restartPop);
                currentWindowPopUp.Add(label);
                currentWindowPopUp.Add(closeGameButton);
                tab.AddItems(restartPop, label, closeGameButton, cross);
            }
        }


        private void AnalyserSaveTab(int tab)
        {
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
            Vector2 rectPos = new Vector2(spacing, mainScroll.contentSize - rectSize.y - spacing);
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
}
