using CustomRegions.Mod;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CustomRegions.Arena
{
    public static class ArenaCreatureSpawnerHook
    {
        public static void ApplyHooks()
        {
            On.ArenaCreatureSpawner.SpawnArenaCreatures += ArenaCreatureSpawner_SpawnArenaCreatures;
        }

        public static void RemoveHooks()
        {
            On.ArenaCreatureSpawner.SpawnArenaCreatures -= ArenaCreatureSpawner_SpawnArenaCreatures;
        }

        public static Dictionary<string, int> tempDictionary = null;
        public static Dictionary<string, int> tempDictionary2 = null;

        private static void ArenaCreatureSpawner_SpawnArenaCreatures(On.ArenaCreatureSpawner.orig_SpawnArenaCreatures orig, 
            RainWorldGame game, ArenaSetup.GameTypeSetup.WildLifeSetting wildLifeSetting, ref List<AbstractCreature> availableCreatures, 
            ref MultiplayerUnlocks unlocks)
        {
            foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
            {
                string settingsPath = CRExtras.BuildPath(keyValues.Value, CRExtras.CustomFolder.Levels) + game.world.GetAbstractRoom(0).name + "_Arena.txt";
                if (File.Exists(settingsPath))
                {
                    CustomWorldMod.Log($"Custom Regions: Loading settings file in SpawnArenaCreatures. Path [{settingsPath}]");
                    string[] settingsFile = File.ReadAllLines(settingsPath);
                    SpawnArenaCreaturesVanilla(game, wildLifeSetting, ref availableCreatures, ref unlocks, settingsFile);
                    break;
                }
            }
            orig(game, wildLifeSetting, ref availableCreatures, ref unlocks);
        }

        public static void SpawnArenaCreaturesVanilla(RainWorldGame game, ArenaSetup.GameTypeSetup.WildLifeSetting wildLifeSetting, 
            ref List<AbstractCreature> availableCreatures, ref MultiplayerUnlocks unlocks, string[] array)
        {
            float num = 1f;
            switch (wildLifeSetting)
            {
                case ArenaSetup.GameTypeSetup.WildLifeSetting.Off:
                    return;
                case ArenaSetup.GameTypeSetup.WildLifeSetting.Low:
                    num = 0.5f;
                    break;
                case ArenaSetup.GameTypeSetup.WildLifeSetting.Medium:
                    num = 1f;
                    break;
                case ArenaSetup.GameTypeSetup.WildLifeSetting.High:
                    num = 1.5f;
                    break;
            }
            AbstractRoom abstractRoom = game.world.GetAbstractRoom(0);
            List<ArenaCreatureSpawner.Spawner> list = new List<ArenaCreatureSpawner.Spawner>();
            List<ArenaCreatureSpawner.CritterSpawnData> list2 = new List<ArenaCreatureSpawner.CritterSpawnData>();
            List<ArenaCreatureSpawner.CreatureGroup> list3 = new List<ArenaCreatureSpawner.CreatureGroup>();
            List<ArenaCreatureSpawner.SpawnSymbol> list4 = new List<ArenaCreatureSpawner.SpawnSymbol>();
            List<ArenaCreatureSpawner.DenGroup> list5 = new List<ArenaCreatureSpawner.DenGroup>();
            float num2 = -1f;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Length > 2 && array[i].Substring(0, 2) != "//")
                {
                    string[] array2 = Regex.Split(array[i], " - ");
                    int num3 = 1;
                    ArenaCreatureSpawner.Spawner spawner = null;
                    string text = array2[0];
                    int num4;
                    if (text != null)
                    {
                        if (ArenaCreatureSpawnerHook.tempDictionary == null)
                        {
                            Dictionary<string, int> dictionary = new Dictionary<string, int>(5);
                            dictionary.Add("Creature", 0);
                            dictionary.Add("CreatureGroup", 1);
                            dictionary.Add("DenGroup", 2);
                            dictionary.Add("PlayersGlow", 3);
                            dictionary.Add("GoalAmount", 4);
                            ArenaCreatureSpawnerHook.tempDictionary = dictionary;
                        }
                        if (ArenaCreatureSpawnerHook.tempDictionary.TryGetValue(text, out num4))
                        {
                            switch (num4)
                            {
                                case 0:
                                    {
                                        CreatureTemplate.Type? type = WorldLoader.CreatureTypeFromString(array2[1]);
                                        if (type != null)
                                        {
                                            spawner = new ArenaCreatureSpawner.CritterSpawnData(type.Value);
                                        }
                                        else
                                        {
                                            CustomWorldMod.Log("not rec. " + array2[1]);
                                        }
                                        break;
                                    }
                                case 1:
                                    spawner = new ArenaCreatureSpawner.CreatureGroup(array2[1]);
                                    break;
                                case 2:
                                    {
                                        list5.Add(new ArenaCreatureSpawner.DenGroup(array2[1]));
                                        string[] array3 = array2[2].Split(new char[]
                                        {
                                ','
                                        });
                                        for (int j = 0; j < array3.Length; j++)
                                        {
                                            list5[list5.Count - 1].dens.Add(int.Parse(array3[j]));
                                        }
                                        break;
                                    }
                                case 3:
                                    game.GetArenaGameSession.playersGlowing = true;
                                    break;
                                case 4:
                                    num2 = float.Parse(array2[1]);
                                    break;
                            }
                        }
                    }
                    if (spawner != null)
                    {
                        for (int k = 2; k < array2.Length; k++)
                        {
                            string[] array4 = Regex.Split(array2[k], ":");
                            text = array4[0];
                            if (text != null)
                            {
                                if (ArenaCreatureSpawnerHook.tempDictionary2 == null)
                                {
                                    Dictionary<string, int> dictionary = new Dictionary<string, int>(8);
                                    dictionary.Add("chance", 0);
                                    dictionary.Add("RARE", 1);
                                    dictionary.Add("group", 2);
                                    dictionary.Add("dens", 3);
                                    dictionary.Add("spawnDataString", 4);
                                    dictionary.Add("amount", 5);
                                    dictionary.Add("symbol", 6);
                                    dictionary.Add("invSymbol", 7);
                                    ArenaCreatureSpawnerHook.tempDictionary2 = dictionary;
                                }
                                if (ArenaCreatureSpawnerHook.tempDictionary2.TryGetValue(text, out num4))
                                {
                                    switch (num4)
                                    {
                                        case 0:
                                            spawner.spawnChance = float.Parse(array4[1]);
                                            break;
                                        case 1:
                                            if (spawner is ArenaCreatureSpawner.CritterSpawnData)
                                            {
                                                (spawner as ArenaCreatureSpawner.CritterSpawnData).rare = true;
                                            }
                                            break;
                                        case 2:
                                            spawner.groupString = array4[1];
                                            break;
                                        case 3:
                                            if (spawner is ArenaCreatureSpawner.CritterSpawnData)
                                            {
                                                bool flag = true;
                                                for (int l = 0; l < list5.Count; l++)
                                                {
                                                    if (list5[l].name == array4[1])
                                                    {
                                                        (spawner as ArenaCreatureSpawner.CritterSpawnData).dens = list5[l].dens;
                                                        flag = false;
                                                        break;
                                                    }
                                                }
                                                if (flag)
                                                {
                                                    string[] array5 = array4[1].Split(new char[]
                                                    {
                                                ','
                                                    });
                                                    for (int m = 0; m < array5.Length; m++)
                                                    {
                                                        (spawner as ArenaCreatureSpawner.CritterSpawnData).dens.Add(int.Parse(array5[m]));
                                                    }
                                                }
                                            }
                                            break;
                                        case 4:
                                            if (spawner is ArenaCreatureSpawner.CritterSpawnData)
                                            {
                                                (spawner as ArenaCreatureSpawner.CritterSpawnData).spawnDataString = array4[1];
                                            }
                                            break;
                                        case 5:
                                            if (spawner is ArenaCreatureSpawner.CritterSpawnData)
                                            {
                                                if (array4[1] == "players")
                                                {
                                                    num3 = game.GetArenaGameSession.arenaSitting.players.Count;
                                                }
                                                else
                                                {
                                                    string[] array5 = array4[1].Split(new char[]
                                                    {
                                                '-'
                                                    });
                                                    if (array5.Length == 1)
                                                    {
                                                        num3 = int.Parse(array5[0]);
                                                    }
                                                    else
                                                    {
                                                        num3 = UnityEngine.Random.Range(int.Parse(array5[0]), int.Parse(array5[1]) + 1);
                                                    }
                                                }
                                            }
                                            break;
                                        case 6:
                                            ArenaCreatureSpawner.AddToSymbol(array4[1], false, ref list4);
                                            spawner.symbolString = array4[1];
                                            break;
                                        case 7:
                                            ArenaCreatureSpawner.AddToSymbol(array4[1], true, ref list4);
                                            spawner.invSymbolString = array4[1];
                                            break;
                                    }
                                }
                            }
                        }
                        if (num3 > 0)
                        {
                            if (spawner is ArenaCreatureSpawner.CreatureGroup)
                            {
                                list3.Add(spawner as ArenaCreatureSpawner.CreatureGroup);
                            }
                            else if (spawner is ArenaCreatureSpawner.CritterSpawnData)
                            {
                                list2.Add(spawner as ArenaCreatureSpawner.CritterSpawnData);
                            }
                            list.Add(spawner);
                            spawner.spawn = true;
                            for (int n = 1; n < num3; n++)
                            {
                                ArenaCreatureSpawner.CritterSpawnData critterSpawnData = new ArenaCreatureSpawner.CritterSpawnData((spawner as ArenaCreatureSpawner.CritterSpawnData).type);
                                critterSpawnData.dens = (spawner as ArenaCreatureSpawner.CritterSpawnData).dens;
                                critterSpawnData.groupString = (spawner as ArenaCreatureSpawner.CritterSpawnData).groupString;
                                critterSpawnData.symbolString = (spawner as ArenaCreatureSpawner.CritterSpawnData).symbolString;
                                critterSpawnData.invSymbolString = (spawner as ArenaCreatureSpawner.CritterSpawnData).invSymbolString;
                                critterSpawnData.spawnChance = (spawner as ArenaCreatureSpawner.CritterSpawnData).spawnChance;
                                critterSpawnData.spawnDataString = (spawner as ArenaCreatureSpawner.CritterSpawnData).spawnDataString;
                                critterSpawnData.rare = (spawner as ArenaCreatureSpawner.CritterSpawnData).rare;
                                list2.Add(critterSpawnData);
                                list.Add(critterSpawnData);
                                critterSpawnData.spawn = true;
                            }
                        }
                    }
                }
            }
            for (int num5 = 0; num5 < list.Count; num5++)
            {
                if (list[num5].symbolString != null)
                {
                    for (int num6 = 0; num6 < list4.Count; num6++)
                    {
                        if (list[num5].symbolString.Substring(0, 1) == list4[num6].name)
                        {
                            list[num5].symbol = list4[num6];
                            list4[num6].connectedSpawners.Add(list[num5]);
                            break;
                        }
                    }
                }
                if (list[num5].invSymbolString != null)
                {
                    for (int num7 = 0; num7 < list4.Count; num7++)
                    {
                        if (list[num5].invSymbolString.Substring(0, 1) == list4[num7].name)
                        {
                            list[num5].invSymbol = list4[num7];
                            list4[num7].connectedSpawners.Add(list[num5]);
                            break;
                        }
                    }
                }
                if (list[num5].groupString != null)
                {
                    for (int num8 = 0; num8 < list3.Count; num8++)
                    {
                        if (list[num5].groupString == list3[num8].name)
                        {
                            list[num5].group = list3[num8];
                            list3[num8].connectedSpawners.Add(list[num5]);
                            break;
                        }
                    }
                }
            }
            Dictionary<CreatureTemplate.Type, bool> dictionary2 = new Dictionary<CreatureTemplate.Type, bool>();
            float num9 = 0f;
            List<CreatureTemplate.Type> list6 = new List<CreatureTemplate.Type>();
            List<CreatureTemplate.Type> list7 = new List<CreatureTemplate.Type>();
            for (int num10 = 0; num10 < list2.Count; num10++)
            {
                float num11 = Mathf.Clamp01(list2[num10].spawnChance);
                if (list2[num10].group != null)
                {
                    num11 *= Mathf.Clamp01(list2[num10].group.spawnChance);
                }
                if (list2[num10].symbol != null)
                {
                    num11 *= 1f / (float)list2[num10].symbol.possibleOutcomes.Count;
                }
                if (list2[num10].invSymbol != null)
                {
                    num11 *= 1f - 1f / (float)list2[num10].invSymbol.possibleOutcomes.Count;
                }
                if (unlocks.IsCreatureUnlockedForLevelSpawn(list2[num10].type))
                {
                    if (!list6.Contains(list2[num10].type))
                    {
                        list6.Add(list2[num10].type);
                    }
                }
                else
                {
                    if (!list7.Contains(list2[num10].type))
                    {
                        list7.Add(list2[num10].type);
                    }
                    CreatureTemplate.Type? type2 = unlocks.RecursiveFallBackCritter(new CreatureTemplate.Type?(list2[num10].type));
                    if (type2 != null)
                    {
                        CustomWorldMod.Log(list2[num10].type + " fall back to " + type2.Value);
                        list2[num10].type = type2.Value;
                        list2[num10].spawnChance = Mathf.Clamp01(list2[num10].spawnChance) * 0.01f;
                        num11 *= 0.5f;
                    }
                    else
                    {
                        list2[num10].Disable();
                        num11 *= 0f;
                    }
                }
                num9 += num11;
            }
            float num12 = (float)list6.Count / (float)(list6.Count + list7.Count);
            CustomWorldMod.Log("percentCritTypesAllowed: " + num12);
            float num13 = Mathf.InverseLerp(0.7f, 0.3f, num12);
            if (num2 > 0f)
            {
                num2 *= Mathf.Lerp(Mathf.InverseLerp(0.15f, 0.75f, num12), 1f, 0.5f) * num;
            }
            CustomWorldMod.Log("diversify: " + num13);
            for (int num14 = 0; num14 < list3.Count; num14++)
            {
                if (UnityEngine.Random.value > list3[num14].spawnChance || !list3[num14].AnyConnectedSpawnersActive())
                {
                    list3[num14].Disable();
                }
            }
            for (int num15 = 0; num15 < list2.Count; num15++)
            {
                if (list2[num15].rare && UnityEngine.Random.value > Mathf.Pow(list2[num15].spawnChance, Custom.LerpMap(num12, 0.35f, 0.85f, 0.5f, 0.05f)))
                {
                    list2[num15].Disable();
                }
            }
            for (int num16 = 0; num16 < list4.Count; num16++)
            {
                list4[num16].decidedOutcome = list4[num16].possibleOutcomes[UnityEngine.Random.Range(0, list4[num16].possibleOutcomes.Count)];
                for (int num17 = 0; num17 < 10; num17++)
                {
                    if (list4[num16].AnyConnectedSpawnersActiveUnderCurrentRoll())
                    {
                        break;
                    }
                    list4[num16].decidedOutcome = list4[num16].possibleOutcomes[UnityEngine.Random.Range(0, list4[num16].possibleOutcomes.Count)];
                }
            }
            for (int num18 = 0; num18 < list.Count; num18++)
            {
                if (list[num18].group != null && !list[num18].group.spawn)
                {
                    list[num18].Disable();
                }
                else if (list[num18].symbol != null && list[num18].symbol.decidedOutcome != list[num18].symbolString.Substring(1, 1))
                {
                    list[num18].Disable();
                }
                else if (list[num18].invSymbol != null && list[num18].invSymbol.decidedOutcome == list[num18].invSymbolString.Substring(1, 1))
                {
                    list[num18].Disable();
                }
            }
            CustomWorldMod.Log(string.Concat(new object[]
            {
            "weight total pre rand: ",
            num9,
            " ga:",
            num2
            }));
            if (num2 > -1f)
            {
                num9 = Mathf.Lerp(num9, num2, 0.5f);
            }
            num9 *= num * Mathf.Lerp(0.8f, 1.2f, UnityEngine.Random.value);
            if (num2 > -1f)
            {
                num9 += Mathf.Lerp(-1.2f, 1.2f, Mathf.Pow(UnityEngine.Random.value, Custom.LerpMap(num9, num2 - 2f, num2 + 2f, 0.25f, 2f)));
            }
            else
            {
                num9 += Mathf.Lerp(-1.2f, 1.2f, Mathf.Pow(UnityEngine.Random.value, Custom.LerpMap(num9, 2f, 10f, 0.25f, 3f)));
            }
            CustomWorldMod.Log("weight total: " + num9);
            int num19 = Custom.IntClamp(Mathf.RoundToInt(num9), (int)wildLifeSetting, 25);
            CustomWorldMod.Log("creaturesToSpawn: " + num19);
            List<ArenaCreatureSpawner.CritterSpawnData> list8 = new List<ArenaCreatureSpawner.CritterSpawnData>();
            for (int num20 = 0; num20 < num19; num20++)
            {
                ArenaCreatureSpawner.CritterSpawnData critterSpawnData2 = ArenaCreatureSpawner.WeightedRandom(list2);
                if (critterSpawnData2 != null)
                {
                    critterSpawnData2.Disable();
                    list8.Add(critterSpawnData2);
                    if (num13 > 0f)
                    {
                        ArenaCreatureSpawner.Diversify(ref list2, critterSpawnData2.type, num13);
                    }
                }
            }
            CustomWorldMod.Log("-- ACTUALLY SPAWNING");
            for (int num21 = 0; num21 < list8.Count; num21++)
            {
                CustomWorldMod.Log(string.Concat(new object[]
                {
                num21,
                " ---- ",
                list8[num21].type,
                "(",
                list8[num21].ID,
                ")"
                }));
            }
            int[] array6 = new int[abstractRoom.nodes.Length];
            for (int num22 = 0; num22 < list8.Count; num22++)
            {
                ArenaCreatureSpawner.CritterSpawnData critterSpawnData3 = list8[num22];
                if (critterSpawnData3.dens.Count < 1 || critterSpawnData3.dens[0] == -1)
                {
                    AbstractCreature abstractCreature = ArenaCreatureSpawner.CreateAbstractCreature(game.world, critterSpawnData3.type, new WorldCoordinate(game.world.offScreenDen.index, -1, -1, 0), ref availableCreatures);
                    if (abstractCreature != null)
                    {
                        game.world.offScreenDen.AddEntity(abstractCreature);
                    }
                }
                else
                {
                    int num23 = int.MaxValue;
                    for (int num24 = 0; num24 < critterSpawnData3.dens.Count; num24++)
                    {
                        num23 = Math.Min(num23, array6[critterSpawnData3.dens[num24]]);
                    }
                    List<int> list9 = new List<int>();
                    for (int num25 = 0; num25 < critterSpawnData3.dens.Count; num25++)
                    {
                        if (array6[critterSpawnData3.dens[num25]] <= num23)
                        {
                            list9.Add(critterSpawnData3.dens[num25]);
                        }
                    }
                    int num26 = list9[UnityEngine.Random.Range(0, list9.Count)];
                    array6[num26]++;
                    if (StaticWorld.GetCreatureTemplate(critterSpawnData3.type).quantified)
                    {
                        abstractRoom.AddQuantifiedCreature(num26, critterSpawnData3.type, UnityEngine.Random.Range(7, 11));
                    }
                    else
                    {
                        AbstractCreature abstractCreature2 = ArenaCreatureSpawner.CreateAbstractCreature(game.world, critterSpawnData3.type, new WorldCoordinate(abstractRoom.index, -1, -1, num26), ref availableCreatures);
                        if (abstractCreature2 != null)
                        {
                            abstractRoom.MoveEntityToDen(abstractCreature2);
                            CreatureTemplate.Type type3 = abstractCreature2.creatureTemplate.type;
                            if (type3 == CreatureTemplate.Type.BigNeedleWorm)
                            {
                                for (int num27 = UnityEngine.Random.Range(0, 4); num27 >= 0; num27--)
                                {
                                    AbstractCreature ent = new AbstractCreature(game.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.SmallNeedleWorm), null, new WorldCoordinate(abstractRoom.index, -1, -1, num26), game.GetNewID());
                                    abstractRoom.MoveEntityToDen(ent);
                                }
                            }
                        }
                    }
                }

            }
        }
    }
}
