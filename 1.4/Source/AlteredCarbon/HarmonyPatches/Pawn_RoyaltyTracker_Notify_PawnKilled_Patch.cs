using HarmonyLib;
using RimWorld;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Pawn_RoyaltyTracker), "Notify_PawnKilled")]
    public static class Pawn_RoyaltyTracker_Notify_PawnKilled_Patch
    {
        public static bool disableKilledEffect = false;
        public static bool Prefix()
        {
            if (disableKilledEffect)
            {
                disableKilledEffect = false;
                return false;
            }
            return true;
        }
    }
}

