using CustomRegions.Mod;
using Music;
using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CustomRegions.Music
{
    static class MusicPieceHook
    {
        public static void ApplyHooks()
        {
            On.Music.MusicPiece.SubTrack.Update += SubTrack_Update;

            // Only needed if you don't have CustomAssets
			On.Music.MusicPiece.Update += MusicPiece_Update;
        }

        // COULD LOOK FIRST FOR VANILLA TO BE A LITTLE MORE EFFICIENT
		private static void SubTrack_Update(On.Music.MusicPiece.SubTrack.orig_Update orig, MusicPiece.SubTrack self)
        {
            if (!self.readyToPlay)
			{
                foreach (KeyValuePair<string, string> keyValues in CustomWorldMod.activatedPacks)
                {
                    if (self.source.clip == null)
                    {
                        //string dataPath = Application.dataPath;
                        //string dataPath2 = Application.dataPath;

                        string dataPath = Custom.RootFolderDirectory() + CustomWorldMod.resourcePath + keyValues.Value + Path.DirectorySeparatorChar;

                        if (!self.piece.IsProcedural)
                        {
                            string text = dataPath.Substring(0, dataPath.LastIndexOf
                                (Path.DirectorySeparatorChar)) + Path.DirectorySeparatorChar
                                 + "Assets" + Path.DirectorySeparatorChar + "Futile" + Path.DirectorySeparatorChar
                                + "Resources" + Path.DirectorySeparatorChar + "Music" + Path.DirectorySeparatorChar + "Songs" + Path.DirectorySeparatorChar
                                + self.trackName + ".ogg";

                            CustomWorldMod.Log($"Subtrack-path [{text}]", false, CustomWorldMod.DebugLevel.FULL);
                            if (File.Exists(text))
                            {
                                CustomWorldMod.Log($"Loaded track [{self.trackName}] from [{keyValues.Value}]");
                                WWW www = new WWW("file://" + text);
                                self.source.clip = www.GetAudioClip(false, true, AudioType.OGGVORBIS);
                                break;
                            }
                                /*
                            else
                            {
                                self.source.clip = (Resources.Load("Music/Songs/" + self.trackName, typeof(AudioClip)) as AudioClip);
                            }
                            */
                        }
                        else
                        {
                            string text2 = dataPath.Substring(0, dataPath.LastIndexOf
                                (Path.DirectorySeparatorChar)) + Path.DirectorySeparatorChar 
                                + "Assets" + Path.DirectorySeparatorChar + "Futile" + Path.DirectorySeparatorChar
                                + "Resources" + Path.DirectorySeparatorChar + "Music" + Path.DirectorySeparatorChar + "Procedural" + Path.DirectorySeparatorChar
                                + self.trackName + ".ogg";

                            CustomWorldMod.Log($"Subtrack-path [{text2}]", false, CustomWorldMod.DebugLevel.FULL);
                            if (File.Exists(text2))
                            {
                                CustomWorldMod.Log($"Loaded procedural track [{self.trackName}] from [{keyValues.Value}]");
                                WWW www2 = new WWW("file://" + text2);
                                self.source.clip = www2.GetAudioClip(false, true, AudioType.OGGVORBIS);
                                    break;
                            }
                            /*
                            else
                            {
                                self.source.clip = (Resources.Load("Music/Procedural/" + self.trackName, typeof(AudioClip)) as AudioClip);
                            }
                            */
                        }
                    }
                    else if (!self.source.isPlaying && self.source.clip.isReadyToPlay)
                    {
                        self.readyToPlay = true;
                        break;
                    }
                }
            }

			orig(self);
			/*
			if (this.piece.startedPlaying)
			{
				this.source.volume = Mathf.Pow(this.volume * this.piece.volume * this.piece.musicPlayer.manager.rainWorld.options.musicVolume, this.piece.musicPlayer.manager.soundLoader.volumeExponent);
			}
			*/
            
		}

        private static void MusicPiece_Update(On.Music.MusicPiece.orig_Update orig, MusicPiece self)
        {
            AudioSource audioSource = null;
            for (int i = 0; i < self.subTracks.Count; i++)
            {
                //this.subTracks[i].Update();
                if (self.IsProcedural)
                {
                    if (audioSource == null && self.subTracks[i].source.isPlaying)
                    {
                        audioSource = self.subTracks[i].source;
                    }
                    else if (audioSource != null && self.subTracks[i].source.isPlaying && Math.Abs(audioSource.timeSamples - self.subTracks[i].source.timeSamples) >= audioSource.clip.frequency / 4)
                    {
                        self.subTracks[i].source.timeSamples = audioSource.timeSamples;
                    }
                }
            }

            orig(self);

        }
    }
}
