using HarmonyLib;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Pawn), "Destroy")]
    public static class Pawn_Destroy_Patch
    {
        public static void Prefix(Pawn __instance)
        {
            if (__instance.Corpse is null && __instance.HasNeuralStack(out var stackHediff))
            {
                if (stackHediff.def == AC_DefOf.AC_ArchotechStack)
                {
                    stackHediff.preventKill = true;
                    stackHediff.SpawnStack(placeMode: ThingPlaceMode.Direct, psycastEffect: true);
                }
            }
        }

        //public static void Postfix(Pawn __instance)
        //{
        //    if (__instance.Destroyed && __instance.HasNeuralStack(out var stackHediff) && stackHediff.def != AC_DefOf.AC_ArchotechStack)
        //    {
        //        stackHediff.NeuralData.TryQueueAutoRestoration();
        //    }
        //}
    }
}

