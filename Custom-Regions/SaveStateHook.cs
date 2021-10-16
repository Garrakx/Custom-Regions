using CustomRegions.Mod;
using System;
using System.Text.RegularExpressions;

namespace CustomRegions
{
    static class SaveStateHook
    {
        public static void ApplyHooks()
        {
            On.SaveState.ctor += SaveState_ctor;
            On.SaveState.LoadGame += SaveState_LoadGame;

            // Pearl
            if (CustomWorldMod.usingBepinex)
            {
                CustomWorldMod.Log($"Using regular hook for On_SaveState_AbstractPhysicalObjectFromString", false, CustomWorldMod.DebugLevel.MEDIUM);
                On.SaveState.AbstractPhysicalObjectFromString += SaveState_AbstractPhysicalObjectFromString;
            }
            else
            {
                CustomWorldMod.Log($"Using Slime_Cubed's fix for On_SaveState_AbstractPhysicalObjectFromString. Consider switching to BepInEx");
                APOFSFix.On_SaveState_AbstractPhysicalObjectFromString += SaveState_AbstractPhysicalObjectFromString;
            }
        }

        private static AbstractPhysicalObject SaveState_AbstractPhysicalObjectFromString(On.SaveState.orig_AbstractPhysicalObjectFromString orig, World world, string objString)
        {
            AbstractPhysicalObject result = orig(world, objString);
            if (result != null && result.type == AbstractPhysicalObject.AbstractObjectType.DataPearl)
            {
                try
                {
                    if (result is DataPearl.AbstractDataPearl dataPearl)
                    {
                        string[] array = Regex.Split(objString, "<oA>");
                        int hash = int.Parse(array[5]);
                        if (CustomWorldMod.customPearls.ContainsKey(hash))
                        {
                            CustomWorldMod.customPearls.TryGetValue(hash, out CustomWorldStructs.CustomPearl customPearl);
                            string pearlName = customPearl.name;
                            DataPearl.AbstractDataPearl.DataPearlType type = (DataPearl.AbstractDataPearl.DataPearlType)Enum.Parse(typeof(DataPearl.AbstractDataPearl.DataPearlType), pearlName);
                            CustomWorldMod.Log($"Loaded custom pearl [{type.ToString()}] Hash [{hash}]");
                            dataPearl.dataPearlType = type;
                        }
                        else if (hash > 0x25)
                        {
                            CustomWorldMod.Log($"Error loading data pearl (AbsPhysObj). You are missing dataPearls.txt or your pack needs to be updated", true);
                        }

                    }

                }
                catch (Exception e){ CustomWorldMod.Log($"Error while loading dataPearl from SaveState [{e}]");  }
            }
            return result;
        }

        private static void SaveState_ctor(On.SaveState.orig_ctor orig, SaveState self, int saveStateNumber, PlayerProgression progression)
        {
            orig(self, saveStateNumber, progression);
            CustomWorldMod.Log($"DEBUG: RegionLoadStrings: [{self.regionLoadStrings.Length}]. RegionNames: [{string.Join(", ", progression.regionNames)}]");
        }

        private static void SaveState_LoadGame(On.SaveState.orig_LoadGame orig, SaveState self, string str, RainWorldGame game)
        {
            // New game, generate regionsave checksum
            if (str == string.Empty)
            {
                CustomWorldMod.Log(CustomWorldMod.GetSaveInformation());
            }
            // Existing game, validate regionsave checksum
            else
            {
                // Check if player playing on existing save before CR

            }

			orig(self, str, game);
        }
    }
}
