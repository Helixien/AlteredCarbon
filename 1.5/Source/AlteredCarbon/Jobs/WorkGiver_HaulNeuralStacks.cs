using System;
using Verse;
using Verse.AI;
using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace AlteredCarbon
{
    [HotSwappable]
    public class WorkGiver_HaulNeuralStacks : WorkGiver_Scanner
    {
        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            var prints = pawn.Map.listerThings.AllThings.OfType<NeuralStack>().Where(x => x.autoLoad 
                && x.NeuralData.ContainsNeural
                && pawn.CanReserveAndReach(x, PathEndMode.Touch, Danger.Deadly));
            return prints;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return GetNeuralCaches(pawn, t).Any();;
        }
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var neuralCache = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, GetNeuralCaches(pawn, t), PathEndMode.Touch, TraverseParms.For(pawn));
            var job = JobMaker.MakeJob(AC_DefOf.AC_HaulThingsToContainer, t, neuralCache);
            job.count = 1;
            return job;
        }

        private static IEnumerable<Thing> GetNeuralCaches(Pawn hauler, Thing stack)
        {
            var storages = hauler.Map.listerThings.ThingsOfDef(AC_DefOf.AC_StackCache)
                .Where(x => x.TryGetComp<CompNeuralCache>() is CompNeuralCache comp
                && comp.Accepts(stack) && hauler.CanReserveAndReach(x, PathEndMode.Touch, Danger.Deadly));
            return storages;
        }
    }
}


