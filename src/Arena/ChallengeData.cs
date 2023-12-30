using CustomRegions.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using BepInEx;
using System.Runtime.CompilerServices;

namespace CustomRegions.Arena
{
    public static class ChallengeData
    {
        internal static Structs.CustomChallenge? currentChallenge;

        public static List<Structs.CustomChallenge> customChallenges = new();

        public static void Refresh()
        {
            UnregisterChallenges();
            RegisterChallenges();
            ChallengeRequirements.RegisterBuiltinRequirements();
        }

        public static void UnregisterChallenges()
        {
            foreach (Structs.CustomChallenge challenge in customChallenges)
            {
                challenge.id.Unregister();
            }
            customChallenges.Clear();
        }

        public static void RegisterChallenges()
        {
            string filePath = AssetManager.ResolveFilePath("CustomChallenges.txt");
            if (!File.Exists(filePath)) return;

            CustomRegionsMod.CustomLog("\nRegistering Custom Challenges");

            foreach (string line in File.ReadAllLines(filePath))
            {
                if (line.Equals(string.Empty))
                {
                    // Line empty, skip
                    continue;
                }

                if (Structs.CustomChallenge.TryParse(line, out var result))
                {
                    result.id = new ChallengeUnlockID(result.id.value, true);
                    customChallenges.Add(result);
                }
            }
        }

        public static void ApplyHooks()
        {
            On.PlayerProgression.MiscProgressionData.FromString += MiscProgressionData_FromString;
            On.PlayerProgression.MiscProgressionData.ToString += MiscProgressionData_ToString;
        }

        private static string MiscProgressionData_ToString(On.PlayerProgression.MiscProgressionData.orig_ToString orig, PlayerProgression.MiscProgressionData self)
        {
            return orig(self) + self.CustomSaveData().ToString();
        }

        private static void MiscProgressionData_FromString(On.PlayerProgression.MiscProgressionData.orig_FromString orig, PlayerProgression.MiscProgressionData self, string s)
        {
            orig(self, s);
            self.CustomSaveData().FromString(self.unrecognizedSaveStrings);
        }

        private static ConditionalWeakTable<PlayerProgression.MiscProgressionData, SaveData> _CustomSaveData = new();

        public static SaveData CustomSaveData(this PlayerProgression.MiscProgressionData p) => _CustomSaveData.GetValue(p, _ => new());

        public class SaveData
        {
            public List<ChallengeUnlockID> unlockedChallenges = new();

            public Dictionary<ChallengeUnlockID, List<bool>> completeChallenges = new();

            public bool ChallengeTokenUnlocked(ChallengeUnlockID id) => id != null && unlockedChallenges.Contains(id);

            public void ChallengeTokenCollected(ChallengeUnlockID id) => unlockedChallenges.Add(id);

            public bool ChallengeComplete(ChallengeUnlockID id, int num) => completeChallenges.ContainsKey(id) && completeChallenges[id] != null && completeChallenges[id].Count > num && completeChallenges[id][num];

            public override string ToString()
            {
                string text = "";
                if (unlockedChallenges.Count > 0)
                {
                    text += "CRSCHALLENGETOKENS<mpdB>";
                    text += string.Join(",", unlockedChallenges.Select(x => x.value));
                    text += "<mpdA>";
                }
                if (completeChallenges.Count > 0)
                {
                    text += "CRSCHALLENGECOMPLETE<mpdB>";
                    text += string.Join(",", completeChallenges.Where(x => x.Value.Count > 0).Select(x => x.Key.value + "|" + string.Concat(x.Value.Select(x => x ? "1" : "0"))));
                    text += "<mpdA>";
                }
                return text;
            }

            public void FromString(List<string> list)
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    string[] array = Regex.Split(list[i], "<mpdB>");
                    if (array.Length < 2 || array[0].IsNullOrWhiteSpace()) continue;

                    switch (array[0])
                    {
                        case "CRSCHALLENGETOKENS":
                            unlockedChallenges.Clear();
                            unlockedChallenges = Regex.Split(array[1], ",").Select(x => new ChallengeUnlockID(x, false)).ToList();
                            break;
                        case "CRSCHALLENGECOMPLETE":
                            completeChallenges.Clear();
                            foreach (string st in array[1].Split(','))
                            {
                                string[] array2 = st.Split('|');
                                if (array2.Length >= 2 && array2[1].Length > 0)
                                {
                                    completeChallenges[new ChallengeUnlockID(array2[0], false)] = array2[1].Select(x => x == '1').ToList();
                                }
                            }
                            break;
                        default:
                            continue;
                    }
                    list.RemoveAt(i);
                }
            }
        }

        public class ChallengeUnlockID : ExtEnum<ChallengeUnlockID>
        {
            public ChallengeUnlockID(string value, bool register = false) : base(value, register) { }
        }
    }
}
