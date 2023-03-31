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
    public class SleevePreset : IExposable
    {
        public Pawn sleeve;
        public void ExposeData()
        {
            Scribe_Deep.Look(ref sleeve, "sleeve");
        }
    }

    [HotSwappable]
    public class AlteredCarbonSettings : ModSettings
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
            base.ExposeData();
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

        private Vector2 scrollPos;
        private float scrollHeight = 99999999;
        public void DoSettingsWindowContents(Rect inRect)
        {
            var viewRect = new Rect(inRect.x, inRect.y, inRect.width - 16, scrollHeight);
            scrollHeight = 0;
            Widgets.BeginScrollView(inRect, ref scrollPos, viewRect);
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(viewRect);
            var initY = listingStandard.curY;
            DoCategory(listingStandard, "AC.General".Translate());
            DoCheckbox(listingStandard, "AC.EnableStackSpawning".Translate(), ref enableStackSpawning, "AC.EnableStackSpawningDesc".Translate());
            DoCheckbox(listingStandard, "AC.EnableTechprintRequirement".Translate(), ref enableTechprintRequirement, "AC.EnableTechprintRequirementDesc".Translate());
            DoCategory(listingStandard, "AC.SleeveGrowing".Translate());
            DoSlider(listingStandard, "AC.TimeToGrowSleeveMultiplier".Translate(), ref sleeveGrowingTimeMultiplier,
                sleeveGrowingTimeMultiplier.ToStringPercent(), 0, 5f, "AC.TimeToGrowSleeveMultiplierDesc".Translate()); 
            DoSlider(listingStandard, "AC.CostToGrowSleeveMultiplier".Translate(), ref sleeveGrowingCostMultiplier,
                sleeveGrowingCostMultiplier.ToStringPercent(), 0, 5f, "AC.CostToGrowSleeveMultiplierDesc".Translate());
            DoCategory(listingStandard, "AC.StackRewriting".Translate());
            DoCheckbox(listingStandard, "AC.EnableStackDegradation".Translate(), ref enableStackDegradation, "AC.EnableStackDegradationDesc".Translate());
            DoSlider(listingStandard, "AC.StackRewriteEditTimeValueMultiplier".Translate(), ref stackRewriteEditTimeValueMultiplier,
                stackRewriteEditTimeValueMultiplier.ToStringPercent(), 0f, 5f, "AC.StackRewriteEditTimeValueMultiplierDesc".Translate());
            
            if (enableStackDegradation)
            {
                DoSlider(listingStandard, "AC.StackRewriteDegradationValueMultiplier".Translate(), ref stackRewriteDegradationValueMultiplier,
                    stackRewriteDegradationValueMultiplier.ToStringPercent(), 0f, 5f, "AC.StackRewriteDegradationValueMultiplierDesc".Translate());
            }
            DoCheckbox(listingStandard, "AC.EnableArchostackRewriting".Translate(), ref enableArchostackRewriting, "AC.EnableArchostackRewritingDesc".Translate());
            if (listingStandard.ButtonText("Reset".Translate()))
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
            listingStandard.End();
            Widgets.EndScrollView();
            scrollHeight = listingStandard.curY - initY;
        }

        private static void DoCategory(Listing_Standard listingStandard, string categoryName)
        {
            Text.Font = GameFont.Medium;
            listingStandard.Label(categoryName);
            Text.Font = GameFont.Small;
            listingStandard.GapLine(24);
        }

        private void DoCheckbox(Listing_Standard listingStandard, string optionLabel, ref bool field, string explanation)
        {
            listingStandard.CheckboxLabeled(optionLabel, ref field);
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
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            listingStandard.Gap();
        }

        private void DoSlider(Listing_Standard listingStandard, string label, ref int value, string valueLabel, int min, int max, string explanation)
        {
            Rect rect = listingStandard.GetRect(Text.LineHeight);
            Rect sliderRect = rect.RightPart(.60f).Rounded();
            Widgets.Label(rect, label);
            value = (int)Widgets.HorizontalSlider_NewTemp(sliderRect, (float)value, min, max, true, valueLabel);
            listingStandard.Gap(5);
            if (explanation.NullOrEmpty() is false)
            {
                DoExplanation(listingStandard, explanation);
            }
        }

        private void DoSlider(Listing_Standard listingStandard, string label, ref float value, string valueLabel, float min, float max, string explanation)
        {
            Rect rect = listingStandard.GetRect(Text.LineHeight);
            Rect sliderRect = rect.RightPart(.60f).Rounded();
            Widgets.Label(rect, label);
            value = Widgets.HorizontalSlider_NewTemp(sliderRect, (float)value, min, max, true, valueLabel);
            listingStandard.Gap(5);
            if (explanation.NullOrEmpty() is false)
            {
                DoExplanation(listingStandard, explanation);
            }
        }
    }
}
