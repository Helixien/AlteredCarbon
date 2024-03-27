using HarmonyLib;
using RimWorld;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Pawn_GuestTracker), "Notify_PawnUndowned")]
    public static class Pawn_GuestTracker_Notify_PawnUndowned_Patch
    {
        public static bool Prefix(Pawn_GuestTracker __instance)
        {
            if (__instance.pawn.IsEmptySleeve())
            {
                return false;
            }
            return true;
        }
    }
}

