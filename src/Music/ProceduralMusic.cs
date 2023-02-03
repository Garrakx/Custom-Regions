using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;


namespace CustomRegions.CustomMusic
{
    internal static class ProceduralMusicHooks
    {
        public static void ApplyHooks()
        {
            On.Music.ProceduralMusic.ProceduralMusicInstruction.ctor += ProceduralMusicInstruction_ctor;
        }

        private static void ProceduralMusicInstruction_ctor(On.Music.ProceduralMusic.ProceduralMusicInstruction.orig_ctor orig, Music.ProceduralMusic.ProceduralMusicInstruction self, string name)
        {
            orig(self, name);

            if (self.tracks.Count >= 1) { return; }

            self.layers = new List<Music.ProceduralMusic.ProceduralMusicInstruction.Layer>();
            self.tracks = new List<Music.ProceduralMusic.ProceduralMusicInstruction.Track>();

            string proceduralFolder = "Music" + Path.DirectorySeparatorChar + "Procedural" + Path.DirectorySeparatorChar;
            string path = AssetManager.ResolveFilePath("Music" + Path.DirectorySeparatorChar.ToString() + "Procedural" + Path.DirectorySeparatorChar.ToString() + name + ".txt");

            if (!File.Exists(path)) { return; }


            foreach (string line in File.ReadAllLines(path)) {
                string[] array2 = Regex.Split(line, " : ");
                if (array2.Length != 0 && array2[0].Length > 4 && array2[0] == "Layer") {
                    self.layers.Add(new Music.ProceduralMusic.ProceduralMusicInstruction.Layer(self.layers.Count));

                    foreach (string str in Regex.Split(RWCustom.Custom.ValidateSpacedDelimiter(array2[1], ","), ", ")) {
                        if (str.Length == 0) { continue; }

                        foreach (Music.ProceduralMusic.ProceduralMusicInstruction.Track track in self.tracks) {
                            string text2 = "";
                            string a;
                            if (str.Length > 3 && str.Substring(0, 1) == "{" && str.Contains("}")) {
                                text2 = str.Substring(1, str.IndexOf("}") - 1);
                                a = str.Substring(str.IndexOf("}") + 1);
                            } else { a = str; }

                            if (a == track.name) {
                                string[] subRegions = null;
                                int dayNight = 0;
                                bool mushroom = false;

                                switch (text2) {
                                    case "":
                                        break;
                                    case "D":
                                        dayNight = 1;
                                        break;
                                    case "N":
                                        dayNight = 2;
                                        break;
                                    case "M":
                                        mushroom = true;
                                        break;
                                    default:
                                        subRegions = text2.Split(new char[] { '|' });
                                        break;
                                }
                                track.subRegions = subRegions;
                                track.dayNight = dayNight;
                                track.mushroom = mushroom;
                                self.layers[self.layers.Count - 1].tracks.Add(track);
                                break;
                            }
                        }
                    }


                } else if (array2.Length != 0 && array2[0].Length > 0 && File.Exists(AssetManager.ResolveFilePath(proceduralFolder + array2[0] + ".ogg"))) {
                    self.tracks.Add(new Music.ProceduralMusic.ProceduralMusicInstruction.Track(array2[0]));
                    string[] array4 = Regex.Split(array2[1], ", ");

                    foreach (string str in array4) {
                        if (str.Length == 0) { continue; }

                        if (str == "<PA>") { self.tracks[self.tracks.Count - 1].remainInPanicMode = true; } else { self.tracks[self.tracks.Count - 1].tags.Add(str); }
                    }
                }
            }
        }


    }
}
