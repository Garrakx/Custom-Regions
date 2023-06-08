using CustomRegions.Mod;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using Newtonsoft.Json.Linq;
using BepInEx.Logging;

namespace CustomRegions.CustomMusic
{
    internal static class ProceduralMusicHooks
    {
        public static void ApplyHooks()
        {
            On.Music.ProceduralMusic.ctor += ProceduralMusic_ctor;
            IL.Music.ProceduralMusic.ProceduralMusicInstruction.ctor += ProceduralMusicInstruction_ctor;
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
                CustomRegionsMod.BepLogError("Failed to IL hook ProceduralMusicInstruction.ctor!");
            }
        }
    }
}
