using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CustomRegions.Mod;
using UnityEngine;


namespace CustomRegions
{

    // This code comes from EasyModPack by topicular
    // Adapted to work with any region by Garrakx

    public static class RegionGateHook
    {
        public static void ApplyHooks()
        {
            On.RegionGate.ctor += RegionGate_ctor;
        }


        /// <summary>
        /// Loads karmaGate requirements
        /// </summary>
        private static void RegionGate_ctor(On.RegionGate.orig_ctor orig, RegionGate self, Room room)
        {
            orig(self, room);
            //if (!enabled) { return; }

            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
            {
                Debug.Log($"Custom Regions: Loading karmaGate requirement for {keyValues.Key}");
                string path = CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar;

                string text = string.Concat(new object[]
                {
                Custom.RootFolderDirectory(),
                path.Replace('/', Path.DirectorySeparatorChar),
                "World",
                Path.DirectorySeparatorChar,
                "selfs",
                Path.DirectorySeparatorChar,
                "locks.txt"
                });

                if (!File.Exists(text)) { return; }

                self.karmaGlyphs[0].Destroy();
                self.karmaGlyphs[1].Destroy();

                string[] array = File.ReadAllLines(text);

                for (int i = 0; i < array.Length; i++)
                {
                    if (Regex.Split(array[i], " : ")[0] == room.abstractRoom.name)
                    {
                        Debug.Log($"Custom Regions: Found custom karmaGate requirement for {keyValues.Key}");
                        self.karmaRequirements[0] = Custom.IntClamp(int.Parse(Regex.Split(array[i], " : ")[1]) - 1, 0, 4);
                        self.karmaRequirements[1] = Custom.IntClamp(int.Parse(Regex.Split(array[i], " : ")[2]) - 1, 0, 4);
                        break;
                    }
                }
            }

            self.karmaGlyphs = new GateKarmaGlyph[2];
            for (int j = 0; j < 2; j++)
            {
                self.karmaGlyphs[j] = new GateKarmaGlyph(j == 1, self, self.karmaRequirements[j]);
                room.AddObject(self.karmaGlyphs[j]);
            }
        }
    }
}
