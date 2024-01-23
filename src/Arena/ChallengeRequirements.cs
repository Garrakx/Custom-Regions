using CustomRegions.Mod;
using System;
using System.Collections.Generic;
using static CustomRegions.Arena.ChallengeData;

namespace CustomRegions.Arena
{
    internal class ChallengeRequirements
    {
        public static bool AllRequirementsFulfilled(string[] requirements, MultiplayerUnlocks self)
        {
            foreach (string req in requirements)
            { if (!ChallengeRequirementFulfilled(req, self)) return false; }

            return true;
        }

        private static bool ChallengeRequirementFulfilled(string input, MultiplayerUnlocks self)
        {
            string[] array = input.Split(new[] { ':' }, 2);
            if (array.Length < 2 || !Reqs.ContainsKey(array[0])) return false;

            return Reqs[array[0]](array[1], self);
        }


        public static string AllRequirementDescriptions(Structs.CustomChallenge challenge, int id, Menu.MultiplayerMenu self)
        {
            string result = "";

            var saveData = self.manager.rainWorld.progression.miscProgressionData.CustomSaveData();
            if (!saveData.ChallengeTokenUnlocked(challenge.id)) result += self.Translate("Collect a challenge token to unlock.");

            foreach (string req in challenge.UnlockRequirement[challenge.levels[id - 1]])
            {
                if (ChallengeRequirementFulfilled(req, self.multiplayerUnlocks)) continue;

                if (result != "") result += "\n";

                result += ChallengeRequirementDescription(req, self);
            }

            return result;
        }

        private static string ChallengeRequirementDescription(string input, Menu.Menu self)
        {
            CustomRegionsMod.CustomLog("challenge requirement check" + input);
            string[] array = input.Split(new[] { ':' }, 2);
            if (array.Length < 2 || !Descs.ContainsKey(array[0])) return "Challenge locked";

            return Descs[array[0]](array[1], self);
        }

        public delegate bool LevelUnlockReq(string input, MultiplayerUnlocks self);

        public delegate string LevelUnlockDescription(string input, Menu.Menu self);

        private static Dictionary<string, LevelUnlockReq> Reqs = new();

        private static Dictionary<string, LevelUnlockDescription> Descs = new();

        public static void RegisterRequirement(string name, LevelUnlockReq req, LevelUnlockDescription desc)
        {
            Reqs[name] = req;
            Descs[name] = desc;
        }

        public static void RegisterBuiltinRequirements()
        {
            RegisterRequirement("CharacterBeaten", CharacterBeaten, CharacterDescription);
            RegisterRequirement("CharacterUnlocked", ClassUnlocked, ClassDescription);
            RegisterRequirement("SafariUnlocked", SafariUnlocked, SafariDescription);
            RegisterRequirement("LevelUnlocked", LevelUnlocked, LevelDescription);
            RegisterRequirement("SandboxUnlocked", SandboxItemUnlocked, SandboxIitemDescription);
            RegisterRequirement("ChallengeBeaten", ChallengeBeaten, ChallengeBeatenDescription);
        }

        private static string SlugcatNameFix(string input)
        {
            return "The " + input switch
            {
                "White" => "Survivor",
                "Yellow" => "Monk",
                "Red" => "Hunter",
                "Spear" => "Spearmaster",
                _ => input
            };
        }

        private static bool CharacterBeaten(string input, MultiplayerUnlocks self)
        {
            return input switch
            {
                "Survivor" or "White" => self.progression.miscProgressionData.beaten_Survivor,
                "Monk" or "Yellow" => true,
                "Hunter" or "Red" => self.progression.miscProgressionData.redUnlocked,
                "Gourmand" => self.progression.miscProgressionData.beaten_Gourmand,
                "Artificer" => self.progression.miscProgressionData.beaten_Artificer,
                "Rivulet" => self.progression.miscProgressionData.beaten_Rivulet,
                "Spear" or "Spearmaster" => self.progression.miscProgressionData.beaten_SpearMaster,
                "Saint" => self.progression.miscProgressionData.beaten_Saint,
                _ => true,
            };
        }

