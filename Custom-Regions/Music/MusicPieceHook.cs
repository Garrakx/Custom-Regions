using CustomRegions.Mod;
using Music;
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
                        if (!self.piece.IsProcedural)
                        {
                            string subTrackPath = CRExtras.BuildPath(keyValues.Value, CRExtras.CustomFolder.Songs, file: self.trackName);
                            CustomWorldMod.Log($"[MusicPiece] Searching song subtrack at [{subTrackPath}]", false, CustomWorldMod.DebugLevel.FULL);
                            bool isMP3 = false;
                            if (File.Exists(subTrackPath + ".ogg") || (isMP3 = File.Exists(subTrackPath + ".mp3")))
                            {
                                string extension = isMP3 ? ".mp3" : ".ogg";
                                AudioType audioType = isMP3 ? AudioType.MPEG : AudioType.OGGVORBIS;

                                CustomWorldMod.Log($"[MusicPiece] Loaded track [{self.trackName}] from [{keyValues.Value}]. Extension [{audioType}]",
                                    false, CustomWorldMod.DebugLevel.MEDIUM);

                                WWW www = new WWW("file://" + subTrackPath + extension);
                                self.source.clip = www.GetAudioClip(false, true, audioType);
                                break;
                            }
                        }
                        else
                        {

                            string subTrackPath = CRExtras.BuildPath(keyValues.Value, CRExtras.CustomFolder.Procedural, file: self.trackName);
                            CustomWorldMod.Log($"[MusicPiece] Searching threat subtrack at [{subTrackPath}]", false, CustomWorldMod.DebugLevel.FULL);
                            bool isMP3 = false;
                            if (File.Exists(subTrackPath + ".ogg") || (isMP3 = File.Exists(subTrackPath + ".mp3")))
                            {
                                string extension = isMP3 ? ".mp3" : ".ogg";
                                AudioType audioType = isMP3 ? AudioType.MPEG : AudioType.OGGVORBIS;

                                CustomWorldMod.Log($"[MusicPiece] Loaded threat track [{self.trackName}] from [{keyValues.Value}] Extension [{audioType}]", 
                                    false, CustomWorldMod.DebugLevel.MEDIUM);

                                WWW www2 = new WWW("file://" + subTrackPath + extension);
                                self.source.clip = www2.GetAudioClip(false, true, audioType);
                                break;
                            }
    
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
                    else if (audioSource != null && self.subTracks[i].source.isPlaying && 
                        Math.Abs(audioSource.timeSamples - self.subTracks[i].source.timeSamples) >= audioSource.clip.frequency / 4)
                    {
                        self.subTracks[i].source.timeSamples = audioSource.timeSamples;
                    }
                }
            }

            orig(self);

        }
    }
}
