using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using CustomRegions.Mod;
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

        private static void MultiplayerMenu_ctor(ILContext il)
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
                CustomRegionsMod.BepLogError("CustomRegions.Arena.Properties.MultiplayerMenu_ctor: IL Hook failed.");
            }
        }

        private static void OverWorld_LoadWorld(On.OverWorld.orig_LoadWorld orig, OverWorld self, string worldName, SlugcatStats.Name playerCharacterNumber, bool singleRoomWorld)
        {
            orig(self, worldName, playerCharacterNumber, singleRoomWorld);

            if (singleRoomWorld)
            {
                string text = WorldLoader.FindRoomFile(self.activeWorld.GetAbstractRoom(0).name, false, "_Properties.txt");
                if (File.Exists(text))
                {
                    self.activeWorld.region = new Region(self.activeWorld.GetAbstractRoom(0).name, 0, -1, null);
                }
            }
        }

        private static void Region_ctor(On.Region.orig_ctor orig, Region self, string name, int firstRoomIndex, int regionNumber, SlugcatStats.Name storyIndex)
        {
            orig(self, name, firstRoomIndex, regionNumber, storyIndex);

            try
            {
                if (Region.GetFullRegionOrder().Contains(name)) return;

                string properties = WorldLoader.FindRoomFile(name, false, "_Properties.txt");
                if (!File.Exists(properties)) return;

                CustomRegionsMod.CustomLog($"loading arena properties for room [{name}]");
                foreach (string line in RegionProperties.RegionProperties.GenerateProperties(File.ReadAllLines(properties), self, storyIndex))
                {
                    string[] array = Regex.Split(RWCustom.Custom.ValidateSpacedDelimiter(line, ":"), ": ");
                    if (array.Length < 2) { continue; }
                    PropertiesForArena(array[0], array[1], self);
                }
            }
            catch (Exception e) { CustomRegionsMod.CustomLog($"[ERROR] while loading arena properties, aborting...\n{e}", true); }
        }

        private static void PropertiesForArena(string key, string value, Region self)
        {
            try
            {
                var regionParams = self.regionParams;
                switch (key)
                {
                    case "albinos": regionParams.albinos = value.Trim().ToLower() == "true"; break;
                    case "blackSalamanderChance": regionParams.blackSalamanderChance = float.Parse(value); break;
                    case "corruptionEffectColor": regionParams.corruptionEffectColor = Utils.ParseColor(value); break;
                    case "corruptionEyeColor": regionParams.corruptionEyeColor = Utils.ParseColor(value); break;
                    case "kelpColor": regionParams.kelpColor = Utils.ParseColor(value); break;
                    case "GlacialWasteland": regionParams.glacialWasteland = int.Parse(value) > 0; break;
                    //none of the rest are really usable in arena :/
                    /*case "Room Setting Templates":
                        {
                            string[] array7 = Regex.Split(RWCustom.Custom.ValidateSpacedDelimiter(value, ","), ", ");
                            self.roomSettingsTemplates = new RoomSettings[array7.Length];
                            self.roomSettingTemplateNames = new string[array7.Length];
                            for (int j = 0; j < array7.Length; j++)
                            {
                                self.roomSettingTemplateNames[j] = array7[j];
                                self.ReloadRoomSettingsTemplate(array7[j]);
                            }

                            break;
                        }
                    case "waterColorOverride": self.propertiesWaterColor = Utils.ParseColor(value); break;
                    case "batDepleteCyclesMin": regionParams.batDepleteCyclesMin = int.Parse(value); break;
                    case "batDepleteCyclesMax": regionParams.batDepleteCyclesMax = int.Parse(value); break;
                    case "batDepleteCyclesMaxIfLessThanTwoLeft": regionParams.batDepleteCyclesMaxIfLessThanTwoLeft = int.Parse(value); break;
                    case "batDepleteCyclesMaxIfLessThanFiveLeft": regionParams.batDepleteCyclesMaxIfLessThanFiveLeft = int.Parse(value); break;
                    case "overseersSpawnChance": regionParams.overseersSpawnChance = float.Parse(value); break;
                    case "overseersMin": regionParams.overseersMin = int.Parse(value); break;
                    case "overseersMax": regionParams.overseersMax = int.Parse(value); break;
                    case "playerGuideOverseerSpawnChance": regionParams.playerGuideOverseerSpawnChance = float.Parse(value); break;
                    case "scavsMin": regionParams.scavsMin = int.Parse(value); break;
                    case "scavsMax": regionParams.scavsMax = int.Parse(value); break;
                    case "scavsSpawnChance": regionParams.scavsSpawnChance = int.Parse(value); break;
                    case "Subregion": self.subRegions.Add(value); self.altSubRegions.Add(null); break;
                    case "batsPerActiveSwarmRoom": regionParams.batsPerActiveSwarmRoom = int.Parse(value); break;
                    case "batsPerInactiveSwarmRoom": regionParams.batsPerInactiveSwarmRoom = int.Parse(value); break;
                    case "scavsDelayInitialMin": regionParams.scavengerDelayInitialMin = int.Parse(value); break;
                    case "scavsDelayInitialMax": regionParams.scavengerDelayInitialMax = int.Parse(value); break;
                    case "scavsDelayRepeatMin": regionParams.scavengerDelayRepeatMin = int.Parse(value); break;
                    case "scavsDelayRepeatMax": regionParams.scavengerDelayRepeatMax = int.Parse(value); break;
                    case "pupSpawnChance": regionParams.slugPupSpawnChance = float.Parse(value); break;
                    case "earlyCycleChance": regionParams.earlyCycleChance = float.Parse(value); break;
                    case "earlyCycleFloodChance": regionParams.earlyCycleFloodChance = float.Parse(value); break;*/
                }
            }
            catch (Exception e) { CustomRegionsMod.CustomLog($"[ERROR] failed to parse arena property [{key}: {value}]\n{e}", true); }
        }
    }
}
