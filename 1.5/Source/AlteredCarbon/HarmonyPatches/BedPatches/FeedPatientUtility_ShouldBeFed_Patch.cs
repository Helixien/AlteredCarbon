using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(FeedPatientUtility), "ShouldBeFed")]
    public static class FeedPatientUtility_ShouldBeFed_Patch
    {
        public static void Postfix(ref bool __result, Pawn p)
        {
            if (p.IsEmptySleeve())
            {
                __result = false;
            }
        }
    }
}

