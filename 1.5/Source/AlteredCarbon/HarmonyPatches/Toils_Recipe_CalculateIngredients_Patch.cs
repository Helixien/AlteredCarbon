using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Toils_Recipe), "CalculateIngredients")]
    public static class Toils_Recipe_CalculateIngredients_Patch
    {
        public static void Prefix(Job job, Pawn actor)
        {
            if (job.RecipeDef == AC_DefOf.CremateCorpse || job.RecipeDef == AC_DefOf.ButcherCorpseFlesh)
            {
                if (job.placedThings != null)
                {
                    for (int i = 0; i < job.placedThings.Count; i++)
                    {
                        var thing = job.placedThings[i].thing;
                        if (thing is Corpse corpse && corpse.InnerPawn.HasNeuralStack(out var hediff))
                        {
                            hediff.SpawnStack();
                        }
                    }
                }
            }
        }
    }
}

