using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CustomRegions.Mod;
using Menu;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace CustomRegions.Arena
{
    internal static class ChallengeSupport
    {
        

        public static ConditionalWeakTable<MoreSlugcats.ChallengeInformation.ChallengeMeta, StrongBox<Structs.CustomChallenge?>> _challengeMeta = new();
        public static StrongBox<Structs.CustomChallenge?> ChallengeCustomData(this MoreSlugcats.ChallengeInformation.ChallengeMeta p) => _challengeMeta.GetValue(p, _ => new(null));

        public static ConditionalWeakTable<ArenaSetup.GameTypeSetup, StrongBox<Structs.CustomChallenge?>> _gameTypeSetup = new();
        public static StrongBox<Structs.CustomChallenge?> CustomChallengeID(this ArenaSetup.GameTypeSetup p) => _gameTypeSetup.GetValue(p, _ => new(null));

        public static void ApplyHooks()
        {
            On.Menu.MultiplayerMenu.IsChallengeUnlocked += MultiplayerMenu_IsChallengeUnlocked;
            On.MoreSlugcats.ChallengeInformation.ChallengeMeta.ctor += ChallengeMeta_ctor1;
            IL.MoreSlugcats.ChallengeInformation.ChallengeMeta.ctor += ChallengeMeta_ctor;
            On.MoreSlugcats.ChallengeInformation.ChallengePath += ChallengeInformation_ChallengePath;
            IL.Room.ctor += Room_ctor;
            On.SandboxGameSession.SpawnCreatures += SandboxGameSession_SpawnCreatures;
            On.ArenaGameSession.EndSession += ArenaGameSession_EndSession;
            On.Menu.MultiplayerMenu.ChallengeUnlockDescription += MultiplayerMenu_ChallengeUnlockDescription;
            On.MoreSlugcats.ChallengeInformation.ctor += ChallengeInformation_ctor;
            On.ArenaSetup.GameTypeSetup.InitAsGameType += GameTypeSetup_InitAsGameType;
            On.ArenaSetup.GameTypeSetup.FromString += GameTypeSetup_FromString;
            On.Menu.PauseMenu.ctor += PauseMenu_ctor;
        }

        private static void PauseMenu_ctor(On.Menu.PauseMenu.orig_ctor orig, PauseMenu self, ProcessManager manager, RainWorldGame game)
        {
            orig(self, manager, game);
            if (game.IsArenaSession && game.GetArenaGameSession.chMeta?.ChallengeCustomData().Value is Structs.CustomChallenge challenge)
            {
                foreach (MenuObject obj in self.pages[0].subObjects)
                {
                    if (obj is MenuLabel label && label.text.StartsWith(self.Translate("Challenge #<X>").Replace("<X>", game.GetArenaGameSession.chMeta.challengeNumber.ToString())))
                    {
                        label.text = challenge.LocalizedID(self) + " " + label.text;
                    }
                }
            }
        }

        private static void GameTypeSetup_FromString(On.ArenaSetup.GameTypeSetup.orig_FromString orig, ArenaSetup.GameTypeSetup self, string s)
        {
            orig(self, s);
            if (ModManager.MSC && self.gameType == MoreSlugcats.MoreSlugcatsEnums.GameTypeID.Challenge && self.CustomChallengeID().Value is Structs.CustomChallenge challenge)
            {
                try
                {
                    ChallengeData.currentChallenge = challenge;
                    self.challengeMeta = new MoreSlugcats.ChallengeInformation.ChallengeMeta(self.challengeID);
                }
                catch (Exception e) { CustomRegionsMod.CustomLog(e.ToString()); }
                ChallengeData.currentChallenge = null;
            }
        }

        private static void GameTypeSetup_InitAsGameType(On.ArenaSetup.GameTypeSetup.orig_InitAsGameType orig, ArenaSetup.GameTypeSetup self, ArenaSetup.GameTypeID gameType)
        {
            orig(self, gameType);
            if (ModManager.MSC && gameType == MoreSlugcats.MoreSlugcatsEnums.GameTypeID.Challenge && self.CustomChallengeID().Value is Structs.CustomChallenge challenge)
            {
                try
                {
                    ChallengeData.currentChallenge = challenge;
                    self.challengeMeta = new MoreSlugcats.ChallengeInformation.ChallengeMeta(self.challengeID);
                }
                catch (Exception e) { CustomRegionsMod.CustomLog(e.ToString()); }
                ChallengeData.currentChallenge = null;
            }
        }

        private static void ChallengeInformation_ctor(On.MoreSlugcats.ChallengeInformation.orig_ctor orig, MoreSlugcats.ChallengeInformation self, Menu.Menu menu, MenuObject owner, int challengeID)
        {
            orig(self, menu, owner, challengeID);
            if (ChallengeData.currentChallenge is Structs.CustomChallenge challenge)
            {
                foreach (MenuObject obj in self.subObjects)
                {
                    if (obj is MenuLabel label && label.text.StartsWith(menu.Translate("Challenge #<X>").Replace("<X>", challengeID.ToString())))
                    {
                        label.text = challenge.LocalizedID(menu) + " " + label.text;
                    }
                }
            }
        }

        private static string MultiplayerMenu_ChallengeUnlockDescription(On.Menu.MultiplayerMenu.orig_ChallengeUnlockDescription orig, MultiplayerMenu self, int challengeNumber)
        {
            if (ChallengeData.currentChallenge is Structs.CustomChallenge challenge) return ChallengeRequirements.AllRequirementDescriptions(challenge, challengeNumber, self);
            return orig(self, challengeNumber);
        }

        private static void ArenaGameSession_EndSession(On.ArenaGameSession.orig_EndSession orig, ArenaGameSession self)
        {
            if (self.chMeta?.ChallengeCustomData().Value is Structs.CustomChallenge challenge && self.exitManager.challengeCompleted)
            {
                self.challengeCompleted = true;
                var completeChallenges = self.game.rainWorld.progression.miscProgressionData.CustomSaveData().completeChallenges;
                if (!completeChallenges.ContainsKey(challenge.id))
                { completeChallenges[challenge.id] = new(); }

                for (int i = completeChallenges[challenge.id].Count; i < self.GameTypeSetup.challengeID; i++)
                { completeChallenges[challenge.id].Add(false); }

                completeChallenges[challenge.id][self.GameTypeSetup.challengeID - 1] = true;

                if (self.chMeta.specialUnlock && !self.game.rainWorld.progression.miscProgressionData.challengeArenaUnlocks.Contains(self.chMeta.arena))
                {
                    self.game.rainWorld.progression.miscProgressionData.challengeArenaUnlocks.Add(self.chMeta.arena);
                }
                self.game.rainWorld.progression.SaveProgression(false, true);

                self.exitManager.challengeCompleted = false; //don't run orig stuffs lol
            }
            orig(self);

        }

        private static void SandboxGameSession_SpawnCreatures(On.SandboxGameSession.orig_SpawnCreatures orig, SandboxGameSession self)
        {
            try
            {
                if (self.arenaSitting.gameTypeSetup.CustomChallengeID().Value is Structs.CustomChallenge challenge)
                {
                    ChallengeData.currentChallenge = challenge;
                }
            }
            catch (Exception e) { CustomRegionsMod.CustomLog(e.ToString()); ChallengeData.currentChallenge = null; }
            try
            {
                orig(self);
            }
            catch (Exception e) { CustomRegionsMod.CustomLog(e.ToString()); }
            ChallengeData.currentChallenge = null;
        }

        //ArenaSetup.GameTypeSetup.ChallengeID
        // - used by Room.Room (for finding alt _settings.txt files)
        // - used by SandboxGameSession.SpawnCreatures for spawning sandbox specific creatures

        //ArenaSetup.GameTypeSetup.challengeMeta
        // - assigned by ArenaSetup.GameTypeSetup.FromString (loading arena save data, needs hook)
        // - assigned by ArenaSetup.GameTypeSetup.InitAsGameType (used almost the same time as above, needs hook)
        // - assigned by ArenaSetup.GameTypeSetup.challengeID.set (called when init menu or clicking a button, currently doesn't need hook)

        //



        /// <summary>
        /// challenge room settings
        /// </summary>
        private static void Room_ctor(ILContext il)
        {
            var c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdstr("_Settings.txt"),
                x => x.MatchStelemRef(),
                x => x.MatchCall<string>(nameof(string.Concat))
                ))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((string orig, Room self) => {
                    if (self.game.GetArenaGameSession.arenaSitting.gameTypeSetup.CustomChallengeID().Value is Structs.CustomChallenge challenge)
                        return $"Levels\\Challenges\\{challenge.levels[self.game.GetArenaGameSession.arenaSitting.gameTypeSetup.challengeID - 1]}_settings.txt";

                    else return orig;
                });
            }

            if (c.TryGotoNext(MoveType.After,
                x => x.MatchCallvirt<ArenaSetup.GameTypeSetup>("get_challengeID"),
                x => x.MatchStloc(1),
                x => x.MatchLdloca(1),
                x => x.MatchCall<int>(nameof(int.ToString)),
                x => x.MatchCall<string>(nameof(string.Concat))
                ))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((string orig, Room self) => {
                    if (self.game.GetArenaGameSession.arenaSitting.gameTypeSetup.CustomChallengeID().Value is Structs.CustomChallenge challenge)
                        return $"Challenges\\{challenge.levels[self.game.GetArenaGameSession.arenaSitting.gameTypeSetup.challengeID - 1]}";

                    else return orig;
                });
            }
        }

        private static string ChallengeInformation_ChallengePath(On.MoreSlugcats.ChallengeInformation.orig_ChallengePath orig, int challengeID)
        {
            if (ChallengeData.currentChallenge is Structs.CustomChallenge challenge) return AssetManager.ResolveFilePath($"Levels\\Challenges\\{challenge.levels[challengeID - 1]}.txt");
            return orig(challengeID);
        }

        private static bool MultiplayerMenu_IsChallengeUnlocked(On.Menu.MultiplayerMenu.orig_IsChallengeUnlocked orig, MultiplayerMenu self, PlayerProgression progression, int challengeNumber)
        {
            if (ChallengeData.currentChallenge is Structs.CustomChallenge challenge)
            {
                return (challenge.unlocked || progression.miscProgressionData.CustomSaveData().ChallengeTokenUnlocked(challenge.id))
                    && ChallengeRequirements.AllRequirementsFulfilled(challenge.UnlockRequirement[challenge.levels[challengeNumber - 1]], self.multiplayerUnlocks);
            }
            return orig(self, progression, challengeNumber);
        }

        private static void ChallengeMeta_ctor1(On.MoreSlugcats.ChallengeInformation.ChallengeMeta.orig_ctor orig, MoreSlugcats.ChallengeInformation.ChallengeMeta self, int challengeID)
        {
            if (ChallengeData.currentChallenge is Structs.CustomChallenge challenge)
            {
                self.ChallengeCustomData().Value = challenge;
            }
            orig(self, challengeID);
        }

        private static void ChallengeMeta_ctor(ILContext il)
        {
            var c = new ILCursor(il);

            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdstr("_Meta.txt"),
                x => x.MatchStelemRef(),
                x => x.MatchCall<string>(nameof(string.Concat))
                ))
            {
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((string orig, MoreSlugcats.ChallengeInformation.ChallengeMeta self) => { if (self.ChallengeCustomData().Value is Structs.CustomChallenge challenge && self.challengeNumber > 0) return $"Levels\\Challenges\\{challenge.levels[self.challengeNumber - 1]}_Meta.txt"; else return orig; });
            }
            else
            {
                CustomRegionsMod.BepLogError("failed to il hook ChallengeMeta.ctor");
            }
        }
    }
}
