using CustomRegions.Mod;
using On.RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CustomRegions.CustomPearls
{
    static class PlacedObjectHook
    {
        
        public static void ApplyHooks()
        {
            On.PlacedObject.DataPearlData.FromString += DataPearlData_FromString;
            On.PlacedObject.DataPearlData.ToString += DataPearlData_ToString;
        }

        private static string DataPearlData_ToString(On.PlacedObject.DataPearlData.orig_ToString orig, PlacedObject.DataPearlData self)
        {
            DataPearl.AbstractDataPearl.DataPearlType backUpType = self.pearlType;
            KeyValuePair<int, CustomWorldStructs.CustomPearl> entry = CustomWorldMod.customPearls.FirstOrDefault(x => x.Value.name.Equals(backUpType.ToString()));

            if (!entry.Equals(default(KeyValuePair<int, CustomWorldStructs.CustomPearl>)))
            {
                self.pearlType = (DataPearl.AbstractDataPearl.DataPearlType)entry.Key;
            }
            /*
            if (backUpType > DataPearl.AbstractDataPearl.DataPearlType.Red_stomach)
            {
            }
            */
            CustomWorldMod.Log($"DataPearl to string. PearlType [{self.pearlType}] [{backUpType.ToString()}]");
            string toString = orig(self);
            self.pearlType = backUpType;
            return toString;
        }

        private static void DataPearlData_FromString(On.PlacedObject.DataPearlData.orig_FromString orig, PlacedObject.DataPearlData self, string s)
        {
            try
            {
                orig(self, s);
            } 
            catch (Exception e) { CustomWorldMod.Log("Fatal Error: "+e, true); }

            string[] array = Regex.Split(s, "~");
            try
            {
                if (array.Length >= 5)
                {
                    int hash = int.Parse(array[4]);
                    CustomWorldMod.Log($"Loading datapearl. hash [{hash}]");
                    if (CustomWorldMod.customPearls.ContainsKey(hash))
                    {
                        CustomWorldMod.customPearls.TryGetValue(hash, out CustomWorldStructs.CustomPearl customPearl);
                        string pearlName = customPearl.name;
                        DataPearl.AbstractDataPearl.DataPearlType type = (DataPearl.AbstractDataPearl.DataPearlType)Enum.Parse(typeof(DataPearl.AbstractDataPearl.DataPearlType), pearlName);
                        CustomWorldMod.Log($"Loaded custom pearl [{type.ToString()}] Hash [{hash}]");
                        self.pearlType = type;
                        self.hidden = (array[5] == "1");
                    }
                }
            }
            catch (Exception e){ CustomWorldMod.Log($"Exception loading pearl [{e}]", true);  }
        }
        
    }
}
