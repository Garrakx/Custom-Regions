using CustomRegions.Mod;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CustomRegions.DevInterface
{
    static class CustomDecalRepresentationHook
    {
        public static void ApplyHooks()
        {
            On.DevInterface.CustomDecalRepresentation.ctor += CustomDecalRepresentation_ctor;
        }

        public static void RemoveHooks()
        {
            On.DevInterface.CustomDecalRepresentation.ctor -= CustomDecalRepresentation_ctor;
        }

        private static void CustomDecalRepresentation_ctor(On.DevInterface.CustomDecalRepresentation.orig_ctor orig, 
            global::DevInterface.CustomDecalRepresentation self, global::DevInterface.DevUI owner, string IDstring, 
            global::DevInterface.DevUINode parentNode, PlacedObject pObj, string name)
        {
            orig(self, owner, IDstring, parentNode, pObj, name);

            List<string> customDecalFiles = null;

            string customFilePath = string.Empty;
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
            {
                customFilePath = CRExtras.BuildPath(keyValues.Value, CRExtras.CustomFolder.Decals);
                CustomWorldMod.Log($"Looking for decals at [{customFilePath}]");

                if (Directory.Exists(customFilePath))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(customFilePath);

                    if (customDecalFiles == null) { customDecalFiles = new List<string>(); }
                    foreach(FileInfo file in directoryInfo.GetFiles())
                    {
                        if (!file.Name.Contains(".png"))
                        {
                            continue;
                        }
                        string decalName = file.Name.Substring(0, file.Name.IndexOf(".png"));
                        if (!file.Name.Contains("meta") && !customDecalFiles.Contains(decalName) && !self.decalFiles.Contains(decalName)) 
                        {
                            customDecalFiles.Add(decalName);
                        }
                    }
                }
            }
            if (customDecalFiles != null)
            {
                int pointerDecal = self.decalFiles.Length;
                Array.Resize(ref self.decalFiles, pointerDecal + customDecalFiles.Count);
                for (int i = 0; i < customDecalFiles.Count; i++)
                {
                    self.decalFiles[pointerDecal + i] = customDecalFiles[i];
                }
                CustomWorldMod.Log($"Loaded custom decals for DevInterface: [{string.Join(", ", customDecalFiles.ToArray())}]", 
                    false, CustomWorldMod.DebugLevel.MEDIUM);
            }

            
        }
    }
}
