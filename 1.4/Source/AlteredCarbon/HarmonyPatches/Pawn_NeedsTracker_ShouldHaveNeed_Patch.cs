using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Pawn_NeedsTracker), "ShouldHaveNeed")]
    public static class Pawn_NeedsTracker_ShouldHaveNeed_Patch
    {
        public static HashSet<string> bodyNeeds = new HashSet<string>
        {
            "Food",
            "Bladder",
            "Hygiene",
            "DBHThirst",
        };
        public static bool Prefix(Pawn ___pawn, NeedDef nd)
        {
            if (___pawn.IsEmptySleeve() && bodyNeeds.Contains(nd.defName) is false)
            {
                return false;
            }
            return true;
        }
    }
}

