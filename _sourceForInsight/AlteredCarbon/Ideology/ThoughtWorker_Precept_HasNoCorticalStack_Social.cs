using RimWorld;
using Verse;

namespace AlteredCarbon
{
	public class ThoughtWorker_Precept_HasNoCorticalStack_Social : ThoughtWorker_Precept_Social
	{
		public override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn)
		{
			return !otherPawn.HasStack();
		}
	}
}

