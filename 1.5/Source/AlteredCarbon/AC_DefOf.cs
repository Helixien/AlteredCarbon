using RimWorld;
using Verse;
using VFECore;

namespace AlteredCarbon
{
    public class MayRequireHARModAttribute : MayRequireAttribute
    {
        public MayRequireHARModAttribute()
            : base("erdelf.HumanoidAlienRaces")
        {
        }
    }
    public class MayRequireVREAndroidModAttribute : MayRequireAttribute
    {
        public MayRequireVREAndroidModAttribute()
            : base("vanillaracesexpanded.android")
        {
        }
    }
    [DefOf]
    public static class AC_DefOf
    {
        public static ThoughtDef AC_JustCopy;
        public static ThoughtDef AC_LostMySpouse;
        public static ThoughtDef AC_LostMyFiance;
        public static ThoughtDef AC_LostMyLover;
        public static PawnRelationDef AC_Original;
        public static PawnRelationDef AC_Copy;
        public static ThingDef AC_FilledArchotechStack;
        public static ThingDef AC_EmptyArchotechStack;
        public static HediffDef AC_ArchotechStack;
        public static RecipeDef AC_InstallArchotechStack;
        public static RecipeDef AC_InstallEmptyArchotechStack;
        public static VFECore.Abilities.AbilityDef AC_ArchotechStackSkip;
        public static HediffDef AC_StackDegradation;
        public static HediffDef AC_BrainTrauma;
        public static RecipeDef AC_RewriteFilledPersonaStack;
        public static HistoryEventDef AC_UsedArchotechStack;
        public static ThoughtDef AC_StackDegradationThought;
        [MayRequireIdeology] public static HistoryEventDef AC_InstalledPersonaStack;

        public static VEBackstoryDef AC_VatGrownChild;
        public static VEBackstoryDef AC_VatGrownAdult;
        public static ThingDef AC_EmptyPersonaStack;
        public static ThingDef AC_FilledPersonaStack;
        public static JobDef AC_ExtractStack;
        public static JobDef AC_StartGestationProcess;
        public static JobDef AC_CancelGestationProcess;
        public static JobDef AC_TakeEmptySleeve;
        public static HediffDef AC_PersonaStack;
        public static GeneDef AC_SleeveQuality_Awful;
        public static GeneDef AC_SleeveQuality_Poor;
        public static GeneDef AC_SleeveQuality_Normal;
        public static GeneDef AC_SleeveQuality_Good;
        public static GeneDef AC_SleeveQuality_Excellent;
        public static GeneDef AC_SleeveQuality_Masterwork;
        public static GeneDef AC_SleeveQuality_Legendary;
        public static HediffDef AC_EmptySleeve;
        public static HediffDef AC_SleeveShock;
        public static ThingDef AC_SleeveGestator;
        public static ThingDef AC_SleeveCasket;
        public static ThingDef AC_DecryptionBench;
        public static RecipeDef AC_WipeFilledPersonaStack;
        public static RecipeDef AC_InstallPersonaStack;
        public static RecipeDef AC_InstallEmptyPersonaStack;
        public static SpecialThingFilterDef AC_AllowStacksColonist;
        public static SpecialThingFilterDef AC_AllowStacksStranger;
        public static SpecialThingFilterDef AC_AllowStacksHostile;
        public static ThoughtDef AC_WrongGender;
        public static ThoughtDef AC_WrongGenderDouble;
        public static ThoughtDef AC_WrongGenderPregnant;
		[MayRequireHARMod] public static ThoughtDef AC_WrongRace;
        public static ThoughtDef AC_WrongXenotype;
        public static ThoughtDef AC_NewSleeve;
        public static ThoughtDef AC_NewSleeveDouble;
        public static ThoughtDef AC_MansBody;
        public static ThoughtDef AC_WomansBody;
        public static DesignationDef AC_ExtractStackDesignation;
        public static DamageDef AC_Deterioration;
        public static ThingDef AC_Mote_VatGlow;
        public static EffecterDef AC_Vat_Bubbles;
        public static LetterDef HumanPregnancy;
        public static FleckDef PsycastAreaEffect;
        public static HediffDef TraumaSavant;
        public static RecipeDef CremateCorpse;
        public static RecipeDef ButcherCorpseFlesh;
        public static TraitDef AC_Sleever;
        [MayRequireVREAndroidMod] public static GeneDef AC_PersonaModule;
        [MayRequireVREAndroidMod] public static ThoughtDef AC_NewShell, AC_NewShellDouble, AC_WantsShell, AC_WrongShellGender, AC_WrongShellGenderDouble;
        [MayRequireVREAndroidMod] public static TraitDef AC_Shellwalker;
        public static SoundDef Message_NegativeEvent;
        public static XenotypeDef AC_Sleeveliner;
        public static TraitDef Beauty, NaturalMood, Nerves;
        public static HeadTypeDef Stump;
        public static BodyPartDef Neck;

        public static ThingDef AC_StackArray;
        public static JobDef AC_DuplicateStack;
        public static JobDef AC_CreateStackFromBackup;
        public static JobDef AC_HaulingStacks;
        public static ResearchProjectDef Xenogermination;
        public static JobDef AC_HaulCorpseToGestation;
        public static JobDef AC_CancelRepurposingBody;
        public static ResearchProjectDef AC_RewritePersonaStack;
        public static EffecterDef AC_Hacking;

        [MayRequireIdeology] public static PreceptDef AC_Stacking_Despised;
        [MayRequireIdeology] public static PreceptDef AC_Sleeving_Despised;
        [MayRequireIdeology] public static PreceptDef AC_CrossSleeving_DontCare;
        [MayRequireIdeology] public static PreceptDef AC_CrossSleeving_Despised;
        [MayRequireIdeology] public static HistoryEventDef AC_RewroteStack;

        public static RecipeDef AC_HackBiocodedThings;
        public static ThingDef AC_Apparel_DragoonHelmet;
        public static ThingDef AC_Apparel_FusilierHelmet;
    }
}