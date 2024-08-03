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
        public bool sleeveDeathDoesNotCauseGearTainting = true;
        public bool enableSoldPersonaPrintsCreatingPawnDuplicates = true;
        public bool enablePersonaPrintsInAncientDangers = true;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref enableStackSpawning, "enableStackSpawning", true);
            Scribe_Values.Look(ref enableTechprintRequirement, "enableTechprintRequirement", true);
            Scribe_Values.Look(ref sleeveDeathDoesNotCauseGearTainting, "sleeveDeathDoesNotCauseGearTainting", true);
            Scribe_Values.Look(ref enableSoldPersonaPrintsCreatingPawnDuplicates, "enableSoldPersonaPrintsCreatingPawnDuplicates", true);
            Scribe_Values.Look(ref enablePersonaPrintsInAncientDangers, "enablePersonaPrintsInAncientDangers", true);
        }

        public override void CopyFrom(PatchOperationWorker savedWorker)
        {
            var copy = savedWorker as AlteredCarbonSettingsWorker_General;
            this.enableStackSpawning = copy.enableStackSpawning;
            this.enableTechprintRequirement = copy.enableTechprintRequirement;
            this.sleeveDeathDoesNotCauseGearTainting = copy.sleeveDeathDoesNotCauseGearTainting;
            this.enableSoldPersonaPrintsCreatingPawnDuplicates = copy.enableSoldPersonaPrintsCreatingPawnDuplicates;
            this.enablePersonaPrintsInAncientDangers = copy.enablePersonaPrintsInAncientDangers;
        }

        public override void DoSettings(ModSettingsContainer container, Listing_Standard list)
        {
            DoCheckbox(list, "AC.EnableStackSpawning".Translate(), ref enableStackSpawning, "AC.EnableStackSpawningDesc".Translate());
            DoCheckbox(list, "AC.EnableTechprintRequirement".Translate(), ref enableTechprintRequirement, "AC.EnableTechprintRequirementDesc".Translate());
            DoCheckbox(list, "AC.SleeveDeathDoesNotCauseGearTainting".Translate(), ref sleeveDeathDoesNotCauseGearTainting, null);
            DoCheckbox(list, "AC.EnableSoldPersonaPrintsCreatingPawnDuplicates".Translate(), ref enableSoldPersonaPrintsCreatingPawnDuplicates, "AC.EnableSoldPersonaPrintsCreatingPawnDuplicatesDesc".Translate());
            DoCheckbox(list, "AC.EnablePersonaPrintsInAncientDangers".Translate(), ref enablePersonaPrintsInAncientDangers, "AC.EnablePersonaPrintsInAncientDangersDesc".Translate());
        }

        public override void Reset()
        {
            enableStackSpawning = true;
            enableTechprintRequirement = true;
            sleeveDeathDoesNotCauseGearTainting = true;
            enableSoldPersonaPrintsCreatingPawnDuplicates = true;
            enablePersonaPrintsInAncientDangers = true;
        }

        public override void ApplySettings()
        {
            base.ApplySettings();
            AC_Utils.ApplySettings();
        }
    }
}
