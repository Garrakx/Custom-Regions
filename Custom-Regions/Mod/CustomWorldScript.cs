using RWCustom;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CustomRegions.Mod
{
    public class CustomWorldScript : MonoBehaviour
    {

        public void Initialize()
        {
            CustomWorldMod.script = this;

            CustomWorldMod.CreateCustomWorldLog();

            MapHook.ApplyHook();
            RegionGateHook.ApplyHooks();
            RegionHook.ApplyHook();
            RoomSettingsHook.ApplyHook();
            WorldHook.ApplyHook();
            WorldLoaderHook.ApplyHooks();
            OverWorldHook.ApplyHooks();
            PlayerProgressionHook.ApplyHooks();
        }

        public RainWorld rw;
        public static CustomWorldMod mod;
        public static ProcessManager pm;

        public void Update()
        {
            if (rw == null)
            {
                rw = FindObjectOfType<RainWorld>();
                pm = rw.processManager;
            }
        }

    }

}
