using RimWorld;
using Verse;

namespace AlteredCarbon;

public class ThoughtWorker_Precept_ArchotechStack : ThoughtWorker_Precept
{
    public override ThoughtState ShouldHaveThought(Pawn p)
    {
        return p.AcceptsStacks() && p.HasNeuralStack(out var stackHediff) && stackHediff.def == AC_DefOf.AC_ArchotechStack;
    }
}