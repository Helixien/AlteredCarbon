using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{

    [HarmonyPatch(typeof(TraitSet), "GainTrait")]
    public static class TraitSet_GainTrait_Patch
    {
        public static void Postfix(Pawn ___pawn, Trait trait)
        {
            if (PawnGenerator.IsBeingGenerated(___pawn))
            {
                LongEventHandler.toExecuteWhenFinished.Add(delegate
                {

                    var extension = trait?.def.GetModExtension<StackSpawnModExtension>();
                    if (extension != null)
                    {
                        extension.TryAddStack(___pawn);
                    }
                });
            }
        }
    }
}

