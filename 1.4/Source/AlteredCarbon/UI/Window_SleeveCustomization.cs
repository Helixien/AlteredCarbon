using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;
using Verse.Sound;
using static AlteredCarbon.UIHelper;

namespace AlteredCarbon
{
    [HotSwappable]
    public class Window_SleeveCustomization : Window
    {
        //Variables
        public Pawn curSleeve;
        private PawnKindDef currentPawnKindDef;
        private readonly Building_SleeveGrower sleeveGrower;
        private Xenogerm curXenogerm;
        private bool convertXenogenesToEndegones;
        private List<Gene> convertedGenes = new List<Gene>();

        private bool allowMales = true;
        private bool allowFemales = true;

        // UI variables
        private readonly float leftOffset = 20;
        private readonly float pawnSpacingFromEdge = 5;
        private int scrollHeightCount = 0;
        private Vector2 scrollPosition;

        // indexes for lists
        private Dictionary<string, int> indexesPerCategory;
        private int hairTypeIndex = 0;
        private int beardTypeIndex = 0;
        private int raceTypeIndex = 0;
        private int headTypeIndex = 0;
        private int maleBodyTypeIndex = 0;
        private int femaleBodyTypeIndex = 0;
        private int sleeveQualityIndex = 2;

        private int ticksToGrow = 900000;
        private int growCost = 250;
        private HediffDef qualityDiff;

        private static readonly List<HediffDef> sleeveQualities = new List<HediffDef>
        {
            AC_DefOf.VFEU_Sleeve_Quality_Awful,
            AC_DefOf.VFEU_Sleeve_Quality_Poor,
            AC_DefOf.VFEU_Sleeve_Quality_Normal,
            AC_DefOf.VFEU_Sleeve_Quality_Good,
            AC_DefOf.VFEU_Sleeve_Quality_Excellent,
            AC_DefOf.VFEU_Sleeve_Quality_Masterwork,
            AC_DefOf.VFEU_Sleeve_Quality_Legendary
        };

        public static Dictionary<HediffDef, int> sleeveQualitiesTimeCost = new Dictionary<HediffDef, int>
        {
            {AC_DefOf.VFEU_Sleeve_Quality_Awful, 0 },
            {AC_DefOf.VFEU_Sleeve_Quality_Poor, GenDate.TicksPerDay * 2 },
            {AC_DefOf.VFEU_Sleeve_Quality_Normal, GenDate.TicksPerDay * 3 },
            {AC_DefOf.VFEU_Sleeve_Quality_Good, GenDate.TicksPerDay * 5 },
            {AC_DefOf.VFEU_Sleeve_Quality_Excellent, GenDate.TicksPerDay * 10 },
            {AC_DefOf.VFEU_Sleeve_Quality_Masterwork, GenDate.TicksPerDay * 15 },
            {AC_DefOf.VFEU_Sleeve_Quality_Legendary, GenDate.TicksPerDay * 30 },
        };

        //Static Values
        [TweakValue("0AC", 0, 200)] public static float AlienRacesYOffset = 32;
        [TweakValue("0AC", 500, 1500)] public static float InitialWindowXSize = 728f;
        [TweakValue("0AC", 500, 1000)] public static float InitialWindowYSize = 690f;
        [TweakValue("0AC", 300, 500)] public static float LineSeparatorWidth = 500;

        public override Vector2 InitialSize
        {
            get
            {
                float xSize = 900;
                float ySize = UI.screenHeight;
                return new Vector2(xSize, ySize);
            }
        }

        public Window_SleeveCustomization(Building_SleeveGrower sleeveGrower)
        {
            this.sleeveGrower = sleeveGrower;
            Init(PawnKindDefOf.Colonist);
            InitUI();
        }

        public Window_SleeveCustomization(Building_SleeveGrower sleeveGrower, Pawn pawnToClone)
        {
            this.sleeveGrower = sleeveGrower;
            Init(pawnToClone.kindDef, pawnToClone.gender);
            CopyBodyFrom(pawnToClone);
            InitUI();
        }

        private void InitUI()
        {
            forcePause = true;
            absorbInputAroundWindow = false;
        }

        private void Init(PawnKindDef pawnKindDef, Gender? gender = null)
        {
            currentPawnKindDef = pawnKindDef; ;
            if (!gender.HasValue)
            {
                gender = Rand.Bool ? Gender.Male : Gender.Female;
            }
            CreateSleeve(gender.Value);
        }


