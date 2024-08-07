using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon;

[HarmonyPatch(typeof(MemoryThoughtHandler), "TryGainMemory", typeof(Thought_Memory), typeof(Pawn))]
public static class MemoryThoughtHandler_TryGainMemory_Patch
{
    public static void Postfix(MemoryThoughtHandler __instance, ref Thought_Memory newThought)
    {
        if (__instance.pawn.health.hediffSet.HasHediff(AC_DefOf.AC_Dreamcatcher))
        {
            if (newThought.MoodOffset() < 0)
            {
                newThought.durationTicksOverride = (int)(newThought.DurationTicks * 0.75f);
            }
            else
            {
                newThought.durationTicksOverride = (int)(newThought.DurationTicks * 1.25f);
            }
        }
    }
}