using ModSettingsFramework;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    public class AlteredCarbonSettingsWorker_SleeveGrowing : PatchOperationWorker
    {
        public float sleeveGrowingTimeMultiplier = 1f;
        public float sleeveGrowingCostMultiplier = 1f;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref sleeveGrowingTimeMultiplier, "sleeveGrowingTimeMultiplier", 1f);
            Scribe_Values.Look(ref sleeveGrowingCostMultiplier, "sleeveGrowingCostMultiplier", 1f);
        }

        public override void CopyFrom(PatchOperationWorker savedWorker)
        {
            var copy = savedWorker as AlteredCarbonSettingsWorker_SleeveGrowing;
            this.sleeveGrowingTimeMultiplier = copy.sleeveGrowingTimeMultiplier;
            this.sleeveGrowingCostMultiplier = copy.sleeveGrowingCostMultiplier;
        }

        public override void DoSettings(ModSettingsContainer container, Listing_Standard list)
        {
            DoSlider(list, "AC.TimeToGrowSleeveMultiplier".Translate(), ref sleeveGrowingTimeMultiplier,
                sleeveGrowingTimeMultiplier.ToStringPercent(), 0, 5f, "AC.TimeToGrowSleeveMultiplierDesc".Translate());
            DoSlider(list, "AC.CostToGrowSleeveMultiplier".Translate(), ref sleeveGrowingCostMultiplier,
                sleeveGrowingCostMultiplier.ToStringPercent(), 0, 5f, "AC.CostToGrowSleeveMultiplierDesc".Translate());
        }

        public override void Reset()
        {
            sleeveGrowingTimeMultiplier = 1f;
            sleeveGrowingCostMultiplier = 1f;
        }
    }
}
