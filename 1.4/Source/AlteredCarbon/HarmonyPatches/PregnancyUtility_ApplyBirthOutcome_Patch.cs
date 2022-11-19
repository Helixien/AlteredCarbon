using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(PregnancyUtility), "ApplyBirthOutcome")]
    public static class PregnancyUtility_ApplyBirthOutcome_Patch
    {
        public static void Prefix(OutcomeChance outcome, float quality, Precept_Ritual ritual, List<GeneDef> genes,
            Pawn geneticMother, Thing birtherThing, Pawn father = null, Pawn doctor = null, LordJob_Ritual lordJobRitual = null, RitualRoleAssignments assignments = null)
        {
            if (birtherThing is Pawn pawn && pawn.HasCorticalStack(out var hediff))
            {
                var origPawn = hediff.PersonaData.origPawn;
                if (origPawn != null && origPawn.gender != pawn.gender)
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemory(AC_DefOf.VFEU_WrongGenderChild);
                }
            }
        }
    }
}

