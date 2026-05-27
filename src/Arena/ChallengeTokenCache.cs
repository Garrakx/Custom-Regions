using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System.Collections;
using CustomRegions.Mod;

namespace CustomRegions.Arena
{
    internal static class ChallengeTokenCache
    {
        const string tokenind = "CRSPurpleToken: ";

        private static ConditionalWeakTable<RainWorld, Dictionary<string, List<List<SlugcatStats.Name>>>> _regionPurpleTokensAccessibility = new();
        public static Dictionary<string, List<List<SlugcatStats.Name>>> regionPurpleTokensAccessibility(this RainWorld p) => _regionPurpleTokensAccessibility.GetValue(p, _ => new());


        private static ConditionalWeakTable<RainWorld, Dictionary<string, List<ChallengeData.ChallengeUnlockID>>> _regionPurpleTokens = new();
        public static Dictionary<string, List<ChallengeData.ChallengeUnlockID>> regionPurpleTokens(this RainWorld p) => _regionPurpleTokens.GetValue(p, _ => new());

        private static ConditionalWeakTable<MoreSlugcats.CollectiblesTracker.SaveGameData, List<ChallengeData.ChallengeUnlockID>> _unlockedPurples = new();
        public static List<ChallengeData.ChallengeUnlockID> unlockedPurples(this MoreSlugcats.CollectiblesTracker.SaveGameData self, PlayerProgression progression)
        {
            return progression.miscProgressionData.CustomSaveData().unlockedChallenges;
        }

        public static void ApplyHooks()
        {
            On.RainWorld.ReadTokenCache += RainWorld_ReadTokenCache;
            IL.RainWorld.BuildTokenCache += RainWorld_BuildTokenCache;
            On.MoreSlugcats.CollectiblesTracker.ctor += CollectiblesTracker_ctor;
            On.RainWorld.BuildTokenCache += RainWorld_BuildTokenCache1;
        }

        private static void RainWorld_BuildTokenCache1(On.RainWorld.orig_BuildTokenCache orig, RainWorld self, bool modded, string region)
        {
            if (ChallengeData.customChallenges.Count == 0 && Collectables.ArenaUnlocks.customLevelUnlocks.Count == 0)
            {
                ChallengeData.Refresh();
                Collectables.ArenaUnlocks.RefreshArenaUnlocks();
            }
            orig(self, modded, region);
        }

        private static void CollectiblesTracker_ctor(On.MoreSlugcats.CollectiblesTracker.orig_ctor orig, MoreSlugcats.CollectiblesTracker self, Menu.Menu menu, Menu.MenuObject owner, UnityEngine.Vector2 pos, FContainer container, SlugcatStats.Name saveSlot)
        {
            orig(self, menu, owner, pos, container, saveSlot);
            RainWorld rainWorld = menu.manager.rainWorld;
            for (int l = 0; l < self.displayRegions.Count; l++)
            {
                if (self.collectionData == null || !self.collectionData.regionsVisited.Contains(self.displayRegions[l])) continue;

                UnityEngine.Debug.Log($"loading purple tokens for region [{self.displayRegions[l]}] slugcat [{saveSlot}]");

                for (int m = 0; m < rainWorld.regionPurpleTokens()[self.displayRegions[l]].Count; m++)
                {
                    UnityEngine.Debug.Log(string.Join("|", rainWorld.regionPurpleTokensAccessibility()[self.displayRegions[l]][m]));
                    if (!rainWorld.regionPurpleTokensAccessibility()[self.displayRegions[l]][m].Contains(saveSlot)) continue;

                    bool unlocked = rainWorld.progression.miscProgressionData.CustomSaveData().unlockedChallenges.Contains(rainWorld.regionPurpleTokens()[self.displayRegions[l]][m]);

                    UnityEngine.Color color = ChallengeToken.PurpleColor.rgb;

                    var sprite = new FSprite(unlocked ? "ctOn" : "ctOff", true);
                    sprite.color = color;

                    self.spriteColors[self.displayRegions[l]].Add(color);
                    self.sprites[self.displayRegions[l]].Add(sprite);

                    container.AddChild(sprite);
                }

            }
        }

