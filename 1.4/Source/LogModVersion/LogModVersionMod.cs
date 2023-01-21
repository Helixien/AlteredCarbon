using HarmonyLib;
using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace LogModVersion
{
    [StaticConstructorOnStartup]
    public static class LogModVersion
    {
        static LogModVersion()
        {
            foreach (var mod in LoadedModManager.RunningMods)
            {
                if (mod.assemblies.loadedAssemblies.Contains(typeof(LogModVersion).Assembly))
                {
                    Log.Message(mod.Name + " " + mod.ModMetaData.ModVersion);
                }
            }
        }
    }
}
