using HarmonyLib;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Corpse), "Destroy")]
    public static class Corpse_Destroy_Patch
    {
        public static void Prefix(Corpse __instance)
        {
            if (__instance.InnerPawn.HasNeuralStack(out var stackHediff))
            {
                if (stackHediff.def == AC_DefOf.AC_ArchotechStack)
                {
                    stackHediff.SpawnStack(placeMode: ThingPlaceMode.Direct, psycastEffect: true);
                }
            }
        }
    }
}

