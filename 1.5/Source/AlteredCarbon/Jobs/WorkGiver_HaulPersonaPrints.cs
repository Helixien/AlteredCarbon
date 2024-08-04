using System;
using Verse;
using Verse.AI;
using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace AlteredCarbon
{
    [HotSwappable]
    public class WorkGiver_HaulPersonaPrints : WorkGiver_Scanner
    {
        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            var frames = pawn.Map.listerThings.AllThings.OfType<PersonaPrint>().Where(x => x.autoLoad 
                && x.PersonaData.ContainsPersona
                && pawn.CanReserveAndReach(x, PathEndMode.Touch, Danger.Deadly));
            return frames;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return GetPersonaMatrices(pawn, t).Any();;
        }
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var personaMatrix = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, GetPersonaMatrices(pawn, t), PathEndMode.Touch, TraverseParms.For(pawn));
            var job = JobMaker.MakeJob(AC_DefOf.AC_HaulThingsToContainer, t, personaMatrix);
            job.count = 1;
            return job;
        }

        private static IEnumerable<Building_PersonaMatrix> GetPersonaMatrices(Pawn hauler, Thing stack)
        {
            var storages = hauler.Map.listerThings.ThingsOfDef(AC_DefOf.AC_PersonaMatrix).Cast<Building_PersonaMatrix>()
                .Where(x => x.HasFreeSpace && x.Accepts(stack) && hauler.CanReserveAndReach(x, PathEndMode.Touch, Danger.Deadly));
            return storages;
        }
    }
}


