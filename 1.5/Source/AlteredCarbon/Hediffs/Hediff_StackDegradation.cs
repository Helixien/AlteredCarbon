using RimWorld;
using Verse;

namespace AlteredCarbon
{

    public class Hediff_StackDegradation : HediffWithComps
    {
        public float stackDegradation;
        public override string Label => base.Label + " (" + stackDegradation.ToStringPercent() + ")";
        public override bool ShouldRemove => stackDegradation <= 0;
        public override void Tick()
        {
            base.Tick();
            stackDegradation -= 0.01f / GenDate.TicksPerDay;
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref stackDegradation, "hediff");
        }
    }
}