        public override void DoWindowContents(Rect inRect)
        {

            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(new Rect(0f, 0f, inRect.width, 32f), "AC.SleeveCustomization".Translate());

            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;

            float innerRectYOffset = 50;
            Vector2 firstColumnPos = new Vector2(leftOffset, innerRectYOffset);
            Vector2 secondColumnPos = new Vector2(600, firstColumnPos.y);

            var outRect = new Rect(0, firstColumnPos.y, inRect.width, inRect.height - 250);
            var viewRect = new Rect(0, outRect.y, inRect.width - 30, scrollHeightCount);
            scrollHeightCount = 0;
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            Text.Anchor = TextAnchor.MiddleLeft;
            var genderLabel = "Gender".Translate() + ":";
            Rect genderRect = GetLabelRect(genderLabel, ref firstColumnPos);
            Widgets.Label(genderRect, genderLabel);
            Rect maleGenderRect = new Rect(genderRect.xMax + buttonOffsetFromText, genderRect.y, buttonWidth, buttonHeight);
            Rect femaleGenderRect = new Rect(maleGenderRect.xMax + buttonOffsetFromButton, genderRect.y, buttonWidth, buttonHeight);
            if (allowMales && Widgets.ButtonText(maleGenderRect, "Male".Translate().CapitalizeFirst()))
            {
                SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                CreateSleeve(Gender.Male);
            }
            if (allowFemales && Widgets.ButtonText(femaleGenderRect, "Female".Translate().CapitalizeFirst()))
            {
                SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                CreateSleeve(Gender.Female);
            }

            if (Widgets.ButtonText(femaleGenderRect, "Female".Translate().CapitalizeFirst()))
            {
                SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                CreateSleeve(Gender.Female);
            }

            var label = "SelectXenogerm".Translate() + ":";
            Rect labelRect = GetLabelRect(label, ref firstColumnPos);
            Widgets.Label(labelRect, label);
            Rect highlightRect = new Rect(labelRect.xMax + buttonOffsetFromText, labelRect.y, (buttonWidth * 2) + buttonOffsetFromButton,
                buttonHeight);

            if (Widgets.ButtonText(highlightRect, curXenogerm != null ? curXenogerm.LabelCap : "-"))
            {
                Find.WindowStack.Add(new Dialog_SelectXenogermForSleeve(curSleeve, sleeveGrower.Map, curXenogerm, delegate (Xenogerm x)
                {
                    curXenogerm = x;
                    GeneUtility.ImplantXenogermItem(curSleeve, curXenogerm);
                    convertedGenes = new List<Gene>();
                    RecheckBodyOptions();
                    InitializeIndexes();
                }));
            }

            DrawExplanation(ref firstColumnPos, highlightRect.width + labelWidth + 15, 32, "AC.XenogermExplanation".Translate());
            if (ModCompatibility.AlienRacesIsActive)
            {
                var permittedRaces = GetPermittedRaces();
                DoSelectionButtons(ref firstColumnPos, "AC.SelectRace".Translate(), ref raceTypeIndex,
                    (ThingDef x) => x.LabelCap, permittedRaces, delegate (ThingDef x)
                    {
                        currentPawnKindDef.race = x;
                        CreateSleeve(curSleeve.gender);
                        raceTypeIndex = permittedRaces.IndexOf(x);
                    });
            }

            var genes = curSleeve.genes?.GenesListForReading;
            if (curXenogerm != null)
            {
                bool geneOptionsDrawn = false;
                foreach (var category in ACUtils.genesByCategories)
                {
                    var xenogermGenes = curXenogerm.GeneSet.genes.Where(x => x.exclusionTags.NullOrEmpty() is false
                    && x.exclusionTags.Contains(category.Key)).ToList();
                    if (xenogermGenes.Count > 1)
                    {
                        if (geneOptionsDrawn is false)
                        {
                            ListSeparator(ref firstColumnPos, LineSeparatorWidth, "AC.GeneOptions".Translate());
                            firstColumnPos.y += 5f;
                            geneOptionsDrawn = true;
                        }
                        var index = indexesPerCategory[category.Key];
                        DoSelectionButtons(ref firstColumnPos, category.Key.SplitCamelCase().FirstCharToUpper(), ref index,
                            (GeneDef x) => x.LabelCap, xenogermGenes, delegate (GeneDef x)
                            {
                                ApplyGene(x);
                                RecheckEverything();
                                indexesPerCategory[category.Key] = xenogermGenes.IndexOf(x);
                            });
                    }
                }
                if (ModCompatibility.HelixienAlteredCarbonIsActive)
                {
                    label = "AC.ConvertGenesToGermline".Translate();
                    var size = Text.CalcSize(label);
                    labelRect = GetLabelRect(label, ref firstColumnPos, size.x + 15);
                    Widgets.Label(labelRect, label);
                    bool oldValue = convertXenogenesToEndegones;
                    Widgets.Checkbox(new Vector2(labelRect.xMax, labelRect.y), ref convertXenogenesToEndegones);
                    if (oldValue != convertXenogenesToEndegones)
                    {
                        if (convertXenogenesToEndegones)
                        {
                            convertedGenes = genes.Where(x => curSleeve.genes.Xenogenes.Contains(x) 
                                && x.def.displayCategory != GeneCategoryDefOf.Archite).ToList();
                            curSleeve.genes.endogenes.AddRange(convertedGenes);
                            curSleeve.genes.xenogenes.RemoveAll(x => convertedGenes.Contains(x));
                        }
                        else
                        {
                            curSleeve.genes.endogenes.RemoveAll(x => convertedGenes.Contains(x));
                            curSleeve.genes.xenogenes.AddRange(convertedGenes);
                        }
                        RecheckEverything();
                    }
                }

                if (geneOptionsDrawn)
                {
                    DrawExplanation(ref firstColumnPos, highlightRect.width + labelWidth + 15, 32, "AC.GeneOptionsExplanation".Translate());
                }
            }
            ListSeparator(ref firstColumnPos, LineSeparatorWidth, "AC.BodyOptions".Translate());
            firstColumnPos.y += 5f;
            DoColorButtons(ref firstColumnPos, "AC.SkinColour".Translate(), GetPermittedSkinColors(), (KeyValuePair<GeneDef, Color> x) => x.Value,
                delegate (KeyValuePair<GeneDef, Color> selected)
                {
                    var gene = ApplyGene(selected.Key);
                    if (selected.Key.endogeneCategory == EndogeneCategory.Melanin)
                    {
                        var melaninGene = curSleeve.genes.GenesListForReading
                            .FirstOrDefault(x => x.def.endogeneCategory == EndogeneCategory.Melanin);
                        if (melaninGene != null && gene != melaninGene)
                        {
                            curSleeve.genes.RemoveGene(melaninGene);
                        }
                    }
                    RecheckEverything();
                });

            var permittedHeads = GetPermittedHeads();
            DoSelectionButtons(ref firstColumnPos, "AC.HeadShape".Translate(), ref headTypeIndex,
                (KeyValuePair<GeneDef, HeadTypeDef> x) => ExtractHeadLabels(x.Value.defName), permittedHeads,
                delegate (KeyValuePair<GeneDef, HeadTypeDef> selected)
                {
                    if (selected.Key != null)
                    {
                        ApplyGene(selected.Key);
                    }
                    else
                    {
                        curSleeve.story.headType = selected.Value;
                    }
                    RecheckEverything();
                    headTypeIndex = permittedHeads.IndexOf(selected);
                });

            if (curSleeve.gender == Gender.Male)
            {
                var permittedBodyTypes = GetPermittedBodyTypes();
                DoSelectionButtons(ref firstColumnPos, "AC.BodyShape".Translate(), ref maleBodyTypeIndex,
                    (KeyValuePair<GeneDef, BodyTypeDef> x) => x.Value.defName, permittedBodyTypes, delegate (KeyValuePair<GeneDef, BodyTypeDef> x)
                    {
                        if (x.Key != null)
                        {
                            ApplyGene(x.Key);
                        }
                        else
                        {
                            curSleeve.story.bodyType = x.Value;
                        }
                        RecheckEverything();
                        maleBodyTypeIndex = permittedBodyTypes.IndexOf(x);
                    });
            }
            else if (curSleeve.gender == Gender.Female)
            {
                var permittedBodyTypes = GetPermittedBodyTypes();
                DoSelectionButtons(ref firstColumnPos, "AC.BodyShape".Translate(), ref femaleBodyTypeIndex,
                    (KeyValuePair<GeneDef, BodyTypeDef> x) => x.Value.defName, permittedBodyTypes, delegate (KeyValuePair<GeneDef, BodyTypeDef> x)
                    {
                        if (x.Key != null)
                        {
                            ApplyGene(x.Key);
                        }
                        else
                        {
                            curSleeve.story.bodyType = x.Value;
                        }
                        RecheckEverything();
                        femaleBodyTypeIndex = permittedBodyTypes.IndexOf(x);
                    });
            }
            var permittedHairs = GetPermittedHairs();
            if (!permittedHairs.NullOrEmpty())
            {
                DoColorButtons(ref firstColumnPos, "AC.HairColour".Translate(), GetPermittedHairColors(), (KeyValuePair<GeneDef, Color> x) => x.Value,
                    delegate (KeyValuePair<GeneDef, Color> selected)
                    {
                        var gene = ApplyGene(selected.Key);
                        var hairGene = curSleeve.genes.GenesListForReading
                            .FirstOrDefault(x => selected.Key.endogeneCategory == x.def.endogeneCategory);
                        if (hairGene != null && gene != hairGene)
                        {
                            curSleeve.genes.RemoveGene(hairGene);
                        }
                        RecheckEverything();
                    });

                DoSelectionButtons(ref firstColumnPos, "AC.HairType".Translate(), ref hairTypeIndex,
                (HairDef x) => x.LabelCap, permittedHairs, delegate (HairDef x)
                {
                    curSleeve.story.hairDef = x;
                    RecheckEverything();
                    hairTypeIndex = permittedHairs.IndexOf(x);
                });
            }
            var permittedBeards = GetPermittedBeards();
            DoSelectionButtons(ref firstColumnPos, "AC.BeardType".Translate(), ref beardTypeIndex,
                (BeardDef x) => x.LabelCap, permittedBeards, delegate (BeardDef x)
                {
                    curSleeve.style.beardDef = x;
                    RecheckEverything();
                    beardTypeIndex = permittedBeards.IndexOf(x);
                });

            DoSelectionButtons(ref firstColumnPos, "AC.SleeveQuality".Translate(), ref sleeveQualityIndex,
                (HediffDef x) => GetQualityLabel(sleeveQualities.IndexOf(x)), sleeveQualities, delegate (HediffDef x)
                {
                    sleeveQualityIndex = sleeveQualities.IndexOf(x);
                    ApplyHediffs();
                    UpdateGrowCost();
                });

            DrawExplanation(ref firstColumnPos, highlightRect.width + labelWidth + 15, 32, "AC.BodyOptionsExplanation".Translate());

            //Pawn Box
            Rect pawnBox = new Rect(secondColumnPos.x, secondColumnPos.y, 200, 200);
            Widgets.DrawMenuSection(pawnBox);
            Widgets.DrawShadowAround(pawnBox);
            Rect pawnBoxPawn = new Rect(pawnBox.x + pawnSpacingFromEdge, pawnBox.y + pawnSpacingFromEdge, pawnBox.width - (pawnSpacingFromEdge * 2), pawnBox.height - (pawnSpacingFromEdge * 2));
            UpdateSleeveGraphic();
            GUI.DrawTexture(pawnBoxPawn, PortraitsCache.Get(curSleeve, pawnBoxPawn.size, curSleeve.Rotation, default, 1f));
            Widgets.InfoCardButton(pawnBox.x + pawnBox.width - Widgets.InfoCardButtonSize - 10f, pawnBox.y + pawnBox.height - 
                Widgets.InfoCardButtonSize - 10f, curSleeve);
            if (Widgets.ButtonImage(new Rect(pawnBox.x + pawnBox.width - Widgets.InfoCardButtonSize - 10f, pawnBox.y + 10, 24, 24), RotateSleeve))
            {
                curSleeve.Rotation = curSleeve.Rotation.Rotated(RotationDirection.Clockwise);
            }

            secondColumnPos.y = pawnBox.yMax + 15;
            //Hediff printout
            var heDiffListing = HealthCardUtility.VisibleHediffGroupsInOrder(curSleeve, false);
            List<Hediff> diffs = heDiffListing.SelectMany(group => group).ToList();
            Rect healthBoxLabel = GetLabelRect(ref secondColumnPos, 150, 32);
            Text.Font = GameFont.Medium;
            Widgets.Label(healthBoxLabel, "AC.SleeveHealthPreview".Translate().CapitalizeFirst());
            Rect healthBox = new Rect(pawnBox.x - 15, healthBoxLabel.yMax, pawnBox.width + 30, (diffs.Count * 25f) + 10);
            Widgets.DrawHighlight(healthBox);
            GUI.color = HealthUtility.GoodConditionColor;
            Listing_Standard diffListing = new Listing_Standard();
            diffListing.Begin(healthBox.ContractedBy(5));
            Text.Anchor = TextAnchor.MiddleLeft;
            for (int ii = 0; ii < diffs.Count; ++ii)
            {
                diffListing.Label(diffs[ii].LabelCap);
            }
            diffListing.End();

            GUI.color = Color.white;
            if (Mouse.IsOver(healthBox))
            {
                Widgets.DrawHighlight(healthBox);
                TooltipHandler.TipRegion(healthBox, new TipSignal(() => GetHediffToolTip(diffs, curSleeve), 1147682));
            }

            secondColumnPos.y = healthBox.yMax + 15;

            Text.Font = GameFont.Small;
            if (genes == null || genes.Count == 0)
            {
                Color color = GUI.color;
                GUI.color = Color.gray;
                Rect rect13 = new Rect(secondColumnPos.x - 15, secondColumnPos.y, pawnBox.width + 30, 24f);
                if (Mouse.IsOver(rect13))
                {
                    Widgets.DrawHighlight(rect13);
                }
                Widgets.Label(rect13, "None".Translate());
                TooltipHandler.TipRegionByKey(rect13, "None");
                GUI.color = color;
            }
            else
            {
                Rect geneBox = default;
                Rect rect = default;

                var endogenes = genes.Where(x => curSleeve.genes.Endogenes.Contains(x)).ToList();
                DrawGenes(ref secondColumnPos, "Endogenes".Translate().CapitalizeFirst(), pawnBox, healthBox, ref geneBox, endogenes, ref rect);
                var xenogenes = genes.Where(x => curSleeve.genes.Xenogenes.Contains(x)).ToList();
                DrawGenes(ref secondColumnPos, "Xenogenes".Translate().CapitalizeFirst(), pawnBox, healthBox, ref geneBox, xenogenes, ref rect);
            }

            if (ModCompatibility.RimJobWorldIsActive && ModCompatibility.HelixienAlteredCarbonIsActive)
            {
                Rect setBodyParts = new Rect(healthBox.x, secondColumnPos.y, healthBox.width, buttonHeight);
                if (ButtonTextSubtleCentered(setBodyParts, "AC.SetBodyParts".Translate().CapitalizeFirst()))
                {
                    Find.WindowStack.Add(new Window_BodyPartPicker(curSleeve, this));
                }
            }

            Widgets.EndScrollView();
            scrollHeightCount = (int)(Mathf.Max(firstColumnPos.y, secondColumnPos.y) - innerRectYOffset);

            firstColumnPos.x = leftOffset;
            firstColumnPos.y = (inRect.y + inRect.height) - 170;
            Rect timeToGrowRect = GetLabelRect(ref firstColumnPos, 300, 32);
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(timeToGrowRect, "AC.TimeToGrow".Translate(GenDate.ToStringTicksToDays(ticksToGrow)));
            Text.Font = GameFont.Small;
            Rect growCostRect = GetLabelRect(ref firstColumnPos, inRect.width, 32);
            Widgets.Label(growCostRect, "  " + "AC.GrowCost".Translate(growCost));
            Widgets.DrawHighlight(new Rect(growCostRect.x, growCostRect.y, inRect.width - 50, growCostRect.height));
            Text.Anchor = TextAnchor.UpperLeft;

            DrawExplanation(ref firstColumnPos, inRect.width - 50, 50, "AC.SleeveCustomizationExplanation".Translate());

            Rect saveTemplateRect = new Rect(inRect.x, inRect.y + inRect.height - 32, 180, 32);
            if (Widgets.ButtonText(saveTemplateRect, "AC.SaveTemplate".Translate()))
            {
                Find.WindowStack.Add(new Dialog_PresetList_Save(this));
            }
            Rect loadTemplateRect = new Rect(saveTemplateRect.xMax + 15, saveTemplateRect.y, saveTemplateRect.width, saveTemplateRect.height);
            if (Widgets.ButtonText(loadTemplateRect, "AC.LoadTemplate".Translate()))
            {
                Find.WindowStack.Add(new Dialog_PresetList_Load(this));
            }

            Rect acceptRect = new Rect(inRect.xMax - (loadTemplateRect.width * 2f) - 15, loadTemplateRect.y, loadTemplateRect.width, loadTemplateRect.height);
            if (Widgets.ButtonText(acceptRect, "Accept".Translate().CapitalizeFirst()))
            {
                sleeveGrower.StartGrowth(curSleeve, curXenogerm, ticksToGrow, growCost);
                Close();
            }

            Rect cancelRect = new Rect(acceptRect.xMax + 15, acceptRect.y, acceptRect.width, acceptRect.height);
            if (Widgets.ButtonText(cancelRect, "AC.Cancel".Translate().CapitalizeFirst()))
            {
                Close();
            }

            Text.Anchor = TextAnchor.UpperLeft;
        }

