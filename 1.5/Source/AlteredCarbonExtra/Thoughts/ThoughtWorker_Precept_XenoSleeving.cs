using RimWorld;
using Verse;

namespace AlteredCarbon;

public class ThoughtWorker_Precept_XenoSleeving : ThoughtWorker_Precept
{
    public override ThoughtState ShouldHaveThought(Pawn p)
    {
        return p.HasCorticalStack(out var stack) && p.SleeveMatchesOriginalXenotype(stack.PersonaData);
    }
}