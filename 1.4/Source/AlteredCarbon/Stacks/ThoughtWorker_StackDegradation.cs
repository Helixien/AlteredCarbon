using RimWorld;
using Verse;

namespace AlteredCarbon
{
    public class ThoughtWorker_StackDegradation : ThoughtWorker
    {
        public override ThoughtState CurrentStateInternal(Pawn p)
        {
            var hediff = p.health.hediffSet.GetFirstHediffOfDef(def.hediff);
            if (hediff == null)
            {
                return ThoughtState.Inactive;
            }
            return ThoughtState.ActiveDefault;
        }

        public override float MoodMultiplier(Pawn p)
        {
            var hediff = p.health.hediffSet.GetFirstHediffOfDef(def.hediff) as Hediff_StackDegradation;
            return hediff.stackDegradation;
        }
    }
}