using RimWorld;
using Verse;
using VFECore;

namespace AlteredCarbon
{
    public class MayRequireHelixienModAttribute : MayRequireAttribute
    {
        public MayRequireHelixienModAttribute()
            : base("hlx.UltratechAlteredCarbon")
        {
        }
    }
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
        [MayRequireHelixienMod] public static ThoughtDef AC_JustCopy;
        [MayRequireHelixienMod] public static ThoughtDef AC_LostMySpouse;
        [MayRequireHelixienMod] public static ThoughtDef AC_LostMyFiance;
        [MayRequireHelixienMod] public static ThoughtDef AC_LostMyLover;
        [MayRequireHelixienMod] public static PawnRelationDef AC_Original;
        [MayRequireHelixienMod] public static PawnRelationDef AC_Copy;
        [MayRequireHelixienMod] public static ThingDef AC_FilledArchoStack;
        [MayRequireHelixienMod] public static ThingDef AC_EmptyArchoStack;
        [MayRequireHelixienMod] public static HediffDef AC_ArchoStack;
        [MayRequireHelixienMod] public static RecipeDef AC_InstallArchoStack;
        [MayRequireHelixienMod] public static RecipeDef AC_InstallEmptyArchoStack;
        [MayRequireHelixienMod] public static VFECore.Abilities.AbilityDef AC_ArchoStackSkip;
        [MayRequireHelixienMod] public static HediffDef AC_StackDegradation;
        [MayRequireHelixienMod] public static HediffDef AC_BrainTrauma;
        [MayRequireHelixienMod] public static RecipeDef AC_RewriteFilledCorticalStack;
        [MayRequireHelixienMod] public static HistoryEventDef AC_UsedArchoStack;
        [MayRequireHelixienMod] public static ThoughtDef AC_StackDegradationThought;
        [MayRequireIdeology] public static HistoryEventDef VFEU_InstalledCorticalStack;

        public static VEBackstoryDef VFEU_VatGrownChild;
        public static VEBackstoryDef VFEU_VatGrownAdult;
        public static ThingDef VFEU_EmptyCorticalStack;
        public static ThingDef VFEU_FilledCorticalStack;
        public static JobDef VFEU_ExtractStack;
        public static JobDef VFEU_StartIncubatingProcess;
        public static JobDef VFEU_CancelIncubatingProcess;
        public static JobDef AC_TakeEmptySleeve;
        public static HediffDef VFEU_CorticalStack;
        public static GeneDef VFEU_SleeveQuality_Awful;
        public static GeneDef VFEU_SleeveQuality_Poor;
        public static GeneDef VFEU_SleeveQuality_Normal;
        public static GeneDef VFEU_SleeveQuality_Good;
        public static GeneDef VFEU_SleeveQuality_Excellent;
        public static GeneDef VFEU_SleeveQuality_Masterwork;
        public static GeneDef VFEU_SleeveQuality_Legendary;
        public static HediffDef VFEU_EmptySleeve;
        public static HediffDef VFEU_SleeveShock;
        public static ThingDef VFEU_SleeveIncubator;
        public static ThingDef VFEU_SleeveCasket;
        public static ThingDef VFEU_DecryptionBench;
        public static RecipeDef VFEU_WipeFilledCorticalStack;
        public static RecipeDef VFEU_InstallCorticalStack;
        public static RecipeDef VFEU_InstallEmptyCorticalStack;
        public static SpecialThingFilterDef VFEU_AllowStacksColonist;
        public static SpecialThingFilterDef VFEU_AllowStacksStranger;
        public static SpecialThingFilterDef VFEU_AllowStacksHostile;
        public static ThoughtDef VFEU_WrongGender;
        public static ThoughtDef VFEU_WrongGenderDouble;
        public static ThoughtDef VFEU_WrongGenderPregnant;
		[MayRequireHARMod] public static ThoughtDef VFEU_WrongRace;
        public static ThoughtDef VFEU_WrongXenotype;
        public static ThoughtDef VFEU_NewSleeve;
        public static ThoughtDef VFEU_NewSleeveDouble;
        public static ThoughtDef VFEU_MansBody;
        public static ThoughtDef VFEU_WomansBody;
        public static DesignationDef VFEU_ExtractStackDesignation;
        public static DamageDef VFEU_Deterioration;
        public static ThingDef VFEU_Mote_VatGlow;
        public static EffecterDef VFEU_Vat_Bubbles;
        public static LetterDef HumanPregnancy;
        public static FleckDef PsycastAreaEffect;
        public static HediffDef TraumaSavant;
        public static RecipeDef CremateCorpse;
        public static RecipeDef ButcherCorpseFlesh;
        public static TraitDef VFEU_Sleever;
        [MayRequireVREAndroidMod] public static GeneDef AC_CorticalModule;
        [MayRequireVREAndroidMod] public static ThoughtDef VFEU_NewShell, VFEU_NewShellDouble, VFEU_WantsShell, VFEU_WrongShellGender, VFEU_WrongShellGenderDouble;
        [MayRequireVREAndroidMod] public static TraitDef VFEU_Shellwalker;
    }
}