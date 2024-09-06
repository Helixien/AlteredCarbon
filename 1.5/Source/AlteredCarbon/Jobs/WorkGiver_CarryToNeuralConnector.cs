using Verse;
using RimWorld;

namespace AlteredCarbon
{
    public class WorkGiver_CarryToNeuralConnector : WorkGiver_CarryToBuilding
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(AC_DefOf.AC_NeuralConnector);

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            return !ModsConfig.BiotechActive;
        }
    }
}
