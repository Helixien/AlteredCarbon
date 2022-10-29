using System;
using Verse;
using Verse.AI;
using RimWorld;
using System.Collections.Generic;

namespace AlteredCarbon
{
	public class WorkGiver_StartIncubatingProcess : WorkGiver_Scanner
	{
		public override PathEndMode PathEndMode => PathEndMode.InteractionCell;
		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			if (ModCompatibility.HelixienAlteredCarbonIsActive)
			{
                foreach (var thing in pawn.Map.listerThings.ThingsOfDef(AC_DefOf.VFEU_OrganIncubator))
                {
                    yield return thing;
                }
            }
			foreach (var thing in pawn.Map.listerThings.ThingsOfDef(AC_DefOf.VFEU_SleeveIncubator))
            {
				yield return thing;
            }
		}
		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (t is Building_Incubator incubator && incubator.incubatorState != IncubatorState.ToBeActivated)
            {
				return false;
            }
			if (!pawn.CanReserveAndReach(t, PathEndMode, Danger.Deadly))
			{
				return false;
			}
			return t is Building_Incubator;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return JobMaker.MakeJob(AC_DefOf.VFEU_StartIncubatingProcess, t);
		}
	}
}
