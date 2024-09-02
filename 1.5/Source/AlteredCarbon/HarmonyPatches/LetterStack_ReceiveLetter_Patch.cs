using System;
using HarmonyLib;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(LetterStack), "ReceiveLetter", new Type[]
    {
        typeof(Letter),
        typeof(string),
        typeof(int),
        typeof(bool)
    })]
    internal static class LetterStack_ReceiveLetter_Patch
    {
        private static void Prefix(Letter let)
        {
            if (let.def == AC_DefOf.HumanPregnancy && let.lookTargets.PrimaryTarget.Thing is Pawn pawn && 
                pawn.HasNeuralStack(out var hediff) && pawn.CanThink())
            {
                if (hediff.NeuralData.OriginalGender != pawn.gender) 
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemory(AC_DefOf.AC_WrongGenderPregnant);
                }
            }
        }
    }
}

