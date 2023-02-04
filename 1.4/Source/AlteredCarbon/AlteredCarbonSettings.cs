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
        public int baseGrowingTimeDuration = 900000;

        public static int editTimeOffsetPerNameChange = 2000;
        public static int editTimeOffsetPerGenderChange = 5000;
        public static int editTimeOffsetPerSkillLevelChange = 250;
        public static int editTimeOffsetPerSkillPassionChange = 1000;
        public static int editTimeOffsetPerTraitChange = 1200;
        public static int editTimeOffsetPerChildhoodChange = 5000;
        public static int editTimeOffsetPerAdulthoodChange = 5000;
        public static int editTimeOffsetPerIdeologyChange = 2500;
        public static int editTimeOffsetPerCertaintyChange = 50;
        public static int editTimeOffsetPerFactionChange = 2500;
        public static int editTimeOffsetPerUnwaveringLoyalChange = 1200;

        public static float stackDegradationOffsetPerNameChange = 0.25f;
        public static float stackDegradationOffsetPerGenderChange = 0.5f;
        public static float stackDegradationOffsetPerSkillLevelChange = 0.03f;
        public static float stackDegradationOffsetPerSkillPassionChange = 0.1f;
        public static float stackDegradationOffsetPerTraitChange = 0.15f;
        public static float stackDegradationOffsetPerChildhoodChange = 0.5f;
        public static float stackDegradationOffsetPerAdulthoodChange = 0.5f;
        public static float stackDegradationOffsetPerIdeologyChange = 0.25f;
        public static float stackDegradationOffsetPerCertaintyChange = 0.01f;
        public static float stackDegradationOffsetPerFactionChange = 0.25f;
        public static float stackDegradationOffsetPerUnwaveringLoyalChange = 0.25f;


        public Dictionary<string, SleevePreset> presets = new Dictionary<string, SleevePreset>();
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref baseGrowingTimeDuration, "baseGrowingTimeDuration", 900000);

            Scribe_Values.Look(ref editTimeOffsetPerNameChange, "editTimeOffsetPerNameChange", 2000);
            Scribe_Values.Look(ref editTimeOffsetPerGenderChange, "editTimeOffsetPerGenderChange", 5000);
            Scribe_Values.Look(ref editTimeOffsetPerSkillLevelChange, "editTimeOffsetPerSkillLevelChange", 250);
            Scribe_Values.Look(ref editTimeOffsetPerSkillPassionChange, "editTimeOffsetPerSkillPassionChange", 1000);
            Scribe_Values.Look(ref editTimeOffsetPerTraitChange, "editTimeOffsetPerTraitChange", 1200);
            Scribe_Values.Look(ref editTimeOffsetPerChildhoodChange, "editTimeOffsetPerChildhoodChange", 5000);
            Scribe_Values.Look(ref editTimeOffsetPerAdulthoodChange, "editTimeOffsetPerAdulthoodChange", 5000);
            Scribe_Values.Look(ref editTimeOffsetPerIdeologyChange, "editTimeOffsetPerIdeologyChange", 2500);
            Scribe_Values.Look(ref editTimeOffsetPerCertaintyChange, "editTimeOffsetPerCertaintyChange", 50);
            Scribe_Values.Look(ref editTimeOffsetPerFactionChange, "editTimeOffsetPerFactionChange", 2500);
            Scribe_Values.Look(ref editTimeOffsetPerUnwaveringLoyalChange, "editTimeOffsetPerUnwaveringLoyalChange", 1200);

            Scribe_Values.Look(ref stackDegradationOffsetPerNameChange, "stackDegradationOffsetPerNameChange", 0.25f);
            Scribe_Values.Look(ref stackDegradationOffsetPerGenderChange, "stackDegradationOffsetPerGenderChange", 0.5f);
            Scribe_Values.Look(ref stackDegradationOffsetPerSkillLevelChange, "stackDegradationOffsetPerSkillLevelChange", 0.05f);
            Scribe_Values.Look(ref stackDegradationOffsetPerSkillPassionChange, "stackDegradationOffsetPerSkillPassionChange", 0.1f);
            Scribe_Values.Look(ref stackDegradationOffsetPerTraitChange, "stackDegradationOffsetPerTraitChange", 0.15f);
            Scribe_Values.Look(ref stackDegradationOffsetPerChildhoodChange, "stackDegradationOffsetPerChildhoodChange", 0.5f);
            Scribe_Values.Look(ref stackDegradationOffsetPerAdulthoodChange, "stackDegradationOffsetPerAdulthoodChange", 0.5f);
            Scribe_Values.Look(ref stackDegradationOffsetPerIdeologyChange, "stackDegradationOffsetPerIdeologyChange", 0.25f);
            Scribe_Values.Look(ref stackDegradationOffsetPerCertaintyChange, "stackDegradationOffsetPerCertaintyChange", 0.01f);
            Scribe_Values.Look(ref stackDegradationOffsetPerFactionChange, "stackDegradationOffsetPerFactionChange", 0.25f);
            Scribe_Values.Look(ref stackDegradationOffsetPerUnwaveringLoyalChange, "stackDegradationOffsetPerUnwaveringLoyalChange", 0.25f);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (presets is null) 
                    presets = new Dictionary<string, SleevePreset>();
            }
        }
        public void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            DoSlider(listingStandard, "AC.GrowingTimeDuration".Translate(), ref baseGrowingTimeDuration, baseGrowingTimeDuration.ToStringTicksToPeriod(), 1000, 9000000);
            DoSlider(listingStandard, "AC.editTimeOffsetPerNameChange".Translate(), ref editTimeOffsetPerNameChange, editTimeOffsetPerNameChange.ToStringTicksToPeriod(), 0, 60000);
            DoSlider(listingStandard, "AC.editTimeOffsetPerGenderChange".Translate(), ref editTimeOffsetPerGenderChange, editTimeOffsetPerGenderChange.ToStringTicksToPeriod(), 0, 60000);
            DoSlider(listingStandard, "AC.editTimeOffsetPerSkillLevelChange".Translate(), ref editTimeOffsetPerSkillLevelChange, editTimeOffsetPerSkillLevelChange.ToStringTicksToPeriod(), 0, 60000);
            DoSlider(listingStandard, "AC.editTimeOffsetPerSkillPassionChange".Translate(), ref editTimeOffsetPerSkillPassionChange, editTimeOffsetPerSkillPassionChange.ToStringTicksToPeriod(), 0, 60000);
            DoSlider(listingStandard, "AC.editTimeOffsetPerTraitChange".Translate(), ref editTimeOffsetPerTraitChange, editTimeOffsetPerTraitChange.ToStringTicksToPeriod(), 0, 60000);
            DoSlider(listingStandard, "AC.editTimeOffsetPerChildhoodChange".Translate(), ref editTimeOffsetPerChildhoodChange, editTimeOffsetPerChildhoodChange.ToStringTicksToPeriod(), 0, 60000);
            DoSlider(listingStandard, "AC.editTimeOffsetPerAdulthoodChange".Translate(), ref editTimeOffsetPerAdulthoodChange, editTimeOffsetPerAdulthoodChange.ToStringTicksToPeriod(), 0, 60000);
            DoSlider(listingStandard, "AC.editTimeOffsetPerIdeologyChange".Translate(), ref editTimeOffsetPerIdeologyChange, editTimeOffsetPerIdeologyChange.ToStringTicksToPeriod(), 0, 60000);
            DoSlider(listingStandard, "AC.editTimeOffsetPerCertaintyChange".Translate(), ref editTimeOffsetPerCertaintyChange, editTimeOffsetPerCertaintyChange.ToStringTicksToPeriod(), 0, 60000);
            DoSlider(listingStandard, "AC.editTimeOffsetPerFactionChange".Translate(), ref editTimeOffsetPerFactionChange, editTimeOffsetPerFactionChange.ToStringTicksToPeriod(), 0, 60000);
            DoSlider(listingStandard, "AC.editTimeOffsetPerUnwaveringLoyalChange".Translate(), ref editTimeOffsetPerUnwaveringLoyalChange, editTimeOffsetPerUnwaveringLoyalChange.ToStringTicksToPeriod(), 0, 60000);
            
            DoSlider(listingStandard, "AC.stackDegradationOffsetPerNameChange".Translate(), ref stackDegradationOffsetPerNameChange, stackDegradationOffsetPerNameChange.ToStringPercent(), 0f, 1f);
            DoSlider(listingStandard, "AC.stackDegradationOffsetPerGenderChange".Translate(), ref stackDegradationOffsetPerGenderChange, stackDegradationOffsetPerGenderChange.ToStringPercent(), 0f, 1f);
            DoSlider(listingStandard, "AC.stackDegradationOffsetPerSkillLevelChange".Translate(), ref stackDegradationOffsetPerSkillLevelChange, stackDegradationOffsetPerSkillLevelChange.ToStringPercent(), 0f, 1f);
            DoSlider(listingStandard, "AC.stackDegradationOffsetPerSkillPassionChange".Translate(), ref stackDegradationOffsetPerSkillPassionChange, stackDegradationOffsetPerSkillPassionChange.ToStringPercent(), 0f, 1f);
            DoSlider(listingStandard, "AC.stackDegradationOffsetPerTraitChange".Translate(), ref stackDegradationOffsetPerTraitChange, stackDegradationOffsetPerTraitChange.ToStringPercent(), 0f, 1f);
            DoSlider(listingStandard, "AC.stackDegradationOffsetPerChildhoodChange".Translate(), ref stackDegradationOffsetPerChildhoodChange, stackDegradationOffsetPerChildhoodChange.ToStringPercent(), 0f, 1f);
            DoSlider(listingStandard, "AC.stackDegradationOffsetPerAdulthoodChange".Translate(), ref stackDegradationOffsetPerAdulthoodChange, stackDegradationOffsetPerAdulthoodChange.ToStringPercent(), 0f, 1f);
            DoSlider(listingStandard, "AC.stackDegradationOffsetPerIdeologyChange".Translate(), ref stackDegradationOffsetPerIdeologyChange, stackDegradationOffsetPerIdeologyChange.ToStringPercent(), 0f, 1f);
            DoSlider(listingStandard, "AC.stackDegradationOffsetPerCertaintyChange".Translate(), ref stackDegradationOffsetPerCertaintyChange, stackDegradationOffsetPerCertaintyChange.ToStringPercent(), 0f, 1f);
            DoSlider(listingStandard, "AC.stackDegradationOffsetPerFactionChange".Translate(), ref stackDegradationOffsetPerFactionChange, stackDegradationOffsetPerFactionChange.ToStringPercent(), 0f, 1f);
            DoSlider(listingStandard, "AC.stackDegradationOffsetPerUnwaveringLoyalChange".Translate(), ref stackDegradationOffsetPerUnwaveringLoyalChange, stackDegradationOffsetPerFactionChange.ToStringPercent(), 0f, 1f);
            listingStandard.Gap(10);
            if (listingStandard.ButtonText("Reset".Translate()))
            {
                baseGrowingTimeDuration = 900000;

                editTimeOffsetPerNameChange = 2000;
                editTimeOffsetPerGenderChange = 5000;
                editTimeOffsetPerSkillLevelChange = 250;
                editTimeOffsetPerSkillPassionChange = 1000;
                editTimeOffsetPerTraitChange = 1200;
                editTimeOffsetPerChildhoodChange = 5000;
                editTimeOffsetPerAdulthoodChange = 5000;
                editTimeOffsetPerIdeologyChange = 2500;
                editTimeOffsetPerCertaintyChange = 50;
                editTimeOffsetPerFactionChange = 2500;

                stackDegradationOffsetPerNameChange = 0.25f;
                stackDegradationOffsetPerGenderChange = 0.5f;
                stackDegradationOffsetPerSkillLevelChange = 0.03f;
                stackDegradationOffsetPerSkillPassionChange = 0.1f;
                stackDegradationOffsetPerTraitChange = 0.15f;
                stackDegradationOffsetPerChildhoodChange = 0.5f;
                stackDegradationOffsetPerAdulthoodChange = 0.5f;
                stackDegradationOffsetPerIdeologyChange = 0.25f;
                stackDegradationOffsetPerCertaintyChange = 0.01f;
                stackDegradationOffsetPerFactionChange = 0.25f;
            }
            listingStandard.End();
        }

        private void DoSlider(Listing_Standard listingStandard, string label, ref int value, string valueLabel, int min, int max)
        {
            Rect rect = listingStandard.GetRect(Text.LineHeight);
            Rect sliderRect = rect.RightPart(.60f).Rounded();
            Widgets.Label(rect, label);
            value = (int)Widgets.HorizontalSlider_NewTemp(sliderRect, (float)value, min, max, true, valueLabel);
            listingStandard.Gap(5);

        }

        private void DoSlider(Listing_Standard listingStandard, string label, ref float value, string valueLabel, float min, float max)
        {
            Rect rect = listingStandard.GetRect(Text.LineHeight);
            Rect sliderRect = rect.RightPart(.60f).Rounded();
            Widgets.Label(rect, label);
            value = (float)Widgets.HorizontalSlider_NewTemp(sliderRect, (float)value, min, max, true, valueLabel);
            listingStandard.Gap(5);
        }
    }
}
