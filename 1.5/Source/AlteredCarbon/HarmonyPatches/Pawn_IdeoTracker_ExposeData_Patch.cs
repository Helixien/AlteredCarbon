using HarmonyLib;
using RimWorld;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Pawn_IdeoTracker), "ExposeData")]
    public static class Pawn_IdeoTracker_ExposeData_Patch
    {
        public static void Prefix(Pawn_IdeoTracker __instance)
        {
            if (__instance.pawn.IsEmptySleeve())
            {
                __instance.ideo = new Ideo();
            }
        }
        public static void Postfix(Pawn_IdeoTracker __instance)
        {
            if (__instance.pawn.IsEmptySleeve())
            {
                __instance.ideo = null;
            }
        }
    }
}

