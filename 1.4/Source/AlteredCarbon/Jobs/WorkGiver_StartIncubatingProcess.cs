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
			var grower = t as Building_SleeveGrower;
			if (grower.xenogermToConsume != null)
			{
				if (grower.xenogermToConsume.Destroyed || grower.xenogermToConsume.Spawned is false 
					|| pawn.CanReserveAndReach(grower.xenogermToConsume, PathEndMode.ClosestTouch, Danger.Deadly) is false)
				{
					return null;
				}
                return JobMaker.MakeJob(AC_DefOf.VFEU_StartIncubatingProcess, t, grower.xenogermToConsume);
            }
            return JobMaker.MakeJob(AC_DefOf.VFEU_StartIncubatingProcess, t);
		}
	}
}
