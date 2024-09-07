using Verse;
using RimWorld;
using Verse.AI;

namespace AlteredCarbon
{
    public class WorkGiver_EnterNeuralConnector : WorkGiver_EnterBuilding
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(AC_DefOf.AC_NeuralConnector);

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return !ModsConfig.BiotechActive;
        }
    }
}
