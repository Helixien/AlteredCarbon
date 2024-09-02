using RimWorld;
using Verse;

namespace AlteredCarbon;

public class ThoughtWorker_Precept_Stack_Social : ThoughtWorker_Precept_Social
{
    public override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn)
    {
        return otherPawn.AcceptsStacks() && otherPawn.HasNeuralStack(out _);
    }
}