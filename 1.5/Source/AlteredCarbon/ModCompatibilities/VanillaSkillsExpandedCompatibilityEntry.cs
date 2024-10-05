using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AlteredCarbon
{
    public class VanillaSkillsExpandedCompatibilityEntry : ModCompatibilityEntry
    {
        private List<IExposable> expertiseRecords;
        public VanillaSkillsExpandedCompatibilityEntry() { }

        public VanillaSkillsExpandedCompatibilityEntry(string modIdentifier) : base(modIdentifier) { }

        public override void FetchData(Pawn pawn)
        {
            expertiseRecords = GetExpertises(pawn);
        }

        public override void SetData(Pawn pawn)
        {
            if (expertiseRecords != null)
            {
                SetExpertises(pawn, expertiseRecords);
            }
        }

        public List<IExposable> GetExpertises(Pawn pawn)
        {
            VSE.ExpertiseTracker expertiseTracker = VSE.ExpertiseTrackers.Expertise(pawn);
            if (expertiseTracker != null)
            {
                return expertiseTracker.AllExpertise.Cast<IExposable>().ToList();
            }
            return null;
        }

        public void SetExpertises(Pawn pawn, List<IExposable> expertises)
        {
            VSE.ExpertiseTracker expertiseTracker = VSE.ExpertiseTrackers.Expertise(pawn);
            if (expertiseTracker != null)
            {
                expertiseTracker.ClearExpertise();
                if (expertises != null)
                {
                    foreach (var expertise in expertises.Cast<VSE.ExpertiseRecord>())
                    {
                        expertiseTracker.AllExpertise.Add(expertise);
                    }
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref expertiseRecords, "expertiseRecords", LookMode.Deep);
        }

        public override void CopyFrom(ModCompatibilityEntry other)
        {
            expertiseRecords = (other as VanillaSkillsExpandedCompatibilityEntry).expertiseRecords.CopyList();
        }
    }
}