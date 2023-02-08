using System;
using Verse;
using Verse.AI;
using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace AlteredCarbon
{
    [HotSwappable]
    public class WorkGiver_InsertGenepack : WorkGiver_Scanner
    {
        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            var genepacks = pawn.Map.listerThings.ThingsOfDef(ThingDefOf.Genepack)
                .Where(x => pawn.CanReserveAndReach(x, PathEndMode.Touch, Danger.Deadly));
            return genepacks;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return GetGeneCentrifuges(pawn, t).Any();;
        }
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var centrifuge = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, GetGeneCentrifuges(pawn, t), PathEndMode.Touch, TraverseParms.For(pawn));
            var job = JobMaker.MakeJob(AC_Extra_DefOf.AC_InsertingGenepackIntoCentrifuge, t, centrifuge);
            job.count = 1;
            return job;
        }

        private static IEnumerable<Building_GeneCentrifuge> GetGeneCentrifuges(Pawn hauler, Thing genepack)
        {
            var storages = hauler.Map.listerThings.ThingsOfDef(AC_Extra_DefOf.AC_GeneCentrifuge).Cast<Building_GeneCentrifuge>()
                .Where(x => x.Accepts(genepack) && hauler.CanReserveAndReach(x, PathEndMode.Touch, Danger.Deadly));
            return storages;
        }
    }
}


