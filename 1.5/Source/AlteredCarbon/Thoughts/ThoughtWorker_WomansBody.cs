using RimWorld;
using Verse;

namespace AlteredCarbon
{
	public class ThoughtWorker_WomansBody : ThoughtWorker
	{
		public override ThoughtState CurrentStateInternal(Pawn p)
		{
			return p.story.traits.HasTrait(TraitDefOf.DislikesWomen) && p.HasNeuralStack() && p.gender == Gender.Female
				? ThoughtState.ActiveDefault
				: ThoughtState.Inactive;
		}
	}
}

