using Verse;
using Verse.AI;
using RimWorld;
using UnityEngine;

namespace AlteredCarbon
{
    public class WorkGiver_HaulToPersonaConnector : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(AC_DefOf.AC_PersonaConnector);

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (t.IsForbidden(pawn))
            {
                return false;
            }
            if (!(t is Building_PersonaConnector { State: SubcoreScannerState.WaitingForIngredients } Building_PersonaConnector))
            {
                return false;
            }
            if (!pawn.CanReserve(t, 1, -1, null, forced))
            {
                return false;
            }
            return FindIngredients(pawn, Building_PersonaConnector).Thing != null;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Building_PersonaConnector { State: SubcoreScannerState.WaitingForIngredients } Building_PersonaConnector))
            {
                return null;
            }
            ThingCount thingCount = FindIngredients(pawn, Building_PersonaConnector);
            if (thingCount.Thing != null)
            {
                Job job = HaulAIUtility.HaulToContainerJob(pawn, thingCount.Thing, t);
                job.count = Mathf.Min(job.count, thingCount.Count);
                return job;
            }
            return null;
        }

        private ThingCount FindIngredients(Pawn pawn, Building_PersonaConnector scanner)
        {
            Thing thing = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.HaulableEver), PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, Validator);
            if (thing == null)
            {
                return default(ThingCount);
            }
            int requiredCountOf = scanner.GetRequiredCountOf(thing.def);
            return new ThingCount(thing, Mathf.Min(thing.stackCount, requiredCountOf));
            bool Validator(Thing x)
            {
                if (x.IsForbidden(pawn) || !pawn.CanReserve(x))
                {
                    return false;
                }
                return scanner.CanAcceptIngredient(x);
            }
        }
    }
}
