using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace AlteredCarbon
{
	public class LordToil_TakeStack : LordToil
	{
		protected DutyDef DutyDef => AC_Extra_DefOf.VFEU_TakeStacks;

		public override bool ForceHighStoryDanger => false;

		public override bool AllowSelfTend => true;

		public override void UpdateAllDuties()
		{
			List<Thing> list = null;
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				Pawn pawn = lord.ownedPawns[i];
				Thing target = null;
				if (TryFindGoodOpportunisticTaskTarget(pawn, out target, list) && !GenAI.InDangerousCombat(pawn))
				{
					if (pawn.mindState.duty == null || pawn.mindState.duty.def != DutyDef)
					{
						pawn.mindState.duty = new PawnDuty(DutyDef);
						pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
					}
					if (list == null)
					{
						list = new List<Thing>();
					}
					list.Add(target);
				}
				else
				{
					pawn.mindState.duty = new PawnDuty(DutyDefOf.HuntEnemiesIndividual);
				}
			}
		}

		protected bool TryFindGoodOpportunisticTaskTarget(Pawn pawn, out Thing target, List<Thing> alreadyTakenTargets)
		{
			if (pawn.mindState.duty != null && pawn.mindState.duty.def == DutyDef && pawn.carryTracker.CarriedThing is CorticalStack)
			{
				target = pawn.carryTracker.CarriedThing;
				return true;
			}
			var corticalStacks = pawn.Map.listerThings.ThingsOfDef(AC_DefOf.VFEU_FilledCorticalStack).Cast<CorticalStack>().Where(x => x.PersonaData.faction == pawn.Faction);
			if (corticalStacks.Any())
			{
				var stack = corticalStacks.FirstOrDefault(x => pawn.CanReserveAndReach(x, PathEndMode.Touch, Danger.Deadly));
				if (stack != null)
				{
					target = stack;
					return true;
				}
			}
			target = null;
			return false;
		}

		public override void LordToilTick()
		{
			if (Find.TickManager.TicksGame % 181 != 0)
			{
				return;
			}
			List<Thing> list = null;
			for (int i = 0; i < lord.ownedPawns.Count; i++)
			{
				Pawn pawn = lord.ownedPawns[i];
				if (pawn.Downed || pawn.mindState.duty.def != DutyDefOf.HuntEnemiesIndividual)
				{
					continue;
				}
				Thing target = null;
				if (TryFindGoodOpportunisticTaskTarget(pawn, out target, list) && !base.Map.reservationManager.IsReservedByAnyoneOf(target, lord.faction) && !GenAI.InDangerousCombat(pawn))
				{
					pawn.mindState.duty = new PawnDuty(DutyDef);
					pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
					if (list == null)
					{
						list = new List<Thing>();
					}
					list.Add(target);
				}
			}
		}
	}

	public class LordJob_TakeStacks : LordJob
	{
		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil_TakeStack lordToil_KidnapCover = new LordToil_TakeStack
			{
				useAvoidGrid = true
			};
			stateGraph.AddToil(lordToil_KidnapCover);
			return stateGraph;
		}

		public override void ExposeData()
		{
		}
	}
}

