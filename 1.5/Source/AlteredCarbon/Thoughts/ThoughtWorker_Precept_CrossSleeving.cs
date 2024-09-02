using RimWorld;
using Verse;

namespace AlteredCarbon;

public class ThoughtWorker_Precept_CrossSleeving : ThoughtWorker_Precept
{
    public override ThoughtState ShouldHaveThought(Pawn p)
    {
        return p.AcceptsStacks() && (p.HasNeuralStack(out Hediff_NeuralStack hediff) && hediff.NeuralData.OriginalGender != p.gender);
    }
}