        private Gene ApplyGene(GeneDef geneDef)
        {
            var gene = curSleeve.genes.GetGene(geneDef);
            if (gene is null)
            {
                gene = curSleeve.genes.AddGene(geneDef, false);
            }
            ApplyGene(gene);
            return gene;
        }

        private void ApplyGene(Gene gene)
        {
            OverrideAllConflicting(gene);
            if (ModLister.BiotechInstalled && gene.def.graphicData != null && gene.def.graphicData.skinIsHairColor)
            {
                curSleeve.story.skinColorOverride = curSleeve.story.HairColor;
            }
            if (gene.def.hairColorOverride.HasValue)
            {
                Color value = gene.def.hairColorOverride.Value;
                if (gene.def.randomBrightnessFactor != 0f)
                {
                    value *= 1f + Rand.Range(0f - gene.def.randomBrightnessFactor, gene.def.randomBrightnessFactor);
                }
                curSleeve.story.HairColor = value.ClampToValueRange(GeneTuning.HairColorValueRange);
            }
            if (gene.def.skinColorBase.HasValue)
            {
                if (gene.def.skinColorBase.HasValue)
                {
                    curSleeve.story.SkinColorBase = gene.def.skinColorBase.Value;
                }
            }
            if (ModLister.BiotechInstalled)
            {
                if (gene.def.skinColorOverride.HasValue)
                {
                    if (gene.def.skinColorOverride.HasValue)
                    {
                        Color value2 = gene.def.skinColorOverride.Value;
                        if (gene.def.randomBrightnessFactor != 0f)
                        {
                            value2 *= 1f + Rand.Range(0f - gene.def.randomBrightnessFactor, gene.def.randomBrightnessFactor);
                        }
                        curSleeve.story.skinColorOverride = value2.ClampToValueRange(GeneTuning.SkinColorValueRange);
                    }
                }
                if (gene.def.bodyType.HasValue && !curSleeve.DevelopmentalStage.Juvenile())
                {
                    if (gene.def.bodyType.HasValue)
                    {
                        curSleeve.story.bodyType = gene.def.bodyType.Value.ToBodyType(curSleeve);
                    }
                }
                if (!gene.def.forcedHeadTypes.NullOrEmpty())
                {
                    if (!gene.def.forcedHeadTypes.NullOrEmpty())
                    {
                        curSleeve.story.TryGetRandomHeadFromSet(gene.def.forcedHeadTypes);
                    }
                }
                if ((gene.def.forcedHair != null || gene.def.hairTagFilter != null) 
                    && !PawnStyleItemChooser.WantsToUseStyle(curSleeve, curSleeve.story.hairDef))
                {
                    curSleeve.story.hairDef = PawnStyleItemChooser.RandomHairFor(curSleeve);
                }
                if (gene.def.beardTagFilter != null && curSleeve.style != null 
                    && !PawnStyleItemChooser.WantsToUseStyle(curSleeve, curSleeve.style.beardDef))
                {
                    curSleeve.style.beardDef = PawnStyleItemChooser.RandomBeardFor(curSleeve);
                }
                if (gene.def.graphicData?.fur != null)
                {
                    curSleeve.story.furDef = gene.def.graphicData.fur;
                }


                if (gene.def.soundCall != null)
                {
                    PawnComponentsUtility.AddAndRemoveDynamicComponents(curSleeve);
                }
                curSleeve.needs?.AddOrRemoveNeedsAsAppropriate();
                curSleeve.health.hediffSet.DirtyCache();
                curSleeve.skills?.Notify_GenesChanged();
                curSleeve.Notify_DisabledWorkTypesChanged();
            }
        }

