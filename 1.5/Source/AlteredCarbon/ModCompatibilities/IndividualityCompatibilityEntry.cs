using Verse;

namespace AlteredCarbon
{
    public class IndividualityCompatibilityEntry : ModCompatibilityEntry
    {
        private int sexuality = -1;
        private float romanceFactor = -1;
        public IndividualityCompatibilityEntry() { }

        public IndividualityCompatibilityEntry(string modIdentifier) : base(modIdentifier) { }

        public override void FetchData(Pawn pawn)
        {
            sexuality = GetSyrTraitsSexuality(pawn);
            romanceFactor = GetSyrTraitsRomanceFactor(pawn);
        }

        public override void SetData(Pawn pawn)
        {
            SetSyrTraitsSexuality(pawn, sexuality);
            SetSyrTraitsRomanceFactor(pawn, romanceFactor);
        }

        private int GetSyrTraitsSexuality(Pawn pawn)
        {
            SyrTraits.CompIndividuality comp = ThingCompUtility.TryGetComp<SyrTraits.CompIndividuality>(pawn);
            return comp != null ? (int)comp.sexuality : -1;
        }
        private float GetSyrTraitsRomanceFactor(Pawn pawn)
        {
            SyrTraits.CompIndividuality comp = ThingCompUtility.TryGetComp<SyrTraits.CompIndividuality>(pawn);
            return comp != null ? comp.RomanceFactor : -1f;
        }

        private void SetSyrTraitsSexuality(Pawn pawn, int sexuality)
        {
            SyrTraits.CompIndividuality comp = ThingCompUtility.TryGetComp<SyrTraits.CompIndividuality>(pawn);
            if (comp != null)
            {
                comp.sexuality = (SyrTraits.CompIndividuality.Sexuality)sexuality;
            }
        }

        private void SetSyrTraitsRomanceFactor(Pawn pawn, float romanceFactor)
        {
            SyrTraits.CompIndividuality comp = ThingCompUtility.TryGetComp<SyrTraits.CompIndividuality>(pawn);
            if (comp != null)
            {
                comp.RomanceFactor = romanceFactor;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref sexuality, "sexuality", -1);
            Scribe_Values.Look(ref romanceFactor, "romanceFactor", -1f);
        }

        public override void CopyFrom(ModCompatibilityEntry other)
        {
            var otherData = (other as IndividualityCompatibilityEntry);
            sexuality = otherData.sexuality;
            romanceFactor = otherData.romanceFactor;
        }
    }
}