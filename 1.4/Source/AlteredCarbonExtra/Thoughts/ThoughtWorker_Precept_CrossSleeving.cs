using RimWorld;
using Verse;

namespace AlteredCarbon;

public class ThoughtWorker_Precept_CrossSleeving : ThoughtWorker_Precept
{
    public override ThoughtState ShouldHaveThought(Pawn p)
    {
        return (p.HasCorticalStack(out Hediff_CorticalStack hediff) && hediff.PersonaData.gender != p.gender);
    }
}