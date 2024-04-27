using RimWorld;
using Verse;

namespace AlteredCarbon;

public class ThoughtWorker_Precept_CrossSleeving : ThoughtWorker_Precept
{
    public override ThoughtState ShouldHaveThought(Pawn p)
    {
        return p.AcceptsStacks() && (p.HasPersonaStack(out Hediff_PersonaStack hediff) && hediff.PersonaData.OriginalGender != p.gender);
    }
}