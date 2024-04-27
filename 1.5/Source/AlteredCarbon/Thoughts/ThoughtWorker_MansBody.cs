using RimWorld;
using Verse;

namespace AlteredCarbon
{
	public class ThoughtWorker_MansBody : ThoughtWorker
	{
		public override ThoughtState CurrentStateInternal(Pawn p)
		{
			return p.story.traits.HasTrait(TraitDefOf.DislikesMen) && p.HasPersonaStack() && p.gender == Gender.Male ? ThoughtState.ActiveDefault : ThoughtState.Inactive;
		}
	}
}

