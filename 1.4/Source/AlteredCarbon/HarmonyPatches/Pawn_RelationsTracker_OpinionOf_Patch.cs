using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Pawn_RelationsTracker), "OpinionOf")]
    public static class Pawn_RelationsTracker_OpinionOf_Patch
    {
        public static bool Prefix(Pawn other)
        {
            if (other.IsEmptySleeve())
            {
                return false;
            }
            return true;
        }
    }
}

