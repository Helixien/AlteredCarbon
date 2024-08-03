using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    public class WorkGiver_CreateStackFromPersonaPrint : WorkGiver_Scanner
    {
        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return pawn.Map.listerThings.ThingsOfDef(AC_DefOf.AC_PersonaEditor).Cast<Building_PersonaEditor>()
                .Where(x => x.Powered && x.HasPersonaPrintToRestore
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
            var personaPrint = (t as Building_PersonaEditor).GetPersonaPrintToRestore;
            Job job = JobMaker.MakeJob(AC_DefOf.AC_CreateStackFromPersonaPrint, t, emptyPersonaStack, personaPrint);
            job.count = 1;
            return job;
        }
    }
}