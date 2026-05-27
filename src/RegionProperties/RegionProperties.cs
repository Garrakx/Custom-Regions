using BepInEx;
using CustomRegions.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;
using static CustomRegions.CustomWorld.RegionPreprocessors;

namespace CustomRegions.RegionProperties
{
    internal static class RegionProperties
    {
        private static ConditionalWeakTable<Region, CRSProperties> _crsProperties = new();
        public static CRSProperties GetCRSProperties(this Region r) => _crsProperties.GetValue(r, _ => new(r));

        public class CRSProperties
        {
            public CRSProperties(Region region)
            {
                CustomRegionsMod.CustomLog("CRSproperties");
                var properties = region.regionParams.GetRawProperties().CustomProperties;
                foreach (string key in properties.Keys)
                {
                    ParseCustomProperty(key, properties[key]);
                }
            }

            public void ParseCustomProperty(string key, string value)
            {
                try
                {
                    switch (key)
                    {
                        case nameof(musicAfterCycle): musicAfterCycle = value.ToLower() == "true"; break;
                        case nameof(hideTimer): hideTimer = value.ToLower() == "true"; break;
                        case nameof(wormGrassLight): wormGrassLight = value.ToLower() == "true"; break;
                        case nameof(postCycleMusic): postCycleMusic = value.ToLower() == "true"; break;
                        case nameof(hideVoidSpawn): hideVoidSpawn = value.ToLower() == "true"; break;
                        case nameof(voidSpawnTarget): voidSpawnTarget = value; break;
                        case nameof(sundownMusic): sundownMusic = value; break;
                        case nameof(cycleLength): cycleLength = float.Parse(value); break;
                        case nameof(rivStormyCycleLength): rivStormyCycleLength = float.Parse(value); break;
                        case nameof(rivStormyPreCycleChance): rivStormyPreCycleChance = float.Parse(value); break;
                        case nameof(forcePreCycleChance): rivStormyPreCycleChance = float.Parse(value); break;
                        case nameof(throwObjectsThreshold): throwObjectsThreshold = float.Parse(value); break;
                        case nameof(superStructureFusesBroken): superStructureFusesBroken = float.Parse(value); break;
                        case nameof(minScavSquad): minScavSquad = int.Parse(value); break;
                        case nameof(maxScavSquad): maxScavSquad = int.Parse(value); break;
                        case nameof(scavMainTradeItem): scavMainTradeItem = new AbstractPhysicalObject.AbstractObjectType(value); break;
                        case nameof(scavTradeItems): scavTradeItems = ParseObjectTypeDictionary(value); break;
                        case nameof(scavTreasuryItems): scavTreasuryItems = ParseObjectTypeDictionary(value); break;
                        case nameof(scavGearItems): scavGearItems = ParseObjectTypeDictionary(value); break;
                        case nameof(eliteScavGearItems): eliteScavGearItems = ParseObjectTypeDictionary(value); break;
                        case nameof(scavScoreItems): scavScoreItems = ParseObjectTypeDictionary(value); break;
                        case nameof(dropwigBaitItems): dropwigBaitItems = ParseObjectTypeDictionary(value); break;


                        case nameof(rotEyeColor): rotEyeColor = ParseDLLColorDictionary(value); break;
                        case nameof(rotEffectColor): rotEffectColor = ParseDLLColorDictionary(value); break;

                        case "SSLightRodColor":
                        case nameof(lightRodColor): lightRodColor = Utils.ParseColor(value); break;
                        case nameof(batGlowColor): batGlowColor = Utils.ParseColor(value); break;

                        case nameof(dragonflyColor):
                            var color = RWCustom.Custom.RGB2HSL(Utils.ParseColor(value));
                            dragonflyColor = new(color.x, color.y, color.z); break;

                        case nameof(fireflyColor):
                            var color2 = RWCustom.Custom.RGB2HSL(Utils.ParseColor(value));
                            fireflyColor = new(color2.x, color2.y, color2.z); break;

                        case nameof(mapDefaultMatLayers):
                            mapDefaultMatLayers = new bool[3];
                            string[] array = value.Split(',').Select(x => x.Trim()).ToArray();
                            foreach (string s in array)
                            {
                                var number = int.Parse(s);
                                if (0 <= number && number <= 2)
                                {
                                    mapDefaultMatLayers[number] = true;
                                }
                            }
                            break;

                        case nameof(invPainJumps): invPainJumps = value == "True"; break;
                        case nameof(invExplosiveSnails): invExplosiveSnails = value == "True"; break;
                        case nameof(invWormgrassSpam): invWormgrassSpam = float.Parse(value); break;
                        case nameof(invGrimeSpam): invGrimeSpam = float.Parse(value); break;
                        case nameof(invBlackFade): invBlackFade = float.Parse(value); break;
                    }
                }
                catch (Exception e) { CustomRegionsMod.CustomLog($"[ERROR] failed to parse property [{key}: {value}]\n{e}", true); }
            }

