using RimWorld;
using Verse;

namespace AlteredCarbon;

public class ThoughtWorker_Precept_Stack : ThoughtWorker_Precept
{
    public override ThoughtState ShouldHaveThought(Pawn p)
    {
        return p.AcceptsStacks() && p.HasPersonaStack(out var stackHediff) && stackHediff.def == AC_DefOf.AC_PersonaStack;
    }
}