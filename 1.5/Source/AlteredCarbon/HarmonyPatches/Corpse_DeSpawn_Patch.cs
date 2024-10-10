using HarmonyLib;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Corpse), "DeSpawn")]
    public static class Corpse_DeSpawn_Patch
    {
        public static void Prefix(Corpse __instance)
        {
            if (__instance.InnerPawn.HasNeuralStack(out var stackHediff) && Pawn_Kill_Patch.pawnWithStackBeingKilled == __instance.InnerPawn)
            {
                if (stackHediff.def == AC_DefOf.AC_ArchotechStack)
                {
                    stackHediff.SpawnStack(placeMode: ThingPlaceMode.Direct, psycastEffect: true);
                }
            }
        }
    }
}

