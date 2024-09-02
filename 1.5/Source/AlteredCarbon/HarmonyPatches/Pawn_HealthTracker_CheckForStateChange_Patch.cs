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
            if (___pawn.GetNeck() is null && ___pawn.HasNeuralStack(out var stackHediff))
            {
                if (Rand.Chance(0.25f))
                {
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