        private void OverrideAllConflicting(Gene gene)
        {
            gene.OverrideBy(null);
            foreach (Gene item in curSleeve.genes.GenesListForReading)
            {
                if (item != gene && item.def.ConflictsWith(gene.def))
                {
                    item.OverrideBy(gene);
                }
            }
        }
        private void DrawGenes(ref Vector2 pos, TaggedString label, Rect pawnBox, 
            Rect healthBox, ref Rect geneBox, List<Gene> genes, ref Rect rect)
        {
            if (genes.Any())
            {
                genes.SortGenes();
                pos.x -= 15;
                ListSeparator(ref pos, healthBox.width, label);
                pos.x += 15;
                pos.y += 5;

                geneBox = new Rect(pos.x - 15, pos.y, pawnBox.width + 50, 60);
                rect = GenUI.DrawElementStack(geneBox, 22f, genes, delegate (Rect r, Gene gene)
                {
                    Color color2 = GUI.color;
                    GUI.color = StackElementBackground;
                    GUI.DrawTexture(r, BaseContent.WhiteTex);
                    GUI.color = color2;

                    if (Mouse.IsOver(r))
                    {
                        Widgets.DrawHighlight(r);
                    }

                    var iconRect = new Rect(r.x, r.y, 22f, 22f);
                    Color iconColor = gene.def.IconColor;
                    if (gene.Overridden)
                    {
                        iconColor.a = 0.75f;
                        GUI.color = ColoredText.SubtleGrayColor;
                    }

                    Widgets.DefIcon(iconRect, gene.def, null, 1f, null, drawPlaceholder: false, iconColor);
                    var labelRect = new Rect(iconRect.xMax, r.y, r.width - 10f - 15f, r.height);
                    if (gene.Overridden)
                    {
                        GUI.color = ColoredText.SubtleGrayColor;
                    }
                    Widgets.Label(labelRect, gene.LabelCap);
                    GUI.color = Color.white;
                    if (Mouse.IsOver(r))
                    {
                        Gene trLocal = gene;
                        var tooltip = gene.LabelCap.Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + gene.def.DescriptionFull;
                        if (gene.Overridden)
                        {
                            tooltip += "\n\n";
                            tooltip = ((gene.overriddenByGene.def != gene.def) ? (tooltip + ("OverriddenByGene".Translate() 
                            + ": " + gene.overriddenByGene.LabelCap).Colorize(ColorLibrary.RedReadable)) 
                            : (tooltip + ("OverriddenByIdenticalGene".Translate() 
                            + ": " + gene.overriddenByGene.LabelCap).Colorize(ColorLibrary.RedReadable)));
                        }
                        TooltipHandler.TipRegion(tip: new TipSignal(() => tooltip, gene.LabelCap.GetHashCode()), rect: r);
                    }
                }, (Gene gene) => Text.CalcSize(gene.LabelCap).x + 10f + 22);
                pos.y += rect.height;
            }
        }

