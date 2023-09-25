using ModSettingsFramework;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    public abstract class AlteredCarbonSettingsWorkerBase : PatchOperationWorker
    {
        protected float scrollHeight = 99999999;

        protected void DoCheckbox(Listing_Standard listingStandard, string optionLabel, ref bool field, string explanation)
        {
            listingStandard.CheckboxLabeled(optionLabel, ref field);
            scrollHeight += 24;
            if (explanation.NullOrEmpty() is false)
            {
                DoExplanation(listingStandard, explanation);
            }
        }

        protected void DoExplanation(Listing_Standard listingStandard, string explanation)
        {
            Text.Font = GameFont.Tiny;
            GUI.color = Color.grey;
            var rect = listingStandard.Label(explanation);
            scrollHeight += rect.height;
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            listingStandard.Gap();
            scrollHeight += 12;
        }

        protected void DoSlider(Listing_Standard listingStandard, string label, ref float value, string valueLabel, float min, float max, string explanation)
        {
            Rect rect = listingStandard.GetRect(Text.LineHeight);
            Rect sliderRect = rect.RightPart(.60f).Rounded();
            Widgets.Label(rect, label);
            scrollHeight += rect.height;
            value = Widgets.HorizontalSlider_NewTemp(sliderRect, (float)value, min, max, true, valueLabel);
            listingStandard.Gap(5);
            scrollHeight += 5;
            if (explanation.NullOrEmpty() is false)
            {
                DoExplanation(listingStandard, explanation);
            }
        }
    }
}
