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
                CustomRegionsMod.CustomLog("[SAFARI UNLOCKS] CRS Registering safari unlocks...");
                if (Region.GetFullRegionOrder() == null) { return; }

                CustomStaticCache.CheckForRefresh();
                foreach (string regionName in Region.GetFullRegionOrder()) {
                    CustomRegionsMod.CustomLog($"[SAFARI UNLOCKS] Checking if region [{regionName}] has safari unlock...", CustomRegionsMod.DebugLevel.FULL);
                    if (!CustomStaticCache.NoSafariRegions.Contains(regionName) && !MultiplayerUnlocks.SafariUnlockID.values.entries.Contains(regionName)) {
                        CustomRegionsMod.CustomLog($"[SAFARI UNLOCKS] unlock is found for [{regionName}]");
                        CustomSafariUnlocks.Add(new MultiplayerUnlocks.SafariUnlockID(regionName, true));
                    } else { CustomRegionsMod.CustomLog("[SAFARI UNLOCKS] No safari unlock found or already exists", CustomRegionsMod.DebugLevel.FULL); }
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
