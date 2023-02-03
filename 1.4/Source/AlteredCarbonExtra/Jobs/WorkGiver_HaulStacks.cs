using System;
using Verse;
using Verse.AI;
using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace AlteredCarbon
{
    [HotSwappable]
    public class WorkGiver_HaulStacks : WorkGiver_Scanner
    {
        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            var stacks = CorticalStack.corticalStacks.Where(x => x.Spawned && pawn.Map == x.Map && x.PersonaData.ContainsInnerPersona
            && !pawn.Map.mapPawns.AllPawnsSpawned.Any(y => y.BillStack.Bills.Any(c => c is Bill_InstallStack installStack
                && installStack.stackToInstall == x))
            && pawn.CanReserveAndReach(x, PathEndMode.Touch, Danger.Deadly));
            return stacks;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return GetStackArrays(pawn, t).Any();;
        }
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var stackArrays = GetStackArrays(pawn, t).ToList();
            var stackArray = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, stackArrays, PathEndMode.Touch, TraverseParms.For(pawn));
            var job = JobMaker.MakeJob(AC_Extra_DefOf.AC_HaulingSTacks, t, stackArray);
            job.count = 1;
            return job;
        }

        private static IEnumerable<Building_StackStorage> GetStackArrays(Pawn hauler, Thing stack)
        {
            var storages = Building_StackStorage.building_StackStorages.Where(x => x.HasFreeSpace
                            && x.Accepts(stack) && hauler.CanReserveAndReach(x, PathEndMode.Touch, Danger.Deadly));
            return storages;
        }
    }
}


