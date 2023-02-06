using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{

    [HarmonyPatch(typeof(Pawn), "SpawnSetup")]
    public static class Pawn_SpawnSetup_Patch
    {
        public static void Postfix(Pawn __instance, Map map, bool respawningAfterLoad)
        {
            if (!respawningAfterLoad && __instance.RaceProps.Humanlike && __instance.kindDef.HasModExtension<StackSpawnModExtension>())
            {
                var extension = __instance.kindDef.GetModExtension<StackSpawnModExtension>();
                if (extension.SpawnsWithStack && __instance.HasCorticalStack(out _) is false
                    && Rand.Chance((float)extension.ChanceToSpawnWithStack / 100f))
                {
                    BodyPartRecord neckRecord = __instance.GetNeck();
                    var hediff = HediffMaker.MakeHediff(AC_DefOf.VFEU_CorticalStack, __instance, neckRecord) as Hediff_CorticalStack;
                    __instance.health.AddHediff(hediff, neckRecord);
                    AlteredCarbonManager.Instance.RegisterPawn(__instance);
                    AlteredCarbonManager.Instance.TryAddRelationships(__instance);
                }
            }
        }
    }
}

