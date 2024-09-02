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
        public bool spawnArchotechStack;

        public void TryAddStack(Pawn pawn)
        {
            if (AC_Utils.generalSettings.enableStackSpawning)
            {
                if (ModCompatibility.VanillaRacesExpandedAndroidIsActive && ModCompatibility.IsAndroid(pawn))
                {
                    return;
                }
                if (pawn.HasNeuralStack() is false && Rand.Chance(chanceToSpawnWithStack / 100f))
                {
                    var neckRecord = pawn.GetNeck();
                    var hediff = HediffMaker.MakeHediff(spawnArchotechStack ? AC_DefOf.AC_ArchotechStack 
                        : AC_DefOf.AC_NeuralStack, pawn, neckRecord) as Hediff_NeuralStack;
                    pawn.health.AddHediff(hediff, neckRecord);
                }

                if (pawn.HasNeuralStack() && Rand.Chance(chanceToSpawnSleeveQuality / 100f))
                {
                    if (pawn.genes.GenesListForReading.Any(x => AC_Utils.sleeveQualities.Contains(x.def)) is false)
                    {
                        List<(GeneDef gene, float weight)> genesWithChances = new List<(GeneDef, float)>
                        {
                            (AC_DefOf.AC_SleeveQuality_Normal, 0.5f),
                            (AC_DefOf.AC_SleeveQuality_Poor, 0.2f),
                            (AC_DefOf.AC_SleeveQuality_Good, 0.2f),
                            (AC_DefOf.AC_SleeveQuality_Awful, 0.1f),
                            (AC_DefOf.AC_SleeveQuality_Excellent, 0.1f),
                            (AC_DefOf.AC_SleeveQuality_Masterwork, 0.05f),
                        };
                        var qualityGene = genesWithChances.RandomElementByWeight(x => x.weight).gene;
                        pawn.genes.AddGene(GeneMaker.MakeGene(qualityGene, pawn), false);
                    }
                }
            }
        }
    }
}

