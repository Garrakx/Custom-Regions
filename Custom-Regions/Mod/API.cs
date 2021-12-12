using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CustomRegions.Mod
{
    public static class API
    {

        public class RegionInfo
        {
            /// <summary>
            /// Pack name, empty used for vanilla.
            /// </summary>
            public string PackName { get; internal set; }

            public string RegionID { get; internal set; }
            /// <summary>
            /// World lines from world_XX.txt file.
            /// </summary>
            public List<string> Lines { get; internal set; }

            public bool Vanilla { get; internal set; } = false;
        }

        public delegate void RegionPreprocessor(RegionInfo info);

        public static Dictionary<string, CustomWorldStructs.RegionPack> InstalledPacks { get => CustomWorldMod.installedPacks; }
        public static Dictionary<string, string> ActivatedPacks { get => CustomWorldMod.activatedPacks; }
        public static List<CustomWorldStructs.PackDependency> InstalledDependencies { get => CustomWorldMod.installedDependencies; }

        /// <summary>
        /// Indicates wheter CRS is currently downloading a pack or not.
        /// </summary>
        public static bool DownloadInProcess { get => (CustomWorldMod.scripts.Count > 0 && CustomWorldMod.scripts.FindAll(x => x is PackDownloader).Count != 0); }
        

        /// <summary>
        /// Enables/Disables the provided pack. Make sure to check if there is a download in process first
        /// </summary>
        /// <param name="packName"></param>
        public static void DisableEnablePack(string packName)
        {
            if (DownloadInProcess) { throw new AccessViolationException("Mod tried to disable a pack while CRS was downloading a pack... Use the API.DownloadInProcess bool"); }

            CRExtras.DisableTogglePack(packName);
        }

        /// <summary>
        /// Forces CRS to reset, make sure you know what you are doing. Check for API.DownloadInProcess first.
        /// </summary>
        public static void ForceReloadCRS()
        {
            if (DownloadInProcess) { throw new AccessViolationException("Mod tried to reset CRS while CRS was downloading a pack... Use the API.DownloadInProcess bool"); }
            CustomWorldMod.LoadCustomWorldResources();
        }

    }
}
