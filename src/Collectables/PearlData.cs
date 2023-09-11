using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CustomRegions.Mod.Structs;
using System.IO;
using UnityEngine;
using CustomRegions.Mod;
using System.Text.RegularExpressions;
using System.CodeDom;

namespace CustomRegions.Collectables
{
    internal static class PearlData
    {
        public static void Refresh()
        {
            Unregister();
            FindCustomPearlData();
            Encryption.EncryptAllCustomPearls();
        }

        public static void FindCustomPearlData()
        {
            try
            {
                CustomDataPearlsList = new Dictionary<DataPearl.AbstractDataPearl.DataPearlType, CustomPearl>();
                string customFilePath = AssetManager.ResolveFilePath("CustomPearls.txt");
                if (!File.Exists(customFilePath)) return;
                foreach (string str in File.ReadAllLines(customFilePath))
                {
                    string[] array = Regex.Split(str, " : ");
                    string pearlName = array[0];
                    CustomRegionsMod.CustomLog("Pearl text name is " + pearlName);

                    if (ExtEnumBase.TryParse(typeof(DataPearl.AbstractDataPearl.DataPearlType), pearlName, false, out _))
                    { continue; }

                    DataPearl.AbstractDataPearl.DataPearlType type = RegisterPearlType(pearlName);


                    Color color = RWCustom.Custom.hexToColor(array[1]);
                    Color colorHighlight = RWCustom.Custom.hexToColor(array[2]);
                    string filePath = array[3];

                    CustomPearl pearl = new CustomPearl(type, color, colorHighlight, filePath, RegisterConversations(pearlName));
                    CustomDataPearlsList.Add(type, pearl);
                }
            }
            catch (Exception e) { CustomRegionsMod.CustomLog("Error loading custom pearls" + e, true); }
        }

        public static void Unregister()
        {
            foreach (KeyValuePair<DataPearl.AbstractDataPearl.DataPearlType, CustomPearl> pearl in CustomDataPearlsList)
            {
                pearl.Value.type?.Unregister();
                pearl.Value.conversationID?.Unregister();
            }
            CustomDataPearlsList = new Dictionary<DataPearl.AbstractDataPearl.DataPearlType, CustomPearl>();
        }

        public static DataPearl.AbstractDataPearl.DataPearlType RegisterPearlType(string name)
        {
            return new DataPearl.AbstractDataPearl.DataPearlType(name, true);
        }

        public static Conversation.ID RegisterConversations(string name)
        {
            return new Conversation.ID(name, true);
        }


        public static Dictionary<DataPearl.AbstractDataPearl.DataPearlType, CustomPearl> CustomDataPearlsList = new();

        #region colors
        public static void ApplyHooks()
        {
            On.DataPearl.ApplyPalette += DataPearl_ApplyPalette;
            On.DataPearl.UniquePearlMainColor += DataPearl_UniquePearlMainColor;
            On.DataPearl.UniquePearlHighLightColor += DataPearl_UniquePearlHighLightColor;
        }

        private static Color DataPearl_UniquePearlMainColor(On.DataPearl.orig_UniquePearlMainColor orig, DataPearl.AbstractDataPearl.DataPearlType pearlType)
        {
            if (CustomDataPearlsList.TryGetValue(pearlType, out CustomPearl customPearl))
            { return customPearl.color; }
            else
            { return orig(pearlType); }
        }

        private static Color? DataPearl_UniquePearlHighLightColor(On.DataPearl.orig_UniquePearlHighLightColor orig, DataPearl.AbstractDataPearl.DataPearlType pearlType)
        {
            if (CustomDataPearlsList.TryGetValue(pearlType, out CustomPearl customPearl))
            { return customPearl.highlightColor; }
            else
            { return orig(pearlType); }
        }

        private static void DataPearl_ApplyPalette(On.DataPearl.orig_ApplyPalette orig, DataPearl self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);

            DataPearl.AbstractDataPearl.DataPearlType pearlType = (self.abstractPhysicalObject as DataPearl.AbstractDataPearl).dataPearlType;

            if (CustomDataPearlsList.TryGetValue(pearlType, out CustomPearl customPearl))
            {
                self.color = customPearl.color;
                self.highlightColor = customPearl.highlightColor;
                return;
            }
        }
        #endregion colors
    }
}
