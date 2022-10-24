using System;
using Verse;
using Verse.AI;
using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace AlteredCarbon
{
    public class WorkGiver_HaulStacks : WorkGiver_Scanner
    {
        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return CorticalStack.corticalStacks.Where(x => x.Spawned && pawn.Map == x.Map && x.PersonaData.ContainsInnerPersona 
            && !pawn.Map.mapPawns.AllPawnsSpawned.Any(y => y.BillStack.Bills.Any(c => c is Bill_InstallStack installStack && installStack.stackToInstall == x))
            && pawn.CanReserveAndReach(x, PathEndMode.Touch, Danger.Deadly));
        }
        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return !CorticalStack.corticalStacks.Any(x => x.Spawned && pawn.Map == x.Map) || !Building_StackStorage.building_StackStorages
                    .Any(x => x.HasFreeSpace && pawn.CanReserveAndReach(x, PathEndMode.Touch, Danger.Deadly));
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return Building_StackStorage.building_StackStorages.Any(x => x.HasFreeSpace && x.Accepts(t) && pawn.CanReserveAndReach(x, PathEndMode.Touch, Danger.Deadly));
        }
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var containers = Building_StackStorage.building_StackStorages.Where(x => x.HasFreeSpace && x.Accepts(t) && pawn.CanReserveAndReach(x, PathEndMode.Touch, Danger.Deadly));
            if (containers.Any())
            {
                var container = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, containers, PathEndMode.Touch, TraverseParms.For(pawn));
                var job = JobMaker.MakeJob(JobDefOf.HaulToContainer, t, container);
                job.count = 1;
                return job;
            }
            Log.Error("Altered Carbon can't find any suitable storage");
            return null;
        }
    }
}


