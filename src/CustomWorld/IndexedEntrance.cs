using CustomRegions.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static CustomRegions.CustomWorld.RegionPreprocessors;
using static CustomRegions.Mod.Structs;

namespace CustomRegions.CustomWorld
{
    internal static class IndexedEntranceClass
    {
        public static void Apply()
        {
            On.WorldLoader.CreatingWorld += WorldLoader_CreatingWorld;
            On.AbstractRoom.ExitIndex += AbstractRoom_ExitIndex;
        }

        private static int AbstractRoom_ExitIndex(On.AbstractRoom.orig_ExitIndex orig, AbstractRoom self, int targetRoom)
        {
            foreach (LoadingIndex index in self.world.GetAbstractRoom(targetRoom).AbstractIndexes().Value)
            {
                if (!index.IsDefault && index.toRoom == self.name)
                {
                    return index.toRoomIndex;
                }
            }
            return orig(self, targetRoom);
        }

        private static void WorldLoader_CreatingWorld(On.WorldLoader.orig_CreatingWorld orig, WorldLoader self)
        {
            foreach (AbstractRoom absRoom in self.abstractRooms)
            {
                if (WorldLoadingIndexes.TryGetValue(absRoom.name, out List<LoadingIndex> loadingIndexes))
                {
                    foreach (LoadingIndex index in loadingIndexes)
                    {
                        bool foundOtherRoom = false;
                        for (int m = 0; m < self.abstractRooms.Count; m++)
                        {
                            if (index.toRoom == self.abstractRooms[m].name)
                            {
                                absRoom.connections[index.fromRoomIndex] = self.abstractRooms[m].index;
                                foundOtherRoom = true;
                                break;
                            }
                        }
                        if (!foundOtherRoom) continue;

                        if (absRoom.AbstractIndexes().Value.Length == 0)
                        { absRoom.AbstractIndexes().Value = new LoadingIndex[absRoom.connections.Length]; }
                        absRoom.AbstractIndexes().Value[index.fromRoomIndex] = index;
                    }
                }
            }

            orig(self);
        }

        public static void IndexedEntrance(RegionInfo info)
        {
            WorldLoadingIndexes = new();
            for (int i = 0; i < info.LinesSection("ROOMS")?.Count; i++)
            {
                string line = info.LinesSection("ROOMS")[i];
                if (!RoomLine2.TryParse(info.LinesSection("ROOMS")[i], out RoomLine2 roomLine))
                { continue; }

                for (int j = 0; j < roomLine.connections.Count; j++)
                {
                    string connection = roomLine.connections[j]; 
                    
                    if (!(connection.Contains("{#") && connection.Contains('}')))
                    { continue; }

                    int split = roomLine.connections[j].IndexOf("{#");

                    CustomRegionsMod.CustomLog(roomLine.ToString() + $"\nindex [{split}] index 2 [{roomLine.connections[j].IndexOf("}")}]");

                    if (int.TryParse(roomLine.connections[j].Substring(split + 2, roomLine.connections[j].IndexOf("}") - (split + 2)), out int index))
                    {
                        LoadingIndex li = new()
                        {
                            toRoom = roomLine.connections[j].Substring(0, split),
                            toRoomIndex = index,
                            fromRoomIndex = j,
                        };
                        CustomRegionsMod.CustomLog(li.ToString());

                        roomLine.connections[j] = "DISCONNECTED";

                        info.Lines[i + info.sectionBounds["ROOMS"][0]] = roomLine.ToString();

                        if (WorldLoadingIndexes.ContainsKey(roomLine.room))
                        {
                            WorldLoadingIndexes[roomLine.room].Add(li);
                        }
                        else
                        {
                            WorldLoadingIndexes.Add(roomLine.room, new() { li });
                        }
                    }
                }
            }
        }

        private static ConditionalWeakTable<AbstractRoom, StrongBox<LoadingIndex[]>> _AbstractIndexes = new();

        public static StrongBox<LoadingIndex[]> AbstractIndexes(this AbstractRoom p) => _AbstractIndexes.GetValue(p, _ => new(){Value = new LoadingIndex[0]});
        
    public static Dictionary<string, List<LoadingIndex>> WorldLoadingIndexes = new();

        public struct LoadingIndex
        {
            public bool IsDefault => toRoom == default;
            public string toRoom;
            public int toRoomIndex;
            public int fromRoomIndex;

            public override string ToString()
            {
                return $"IsDefault: [{IsDefault}], toRoom: [{toRoom}], toRoomIndex: [{toRoomIndex}], fromRoomIndex: [{fromRoomIndex}]";
            }
        }
    }
}
