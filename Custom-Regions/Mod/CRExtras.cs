using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Threading;
using Partiality.Modloader;
using System.IO;
using System.Security.Policy;
using RWCustom;
using System.Runtime.CompilerServices;

namespace CustomRegions.Mod
{
    // MOVE ALL THE EXTERNAL METHODS HERE
    static class CRExtras
    {
        /*
        string path = Custom.RootFolderDirectory() + CustomWorldMod.resourcePath +
    CustomWorldMod.availableRegions.ElementAt(i).Value.folderName + Path.DirectorySeparatorChar + "thumb.png";

                using (WWW www = new WWW(path))
                {
                    yield return www;
                    www.LoadImageIntoTexture(tex);
                }
*/
        static IEnumerator Start(string url, Texture2D tex)
        {
            tex = new Texture2D(160, 175, TextureFormat.DXT1, false);
            using (WWW www = new WWW(url))
            {
                yield return www;
                www.LoadImageIntoTexture(tex);
                //GetComponent<Renderer>().material.mainTexture = tex;
            }
        }
    }

    public class ThumbnailDownloader : MonoBehaviour
    {
        public static ThumbnailDownloader instance;

        int currentThumb;
        WWW www;
        bool next;
        string path;
        private List<string> regionFolders;
        public bool readyToDelete;
        private List<string> urls;
        //  string filePath = Custom.RootFolderDirectory() + CustomWorldMod.resourcePath + CustomWorldMod.availableRegions.ElementAt(i).Value.folderName + Path.DirectorySeparatorChar + "thumb.png";
        public void Init(Dictionary<string, string> thumbInfo)
        {
            if (thumbInfo == null || thumbInfo.Count < 1)
            {
                CustomWorldMod.Log("Error creating thumbnail downloader", true);
                this.readyToDelete = true;
                return;
            }

            currentThumb = 0;
            this.regionFolders = thumbInfo.Keys.ToList();
            this.urls = thumbInfo.Values.ToList();

            this.path = Custom.RootFolderDirectory() + CustomWorldMod.resourcePath + regionFolders[currentThumb] + Path.DirectorySeparatorChar + "thumb.png";
            this.www = new WWW(urls[currentThumb]);
            this.readyToDelete = false;
            this.next = false;

            //this.filename = filename;
        }

        public static void Create(Dictionary<string, string> thumbInfo)
        {
            GameObject gObject = new GameObject("Thumbdownloader");
            gObject.AddComponent<ThumbnailDownloader>();
            DontDestroyOnLoad(gObject);

            instance.Init(thumbInfo);
        }

        public void Awake()
        {
            instance = this;
        }

        public void Update()
        {

            if (urls == null || currentThumb >= this.urls.Count  || regionFolders == null || readyToDelete)
            {
                this.Clear();
                readyToDelete = true;
                return;
            }

            if (www == null || string.IsNullOrEmpty(www.error))
            {
                if (www.isDone && !next)
                {

                    CustomWorldMod.Log($"Dowloading thumb[{currentThumb}].. path [{path}]");
                    Texture2D tex;
                    tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
                    www.LoadImageIntoTexture(tex);
                    tex.Apply();
                    byte[] file = tex.EncodeToPNG();
                    File.WriteAllBytes(path, file);
                    CustomWorldMod.Log("Thumb downloaded " + path);


                    next = true;
                    currentThumb++;
                }
                else
                {
                    this.path = Custom.RootFolderDirectory() + CustomWorldMod.resourcePath + regionFolders[currentThumb] + Path.DirectorySeparatorChar + "thumb.png";
                    this.www = new WWW(urls[currentThumb]);
                    next = false;
                }
            }
            else
            {
                readyToDelete = true;
                CustomWorldMod.Log(www.error, true);
            }
        }

        public void Clear()
        {
            try
            {
                this.regionFolders.Clear();
            } catch (Exception) { }
            try
            {
                this.urls.Clear();
            }
            catch (Exception) { }
        }
        /*
        internal void Create()
        {
            GameObject gObject = new GameObject("Thumbdownloader");
            gObject.AddComponent<ThumbnailDownloader>();
            DontDestroyOnLoad(gObject);
        }
        */
    }


    // SOURCE: AUTOUPDATE
    // AUTHOR: BEE
    /*
    public class DownloadScript : MonoBehaviour
    {
        public PartialityMod mod;
        public string path;
        public WWW www;
        public bool ready;
        public bool done;
        public string filename;

        public void Initialize(AutoUpdateMod au, PartialityMod mod, string path, string url, string filename)
        {
            this.au = au;
            this.mod = mod;
            this.path = path + Path.DirectorySeparatorChar + filename;
            this.www = new WWW(url);
            this.ready = true;
            this.done = false;
            this.filename = filename;
        }

        public void Update()
        {
            if (this.ready && this.www.isDone)
            {

                //File.WriteAllBytes(this.path, this.www.bytes);
                if (CustomWorldMod.www.isDone && !CustomWorldMod.complete)
                {
                    Debug.Log("Dowloading thumb..");
                    Texture2D tex;
                    tex = new Texture2D(4, 4, TextureFormat.DXT1, false);
                    www.LoadImageIntoTexture(tex);
                    byte[] file = tex.EncodeToPNG();
                    File.WriteAllBytes(path, file);
                    Debug.Log("Thumb downloaded " + path);

                }

                lock (this.au.otherLockObj)
                {
                    if (File.Exists(this.path))
                    {
                        this.au.actuallyUpdated = true;
                    }
                    this.au.needUpdate.Remove(this.mod);
                    if (this.au.needUpdate.Count == 0)
                    {
                        Debug.Log("Calling Done");
                        this.au.Done();
                    }
                }
                this.ready = false;
            }
        }

    }
    */



