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

    public class AlteredCarbonSettings : ModSettings
    {
        public int baseGrowingTimeDuration = 900000;
        public int baseBeautyDuration = 105000;
        public int baseQualityDurationTime = 210000;
        public Dictionary<string, SleevePreset> presets = new Dictionary<string, SleevePreset>();
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref baseGrowingTimeDuration, "baseGrowingTimeDuration", 900000);
            Scribe_Values.Look(ref baseBeautyDuration, "baseBeautyLevel", 105000);
            Scribe_Values.Look(ref baseQualityDurationTime, "baseQualityLevel", 210000);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (presets is null) presets = new Dictionary<string, SleevePreset>();
            }
        }
        public void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);
            Rect rect = listingStandard.GetRect(Text.LineHeight);
            rect.y += 5f;
            Rect rect2 = rect.RightPart(.70f).Rounded();
            Widgets.Label(rect, "AC.GrowingTimeDuration".Translate());
            baseGrowingTimeDuration = (int)Widgets.HorizontalSlider(rect2, baseGrowingTimeDuration, 1000, 9000000, true, baseGrowingTimeDuration.ToStringTicksToPeriod());
            listingStandard.Gap(listingStandard.verticalSpacing);
            if (listingStandard.ButtonText("Reset".Translate()))
            {
                baseGrowingTimeDuration = 900000;
                baseBeautyDuration = 105000;
                baseQualityDurationTime = 210000;
            }
            listingStandard.End();
        }

    }
}
