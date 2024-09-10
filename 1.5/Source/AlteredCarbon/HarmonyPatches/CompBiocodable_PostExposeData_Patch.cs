using HarmonyLib;
using RimWorld;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(CompBiocodable), "PostExposeData")]
    public static class CompBiocodable_PostExposeData_Patch
    {
        public static SaveDataHandler<CompBiocodable, bool> wasBiocoded = 
            new SaveDataHandler<CompBiocodable, bool>("wasBiocoded");
        public static void Postfix(CompBiocodable __instance)
        {
            wasBiocoded.ExposeData(__instance);
        }
    }
}