            public static Dictionary<CreatureTemplate.Type, Color> ParseDLLColorDictionary(string s)
            {
                Dictionary<CreatureTemplate.Type, Color> result = new();
                string[] array = s.Split(',').Select(x => x.Trim()).ToArray();
                foreach (var pair in ParseColorDictionary(s))
                {
                    var type = new CreatureTemplate.Type(pair.Key, false);
                    if (type.index != -1 && StaticWorld.GetCreatureTemplate(type).TopAncestor().type == CreatureTemplate.Type.DaddyLongLegs)
                    {
                        result[type] = pair.Value;
                    }
                }

                if (result.Count > 0) return result;
                else return null;
            }

            public static Dictionary<string, Color> ParseColorDictionary(string s)
            {
                Dictionary<string, Color> result = new();
                string[] array = s.Split(',').Select(x => x.Trim()).ToArray();
                foreach (string str in array)
                {
                    string[] array2 = Regex.Split(str, "-");
                    result[array2[0]] = Utils.ParseColor(array2[1]);
                }

                if (result.Count > 0) return result;
                else return null;
            }

            public static Dictionary<AbstractPhysicalObject.AbstractObjectType, float> ParseObjectTypeDictionary(string s)
            {
                Dictionary<AbstractPhysicalObject.AbstractObjectType, float> result = new();
                string[] array = s.Split(',').Select(x => x.Trim()).ToArray();
                foreach (string str in array)
                {
                    string[] array2 = Regex.Split(str, "-");
                    AbstractPhysicalObject.AbstractObjectType type = new(array2[0]);
                    result[type] = float.Parse(array2[1]);
                }

                if (result.Count > 0) return result;
                else return null;
            }

            public bool? musicAfterCycle;

            public bool? hideTimer;

            public float? cycleLength;

            public float? rivStormyCycleLength;

            public float? rivStormyPreCycleChance;

            public float? forcePreCycleChance;

            public bool? wormGrassLight;

            public float? throwObjectsThreshold;

            public float? superStructureFusesBroken;

            public bool[] mapDefaultMatLayers;

            public int? minScavSquad;
            public int? maxScavSquad;

            public string voidSpawnTarget;

            public bool? hideVoidSpawn;

            public bool? postCycleMusic;

            public string sundownMusic;

            public HSLColor? dragonflyColor;

            public HSLColor? fireflyColor;

            public Color? lightRodColor;

            public Color? batGlowColor;

            public Dictionary<CreatureTemplate.Type, Color> rotEyeColor;

            public Dictionary<CreatureTemplate.Type, Color> rotEffectColor;

            public AbstractPhysicalObject.AbstractObjectType scavMainTradeItem = null;

            public Dictionary<AbstractPhysicalObject.AbstractObjectType, float> scavGearItems;

            public Dictionary<AbstractPhysicalObject.AbstractObjectType, float> eliteScavGearItems;

            public Dictionary<AbstractPhysicalObject.AbstractObjectType, float> scavTradeItems;

