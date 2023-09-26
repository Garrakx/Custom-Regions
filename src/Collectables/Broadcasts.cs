using CustomRegions.Mod;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using static MoreSlugcats.ChatlogData;

namespace CustomRegions.Collectables
{
    internal static class Broadcasts
    {
        public static void ApplyHooks()
        {
            On.MoreSlugcats.ChatlogData.HasUnique += ChatlogData_HasUnique;
            On.MoreSlugcats.ChatlogData.UniquePath += ChatlogData_UniquePath;
            On.MoreSlugcats.ChatlogData.getChatlog_ChatlogID += ChatlogData_getChatlog_ChatlogID;
            On.Player.ProcessChatLog += Player_ProcessChatLog;

            IL.DeathPersistentSaveData.FromString += DeathPersistentSaveData_FromString;
            IL.Room.Loaded += Room_Loaded;
        }

        #region savedata
        private static void Room_Loaded(ILContext il)
        {
            var c = new ILCursor(il);
            int num = 31;
            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdfld<DeathPersistentSaveData>(nameof(DeathPersistentSaveData.chatlogsRead)),
                x => x.MatchLdloc(out num),
                x => x.MatchCallvirt(typeof(List<ChatlogID>).GetMethod(nameof(List<ChatlogID>.Contains)))
                ))
            {
                c.Emit(OpCodes.Ldloc, num);
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((bool result, ChatlogID chat, Room self) =>
                {
                    if (!IsCustomChatlog(chat, out var broadcast) || broadcast.IsSingle) return result;
                    int num = 0;
                    foreach (BroadcastSaveData data in self.game.GetStorySession.saveState.deathPersistentSaveData.CustomBroadcastData())
                    {
                        //CustomRegionsMod.CustomLog($"comparing data [{data}] to [{chat}, {self.abstractRoom.name}]");
                        if (data.room == self.abstractRoom.name && data.id == chat) return true;

                        if (data.id == chat) num++;
                    }
                    //CustomRegionsMod.CustomLog($"num [{num}], total length [{broadcast.TotalLength}]");
                    return num >= broadcast.TotalLength;
                });
            }
            else
            {
                CustomRegionsMod.BepLogError("failed to il hook Room.Loaded!");
            }
        }

        private static void DeathPersistentSaveData_FromString(ILContext il)
        {
            var c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<DeathPersistentSaveData>(nameof(DeathPersistentSaveData.chatlogsRead)),
                x => x.MatchCallvirt(typeof(List<ChatlogID>).GetMethod(nameof(List<ChatlogID>.Clear)))
                ))
            {
                c.Emit(OpCodes.Ldloc, 4);
                c.EmitDelegate((string[] array) => { return array[1].Split(','); });
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((string[] broadcasts, DeathPersistentSaveData self) => {
                    {
                        self.CustomBroadcastData().Clear();

                        foreach (string text in broadcasts)
                        if (BroadcastSaveData.TryParse(text, out var data))
                        {
                            self.CustomBroadcastData().Add(data);
                            self.chatlogsRead.Add(new(data.ToString(), false));
                        }
                    }
                });
            }
            else
            {
                CustomRegionsMod.BepLogError("failed to il hook DeathPersistentSaveData.FromString!");
            }

            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<DeathPersistentSaveData>(nameof(DeathPersistentSaveData.prePebChatlogsRead)),
                x => x.MatchCallvirt(typeof(List<ChatlogID>).GetMethod(nameof(List<ChatlogID>.Clear)))
                ))
            {
                c.Emit(OpCodes.Ldloc, 4);
                c.EmitDelegate((string[] array) => { return array[1].Split(','); });
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate((string[] broadcasts, DeathPersistentSaveData self) => {
                    foreach (string text in broadcasts)
                    {
                        //self.CustomBroadcastData().Clear();

                        if (BroadcastSaveData.TryParse(text, out var data))
                        {
                            /*if (!self.CustomBroadcastData().Contains(data))
                            { self.CustomBroadcastData().Add(data); }*/
                            self.prePebChatlogsRead.Add(new(data.ToString(), false));
                        }
                    }
                });
            }
            else
            {
                CustomRegionsMod.BepLogError("failed to il hook DeathPersistentSaveData.FromString!");
            }
        }

        private static ConditionalWeakTable<DeathPersistentSaveData, List<BroadcastSaveData>> _CustomBroadcastData = new();

        public static List<BroadcastSaveData> CustomBroadcastData(this DeathPersistentSaveData d) => _CustomBroadcastData.GetValue(d, _ => new());

        public class BroadcastSaveData
        {
            public BroadcastSaveData(string room, string chatName, ChatlogID id)
            {
                this.room = room;
                this.chatName = chatName;
                this.id = id;
            }

            public static bool TryParse(string s, out BroadcastSaveData data)
            {
                data = null;

                if (string.IsNullOrEmpty(s)) return false;

                string[] array = s.Split('~');

                if (array.Length != 4 || array[0] != "CRSData") return false;

                data = new(array[1], array[2], new ChatlogID(array[3], false));
                return true;
            }

            public override string ToString()
            {
                return $"CRSData~{room}~{chatName}~{id}";
            }

            public string room;
            public string chatName;
            public ChatlogID id;
        }

        #endregion

        #region processing
        private static void Player_ProcessChatLog(On.Player.orig_ProcessChatLog orig, Player self)
        {
            if (self.chatlogCounter == 59 && self.room.game.cameras[0].hud.chatLog == null && IsCustomChatlog(self.chatlogID, out _) && !HasUnique(self.chatlogID))
            {
                CustomRegionsMod.CustomLog($"\n~~~Init Custom Chatlog [{self.chatlogID}]~~~");
                self.room.game.cameras[0].hud.InitChatLog(getChatlog(self.chatlogID));
            }

            orig(self);

            if (self.room.game.cameras[0].hud.chatLog == null && self.chatlogCounter >= 60 && IsCustomChatlog(self.chatlogID, out _) && !HasUnique(self.chatlogID))
            {
                DeathPersistentSaveData deathPersistentSaveData = self.room.game.GetStorySession.saveState.deathPersistentSaveData;
                deathPersistentSaveData.chatlogsRead.Remove(self.chatlogID);
                deathPersistentSaveData.prePebChatlogsRead.Remove(self.chatlogID);

                ChatlogID saveData = null;
                foreach (BroadcastSaveData d in deathPersistentSaveData.CustomBroadcastData())
                {
                    if (d.room == self.room.abstractRoom.name && d.id == self.chatlogID) saveData = new ChatlogID(d.ToString(), false);
                }
                if (saveData != null && !deathPersistentSaveData.chatlogsRead.Contains(saveData))
                {
                    deathPersistentSaveData.chatlogsRead.Add(saveData);
                }
                if (self.room.game.GetStorySession.saveState.miscWorldSaveData.SSaiConversationsHad == 0 && !deathPersistentSaveData.prePebChatlogsRead.Contains(saveData))
                {
                    deathPersistentSaveData.prePebChatlogsRead.Add(saveData);
                }
            }
        }

        private static string[] ChatlogData_getChatlog_ChatlogID(On.MoreSlugcats.ChatlogData.orig_getChatlog_ChatlogID orig, ChatlogID id)
        {
            if (!IsCustomChatlog(id, out var broadcast)) return orig(id);

            int progression = 0;

            if (!broadcast.IsSingle)
            {
                foreach (BroadcastSaveData data in myPlayer.room.game.GetStorySession.saveState.deathPersistentSaveData.CustomBroadcastData())
                { if (data.id == broadcast.id) progression++; }
                CustomRegionsMod.CustomLog($"chatlogID has been read [{progression}] times");
            }
            string fileName = GetCustomChatPath(broadcast, progression);
            CustomRegionsMod.CustomLog($"file to be read is [{fileName}]");

            if (fileName != null) {
                myPlayer.room.game.GetStorySession.saveState.deathPersistentSaveData.CustomBroadcastData().Add(new(myPlayer.room.abstractRoom.name, fileName, id));
                return getChatlog(new ChatlogID(fileName, false)); 
            }
            else return new string[] { "UNABLE TO ESTABLISH COMMUNICATION" };
        }


        public static string GetCustomChatPath(Structs.CustomBroadcast broadcast, int progression)
        {
            if (broadcast.IsSingle)
            {
                return broadcast.files[0].Value[0];
            }

            int index = 0, pos = 0, i = 0;
            while (i < 1000)
            {
                if (broadcast.files.Count <= index) break;
                if (broadcast.files[index].Value.Count <= pos)
                {
                    index++;
                    pos = 0;
                    continue;
                }

                if (i == progression)
                {
                    if (broadcast.files[index].Key == Structs.CustomBroadcast.Type.Random)
                    {
                        List<string> used = broadcast.files[index].Value.ToList();
                        foreach (BroadcastSaveData data in myPlayer.room.game.GetStorySession.saveState.deathPersistentSaveData.CustomBroadcastData())
                        {
                            if (data.id == broadcast.id)
                            { used.Remove(data.chatName); }
                        }
                        CustomRegionsMod.CustomLog($"Choosing random, possible files [{string.Join(", ", used)}]");
                        if (used.Count > 0) { return used[UnityEngine.Random.Range(0, used.Count)]; }
                        else return null;
                    }

                    else if (broadcast.files[index].Key == Structs.CustomBroadcast.Type.Sequence)
                    { return broadcast.files[index].Value[pos]; }

                    else if (broadcast.files[index].Key == Structs.CustomBroadcast.Type.Single) //this shouldn't be possible lol
                    { return broadcast.files[index].Value[0]; }

                    else return null;
                }

                pos++;
                i++;
            }
            return null;
        }
        private static string ChatlogData_UniquePath(On.MoreSlugcats.ChatlogData.orig_UniquePath orig, ChatlogID id)
        {
            if (!IsCustomChatlog(id, out var broadcast) || broadcast.IsSingle) return orig(id);

            string path = AssetManager.ResolveFilePath(rainWorld.inGameTranslator.SpecificTextFolderDirectory() + Path.DirectorySeparatorChar + CustomBroadcasts[id].files[0].Value[0] + ".txt");
            if (File.Exists(path)) return path;
            else
                return AssetManager.ResolveFilePath(rainWorld.inGameTranslator.SpecificTextFolderDirectory(InGameTranslator.LanguageID.English)
                    + Path.DirectorySeparatorChar + broadcast.files[0].Value[0] + ".txt");
        }

        private static bool ChatlogData_HasUnique(On.MoreSlugcats.ChatlogData.orig_HasUnique orig, ChatlogID id)
        {
            if (!IsCustomChatlog(id, out var broadcast)) return orig(id);
            else return broadcast.IsSingle;
        }
        #endregion

        #region registry
        public static void Refresh()
        {
            UnregisterBroadcasts();
            RegisterBroadcasts();
        }

        public static void RegisterBroadcasts()
        {
            CustomRegionsMod.CustomLog("\nRegistering Broadcasts!");
            string customFilePath = AssetManager.ResolveFilePath("CustomBroadcasts.txt");
            if (!File.Exists(customFilePath)) return;
            foreach (string str in File.ReadAllLines(customFilePath))
            {
                string[] array = Regex.Split(str, " : ");
                if (array.Length < 2 || string.IsNullOrEmpty(array[0]) || string.IsNullOrEmpty(array[1])) continue;

                CustomRegionsMod.CustomLog($"New ChatlogID, [{array[0]}]");
                ChatlogID id = new(array[0], true);
                string[] array2 = Regex.Split( array[1], " > ");

                List<KeyValuePair<Structs.CustomBroadcast.Type, List<string>>> files = new();

                foreach (string block in array2)
                {
                    List<string> array3 = Regex.Split(block, ", ").ToList();
                    if (array3.Count > 1)
                    {
                        CustomRegionsMod.CustomLog($"Section is random, includes elements [{string.Join(", ", array3)}]");
                        files.Add(new(Structs.CustomBroadcast.Type.Random, array3));
                    }

                    else if (array2.Length > 1)
                        files.Add(new(Structs.CustomBroadcast.Type.Sequence, array3));

                    else
                        files.Add(new(Structs.CustomBroadcast.Type.Single, array3));
                }

                CustomBroadcasts[id] = new Structs.CustomBroadcast(id, files);
            }
        }

        public static void UnregisterBroadcasts() 
        {
            foreach (ChatlogID id in CustomBroadcasts.Keys) id.Unregister();
            CustomBroadcasts = new();
        }

        public static bool IsCustomChatlog(ChatlogID id, out Structs.CustomBroadcast r) => CustomBroadcasts.TryGetValue(id, out r);

        static Dictionary<ChatlogID, Structs.CustomBroadcast> CustomBroadcasts = new();
        #endregion
    }
}
