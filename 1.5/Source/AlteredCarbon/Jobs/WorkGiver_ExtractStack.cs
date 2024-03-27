using System;
using Verse;
using Verse.AI;
using RimWorld;
using System.Collections.Generic;

namespace AlteredCarbon
{
	public class WorkGiver_ExtractStack : WorkGiver_Scanner
	{
		public override PathEndMode PathEndMode => PathEndMode.ClosestTouch;

		public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
		{
			foreach (Designation item in pawn.Map.designationManager.SpawnedDesignationsOfDef(AC_DefOf.VFEU_ExtractStackDesignation))
			{
				if (!item.target.HasThing)
				{
					Log.ErrorOnce("ExtractStack designation has no target.", 63126);
				}
				else
				{
					yield return item.target.Thing;
				}
			}
		}

		public override Danger MaxPathDanger(Pawn pawn)
		{
			return Danger.Deadly;
		}

		public override bool ShouldSkip(Pawn pawn, bool forced = false)
		{
			return !pawn.Map.designationManager.AnySpawnedDesignationOfDef(AC_DefOf.VFEU_ExtractStackDesignation);
		}

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			if (t.Map.designationManager.DesignationOn(t, AC_DefOf.VFEU_ExtractStackDesignation) == null)
			{
				return false;
			}
			if (!pawn.CanReserve(t, 1, -1, null, forced))
			{
				return false;
			}
			if (t is Corpse corpse && corpse.InnerPawn.HasCorticalStack(out _))
			{
				return true;
			}
			return true;
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return JobMaker.MakeJob(AC_DefOf.VFEU_ExtractStack, t);
		}
	}
}
