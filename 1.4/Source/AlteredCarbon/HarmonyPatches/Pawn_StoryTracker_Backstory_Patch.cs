using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch]
    public static class Pawn_StoryTracker_Backstory_Patch
    {
        [HarmonyTargetMethods]
        public static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.PropertySetter(typeof(Pawn_StoryTracker), nameof(Pawn_StoryTracker.Childhood));
            yield return AccessTools.PropertySetter(typeof(Pawn_StoryTracker), nameof(Pawn_StoryTracker.Adulthood));
        }

        public static void Postfix(Pawn_StoryTracker __instance, BackstoryDef value)
        {
            if (PawnGenerator.IsBeingGenerated(__instance.pawn))
            {
                LongEventHandler.toExecuteWhenFinished.Add(delegate
                {
                    var extension = value?.GetModExtension<StackSpawnModExtension>();
                    if (extension != null)
                    {
                        extension.TryAddStack(__instance.pawn);
                    }
                });
            }
        }
    }
}

