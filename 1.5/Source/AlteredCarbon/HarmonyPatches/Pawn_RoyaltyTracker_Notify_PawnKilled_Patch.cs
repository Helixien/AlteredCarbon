using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Pawn_RoyaltyTracker), "Notify_PawnKilled")]
    public static class Pawn_RoyaltyTracker_Notify_PawnKilled_Patch
    {
        public static Pawn disableKillEffect;
        public static bool Prefix(Pawn_RoyaltyTracker __instance)
        {
            if (__instance.pawn == disableKillEffect)
            {
                return false;
            }
            return true;
        }
    }
}

