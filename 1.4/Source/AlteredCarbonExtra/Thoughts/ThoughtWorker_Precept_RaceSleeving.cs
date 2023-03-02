using RimWorld;
using Verse;

namespace AlteredCarbon;

public class ThoughtWorker_Precept_RaceSleeving : ThoughtWorker_Precept
{
    public override ThoughtState ShouldHaveThought(Pawn p)
    {
        return p.HasCorticalStack(out var stack) && (stack.PersonaData.originalRace != p.def);
    }
}