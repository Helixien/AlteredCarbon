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
    public class AlteredCarbonSettingsWorker_General : PatchOperationWorker
    {
        public bool enableStackSpawning = true;
        public bool enableTechprintRequirement = true;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref enableStackSpawning, "enableStackSpawning", true);
            Scribe_Values.Look(ref enableTechprintRequirement, "enableTechprintRequirement", true);
        }

        public override void CopyFrom(PatchOperationWorker savedWorker)
        {
            var copy = savedWorker as AlteredCarbonSettingsWorker_General;
            this.enableStackSpawning = copy.enableStackSpawning;
            this.enableTechprintRequirement = copy.enableTechprintRequirement;
        }

        public override void DoSettings(ModSettingsContainer container, Listing_Standard list)
        {
            DoCheckbox(list, "AC.EnableStackSpawning".Translate(), ref enableStackSpawning, "AC.EnableStackSpawningDesc".Translate());
            DoCheckbox(list, "AC.EnableTechprintRequirement".Translate(), ref enableTechprintRequirement, "AC.EnableTechprintRequirementDesc".Translate());
        }

        public override void Reset()
        {
            enableStackSpawning = true;
            enableTechprintRequirement = true;
        }

        public override void ApplySettings()
        {
            base.ApplySettings();
            ACUtils.ApplySettings();
        }
    }
}
