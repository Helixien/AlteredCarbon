using RimWorld;
using Verse;

namespace AlteredCarbon;

public class ThoughtWorker_Precept_RaceSleeving : ThoughtWorker_Precept
{
    public override ThoughtState ShouldHaveThought(Pawn p)
    {
        return p.HasNeuralStack(out var stack) && (stack.NeuralData.OriginalRace != p.def);
    }
}