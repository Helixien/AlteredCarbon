using Verse;

namespace AlteredCarbon
{
    public class Hediff_StackDegradation : HediffWithComps
    {
        public float stackDegradation;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref stackDegradation, "stackDegradation");
        }
    }
}