        public string GetQualityLabel(int sleeveQualityIndex)
        {
            return ((QualityCategory)sleeveQualityIndex).GetLabel().CapitalizeFirst();
        }

        public void UpdateGrowCost()
        {
            ticksToGrow = AlteredCarbonMod.settings.baseGrowingTimeDuration;
            ticksToGrow += sleeveQualitiesTimeCost[sleeveQualities[sleeveQualityIndex]];
            ticksToGrow = Mathf.Max(AlteredCarbonMod.settings.baseGrowingTimeDuration, ticksToGrow);
            if (convertXenogenesToEndegones)
            {
                ticksToGrow *= 2;
            }
            growCost = 12 * (ticksToGrow / GenDate.TicksPerDay);
        }

        public void ApplyHediffs()
        {
            if (qualityDiff != null)
            {
                Hediff hediff = curSleeve.health.hediffSet.GetFirstHediffOfDef(qualityDiff);
                if (hediff != null)
                {
                    curSleeve.health.RemoveHediff(hediff);
                }
            }
            qualityDiff = sleeveQualities[sleeveQualityIndex];
            curSleeve.health.AddHediff(qualityDiff, null);
        }

        public static string GetHediffToolTip(IEnumerable<Hediff> diffs, Pawn pawn)
        {
            string str = "";
            foreach (Hediff hediff in diffs)
            {
                str += hediff.GetTooltip(pawn, false) + "\n";
            }
            return str;
        }

