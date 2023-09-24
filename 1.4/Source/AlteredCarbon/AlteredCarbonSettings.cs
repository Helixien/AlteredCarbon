using ModSettingsFramework;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    public class AlteredCarbonSettingsWorker : PatchOperationWorker
    {
        public float sleeveGrowingTimeMultiplier = 1f;
        public float sleeveGrowingCostMultiplier = 1f;
        public bool enableStackSpawning = true;
        public bool enableStackDegradation = true;
        public bool enableArchostackRewriting = false;
        public float stackRewriteDegradationValueMultiplier = 1f;
        public float stackRewriteEditTimeValueMultiplier = 1f;
        public bool enableTechprintRequirement = true;
        public Dictionary<string, SleevePreset> presets = new Dictionary<string, SleevePreset>();

        public override void ExposeData()
        {
            Scribe_Values.Look(ref sleeveGrowingTimeMultiplier, "sleeveGrowingTimeMultiplier", 1f);
            Scribe_Values.Look(ref sleeveGrowingCostMultiplier, "sleeveGrowingCostMultiplier", 1f);
            Scribe_Values.Look(ref enableStackSpawning, "enableStackSpawning", true);
            Scribe_Values.Look(ref enableStackDegradation, "enableStackDegradation", true);
            Scribe_Values.Look(ref enableTechprintRequirement, "enableTechprintRequirement", true);
            Scribe_Values.Look(ref enableArchostackRewriting, "enableArchostackRewriting", false);
            Scribe_Values.Look(ref stackRewriteDegradationValueMultiplier, "stackRewriteDegradationValueMultiplier", 1f);
            Scribe_Values.Look(ref stackRewriteEditTimeValueMultiplier, "stackRewriteEditTimeValueMultiplier", 1f);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (presets is null)
                    presets = new Dictionary<string, SleevePreset>();
            }
        }

        public override void CopyFrom(PatchOperationWorker savedWorker)
        {
            var copy = savedWorker as AlteredCarbonSettingsWorker;
            this.sleeveGrowingTimeMultiplier = copy.sleeveGrowingTimeMultiplier;
            this.sleeveGrowingCostMultiplier = copy.sleeveGrowingCostMultiplier;
            this.enableStackSpawning = copy.enableStackSpawning;
            this.enableStackDegradation = copy.enableStackDegradation;
            this.enableArchostackRewriting = copy.enableArchostackRewriting;
            this.stackRewriteDegradationValueMultiplier = copy.stackRewriteDegradationValueMultiplier;
            this.stackRewriteEditTimeValueMultiplier = copy.stackRewriteEditTimeValueMultiplier;
            this.enableTechprintRequirement = copy.enableTechprintRequirement;
            this.presets = copy.presets.ToDictionary(entry => entry.Key, entry => entry.Value);
        }

        private float scrollHeight = 99999999;

        public override void DoSettings(ModSettingsContainer container, Listing_Standard list)
        {
            scrollHeight = 0;
            DoCategory(list, "AC.General".Translate());
            DoCheckbox(list, "AC.EnableStackSpawning".Translate(), ref enableStackSpawning, "AC.EnableStackSpawningDesc".Translate());
            DoCheckbox(list, "AC.EnableTechprintRequirement".Translate(), ref enableTechprintRequirement, "AC.EnableTechprintRequirementDesc".Translate());
            DoSlider(list, "AC.TimeToGrowSleeveMultiplier".Translate(), ref sleeveGrowingTimeMultiplier,
                sleeveGrowingTimeMultiplier.ToStringPercent(), 0, 5f, "AC.TimeToGrowSleeveMultiplierDesc".Translate());
            DoSlider(list, "AC.CostToGrowSleeveMultiplier".Translate(), ref sleeveGrowingCostMultiplier,
                sleeveGrowingCostMultiplier.ToStringPercent(), 0, 5f, "AC.CostToGrowSleeveMultiplierDesc".Translate());
            DoCategory(list, "AC.StackRewriting".Translate());
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

        public override void Reset()
        {
            sleeveGrowingTimeMultiplier = 1f;
            sleeveGrowingCostMultiplier = 1f;
            enableStackSpawning = true;
            enableStackDegradation = true;
            enableArchostackRewriting = false;
            stackRewriteDegradationValueMultiplier = 1f;
            stackRewriteEditTimeValueMultiplier = 1f;
            enableTechprintRequirement = true;
        }

        public override int SettingsHeight()
        {
            return (int)scrollHeight;
        }

        private void DoCategory(Listing_Standard listingStandard, string categoryName)
        {
            Text.Font = GameFont.Medium;
            listingStandard.Label(categoryName);
            scrollHeight += 24;
            Text.Font = GameFont.Small;
            listingStandard.GapLine(24); 
            scrollHeight += 24;
        }

        private void DoCheckbox(Listing_Standard listingStandard, string optionLabel, ref bool field, string explanation)
        {
            listingStandard.CheckboxLabeled(optionLabel, ref field);
            scrollHeight += 24;
            if (explanation.NullOrEmpty() is false)
            {
                DoExplanation(listingStandard, explanation);
            }
        }

        private void DoExplanation(Listing_Standard listingStandard, string explanation)
        {
            Text.Font = GameFont.Tiny;
            GUI.color = Color.grey;
            listingStandard.Label(explanation);
            scrollHeight += 24;
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            listingStandard.Gap();
            scrollHeight += 12;
        }

        private void DoSlider(Listing_Standard listingStandard, string label, ref float value, string valueLabel, float min, float max, string explanation)
        {
            Rect rect = listingStandard.GetRect(Text.LineHeight);
            Rect sliderRect = rect.RightPart(.60f).Rounded();
            Widgets.Label(rect, label);
            scrollHeight += 24;
            value = Widgets.HorizontalSlider_NewTemp(sliderRect, (float)value, min, max, true, valueLabel);
            listingStandard.Gap(5);
            scrollHeight += 5;
            if (explanation.NullOrEmpty() is false)
            {
                DoExplanation(listingStandard, explanation);
            }
        }

        public override void ApplySettings()
        {
            base.ApplySettings();
            ACUtils.ApplySettings();
        }
    }
}
