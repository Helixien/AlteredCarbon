using ModSettingsFramework;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    public class AlteredCarbonSettingsWorker_RewriteStack : AlteredCarbonSettingsWorkerBase
    {
        public bool enableStackDegradation = true;
        public bool enableArchostackRewriting = false;
        public float stackRewriteDegradationValueMultiplier = 1f;
        public float stackRewriteEditTimeValueMultiplier = 1f;

        public override void CopyFrom(PatchOperationWorker savedWorker)
        {
            var copy = savedWorker as AlteredCarbonSettingsWorker_RewriteStack;
            this.enableStackDegradation = copy.enableStackDegradation;
            this.enableArchostackRewriting = copy.enableArchostackRewriting;
            this.stackRewriteDegradationValueMultiplier = copy.stackRewriteDegradationValueMultiplier;
            this.stackRewriteEditTimeValueMultiplier = copy.stackRewriteEditTimeValueMultiplier;
        }

        public override void DoSettings(ModSettingsContainer container, Listing_Standard list)
        {
            scrollHeight = 0;
            DoCheckbox(list, "AC.EnableStackDegradation".Translate(), ref enableStackDegradation, "AC.EnableStackDegradationDesc".Translate());
            DoSlider(list, "AC.StackRewriteEditTimeValueMultiplier".Translate(), ref stackRewriteEditTimeValueMultiplier,
                stackRewriteEditTimeValueMultiplier.ToStringPercent(), 0f, 5f, "AC.StackRewriteEditTimeValueMultiplierDesc".Translate());
            if (enableStackDegradation)
            {
                DoSlider(list, "AC.StackRewriteDegradationValueMultiplier".Translate(), ref stackRewriteDegradationValueMultiplier,
                    stackRewriteDegradationValueMultiplier.ToStringPercent(), 0f, 5f, "AC.StackRewriteDegradationValueMultiplierDesc".Translate());
            }
            DoCheckbox(list, "AC.EnableArchostackRewriting".Translate(), ref enableArchostackRewriting, "AC.EnableArchostackRewritingDesc".Translate());
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref enableArchostackRewriting, "enableArchostackRewriting", false);
            Scribe_Values.Look(ref enableStackDegradation, "enableStackDegradation", true);
            Scribe_Values.Look(ref stackRewriteDegradationValueMultiplier, "stackRewriteDegradationValueMultiplier", 1f);
            Scribe_Values.Look(ref stackRewriteEditTimeValueMultiplier, "stackRewriteEditTimeValueMultiplier", 1f);
        }

        public override void Reset()
        {
            enableStackDegradation = true;
            enableArchostackRewriting = false;
            stackRewriteDegradationValueMultiplier = 1f;
            stackRewriteEditTimeValueMultiplier = 1f;
        }

        public override int SettingsHeight()
        {
            return (int)scrollHeight;
        }
    }
}
