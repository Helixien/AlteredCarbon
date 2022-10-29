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
            if (targ is Corpse corpse && targ.HitPoints <= 3 && (corpse.InnerPawn?.health?.hediffSet?.HasHediff(AC_DefOf.VFEU_CorticalStack) ?? true))
            {
                var corticalStack = ThingMaker.MakeThing(AC_DefOf.VFEU_FilledCorticalStack) as CorticalStack;
                corticalStack.PersonaData.CopyPawn(corpse.InnerPawn);
                GenPlace.TryPlaceThing(corticalStack, corpse.Position, corpse.Map, ThingPlaceMode.Direct);
                corpse.InnerPawn.health.hediffSet.hediffs.RemoveAll(x => x.def == AC_DefOf.VFEU_CorticalStack);
                __instance.Destroy(DestroyMode.Vanish);
            }
            else if (targ is Pawn pawn && pawn.health.summaryHealth.SummaryHealthPercent < 0.001f
                && (pawn.health?.hediffSet?.HasHediff(AC_DefOf.VFEU_CorticalStack) ?? true))
            {
                var corticalStack = ThingMaker.MakeThing(AC_DefOf.VFEU_FilledCorticalStack) as CorticalStack;
                corticalStack.PersonaData.CopyPawn(pawn);
                GenPlace.TryPlaceThing(corticalStack, pawn.Position, pawn.Map, ThingPlaceMode.Direct);
                pawn.health.hediffSet.hediffs.RemoveAll(x => x.def == AC_DefOf.VFEU_CorticalStack);
                __instance.Destroy(DestroyMode.Vanish);
            }
        }
    }
}
