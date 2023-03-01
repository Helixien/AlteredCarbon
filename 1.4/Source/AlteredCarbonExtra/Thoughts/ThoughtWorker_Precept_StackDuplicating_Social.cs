using RimWorld;
using Verse;

namespace AlteredCarbon;

public class ThoughtWorker_Precept_StackDuplicating_HasDuplicates_Social : ThoughtWorker_Precept_Social
{
    public override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn)
    {
        return otherPawn.HasCorticalStack(out var stack) && stack.PersonaData.StackGroupData.originalPawn == otherPawn && stack.PersonaData.StackGroupData.copiedPawns.Count > 0;
    }
}

public class ThoughtWorker_Precept_StackDuplicating_ForOriginal_Social : ThoughtWorker_Precept_Social
{
    public override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn)
    {
        return otherPawn.HasCorticalStack(out var otherPawnStack) && otherPawnStack.PersonaData.StackGroupData.originalPawn == p;
    }
}

public class ThoughtWorker_Precept_StackDuplicating_ForCopy_Social : ThoughtWorker_Precept_Social
{
    public override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn)
    {
        return otherPawn.HasCorticalStack(out var otherPawnStack) && otherPawnStack.PersonaData.StackGroupData.copiedPawns.Contains(p);
    }
}

