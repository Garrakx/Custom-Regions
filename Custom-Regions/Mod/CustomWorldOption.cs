using CompletelyOptional;
using Menu;
using OptionalUI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

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
            return true;
        }

        public override void Initialize()
        {
            base.Initialize();
            Tabs = new OpTab[3];
            Tabs[0] = new OpTab("Main Tab");
            Tabs[1] = new OpTab("Installation");
            Tabs[2] = new OpTab("SaveSlot");
            MainTab(0);
            AnalyseSaveTab(1);
            AnalyseInstallationTab(2);
        }

        public override void Update(float dt)
        {
            base.Update(dt);
        }

        public override void ConfigOnChange()
        {
            base.ConfigOnChange();
        }

        public override void Signal(UItrigger trigger, string signal)
        {
            base.Signal(trigger, signal);
            if(signal != null)
            {
                if (signal.Equals("reloadRegions"))
                {
                    CustomWorldMod.LoadCustomWorldResources();
                    ConfigMenu.ResetCurrentConfig();
                }
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


            
            Tabs[tab].AddItems(new OpSimpleButton(new Vector2(525, 550), new Vector2(60,30), "reloadRegions", "Reload")); 


            //How Many Options
            int numberOfOptions = CustomWorldMod.availableRegions.Count;

            if (numberOfOptions < 1)
            {
                OpLabel label2 = new OpLabel(new Vector2(100f, 600), new Vector2(400f, 20f), "No regions available.", FLabelAlignment.Center, false);
                Tabs[tab].AddItems(labelDsc);
                return;
            }

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

            OpLabel errorLabel = new OpLabelLong(new Vector2(10, 500), new Vector2(600, 20), "", true, FLabelAlignment.Left)
            {
                text = CustomWorldMod.analyzingLog
            };

            if (errorLabel.text.Equals(string.Empty))
            {
                errorLabel.text = "After running loading the game once, any problems will show here.";
            }

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

            OpLabel labelID = new OpLabel(new Vector2(100f, 560), new Vector2(400f, 40f), $"Analyze Save Slot {saveSlot+1}", FLabelAlignment.Center, true);
            Tabs[tab].AddItems(labelID);

            OpLabel labelDsc = new OpLabel(new Vector2(100f, 540), new Vector2(400f, 20f), $"Check problems in savelot {saveSlot+1}", FLabelAlignment.Center, false);
            Tabs[tab].AddItems(labelDsc);

            OpLabel errorLabel = new OpLabelLong(new Vector2(100, 490), new Vector2(600, 20), "", true, FLabelAlignment.Center)
            {
                text = "No problems found in your save :D"
            };

            Tabs[tab].AddItems(errorLabel);

            if (!CustomWorldMod.saveProblems[saveSlot].anyProblems)
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
                foreach(CustomWorldMod.RegionInformation info in CustomWorldMod.regionInfoInSaveSlot[saveSlot])
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
