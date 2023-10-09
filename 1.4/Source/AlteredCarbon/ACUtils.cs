using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    public struct StackInstallInfo
    {
        public RecipeDef recipe;
        public string installLabel;
        public string installDesc;
        public Texture2D installIcon;
    }
    [HotSwappable]
    [StaticConstructorOnStartup]
    public static class ACUtils
    {
        public static Harmony harmony;
        public static AlteredCarbonSettingsWorker_General generalSettings;
        public static AlteredCarbonSettingsWorker_RewriteStack generalRewriteStacks;
        public static Dictionary<string, SleevePreset> presets = new Dictionary<string, SleevePreset>();

        public static HashSet<ThingDef> unstackableRaces;
        public static Dictionary<string, List<GeneDef>> genesByCategories = new Dictionary<string, List<GeneDef>>();
        public static Dictionary<ThingDef, ThingDef> stacksPairs = new Dictionary<ThingDef, ThingDef>
        {
            { AC_DefOf.VFEU_FilledCorticalStack, AC_DefOf.VFEU_EmptyCorticalStack },
        };
        
        public static readonly List<GeneDef> sleeveQualities = new List<GeneDef>
        {
            AC_DefOf.VFEU_SleeveQuality_Awful,
            AC_DefOf.VFEU_SleeveQuality_Poor,
            AC_DefOf.VFEU_SleeveQuality_Normal,
            AC_DefOf.VFEU_SleeveQuality_Good,
            AC_DefOf.VFEU_SleeveQuality_Excellent,
            AC_DefOf.VFEU_SleeveQuality_Masterwork,
            AC_DefOf.VFEU_SleeveQuality_Legendary
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
            {
                AC_DefOf.VFEU_EmptyCorticalStack, new StackInstallInfo
                {
                    recipe = AC_DefOf.VFEU_InstallEmptyCorticalStack,
                    installLabel = "AC.InstallStack".Translate(),
                    installDesc = "AC.InstallEmptyStackDesc".Translate(),
                    installIcon = ContentFinder<Texture2D>.Get("UI/Icons/InstallStack")
                }
            }
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

        public static BodyPartRecord GetNeck(this Pawn pawn)
        {
            return pawn.health.hediffSet.GetNotMissingParts().FirstOrDefault(x => x.def == BodyPartDefOf.Neck);
        }


        private static void PostfixLogMethod(MethodBase __originalMethod)
        {
            Log.Message("Running " + __originalMethod.FullDescription() + " - " + new StackTrace());
            Log.ResetMessageCount();
        }

        private static void AddHarmonyLogging()
        {
            //var postfixLogMethod = AccessTools.Method(typeof(ACUtils), "PostfixLogMethod");
            //foreach (var method in typeof(ACUtils).Assembly.GetTypes().SelectMany(x => x.GetMethods(AccessTools.all)))
            //{
            //    try
            //    {
            //        var toIgnore = new List<string>
            //        {
            //            "PostfixLogMethod"
            //        };
            //        if (toIgnore.Any(x => method.Name.Contains(x)) is false)
            //        {
            //            Log.Message("Patching " + method.FullDescription());
            //            harmony.Patch(method, postfix: new HarmonyMethod(postfixLogMethod));
            //        }
            //
            //    }
            //    catch { }
            //}
        }
        static ACUtils()
        {
            generalSettings = AlteredCarbonMod.modContentPack.Patches.OfType<AlteredCarbonSettingsWorker_General>().First();
            generalRewriteStacks = AlteredCarbonMod.modContentPack.Patches.OfType<AlteredCarbonSettingsWorker_RewriteStack>().First();
            harmony = new Harmony("Altered.Carbon");
            harmony.PatchAll();
            var field = typeof(OverlayDrawer).GetField("NeedsPowerMat", BindingFlags.Static | BindingFlags.NonPublic);
            field.SetValue(null, MaterialPool.MatFrom("UI/Overlays/NeedsPower", ShaderDatabase.MetaOverlay));
            AddHarmonyLogging();
            if (ModCompatibility.HelixienAlteredCarbonIsActive)
            {
                stackRecipesByDef[AC_DefOf.AC_FilledArchoStack] = new StackInstallInfo
                {
                    recipe = AC_DefOf.AC_InstallArchoStack,
                    installLabel = "AC.InstallArchoStack".Translate(),
                    installDesc = "AC.InstallArchoStackDesc".Translate(),
                    installIcon = ContentFinder<Texture2D>.Get("UI/Icons/InstallArchoStack")
                };
                stackRecipesByDef[AC_DefOf.AC_EmptyArchoStack] = new StackInstallInfo
                {
                    recipe = AC_DefOf.AC_InstallEmptyArchoStack,
                    installLabel = "AC.InstallArchoStack".Translate(),
                    installDesc = "AC.InstallEmptyArchoStackDesc".Translate(),
                    installIcon = ContentFinder<Texture2D>.Get("UI/Icons/InstallArchoStack")
                };
                stacksPairs[AC_DefOf.AC_FilledArchoStack] = AC_DefOf.AC_EmptyArchoStack;
                installEmptyStacksRecipes.Add(AC_DefOf.AC_InstallEmptyArchoStack);
                installFilledStacksRecipes.Add(AC_DefOf.AC_InstallArchoStack);
            }
            unstackableRaces = GetUnstackableRaces();
            foreach (var gene in DefDatabase<GeneDef>.AllDefs)
            {
                if (ModCompatibility.VanillaRacesExpandedAndroidIsActive && ModCompatibility.IsAndroidGene(gene))
                {
                    continue;
                }
                if (gene.exclusionTags.NullOrEmpty() is false)
                {
                    for (var i = 0; i < gene.exclusionTags.Count; i++)
                    {
                        var tag = gene.exclusionTags[i];
                        if (tag == "SkinColorOverride" || tag == "SkinColor" || tag == "HairColor")
                        {
                            continue;
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

            ApplySettings();
        }

        public static void ApplySettings()
        {
            if (ACUtils.generalSettings.enableTechprintRequirement is false)
            {
                foreach (var researchProjectDef in DefDatabase<ResearchProjectDef>.AllDefs)
                {
                    if (researchProjectDef.modContentPack == AlteredCarbonMod.modContentPack)
                    {
                        researchProjectDef.techprintCount = 0;
                        researchProjectDef.techprintCommonality = 0;
                        researchProjectDef.techprintMarketValue = 0;
                        researchProjectDef.heldByFactionCategoryTags = null;
                    }
                }
            }
        }

        static HashSet<ThingDef> GetUnstackableRaces()
        {
            if (ModCompatibility.AlienRacesIsActive)
            {
                HashSet<ThingDef> excludedRaces = new HashSet<ThingDef>();
                foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs.Where(def => def.category == ThingCategory.Pawn))
                {
                    if (def.GetModExtension<ExcludeRacesModExtension>() is ExcludeRacesModExtension props)
                    {
                        if (!props.acceptsStacks)
                        {
                            excludedRaces.Add(def);
                        }
                    }
                }
                return excludedRaces;
            }
            else
            {
                return new HashSet<ThingDef>();
            }
        }

        public static bool CanImplantStackTo(HediffDef stackToImplant, Pawn pawn, CorticalStack corticalStack = null, bool throwMessages = false)
        {
            if (corticalStack != null && pawn.IsEmptySleeve() && corticalStack.IsFilledStack is false)
            {
                if (throwMessages)
                {
                    Messages.Message("AC.CannotInstallEmptyStackInEmptySleeve".Translate(), MessageTypeDefOf.RejectInput);
                }
                return false;
            }
            if (unstackableRaces.Contains(pawn.def))
            {
                Messages.Message("AC.CannotInstallStackInBodyOfThisRace".Translate(pawn.def.label), MessageTypeDefOf.RejectInput);
                return false;
            }
            if (pawn.RaceProps.Humanlike is false)
            {
                Messages.Message("AC.CannotInstallStackInNonHumanlikes".Translate(), MessageTypeDefOf.RejectInput);
                return false;
            }
            if (pawn.DevelopmentalStage != DevelopmentalStage.Adult)
            {
                if (corticalStack.IsFilledStack)
                {
                    if (throwMessages)
                    {
                        Messages.Message("AC.CannotInstallFilledStackInChildren".Translate(), MessageTypeDefOf.RejectInput);
                    }
                    return false;
                }
            }
            if (pawn.HasCorticalStack(out var stackHediff)
                && (stackHediff.def == AC_DefOf.AC_ArchoStack || stackToImplant == stackHediff.def))
            {
                if (throwMessages)
                {
                    Messages.Message("AC.CannotInstallAlreadyHaveStack".Translate(), MessageTypeDefOf.RejectInput);
                }
                return false;
            }
            if (ModCompatibility.IsAndroid(pawn))
            {
                if (pawn.genes.HasGene(AC_DefOf.AC_CorticalModule))
                {
                    return true;
                }
                else if (throwMessages)
                {
                    Messages.Message("AC.CannotInstallStackOnAndroidWithoutCorticalModule".Translate(), MessageTypeDefOf.RejectInput);
                    return false;
                }
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
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                try
                {
                    pawn.Drawer.renderer.graphics.nakedGraphic = null;
                    pawn.Drawer.renderer.WoundOverlays.ClearCache();
                    pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                    PortraitsCache.SetDirty(pawn);
                    PortraitsCache.PortraitsCacheUpdate();
                    GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(pawn);
                    if (Find.World != null)
                    {
                        Find.ColonistBar.MarkColonistsDirty();
                    }
                }
                catch { }
            });
        }

        public static Pawn CreateEmptyPawn(PawnKindDef kindDef, Faction faction, ThingDef race = null, long ageBiologicalTicks = default, XenotypeDef xenotypeDef = null)
        {
            Pawn pawn = (Pawn)ThingMaker.MakeThing(race ?? kindDef.race ?? ThingDefOf.Human);
            pawn.kindDef = kindDef;
            pawn.SetFactionDirect(faction);
            PawnComponentsUtility.CreateInitialComponents(pawn);
            if (ageBiologicalTicks != default)
            {
                pawn.ageTracker.AgeBiologicalTicks = ageBiologicalTicks;
            }
            if (xenotypeDef != null)
            {
                pawn.genes.SetXenotype(xenotypeDef);
            }
            return pawn;
        }

        public static Pawn ClonePawn(Pawn source)
        {
            var clone = CreateEmptyPawn(source.kindDef, source.Faction);
            CopyBody(source, clone, copyAgeInfo: true, copyGenesFully: true, copyHealth: true);
            clone.MakeEmptySleeve(keepNaturalAbilities: true, keepPsycastAbilities: true);
            CopyAbilities(source, clone);
            return clone;
        }

        public static void CopyAbilities(Pawn source, Pawn dest)
        {
            var comp = source.GetComp<VFECore.Abilities.CompAbilities>();
            if (comp != null && comp.LearnedAbilities != null)
            {
                var veAbilities = comp.LearnedAbilities.Select(x => x.def).ToList();
                var otherComp = dest.GetComp<VFECore.Abilities.CompAbilities>();
                if (otherComp != null)
                {
                    foreach (var ability in veAbilities)
                    {
                        otherComp.GiveAbility(ability);
                    }
                }
            }

            var abilities = source.abilities?.abilities;
            if (abilities != null)
            {
                foreach (var ability in abilities)
                {
                    dest.abilities.GainAbility(ability.def);
                }
            }

            if (ModCompatibility.VanillaFactionsExpandedAncientsIsActive)
            {
                var powers = ModCompatibility.GetPowers(source);
                if (powers != null && powers.Any())
                {
                    ModCompatibility.SetPowers(dest, powers);
                }
            }
        }

        public static void CopyBody(Pawn source, Pawn dest, bool copyAgeInfo = false, bool copyGenesPartially = false, bool copyGenesFully = false, 
            bool copyHealth = false)
        {
            dest.gender = source.gender;
            if (ModCompatibility.AlienRacesIsActive)
            {
                ModCompatibility.CopyBodyAddons(source, dest);
                ModCompatibility.SetSkinColorFirst(dest, ModCompatibility.GetSkinColorFirst(source));
                ModCompatibility.SetSkinColorSecond(dest, ModCompatibility.GetSkinColorSecond(source));
                ModCompatibility.SetHairColorFirst(dest, ModCompatibility.GetHairColorFirst(source));
                ModCompatibility.SetHairColorSecond(dest, ModCompatibility.GetHairColorSecond(source));
            }

            if (copyGenesPartially || copyGenesFully)
            {
                var genes = dest.genes.GenesListForReading;
                foreach (var oldGene in genes)
                {
                    dest.genes.RemoveGene(oldGene);
                }
            }

            if (copyGenesPartially)
            {
                CopyEndogenes(source, dest);
            }
            else if (copyGenesFully)
            {
                CopyEndogenes(source, dest);
                CopyXenogenes(source, dest);
            }

            dest.health.hediffSet.hediffs.Clear();
            if (copyHealth)
            {
                foreach (var hediff in source.health.hediffSet.hediffs)
                {
                    var newHediff = hediff.Clone();
                    newHediff.loadID = Find.UniqueIDsManager.GetNextHediffID();
                    newHediff.pawn = dest;
                    dest.health.hediffSet.hediffs.Add(newHediff);
                }
            }

            if (ModCompatibility.RimJobWorldIsActive)
            {
                ModCompatibility.CopyPrivateParts(source, dest);
            }

            if (copyAgeInfo)
            {
                dest.ageTracker = source.ageTracker.Clone();
                dest.ageTracker.pawn = dest;
            }

            dest.genes.xenotype = source.genes.xenotype;
            dest.genes.xenotypeName = source.genes.xenotypeName;

            dest.story.skinColorOverride = source.story.skinColorOverride;
            dest.story.skinColorBase = source.story.skinColorBase;
            dest.story.hairColor = source.story.hairColor;
            dest.story.bodyType = source.story.bodyType;
            dest.story.hairDef = source.story.hairDef;
            dest.style.beardDef = source.style.beardDef;
            dest.story.headType = source.story.headType;

            if (ModCompatibility.FacialAnimationsIsActive)
            {
                ModCompatibility.CopyFacialFeatures(source, dest);
            }
        }

        public static string GetFullName(this Pawn pawn)
        {
            return pawn.Name.ToStringFull + " - " + pawn.thingIDNumber;
        }
        private static void CopyEndogenes(Pawn source, Pawn dest)
        {
            var genes = source.genes.Endogenes.Where(x => x?.def != null).ToList();
            CopyGenes(dest, genes, false);
        }
        private static void CopyXenogenes(Pawn source, Pawn dest)
        {
            var genes = source.genes.Xenogenes.Where(x => x?.def != null).ToList();
            CopyGenes(dest, genes, true);
        }
        private static void CopyGenes(Pawn dest, List<Gene> sourceGenes, bool xenogene)
        {
            var copiedGenes = new List<Gene>();
            foreach (var sourceGene in sourceGenes)
            {
                copiedGenes.Add(dest.genes.AddGene(sourceGene.def, xenogene));
            }
            for (var i = 0; i < sourceGenes.Count; i++)
            {
                var originalGene = sourceGenes[i];
                var copiedGene = copiedGenes[i];
                if (copiedGene != null)
                {
                    if (originalGene.Active)
                    {
                        GeneUtils.ApplyGene(copiedGene, dest);
                    }
                }
            }
        }


        public static void AddTakeEmptySleeveJob(Pawn pawn, Pawn pawnTarget, bool failMessage)
        {
            Building_Bed building_Bed3 = RestUtility.FindBedFor(pawnTarget, pawn, checkSocialProperness: false);
            if (building_Bed3 == null)
            {
                building_Bed3 = RestUtility.FindBedFor(pawnTarget, pawn, checkSocialProperness: false, ignoreOtherReservations: true);
            }
            if (building_Bed3 == null)
            {
                if (failMessage)
                {
                    Messages.Message("AC.CannotTakeToSleeveCasketOrMedicalBed".Translate(), pawnTarget, MessageTypeDefOf.RejectInput, historical: false);
                }
            }
            else
            {
                Job job28 = JobMaker.MakeJob(AC_DefOf.AC_TakeEmptySleeve, pawnTarget, building_Bed3);
                job28.count = 1;
                pawn.jobs.TryTakeOrderedJob(job28, JobTag.Misc);
            }
        }

        public static void LockBehindReseach(this Command_Action command, List<ResearchProjectDef> researchProjects)
        {
            if (researchProjects != null && researchProjects.Any() && IsResearchFinished(researchProjects) is false)
            {
                command.Disable("MissingRequiredResearch".Translate() + ": " + (from x in researchProjects where !x.IsFinished select x.label)
                    .ToCommaList(useAnd: true).CapitalizeFirst());
            }
        }
        public static bool IsResearchFinished(List<ResearchProjectDef> research)
        {
            for (int i = 0; i < research.Count; i++)
            {
                if (!research[i].IsFinished)
                {
                    return false;
                }
            }
            return true;
        }
        public static string PawnTemplatesPath => Path.Combine(GenFilePaths.ConfigFolderPath, "AC_PawnTemplates.xml");

        public static void SavePresets()
        {
            Scribe.saver.InitSaving(PawnTemplatesPath, "PawnTemplates");
            Scribe_Collections.Look(ref ACUtils.presets, "presets", LookMode.Value, LookMode.Deep);
            Scribe.saver.FinalizeSaving();
        }

        public static void LoadPresets()
        {
            FileInfo info = new FileInfo(PawnTemplatesPath);
            if (info.Exists)
            {
                Scribe.loader.InitLoading(PawnTemplatesPath);
                Scribe_Collections.Look(ref ACUtils.presets, "presets", LookMode.Value, LookMode.Deep);
                Scribe.loader.FinalizeLoading();
            }
        }

        public static IEnumerable<Xenogerm> GetXenogerms(this Map map)
        {
            return map.listerThings.ThingsOfDef(ThingDefOf.Xenogerm).OfType<Xenogerm>().Where(x => 
            x.PositionHeld.Fogged(map) is false && !x.IsForbidden(Faction.OfPlayer));
        }
        public static bool HasCorticalStack(this Pawn pawn)
        {
            return pawn.HasCorticalStack(out _);
        }

        public static void ResetInitialComponents(this Pawn pawn)
        {
            pawn.records = new Pawn_RecordsTracker(pawn);
            pawn.meleeVerbs = new Pawn_MeleeVerbs(pawn);
            pawn.verbTracker = new VerbTracker(pawn);
            pawn.mindState = new Pawn_MindState(pawn);
            pawn.ownership = new Pawn_Ownership(pawn);
            pawn.connections = new Pawn_ConnectionsTracker(pawn);
            pawn.thinker = new Pawn_Thinker(pawn);
            pawn.jobs = new Pawn_JobTracker(pawn);
            pawn.stances = new Pawn_StanceTracker(pawn);
            pawn.skills = new Pawn_SkillTracker(pawn);
            pawn.guest = new Pawn_GuestTracker(pawn);
            pawn.guilt = new Pawn_GuiltTracker(pawn);
            pawn.playerSettings = new Pawn_PlayerSettings(pawn);
            pawn.workSettings = new Pawn_WorkSettings(pawn);
            pawn.royalty = new Pawn_RoyaltyTracker(pawn);
            pawn.ideo = new Pawn_IdeoTracker(pawn);
            pawn.styleObserver = new Pawn_StyleObserverTracker(pawn);
            pawn.surroundings = new Pawn_SurroundingsTracker(pawn);
            pawn.abilities = new Pawn_AbilityTracker(pawn);
            foreach (var rel in pawn.relations.directRelations)
            {
                rel.otherPawn.relations.directRelations.RemoveAll(x => x.otherPawn == pawn);
            }
            pawn.relations = new Pawn_RelationsTracker(pawn);
            pawn.psychicEntropy = new Pawn_PsychicEntropyTracker(pawn);
            pawn.timetable = new Pawn_TimetableTracker(pawn);
            pawn.needs ??= new Pawn_NeedsTracker(pawn);
            pawn.needs.mood = new Need_Mood(pawn);
            pawn.story.traits = new TraitSet(pawn);
        }

        public static void MakeEmptySleeve(this Pawn pawn, bool keepNaturalAbilities, bool keepPsycastAbilities)
        {
            var entropy = pawn.psychicEntropy.currentEntropy;
            var psyfocus = pawn.psychicEntropy.currentPsyfocus;
            var oldAbilities = pawn.abilities?.abilities.Select(x => x.def).ToList();
            pawn.ResetInitialComponents();
            if (keepNaturalAbilities)
            {
                if (oldAbilities != null)
                {
                    foreach (var ability in oldAbilities)
                    {
                        if (PersonaData.IsNaturalAbility(pawn, ability))
                        {
                            pawn.abilities.GainAbility(ability);
                        }

                    }
                }
            }
            if (keepPsycastAbilities)
            {
                if (oldAbilities != null)
                {
                    foreach (var ability in oldAbilities)
                    {
                        if (PersonaData.IsPsycastAbility(ability))
                        {
                            pawn.abilities.GainAbility(ability);
                        }
                    }
                }

                pawn.psychicEntropy.currentEntropy = Mathf.Max(0, entropy);
                pawn.psychicEntropy.currentPsyfocus = Mathf.Max(0, psyfocus);
            }
            if (pawn.Faction != null)
            {
                pawn.SetFaction(null);
            }
            pawn.guest.recruitable = true;
            pawn.Name = new NameSingle("AC.EmptySleeve".Translate());
            pawn.story.title = null;
            pawn.playerSettings.medCare = MedicalCareCategory.Best;
            pawn.workSettings.EnableAndInitialize();
            pawn.skills.Notify_SkillDisablesChanged();
            pawn.story.Childhood = AC_DefOf.VFEU_VatGrownChild;
            pawn.story.Adulthood = AC_DefOf.VFEU_VatGrownAdult;
            pawn.story.favoriteColor = null;
            if (ModsConfig.IdeologyActive)
            {
                pawn.style.BodyTattoo = TattooDefOf.NoTattoo_Body;
                pawn.style.FaceTattoo = TattooDefOf.NoTattoo_Face;
            }
            AlteredCarbonManager.Instance.emptySleeves.Add(pawn);
        }
        public static IEnumerable<Hediff_MissingPart> GetMissingParts(this Pawn pawn)
        {
            return pawn.health.hediffSet.hediffs.OfType<Hediff_MissingPart>();
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
            if (pawn.HasCorticalStack(out var hediff))
            {
                var stackGroupData = hediff.PersonaData.StackGroupData;
                if (stackGroupData.copiedPawns.Contains(pawn))
                {
                    return true;
                }
            }
            else
            {
                foreach (var stackGroup in AlteredCarbonManager.Instance.stacksRelationships)
                {
                    if (stackGroup.Value.copiedPawns.Contains(pawn))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsEmptySleeve(this Pawn pawn)
        {
            return pawn.Dead is false && AlteredCarbonManager.Instance.emptySleeves.Contains(pawn);
        }
        public static void DisableKillEffects(this Pawn pawn)
        {
            Pawn_DoKillSideEffects.disableKillEffect = pawn;
            Faction_Notify_LeaderDied_Patch.disableKillEffect = pawn;
            PawnDiedOrDownedThoughtsUtility_AppendThoughts_ForHumanlike_Patch.disableKillEffect = pawn;
            PawnDiedOrDownedThoughtsUtility_AppendThoughts_Relations_Patch.disableKillEffect = pawn;
            Pawn_HealthTracker_NotifyPlayerOfKilled_Patch.disableKillEffect = pawn;
            Pawn_RoyaltyTracker_Notify_PawnKilled_Patch.disableKillEffect = pawn;
            Ideo_Notify_MemberDied_Patch.disableKillEffect = pawn;
        }

        public static void EnableKillEffects(this Pawn pawn)
        {
            Pawn_DoKillSideEffects.disableKillEffect = null;
            Faction_Notify_LeaderDied_Patch.disableKillEffect = null;
            PawnDiedOrDownedThoughtsUtility_AppendThoughts_ForHumanlike_Patch.disableKillEffect = null;
            PawnDiedOrDownedThoughtsUtility_AppendThoughts_Relations_Patch.disableKillEffect = null;
            Pawn_HealthTracker_NotifyPlayerOfKilled_Patch.disableKillEffect = null;
            Pawn_RoyaltyTracker_Notify_PawnKilled_Patch.disableKillEffect = null;
            Ideo_Notify_MemberDied_Patch.disableKillEffect = null;
        }

        public static Hediff MakeHediff(HediffDef hediffDef, Pawn pawn, BodyPartRecord part)
        {
            return ModCompatibility.RimJobWorldIsActive
                ? rjw.SexPartAdder.MakePart(hediffDef, pawn, part)
                : HediffMaker.MakeHediff(hediffDef, pawn, part);
        }

        public static bool HasStackInsideOrOutside(this Pawn pawn)
        {
            return AlteredCarbonManager.Instance.StacksIndex.ContainsKey(pawn.thingIDNumber)
                || AlteredCarbonManager.Instance.PawnsWithStacks.Contains(pawn);
        }

        public static bool UsesSleeve(this Pawn pawn)
        {
            return sleeveQualities.Exists(def => pawn.genes.GetGene(def) != null);
        }

        public static GeneDef GetSleeveQuality(this Pawn pawn)
        {
            return sleeveQualities.First(def => pawn.genes.GetGene(def) != null);
        }
        
        public static bool AcceptsStacks(this Pawn p)
        {
            if (p.def.GetModExtension<ExcludeRacesModExtension>() is ExcludeRacesModExtension props)
            {
                return (props.acceptsStacks);
            }

            return true;
        }

        public static bool SleeveMatchesOriginalXenotype(this Pawn p, PersonaData stackPersonaData)
        {
            return stackPersonaData.OriginalXenotypeName != null && p.genes.xenotypeName != stackPersonaData.OriginalXenotypeName
                   || stackPersonaData.OriginalXenotypeDef != null && p.genes.xenotype != stackPersonaData.OriginalXenotypeDef;
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

        public static List<T> CopyList<T>(this List<T> list)
        {
            if (list is null) return new List<T>();
            return list.ToList();
        }

        public static Dictionary<K, V> CopyDict<K, V>(this Dictionary<K, V> dict)
        {
            if (dict is null) return new Dictionary<K, V>();
            return dict.ToDictionary(x => x.Key, x => x.Value);
        }

        public static bool IsNullValue<T>(this T obj)
        {
            var isNull = obj is null;
            if (!isNull)
            {
                var field = typeof(T).GetField("def");
                if (field != null)
                {
                    if (field.GetValue(obj) is null)
                    {
                        isNull = true;
                    }
                }
            }
            return isNull;
        }

        public static T Clone<T>(this T obj)
        {
            var inst = obj.GetType().GetMethod("MemberwiseClone", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            return (T)inst?.Invoke(obj, null);
        }
    }
}