        private static string SandboxName(string input, Menu.Menu self)
        {
            if (self.manager.rainWorld.inGameTranslator.HasShortstringTranslation("objecttype-" + input))
            { return "objecttype-" + input; }

            if (self.manager.rainWorld.inGameTranslator.HasShortstringTranslation("creaturetype-" + input))
            { return "creaturetype-" + input; }

            return input;
        }

        private static string CharacterDescription(string input, Menu.Menu self) => self.Translate("Clear the game as ## to unlock.").Replace("##", self.Translate(SlugcatNameFix(input)));

        private static bool ClassUnlocked(string input, MultiplayerUnlocks self) => new SlugcatStats.Name(input).index != -1 && self.ClassUnlocked(new(input));

        private static string ClassDescription(string input, Menu.Menu self) => self.Translate("Collect the character token for ## to unlock.").Replace("##", self.Translate(SlugcatNameFix(input)));

        private static bool SafariUnlocked(string input, MultiplayerUnlocks self) => self.progression.miscProgressionData.GetTokenCollected(new MultiplayerUnlocks.SafariUnlockID(input)) || MultiplayerUnlocks.CheckUnlockSafari();

        private static string SafariDescription(string input, Menu.Menu self) => self.Translate("Collect the safari token for ## to unlock.").Replace("##", self.Translate(Region.GetRegionFullName(input, null)));

        private static bool LevelUnlocked(string input, MultiplayerUnlocks self) => self.IsLevelUnlocked(input);

        private static string LevelDescription(string input, Menu.Menu self) => self.Translate("Collect the arena token for ## to unlock.").Replace("##", self.Translate(MultiplayerUnlocks.LevelDisplayName(input)));

        private static bool SandboxItemUnlocked(string input, MultiplayerUnlocks self) => self.SandboxItemUnlocked(new(input));

        private static string SandboxIitemDescription(string input, Menu.Menu self) => self.Translate("Collect the sandbox token for ## to unlock.").Replace("##", self.Translate(SandboxName(input, self)));

        private static bool ChallengeBeaten(string input, MultiplayerUnlocks self)
        {
            string[] array = input.Split(':');
            if (array.Length == 2 && new ChallengeUnlockID(array[0]).index != -1)
            {
                return ChallengeBeatenRange(array[1], self, new ChallengeUnlockID(array[0]));
            }
            return ChallengeBeatenRange(input, self);
        }

        private static bool ChallengeBeatenRange(string input, MultiplayerUnlocks self, ChallengeUnlockID challengeUnlockID = null)
        {
            PlayerProgression progression = self.progression;
            if (int.TryParse(input, out var num))
            {
                num--; //base one to base 0

                if (challengeUnlockID == null || challengeUnlockID.index == -1)
                    return progression.miscProgressionData.completedChallenges.Count > num && progression.miscProgressionData.completedChallenges[num];
                else
                    return progression.miscProgressionData.CustomSaveData().ChallengeComplete(challengeUnlockID, num);
            }
            string[] array = input.Split('-');
            if (array.Length == 2 && int.TryParse(array[0], out var start) && int.TryParse(array[1], out var end))
            {
                start--; //base one to base 0
                end--;

                for (int i = start; i < end; i++)
                {
                    if (challengeUnlockID == null || challengeUnlockID.index == -1)
                    {
                        if (progression.miscProgressionData.completedChallenges.Count < i || progression.miscProgressionData.completedChallenges[i])
                            return false;
                    }
                    else
                    {
                        if (progression.miscProgressionData.CustomSaveData().ChallengeComplete(challengeUnlockID, i))
                            return false;
                    }
                }
                return true;
            }
            return false;
        }
        private static string ChallengeBeatenDescription(string input, Menu.Menu self)
        {
            string[] array = input.Split(':');
            if (array.Length == 2 && new ChallengeUnlockID(array[0]).index != -1)
            {
                var data = new Structs.CustomChallenge
                { id = new ChallengeUnlockID(array[0]) };

                array[0] = data.LocalizedID(self);

                return self.Translate("Clear challenges ## to unlock.").Replace("##", array[0] + " " + array[1]);
            }

            return self.Translate("Clear challenges ## to unlock.").Replace("##", input);
        }

    }
}
