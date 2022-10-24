using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    public class JobGiver_TakeStackWhenClose : ThinkNode_JobGiver
    {
        public override Job TryGiveJob(Pawn pawn)
        {
            if (!RCellFinder.TryFindBestExitSpot(pawn, out IntVec3 spot))
            {
                return null;
            }
            IEnumerable<CorticalStack> corticalStacks = pawn.Map.listerThings.ThingsOfDef(AC_DefOf.VFEU_FilledCorticalStack).Cast<CorticalStack>()
                .Where(x => x.PersonaData.faction == pawn.Faction && x.Position.DistanceTo(pawn.Position) < 10);
            if (corticalStacks.Any())
            {
                CorticalStack stack = corticalStacks.FirstOrDefault(x => pawn.CanReserveAndReach(x, PathEndMode.Touch, Danger.Deadly));
                if (stack != null)
                {
                    Job job = JobMaker.MakeJob(JobDefOf.Steal);
                    job.targetA = stack;
                    job.locomotionUrgency = LocomotionUrgency.Jog;
                    job.targetB = spot;
                    job.count = 1;
                    return job;
                }
            }
            return null;
        }
    }
}