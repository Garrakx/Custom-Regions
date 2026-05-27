using CustomRegions.Mod;
using System.Collections.Generic;
using System.IO;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using UnityEngine;
using System.Linq;

namespace CustomRegions.CustomMusic
{
    internal static class ProceduralMusicHooks
    {
        public static void ApplyHooks()
        {
            On.Music.ProceduralMusic.ctor += ProceduralMusic_ctor;
            On.Music.ProceduralMusic.ProceduralMusicInstruction.ctor += ProceduralMusicInstruction_ctor1;
            IL.Music.ProceduralMusic.ProceduralMusicInstruction.ctor += ProceduralMusicInstruction_ctor;
            IL.Music.MusicPiece.SubTrack.Update += SubTrack_Update;
        }

        private static void ProceduralMusicInstruction_ctor1(On.Music.ProceduralMusic.ProceduralMusicInstruction.orig_ctor orig, Music.ProceduralMusic.ProceduralMusicInstruction self, string name)
        {
            orig(self, name);
            foreach (var track in self.tracks)
            {
                if (track.subRegions == null) continue;
                List<string> subRegions = track.subRegions.ToList();
                for (int i = subRegions.Count - 1; i >= 0; i--)
                {
                    if (subRegions[i] == "D")
                    {
                        track.dayNight = 1;
                        subRegions.RemoveAt(i);
                    }

                    else if (subRegions[i] == "N")
                    {
                        track.dayNight = 2;
                        subRegions.RemoveAt(i);
                    }

                    else if (subRegions[i] == "M")
                    {
                        track.mushroom = true;
                        subRegions.RemoveAt(i);
                    }

                    else if (subRegions[i].Contains("%2"))
                    {  subRegions[i] = subRegions[i].Replace("%2", ","); }
                }

                track.subRegions = subRegions.ToArray();
            }
        }

        private static void SubTrack_Update(ILContext il)
        {
            var c = new ILCursor(il);
            while (c.TryGotoNext(MoveType.AfterLabel,
                x => x.MatchCall<AssetManager>(nameof(AssetManager.SafeWWWAudioClip))
                ))
            {
                c.Remove();
                c.EmitDelegate(AsyncLoad);
            }
        }

        public static AudioClip AsyncLoad(string path, bool threeD, bool stream, AudioType audioType)
        {
            if (RemixMenu.ThreatStream.Value)
            {
                WWW www = new WWW(path);
                return www.GetAudioClip(false, true, AudioType.OGGVORBIS);
            }
            else
            {
                return AssetManager.SafeWWWAudioClip(path, threeD, stream, audioType);
            }
        }

        private static void ProceduralMusic_ctor(On.Music.ProceduralMusic.orig_ctor orig, Music.ProceduralMusic self, Music.MusicPlayer musicPlayer, string name)
        {
            if (musicPlayer.manager.currentMainLoop is RainWorldGame game && game.StoryCharacter != null)
            {
                string slugName = game.StoryCharacter.ToString();
                if (File.Exists(AssetManager.ResolveFilePath(Path.Combine(new string[] { "Music", "Procedural", $"{name}-{slugName}.txt"}))))
                { 
                    name = $"{name}-{slugName}";
                    CustomRegionsMod.CustomLog($"custom slug threat [{name}]");
                }
            }
            orig(self, musicPlayer, name);
        }
        public static readonly string proceduralFolder = "Music" + Path.DirectorySeparatorChar + "Procedural" + Path.DirectorySeparatorChar;
        private static void ProceduralMusicInstruction_ctor(ILContext il)
        {
            int arrayIndex = 6;
            var c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.After,
                x => x.MatchLdloc(out _),
                x => x.MatchLdfld(typeof(AssetBundles.LoadedAssetBundle), "m_AssetBundle"),
                x => x.MatchLdloc(out arrayIndex),
                x => x.MatchLdcI4(0),
                x => x.MatchLdelemRef(),
                x => x.MatchCallvirt(typeof(UnityEngine.AssetBundle), "Contains")))
            {
                c.Emit(OpCodes.Ldloc, arrayIndex);
                c.EmitDelegate((bool flag, string[] array2) =>
                {
                    if (!flag && File.Exists(AssetManager.ResolveFilePath(proceduralFolder + array2[0] + ".ogg"))) { CustomRegionsMod.CustomLog($"adding track from ogg [{array2[0]}]", false, CustomRegionsMod.DebugLevel.FULL); }
                    return flag || File.Exists(AssetManager.ResolveFilePath(proceduralFolder + array2[0] + ".ogg"));
                });
            }
            else
            {
                CustomRegionsMod.BepLogError("CustomRegions.CustomMusic.ProceduralMusicHooks.ProceduralMusicInstruction_ctor: IL Hook failed.");
            }
        }
    }
}