        public void RecheckEverything()
        {
            RecheckBodyOptions();
            InitializeIndexes();
            UpdateGrowCost();
        }

        public void UpdateSleeveGraphic()
        {
            curSleeve.Drawer.renderer.graphics.ResolveAllGraphics();
            PortraitsCache.SetDirty(curSleeve);
            PortraitsCache.PortraitsCacheUpdate();
            GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(curSleeve);
        }

        private void RecheckBodyOptions()
        {
            var permittedHeads = GetPermittedHeads(true).Select(x => x.Value).ToList();
            var permittedBodyTypes = GetPermittedBodyTypes(true).Select(x => x.Value).ToList();
            var permittedHairs = GetPermittedHairs();
            var permittedBeards = GetPermittedBeards();
            if (permittedHeads.Contains(curSleeve.story.headType) is false)
            {
                curSleeve.story.headType = permittedHeads.RandomElement();
            }

            if (permittedBodyTypes.Contains(curSleeve.story.bodyType) is false)
            {
                curSleeve.story.bodyType = permittedBodyTypes.RandomElement();
            }
            if (permittedHairs.Contains(curSleeve.story.hairDef) is false)
            {
                curSleeve.story.hairDef = permittedHairs.RandomElement();
            }
            if (permittedBeards.Contains(curSleeve.style.beardDef) is false)
            {
                curSleeve.style.beardDef = permittedBeards.RandomElement();
            }
        }

        public void RemoveAllTraits(Pawn pawn)
        {
            if (pawn.story != null)
            {
                pawn.story.traits = new TraitSet(pawn);
            }
        }

        public void RemoveAllHediffs(Pawn pawn)
        {
            pawn.health = new Pawn_HealthTracker(pawn);
        }

        public string ExtractHeadLabels(string headLabel)
        {
            headLabel = Regex.Replace(headLabel, @"^[A-Z]+_", @"");
            headLabel = Regex.Replace(headLabel, @"^[A-Z]+([A-Z])", @"$1");
            headLabel = headLabel.Replace("_", " ");
            headLabel = headLabel.SplitCamelCase();
            headLabel = Regex.Replace(headLabel, @" +", " ");
            headLabel = headLabel.FirstCharToUpper();
            return headLabel;
        }

        private List<ThingDef> GetPermittedRaces()
        {
            List<ThingDef> excludedRaces = new List<ThingDef>();
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs.Where(def => def.category == ThingCategory.Pawn))
            {
                if (def.GetModExtension<ExcludeRacesModExtension>() is ExcludeRacesModExtension props)
                {
                    if (!props.canBeGrown)
                    {
                        excludedRaces.Add(def);
                    }
                }
            }
            return ModCompatibility.GetGrowableRaces(excludedRaces).OrderBy(entry => entry.LabelCap.RawText).ToList();
        }


        private static List<BodyTypeDef> invalidBodies = new List<BodyTypeDef>
        {
            BodyTypeDefOf.Baby, BodyTypeDefOf.Child
        };
        public List<KeyValuePair<GeneDef, BodyTypeDef>> GetPermittedBodyTypes(bool geneActiveCheck = false)
        {
            var keyValuePairs = new List<KeyValuePair<GeneDef, BodyTypeDef>>();
            foreach (var gene in curSleeve.genes.GenesListForReading)
            {
                if ((!geneActiveCheck || gene.Active) && gene.def.bodyType != null)
                {
                    keyValuePairs.Add(new KeyValuePair<GeneDef, BodyTypeDef>(gene.def, gene.def.bodyType.Value.ToBodyType(curSleeve)));
                }
            }
            if (keyValuePairs.Any())
            {
                return keyValuePairs;
            }

            var list = (ModCompatibility.AlienRacesIsActive ?
                ModCompatibility.GetAllowedBodyTypes(curSleeve.def) :
                DefDatabase<BodyTypeDef>.AllDefsListForReading).Except(invalidBodies).ToList();
            if (curSleeve.gender == Gender.Male)
            {
                list = list.Where(x => x != BodyTypeDefOf.Female).ToList();
            }
            else if (curSleeve.gender == Gender.Female)
            {
                list = list.Where(x => x != BodyTypeDefOf.Male).ToList();
            }
            foreach (var entry in list)
            {
                keyValuePairs.Add(new KeyValuePair<GeneDef, BodyTypeDef>(null, entry));
            }
            return keyValuePairs;
        }

