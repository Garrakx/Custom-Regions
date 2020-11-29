using CompletelyOptional;
using Menu;
using RWCustom;
using OptionalUI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;
using System.IO;
using static CustomRegions.Mod.CustomWorldStructs;

namespace CustomRegions.Mod
{
    public class CustomWorldOption : OptionInterface
    {

        public CustomWorldOption() : base(CustomWorldScript.mod)
        {
            mod = CustomWorldScript.mod;
        }

        public override bool Configuable()
        {
            return false;
        }

        public override void Initialize()
        {
            base.Initialize();
            Tabs = new OpTab[3];

            Tabs[0] = new OpTab("Main Tab");
            MainTabRedux(0);

            Tabs[1] = new OpTab("Save Slot");
            AnalyseSaveTab(1);

            Tabs[2] = new OpTab("Installation");
            AnalyseInstallationTab(2);
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            if (ThumbnailDownloader.instance != null && ThumbnailDownloader.instance.readyToDelete)
            {
                ThumbnailDownloader.instance.Clear();
                ThumbnailDownloader.instance = null;
            }
        }

        public override void ConfigOnChange()
        {
            base.ConfigOnChange();
        }

        public override void Signal(UItrigger trigger, string signal)
        {
            base.Signal(trigger, signal);
            if (signal != null)
            {
                if (signal.Equals("reloadRegions"))
                {
                    CustomWorldMod.LoadCustomWorldResources();
                    ConfigMenu.ResetCurrentConfig();
                }
            }
        }


        public void MainTabRedux(int tab)
        {

            //MOD DESCRIPTION
            OpLabel labelID = new OpLabel(new Vector2(50, 560), new Vector2(500, 40f), mod.ModID.ToUpper(), FLabelAlignment.Center, true);
            Tabs[tab].AddItems(labelID);
            OpLabel labelDsc = new OpLabel(new Vector2(100f, 545), new Vector2(400f, 20f), "Support for custom regions.", FLabelAlignment.Center, false);
            Tabs[tab].AddItems(labelDsc);

            //VERSION AND AUTHOR
            OpLabel labelVersion = new OpLabel(new Vector2(50, 530), new Vector2(200f, 20f), "Version: " + mod.Version, FLabelAlignment.Left, false);
            Tabs[tab].AddItems(labelVersion);
            OpLabel labelAuthor = new OpLabel(new Vector2(430, 560), new Vector2(60, 20f), "by Garrakx", FLabelAlignment.Right, false);
            Tabs[tab].AddItems(labelAuthor);



            Tabs[tab].AddItems(new OpSimpleButton(new Vector2(525, 550), new Vector2(60, 30), "reloadRegions", "Reload"));


            //How Many Options
            int numberOfOptions = CustomWorldMod.availableRegions.Count;

            if (numberOfOptions < 1)
            {
                OpLabel label2 = new OpLabel(new Vector2(100f, 600), new Vector2(400f, 20f), "No regions available.", FLabelAlignment.Center, false);
                Tabs[tab].AddItems(label2);
                return;
            }

            OpLabel errorLabel = new OpLabelLong(new Vector2(25, 1), new Vector2(500, 15), "", true, FLabelAlignment.Center)
            {
                text = "Green means activated, red means deactivated"
            };

            Tabs[tab].AddItems(errorLabel);

            int spacing = 25;

            Vector2 rectSize = new Vector2(475, 175);
            OpScrollBox mainScroll = new OpScrollBox(new Vector2(25, 25), new Vector2(550, 500), (int)(spacing + ((rectSize.y + spacing) * numberOfOptions)));

            Vector2 descripSize = new Vector2(200, 130);
            Vector2 thumbSize = new Vector2(225, 156);
            Vector2 rectPos = new Vector2(spacing, mainScroll.contentSize - rectSize.y - spacing);
            Vector2 labelSize = new Vector2(thumbSize.x, 25);


            Tabs[tab].AddItems(mainScroll);

            for (int i = 0; i < numberOfOptions; i++)
            {
                bool activated = CustomWorldMod.availableRegions.ElementAt(i).Value.activated;
                Color colorEnabled = activated ? new Color((206f / 255f), 1f, (206f / 255f)) : new Color((108f / 255f), 0.001f, 0.001f);

                /*
                OpRect relieve = new OpRect(rectPos + new Vector2(15, 15), rectSize, 0.3f);
                mainScroll.AddItems(relieve);
                */
                
                OpRect rectOption = new OpRect(rectPos, rectSize, 0.2f)
                {
                    doesBump = activated,
                    colorEdge = colorEnabled//new Color((206f / 255f), 1f, (206f / 255f))

                };
                if (!activated)
                {
                    rectOption.colorEdge = colorEnabled;//new Color((108f / 255f), 0.001f, 0.001f);
                }
                mainScroll.AddItems(rectOption);


                OpLabel labelRegionName = new OpLabel(rectPos + new Vector2(thumbSize.x + spacing, 140), labelSize, "", FLabelAlignment.Left)
                {
                    text = (i + 1).ToString() + ") " + CustomWorldMod.availableRegions.ElementAt(i).Value.regionName,
                    color = colorEnabled// new Color((108f / 255f), 0.001f, 0.001f)
                };
                //Debug.Log(labelBox.text);
                //Tabs[tab].AddItems(labelBox);
                mainScroll.AddItems(labelRegionName);



                string filePath = Custom.RootFolderDirectory() + CustomWorldMod.resourcePath +
                    CustomWorldMod.availableRegions.ElementAt(i).Value.folderName + Path.DirectorySeparatorChar + "thumb.png";

                Texture2D oldTex = new Texture2D((int)thumbSize.x, (int)thumbSize.y);
                if (File.Exists(filePath))
                {
                    byte[] fileData;
                    fileData = File.ReadAllBytes(filePath);

                    oldTex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    oldTex.LoadImage(fileData); //..this will auto-resize the texture dimensions.

                    Texture2D newTex = new Texture2D(oldTex.width, oldTex.height, TextureFormat.RGBA32, false);
                    Color[] convertedImage = oldTex.GetPixels();
                    if (!activated)
                    {
                        for (int c = 0; c < convertedImage.Length; c++)
                        {
                            convertedImage[c].a *= 0.5f;
                        }
                    }
                    newTex.SetPixels(convertedImage);
                    newTex.Apply();

                    TextureScale.Point(newTex, (int)thumbSize.x, (int)thumbSize.y);//(int)thumbSize.x, (int)thumbSize.y );

                    oldTex = newTex;
                }


                OpImage thumbnail = new OpImage(rectPos + new Vector2((rectSize.y - thumbSize.y) / 2f, (rectSize.y - thumbSize.y) / 2f), oldTex);
                mainScroll.AddItems(thumbnail);

                /*
                OpRect thumb = new OpRect(thumbnail.GetPos() + new Vector2(2, 2), thumbSize, 0f);
                mainScroll.AddItems(thumb);
                */


                //descripSize.x = rectSize.x - labelRegionName.text.Length * 7f - 2f;
                OpLabel labelDesc = new OpLabel(rectPos + new Vector2(spacing + thumbSize.x, (rectSize.y - descripSize.y - labelSize.y)), descripSize, "", FLabelAlignment.Left)
                {
                    autoWrap = true,
                    text = CustomWorldMod.availableRegions.ElementAt(i).Value.description,
                    color = colorEnabled//new Color((108f / 255f), 0.001f, 0.001f)
                };
                //Tabs[tab].AddItems(labelDesc);
                mainScroll.AddItems(labelDesc);

                rectPos.y -= rectSize.y + spacing;
                //rectPos.y -= Mathf.Min((spacing / (numberOfOptions)), 150);
            }
        }

