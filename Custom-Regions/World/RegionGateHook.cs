using RWCustom;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using CustomRegions.Mod;


namespace CustomRegions.CWorld
{
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

            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
            {
                CustomWorldMod.Log($"Custom Regions: Loading karmaGate requirement for {keyValues.Key}", false, CustomWorldMod.DebugLevel.FULL);
                string path = CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar;

                /*
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
                */

                string path2 = path + "World" + Path.DirectorySeparatorChar + "Gates" + Path.DirectorySeparatorChar + "locks.txt";
                bool foundKarma = false;
                if (File.Exists(path2))
                {
                    string[] array = File.ReadAllLines(path2);

                    for (int i = 0; i < array.Length; i++)
                    {
                        if (Regex.Split(array[i], " : ")[0] == room.abstractRoom.name)
                        {
                            self.karmaGlyphs[0].Destroy();
                            self.karmaGlyphs[1].Destroy();
                            self.karmaRequirements[0] = Custom.IntClamp(int.Parse(Regex.Split(array[i], " : ")[1]) - 1, 0, 4);
                            self.karmaRequirements[1] = Custom.IntClamp(int.Parse(Regex.Split(array[i], " : ")[2]) - 1, 0, 4);
                            self.karmaGlyphs = new GateKarmaGlyph[2];
                            for (int j = 0; j < 2; j++)
                            {
                                self.karmaGlyphs[j] = new GateKarmaGlyph(j == 1, self, self.karmaRequirements[j]);
                                room.AddObject(self.karmaGlyphs[j]);
                            }

                            CustomWorldMod.Log($"Custom Regions: Found custom karmaGate requirement for {keyValues.Key}. Gate [{self.karmaRequirements[0]}/{self.karmaRequirements[1]}]");
                            foundKarma = true;
                            break;
                        }
                    }
                    if (foundKarma) { break; }
                }
            }
        }
    }
}
