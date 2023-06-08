using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RWCustom;

namespace CustomRegions.Mod
{
    internal static class CustomMerge
    {
        public static void MergePearlsAndArenas()
        {
            string filePath = Path.Combine(Path.Combine(Custom.RootFolderDirectory(), "mergedmods"), "custompearls.txt");
            if (!File.Exists(filePath))
            {
                CustomRegionsMod.CustomLog("merging CustomPearls");
                MergeSpecific("CustomPearls.txt");
            }

            filePath = Path.Combine(Path.Combine(Custom.RootFolderDirectory(), "mergedmods"), "CustomUnlocks.txt");
            if (!File.Exists(filePath))
            {
                CustomRegionsMod.CustomLog("merging CustomUnlocks");
                MergeSpecific("CustomUnlocks.txt"); 
            }
        }

        private static void MergeSpecific(string filePath)
        {
            //copied then altered from the back half of ModManager.GenerateMergedMods
            ModManager.ModMerger modMerger = new();

            foreach (ModManager.Mod mod in ModManager.ActiveMods.OrderBy(o => o.loadOrder).ToList())
            {
                string text = Path.Combine(mod.path, Path.Combine("modify", filePath));
                if (!File.Exists(text))
                { continue; }
                string text2 = Path.DirectorySeparatorChar + filePath;

                modMerger.AddPendingApply(mod, text2, text, false, true); //only modification files are added to this list now
            }
            modMerger.ExecuteCustomMerge();
        }

        private static void ExecuteCustomMerge(this ModManager.ModMerger self, ModManager.ModApplyer applyer = null)
        {
            //copied then altered from ModManager.ModMerger.ExecutePendingMerge
            try
            {
                if (applyer != null)
                {
                    applyer.applyFileInd = 0;
                    applyer.applyFileLength = self.moddedFiles.Count;
                }

                foreach (KeyValuePair<string, List<ModManager.ModMerger.PendingApply>> kvp in self.moddedFiles)
                {
                    if (applyer != null)
                    { applyer.applyFileInd++; }

                    string fileName = kvp.Key;
                    List<ModManager.ModMerger.PendingApply> pendingApplies = kvp.Value;

                    //don't generate if is strings.txt
                    if (pendingApplies.Where(x => x.filePath.Contains("strings.txt")).Count() > 0) continue;

                    //find original file
                    string originPath = AssetManager.ResolveFilePath(fileName.Substring(1));
                    if (!File.Exists(originPath))
                    { originPath = ""; }

                    //create base merged file
                    string mergedPath = (Custom.RootFolderDirectory() + Path.DirectorySeparatorChar.ToString() + "mergedmods" + fileName).ToLowerInvariant();
                    Directory.CreateDirectory(Path.GetDirectoryName(mergedPath));

                    if (originPath == "")
                    { File.WriteAllText(mergedPath, ""); } //yes, we might want files that don't have origins

                    else { File.Copy(originPath, mergedPath, true); }

                    //apply modifications
                    List<ModManager.ModMerger.PendingApply> merges = new();
                    List<ModManager.ModMerger.PendingApply> modifications = new();

                    for (int i = 0; i < pendingApplies.Count; i++)
                    {
                        if (pendingApplies[i].mergeLines != null)
                        { merges.Add(pendingApplies[i]); }

                        if (pendingApplies[i].isModification)
                        { modifications.Add(pendingApplies[i]); }
                    }

                    //merges always go first
                    foreach (ModManager.ModMerger.PendingApply pa in merges)
                    {
                        pa.ApplyMerges(pa.modApplyFrom, self, mergedPath);
                    }
                    foreach (ModManager.ModMerger.PendingApply pa in modifications)
                    {
                        pa.ApplyModifications(mergedPath);
                    }
                }
                if (applyer != null)
                {
                    applyer.applyFileInd = 0;
                    applyer.applyFileLength = self.moddedFiles.Count;
                }

            }
            catch (Exception e) { CustomRegionsMod.CustomLog(e.ToString(), true); throw; }
        }

    }
}
