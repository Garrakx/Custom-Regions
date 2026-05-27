using System;
using System.Collections.Generic;
using Menu.Remix.MixedUI;
using UnityEngine;

namespace CustomRegions.Mod
{
    internal class RemixMenu : OptionInterface
    {
        public static RemixMenu Instance { get; } = new();
        public static Configurable<CustomRegionsMod.DebugLevel> DebugLevel { get; } = Instance.config.Bind("DebugLevel", CustomRegionsMod.DebugLevel.DEFAULT, new ConfigurableInfo("Determines how much CRS should log", tags: "this is a tag"));
        public static Configurable<bool> ThreatStream { get; } = Instance.config.Bind("ThreatStream", true, new ConfigurableInfo("Checked to stream custom threat themes from file, unchecked to load them on threat shuffle (may cause infrequent freezes)"));
        public static Configurable<bool> CacheAlignmentFix { get; } = Instance.config.Bind("CacheAlignmentFix", true, new ConfigurableInfo("Checked to fix room index cache misalignment, a bug which causes items and creatures to disappear from shelters. Only uncheck if offscreen creatures are disappearing"));


        const float labelMargin = 20f;

        const float lineSpacing = 40f;

        const float itemSpacing = 50f;

        readonly Vector2 origin = new(150f, 300f);

        public static void RegisterOptionInterface()
        {
            if (MachineConnector.GetRegisteredOI(CustomRegionsMod.JSON_ID) != Instance)
            {
                MachineConnector.SetRegisteredOI(CustomRegionsMod.JSON_ID, Instance);
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            Tabs = new OpTab[1];
            Tabs[0] = new OpTab(this, "Options");

            Vector2 pos = origin;
            var debugSelector = new OpResourceList(DebugLevel, pos + new Vector2(40f, 0f), 100f);
            debugSelector.OnChange += CustomRegionsMod.SetDebugFromRemix;

            AddLabeledUIelement(debugSelector, "Debug Level", 100f);

            pos.y -= lineSpacing;

            AddLabeledUIelement(new OpCheckBox(ThreatStream, pos), "Stream Threat Music?", 150f);

            pos.y -= lineSpacing;

            AddLabeledUIelement(new OpCheckBox(CacheAlignmentFix, pos), "Fix Room Index Cache Misalignment?", 200f);
        }

        public void AddLabeledUIelement(UIconfig element, string label, float width)
        {
            string description = Translate(element.cfgEntry.info.description);
            element.description = description;
            Vector2 change = new();
            if (element is OpListBox box)
            {
                change = new(0f, box._rectList.size.y);
            }
            var uiLabel = new OpLabel(element.pos + new Vector2(element.size.x + labelMargin, 0f) + change, new Vector2(width, 24f), label, FLabelAlignment.Left)
            { description = description };

            Tabs[0].AddItems(uiLabel, element);
        }
    }
}
