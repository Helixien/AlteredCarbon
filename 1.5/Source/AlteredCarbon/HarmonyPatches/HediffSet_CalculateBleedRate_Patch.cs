using HarmonyLib;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(HediffSet), "CalculateBleedRate")]
    public static class HediffSet_CalculateBleedRate_Patch
    {
        private static void Postfix(HediffSet __instance, ref float __result)
        {
            if (__result > 0 && __instance.pawn.HasHediff(AC_DefOf.AC_CryptoStasis))
            {
                __result /= 10;
            }
        }
    }
}