            public Dictionary<AbstractPhysicalObject.AbstractObjectType, float> scavTreasuryItems;

            public Dictionary<AbstractPhysicalObject.AbstractObjectType, float> scavScoreItems;


            public Dictionary<AbstractPhysicalObject.AbstractObjectType, float> dropwigBaitItems;

            public bool? invPainJumps; //CC gimmick
            public bool? invExplosiveSnails; //DS gimmick
            public float? invWormgrassSpam; //LF gimmick
            public float? invGrimeSpam; //VS gimmic
            public float? invBlackFade; //SB gimmick

        }

        private static ConditionalWeakTable<Region.RegionParams, RawProperties> _RawProperties = new();

        public static RawProperties GetRawProperties(this Region.RegionParams p) => _RawProperties.GetValue(p, _ => new());

        public static void ApplyHooks()
        {
            IL.Region.ctor += Region_ctor;
            IL.World.LoadMapConfig += World_LoadMapConfig;
        }

        private static void World_LoadMapConfig(ILContext il)
        {
            var c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.After, x => x.MatchCall(typeof(System.IO.File), nameof(System.IO.File.ReadAllLines))))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldfld, typeof(World).GetField("region"));
                c.Emit(OpCodes.Ldarg, 1);
                c.EmitDelegate(GenerateProperties);
            }
            else
            {
                CustomRegionsMod.BepLogError($"CustomRegions.RegionProperties.RegionProperties.World_LoadMapConfig: IL Hook failed.");
            }
        }


        private static void Region_ctor(ILContext il)
        {
            var c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.After, x => x.MatchCall(typeof(System.IO.File), nameof(System.IO.File.ReadAllLines))))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldarg, 4);
                c.EmitDelegate(GenerateProperties);
            }
            else
            {
                CustomRegionsMod.BepLogError($"CustomRegions.RegionProperties.RegionProperties.Region_ctor: IL Hook failed.");
            }
        }

        public static string[] GenerateProperties(string[] lines, Region self, SlugcatStats.Name slug)
        {
            try
            {
                RawProperties p = self.regionParams.GetRawProperties();
                foreach (string line in PreprocessProperties(lines, self, slug))
                {
                    if (line.IsNullOrWhiteSpace()) continue;

                    string[] array = Regex.Split(line, ": ");
                    p.RegisterProperty(array);
                }

                if (p.parent != null)
                {
                    p.InheritFromParent(GetParentProperties(self, p.parent));
                    CustomRegionsMod.CustomLog($"[{self.name}] properties inheriting from parent [{p.parent}]");
                    CustomRegionsMod.CustomLog(string.Join("\n", p.AllProperties), false, CustomRegionsMod.DebugLevel.FULL);
                }

                return p.AllProperties;
            }
            catch (Exception e) 
            {
                CustomRegionsMod.CustomLog("Error when generating custom properties!\n" + e, true);
                return lines;
            }
        }

        public static string[] PreprocessProperties(string[] lines, Region self, SlugcatStats.Name slug)
        {
            RegionInfo regionInfo = new()
            {
                RegionID = self.name,
                Lines = lines.ToList(),
                playerCharacter = slug
            };

            foreach (RegionPreprocessor filter in regionPreprocessors)
            {
                try
                {
                    filter(regionInfo);
                }
                catch (Exception e) { CustomRegionsMod.CustomLog($"Error when executing PreProcessor [{filter.Method.Name}]\n" + e.ToString(), true); }
            }

            return Utils.ProcessSlugcatConditions(regionInfo.Lines, slug).ToArray();
        }

        public static RawProperties GetParentProperties(Region self, string parentName)
        {
            if (self.regionNumber != RegionParentID) RecursionStopper = 0;
            else RecursionStopper++;

            if (RecursionStopper > 100)
            {
                CustomRegionsMod.CustomLog($"[WARNING]: Region [{self.name}] properties file exceeded 100 parents! Possible infinite inheritance loop?");
                return null;
            }

            string[] array2 = Regex.Split(parentName, "-");
            SlugcatStats.Name parentSlug = array2.Length >= 2 ? new(array2[1]) : null;
            CustomRegionsMod.CustomLog($"region [{self.name}] is loading parent [{array2[0]}] for slug [{parentSlug}]");
            Region parent = new(array2[0], 0, RegionParentID, parentSlug);

            return parent.regionParams.GetRawProperties();
        }

        private static int RecursionStopper = 0;
        private const int RegionParentID = -10;


        public class RawProperties
        {
            public void RegisterProperty(string[] propertyLine)
            {
                if (propertyLine[0] == "Room_Attr" && propertyLine.Length >= 3)
                { RoomAttractions[propertyLine[1]] = propertyLine[2]; }

                else if (propertyLine.Length != 2)
                { unrecognized.Add(string.Join(": ", propertyLine)); }

                else if (propertyLine[0] == "PARENT")
                { parent = propertyLine[1]; }

                else if (propertyLine[0] == "Subregion")
                { subregions.Add(propertyLine[1]); }

                else if (VanillaProperty(propertyLine[0]))
                { VanillaProperties[propertyLine[0]] = propertyLine[1]; }

                else
                { CustomProperties[propertyLine[0]] = propertyLine[1]; }
            }

            public void InheritFromParent(RawProperties parent)
            {
                foreach (var pair in parent.RoomAttractions)
                {
                    if (!RoomAttractions.ContainsKey(pair.Key))
                    { RoomAttractions[pair.Key] = pair.Value; }
                }
                foreach (var pair in parent.VanillaProperties)
                {
                    if (!VanillaProperties.ContainsKey(pair.Key))
                    { VanillaProperties[pair.Key] = pair.Value; }
                }
                foreach (var pair in parent.CustomProperties)
                {
                    if (!CustomProperties.ContainsKey(pair.Key))
                    { CustomProperties[pair.Key] = pair.Value; }
                }

                for (int i = 0; i < parent.subregions.Count; i++)
                {
                    if (i >= subregions.Count)
                    { subregions.Add(parent.subregions[i]); }
                }
            }

            public string[] AllProperties => VanillaProperties.Select(p => p.Key + ": " + p.Value)
                                     .Concat(subregions.Select(s => "Subregion: " + s))
                                     .Concat(CustomProperties.Select(p => p.Key + ": " + p.Value))
                                     .Concat(unrecognized)
                                     .Concat(RoomAttractions.Select(p => "Room_Attr: " + p.Key + ": " + p.Value))
                                     .ToArray();

            public Dictionary<string, string> VanillaProperties = new();
            public Dictionary<string, string> CustomProperties = new();
            public Dictionary<string, string> RoomAttractions = new();
            public List<string> subregions = new();
            public List<string> unrecognized = new();

            public string parent;
        }

        private static bool VanillaProperty(string name)
        {
            switch (name)
            {
                case "Room Setting Templates":
                case "batDepleteCyclesMin":
                case "batDepleteCyclesMax":
                case "batDepleteCyclesMaxIfLessThanTwoLeft":
                case "batDepleteCyclesMaxIfLessThanFiveLeft":
                case "overseersSpawnChance":
                case "overseersMin":
                case "overseersMax":
                case "playerGuideOverseerSpawnChance":
                case "scavsMin":
                case "scavsMax":
                case "scavsSpawnChance":
                case "Subregion":
                case "batsPerActiveSwarmRoom":
                case "batsPerInactiveSwarmRoom":
                case "blackSalamanderChance":
                case "corruptionEffectColor":
                case "corruptionEyeColor":
                case "kelpColor":
                case "albinos":
                case "waterColorOverride":
                case "scavsDelayInitialMin":
                case "scavsDelayInitialMax":
                case "scavsDelayRepeatMin":
                case "scavsDelayRepeatMax":
                case "pupSpawnChance":
                case "GlacialWasteland":
                case "earlyCycleChance":
                case "earlyCycleFloodChance":
                    return true;
                default: return false;
            }
        }
    }
}
