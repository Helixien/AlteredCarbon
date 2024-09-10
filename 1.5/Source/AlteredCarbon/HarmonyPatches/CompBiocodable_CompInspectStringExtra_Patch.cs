using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(CompBiocodable), nameof(CompBiocodable.CompInspectStringExtra))]
    public static class CompBiocodable_CompInspectStringExtra_Patch
    {
        public static void Postfix(ref string __result, CompBiocodable __instance)
        {
            if (__instance.biocoded is false && CompBiocodable_PostExposeData_Patch.wasBiocoded.TryGet(__instance,
                out var wasBiocoded) && wasBiocoded)
            {
                __result = "AC.BiocodingNotYetImprinted".Translate();
            }
        }
    }
}

