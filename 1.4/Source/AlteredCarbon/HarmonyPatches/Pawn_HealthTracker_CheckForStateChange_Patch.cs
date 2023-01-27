using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{

    [HarmonyPatch(typeof(Pawn_HealthTracker), "CheckForStateChange", null)]
    public static class Pawn_HealthTracker_CheckForStateChange_Patch
    {
        private static void Postfix(Pawn_HealthTracker __instance, Pawn ___pawn, DamageInfo? dinfo, Hediff hediff)
        {
            if (!___pawn.health.hediffSet.GetNotMissingParts().Any(x => x.def == BodyPartDefOf.Neck) && ___pawn.HasCorticalStack(out var stackHediff))
            {
                if (Rand.Chance(0.25f))
                {
                    StatsRecord_Notify_ColonistKilled_Patch.disableKilledEffect = true;
                    stackHediff.SpawnStack();
                }
                if (!___pawn.Dead)
                {
                    ___pawn.Kill(null);
                }
            }
        }
    }
}

