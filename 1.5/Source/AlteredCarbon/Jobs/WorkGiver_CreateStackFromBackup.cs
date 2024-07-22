using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    public class WorkGiver_CreateStackFromBackup : WorkGiver_Scanner
    {
        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return pawn.Map.listerThings.ThingsOfDef(AC_DefOf.AC_PersonaMatrix).Cast<Building_PersonaMatrix>()
                .Where(x => x.autoRestoreIsEnabled && x.Powered && GameComponent_DigitalStorage.Instance.FirstPersonaStackToRestore != null
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
            Job job = JobMaker.MakeJob(AC_DefOf.AC_CreateStackFromBackup, t, emptyPersonaStack);
            job.count = 1;
            return job;
        }
    }
}