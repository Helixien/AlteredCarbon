using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Building_SubcoreScanner), "CanAcceptPawn")]
    internal static class Building_SubcoreScanner_CanAcceptPawn_Patch
    {
        public static void Postfix(Pawn selPawn, ref AcceptanceReport __result)
        {
            if (selPawn.IsEmptySleeve())
            {
                __result = "AC.IsEmptySleeve".Translate();
            }
        }
    }
}
