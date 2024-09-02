using RimWorld;
using Verse;

namespace AlteredCarbon;

public class ThoughtWorker_Precept_StackDuplicating_HasDuplicates_Social : ThoughtWorker_Precept_Social
{
    public override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn)
    {
        return otherPawn.HasNeuralStack(out var stack) && stack.NeuralData.StackGroupData.originalPawn == otherPawn && stack.NeuralData.StackGroupData.copiedPawns.Count > 0;
    }
}

public class ThoughtWorker_Precept_StackDuplicating_ForOriginal_Social : ThoughtWorker_Precept_Social
{
    public override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn)
    {
        return otherPawn.HasNeuralStack(out var otherPawnStack) && otherPawnStack.NeuralData.StackGroupData.originalPawn == p;
    }
}

public class ThoughtWorker_Precept_StackDuplicating_ForCopy_Social : ThoughtWorker_Precept_Social
{
    public override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn)
    {
        return otherPawn.HasNeuralStack(out var otherPawnStack) && otherPawnStack.NeuralData.StackGroupData.copiedPawns.Contains(p);
    }
}

