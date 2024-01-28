using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomRegions.Mod
{
    internal static class ModPriorities
    {
        private static ConditionalWeakTable<ModManager.Mod, StrongBox<string[]>> _Priorities = new();

        public static StrongBox<string[]> Priorities(this ModManager.Mod mod) => _Priorities.GetValue(mod, _ => new(new string[0]));

        public static void ApplyHooks()
        {
            On.ModManager.LoadModFromJson += ModManager_LoadModFromJson;
            IL.Menu.Remix.MenuModList._CheckRequirementsOrder += MenuModList__CheckRequirementsOrder1;
        }

        private static void MenuModList__CheckRequirementsOrder1(ILContext il)
        {
            int counter = 0;
            var c = new ILCursor(il);
            while (c.TryGotoNext(MoveType.Before, x => x.MatchLdfld<ModManager.Mod>("requirements")))
            {
                counter++;
                c.MoveAfterLabels();
                c.EmitDelegate((ModManager.Mod mod) => mod.requirements.Union(mod.Priorities().Value).ToArray());
                c.Remove();
            }

            if (counter < 2)
            { CustomRegionsMod.BepLog($"IL hook for MenuModList.CheckRequirementsOrder failed! only hooked [{counter}] times"); }
            
        }

        private static ModManager.Mod ModManager_LoadModFromJson(On.ModManager.orig_LoadModFromJson orig, RainWorld rainWorld, string modpath, string consolepath)
        {
            ModManager.Mod mod = orig(rainWorld, modpath, consolepath);

            if (mod != null && File.Exists(modpath + Path.DirectorySeparatorChar.ToString() + "modinfo.json"))
            {
                Dictionary<string, object> dictionary = File.ReadAllText(modpath + Path.DirectorySeparatorChar.ToString() + "modinfo.json").dictionaryFromJson();

                if (dictionary.ContainsKey("priorities"))
                { mod.Priorities().Value = ((List<object>)dictionary["priorities"]).ConvertAll((object x) => x.ToString()).ToArray(); }
            }
            return mod;
        }
    }
}
