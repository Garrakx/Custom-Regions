using CustomRegions.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CustomRegions.Mod.Structs;

namespace CustomRegions.CustomPearls
{
    internal static class CustomConvo
    {
        public static void ApplyHooks()
        {
            On.Conversation.DataPearlToConversation += Conversation_DataPearlToConversation;
            On.SLOracleBehaviorHasMark.MoonConversation.AddEvents += MoonConversation_AddEvents;
        }

        private static void MoonConversation_AddEvents(On.SLOracleBehaviorHasMark.MoonConversation.orig_AddEvents orig, SLOracleBehaviorHasMark.MoonConversation self)
        {
            try
            {
                orig(self);

                foreach (KeyValuePair<DataPearl.AbstractDataPearl.DataPearlType, CustomPearl> customPearl in Data.CustomDataPearlsList)
                {
                    if (self.id == customPearl.Value.conversationID)
                    {
                        self.PearlIntro();
                        Encryption.LoadEventsFromFile(self, customPearl.Value.filePath);
                        return;
                    }
                }
            }
            catch (Exception e) { throw e; }
        }

        private static Conversation.ID Conversation_DataPearlToConversation(On.Conversation.orig_DataPearlToConversation orig, DataPearl.AbstractDataPearl.DataPearlType type)
        {
            try
            {
                if (Data.CustomDataPearlsList.TryGetValue(type, out CustomPearl customPearl))
                {
                    CustomRegionsMod.CustomLog($"Found custom pearl conversation {customPearl.conversationID}");
                    return customPearl.conversationID; 
                }
            }
            catch (Exception e) { throw e; }

            { return orig(type); }
        }
    }
}
