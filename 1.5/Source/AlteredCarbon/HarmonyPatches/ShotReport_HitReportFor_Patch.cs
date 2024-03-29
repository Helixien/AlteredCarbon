using HarmonyLib;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(ShotReport), "HitReportFor")]
    public static class ShotReport_HitReportFor_Patch
    {
        [HarmonyPriority(int.MaxValue)]
        public static void Postfix(ref ShotReport __result, Thing caster, Verb verb, LocalTargetInfo target)
        {
            if (caster is Pawn pawn && pawn.Wears(AC_DefOf.AC_Apparel_FusilierHelmet))
            {
                if (__result.offsetFromDarkness < 0)
                {
                    __result.offsetFromDarkness = 0;
                    __result.factorFromCoveringGas = 1f;
                }
            }
        }
    }
}