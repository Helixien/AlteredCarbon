using RimWorld;
using Verse;

namespace AlteredCarbon;

public class ThoughtWorker_Precept_XenoSleeving_Social : ThoughtWorker_Precept_Social
{
    public override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn)
    {
        return otherPawn.HasPersonaStack(out var stack) && otherPawn.SleeveMatchesOriginalXenotype(stack.PersonaData);
    }
}