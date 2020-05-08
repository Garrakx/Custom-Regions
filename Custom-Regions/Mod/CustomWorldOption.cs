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
            Tabs = new OpTab[1];
            Tabs[0] = new OpTab("Main Tab");
            mainTab(0);
        }
        

        public override void Update(float dt)
        {
            base.Update(dt);
        }

        public override void ConfigOnChange()
        {
            base.ConfigOnChange();
        }

        //TODO: add configuration for Config Machine
        public void mainTab(int tab)
        {

            //MOD DESCRIPTION
            OpLabel labelID = new OpLabel(new Vector2(100f, 560), new Vector2(400f, 40f), mod.ModID.ToUpper(), FLabelAlignment.Center, true);
            Tabs[0].AddItems(labelID);
            OpLabel labelDsc = new OpLabel(new Vector2(100f, 525), new Vector2(400f, 20f), "Support for custom regions.", FLabelAlignment.Center, false);
            Tabs[0].AddItems(labelDsc);

            //VERSION AND AUTHOR
            OpLabel labelVersion = new OpLabel(new Vector2(420, 550), new Vector2(200f, 20f), "Version: " + mod.Version, FLabelAlignment.Left, false);
            Tabs[0].AddItems(labelVersion);
            OpLabel labelAuthor = new OpLabel(new Vector2(270, 545), new Vector2(60, 20f), "by Garrakx", FLabelAlignment.Right, false);
            Tabs[0].AddItems(labelAuthor);

            //How Many Options
            int numberOfOptions = CustomWorldMod.availableRegions.Count;

            if (numberOfOptions < 1)
            {
                OpLabel label2 = new OpLabel(new Vector2(100f, 600), new Vector2(400f, 20f), "No regions available.", FLabelAlignment.Center, false);
                Tabs[0].AddItems(labelDsc);
                return;
            }

            //string keyCheckBox = "";
            string labelCheck = "";
            string labelDescri = "";

            int spacing = 450;

            Vector2 rectPos = new Vector2(92, spacing);
            Vector2 rectSize = new Vector2(420, 55);
            Vector2 labelSize = new Vector2(275, 27);
            Vector2 descripSize = new Vector2(rectSize.x, 35);


            //int spacing = (420 - numberOfOptions * 100) / (numberOfOptions - 1);

            float rectSizeY = Mathf.Clamp((spacing / numberOfOptions) * 0.75f, 200);

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

                /*OpCheckBox checkBox = new OpCheckBox(rectPos + new Vector2(22, rectSizeY * 0.25f), keyCheckBox, activated); 
                checkBox.greyedOut = true;
                checkBox.colorEdge = Color.white;

                Tabs[tab].AddItems(checkBox);*/


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


                rectPos.y -= (spacing / (numberOfOptions));
                //rectPos.y -= (100 + spacing); //* i;
            }

        }
    }
}
