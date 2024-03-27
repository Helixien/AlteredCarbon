using Verse;

namespace AlteredCarbon
{
    public enum OrientationAC
    {
        None,
        Asexual,
        Pansexual,
        Heterosexual,
        MostlyHeterosexual,
        LeaningHeterosexual,
        Bisexual,
        LeaningHomosexual,
        MostlyHomosexual,
        Homosexual
    }
    public class RJWData : IExposable
    {
        public bool Comfort = false;
        public bool Service = false;
        public bool Breeding = false;
        public bool Milking = false;
        public bool Hero = false;
        public bool Ironman = false;
        public string HeroOwner = "";
        public bool BreedingAnimal = false;
        public bool CanChangeDesignationColonist = false;
        public bool CanChangeDesignationPrisoner = false;
        public bool CanDesignateService = false;
        public bool CanDesignateMilking = false;
        public bool CanDesignateComfort = false;
        public bool CanDesignateBreedingAnimal = false;
        public bool CanDesignateBreeding = false;
        public bool CanDesignateHero = false;
        public bool ShowRMB_Menu = false;
        public bool isSlime = false;
        public bool isDemon = false;
        public bool oviPregnancy = false;
        public float raceSexDrive = 1.0f;

        public OrientationAC orientation;
        public string quirksave;
        public int NextHookupTick;
        public void ExposeData()
        {
            Scribe_Values.Look(ref Comfort, "Comfort", false, true);
            Scribe_Values.Look(ref Service, "Service", false, true);
            Scribe_Values.Look(ref Breeding, "Breeding", false, true);
            Scribe_Values.Look(ref Milking, "Milking", false, true);
            Scribe_Values.Look(ref Hero, "Hero", false, true);
            Scribe_Values.Look(ref Ironman, "Ironman", false, true);
            Scribe_Values.Look(ref HeroOwner, "HeroOwner", "", true);
            Scribe_Values.Look(ref BreedingAnimal, "BreedingAnimal", false, true);
            Scribe_Values.Look(ref CanChangeDesignationColonist, "CanChangeDesignationColonist", false, true);
            Scribe_Values.Look(ref CanChangeDesignationPrisoner, "CanChangeDesignationPrisoner", false, true);
            Scribe_Values.Look(ref CanDesignateService, "CanDesignateService", false, true);
            Scribe_Values.Look(ref CanDesignateMilking, "CanDesignateMilking", false, true);
            Scribe_Values.Look(ref CanDesignateComfort, "CanDesignateComfort", false, true);
            Scribe_Values.Look(ref CanDesignateBreedingAnimal, "CanDesignateBreedingAnimal", false, true);
            Scribe_Values.Look(ref CanDesignateBreeding, "CanDesignateBreeding", false, true);
            Scribe_Values.Look(ref CanDesignateHero, "CanDesignateHero", false, true);
            Scribe_Values.Look(ref ShowRMB_Menu, "ShowRMB_Menu", false, true);
            Scribe_Values.Look(ref isSlime, "isSlime", false, true);
            Scribe_Values.Look(ref isDemon, "isDemon", false, true);
            Scribe_Values.Look(ref oviPregnancy, "oviPregnancy", false, true);
            Scribe_Values.Look(ref raceSexDrive, "raceSexDrive", 1.0f, true);
        }
    }
}