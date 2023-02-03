using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace CustomRegionsMod.Progression
{
    internal static class SafariEnums
    {
        public static List<MultiplayerUnlocks.SafariUnlockID> CustomSafariUnlocks = new List<MultiplayerUnlocks.SafariUnlockID>();

        public static void Refresh()
        {
            Unregister();
            Register();
        }

        public static void Register()
        {
            try
            {
                CustomRegionsMod.Log("CRS Registering safari unlocks");
            if (Region.GetFullRegionOrder() == null)
            { return; }
             
            StoryRegionsMod.slugcatStoryStruct.CheckIfRegen();
            foreach (string regionName in Region.GetFullRegionOrder())
            {
                CustomRegionsMod.Log($"Checking if region [{regionName}] has safari unlock...");
                if (!StoryRegionsMod.slugcatStoryStruct.NoSafariRegions.Contains(regionName) && !MultiplayerUnlocks.SafariUnlockID.values.entries.Contains(regionName))
                {
                    CustomRegionsMod.Log("unlock is found!");
                    CustomSafariUnlocks.Add(new MultiplayerUnlocks.SafariUnlockID(regionName, true));
                }
                else { CustomRegionsMod.Log("No safari unlock found or already exists"); }
            }
            }
            catch (Exception e) { throw e; }
        }

        public static void Unregister()
        {
            try
            {
                foreach (MultiplayerUnlocks.SafariUnlockID unlock in CustomSafariUnlocks)
                { if (unlock != null) { unlock.Unregister(); } }

                CustomSafariUnlocks = new List<MultiplayerUnlocks.SafariUnlockID>();
            }
            catch (Exception e){ throw e; }
        }
    }
}