    // Only works on ARGB32, RGB24 and Alpha8 textures that are marked readable

    // SOURCE: http://wiki.unity3d.com/index.php/TextureScale#Usage
    // AUTHOR: Eric Haines (Eric5h5)
    public class TextureScale
    {
        public class ThreadData
        {
            public int start;
            public int end;
            public ThreadData(int s, int e)
            {
                start = s;
                end = e;
            }
        }

        private static Color[] texColors;
        private static Color[] newColors;
        private static int w;
        private static float ratioX;
        private static float ratioY;
        private static int w2;
        private static int finishCount;
        private static Mutex mutex;

        public static void Point(Texture2D tex, int newWidth, int newHeight)
        {
            ThreadedScale(tex, newWidth, newHeight, false);
        }

        public static void Bilinear(Texture2D tex, int newWidth, int newHeight)
        {
            ThreadedScale(tex, newWidth, newHeight, true);
        }

        private static void ThreadedScale(Texture2D tex, int newWidth, int newHeight, bool useBilinear)
        {
            texColors = tex.GetPixels();
            newColors = new Color[newWidth * newHeight];
            if (useBilinear)
            {
                ratioX = 1.0f / ((float)newWidth / (tex.width - 1));
                ratioY = 1.0f / ((float)newHeight / (tex.height - 1));
            }
            else
            {
                ratioX = ((float)tex.width) / newWidth;
                ratioY = ((float)tex.height) / newHeight;
            }
            w = tex.width;
            w2 = newWidth;
            var cores = Mathf.Min(SystemInfo.processorCount, newHeight);
            var slice = newHeight / cores;

            finishCount = 0;
            if (mutex == null)
            {
                mutex = new Mutex(false);
            }
            if (cores > 1)
            {
                int i = 0;
                ThreadData threadData;
                for (i = 0; i < cores - 1; i++)
                {
                    threadData = new ThreadData(slice * i, slice * (i + 1));
                    ParameterizedThreadStart ts = useBilinear ? new ParameterizedThreadStart(BilinearScale) : new ParameterizedThreadStart(PointScale);
                    Thread thread = new Thread(ts);
                    thread.Start(threadData);
                }
                threadData = new ThreadData(slice * i, newHeight);
                if (useBilinear)
                {
                    BilinearScale(threadData);
                }
                else
                {
                    PointScale(threadData);
                }
                while (finishCount < cores)
                {
                    Thread.Sleep(1);
                }
            }
            else
            {
                ThreadData threadData = new ThreadData(0, newHeight);
                if (useBilinear)
                {
                    BilinearScale(threadData);
                }
                else
                {
                    PointScale(threadData);
                }
            }

            tex.Resize(newWidth, newHeight);
            tex.SetPixels(newColors);
            tex.Apply();

            texColors = null;
            newColors = null;
        }

        public static void BilinearScale(System.Object obj)
        {
            ThreadData threadData = (ThreadData)obj;
            for (var y = threadData.start; y < threadData.end; y++)
            {
                int yFloor = (int)Mathf.Floor(y * ratioY);
                var y1 = yFloor * w;
                var y2 = (yFloor + 1) * w;
                var yw = y * w2;

                for (var x = 0; x < w2; x++)
                {
                    int xFloor = (int)Mathf.Floor(x * ratioX);
                    var xLerp = x * ratioX - xFloor;
                    newColors[yw + x] = ColorLerpUnclamped(ColorLerpUnclamped(texColors[y1 + xFloor], texColors[y1 + xFloor + 1], xLerp),
                                                           ColorLerpUnclamped(texColors[y2 + xFloor], texColors[y2 + xFloor + 1], xLerp),
                                                           y * ratioY - yFloor);
                }
            }

            mutex.WaitOne();
            finishCount++;
            mutex.ReleaseMutex();
        }

        public static void PointScale(System.Object obj)
        {
            ThreadData threadData = (ThreadData)obj;
            for (var y = threadData.start; y < threadData.end; y++)
            {
                var thisY = (int)(ratioY * y) * w;
                var yw = y * w2;
                for (var x = 0; x < w2; x++)
                {
                    newColors[yw + x] = texColors[(int)(thisY + ratioX * x)];
                }
            }

            mutex.WaitOne();
            finishCount++;
            mutex.ReleaseMutex();
        }

        private static Color ColorLerpUnclamped(Color c1, Color c2, float value)
        {
            return new Color(c1.r + (c2.r - c1.r) * value,
                              c1.g + (c2.g - c1.g) * value,
                              c1.b + (c2.b - c1.b) * value,
                              c1.a + (c2.a - c1.a) * value);
        }
    }
}