        private static void RainWorld_BuildTokenCache(ILContext il)
        {
            var c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdfld(out var field) && field.Name == "fileName",
                x => x.MatchNewobj<List<List<SlugcatStats.Name>>>(),
                x => x.MatchCallvirt(typeof(Dictionary<string, List<List<SlugcatStats.Name>>>).GetMethod("set_Item"))
            ))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldarg_2);
                c.EmitDelegate((RainWorld self, string region) =>
                {
                    try
                    {
                        var fileName = region.ToLowerInvariant();
                        self.regionPurpleTokens()[fileName] = new();
                        self.regionPurpleTokensAccessibility()[fileName] = new();
                    } catch (Exception e) { CustomRegionsMod.CustomLog($"adding region {region} to purple tokencache broke!\n" + e, true); }
                });
            }
            else
            {
                CustomRegionsMod.BepLogError("CustomRegions.Arena.ChallengeTokenCache.RainWorld_BuildTokenCache: IL Hook failed.");
            }

            int num = 27;
            if (c.TryGotoNext(MoveType.After,
                x => x.MatchNewobj<PlacedObject>(),
                x => x.MatchStloc(out num),
                x => x.MatchLdloc(num),
                x => x.MatchLdloc(out _),
                x => x.MatchCallvirt<PlacedObject>(nameof(PlacedObject.FromString))
                ))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldarg_2);
                c.Emit(OpCodes.Ldloc, 12);
                c.Emit(OpCodes.Ldloc, num);
                c.EmitDelegate((RainWorld self, string region, List<SlugcatStats.Name> slugcats, PlacedObject placedObject) => 
                {
                    try
                    {
                        var fileName = region.ToLowerInvariant();
                        if (!self.regionPurpleTokens().ContainsKey(fileName) || placedObject.type != ChallengeToken.PurpleToken || !ExtEnum<ChallengeData.ChallengeUnlockID>.values.entries.Contains((placedObject.data as CollectToken.CollectTokenData).tokenString))
                            return;

                        var collectTokenData = (placedObject.data as CollectToken.CollectTokenData);
                        var item = new ChallengeData.ChallengeUnlockID(collectTokenData.tokenString, false);
                        if (!self.regionPurpleTokens()[fileName].Contains(item))
                        {
                            self.regionPurpleTokens()[fileName].Add(item);
                            self.regionPurpleTokensAccessibility()[fileName].Add(self.FilterTokenClearance(collectTokenData.availableToPlayers, /*oldData*/ new(), slugcats));
                        }
                        else
                        {
                            int index = self.regionPurpleTokens()[fileName].IndexOf(item);
                            self.regionPurpleTokensAccessibility()[fileName][index] = self.FilterTokenClearance(collectTokenData.availableToPlayers, self.regionPurpleTokensAccessibility()[fileName][index], slugcats);
                        }
                    }
                    catch (Exception e) { CustomRegionsMod.CustomLog($"failed to register PurpleToken for region {region}!\n" + e, true); }
                });
            }
            else
            {
                CustomRegionsMod.BepLogError("failed to il hook BuildTokenCache for adding purple token to region");
            }


            if (c.TryGotoNext(MoveType.AfterLabel,
                x => x.MatchLdloc(1),
                x => x.MatchLdstr("tokencache"),
                x => x.MatchLdloc(0),
                x => x.MatchLdfld(out var field) && field.Name == "fileName",
                x => x.MatchLdstr(".txt"),
                x => x.MatchCall<string>(nameof(string.Concat)),
                x => x.MatchLdloc(5),
                x => x.MatchCall(typeof(File), nameof(File.WriteAllText))
                ))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldarg_2);
                c.Emit(OpCodes.Ldloc, 5);
                c.EmitDelegate((RainWorld self, string region, string text) =>
                {
                    try
                    {
                        var fileName = region.ToLowerInvariant();
                        if (!self.regionPurpleTokens().ContainsKey(fileName) || self.regionPurpleTokens()[fileName].Count == 0) return text;
                        text += "&" + tokenind;
                        for (int num4 = 0; num4 < self.regionPurpleTokens()[fileName].Count; num4++)
                        {
                            string str8 = string.Join("|", Array.ConvertAll(self.regionPurpleTokensAccessibility()[fileName][num4].ToArray(), (SlugcatStats.Name x) => x.ToString()));
                            text = text + self.regionPurpleTokens()[fileName][num4].ToString() + "~" + str8;
                            if (num4 != self.regionPurpleTokens()[fileName].Count - 1)
                            {
                                text += ",";
                            }
                        }
                    }
                    catch (Exception e) { CustomRegionsMod.CustomLog($"building purple token string broke for region {region}!\n" + e, true); }
                    return text;
                });
                c.Emit(OpCodes.Stloc, 5);
            }
            else
            {
                CustomRegionsMod.BepLogError("failed to il hook BuildTokenCache for writing to text file");
            }
        }

        private static void RainWorld_ReadTokenCache(On.RainWorld.orig_ReadTokenCache orig, RainWorld self)
        {
            orig(self);
            self.regionPurpleTokensAccessibility().Clear();
            string[] array = File.ReadAllLines(AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar.ToString() + "regions.txt"));
            for (int i = 0; i < array.Length; i++)
            {
                string region = array[i].ToLowerInvariant();

                self.regionPurpleTokens()[region] = new();
                self.regionPurpleTokensAccessibility()[region] = new();

                string path = AssetManager.ResolveFilePath(string.Concat(new string[]
            {
                "World",
                Path.DirectorySeparatorChar.ToString(),
                "indexmaps",
                Path.DirectorySeparatorChar.ToString(),
                "tokencache",
                region,
                ".txt"
            }));
                if (File.Exists(path))
                {
                    string[] array2 = File.ReadAllText(path).Split('&');

                    if (array2.Length >= 7 && array2[6].StartsWith(tokenind) && array2[6].Length > tokenind.Length)
                    {
                        UnityEngine.Debug.Log(array2[6]);
                        string[] array3 = Regex.Split(array2[6].Substring(tokenind.Length), ",");
                        for (int k = 0; k < array3.Length; k++)
                        {
                            string[] array4 = Regex.Split(array3[k], "~");
                            List<SlugcatStats.Name> list = new();
                            string[] array5 = array4[1].Split( '|' );
                            for (int l = 0; l < array5.Length; l++)
                            {
                                list.Add(new SlugcatStats.Name(array5[l], false));
                            }
                            self.regionPurpleTokens()[region].Add(new ChallengeData.ChallengeUnlockID(array4[0], false));
                            self.regionPurpleTokensAccessibility()[region].Add(list);
                        }
                    }
                }
            }
        }
            
    }
}
