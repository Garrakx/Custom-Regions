using CustomRegions.Mod;
using MonoMod.RuntimeDetour;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CustomRegions.CustomWorld
{
    internal static class Debugging
    {
        public static void ApplyHooks()
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            On.WorldLoader.LoadAbstractRoom += WorldLoader_LoadAbstractRoom;
            On.WorldLoader.SpawnerStabilityCheck += WorldLoader_SpawnerStabilityCheck;
            On.WorldLoader.AddSpawnersFromString += WorldLoader_AddSpawnersFromString;
            On.WorldLoader.AddLineageFromString += WorldLoader_AddLineageFromString;
            try
            {
                new Hook(typeof(WorldLoader).GetMethod("FindingCreaturesThread", flags), ThreadTryCatch<WorldLoader>);
                new Hook(typeof(WorldLoader).GetMethod("CreatingAbstractRoomsThread", flags), ThreadTryCatch<WorldLoader>);
                new Hook(typeof(WorldLoader).GetMethod("UpdateThread", flags), ThreadTryCatch<WorldLoader>);
                new Hook(typeof(AImapper).GetMethod("AIMappingThread", flags), ThreadTryCatch<AImapper>);
                new Hook(typeof(RoomPreparer).GetMethod("UpdateThread", flags), ThreadTryCatch<RoomPreparer>);
            }
            catch (Exception e) { CustomRegionsMod.BepLogError("failed to hook threads\n" + e); }
            On.World.GetNode += World_GetNode;
        }

        private static void ThreadTryCatch<T>(Action<T> orig, T self)
        {
            try { orig(self); }
            catch (Exception e) { CustomRegionsMod.CustomLog(e.ToString(), true); }
        }

        private static AbstractRoomNode World_GetNode(On.World.orig_GetNode orig, World self, WorldCoordinate c)
        {
            try { return orig(self, c); }
            catch(Exception e)
            {
                CustomRegionsMod.CustomLog($"Exception when getting world node for room [{self.GetAbstractRoom(c.room)?.name}] at index [{c.abstractNode}\n{e}");
                throw; 
            }
        }

        private static void WorldLoader_AddLineageFromString(On.WorldLoader.orig_AddLineageFromString orig, WorldLoader self, string[] s)
        {
            try { orig(self, s); }
            catch (Exception e) { CustomRegionsMod.CustomLog($"Skipping broken lineage line!: [{string.Join(" : ", s)}]\n" + e.ToString(), true); }
        }

        private static void WorldLoader_AddSpawnersFromString(On.WorldLoader.orig_AddSpawnersFromString orig, WorldLoader self, string[] line)
        {
            try { orig(self, line); }
            catch (Exception e) { CustomRegionsMod.CustomLog($"Skipping broken spawner line!: [{string.Join(" : ", line)}]\n" + e.ToString(), true); }
        }

        private static void WorldLoader_SpawnerStabilityCheck(On.WorldLoader.orig_SpawnerStabilityCheck orig, WorldLoader self, World.CreatureSpawner spawner)
        {
            try
            { orig(self, spawner); }
            catch
            {
                bool inRange = spawner.den.abstractNode > -1 && spawner.den.abstractNode < self.world.GetAbstractRoom(spawner.den.room).nodes.Length;
                string roomTypeNodeError = string.Concat(new string[]
                        {
                $" room: {self.world.GetAbstractRoom(spawner.den.room).name}",
                $" type: {(inRange? self.world.GetAbstractRoom(spawner.den.room).GetNode(spawner.den).type?.ToString() : "OUT OF RANGE")}",
                $" node: {spawner.den.abstractNode}",
                $" / {self.world.GetAbstractRoom(spawner.den.room).nodes.Length}"
                        });

                if (spawner.den.room != self.world.offScreenDen.index && (spawner.den.room < self.world.firstRoomIndex || spawner.den.room >= self.world.firstRoomIndex + self.world.NumberOfRooms))
                {
                    if (spawner is World.SimpleSpawner)
                    {
                        CreatureTemplate.Type creatureType = (spawner as World.SimpleSpawner).creatureType;
                        UnityEngine.Debug.Log("ERROR SPAWNER IN ROOM NOT LOADED BY REGION'S WORLD FILE. creature: " + (creatureType?.ToString()));
                    }
                    if (spawner is World.Lineage)
                    {
                        CreatureTemplate.Type type6 = (spawner as World.Lineage).CurrentType((self.game.session as StoryGameSession).saveState);
                        UnityEngine.Debug.Log("ERROR SPAWNER LINEAGE IN ROOM NOT LOADED BY REGION'S WORLD FILE. creature: " + (type6?.ToString()));
                        return;
                    }
                }
                else if (!inRange || self.world.GetAbstractRoom(spawner.den.room).GetNode(spawner.den).type != AbstractRoomNode.Type.Den)
                {
                    CreatureTemplate.Type type4 = spawner switch
                    {
                        World.SimpleSpawner => (spawner as World.SimpleSpawner).creatureType,
                        World.Lineage => (spawner as World.Lineage).CurrentType((self.game.session as StoryGameSession).saveState),
                        _ => CreatureTemplate.Type.StandardGroundCreature
                    };

                    if (!inRange || self.world.GetAbstractRoom(spawner.den.room).GetNode(spawner.den).type != AbstractRoomNode.Type.GarbageHoles || type4 != CreatureTemplate.Type.GarbageWorm)
                    {
                        UnityEngine.Debug.Log($"ERROR, SPAWNER IN DEN THAT DOESNT EXIST! creature: {type4?.ToString()}{roomTypeNodeError}");
                        return;
                    }
                }
                else if (spawner is World.Lineage)
                {
                    CreatureTemplate.Type type5 = (spawner as World.Lineage).CurrentType((self.game.session as StoryGameSession).saveState);
                    for (int i = 0; i < (spawner as World.Lineage).progressionChances.Length; i++)
                    {
                        if ((spawner as World.Lineage).progressionChances[i] == 0f && i < (spawner as World.Lineage).progressionChances.Length - 1)
                        {
                            UnityEngine.Debug.Log($"ERROR, SPAWNER LINEAGE ENDS PREMATURELY creature: {type5?.ToString()}{roomTypeNodeError}");
                            return;
                        }
                        if ((spawner as World.Lineage).progressionChances[i] != 0f && i == (spawner as World.Lineage).progressionChances.Length - 1)
                        {
                            UnityEngine.Debug.Log($"ERROR, SPAWNER LINEAGE ENDS WITH NOT 0% CHANCE creature: {type5?.ToString()}{roomTypeNodeError}");
                            return;
                        }
                    }
                }
            }
        }

        private static void WorldLoader_LoadAbstractRoom(On.WorldLoader.orig_LoadAbstractRoom orig, World world, string roomName, AbstractRoom room, RainWorldGame.SetupValues setupValues)
        {

            try { orig(world, roomName, room, setupValues); }
            catch (Exception e)
            {
                string roomPath = WorldLoader.FindRoomFile(roomName, false, ".txt");
                string exceptionMessage = $"An error occured while trying to load {roomName}";
                if (!File.Exists(roomPath))
                {
                    exceptionMessage = $"cannot find room file {roomName}";

                    bool noAcronym = !roomName.Contains("_") || !Region.GetFullRegionOrder().Contains(roomName.Split('_')[0]);
                    bool differentAcronym = roomName.Split('_').Length > 1 && roomName.Split('_')[0] != world.name;

                    char sep = Path.DirectorySeparatorChar;

                    if (File.Exists(WorldLoader.FindRoomFile(roomName.Trim(), false, ".txt")))
                    {
                        exceptionMessage += "\nroom name has extra whitespace in the world file";
                    }

                    else if (File.Exists(AssetManager.ResolveFilePath($"World{sep}{roomName.Split('_')[0]}_Rooms{sep}{roomName}.txt")))
                    {
                        exceptionMessage += $"\nroom file found in [{roomName.Split('_')[0]}_Rooms] folder, should be in [{roomName.Split('_')[0]}-Rooms] instead!";
                    }

                    else if ((differentAcronym || noAcronym))
                    {
                        if (File.Exists(AssetManager.ResolveFilePath($"World{sep}{world.name}-Rooms{sep}{roomName}.txt")))
                        {
                            exceptionMessage += $"\nroom can't be located in the [{world.name}-Rooms] folder unless the room name is prepended by [{world.name}_]";
                            exceptionMessage += $"\nrename the room to [{world.name + "_" + (noAcronym ? roomName : roomName.Split('_')[1])}] or something similar in order to load";
                        }
                        else if (File.Exists(AssetManager.ResolveFilePath($"World{sep}{world.name}_Rooms{sep}{roomName}.txt")))
                        {
                            exceptionMessage += $"\nroom file found in [{roomName.Split('_')[0]}_Rooms] folder, should be in [{roomName.Split('_')[0]}-Rooms] instead!";
                            exceptionMessage += $"\nroom can't be located in the [{world.name}-Rooms] folder unless the room name is prepended by [{world.name}_]";
                            exceptionMessage += $"\nrename the room to [{world.name + "_" + (noAcronym ? roomName : roomName.Split('_')[1])}] or something similar in order to load";
                        }
                    }
                }

                else
                {
                    string[] lines = File.ReadAllLines(roomPath);

                    if (lines[0].StartsWith("[[[["))
                    {
                        exceptionMessage = $"room file is LevelEditorProject file instead of Level file {roomName}" +
                            $"\nthe correct output files for a render will appear in Level Editor\\levels" +
                            $"\nit appears this room file is from Level Editor\\LevelEditorProjects";
                    }
                }
                CustomRegionsMod.CustomLog(exceptionMessage + "\n" + e, true);
            }
        }

    }
}
