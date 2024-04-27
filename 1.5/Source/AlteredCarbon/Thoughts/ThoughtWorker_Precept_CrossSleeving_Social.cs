using RimWorld;
using Verse;

namespace AlteredCarbon;

public class ThoughtWorker_Precept_CrossSleeving_Social: ThoughtWorker_Precept_Social
{
    public override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn)
    {
        return otherPawn.AcceptsStacks() && (otherPawn.HasPersonaStack(out Hediff_PersonaStack hediff) && hediff.PersonaData.OriginalGender != otherPawn.gender);
    }
}