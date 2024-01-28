using System;
using System.Globalization;
using System.IO;
using static MonoMod.InlineRT.MonoModRule;
using System.Text.RegularExpressions;
using UnityEngine;
using CustomRegions.Mod;
using static System.Net.Mime.MediaTypeNames;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace CustomRegions.Arena
{
    internal static class Properties
    {
        public static void ApplyHooks()
        {
            On.OverWorld.LoadWorld += OverWorld_LoadWorld;
            On.Region.ctor += Region_ctor;
            IL.Menu.MultiplayerMenu.ctor += MultiplayerMenu_ctor;
        }

        private static void MultiplayerMenu_ctor(MonoMod.Cil.ILContext il)
        {
            var c = new ILCursor(il);
            if (c.TryGotoNext(
                MoveType.After,
                x => x.MatchLdstr("_arena.txt"),
                x => x.MatchCall<string>("op_Inequality")
                ))
            {
                c.Emit(OpCodes.Ldloc_0);
                c.Emit(OpCodes.Ldloc_2);
                c.EmitDelegate((bool result, string[] array, int i) => { return result && !array[i].EndsWith("_properties.txt"); });
            }
            else
            {
                CustomRegionsMod.BepLogError("failed to IL hook Menu.MultiplayerMenu.ctor!");
            }
        }

        private static void Region_ctor(On.Region.orig_ctor orig, Region self, string name, int firstRoomIndex, int regionNumber, SlugcatStats.Name storyIndex)
        {
            orig(self, name, firstRoomIndex, regionNumber, storyIndex);

            if (!Region.GetFullRegionOrder().Contains(name))
            {
                string properties = WorldLoader.FindRoomFile(name, false, "_Properties.txt");
                if (File.Exists(properties))
                { 
                CustomRegionsMod.CustomLog($"loading arena properties [{properties}]");
                Region.RegionParams regionParams = self.regionParams;
                string[] array = File.ReadAllLines(properties);
                for (int i = 0; i < array.Length; i++)
                {
                    string[] array2 = Regex.Split(RWCustom.Custom.ValidateSpacedDelimiter(array[i], ":"), ": ");
                    if (array2.Length < 2)
                    {
                        continue;
                    }

                    switch (array2[0])
                    {
                        /*case "Room Setting Templates":
                            {
                                string[] array7 = Regex.Split(Custom.ValidateSpacedDelimiter(array2[1], ","), ", ");
                                roomSettingsTemplates = new RoomSettings[array7.Length];
                                roomSettingTemplateNames = new string[array7.Length];
                                for (int j = 0; j < array7.Length; j++)
                                {
                                    roomSettingTemplateNames[j] = array7[j];
                                    ReloadRoomSettingsTemplate(array7[j]);
                                }

                                break;
                            }
                        case "batDepleteCyclesMin":
                            regionParams.batDepleteCyclesMin = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                            break;
                        case "batDepleteCyclesMax":
                            regionParams.batDepleteCyclesMax = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                            break;
                        case "batDepleteCyclesMaxIfLessThanTwoLeft":
                            regionParams.batDepleteCyclesMaxIfLessThanTwoLeft = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                            break;
                        case "batDepleteCyclesMaxIfLessThanFiveLeft":
                            regionParams.batDepleteCyclesMaxIfLessThanFiveLeft = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                            break;
                        case "overseersSpawnChance":
                            regionParams.overseersSpawnChance = float.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                            break;
                        case "overseersMin":
                            regionParams.overseersMin = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                            break;
                        case "overseersMax":
                            regionParams.overseersMax = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                            break;
                        case "playerGuideOverseerSpawnChance":
                            regionParams.playerGuideOverseerSpawnChance = float.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                            break;
                        case "scavsMin":
                            regionParams.scavsMin = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                            break;
                        case "scavsMax":
                            regionParams.scavsMax = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                            break;
                        case "scavsSpawnChance":
                            regionParams.scavsSpawnChance = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                            break;
                        case "Subregion":
                            if (flag)
                            {
                                altSubRegions.Add(array2[1]);
                                break;
                            }

                            subRegions.Add(array2[1]);
                            altSubRegions.Add(null);
                            break;
                        case "batsPerActiveSwarmRoom":
                            regionParams.batsPerActiveSwarmRoom = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                            break;
                        case "batsPerInactiveSwarmRoom":
                            regionParams.batsPerInactiveSwarmRoom = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                            break;*/
                        case "blackSalamanderChance":
                            regionParams.blackSalamanderChance = float.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                            break;
                        case "corruptionEffectColor":
                            {
                                string[] array6 = array2[1].Split(',');
                                if (array6.Length == 3)
                                {
                                    regionParams.corruptionEffectColor = new Color(float.Parse(array6[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array6[1], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array6[2], NumberStyles.Any, CultureInfo.InvariantCulture));
                                }

                                break;
                            }
                        case "corruptionEyeColor":
                            {
                                string[] array5 = array2[1].Split(',');
                                if (array5.Length == 3)
                                {
                                    regionParams.corruptionEyeColor = new Color(float.Parse(array5[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array5[1], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array5[2], NumberStyles.Any, CultureInfo.InvariantCulture));
                                }

                                break;
                            }
                        case "kelpColor":
                            {
                                string[] array4 = array2[1].Split(',');
                                if (array4.Length == 3)
                                {
                                    regionParams.kelpColor = new Color(float.Parse(array4[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array4[1], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array4[2], NumberStyles.Any, CultureInfo.InvariantCulture));
                                }

                                break;
                            }
                        case "albinos":
                            regionParams.albinos = array2[1].Trim().ToLower() == "true";
                            break;
                        /*case "waterColorOverride":
                            {
                                string[] array3 = Regex.Split(RWCustom.Custom.ValidateSpacedDelimiter(array2[1], ","), ", ");
                                self.propertiesWaterColor = new Color(float.Parse(array3[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array3[2], NumberStyles.Any, CultureInfo.InvariantCulture));
                                break;
                            }
                        case "scavsDelayInitialMin":
                            regionParams.scavengerDelayInitialMin = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                            break;
                        case "scavsDelayInitialMax":
                            regionParams.scavengerDelayInitialMax = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                            break;
                        case "scavsDelayRepeatMin":
                            regionParams.scavengerDelayRepeatMin = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                            break;
                        case "scavsDelayRepeatMax":
                            regionParams.scavengerDelayRepeatMax = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                            break;
                        case "pupSpawnChance":
                            regionParams.slugPupSpawnChance = float.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                            break;*/
                        case "GlacialWasteland":
                            regionParams.glacialWasteland = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture) > 0;
                            break;
                            /*case "earlyCycleChance":
                                regionParams.earlyCycleChance = float.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                                break;
                            case "earlyCycleFloodChance":
                                regionParams.earlyCycleFloodChance = float.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
                                break;*/
                    }
                }
                }
            }
        }

        private static void OverWorld_LoadWorld(On.OverWorld.orig_LoadWorld orig, OverWorld self, string worldName, SlugcatStats.Name playerCharacterNumber, bool singleRoomWorld)
        {
            orig(self, worldName, playerCharacterNumber, singleRoomWorld);

            if (singleRoomWorld)
            {
                CustomRegionsMod.CustomLog($"loading single world [{worldName}] with room [{self.activeWorld.GetAbstractRoom(0).name}]");
                string text = WorldLoader.FindRoomFile(self.activeWorld.GetAbstractRoom(0).name, false, "_Properties.txt");
                if (File.Exists(text))
                {
                    CustomRegionsMod.CustomLog($"found arena properties [{text}]");
                    self.activeWorld.region = new Region(self.activeWorld.GetAbstractRoom(0).name, 0, -1, null);
                }
            }
        }
    }
}
