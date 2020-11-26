using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CustomRegions.Mod;
using DevInterface;
using RWCustom;
using UnityEngine;

namespace CustomRegions.DevInterface
{
    static class MapPageHook
    {
        public static void ApplyHooks()
        {
            On.DevInterface.MapPage.LoadMapConfig += MapPage_LoadMapConfig;
            On.DevInterface.MapPage.SaveMapConfig += MapPage_SaveMapConfig;
        }

        private static void MapPage_SaveMapConfig(On.DevInterface.MapPage.orig_SaveMapConfig orig, MapPage self)
        {
			string customFilePath = string.Empty;
			foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
			{
				customFilePath = Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + 
					CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar +
					"World" + Path.DirectorySeparatorChar + "Regions" + Path.DirectorySeparatorChar + self.owner.game.world.name + Path.DirectorySeparatorChar + "Properties.txt";

				if (File.Exists(customFilePath))
				{
					CustomWorldMod.Log($"Saving custom Map Config to Properties.txt from [{keyValues.Value}]");
					SaveCustomMapConfig(self, customFilePath);
						return;
				}
			}

			CustomWorldMod.Log($"No Custom Properties.txt file found for [{self.owner.game.world.name}], using vanilla...");
			orig(self);
		}

        private static void MapPage_LoadMapConfig(On.DevInterface.MapPage.orig_LoadMapConfig orig, MapPage self)
        {
            string customFilePath = string.Empty;
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.loadedRegions)
            {
                customFilePath = Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar + 
                    "World" + Path.DirectorySeparatorChar +"Regions" + Path.DirectorySeparatorChar + 
					self.owner.game.world.name + Path.DirectorySeparatorChar + "map_" + self.owner.game.world.name + ".txt"; 

                if (File.Exists(customFilePath))
                {
                    self.filePath = customFilePath;
                    CustomWorldMod.Log($"new map filepath for [{keyValues.Value}]");
					break;
                }
            }
            orig(self);
        }
        public static void SaveCustomMapConfig(MapPage self, string customPropertiesFilePath)
        {
			Vector2 vector = new Vector2(0f, 0f);
			Vector2 vector2 = new Vector2(0f, 0f);
			int num = 0;
			for (int i = 0; i < self.subNodes.Count; i++)
			{
				if (self.subNodes[i] is RoomPanel)
				{
					num++;
					vector += (self.subNodes[i] as RoomPanel).pos;
					vector2 += (self.subNodes[i] as RoomPanel).devPos;
				}
			}
			vector /= (float)num;
			vector2 /= (float)num;
			for (int j = 0; j < self.subNodes.Count; j++)
			{
				if (self.subNodes[j] is RoomPanel)
				{
					(self.subNodes[j] as RoomPanel).pos -= vector;
					(self.subNodes[j] as RoomPanel).devPos -= vector2;
				}
				else if (self.subNodes[j] is MapRenderDefaultMaterial)
				{
					(self.subNodes[j] as MapRenderDefaultMaterial).handleA.pos -= vector;
					(self.subNodes[j] as MapRenderDefaultMaterial).handleB.pos -= vector;
				}
			}
			using (StreamWriter streamWriter = File.CreateText(self.filePath))
			{
				for (int k = 0; k < self.subNodes.Count; k++)
				{
					if (self.subNodes[k] is RoomPanel)
					{
						streamWriter.WriteLine(string.Concat(new object[]
						{
							(self.subNodes[k] as RoomPanel).roomRep.room.name,
							": ",
							(self.subNodes[k] as RoomPanel).pos.x,
							",",
							(self.subNodes[k] as RoomPanel).pos.y,
							",",
							(self.subNodes[k] as RoomPanel).devPos.x,
							",",
							(self.subNodes[k] as RoomPanel).devPos.y,
							",",
							(self.subNodes[k] as RoomPanel).layer,
							",",
							(self.subNodes[k] as RoomPanel).roomRep.room.subRegion
						}));
					}
				}
				for (int l = 0; l < self.subNodes.Count; l++)
				{
					if (self.subNodes[l] is MapRenderDefaultMaterial)
					{
						streamWriter.WriteLine(string.Concat(new object[]
						{
							"Def_Mat: ",
							(self.subNodes[l] as MapRenderDefaultMaterial).handleA.pos.x,
							",",
							(self.subNodes[l] as MapRenderDefaultMaterial).handleA.pos.y,
							",",
							(self.subNodes[l] as MapRenderDefaultMaterial).handleB.pos.x,
							",",
							(self.subNodes[l] as MapRenderDefaultMaterial).handleB.pos.y,
							",",
							((self.subNodes[l] as MapRenderDefaultMaterial).handleA.subNodes[0] as Panel).pos.x,
							",",
							((self.subNodes[l] as MapRenderDefaultMaterial).handleA.subNodes[0] as Panel).pos.y,
							",",
							(!(self.subNodes[l] as MapRenderDefaultMaterial).materialIsAir) ? "0" : "1"
						}));
					}
				}
				for (int m = 0; m < self.subNodes.Count; m++)
				{
					if (self.subNodes[m] is RoomPanel)
					{
						int index = (self.subNodes[m] as RoomPanel).roomRep.room.index;
						for (int n = 0; n < (self.subNodes[m] as RoomPanel).roomRep.room.connections.Length; n++)
						{
							int num2 = (self.subNodes[m] as RoomPanel).roomRep.room.connections[n];
							if (num2 > index)
							{
								RoomPanel roomPanel = null;
								for (int num3 = 0; num3 < self.subNodes.Count; num3++)
								{
									if (self.subNodes[num3] is RoomPanel && (self.subNodes[num3] as RoomPanel).roomRep.room.index == num2)
									{
										roomPanel = (self.subNodes[num3] as RoomPanel);
										break;
									}
								}
								if (roomPanel != null && n > -1 && n < (self.subNodes[m] as RoomPanel).roomRep.nodePositions.Length)
								{
									Vector2 vector3 = (self.subNodes[m] as RoomPanel).roomRep.nodePositions[n];
									int num4 = roomPanel.roomRep.room.ExitIndex(index);
									if (num4 > -1)
									{
										Vector2 vector4 = roomPanel.roomRep.nodePositions[num4];
										streamWriter.WriteLine(string.Concat(new object[]
										{
											"Connection: ",
											(self.subNodes[m] as RoomPanel).roomRep.room.name,
											",",
											roomPanel.roomRep.room.name,
											",",
											vector3.x,
											",",
											vector3.y,
											",",
											vector4.x,
											",",
											vector4.y,
											",",
											(self.subNodes[m] as RoomPanel).roomRep.exitDirections[n],
											",",
											roomPanel.roomRep.exitDirections[num4]
										}));
									}
									else
									{
										Debug.Log("failed connection: " + roomPanel.roomRep.room.name + " -> " + (self.subNodes[m] as RoomPanel).roomRep.room.name);
									}
								}
							}
						}
					}
				}
			}
			List<string> list = new List<string>();
			for (int num5 = self.owner.game.world.firstRoomIndex; num5 < self.owner.game.world.firstRoomIndex + self.owner.game.world.NumberOfRooms; num5++)
			{
				bool flag = false;
				AbstractRoom abstractRoom = self.owner.game.world.GetAbstractRoom(num5);
				int num6 = 0;
				while (num6 < abstractRoom.roomAttractions.Length && !flag)
				{
					if (abstractRoom.roomAttractions[num6] != AbstractRoom.CreatureRoomAttraction.Neutral)
					{
						flag = true;
					}
					num6++;
				}
				if (flag)
				{
					string text = "Room_Attr: " + abstractRoom.name + ": ";
					for (int num7 = 0; num7 < abstractRoom.roomAttractions.Length; num7++)
					{
						if (abstractRoom.roomAttractions[num7] != AbstractRoom.CreatureRoomAttraction.Neutral)
						{
							string text2 = text;
							text = string.Concat(new object[]
							{
								text2,
								StaticWorld.creatureTemplates[num7].type.ToString(),
								"-",
								(int)abstractRoom.roomAttractions[num7],
								","
							});
						}
					}
					list.Add(text);
				}
			}
			if (list.Count > 0)
			{
				/*
				string text3 = string.Concat(new object[]
				{
					Custom.RootFolderDirectory(),
					"World",
					Path.DirectorySeparatorChar,
					"Regions",
					Path.DirectorySeparatorChar,
					self.owner.game.world.name,
					Path.DirectorySeparatorChar,
					"Properties.txt"
				});
				*/
				//string customPropertiesFilePath

				string[] array = File.ReadAllLines(customPropertiesFilePath);
				using (StreamWriter streamWriter2 = File.CreateText(customPropertiesFilePath))
				{
					for (int num8 = 0; num8 < array.Length; num8++)
					{
						if (array[num8].Substring(0, 10) != "Room_Attr:")
						{
							streamWriter2.WriteLine(array[num8]);
						}
					}
					for (int num9 = 0; num9 < list.Count; num9++)
					{
						streamWriter2.WriteLine(list[num9]);
					}
				}
			}
		}
    }
}