        public void MainTab(int tab)
        {

            //MOD DESCRIPTION
            OpLabel labelID = new OpLabel(new Vector2(100f, 560), new Vector2(400f, 40f), mod.ModID.ToUpper(), FLabelAlignment.Center, true);
            Tabs[tab].AddItems(labelID);
            OpLabel labelDsc = new OpLabel(new Vector2(100f, 525), new Vector2(400f, 20f), "Support for custom regions.", FLabelAlignment.Center, false);
            Tabs[tab].AddItems(labelDsc);

            //VERSION AND AUTHOR
            OpLabel labelVersion = new OpLabel(new Vector2(420, 550), new Vector2(200f, 20f), "Version: " + mod.Version, FLabelAlignment.Left, false);
            Tabs[tab].AddItems(labelVersion);
            OpLabel labelAuthor = new OpLabel(new Vector2(270, 545), new Vector2(60, 20f), "by Garrakx", FLabelAlignment.Right, false);
            Tabs[tab].AddItems(labelAuthor);



            Tabs[tab].AddItems(new OpSimpleButton(new Vector2(525, 550), new Vector2(60, 30), "reloadRegions", "Reload"));


            //How Many Options
            int numberOfOptions = CustomWorldMod.availableRegions.Count;

            if (numberOfOptions < 1)
            {
                OpLabel label2 = new OpLabel(new Vector2(100f, 600), new Vector2(400f, 20f), "No regions available.", FLabelAlignment.Center, false);
                Tabs[tab].AddItems(label2);
                return;
            }

            OpLabel errorLabel = new OpLabelLong(new Vector2(25, 490), new Vector2(575, 20), "", true, FLabelAlignment.Center)
            {
                text = "Green means activated, red means deactivated"
            };

            Tabs[tab].AddItems(errorLabel);

            /*int cumulativeScrollSize = 0;
            string labelCheck = "";
            string labelDescri = "";*/
            //int rectSizeY
            //cumulativeScrollSize += (int)rectSize.y;

            int spacing = 30;

            Vector2 rectPos = new Vector2(spacing + 5, spacing);
            Vector2 rectSize = new Vector2(500, 75);
            Vector2 labelSize = new Vector2(275, 27);
            Vector2 descripSize = new Vector2(rectSize.x, 35);

            OpScrollBox mainScroll = new OpScrollBox(new Vector2(25, 20), new Vector2(575, 500), (int)(spacing + ((rectSize.y + spacing) * numberOfOptions)));
            Tabs[tab].AddItems(mainScroll);

            for (int i = numberOfOptions - 1; i >= 0; i--)
            {
                bool activated = CustomWorldMod.availableRegions.ElementAt(i).Value.activated;
                Color colorEnabled = activated ? new Color((206f / 255f), 1f, (206f / 255f)) : new Color((108f / 255f), 0.001f, 0.001f);

                OpRect rectOption = new OpRect(rectPos, rectSize, 0.3f)
                {
                    doesBump = activated,
                    colorEdge = colorEnabled//new Color((206f / 255f), 1f, (206f / 255f))

                };
                if (!activated)
                {
                    rectOption.colorEdge = colorEnabled;//new Color((108f / 255f), 0.001f, 0.001f);
                }
                //Tabs[tab].AddItems(rectOption);
                mainScroll.AddItems(rectOption);


                OpLabel labelBox = new OpLabel(rectPos + new Vector2(20, rectSize.y * 0.30f), labelSize, "", FLabelAlignment.Left)
                {
                    text = CustomWorldMod.availableRegions.ElementAt(i).Value.regionName + ": ",
                    color = colorEnabled// new Color((108f / 255f), 0.001f, 0.001f)
                };
                //Debug.Log(labelBox.text);
                //Tabs[tab].AddItems(labelBox);
                mainScroll.AddItems(labelBox);

                OpLabel orderLabel = new OpLabel(rectPos + new Vector2(10, rectSize.y * 0.30f), labelSize, "", FLabelAlignment.Left)
                {
                    text = (i + 1).ToString()
                };
                mainScroll.AddItems(orderLabel);

                descripSize.x = rectSize.x - labelBox.text.Length * 7f - 2f;
                OpLabel labelDesc = new OpLabel(rectPos + new Vector2(20 + labelBox.text.Length * 7f, rectSize.y * 0.30f), descripSize, "", FLabelAlignment.Left)
                {
                    autoWrap = true,
                    text = CustomWorldMod.availableRegions.ElementAt(i).Value.description,
                    color = colorEnabled//new Color((108f / 255f), 0.001f, 0.001f)
                };
                //Tabs[tab].AddItems(labelDesc);
                mainScroll.AddItems(labelDesc);

                rectPos.y += rectSize.y + spacing;
                //rectPos.y -= Mathf.Min((spacing / (numberOfOptions)), 150);
            }
        }

