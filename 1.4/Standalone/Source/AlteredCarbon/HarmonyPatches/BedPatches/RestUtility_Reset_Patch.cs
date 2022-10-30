using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(RestUtility), "Reset")]
    public static class RestUtility_Reset_Patch
    {
        private static void Postfix(ref List<ThingDef> ___bedDefsBestToWorst_RestEffectiveness, ref List<ThingDef> ___bedDefsBestToWorst_Medical)
        {
            ___bedDefsBestToWorst_RestEffectiveness.Remove(AC_DefOf.VFEU_SleeveCasket);
            ___bedDefsBestToWorst_Medical.Remove(AC_DefOf.VFEU_SleeveCasket);
        }
    }

}

