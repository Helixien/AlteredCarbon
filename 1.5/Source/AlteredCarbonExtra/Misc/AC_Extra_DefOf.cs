﻿using RimWorld;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    [DefOf]
    public static class AC_Extra_DefOf
    {
        public static ThingDef AC_StackArray;
        public static JobDef AC_DuplicateStack;
        public static JobDef AC_CreateStackFromBackup;
        public static ThoughtDef AC_JustCopy;
        public static ThoughtDef AC_LostMySpouse;
        public static ThoughtDef AC_LostMyFiance;
        public static ThoughtDef AC_LostMyLover;
        public static PawnRelationDef AC_Original;
        public static PawnRelationDef AC_Copy;
        public static JobDef AC_HaulingStacks;
        public static ResearchProjectDef Xenogermination;
        public static JobDef AC_InsertingThingIntoProcessor;
        public static ThingDef AC_GeneCentrifuge;
        public static SoundDef AC_GeneCentrifuge_Ambience;
        public static JobDef AC_HaulCorpseToSleeveGrower;
        public static JobDef AC_CancelRepurposingBody;
        public static ThingDef AC_XenoGermDuplicator;
        public static SoundDef AC_XenoGermDuplicator_Ambience;
        public static ResearchProjectDef AC_RewriteCorticalStack;
        public static EffecterDef VFEU_Hacking;

        [MayRequireIdeology] public static PreceptDef AC_Stacking_Despised;
        [MayRequireIdeology] public static PreceptDef AC_Sleeving_Despised;
        [MayRequireIdeology] public static PreceptDef AC_CrossSleeving_DontCare;
        [MayRequireIdeology] public static PreceptDef AC_CrossSleeving_Despised;
        [MayRequireIdeology] public static HistoryEventDef AC_RewroteStack;

        public static ThoughtDef AC_CortexOverseerFreed;
        public static RecipeDef AC_HackBiocodedThings;

        public static ThingDef AC_Apparel_DragoonHelmet;
        public static ThingDef AC_Apparel_FusilierHelmet;
    }
}

