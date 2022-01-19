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

        /// <summary>
        /// Add region pre processor to the list.
        /// </summary>
        /// <param name="pre"> Method which filters or modifies API.RegionInfo</param>
        public static void AddRegionPreprocessor(RegionPreprocessor pre)
        {
            CustomWorldMod.Log($"Adding preprocessor! [{pre.Method.Name}]");
            if (CustomWorldMod.regionPreprocessors == null)
            {
                CustomWorldMod.Log("Initiation region preprocessors from other mod...");
                CustomWorldMod.regionPreprocessors = new List<RegionPreprocessor>();
            }
            if (CustomWorldMod.regionPreprocessors.Contains(pre))
            {
                CustomWorldMod.Log($"Adding duplicated preprocessor! [{pre.Method.Name}]", true);
                return;
            }
            CustomWorldMod.regionPreprocessors.Add(pre);
        }

        /// <summary>
        /// Dictionary with all installed region packs, where the Key is the region pack name and the value is a struct with its information.
        /// </summary>
        public static Dictionary<string, CustomWorldStructs.RegionPack> InstalledPacks { get => CustomWorldMod.installedPacks; }

        /// <summary>
        /// Dictionary with activated region packs, where the Key is the region pack name and the value is the folder.
        /// </summary>
        public static Dictionary<string, string> ActivatedPacks { get => CustomWorldMod.activatedPacks; }

        public static List<CustomWorldStructs.PackDependency> InstalledDependencies { get => CustomWorldMod.installedDependencies; }

        /// <summary>
        /// List containing unused dependencies, no region pack is using them so they can be disabled.
        /// </summary>
        public static List<CustomWorldStructs.PackDependency> UnusedDependencies { get => CustomWorldMod.installedDependencies.FindAll(x=>x.usedBy.Count() == 0); }

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

        /// <summary>
        /// Builds a folder path. It will return a specific file if specified, otherwise it will end with backslash. Can also be used to navigate vanilla folders.
        /// </summary>
        /// <param name="regionPackFolder"> Folder name of the region pack. Use null for vanilla path. </param>
        /// <param name="folderName"> Folder desired to build the path to (with spaces or underscore)</param>
        /// <param name="regionID"> Region ID needed for Rooms or RegionID folder.</param>
        /// <param name="file"> If specified, it will append a file after the folder path.</param>
        /// <param name="folder"> If specified, it will append an additional folder path.</param>
        /// <returns>Path built.</returns>
        public static string BuildPath(string regionPackFolder, string folderName, string regionID = null,
            string file = null, string folder = null, bool includeRoot = true)
        {
            CRExtras.CustomFolder folderEnum;
            string folderNameEdited = folderName.Replace(" ", "_");
            try { folderEnum = (CRExtras.CustomFolder)Enum.Parse(typeof(CRExtras.CustomFolder), folderNameEdited); }
            catch ( Exception e) { CustomWorldMod.Log($"Folder [{folderName}] not supported by CRS, did you make a typo? {e}", true); return null; }

            return CRExtras.BuildPath(regionPackFolder, folderEnum, regionID, file, folder, includeRoot);
        }
    }
}
