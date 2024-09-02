using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Fire), "DoFireDamage")]
    internal static class Fire_DoFireDamage_Patch
    {
        public static void Prefix(Fire __instance, Thing targ)
        {
            if (targ is Corpse corpse && targ.HitPoints <= 3 && corpse.InnerPawn.HasNeuralStack(out var hediff))
            {
                hediff.SpawnStack(placeMode: ThingPlaceMode.Direct);
                __instance.Destroy(DestroyMode.Vanish);
            }
            else if (targ is Pawn pawn && pawn.health.summaryHealth.SummaryHealthPercent < 0.001f
                && pawn.HasNeuralStack(out var hediff2))
            {
                hediff2.SpawnStack(placeMode: ThingPlaceMode.Direct);
                __instance.Destroy(DestroyMode.Vanish);
            }
        }
    }
}
