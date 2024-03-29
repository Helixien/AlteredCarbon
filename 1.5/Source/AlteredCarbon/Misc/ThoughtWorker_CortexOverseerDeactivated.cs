using RimWorld;
using System.Linq;
using Verse;

namespace AlteredCarbon
{
    public class ThoughtWorker_CortexOverseerDeactivated : ThoughtWorker
    {
        public override ThoughtState CurrentStateInternal(Pawn p)
        {
            var hediff = p.health.hediffSet.hediffs.OfType<Hediff_CortexOverseer>().FirstOrDefault();
            if (hediff != null && hediff.activated is false)
            {
                return ThoughtState.ActiveDefault;
            }
            return ThoughtState.Inactive;
        }
    }
}

