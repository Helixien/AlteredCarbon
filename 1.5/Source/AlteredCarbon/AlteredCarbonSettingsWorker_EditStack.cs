using ModSettingsFramework;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    public class AlteredCarbonSettingsWorker_EditStack : PatchOperationWorker
    {
        public bool enableStackDegradation = true;
        public bool enableArchostackEditing = false;
        public float stackEditDegradationValueMultiplier = 1f;
        public float stackEditEditTimeValueMultiplier = 1f;

        public override void CopyFrom(PatchOperationWorker savedWorker)
        {
            var copy = savedWorker as AlteredCarbonSettingsWorker_EditStack;
            this.enableStackDegradation = copy.enableStackDegradation;
            this.enableArchostackEditing = copy.enableArchostackEditing;
            this.stackEditDegradationValueMultiplier = copy.stackEditDegradationValueMultiplier;
            this.stackEditEditTimeValueMultiplier = copy.stackEditEditTimeValueMultiplier;
        }

        public override void DoSettings(ModSettingsContainer container, Listing_Standard list)
        {
            DoCheckbox(list, "AC.EnableStackDegradation".Translate(), ref enableStackDegradation, "AC.EnableStackDegradationDesc".Translate());
            DoSlider(list, "AC.StackEditEditTimeValueMultiplier".Translate(), ref stackEditEditTimeValueMultiplier,
                stackEditEditTimeValueMultiplier.ToStringPercent(), 0f, 5f, "AC.StackEditEditTimeValueMultiplierDesc".Translate());
            if (enableStackDegradation)
            {
                DoSlider(list, "AC.StackEditDegradationValueMultiplier".Translate(), ref stackEditDegradationValueMultiplier,
                    stackEditDegradationValueMultiplier.ToStringPercent(), 0f, 5f, "AC.StackEditDegradationValueMultiplierDesc".Translate());
            }
            DoCheckbox(list, "AC.EnableArchostackEditing".Translate(), ref enableArchostackEditing, "AC.EnableArchostackEditingDesc".Translate());
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref enableArchostackEditing, "enableArchostackEditing", false);
            Scribe_Values.Look(ref enableStackDegradation, "enableStackDegradation", true);
            Scribe_Values.Look(ref stackEditDegradationValueMultiplier, "stackEditDegradationValueMultiplier", 1f);
            Scribe_Values.Look(ref stackEditEditTimeValueMultiplier, "stackEditEditTimeValueMultiplier", 1f);
        }

        public override void Reset()
        {
            enableStackDegradation = true;
            enableArchostackEditing = false;
            stackEditDegradationValueMultiplier = 1f;
            stackEditEditTimeValueMultiplier = 1f;
        }
    }
}
