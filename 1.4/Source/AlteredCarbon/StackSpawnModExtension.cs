using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AlteredCarbon
{
    public class StackSpawnModExtension : DefModExtension
    {
        public int chanceToSpawnWithStack;
        public int chanceToSpawnSleeveQuality;
        public bool spawnArchoStack;

        public void TryAddStack(Pawn pawn)
        {
            if (ACUtils.generalSettings.enableStackSpawning)
            {
                if (pawn.HasCorticalStack() is false && Rand.Chance(chanceToSpawnWithStack / 100f))
                {
                    var neckRecord = pawn.GetNeck();
                    var hediff = HediffMaker.MakeHediff(spawnArchoStack && ModCompatibility.HelixienAlteredCarbonIsActive
                        ? AC_DefOf.AC_ArchoStack : AC_DefOf.VFEU_CorticalStack, pawn, neckRecord) as Hediff_CorticalStack;
                    pawn.health.AddHediff(hediff, neckRecord);
                }

                if (pawn.HasCorticalStack() && Rand.Chance(chanceToSpawnSleeveQuality / 100f))
                {
                    if (pawn.genes.GenesListForReading.Any(x => ACUtils.sleeveQualities.Contains(x.def)) is false)
                    {
                        List<(GeneDef gene, float weight)> genesWithChances = new List<(GeneDef, float)>
                        {
                            (AC_DefOf.VFEU_SleeveQuality_Normal, 0.5f),
                            (AC_DefOf.VFEU_SleeveQuality_Poor, 0.2f),
                            (AC_DefOf.VFEU_SleeveQuality_Good, 0.2f),
                            (AC_DefOf.VFEU_SleeveQuality_Awful, 0.1f),
                            (AC_DefOf.VFEU_SleeveQuality_Excellent, 0.1f),
                            (AC_DefOf.VFEU_SleeveQuality_Masterwork, 0.05f),
                        };
                        var qualityGene = genesWithChances.RandomElementByWeight(x => x.weight).gene;
                        pawn.genes.AddGene(GeneMaker.MakeGene(qualityGene, pawn), false);
                    }
                }
            }
        }
    }
}

