using System;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(PawnGenerator), "GeneratePawn", new Type[] { typeof(PawnGenerationRequest) })]
    public static class PawnGenerator_GeneratePawn_Patch
    {
        public static void Postfix(Pawn __result)
        {
            if (__result != null && __result.RaceProps.Humanlike && __result.kindDef.HasModExtension<StackSpawnModExtension>())
            {
                var extension = __result.kindDef.GetModExtension<StackSpawnModExtension>();
                if (extension.SpawnsWithStack && __result.HasCorticalStack(out _) is false
                    && Rand.Chance((float)extension.ChanceToSpawnWithStack / 100f))
                {
                    BodyPartRecord neckRecord = __result.GetNeck();
                    var hediff = HediffMaker.MakeHediff(AC_DefOf.VFEU_CorticalStack, __result, neckRecord) as Hediff_CorticalStack;
                    __result.health.AddHediff(hediff, neckRecord);
                }
            }
        }
    }
}

