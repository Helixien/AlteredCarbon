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
        public bool enableSoldMindFramesCreatingPawnDuplicates = true;
        public bool enableMindFramesInAncientDangers = true;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref enableStackSpawning, "enableStackSpawning", true);
            Scribe_Values.Look(ref enableTechprintRequirement, "enableTechprintRequirement", true);
            Scribe_Values.Look(ref sleeveDeathDoesNotCauseGearTainting, "sleeveDeathDoesNotCauseGearTainting", true);
            Scribe_Values.Look(ref enableSoldMindFramesCreatingPawnDuplicates, "enableSoldMindFramesCreatingPawnDuplicates", true);
            Scribe_Values.Look(ref enableMindFramesInAncientDangers, "enableMindFramesInAncientDangers", true);
        }

        public override void CopyFrom(PatchOperationWorker savedWorker)
        {
            var copy = savedWorker as AlteredCarbonSettingsWorker_General;
            this.enableStackSpawning = copy.enableStackSpawning;
            this.enableTechprintRequirement = copy.enableTechprintRequirement;
            this.sleeveDeathDoesNotCauseGearTainting = copy.sleeveDeathDoesNotCauseGearTainting;
            this.enableSoldMindFramesCreatingPawnDuplicates = copy.enableSoldMindFramesCreatingPawnDuplicates;
            this.enableMindFramesInAncientDangers = copy.enableMindFramesInAncientDangers;
        }

        public override void DoSettings(ModSettingsContainer container, Listing_Standard list)
        {
            DoCheckbox(list, "AC.EnableStackSpawning".Translate(), ref enableStackSpawning, "AC.EnableStackSpawningDesc".Translate());
            DoCheckbox(list, "AC.EnableTechprintRequirement".Translate(), ref enableTechprintRequirement, "AC.EnableTechprintRequirementDesc".Translate());
            DoCheckbox(list, "AC.SleeveDeathDoesNotCauseGearTainting".Translate(), ref sleeveDeathDoesNotCauseGearTainting, null);
            DoCheckbox(list, "AC.EnableSoldMindFramesCreatingPawnDuplicates".Translate(), ref enableSoldMindFramesCreatingPawnDuplicates, "AC.EnableSoldMindFramesCreatingPawnDuplicatesDesc".Translate());
            DoCheckbox(list, "AC.EnableMindFramesInAncientDangers".Translate(), ref enableMindFramesInAncientDangers, "AC.EnableMindFramesInAncientDangersDesc".Translate());
        }

        public override void Reset()
        {
            enableStackSpawning = true;
            enableTechprintRequirement = true;
            sleeveDeathDoesNotCauseGearTainting = true;
            enableSoldMindFramesCreatingPawnDuplicates = true;
            enableMindFramesInAncientDangers = true;
        }

        public override void ApplySettings()
        {
            base.ApplySettings();
            AC_Utils.ApplySettings();
        }
    }
}
