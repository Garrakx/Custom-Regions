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
            CustomRegionsMod.CustomLog("\n[SAFARI UNLOCKS] CRS Registering safari unlocks...");
            if (Region.GetFullRegionOrder() == null) { return; }

            CustomStaticCache.CheckForRefresh();
            foreach (string regionName in CustomStaticCache.SafariRegions)
            {
                if (MultiplayerUnlocks.SafariUnlockID.values?.entries.Contains(regionName) ?? false)
                {
                    CustomRegionsMod.CustomLog($"[SAFARI UNLOCKS] region [{regionName}] already has safari unlock");
                    continue;
                }

                CustomRegionsMod.CustomLog($"[SAFARI UNLOCKS] unlock is found for [{regionName}]");
                CustomSafariUnlocks.Add(new MultiplayerUnlocks.SafariUnlockID(regionName, true));
            }
        }

        public static void Unregister()
        {
            foreach (MultiplayerUnlocks.SafariUnlockID unlock in CustomSafariUnlocks) { unlock?.Unregister(); }
            CustomSafariUnlocks = new List<MultiplayerUnlocks.SafariUnlockID>();
        }
    }
}
