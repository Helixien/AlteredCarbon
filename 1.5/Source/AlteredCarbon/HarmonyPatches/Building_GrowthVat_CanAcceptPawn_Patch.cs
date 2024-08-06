using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Building_GrowthVat), "CanAcceptPawn")]
    internal static class Building_GrowthVat_CanAcceptPawn_Patch
    {
        public static void Postfix(Pawn pawn, ref AcceptanceReport __result)
        {
            if (pawn.IsEmptySleeve())
            {
                __result = "AC.IsEmptySleeve".Translate();
            }
        }
    }
}
