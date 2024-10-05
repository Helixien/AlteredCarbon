using Verse;

namespace AlteredCarbon
{
    public class PsychologyCompatibilityEntry : ModCompatibilityEntry
    {
        private PsychologyData psychologyData;
        public PsychologyCompatibilityEntry() { }

        public PsychologyCompatibilityEntry(string modIdentifier) : base(modIdentifier) { }

        public override void FetchData(Pawn pawn)
        {
            psychologyData = GetPsychologyData(pawn);
        }

        public override void SetData(Pawn pawn)
        {
            if (psychologyData != null)
            {
                SetPsychologyData(pawn, psychologyData);
            }
        }


        private PsychologyData GetPsychologyData(Pawn pawn)
        {
            Psychology.CompPsychology comp = ThingCompUtility.TryGetComp<Psychology.CompPsychology>(pawn);
            if (comp != null)
            {
                PsychologyData psychologyData = new PsychologyData();
                Psychology.Pawn_SexualityTracker sexualityTracker = comp.Sexuality;
                psychologyData.sexDrive = sexualityTracker.sexDrive;
                psychologyData.romanticDrive = sexualityTracker.romanticDrive;
                psychologyData.kinseyRating = sexualityTracker.kinseyRating;
                psychologyData.knownSexualities = sexualityTracker.knownSexualities;
                return psychologyData;
            }
            return null;
        }

        private void SetPsychologyData(Pawn pawn, PsychologyData psychologyData)
        {
            Psychology.CompPsychology comp = ThingCompUtility.TryGetComp<Psychology.CompPsychology>(pawn);
            if (comp != null)
            {
                Psychology.Pawn_SexualityTracker sexualityTracker = new Psychology.Pawn_SexualityTracker(pawn)
                {
                    sexDrive = psychologyData.sexDrive,
                    romanticDrive = psychologyData.romanticDrive,
                    kinseyRating = psychologyData.kinseyRating
                };
                sexualityTracker.knownSexualities = psychologyData.knownSexualities;
                comp.Sexuality = sexualityTracker;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref psychologyData, "psychologyData");
        }

        public override void CopyFrom(ModCompatibilityEntry other)
        {
            psychologyData = (other as PsychologyCompatibilityEntry).psychologyData;
        }
    }
}