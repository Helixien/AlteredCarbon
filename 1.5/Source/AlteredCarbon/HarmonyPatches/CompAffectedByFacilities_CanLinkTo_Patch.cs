using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(CompAffectedByFacilities), "CanLinkTo")]
    public static class CompAffectedByFacilities_CanLinkTo_Patch
    {
        public static bool Prefix(CompAffectedByFacilities __instance, Thing facility, ref bool __result)
        {
            if (__instance.parent.def == AC_DefOf.AC_CastingRelay)
            {
                int relayCount = facility.TryGetComp<CompFacility>().LinkedBuildings
                    .Count(linkedFacility => linkedFacility.def == AC_DefOf.AC_CastingRelay);
                if (relayCount >= 4)
                {
                    __result = false;
                    return false;
                }
            }
            return true;
        }
    }
}
