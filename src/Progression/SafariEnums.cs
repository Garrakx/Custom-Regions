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
                foreach (string regionName in CustomStaticCache.SafariRegions)
                {
                    if (MultiplayerUnlocks.SafariUnlockID.values.entries.Contains(regionName))
                    { 
                        CustomRegionsMod.CustomLog($"[SAFARI UNLOCKS] region [{regionName}] already has safari unlock"); 
                        continue; 
                    }

                    CustomRegionsMod.CustomLog($"[SAFARI UNLOCKS] unlock is found for [{regionName}]");
                    CustomSafariUnlocks.Add(new MultiplayerUnlocks.SafariUnlockID(regionName, true));
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