        private List<BeardDef> GetPermittedBeards()
        {
            return DefDatabase<BeardDef>.AllDefs.Where(x => PawnStyleItemChooser.WantsToUseStyle(curSleeve, x)).ToList();
        }

        private List<HairDef> GetPermittedHairs()
        {
            return (ModCompatibility.AlienRacesIsActive
                ? ModCompatibility.GetPermittedHair(currentPawnKindDef.race)
                : DefDatabase<HairDef>.AllDefs.ToList()).Where(x => PawnStyleItemChooser.WantsToUseStyle(curSleeve, x)).ToList();
        }

        private static List<HeadTypeDef> invalidHeads = new List<HeadTypeDef>
        {
            HeadTypeDefOf.Skull, HeadTypeDefOf.Stump
        };
        public List<KeyValuePair<GeneDef, HeadTypeDef>> GetPermittedHeads(bool geneActiveCheck = false)
        {
            var keyValuePairs = new List<KeyValuePair<GeneDef, HeadTypeDef>>();
            foreach (var gene in curSleeve.genes.GenesListForReading)
            {
                if ((!geneActiveCheck || gene.Active) && gene.def.forcedHeadTypes.NullOrEmpty() is false)
                {
                    foreach (var head in gene.def.forcedHeadTypes)
                    {
                        keyValuePairs.Add(new KeyValuePair<GeneDef, HeadTypeDef>(gene.def, head));
                    }
                }
            }

            if (keyValuePairs.Any())
            {
                return keyValuePairs;
            }
            var list = DefDatabase<HeadTypeDef>.AllDefs.Except(invalidHeads).Where(x => CanUseHeadType(x)).ToList();
            foreach (var entry in list)
            {
                keyValuePairs.Add(new KeyValuePair<GeneDef, HeadTypeDef>(null, entry));
            }
            return keyValuePairs;
            bool CanUseHeadType(HeadTypeDef head)
            {
                if (!head.requiredGenes.NullOrEmpty())
                {
                    if (curSleeve.genes == null)
                    {
                        return false;
                    }
                    foreach (GeneDef requiredGene in head.requiredGenes)
                    {
                        if (!curSleeve.genes.HasGene(requiredGene))
                        {
                            return false;
                        }
                    }
                }
                return head.gender == 0 || head.gender == curSleeve.gender;
            }
        }

        public List<KeyValuePair<GeneDef, Color>> GetPermittedSkinColors(bool geneActiveCheck = false)
        {
            var skinColors = new Dictionary<GeneDef, Color>();
            foreach (var geneDef in DefDatabase<GeneDef>.AllDefsListForReading)
            {
                if (geneDef.skinColorBase != null && geneDef.endogeneCategory == EndogeneCategory.Melanin)
                {
                    skinColors[geneDef] = geneDef.skinColorBase.Value;
                }
            }
            foreach (var gene in curSleeve.genes.GenesListForReading)
            {
                if ((!geneActiveCheck || gene.Active))
                {
                    if (gene.def.skinColorBase != null && gene.def.endogeneCategory == EndogeneCategory.Melanin)
                    {
                        skinColors[gene.def] = gene.def.skinColorBase.Value;
                    }
                    else if (gene.def.skinColorOverride != null)
                    {
                        skinColors[gene.def] = gene.def.skinColorOverride.Value;
                    }
                }
            }
            return skinColors.ToList();
        }
        public List<KeyValuePair<GeneDef, Color>> GetPermittedHairColors(bool geneActiveCheck = false)
        {
            var hairColors = new Dictionary<GeneDef, Color>();
            foreach (var geneDef in DefDatabase<GeneDef>.AllDefsListForReading)
            {
                if (geneDef.hairColorOverride != null)
                {
                    hairColors[geneDef] = geneDef.hairColorOverride.Value;
                }
            }
            foreach (var gene in curSleeve.genes.GenesListForReading)
            {
                if ((!geneActiveCheck || gene.Active) && gene.def.hairColorOverride != null && gene.def.endogeneCategory == EndogeneCategory.HairColor)
                {
                    hairColors[gene.def] = gene.def.hairColorOverride.Value;
                }
            }
            return hairColors.ToList();
        }

        private void CopyBodyFrom(Pawn source)
        {
            curSleeve.gender = source.gender;
            curSleeve.kindDef = source.kindDef;

            var genes = curSleeve.genes.GenesListForReading;
            foreach (var oldGene in genes)
            {
                curSleeve.genes.RemoveGene(oldGene);
            }

            var sourceGenes = source.genes.Endogenes;
            foreach (var sourceGene in sourceGenes)
            {
                curSleeve.genes.AddGene(sourceGene.def, false);
            }
            for (var i = 0; i < sourceGenes.Count; i++)
            {
                var gene = curSleeve.genes.Endogenes[i];
                if (sourceGenes[i].Active)
                {
                    ApplyGene(gene);
                }
            }

            curSleeve.story.bodyType = source.story.bodyType;
            curSleeve.story.hairDef = source.story.hairDef;
            curSleeve.style.beardDef = source.style.beardDef;
            if (ModCompatibility.AlienRacesIsActive)
            {
                ModCompatibility.CopyBodyAddons(source, curSleeve);
            }
            else
            {
                curSleeve.story.headType = source.story.headType;
            }
            RecheckEverything();
        }

        public void LoadSleeve(SleevePreset preset)
        {
            curSleeve = preset.sleeve;
            currentPawnKindDef = curSleeve.kindDef;
            InitializeIndexes();
        }

