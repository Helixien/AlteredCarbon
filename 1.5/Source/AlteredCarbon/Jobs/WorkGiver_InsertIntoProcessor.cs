using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    public class WorkGiver_InsertXenogerm : WorkGiver_InsertIntoProcessor
    {
        public override IEnumerable<ThingDef> TargetThings => Gen.YieldSingle(ThingDefOf.Xenogerm);
        public override ThingDef ProcessorDef => AC_DefOf.AC_XenoGermDuplicator;
    }

    public class WorkGiver_InsertGenepack : WorkGiver_InsertIntoProcessor
    {
        public override IEnumerable<ThingDef> TargetThings => AC_Utils.allGenepacks;
        public override ThingDef ProcessorDef => AC_DefOf.AC_GeneCentrifuge;
    }

    [HotSwappable]
    public abstract class WorkGiver_InsertIntoProcessor : WorkGiver_Scanner
    {
        public abstract IEnumerable<ThingDef> TargetThings { get; }
        public abstract ThingDef ProcessorDef { get; }
        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            var things = new List<Thing>();
            foreach (var thing in TargetThings)
            {
                things.AddRange(pawn.Map.listerThings.ThingsOfDef(thing)
                .Where(x => pawn.CanReserveAndReach(x, PathEndMode.Touch, Danger.Deadly)));
            }
            return things;
        }
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return GetProcessors(pawn, t).Any();
        }
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var processor = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, GetProcessors(pawn, t), 
                PathEndMode.Touch, TraverseParms.For(pawn));
            var job = JobMaker.MakeJob(AC_DefOf.AC_InsertingThingIntoProcessor, t, processor);
            job.count = 1;
            return job;
        }

        private IEnumerable<Building_Processor> GetProcessors(Pawn hauler, Thing targetThing)
        {
            var storages = hauler.Map.listerThings.ThingsOfDef(ProcessorDef).Cast<Building_Processor>()
                .Where(x => x.Accepts(targetThing) && hauler.CanReserveAndReach(x, PathEndMode.Touch, Danger.Deadly));
            return storages;
        }
    }
}


