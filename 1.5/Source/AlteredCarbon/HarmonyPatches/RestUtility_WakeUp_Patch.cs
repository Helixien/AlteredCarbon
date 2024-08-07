using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon;

[HarmonyPatch(typeof(RestUtility), "WakeUp")]
public static class RestUtility_WakeUp_Patch
{
    public static void Postfix(Pawn p)
    {
        if (p.health.hediffSet.HasHediff(AC_DefOf.AC_Dreamcatcher))
        {
            p.needs.mood.thoughts.memories.TryGainMemory(ThoughtDefOf.SleepDisturbed, null, null);
        }
    }
}