using ModSettingsFramework;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AlteredCarbon
{
    public class AlteredCarbonSettingsWorker_General : AlteredCarbonSettingsWorkerBase
    {
        public float sleeveGrowingTimeMultiplier = 1f;
        public float sleeveGrowingCostMultiplier = 1f;
        public bool enableStackSpawning = true;
        public bool enableTechprintRequirement = true;
        public Dictionary<string, SleevePreset> presets = new Dictionary<string, SleevePreset>();

        public override void ExposeData()
        {
            Scribe_Values.Look(ref sleeveGrowingTimeMultiplier, "sleeveGrowingTimeMultiplier", 1f);
            Scribe_Values.Look(ref sleeveGrowingCostMultiplier, "sleeveGrowingCostMultiplier", 1f);
            Scribe_Values.Look(ref enableStackSpawning, "enableStackSpawning", true);
            Scribe_Values.Look(ref enableTechprintRequirement, "enableTechprintRequirement", true);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (presets is null)
                    presets = new Dictionary<string, SleevePreset>();
            }
        }

        public override void CopyFrom(PatchOperationWorker savedWorker)
        {
            var copy = savedWorker as AlteredCarbonSettingsWorker_General;
            this.sleeveGrowingTimeMultiplier = copy.sleeveGrowingTimeMultiplier;
            this.sleeveGrowingCostMultiplier = copy.sleeveGrowingCostMultiplier;
            this.enableStackSpawning = copy.enableStackSpawning;
            this.enableTechprintRequirement = copy.enableTechprintRequirement;
            this.presets = copy.presets.ToDictionary(entry => entry.Key, entry => entry.Value);
        }


        public override void DoSettings(ModSettingsContainer container, Listing_Standard list)
        {
            scrollHeight = 0;
            DoCheckbox(list, "AC.EnableStackSpawning".Translate(), ref enableStackSpawning, "AC.EnableStackSpawningDesc".Translate());
            DoCheckbox(list, "AC.EnableTechprintRequirement".Translate(), ref enableTechprintRequirement, "AC.EnableTechprintRequirementDesc".Translate());
            DoSlider(list, "AC.TimeToGrowSleeveMultiplier".Translate(), ref sleeveGrowingTimeMultiplier,
                sleeveGrowingTimeMultiplier.ToStringPercent(), 0, 5f, "AC.TimeToGrowSleeveMultiplierDesc".Translate());
            DoSlider(list, "AC.CostToGrowSleeveMultiplier".Translate(), ref sleeveGrowingCostMultiplier,
                sleeveGrowingCostMultiplier.ToStringPercent(), 0, 5f, "AC.CostToGrowSleeveMultiplierDesc".Translate());
        }

        public override void Reset()
        {
            sleeveGrowingTimeMultiplier = 1f;
            sleeveGrowingCostMultiplier = 1f;
            enableStackSpawning = true;
            enableTechprintRequirement = true;
        }

        public override int SettingsHeight()
        {
            return (int)scrollHeight;
        }

        public override void ApplySettings()
        {
            base.ApplySettings();
            ACUtils.ApplySettings();
        }
    }
}
