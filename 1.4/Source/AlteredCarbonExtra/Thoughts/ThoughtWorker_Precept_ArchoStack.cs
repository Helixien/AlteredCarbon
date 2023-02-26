using RimWorld;
using Verse;

namespace AlteredCarbon;

public class ThoughtWorker_Precept_ArchoStack : ThoughtWorker_Precept
{
    public override ThoughtState ShouldHaveThought(Pawn p)
    {
        return p.AcceptsStacks() && p.HasCorticalStack(out var stackHediff) && stackHediff.def == AC_DefOf.AC_ArchoStack;
    }
}