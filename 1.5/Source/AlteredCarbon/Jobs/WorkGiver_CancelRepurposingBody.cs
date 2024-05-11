using System;
using Verse;
using Verse.AI;
using RimWorld;
using System.Collections.Generic;

namespace AlteredCarbon
{
	public class WorkGiver_CancelRepurposingBody : WorkGiver_Scanner
	{
		public override PathEndMode PathEndMode => PathEndMode.InteractionCell;
		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			foreach (var thing in pawn.Map.listerThings.ThingsOfDef(AC_DefOf.AC_SleeveGestator))
            {
				yield return thing;
            }
		}
		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (t is Building_Incubator incubator && incubator.incubatorState != IncubatorState.ToBeCanceled)
            {
				return false;
            }
			if (!pawn.CanReserveAndReach(t, PathEndMode, Danger.Deadly))
            {
                return false;
			}
			if (t is Building_SleeveGestator sleeveGrower && (sleeveGrower.InnerPawn?.Dead ?? false))
            {
                return true;
            }
            return false;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return JobMaker.MakeJob(AC_DefOf.AC_CancelRepurposingBody, t);
		}
	}
}
