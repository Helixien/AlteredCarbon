using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using Verse;

namespace AlteredCarbon
{
    [StaticConstructorOnStartup]
    public static class ACUtils
    {
        public static Harmony harmony;
        static ACUtils()
        {
            harmony = new Harmony("Altered.Carbon");
            harmony.PatchAll();

            foreach (IngredientCount li in AC_DefOf.VFEU_InstallCorticalStack.ingredients)
            {
                li.filter.SetAllow(AC_DefOf.VFEU_AllowStacksColonist, true);
                li.filter.SetAllow(AC_DefOf.VFEU_AllowStacksStranger, true);
                li.filter.SetAllow(AC_DefOf.VFEU_AllowStacksHostile, true);
            }

            AC_DefOf.VFEU_InstallCorticalStack.fixedIngredientFilter.SetAllow(AC_DefOf.VFEU_AllowStacksColonist, true);
            AC_DefOf.VFEU_InstallCorticalStack.fixedIngredientFilter.SetAllow(AC_DefOf.VFEU_AllowStacksStranger, true);
            AC_DefOf.VFEU_InstallCorticalStack.fixedIngredientFilter.SetAllow(AC_DefOf.VFEU_AllowStacksHostile, true);

            AC_DefOf.VFEU_InstallCorticalStack.defaultIngredientFilter.SetAllow(AC_DefOf.VFEU_AllowStacksColonist, true);
            AC_DefOf.VFEU_InstallCorticalStack.defaultIngredientFilter.SetAllow(AC_DefOf.VFEU_AllowStacksStranger, true);
            AC_DefOf.VFEU_InstallCorticalStack.defaultIngredientFilter.SetAllow(AC_DefOf.VFEU_AllowStacksHostile, true);
        }

        public static string PawnTemplatesPath => Path.Combine(GenFilePaths.ConfigFolderPath, "AC_PawnTemplates.xml");
        public static void SavePresets()
        {
            Scribe.saver.InitSaving(PawnTemplatesPath, "PawnTemplates");
            Scribe_Collections.Look(ref AlteredCarbonMod.settings.presets, "presets", LookMode.Value, LookMode.Deep);
            Scribe.saver.FinalizeSaving();
        }

        public static void LoadPresets()
        {
            FileInfo info = new FileInfo(PawnTemplatesPath);
            if (info.Exists)
            {
                Scribe.loader.InitLoading(PawnTemplatesPath);
                Scribe_Collections.Look(ref AlteredCarbonMod.settings.presets, "presets", LookMode.Value, LookMode.Deep);
                Scribe.loader.FinalizeLoading();
            }
        }

        public static bool IsUltraTech(this Thing thing)
        {
            return thing.def == AC_DefOf.VFEU_SleeveIncubator
                || thing.def == AC_DefOf.VFEU_SleeveCasket || thing.def == AC_DefOf.VFEU_SleeveCasket
                || (AC_DefOf.VFEU_CorticalStackStorage != null && thing.def == AC_DefOf.VFEU_CorticalStackStorage)
                || thing.def == AC_DefOf.VFEU_DecryptionBench;
        }
        public static bool IsCopy(this Pawn pawn)
        {
            if (pawn.health.hediffSet.GetFirstHediffOfDef(AC_DefOf.VFEU_CorticalStack) is Hediff_CorticalStack hediff && AlteredCarbonManager.Instance.stacksRelationships.TryGetValue(hediff.PersonaData.stackGroupID, out StacksData stackData))
            {
                if (stackData.originalPawn != null && pawn != stackData.originalPawn)
                {
                    return true;
                }
                if (stackData.copiedPawns != null)
                {
                    foreach (Pawn copiedPawn in stackData.copiedPawns)
                    {
                        if (pawn == copiedPawn)
                        {
                            return true;
                        }
                    }
                }
            }
            else if (AlteredCarbonManager.Instance.stacksRelationships != null)
            {
                foreach (KeyValuePair<int, StacksData> stackGroup in AlteredCarbonManager.Instance.stacksRelationships)
                {
                    if (stackGroup.Value.copiedPawns != null)
                    {
                        foreach (Pawn copiedPawn in stackGroup.Value.copiedPawns)
                        {
                            if (pawn == copiedPawn)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public static bool IsEmptySleeve(this Pawn pawn)
        {
            return AlteredCarbonManager.Instance.emptySleeves.Contains(pawn);
        }

        public static void DisableKilledEffects(this Pawn pawn)
        {
            Faction_Notify_LeaderDied_Patch.disableKilledEffect = true;
            PawnDiedOrDownedThoughtsUtility_AppendThoughts_ForHumanlike_Patch.disableKilledEffect = true;
            PawnDiedOrDownedThoughtsUtility_AppendThoughts_Relations_Patch.disableKilledEffect = true;
            Pawn_HealthTracker_NotifyPlayerOfKilled_Patch.disableKilledEffect = true;
            StatsRecord_Notify_ColonistKilled_Patch.disableKilledEffect = true;
            Pawn_RoyaltyTracker_Notify_PawnKilled_Patch.disableKilledEffect = true;
        }
        public static bool HasStack(this Pawn pawn)
        {
            return AlteredCarbonManager.Instance.StacksIndex.ContainsKey(pawn.thingIDNumber) || AlteredCarbonManager.Instance.PawnsWithStacks.Contains(pawn);
        }

        public static Hediff MakeHediff(HediffDef hediffDef, Pawn pawn, BodyPartRecord part)
        {
            return ModCompatibility.RimJobWorldIsActive
                ? rjw.SexPartAdder.MakePart(hediffDef, pawn, part)
                : HediffMaker.MakeHediff(hediffDef, pawn, part);
        }
    }
}

