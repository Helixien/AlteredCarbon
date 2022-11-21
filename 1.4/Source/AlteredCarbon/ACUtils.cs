using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Verse;

namespace AlteredCarbon
{
    [StaticConstructorOnStartup]
    public static class ACUtils
    {
        public static Harmony harmony;

        public static Dictionary<string, List<GeneDef>> genesByCategories = new Dictionary<string, List<GeneDef>>();
        static ACUtils()
        {
            harmony = new Harmony("Altered.Carbon");
            harmony.PatchAll();

            foreach (var gene in DefDatabase<GeneDef>.AllDefs)
            {
                if (gene.exclusionTags.NullOrEmpty() is false)
                {
                    for (var i = 0; i < gene.exclusionTags.Count; i++)
                    {
                        var tag = gene.exclusionTags[i];
                        if (tag == "SkinColorOverride")
                        {
                            tag = "SkinColor";
                        }
                        if (genesByCategories.TryGetValue(tag, out var list) is false)
                        {
                            genesByCategories[tag] = list = new List<GeneDef>();
                        }
                        list.Add(gene);
                    }
                }
            }

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

        public static void UpdateGraphic(this Pawn pawn)
        {
            pawn.Drawer.renderer.graphics.ResolveAllGraphics();
            PortraitsCache.SetDirty(pawn);
            PortraitsCache.PortraitsCacheUpdate();
            GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(pawn);
        }
        public static void CopyBody(Pawn source, Pawn dest)
        {
            dest.gender = source.gender;
            dest.kindDef = source.kindDef;
            if (ModCompatibility.AlienRacesIsActive)
            {
                ModCompatibility.CopyBodyAddons(source, dest);
            }

            var genes = dest.genes.GenesListForReading;
            foreach (var oldGene in genes)
            {
                dest.genes.RemoveGene(oldGene);
            }

            var sourceGenes = source.genes.Endogenes;
            foreach (var sourceGene in sourceGenes)
            {
                dest.genes.AddGene(sourceGene.def, false);
            }
            for (var i = 0; i < sourceGenes.Count; i++)
            {
                var gene = dest.genes.Endogenes[i];
                if (sourceGenes[i].Active)
                {
                    GeneUtils.ApplyGene(gene, dest);
                }
            }

            dest.story.skinColorOverride = source.story.skinColorOverride;
            dest.story.skinColorBase = source.story.skinColorBase;
            dest.story.hairColor = source.story.hairColor;
            dest.story.bodyType = source.story.bodyType;
            dest.story.hairDef = source.story.hairDef;
            dest.style.beardDef = source.style.beardDef;
            dest.story.headType = source.story.headType;
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

        public static bool HasCorticalStack(this Pawn pawn, out Hediff_CorticalStack hediff_CorticalStack)
        {
            if (pawn?.health?.hediffSet?.GetFirstHediffOfDef(AC_DefOf.VFEU_CorticalStack) is Hediff_CorticalStack hediff)
            {
                hediff_CorticalStack = hediff;
                return true;
            }
            hediff_CorticalStack = null;
            return false;
        }
        public static bool IsCopy(this Pawn pawn)
        {
            if (pawn.HasCorticalStack(out var hediff) && AlteredCarbonManager.Instance.stacksRelationships.TryGetValue(hediff.PersonaData.stackGroupID, out StacksData stackData))
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
            return AlteredCarbonManager.Instance.StacksIndex.ContainsKey(pawn.thingIDNumber) 
                || AlteredCarbonManager.Instance.PawnsWithStacks.Contains(pawn);
        }

        public static Hediff MakeHediff(HediffDef hediffDef, Pawn pawn, BodyPartRecord part)
        {
            return ModCompatibility.RimJobWorldIsActive
                ? rjw.SexPartAdder.MakePart(hediffDef, pawn, part)
                : HediffMaker.MakeHediff(hediffDef, pawn, part);
        }
    }
}

