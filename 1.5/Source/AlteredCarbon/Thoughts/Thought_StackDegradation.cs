using RimWorld;
using Verse;

namespace AlteredCarbon
{
    public class Thought_StackDegradation : Thought_Memory
    {
        public override int CurStageIndex => GetThoughtStageIndex();
        public int GetThoughtStageIndex()
        {
            var hediff = pawn.GetHediff(def.hediff) as Hediff_StackDegradation;
            if (hediff == null) return 0;
            else if (hediff.stackDegradation >= 0.9f) return 5;
            else if (hediff.stackDegradation >= 0.6f) return 4;
            else if (hediff.stackDegradation >= 0.4f) return 3;
            else if (hediff.stackDegradation >= 0.2f) return 2;
            else if (hediff.stackDegradation >= 0.1f) return 1;
            return 0;
        }
    }
}