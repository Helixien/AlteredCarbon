using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    public struct StackInstallInfo
    {
        public RecipeDef recipe;
        public string installLabel;
        public string installDesc;
        public Texture2D installIcon;
    }
    [StaticConstructorOnStartup]
    public static class ACUtils
    {
        public static Harmony harmony;

        public static Dictionary<string, List<GeneDef>> genesByCategories = new Dictionary<string, List<GeneDef>>();
        public static Dictionary<ThingDef, ThingDef> stacksPairs = new Dictionary<ThingDef, ThingDef>
        {
            { AC_DefOf.VFEU_FilledCorticalStack, AC_DefOf.VFEU_EmptyCorticalStack },
        };
        
        public static readonly List<HediffDef> sleeveQualities = new List<HediffDef>
        {
            AC_DefOf.VFEU_Sleeve_Quality_Awful,
            AC_DefOf.VFEU_Sleeve_Quality_Poor,
            AC_DefOf.VFEU_Sleeve_Quality_Normal,
            AC_DefOf.VFEU_Sleeve_Quality_Good,
            AC_DefOf.VFEU_Sleeve_Quality_Excellent,
            AC_DefOf.VFEU_Sleeve_Quality_Masterwork,
            AC_DefOf.VFEU_Sleeve_Quality_Legendary
        };

        public static Dictionary<ThingDef, StackInstallInfo> stackRecipesByDef = new Dictionary<ThingDef, StackInstallInfo>
        {
            { 
                AC_DefOf.VFEU_FilledCorticalStack, new StackInstallInfo
                {
                    recipe = AC_DefOf.VFEU_InstallCorticalStack, 
                    installLabel = "AC.InstallStack".Translate(), 
                    installDesc = "AC.InstallStackDesc".Translate(),
                    installIcon = ContentFinder<Texture2D>.Get("UI/Icons/InstallStack")
                }
            },
        };
        public static HashSet<RecipeDef> installEmptyStacksRecipes = new HashSet<RecipeDef>
        {
            AC_DefOf.VFEU_InstallEmptyCorticalStack
        };
        public static HashSet<RecipeDef> installFilledStacksRecipes = new HashSet<RecipeDef>
        {
            AC_DefOf.VFEU_InstallCorticalStack
        };

        public static bool CanThink(this Pawn pawn)
        {
            return pawn.needs?.mood?.thoughts?.memories != null;
        }
        static ACUtils()
        {
            harmony = new Harmony("Altered.Carbon");
            harmony.PatchAll();

            if (ModCompatibility.HelixienAlteredCarbonIsActive)
            {
                stackRecipesByDef[AC_DefOf.AC_FilledArchoStack] = new StackInstallInfo
                {
                    recipe = AC_DefOf.AC_InstallArchoStack,
                    installLabel = "AC.InstallArchoStack".Translate(),
                    installDesc = "AC.InstallArchoStackDesc".Translate(),
                    installIcon = ContentFinder<Texture2D>.Get("UI/Icons/InstallArchoStack")
                };
                stacksPairs[AC_DefOf.AC_FilledArchoStack] = AC_DefOf.AC_EmptyArchoStack;
                installEmptyStacksRecipes.Add(AC_DefOf.AC_InstallEmptyArchoStack);
                installFilledStacksRecipes.Add(AC_DefOf.AC_InstallArchoStack);
            }

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

            foreach (var info in stackRecipesByDef.Values)
            {
                foreach (IngredientCount li in info.recipe.ingredients)
                {
                    li.filter.SetAllow(AC_DefOf.VFEU_AllowStacksColonist, true);
                    li.filter.SetAllow(AC_DefOf.VFEU_AllowStacksStranger, true);
                    li.filter.SetAllow(AC_DefOf.VFEU_AllowStacksHostile, true);
                }

                info.recipe.fixedIngredientFilter.SetAllow(AC_DefOf.VFEU_AllowStacksColonist, true);
                info.recipe.fixedIngredientFilter.SetAllow(AC_DefOf.VFEU_AllowStacksStranger, true);
                info.recipe.fixedIngredientFilter.SetAllow(AC_DefOf.VFEU_AllowStacksHostile, true);

                info.recipe.defaultIngredientFilter.SetAllow(AC_DefOf.VFEU_AllowStacksColonist, true);
                info.recipe.defaultIngredientFilter.SetAllow(AC_DefOf.VFEU_AllowStacksStranger, true);
                info.recipe.defaultIngredientFilter.SetAllow(AC_DefOf.VFEU_AllowStacksHostile, true);
            }

        }

        public static bool CanImplantStackTo(HediffDef stackToImplant, Pawn pawn)
        {
            if (pawn.RaceProps.Humanlike is false)
            {
                return false;
            }
            if (pawn.DevelopmentalStage != DevelopmentalStage.Adult)
            {
                return false;
            }
            if (pawn.HasCorticalStack(out var stackHediff)
                && (stackHediff.def == AC_DefOf.AC_ArchoStack || stackToImplant == stackHediff.def))
            {
                return false;
            }
            return true;
        }
        public static ThingDef GetEmptyStackVariant(this CorticalStack corticalStack)
        {
            if (corticalStack.def == AC_DefOf.AC_FilledArchoStack)
            {
                return AC_DefOf.AC_EmptyArchoStack;
            }
            return AC_DefOf.VFEU_EmptyCorticalStack;
        }

        public static ThingDef GetFilledStackVariant(this CorticalStack corticalStack)
        {
            if (corticalStack.def == AC_DefOf.AC_EmptyArchoStack)
            {
                return AC_DefOf.AC_FilledArchoStack;
            }
            return AC_DefOf.VFEU_FilledCorticalStack;
        }
        public static void RefreshGraphic(this Pawn pawn)
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
                ModCompatibility.SetSkinColorFirst(dest, ModCompatibility.GetSkinColorFirst(source));
                ModCompatibility.SetSkinColorSecond(dest, ModCompatibility.GetSkinColorSecond(source));
                ModCompatibility.SetHairColorFirst(dest, ModCompatibility.GetHairColorFirst(source));
                ModCompatibility.SetHairColorSecond(dest, ModCompatibility.GetHairColorSecond(source));
            }

            var genes = dest.genes.GenesListForReading;
            foreach (var oldGene in genes)
            {
                dest.genes.RemoveGene(oldGene);
            }

            var sourceGenes = source.genes.Endogenes;
            if (sourceGenes != null)
            {
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
            }


            dest.story.skinColorOverride = source.story.skinColorOverride;
            dest.story.skinColorBase = source.story.skinColorBase;
            dest.story.hairColor = source.story.hairColor;
            dest.story.bodyType = source.story.bodyType;
            dest.story.hairDef = source.story.hairDef;
            dest.style.beardDef = source.style.beardDef;
            dest.story.headType = source.story.headType;


            dest.genes.xenotype = source.genes.xenotype;
            dest.genes.xenotypeName = source.genes.xenotypeName;

            if (ModCompatibility.FacialAnimationsIsActive)
            {
                ModCompatibility.CopyFacialFeatures(source, dest);
            }
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
            if (pawn?.health?.hediffSet != null)
            {
                if (pawn.health.hediffSet.GetFirstHediffOfDef(AC_DefOf.VFEU_CorticalStack) is Hediff_CorticalStack hediff)
                {
                    hediff_CorticalStack = hediff;
                    return true;
                }
                else if (pawn.health.hediffSet.GetFirstHediffOfDef(AC_DefOf.AC_ArchoStack) is Hediff_CorticalStack hediff2)
                {
                    hediff_CorticalStack = hediff2;
                    return true;
                }
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
            Ideo_Notify_MemberDied_Patch.disableKilledEffect = true;
        }
        public static bool HasStack(this Pawn pawn)
        {
            return AlteredCarbonManager.Instance.StacksIndex.ContainsKey(pawn.thingIDNumber) 
                || AlteredCarbonManager.Instance.PawnsWithStacks.Contains(pawn);
        }

        public static bool UsesSleeve(this Pawn pawn)
        {
            return sleeveQualities.Exists(def => pawn.health.hediffSet.GetFirstHediffOfDef(def) != null);
        }

        public static HediffDef GetSleeveQuality(this Pawn pawn)
        {
            return sleeveQualities.First(def => pawn.health.hediffSet.HasHediff(def));
        }

        public static Hediff MakeHediff(HediffDef hediffDef, Pawn pawn, BodyPartRecord part)
        {
            return ModCompatibility.RimJobWorldIsActive
                ? rjw.SexPartAdder.MakePart(hediffDef, pawn, part)
                : HediffMaker.MakeHediff(hediffDef, pawn, part);
        }

        public static void CleanupList<T>(this List<T> list, Predicate<T> predicate = null)
        {
            if (list is null) return;

            if (predicate is null)
            {
                predicate = (x => x.IsNullValue());
            }

            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (predicate(list[i]))
                {
                    list.RemoveAt(i);
                }
            }
        }

        public static void CleanupDict<K, V>(this Dictionary<K, V> dict, Predicate<KeyValuePair<K, V>> predicate = null)
        {
            if (dict is null) return;
            if (predicate is null)
            {
                predicate = (x => x.Key.IsNullValue() || x.Value.IsNullValue());
            }
            dict.RemoveAll(predicate);
        }

        public static bool IsNullValue<T>(this T obj)
        {
            return obj is null || typeof(T).GetField("def")?.GetValue(obj) is null;
        }
    }
}

