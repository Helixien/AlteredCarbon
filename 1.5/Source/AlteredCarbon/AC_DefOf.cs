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
        public static ThingDef AC_ActiveArchotechStack;
        public static ThingDef AC_EmptyArchotechStack;
        public static HediffDef AC_ArchotechStack;
        public static RecipeDef AC_InstallArchotechStack;
        public static RecipeDef AC_InstallEmptyArchotechStack;
        public static VFECore.Abilities.AbilityDef AC_ArchotechStackSkip;
        public static HediffDef AC_StackDegradation;
        public static HediffDef AC_BrainTrauma;
        public static RecipeDef AC_EditActiveNeuralStack;
        public static HistoryEventDef AC_UsedArchotechStack;
        public static ThoughtDef AC_StackDegradationThought;
        [MayRequireIdeology] public static HistoryEventDef AC_InstalledNeuralStack;

        public static VEBackstoryDef AC_VatGrownChild;
        public static VEBackstoryDef AC_VatGrownAdult;
        public static ThingDef AC_EmptyNeuralStack;
        public static ThingDef AC_ActiveNeuralStack;
        public static JobDef AC_ExtractStack;
        public static JobDef AC_StartGestationProcess;
        public static JobDef AC_CancelGestationProcess;
        public static JobDef AC_TakeEmptySleeve;
        public static HediffDef AC_NeuralStack;
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
        public static ThingDef AC_NeuralEditor;
        public static RecipeDef AC_WipeActiveNeuralStack;
        public static RecipeDef AC_InstallNeuralStack;
        public static RecipeDef AC_InstallEmptyNeuralStack;
        public static SpecialThingFilterDef AC_AllowStacksColonist;
        public static SpecialThingFilterDef AC_AllowStacksStranger;
        public static SpecialThingFilterDef AC_AllowStacksHostile;
        public static ThoughtDef AC_WrongGender;
        public static ThoughtDef AC_WrongGenderPregnant;
		[MayRequireHARMod] public static ThoughtDef AC_WrongRace;
        public static ThoughtDef AC_WrongXenotype;
        public static ThoughtDef AC_NewSleeve;
        public static DesignationDef AC_ExtractStackDesignation;
        public static ThingDef AC_Mote_VatGlow;
        public static EffecterDef AC_Vat_Bubbles;
        public static LetterDef HumanPregnancy;
        public static FleckDef PsycastAreaEffect;
        public static HediffDef TraumaSavant;
        public static RecipeDef CremateCorpse;
        public static RecipeDef ButcherCorpseFlesh;
        public static TraitDef AC_Sleever;
        [MayRequireVREAndroidMod] public static GeneDef AC_NeuralModule;
        [MayRequireVREAndroidMod] public static ThoughtDef AC_NewShell, AC_WantsShell, AC_WrongShellGender;
        [MayRequireVREAndroidMod] public static TraitDef AC_Shellwalker;
        public static SoundDef Message_NegativeEvent;
        public static XenotypeDef AC_Sleeveliner;
        public static TraitDef Beauty, NaturalMood, Nerves;
        public static HeadTypeDef Stump;
        public static BodyPartDef Neck;

        public static ThingDef AC_NeuralMatrix;
        public static RecipeDef AC_DuplicateNeuralStack, AC_DuplicateNeuralStackPawn;
        public static JobDef AC_HaulThingsToContainer;
        public static ResearchProjectDef Xenogermination;
        public static JobDef AC_HaulCorpseToGestation;
        public static JobDef AC_CancelRepurposingBody;
        public static ResearchProjectDef AC_NeuralEditing;

        [MayRequireIdeology] public static PreceptDef AC_CrossSleeving_DontCare;
        public static HistoryEventDef AC_EditedStack;

        public static RecipeDef AC_ResetBiocodedThings;
        public static ThingDef AC_Apparel_DragoonHelmet;
        public static ThingDef AC_Apparel_FusilierHelmet;
        public static JobDef AC_ChargeCuirassierBelt;
        public static ThingDef AC_CuirassierBelt;
        [MayRequireAnomaly] public static HeadTypeDef CultEscapee;
        public static HistoryEventDef AC_ErasedStackEvent, AC_DuplicatedStackEvent;
        public static ThingDef AC_StackCache;
        public static ThoughtDef AC_ErasedStack;
        public static ResearchProjectDef AC_NeuralDigitalization;
        //public static ThingDef AC_NeuralPrint;
        //public static RecipeDef AC_RestoreStackFromNeuralPrint;
        public static ThingDef AC_NeuralConnector;
        
        public static HediffDef AC_VoiceSynthesizer;
        public static HediffDef AC_MentalFuse;
        public static HediffDef AC_Dreamcatcher;
    }
}