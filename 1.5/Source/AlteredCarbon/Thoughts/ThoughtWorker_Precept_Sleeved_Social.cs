using RimWorld;
using Verse;

namespace AlteredCarbon;

public class ThoughtWorker_Precept_Sleeved_Social : ThoughtWorker_Precept_Social
{
    public override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn)
    {
        if (!otherPawn.AcceptsStacks() || !otherPawn.UsesSleeve())
        {
            return false;
        }
        
        return ThoughtState.ActiveAtStage(AC_Utils.sleeveQualities.IndexOf(otherPawn.GetSleeveQuality()));
    }
}