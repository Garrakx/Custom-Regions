using OptionalUI;
using System;
using System.Collections.Generic;
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
            mainTab(0);
            analyseInstallationTab(1);
            AnalyseSaveTab(2);
        }

        public override void Update(float dt)
        {
            base.Update(dt);
        }

        public override void ConfigOnChange()
        {
            base.ConfigOnChange();
        }

        public void mainTab(int tab)
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

            Vector2 rectPos = new Vector2(spacing+5, spacing);
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
                Debug.Log(labelBox.text);
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

            
            
            /*
            //string keyCheckBox = "";
            string labelCheck = "";
            string labelDescri = "";

            int spacing = 450;

            Vector2 rectPos = new Vector2(92, spacing);
            Vector2 rectSize = new Vector2(420, 55);
            Vector2 labelSize = new Vector2(275, 27);
            Vector2 descripSize = new Vector2(rectSize.x, 35);


            //int spacing = (420 - numberOfOptions * 100) / (numberOfOptions - 1);

            // float rectSizeY = Mathf.Min(, 100);
            float rectSizeY = Mathf.Clamp((spacing / numberOfOptions) * 0.75f, 10f, 75f);
            for (int i = 0; i < numberOfOptions; i++)
            {

                //keyCheckBox = CustomWorldMod.availableRegions.ElementAt(i).Key;
                rectSize.y = rectSizeY;
                bool activated = CustomWorldMod.availableRegions.ElementAt(i).Value.activated;

                OpRect rectOption = new OpRect(rectPos, rectSize, 0.3f)
                {
                    doesBump = activated,
                    colorEdge = new Color((206f / 255f), 1f, (206f / 255f))
                    
                };
                if (!activated)
                {
                    rectOption.colorEdge = new Color((108f / 255f), 0.001f, 0.001f);
                }
                Tabs[tab].AddItems(rectOption);



                OpLabel labelBox = new OpLabel(rectPos + new Vector2(20, rectSizeY * 0.25f), labelSize, "", FLabelAlignment.Left);
                labelBox.text = CustomWorldMod.availableRegions.ElementAt(i).Value.regionName + ": ";
                Tabs[tab].AddItems(labelBox);

                descripSize.x = rectSize.x - labelBox.text.Length * 7f;
                OpLabel labelDesc = new OpLabel(rectPos + new Vector2(20 + labelBox.text.Length*7f, rectSizeY * 0.20f), descripSize, "", FLabelAlignment.Left)
                {
                    autoWrap = true,
                    text = CustomWorldMod.availableRegions.ElementAt(i).Value.description
                };
                Tabs[tab].AddItems(labelDesc);


                rectPos.y -= Mathf.Min((spacing / (numberOfOptions)), 100);
                //rectPos.y -= (100 + spacing); //* i;
            }
            */
        }

        private void analyseInstallationTab(int v)
        {
            OpLabel labelID = new OpLabel(new Vector2(100f, 560), new Vector2(400f, 40f), "Analyze installation", FLabelAlignment.Center, true);
            Tabs[v].AddItems(labelID);

            OpLabel errorLabel = new OpLabelLong(new Vector2(10, 500), new Vector2(600, 20), "", true, FLabelAlignment.Left)
            {
                text = CustomWorldMod.analyzingLog
            };

            if (errorLabel.text.Equals(string.Empty))
            {
                errorLabel.text = "After running loading the game once, any problems will show here.";
            }

            Tabs[v].AddItems(errorLabel); 
        }

        private void AnalyseSaveTab(int v)
        {
            OpLabel labelID = new OpLabel(new Vector2(100f, 560), new Vector2(400f, 40f), "Analyze Save Slot", FLabelAlignment.Center, true);
            Tabs[v].AddItems(labelID);

            OpLabel corruptedSave = new OpLabel(new Vector2(10, 500), new Vector2(200, 20), "Coming soon...", FLabelAlignment.Left);
            Tabs[v].AddItems(corruptedSave);

            //string corruptedSave = "[Saveslot 1 is corrupted] / Reason: Missing region / checksum failed";
        }
    }
}
