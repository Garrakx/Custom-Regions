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

namespace CustomRegions.CustomPearls
{
    internal static class Data

        
    {
        public static void Refresh()
        {
            Unregister();
            FindCustomPearlData();

        }


        public static void FindCustomPearlData()
        {
            try
            {
                CustomDataPearlsList = new Dictionary<DataPearl.AbstractDataPearl.DataPearlType, CustomPearl2>();
                foreach (string str in AssetManager.ListDirectory("CustomPearls"))
                {
                    string fileName = Path.GetFileName(str);
                    string pearlName = Path.GetFileNameWithoutExtension(str);
                    CustomRegionsMod.CustomLog("Pearl text name is " + fileName);

                    if (ExtEnumBase.TryParse(typeof(DataPearl.AbstractDataPearl.DataPearlType), Path.GetFileNameWithoutExtension(str), false, out _))
                    { continue; }

                    DataPearl.AbstractDataPearl.DataPearlType type = RegisterPearlType(pearlName);


                    string[] array = Regex.Split(File.ReadAllText(AssetManager.ResolveFilePath("CustomPearls" + Path.DirectorySeparatorChar + fileName)), " : ");
                    UnityEngine.Color color = RWCustom.Custom.hexToColor(array[0]);
                    UnityEngine.Color colorHighlight = RWCustom.Custom.hexToColor(array[1]);
                    string filePath = array[2];

                    CustomPearl2 pearl = new CustomPearl2(type, color, colorHighlight, filePath, RegisterConversations(pearlName));
                    CustomDataPearlsList.Add(type, pearl);
                }
            }
            catch (Exception e) { throw e; }
        }

        public static void Unregister()
        {
            try
            {
                foreach (KeyValuePair<DataPearl.AbstractDataPearl.DataPearlType, CustomPearl2> pearl in CustomDataPearlsList)
                {
                    if (pearl.Value.type != null)
                    {
                        pearl.Value.type.Unregister();
                    }

                    if (pearl.Value.conversationID != null)
                    {
                        pearl.Value.conversationID.Unregister();
                    }
                }

                CustomDataPearlsList = new Dictionary<DataPearl.AbstractDataPearl.DataPearlType, CustomPearl2>();
            }
            catch (Exception e) { throw e; }
        }

        public static DataPearl.AbstractDataPearl.DataPearlType RegisterPearlType(string name)
        {
            return new DataPearl.AbstractDataPearl.DataPearlType(name, true);
        }

        public static Conversation.ID RegisterConversations(string name)
        {
            return new Conversation.ID(name, true);
        }


        public static Dictionary<DataPearl.AbstractDataPearl.DataPearlType, CustomPearl2> CustomDataPearlsList;


    }
}
