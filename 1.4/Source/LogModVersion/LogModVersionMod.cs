using HarmonyLib;
using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace LogModVersion
{
    public class LogModVersion : Mod
    {
        public LogModVersion(ModContentPack content) : base(content)
        {
            new Harmony("LogModVersion").PatchAll();
        }
    }

    [HarmonyPatch(typeof(LoadedModManager), nameof(LoadedModManager.LoadModXML))]
    public static class LoadedModManager_LoadModXML_Patch
    {
        public static void Prefix()
        {
            foreach (var mod in LoadedModManager.RunningMods)
            {
                if (mod.assemblies.loadedAssemblies.Contains(typeof(LogModVersion).Assembly))
                {
                    Log.Message("[" + mod.Name + "] version " + mod.ModMetaData.ModVersion);
                }
            }
        }
    }
}
