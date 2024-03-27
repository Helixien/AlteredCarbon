using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(RitualObligationTrigger_MemberDied), "Notify_MemberDied")]
    public static class RitualObligationTrigger_MemberDied_Notify_MemberDied_Patch
    {
        public static bool Prefix(Pawn p)
        {
            if (Pawn_HealthTracker_NotifyPlayerOfKilled_Patch.ShouldSkip(p))
            {
                return false;
            }
            return true;
        }
    }
}

