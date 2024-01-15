using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(RitualObligationTrigger_MemberCorpseDestroyed), "Notify_MemberCorpseDestroyed")]
    public static class RitualObligationTrigger_MemberCorpseDestroyed_Notify_MemberCorpseDestroyed_Patch
    {
        public static bool Prefix(RitualObligationTrigger_MemberCorpseDestroyed __instance, Pawn p)
        {
            if (p.IsEmptySleeve() || AlteredCarbonManager.Instance.StacksIndex.ContainsKey(p.thingIDNumber))
            {
                return false;
            }
            if (__instance.ritual != null)
            {
                __instance.ritual.completedObligations ??= new List<RitualObligation>();
                __instance.ritual.completedObligations.RemoveAll(x => x is null);
            }
            return true;
        }
    }
}

