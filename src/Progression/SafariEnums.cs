using CustomRegions.Mod;
using System;
using System.Collections.Generic;

namespace CustomRegions.Progression
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
            try {
                CustomRegionsMod.CustomLog("CRS Registering safari unlocks");
                if (Region.GetFullRegionOrder() == null) { return; }

                StoryRegionsMod.slugcatStoryStruct.CheckIfRegen();
                foreach (string regionName in Region.GetFullRegionOrder()) {
                    CustomRegionsMod.CustomLog($"Checking if region [{regionName}] has safari unlock...");
                    if (!StoryRegionsMod.slugcatStoryStruct.NoSafariRegions.Contains(regionName) && !MultiplayerUnlocks.SafariUnlockID.values.entries.Contains(regionName)) {
                        CustomRegionsMod.CustomLog("unlock is found!");
                        CustomSafariUnlocks.Add(new MultiplayerUnlocks.SafariUnlockID(regionName, true));
                    } else { CustomRegionsMod.CustomLog("No safari unlock found or already exists"); }
                }
            } catch (Exception e) { throw e; }
        }

        public static void Unregister()
        {
            try {
                foreach (MultiplayerUnlocks.SafariUnlockID unlock in CustomSafariUnlocks) { if (unlock != null) { unlock.Unregister(); } }

                CustomSafariUnlocks = new List<MultiplayerUnlocks.SafariUnlockID>();
            } catch (Exception e) { throw e; }
        }
    }
}
