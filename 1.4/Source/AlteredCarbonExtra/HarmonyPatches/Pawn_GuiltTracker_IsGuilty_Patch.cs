using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon;

//TODO: test to ensure works without ideology
[HarmonyPatch(typeof(Pawn_GuiltTracker), "get_IsGuilty")]
internal static class Pawn_GuiltTracker_IsGuilty_Patch
{
    private static bool Prefix(ref bool __result, Pawn_GuiltTracker __instance, Pawn ___pawn)
    {
        if (___pawn.Ideo != null &&
            ___pawn.Ideo.precepts.Exists(precept => precept.def == AC_DefOf.AC_Stacking_Despised)
            && ___pawn.HasStack()
           )
        {
            __result = true;
            return false;
        }

        return true;
    }
}