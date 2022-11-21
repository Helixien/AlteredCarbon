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
            if (targ is Corpse corpse && targ.HitPoints <= 3 && corpse.InnerPawn.HasCorticalStack(out var hediff))
            {
                var stackDef = hediff.PersonaData.sourceStack ?? AC_DefOf.VFEU_FilledCorticalStack;
                var corticalStack = ThingMaker.MakeThing(stackDef) as CorticalStack;
                corticalStack.PersonaData.CopyPawn(corpse.InnerPawn, stackDef);
                GenPlace.TryPlaceThing(corticalStack, corpse.Position, corpse.Map, ThingPlaceMode.Direct);
                corpse.InnerPawn.health.RemoveHediff(hediff);
                __instance.Destroy(DestroyMode.Vanish);
            }
            else if (targ is Pawn pawn && pawn.health.summaryHealth.SummaryHealthPercent < 0.001f
                && pawn.HasCorticalStack(out var hediff2))
            {
                var stackDef = hediff2.PersonaData.sourceStack ?? AC_DefOf.VFEU_FilledCorticalStack;
                var corticalStack = ThingMaker.MakeThing(stackDef) as CorticalStack;
                corticalStack.PersonaData.CopyPawn(pawn, stackDef);
                GenPlace.TryPlaceThing(corticalStack, pawn.Position, pawn.Map, ThingPlaceMode.Direct);
                pawn.health.RemoveHediff(hediff2);
                __instance.Destroy(DestroyMode.Vanish);
            }
        }
    }
}
