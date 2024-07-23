using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    public class WorkGiver_CreateStackFromMindFrame : WorkGiver_Scanner
    {
        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return pawn.Map.listerThings.ThingsOfDef(AC_DefOf.AC_NeuralEditor).Cast<Building_NeuralEditor>()
                .Where(x => x.Powered && x.HasMindFrameToRestore
                && pawn.CanReserveAndReach(x, PathEndMode.Touch, Danger.Deadly));
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (pawn.skills.GetSkill(SkillDefOf.Intellectual).Level < 10)
            {
                JobFailReason.Is("AC.CannotCopyNoIntellectual".Translate());
                return false;
            }
            Thing emptyPersonaStack = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map,
                    ThingRequest.ForDef(AC_DefOf.AC_EmptyPersonaStack), PathEndMode.Touch, TraverseParms.For(pawn));
            if (emptyPersonaStack is null)
            {
                JobFailReason.Is("AC.CannotRestoreBackupNoOtherEmptyStacks".Translate());
                return false;
            }
            return true;
        }
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Thing emptyPersonaStack = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map,
                ThingRequest.ForDef(AC_DefOf.AC_EmptyPersonaStack), PathEndMode.Touch, TraverseParms.For(pawn));
            var mindFrame = (t as Building_NeuralEditor).GetMindFrameToRestore;
            Job job = JobMaker.MakeJob(AC_DefOf.AC_CreateStackFromMindFrame, t, emptyPersonaStack, mindFrame);
            job.count = 1;
            return job;
        }
    }
}