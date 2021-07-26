using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using CustomRegions.Mod;
using Music;
using RWCustom;

namespace CustomRegions.Music
{
    static class ProceduralMusicInstructionsHook
    {
        public static void ApplyHooks()
        {
			On.Music.ProceduralMusic.ProceduralMusicInstruction.ctor += ProceduralMusicInstruction_ctor;
        }

        public static void RemoveHooks()
        {
            On.Music.ProceduralMusic.ProceduralMusicInstruction.ctor -= ProceduralMusicInstruction_ctor;
        }

        private static void ProceduralMusicInstruction_ctor(On.Music.ProceduralMusic.ProceduralMusicInstruction.orig_ctor orig, ProceduralMusic.ProceduralMusicInstruction self, string name)
		{
            orig(self, name);

			foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
			{
				string pathToProceduralText = Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar;
				pathToProceduralText += "Assets" + Path.DirectorySeparatorChar + "Futile" +
					Path.DirectorySeparatorChar + "Resources" + Path.DirectorySeparatorChar + "Music" + Path.DirectorySeparatorChar + "Procedural" + Path.DirectorySeparatorChar;

                int numberOfTracksAdded = self.tracks.Count;
                if (File.Exists(pathToProceduralText + name + ".txt"))
                {
                    CustomWorldMod.Log($"[MUSIC] Adding procedural tracks from [{keyValues.Value}]");

                    string[] array = File.ReadAllLines(pathToProceduralText + name + ".txt");
                    for (int i = 0; i < array.Length; i++)
                    {
                        string[] array2 = Regex.Split(array[i], " : ");
                        if (array2.Length > 0 && array2[0].Length > 4 && array2[0] == "Layer")
                        {
                            self.layers.Add(new ProceduralMusic.ProceduralMusicInstruction.Layer(self.layers.Count));
                            string[] array3 = Regex.Split(array2[1], ", ");
                            for (int j = 0; j < array3.Length; j++)
                            {
                                if (array3[j].Length > 0)
                                {
                                    for (int k = 0; k < self.tracks.Count; k++)
                                    {
                                        if (array3[j] == self.tracks[k].name)
                                        {
                                            CustomWorldMod.Log($"Added layer [{array3[j]}]", false, CustomWorldMod.DebugLevel.FULL);
                                            self.layers[self.layers.Count - 1].tracks.Add(self.tracks[k]);
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        else if (array2.Length > 0 && array2[0].Length > 0 && File.Exists(pathToProceduralText + array2[0] + ".ogg"))
                        {
                            CustomWorldMod.Log($"Added track [{array2[0] + ".ogg"}]", false, CustomWorldMod.DebugLevel.FULL);
                            self.tracks.Add(new ProceduralMusic.ProceduralMusicInstruction.Track(array2[0]));
                            string[] array4 = Regex.Split(array2[1], ", ");
                            for (int l = 0; l < array4.Length; l++)
                            {
                                if (array4[l].Length > 0)
                                {
                                    if (array4[l] == "<PA>")
                                    {
                                        self.tracks[self.tracks.Count - 1].remainInPanicMode = true;
                                    }
                                    else
                                    {
                                        self.tracks[self.tracks.Count - 1].tags.Add(array4[l]);
                                    }
                                }
                            }
                        }
                    }
                    CustomWorldMod.Log($"Added [{self.tracks.Count - numberOfTracksAdded}] tracks");
                }
            }
		}

    }
}