        private void InitializeIndexes()
        {
            hairTypeIndex = GetPermittedHairs().IndexOf(curSleeve.story.hairDef);
            beardTypeIndex = GetPermittedBeards().IndexOf(curSleeve.style.beardDef);
            headTypeIndex = GetPermittedHeads().Select(x => x.Value).ToList().IndexOf(curSleeve.story.headType);
            if (curXenogerm != null)
            {
                indexesPerCategory = new Dictionary<string, int>();
                var genes = curXenogerm.GeneSet.genes.Where(x => x.exclusionTags.NullOrEmpty() is false);
                foreach (var gene in genes)
                {
                    foreach (var tag in gene.exclusionTags)
                    {
                        var genesOfThisTag = curSleeve.genes.GenesListForReading.Where(x => x.def.exclusionTags.NullOrEmpty() is false
                            && x.def.exclusionTags.Contains(tag)).ToList();
                        var activeGene = genesOfThisTag.FirstOrDefault(x => x.Active);
                        indexesPerCategory[tag] = genesOfThisTag.IndexOf(activeGene);
                    }
                }
            }

            if (ModCompatibility.AlienRacesIsActive)
            {
                raceTypeIndex = GetPermittedRaces().IndexOf(curSleeve.def);
            }

            if (curSleeve.gender == Gender.Male)
            {
                maleBodyTypeIndex = GetPermittedBodyTypes().Select(x => x.Value).ToList().IndexOf(curSleeve.story.bodyType);
            }
            else if (curSleeve.gender == Gender.Female)
            {
                femaleBodyTypeIndex = GetPermittedBodyTypes().Select(x => x.Value).ToList().IndexOf(curSleeve.story.bodyType);
            }

            foreach (HediffDef hediff in sleeveQualities)
            {
                if (curSleeve.health.hediffSet.HasHediff(hediff))
                {
                    sleeveQualityIndex = sleeveQualities.IndexOf(hediff);
                }
            }
        }
        private void CreateSleeve(Gender gender)
        {
            curXenogerm = null;
            if (ModCompatibility.AlienRacesIsActive)
            {
                ModCompatibility.UpdateGenderRestrictions(currentPawnKindDef.race, out allowMales, out allowFemales);
                if (gender == Gender.Male && !allowMales)
                {
                    gender = Gender.Female;
                }
                if (gender == Gender.Female && !allowFemales)
                {
                    gender = Gender.Male;
                }
            }
            curSleeve = PawnGenerator.GeneratePawn(new PawnGenerationRequest(currentPawnKindDef, null, PawnGenerationContext.NonPlayer,
                -1, true, false, false, false, false, 0f, false, true, true, false, false, false, true,
                fixedGender: gender));

            var lastAdultAge = curSleeve.RaceProps.lifeStageAges.LastOrDefault((LifeStageAge lifeStageAge) => lifeStageAge.def.developmentalStage.Adult())?.minAge ?? 0f;
            curSleeve.ageTracker.AgeBiologicalTicks = (long)Mathf.FloorToInt(lastAdultAge * 3600000f);
            curSleeve.ageTracker.AgeChronologicalTicks = (long)Mathf.FloorToInt(lastAdultAge * 3600000f);
            curSleeve.story.Adulthood = null;
            curSleeve.guest.recruitable = true;
            curSleeve.relations = new Pawn_RelationsTracker(curSleeve);
            curSleeve.Name = new NameSingle("AC.EmptySleeve".Translate());
            curSleeve.story.title = "";
            curSleeve?.equipment.DestroyAllEquipment();
            curSleeve?.inventory.DestroyAll();
            curSleeve.apparel.DestroyAll();
            RemoveAllTraits(curSleeve);
            convertedGenes = new List<Gene>();
            if (curSleeve.playerSettings == null)
            {
                curSleeve.playerSettings = new Pawn_PlayerSettings(curSleeve);
            }

            curSleeve.playerSettings.medCare = MedicalCareCategory.Best;
            curSleeve.skills = new Pawn_SkillTracker(curSleeve);
            curSleeve.needs = new Pawn_NeedsTracker(curSleeve);
            curSleeve.workSettings = new Pawn_WorkSettings(curSleeve);
            curSleeve.needs.mood.thoughts = new ThoughtHandler(curSleeve);
            curSleeve.timetable = new Pawn_TimetableTracker(curSleeve);

            if (curSleeve.needs?.mood?.thoughts?.memories?.Memories != null)
            {
                for (int num = curSleeve.needs.mood.thoughts.memories.Memories.Count - 1; num >= 0; num--)
                {
                    curSleeve.needs.mood.thoughts.memories.RemoveMemory(curSleeve.needs.mood.thoughts.memories.Memories[num]);
                }
            }
            RemoveAllHediffs(curSleeve);

            if (curSleeve.workSettings != null)
            {
                curSleeve.workSettings.EnableAndInitialize();
            }

            if (curSleeve.skills != null)
            {
                curSleeve.skills.Notify_SkillDisablesChanged();
            }
            if (!curSleeve.Dead && curSleeve.RaceProps.Humanlike)
            {
                curSleeve.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
            }

            curSleeve.story.Childhood = AC_DefOf.VFEU_VatGrownChild;
            curSleeve.story.Childhood = AC_DefOf.VFEU_VatGrownAdult;
            curSleeve.ideo = new Pawn_IdeoTracker(curSleeve);
            curSleeve.story.favoriteColor = null;
            if (ModsConfig.IdeologyActive)
            {
                curSleeve.style.BodyTattoo = TattooDefOf.NoTattoo_Body;
                curSleeve.style.FaceTattoo = TattooDefOf.NoTattoo_Face;
            }
            curSleeve.Rotation = Rot4.South;

            ApplyHediffs();
            RecheckEverything();
        }
    }
}
