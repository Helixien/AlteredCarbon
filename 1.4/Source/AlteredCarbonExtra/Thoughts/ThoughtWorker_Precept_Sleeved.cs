using RimWorld;
using Verse;

namespace AlteredCarbon;

public class ThoughtWorker_Precept_Sleeved: ThoughtWorker_Precept
{
    public override ThoughtState ShouldHaveThought(Pawn p)
    {
        if (!p.AcceptsStacks() || !p.UsesSleeve())
        {
            return false;
        }

        return ThoughtState.ActiveAtStage(ACUtils.sleeveQualities.IndexOf(p.GetSleeveQuality()));
    }
}