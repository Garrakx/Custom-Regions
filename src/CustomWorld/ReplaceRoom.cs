using CustomRegions.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static CustomRegions.Mod.Structs;
using static CustomRegions.CustomWorld.RegionPreprocessors;
using MonoMod.Cil;

namespace CustomRegions.CustomWorld
{
    internal static class ReplaceRoomPreprocessor
    {
        public static void Apply()
        {
            On.WorldLoader.LoadAbstractRoom += WorldLoader_LoadAbstractRoom;
            On.WorldLoader.Update += WorldLoader_Update;
            On.RoomCamera.MoveCamera2 += RoomCamera_MoveCamera2;
            try
            {
                IL.RoomCamera.MoveCamera_Room_int += RoomNameILHook;
                IL.RoomCamera.PreLoadTexture += RoomNameILHook;
                IL.RoomPreparer.ctor += RoomNameILHook;
                IL.Room.ctor += RoomNameILHook;
            }
            catch (Exception e) { CustomRegionsMod.BepLogError($"ReplaceRoom IL hook error!\n" + e); }
        }

        private static void RoomNameILHook(ILContext il)
        {
            int count = 0;
            var cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.Before, x => x.MatchLdfld<AbstractRoom>("name")))
            {
                cursor.MoveAfterLabels();
                cursor.EmitDelegate((AbstractRoom room) => room.AltFileName().Value);
                cursor.Remove();
                count++;
            }
            if(count == 0)
            {
                CustomRegionsMod.BepLogError($"failed to IL hook roomname for ReplaceRoom for [{il.Method.Name}]");
            }
        }

        private static void RoomCamera_MoveCamera2(On.RoomCamera.orig_MoveCamera2 orig, RoomCamera self, string roomName, int camPos)
        {
            if (self.room != null)
            {
                if (self.room.abstractRoom.name == roomName)
                { roomName = self.room.abstractRoom.AltFileName().Value; }

                else if (self.room.world != null)
                {
                    foreach (AbstractRoom absRoom in self.room.world.abstractRooms)
                    {
                        if (absRoom.name == roomName)
                        { roomName = absRoom.AltFileName().Value; }
                    }
                }
            }
            orig(self, roomName, camPos);
        }

        private static void WorldLoader_Update(On.WorldLoader.orig_Update orig, WorldLoader self)
        {
            orig(self);
            if (self.Finished)
            { DiscardWorldLoadingValue(self.worldName); }
        }

        private static void WorldLoader_LoadAbstractRoom(On.WorldLoader.orig_LoadAbstractRoom orig, World world, string roomName, AbstractRoom room, RainWorldGame.SetupValues setupValues)
        {
            if (WorldLoaderReplaceRooms(world.name).ContainsKey(roomName))
            {
                room.AltFileName().Value = WorldLoaderReplaceRooms(world.name)[roomName];
                roomName = room.AltFileName().Value;
            }

            orig(world, roomName, room, setupValues);
        }

        private static readonly ConditionalWeakTable<AbstractRoom, StrongBox<string>> _AltFileName = new();
        public static StrongBox<string> AltFileName(this AbstractRoom p) => _AltFileName.GetValue(p, p => new(p.name));


        private static readonly Dictionary<string, Dictionary<string, string>> _LoaderReplaceRooms = new();
        public static Dictionary<string, string> WorldLoaderReplaceRooms(string name)
        {
            if (!_LoaderReplaceRooms.ContainsKey(name)) _LoaderReplaceRooms[name] = new();

            return _LoaderReplaceRooms[name];
        }

        public static void DiscardWorldLoadingValue(string name)
        {
            if (!_LoaderReplaceRooms.ContainsKey(name))
            { _LoaderReplaceRooms.Remove(name); }
        }



        public static void ReplaceRoom(RegionInfo info)
        {
            WorldLoaderReplaceRooms(info.RegionID).Clear();

            string CL = "CONDITIONAL LINKS";
            string RM = "ROOMS";
            string CR = "CREATURES";
            string BM = "BAT MIGRATION BLOCKAGES";

            for (int i = 0; i < info.LinesSection(CL)?.Count; i++)
            {
                if (!string.IsNullOrEmpty(info.LinesSection(CL)[i]))
                {
                    string[] array = Regex.Split(info.LinesSection(CL)[i], " : ");
                    if (array.Length >= 4 && array[1] == "REPLACEROOM" && StringMatchesSlugcat(array[0], info.playerCharacter))
                    {
                        CustomRegionsMod.CustomLog($"adding line [{string.Join(" : ", array)}]");
                        WorldLoaderReplaceRooms(info.RegionID).Add(array[2], array[3]);
                        //info.LinesSection(CL)[i] = "//";
                        info.Lines[i + info.sectionBounds[CL][0]] = "//";
                    }
                }
            }
            info.Lines.RemoveAll(str => str == "//");
            return;
            for (int i = 0; i < info.LinesSection(RM)?.Count; i++)
            {
                if (RoomLine2.TryParse(info.LinesSection(RM)[i], out RoomLine2 roomLine))
                {
                    bool modify = false;
                    if (WorldLoaderReplaceRooms(info.RegionID).ContainsKey(roomLine.room))
                    {
                        roomLine.room = WorldLoaderReplaceRooms(info.RegionID)[roomLine.room];
                        modify = true;
                    }

                    for (int j = 0; j < roomLine.connections.Count; j++)
                    {
                        if (WorldLoaderReplaceRooms(info.RegionID).ContainsKey(roomLine.connections[j]))
                        {
                            roomLine.connections[j] = WorldLoaderReplaceRooms(info.RegionID)[roomLine.connections[j]];
                            modify = true;
                        }

                    }

                    if (modify)
                    { info.Lines[i + info.sectionBounds[RM][0]] = roomLine.ToString(); }
                }
            }

            for (int i = 0; i < info.LinesSection(CR)?.Count; i++)
            {
                if (CreatureLine2.TryParse(info.LinesSection(CR)[i], out CreatureLine2 creatureLine))
                {
                    bool modify = false;
                    if (WorldLoaderReplaceRooms(info.RegionID).ContainsKey(creatureLine.room))
                    {
                        creatureLine.room = WorldLoaderReplaceRooms(info.RegionID)[creatureLine.room];
                        modify = true;
                    }

                    if (modify)
                    { info.Lines[i + info.sectionBounds[CR][0]] = creatureLine.ToString(); }
                }
            }

            for (int i = 0; i < info.LinesSection(BM)?.Count; i++)
            {
                if (WorldLoaderReplaceRooms(info.RegionID).ContainsKey(info.LinesSection(BM)[i]))
                { info.Lines[i + info.sectionBounds[BM][0]] = WorldLoaderReplaceRooms(info.RegionID)[info.LinesSection(BM)[i]]; }
            }

            info.Lines.RemoveAll(str => str == "//");
        }
        public static bool StringMatchesSlugcat(string text, SlugcatStats.Name slug)
        {
            bool include = false;
            bool inverted = false;

            if (text.StartsWith("X-"))
            {
                text = text.Substring(2);
                inverted = true;
            }

            if (slug == null)
            {
                return inverted;
            }

            foreach (string text2 in text.Split(','))
            {
                if (text2.Trim() == slug.ToString())
                {
                    include = true;
                    break;
                }
            }

            return inverted != include;
        }
    }
}