        private void AnalyseInstallationTab(int tab)
        {
            OpLabel labelID = new OpLabel(new Vector2(100f, 560), new Vector2(400f, 40f), "Analyze installation", FLabelAlignment.Center, true);
            Tabs[tab].AddItems(labelID);

            string errorLog = CustomWorldMod.analyzingLog;

            if (errorLog.Equals(string.Empty))
            {
                errorLog = "After running loading the game once, any problems will show here.";
            }

            OpLabel errorLabel = new OpLabelLong(new Vector2(25, 500), new Vector2(550, 20), "", true, FLabelAlignment.Center)
            {
                text = errorLog
            };

            Tabs[tab].AddItems(errorLabel);
        }

        private void AnalyseSaveTab(int tab)
        {
            int saveSlot = 0;
            try
            {
                saveSlot = CustomWorldMod.rainWorldInstance.options.saveSlot;
            }
            catch (Exception) { }

            OpLabel labelID = new OpLabel(new Vector2(100f, 560), new Vector2(400f, 40f), $"Analyze Save Slot {saveSlot + 1}", FLabelAlignment.Center, true);
            Tabs[tab].AddItems(labelID);

            OpLabel labelDsc = new OpLabel(new Vector2(100f, 540), new Vector2(400f, 20f), $"Check problems in savelot {saveSlot + 1}", FLabelAlignment.Center, false);
            Tabs[tab].AddItems(labelDsc);

            OpLabel errorLabel = new OpLabelLong(new Vector2(25, 500), new Vector2(550, 20), "", true, FLabelAlignment.Center)
            {
                text = "No problems found in your save :D"
            };

            Tabs[tab].AddItems(errorLabel);

            if (!CustomWorldMod.saveProblems[saveSlot].AnyProblems)
            {
                return;
            }

            errorLabel.text = "If your save is working fine you can ignore these errors";

            List<string> problems = new List<string>();

            // problem with the installation
            if (CustomWorldMod.saveProblems[saveSlot].installedRegions)
            {
                string temp = string.Empty;
                if (CustomWorldMod.saveProblems[saveSlot].extraRegions != null && CustomWorldMod.saveProblems[saveSlot].extraRegions.Count > 0)
                {
                    temp += "- You have installed / enabled new regions without clearing your save. You will need to uninstall / disable the following regions:\n";
                    temp += $"Extra Regions [{string.Join(", ", CustomWorldMod.saveProblems[saveSlot].extraRegions.ToArray())}]\n\n";
                }
                if (CustomWorldMod.saveProblems[saveSlot].missingRegions != null && CustomWorldMod.saveProblems[saveSlot].missingRegions.Count > 0)
                {
                    temp += "- You have uninstalled / disabled some regions without clearing your save. You will need to install / enable the following regions:\n";
                    temp += $"Missing Regions [{string.Join(", ", CustomWorldMod.saveProblems[saveSlot].missingRegions.ToArray())}]\n\n";
                }
                problems.Add(temp);
            }

            // problem with load order
            else if (CustomWorldMod.saveProblems[saveSlot].loadOrder)
            {
                string temp2 = string.Empty;
                List<string> expectedOrder = new List<string>();
                foreach (RegionInformation info in CustomWorldMod.regionInfoInSaveSlot[saveSlot])
                {
                    expectedOrder.Add(info.regionID);
                }
                temp2 += "- You have changed the order in which regions are loaded:\n";
                temp2 += $"Expected order [{string.Join(", ", expectedOrder.ToArray())}]\n";
                temp2 += $"Installed order [{string.Join(", ", CustomWorldMod.loadedRegions.Keys.ToArray())}]\n\n";
                problems.Add(temp2);
            }

            // problem with check sum
            if (CustomWorldMod.saveProblems[saveSlot].checkSum != null && CustomWorldMod.saveProblems[saveSlot].checkSum.Count != 0)
            {
                string temp3 = string.Empty;
                temp3 += "\n- You have modified the world files of some regions:\n";
                temp3 += $"Corrupted Regions [{string.Join(", ", CustomWorldMod.saveProblems[saveSlot].checkSum.ToArray())}]\n\n";
                problems.Add(temp3);
            }




            int spacing = 30;

            Vector2 rectPos = new Vector2(spacing + 5, spacing);
            Vector2 rectSize = new Vector2(500, 75);
            Vector2 labelSize = new Vector2(480, 27);
            //Vector2 descripSize = new Vector2(rectSize.x, 35);

            int scrollHeight = (int)(spacing + ((rectSize.y + spacing) * problems.Count));

            OpScrollBox mainScroll = new OpScrollBox(new Vector2(25, 20), new Vector2(575, 500), scrollHeight);
            Tabs[tab].AddItems(mainScroll);
            for (int i = 0; i < problems.Count; i++)
            {

                OpRect rectOption = new OpRect(rectPos, rectSize, 0.3f)
                {
                    colorEdge = new Color((108f / 255f), 0.001f, 0.001f)

                };

                //Tabs[tab].AddItems(rectOption);
                mainScroll.AddItems(rectOption);

                OpLabel labelBox = new OpLabel(rectPos + new Vector2(20, rectSize.y * 0.30f), labelSize, "", FLabelAlignment.Left)
                {
                    text = problems[i],
                    autoWrap = true
                };
                //Debug.Log(labelBox.text);
                //Tabs[tab].AddItems(labelBox);
                mainScroll.AddItems(labelBox);


                /*
                descripSize.x = rectSize.x - labelBox.text.Length * 7f - 2f;
                OpLabel labelDesc = new OpLabel(rectPos + new Vector2(20 + labelBox.text.Length * 7f, rectSize.y * 0.30f), descripSize, "", FLabelAlignment.Left)
                {
                    autoWrap = true,
                    text = CustomWorldMod.availableRegions.ElementAt(i).Value.description,
                };
                */
                // mainScroll.AddItems(labelDesc);

                rectPos.y += rectSize.y + spacing;
                //rectPos.y -= Mathf.Min((spacing / (numberOfOptions)), 150);
            }
        }
    }
}
