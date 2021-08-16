using RWCustom;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using CustomRegions.Mod;
using System.Linq;

namespace CustomRegions.CWorld
{
    public static class RegionGateHook
    {
        public static void ApplyHooks()
        {
            On.RegionGate.ctor += RegionGate_ctor;
            On.RegionGate.Update += RegionGate_Update;
        }

        public static void RemoveHooks()
        {
            On.RegionGate.ctor -= RegionGate_ctor;
            On.RegionGate.Update -= RegionGate_Update;
        }

        static bool loggedError = false;
        private static void RegionGate_Update(On.RegionGate.orig_Update orig, RegionGate self, bool eu)
        {
            orig(self, eu);

            AbstractRoom abstractRoom = self.room.abstractRoom;

            // Old World
            //Debug.Log("Old world: " + name);
            string name = self.room.game.overWorld.activeWorld.name;

            string[] arrayName = Regex.Split(abstractRoom.name, "_");
            string text = "ERROR!";
            if (arrayName.Length == 3)
            {
                for (int i = 1; i < 3; i++)
                {
                    if (arrayName[i] != name)
                    {
                        text = arrayName[i];
                        break;
                    }
                }
            }

            if (!self.room.game.overWorld.regions.Select(x => x.name).Contains(text))
            {
                self.dontOpen = true;
                if(!loggedError)
                {
                    CustomWorldMod.Log($"Gate is blocked. Trying to load a region which is not available [{text}]. " +
                        $"Loaded regions [{string.Join(", ", self.room.game.overWorld.regions.Select(x => x.name).ToArray())}] ", true);
                    loggedError = true;
                }
            }
        }

        /// <summary>
        /// Loads karmaGate requirements
        /// </summary>
        private static void RegionGate_ctor(On.RegionGate.orig_ctor orig, RegionGate self, Room room)
        {

            orig(self, room);

            loggedError = false;

            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
            {
                CustomWorldMod.Log($"Custom Regions: Loading karmaGate requirement for {keyValues.Key}", false, CustomWorldMod.DebugLevel.FULL);
                string karmaLocksText = CRExtras.BuildPath(keyValues.Value, CRExtras.CustomFolder.Gates, file: "locks.txt");
                if (File.Exists(karmaLocksText)) 
                {
                    string[] array = File.ReadAllLines(karmaLocksText);

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

                            CustomWorldMod.Log($"Custom Regions: Found custom karmaGate requirement for {keyValues.Key}. " +
                                $"Gate [{self.karmaRequirements[0]}/{self.karmaRequirements[1]}]");
                            return;
                        }
                    }
                }
            }
        }
    }
}
