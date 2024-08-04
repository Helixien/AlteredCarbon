using System;
using Verse;
using Verse.AI;
using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace AlteredCarbon
{
    [HotSwappable]
    public class WorkGiver_HaulPersonaStacks : WorkGiver_Scanner
    {
        public override Danger MaxPathDanger(Pawn pawn)
        {
            return Danger.Deadly;
        }
        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            var frames = pawn.Map.listerThings.AllThings.OfType<PersonaStack>().Where(x => x.autoLoad 
                && x.PersonaData.ContainsPersona
                && pawn.CanReserveAndReach(x, PathEndMode.Touch, Danger.Deadly));
            return frames;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return GetPersonaCaches(pawn, t).Any();;
        }
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var personaCache = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, GetPersonaCaches(pawn, t), PathEndMode.Touch, TraverseParms.For(pawn));
            var job = JobMaker.MakeJob(AC_DefOf.AC_HaulThingsToContainer, t, personaCache);
            job.count = 1;
            return job;
        }

        private static IEnumerable<Thing> GetPersonaCaches(Pawn hauler, Thing stack)
        {
            var storages = hauler.Map.listerThings.ThingsOfDef(AC_DefOf.AC_PersonaCache)
                .Where(x => x.TryGetComp<CompPersonaCache>() is CompPersonaCache comp
                && comp.Accepts(stack) && hauler.CanReserveAndReach(x, PathEndMode.Touch, Danger.Deadly));
            Log.Message("storages: " + storages.ToStringSafeEnumerable() + " - stack: " + stack);
            return storages;
        }
    }
}


