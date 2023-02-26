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
            if (p is Pawn pawn && pawn.IsEmptySleeve())
            {
                return false;
            }
            Precept_Ritual precept_Ritual = (Precept_Ritual)(p.ideo?.Ideo?.GetPrecept(PreceptDefOf.Funeral));
            if (precept_Ritual != null)
            {
                precept_Ritual.completedObligations ??= new List<RitualObligation>();
                precept_Ritual.completedObligations.RemoveAll(x => x is null);
            }
            return true;
        }
    }
}

