using RimWorld;
using Verse;

namespace AlteredCarbon;

public class ThoughtWorker_Precept_XenoSleeving : ThoughtWorker_Precept
{
    public override ThoughtState ShouldHaveThought(Pawn p)
    {
        return p.HasPersonaStack(out var stack) && p.SleeveMatchesOriginalXenotype(stack.PersonaData);
    }
}