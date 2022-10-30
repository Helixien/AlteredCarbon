using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;
using static AlteredCarbon.UIHelpers;

namespace AlteredCarbon
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class HotSwappableAttribute : Attribute
    {
    }

    [HotSwappable]
    public class Window_SleeveCustomization : Window
    {
        private List<ThingDef> orderedValidAlienRaces;
        private List<HairDef> permittedHairs;
        private List<HeadTypeDef> permittedHeads;
        private List<BeardDef> permittedBeards;
        //Variables
        public Pawn curSleeve;
        private PawnKindDef currentPawnKindDef;
        private readonly Building_SleeveGrower sleeveGrower;

        private bool allowMales = true;
        private bool allowFemales = true;
        private readonly List<Color> humanSkinColors = new List<Color>(new Color[] { rgbConvert(242, 237, 224),
            rgbConvert(255, 239, 213), rgbConvert(255, 239, 189), rgbConvert(228, 158, 90), rgbConvert(130, 91, 48) });
        private static readonly List<Color> humanHairColors = new List<Color>(new Color[] { rgbConvert(51, 51, 51),
            rgbConvert(79, 71, 66), rgbConvert(64, 51, 38), rgbConvert(77, 51, 26), rgbConvert(90, 58, 32), rgbConvert(132, 83, 47),
            rgbConvert(193, 146, 85), rgbConvert(237, 202, 156) });

        private readonly float leftOffset = 20;
        private readonly float pawnSpacingFromEdge = 5;

        // indexes for lists
        private int hairTypeIndex = 0;
        private int beardTypeIndex = 0;
        private int raceTypeIndex = 0;
        private int headTypeIndex = 0;
        private int maleBodyTypeIndex = 0;
        private int femaleBodyTypeIndex = 0;
        private int sleeveQualityIndex = 2;
        private int sleeveBeautyIndex = 2;
        private int sleeveSensitivityIndex = 2;

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
        public static Dictionary<int, int> sleeveBeautiesTimeCost = new Dictionary<int, int>
        {
            {-2, 0 },
            {-1, GenDate.TicksPerDay * 1 },
            {0, GenDate.TicksPerDay * 2 },
            {1, GenDate.TicksPerDay * 4 },
            {2, GenDate.TicksPerDay * 5 },
        };

        public static Dictionary<int, int> sensitivityQualitiesTimeCost = new Dictionary<int, int>
        {
            {-2, GenDate.TicksPerDay * 5 },
            {-1, GenDate.TicksPerDay * 2 },
            {0, 0 },
            {1, GenDate.TicksPerDay * 2 },
            {2, GenDate.TicksPerDay * 5 },
        };
        private static readonly List<int> beautyDegrees = new List<int>
        {
            -2,
            -1,
            0,
            1,
            2
        };

        private static readonly List<int> sensitivityDegrees = new List<int>
        {
            -2,
            -1,
            0,
            1,
            2
        };
        //Static Values
        [TweakValue("0AC", 0, 200)] public static float AlienRacesYOffset = 32;
        [TweakValue("0AC", 500, 1000)] public static float InitialWindowXSize = 728f;
        [TweakValue("0AC", 500, 1000)] public static float InitialWindowYSize = 690f;

        public override Vector2 InitialSize
        {
            get
            {
                float xSize = InitialWindowXSize;
                float ySize = InitialWindowYSize;
                if (ModCompatibility.AlienRacesIsActive)
                {
                    ySize += AlienRacesYOffset;
                }
                return new Vector2(xSize, ySize);
            }
        }
        public List<ThingDef> InitializeExclusionsCache(string field)
        {
            List<ThingDef> excludedRaces = new List<ThingDef>();
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs.Where(def => def.category == ThingCategory.Pawn))
            {
                if (def.GetModExtension<ExcludeRacesModExtension>() is ExcludeRacesModExtension props)
                {
                    if (!(bool)typeof(ExcludeRacesModExtension).GetField(field).GetValue(props))
                    {
                        excludedRaces.Add(def);
                    }
                }
            }
            return excludedRaces;
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
            orderedValidAlienRaces = ModCompatibility.GetGrowableRaces(excludedRaces).OrderBy(entry => entry.LabelCap.RawText).ToList();
            return orderedValidAlienRaces;
        }

        private void InitUI()
        {
            forcePause = true;
            absorbInputAroundWindow = false;
        }

        public Window_SleeveCustomization(Building_SleeveGrower sleeveGrower)
        {
            this.sleeveGrower = sleeveGrower;
            Init(PawnKindDefOf.Colonist);
            InitUI();
        }

        public Window_SleeveCustomization(Building_SleeveGrower sleeveGrower, Pawn pawn)
        {
            this.sleeveGrower = sleeveGrower;
            Init(pawn.kindDef, pawn.gender);
            CopyBodyFrom(pawn);
            InitUI();
        }

        public string GetQualityLabel(int sleeveQualityIndex)
        {
            return ((QualityCategory)sleeveQualityIndex).GetLabel().CapitalizeFirst();
        }
        public string GetBeautyLabel(int index)
        {
            if (index != 0)
            {
                Trait beauty = new Trait(TraitDefOf.Beauty, index);
                return beauty.LabelCap;
            }
            else
            {
                return QualityCategory.Normal.GetLabel().CapitalizeFirst();
            }
        }

        public string GetSensitivityLabel(int index)
        {
            if (index != 0)
            {
                Trait sensitivity = new Trait(TraitDefOf.PsychicSensitivity, index);
                return sensitivity.LabelCap;
            }
            else
            {
                return QualityCategory.Normal.GetLabel().CapitalizeFirst();
            }
        }

        public void UpdateGrowCost()
        {
            ticksToGrow = AlteredCarbonMod.settings.baseGrowingTimeDuration;
            ticksToGrow += sleeveQualitiesTimeCost[sleeveQualities[sleeveQualityIndex]]
                + sleeveBeautiesTimeCost[beautyDegrees[sleeveBeautyIndex]]
                + sensitivityQualitiesTimeCost[sensitivityDegrees[sleeveSensitivityIndex]];
            ticksToGrow = Mathf.Max(AlteredCarbonMod.settings.baseGrowingTimeDuration, ticksToGrow);
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

        public void ApplyBeauty()
        {
            Trait trait = curSleeve.story.traits.GetTrait(TraitDefOf.Beauty);
            if (trait != null)
            {
                curSleeve.story.traits.RemoveTrait(trait);
            }
            if (beautyDegrees[sleeveBeautyIndex] != 0)
            {
                curSleeve.story.traits.GainTrait(new Trait(TraitDefOf.Beauty, beautyDegrees[sleeveBeautyIndex]));
            }
        }
        public void ApplySensitivity()
        {
            Trait trait = curSleeve.story.traits.GetTrait(TraitDefOf.PsychicSensitivity);
            if (trait != null)
            {
                curSleeve.story.traits.RemoveTrait(trait);
            }
            if (sensitivityDegrees[sleeveSensitivityIndex] != 0)
            {
                curSleeve.story.traits.GainTrait(new Trait(TraitDefOf.PsychicSensitivity, sensitivityDegrees[sleeveSensitivityIndex]));
            }
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

        public void UpdateSleeveGraphic()
        {
            curSleeve.Drawer.renderer.graphics.ResolveAllGraphics();
            PortraitsCache.SetDirty(curSleeve);
            PortraitsCache.PortraitsCacheUpdate();
            GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty(curSleeve);
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

        public string ExtractHeadLabels(string path)
        {
            string str = Regex.Replace(path, ".*/[A-Z]+?_", "", RegexOptions.IgnoreCase).Replace("_", " ");
            return str;
        }

        public List<BodyTypeDef> GetAllowedBodyTypesFor(Pawn pawn)
        {
            return ModCompatibility.AlienRacesIsActive ? ModCompatibility.GetAllowedBodyTypes(pawn.def) : DefDatabase<BodyTypeDef>.AllDefsListForReading;
        }

        public List<Color> GetSkinColors()
        {
            List<Color> skinColors = new List<Color>();
            if (ModCompatibility.AlienRacesIsActive)
            {
                skinColors = ModCompatibility.GetRacialColorPresets(curSleeve.kindDef.race, "skin");
            }
            if (skinColors.NullOrEmpty())
            {
                skinColors = humanSkinColors;
            }
            return skinColors;
        }
        public List<Color> GetHairColors()
        {
            List<Color> hairColors = null;
            if (ModCompatibility.AlienRacesIsActive)
            {
                hairColors = ModCompatibility.GetRacialColorPresets(curSleeve.kindDef.race, "hair");
            }
            if (hairColors.NullOrEmpty())
            {
                hairColors = humanHairColors;
            }
            return hairColors;
        }

        public Color curSkinColor;
        public Color curHairColor;
        public override void DoWindowContents(Rect inRect)
        {
            Vector2 pos = new Vector2(leftOffset, 0);
            //Draw Pawn stuff.
            if (curSleeve != null)
            {
                Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(new Rect(0f, 0f, inRect.width, 32f), "AC.SleeveCustomization".Translate());

                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;

                pos.y += 50;
                float initialYPos = pos.y;
                //Gender
                Text.Anchor = TextAnchor.MiddleLeft;
                Rect genderRect = GetLabelRect(ref pos);
                Widgets.Label(genderRect, "Gender".Translate() + ":");
                Rect maleGenderRect = new Rect(genderRect.xMax + buttonOffsetFromText, genderRect.y, buttonWidth, buttonHeight);
                Rect femaleGenderRect = new Rect(maleGenderRect.xMax + buttonOffsetFromButton, genderRect.y, buttonWidth, buttonHeight);
                if (allowMales && Widgets.ButtonText(maleGenderRect, "Male".Translate().CapitalizeFirst()))
                {
                    CreateSleeve(Gender.Male);
                }
                if (allowFemales && Widgets.ButtonText(femaleGenderRect, "Female".Translate().CapitalizeFirst()))
                {
                    CreateSleeve(Gender.Female);
                }

                if (ModCompatibility.AlienRacesIsActive)
                {
                    DoSelectionButtons(ref pos, "AC.SelectRace".Translate(), ref raceTypeIndex,
                        (ThingDef x) => x.LabelCap, orderedValidAlienRaces, delegate (ThingDef x)
                        {
                            currentPawnKindDef.race = x;
                            CreateSleeve(curSleeve.gender);
                            UpdateSleeveGraphic();
                            raceTypeIndex = orderedValidAlienRaces.IndexOf(x);
                        });
                }

                //Skin Colour
                List<Color> colors = GetSkinColors();
                DoColorButtons(ref pos, "AC.SkinColour".Translate(), curSkinColor, colors, delegate (Color x)
                {
                    if (ModCompatibility.AlienRacesIsActive)
                    {
                        ModCompatibility.SetSkinColorFirst(curSleeve, x);
                    }
                    else
                    {
                        curSleeve.story.SkinColorBase = x;
                    }
                    curSkinColor = x;
                    UpdateSleeveGraphic();
                }, ModCompatibility.AlienRacesIsActive);

                DoSelectionButtons(ref pos, "AC.HeadShape".Translate(), ref headTypeIndex,
                    (HeadTypeDef x) => ExtractHeadLabels(x.graphicPath),
                    permittedHeads, delegate (HeadTypeDef x)
                    {
                        curSleeve.story.headType = x;
                        UpdateSleeveGraphic();
                        headTypeIndex = permittedHeads.IndexOf(x);
                    });

                if (curSleeve.gender == Gender.Male)
                {
                    List<BodyTypeDef> list = GetAllowedBodyTypesFor(curSleeve).Where(x => x != BodyTypeDefOf.Female).ToList();
                    DoSelectionButtons(ref pos, "AC.BodyShape".Translate(), ref maleBodyTypeIndex,
                        (BodyTypeDef x) => x.defName, list, delegate (BodyTypeDef x)
                        {
                            curSleeve.story.bodyType = x;
                            UpdateSleeveGraphic();
                            maleBodyTypeIndex = list.IndexOf(x);
                        });
                }
                else if (curSleeve.gender == Gender.Female)
                {
                    List<BodyTypeDef> list = GetAllowedBodyTypesFor(curSleeve).Where(x => x != BodyTypeDefOf.Male).ToList();
                    DoSelectionButtons(ref pos, "AC.BodyShape".Translate(), ref femaleBodyTypeIndex,
                        (BodyTypeDef x) => x.defName, list, delegate (BodyTypeDef x)
                        {
                            curSleeve.story.bodyType = x;
                            UpdateSleeveGraphic();
                            femaleBodyTypeIndex = list.IndexOf(x);
                        });
                }
                if (!permittedHairs.NullOrEmpty())
                {
                    //Hair Colour
                    colors = GetHairColors();
                    DoColorButtons(ref pos, "AC.HairColour".Translate(), curHairColor, colors, delegate (Color x)
                    {
                        curHairColor = x;
                        if (ModCompatibility.AlienRacesIsActive)
                        {
                            ModCompatibility.SetHairColorFirst(curSleeve, x);
                        }
                        else
                        {
                            curSleeve.story.HairColor = x;
                        }
                        UpdateSleeveGraphic();
                    }, true);

                    DoSelectionButtons(ref pos, "AC.HairType".Translate(), ref hairTypeIndex,
                        (HairDef x) => x.LabelCap, permittedHairs, delegate (HairDef x)
                        {
                            curSleeve.story.hairDef = x;
                            UpdateSleeveGraphic();
                            hairTypeIndex = permittedHairs.IndexOf(x);
                        });
                }

                DoSelectionButtons(ref pos, "AC.BeardType".Translate(), ref beardTypeIndex,
                    (BeardDef x) => x.LabelCap, permittedBeards, delegate (BeardDef x)
                    {
                        curSleeve.style.beardDef = x;
                        UpdateSleeveGraphic();
                        beardTypeIndex = permittedBeards.IndexOf(x);
                    });

                DoSelectionButtons(ref pos, "AC.SleeveQuality".Translate(), ref sleeveQualityIndex,
                    (HediffDef x) => GetQualityLabel(sleeveQualities.IndexOf(x)), sleeveQualities, delegate (HediffDef x)
                    {
                        sleeveQualityIndex = sleeveQualities.IndexOf(x);
                        ApplyHediffs();
                        UpdateGrowCost();
                    });

                DoSelectionButtons(ref pos, "AC.SleeveBeauty".Translate(), ref sleeveBeautyIndex,
                (int x) => GetBeautyLabel(x), beautyDegrees, delegate (int x)
                {
                    sleeveBeautyIndex = beautyDegrees.IndexOf(x);
                    ApplyBeauty();
                    UpdateGrowCost();
                });

                DoSelectionButtons(ref pos, "AC.PsychicSensitivity".Translate(), ref sleeveSensitivityIndex,
                (int x) => GetSensitivityLabel(x), sensitivityDegrees, delegate (int x)
                {
                    sleeveSensitivityIndex = sensitivityDegrees.IndexOf(x);
                    ApplySensitivity();
                    UpdateGrowCost();
                });

                pos.y += 50;
                Rect timeToGrowRect = GetLabelRect(ref pos, 300, 32);
                Text.Font = GameFont.Medium;
                Widgets.Label(timeToGrowRect, "AC.TimeToGrow".Translate(GenDate.ToStringTicksToDays(ticksToGrow)));
                Text.Font = GameFont.Small;
                Rect growCostRect = GetLabelRect(ref pos, inRect.width, 32);
                Widgets.Label(growCostRect, "  " + "AC.GrowCost".Translate(growCost));
                Widgets.DrawHighlight(new Rect(growCostRect.x, growCostRect.y, inRect.width - 50, growCostRect.height));
                Rect explanationLabelRect = GetLabelRect(ref pos, inRect.width - 50, 50);
                Text.Font = GameFont.Tiny;
                GUI.color = Color.grey;
                Widgets.Label(explanationLabelRect, "AC.SleeveCustomizationExplanation".Translate());
                GUI.color = Color.white;
                Text.Font = GameFont.Small;

                pos.x = 475;
                pos.y = initialYPos;
                //Pawn Box
                Rect pawnBox = new Rect(pos.x, pos.y, 200, 200);
                Widgets.DrawMenuSection(pawnBox);
                Widgets.DrawShadowAround(pawnBox);
                Rect pawnBoxPawn = new Rect(pawnBox.x + pawnSpacingFromEdge, pawnBox.y + pawnSpacingFromEdge, pawnBox.width - (pawnSpacingFromEdge * 2), pawnBox.height - (pawnSpacingFromEdge * 2));
                GUI.DrawTexture(pawnBoxPawn, PortraitsCache.Get(curSleeve, pawnBoxPawn.size, Rot4.South, default, 1f));
                Widgets.InfoCardButton(pawnBox.x + pawnBox.width - Widgets.InfoCardButtonSize - 10f, pawnBox.y + pawnBox.height - Widgets.InfoCardButtonSize - 10f, curSleeve);

                pos.y += 215;
                //Hediff printout
                IEnumerable<IGrouping<BodyPartRecord, Hediff>> heDiffListing;
                MethodInfo heDiffLister = typeof(HealthCardUtility).GetMethod("VisibleHediffGroupsInOrder", BindingFlags.NonPublic | BindingFlags.Static);
                heDiffListing = (IEnumerable<IGrouping<BodyPartRecord, Hediff>>)heDiffLister.Invoke(null, new object[] { curSleeve, false });
                List<Hediff> diffs = heDiffListing.SelectMany(group => group).ToList();
                Rect healthBoxLabel = GetLabelRect(ref pos, 150, 32);
                Text.Font = GameFont.Medium;
                Widgets.Label(healthBoxLabel, "AC.SleeveHealthPreview".Translate().CapitalizeFirst());
                Rect healthBox = new Rect(pawnBox.x - 15, healthBoxLabel.yMax, pawnBox.width + 30, 50f);
                Widgets.DrawHighlight(healthBox);
                GUI.color = HealthUtility.GoodConditionColor;
                Listing_Standard diffListing = new Listing_Standard();
                Rect heDiffPrintout = healthBox.BottomPart(0.95f).LeftPart(0.95f).RightPart(0.95f);
                diffListing.Begin(heDiffPrintout);
                Text.Anchor = TextAnchor.MiddleLeft;
                for (int ii = 0; ii < diffs.Count; ++ii)
                {
                    diffListing.Label(diffs[ii].LabelCap);
                }
                diffListing.End();
                healthBox.height = Mathf.Max(50f, diffs.Count * 25f);

                GUI.color = Color.white;
                if (Mouse.IsOver(healthBox))
                {
                    Widgets.DrawHighlight(healthBox);
                    TooltipHandler.TipRegion(healthBox, new TipSignal(() => GetHediffToolTip(diffs, curSleeve), 1147682));
                }

                Text.Font = GameFont.Medium;
                float currentY2 = healthBox.yMax + 15;
                Widgets.Label(new Rect(pos.x, currentY2, 200f, 30f), "Traits".Translate());
                currentY2 += 30f;
                Text.Font = GameFont.Small;
                List<Trait> traits = curSleeve.story.traits?.allTraits;
                if (traits == null || traits.Count == 0)
                {
                    Color color = GUI.color;
                    GUI.color = Color.gray;
                    Rect rect13 = new Rect(pos.x - 15, currentY2, pawnBox.width + 30, 24f);
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
                    GenUI.DrawElementStack(new Rect(pos.x - 15, currentY2, pawnBox.width + 30, 60), 22f, curSleeve.story.traits.allTraits, delegate (Rect r, Trait trait)
                    {
                        Color color2 = GUI.color;
                        GUI.color = StackElementBackground;
                        GUI.DrawTexture(r, BaseContent.WhiteTex);
                        GUI.color = color2;
                        if (Mouse.IsOver(r))
                        {
                            Widgets.DrawHighlight(r);
                        }
                        Widgets.Label(new Rect(r.x + 5f, r.y, r.width - 10f, r.height), trait.LabelCap);
                        if (Mouse.IsOver(r))
                        {
                            Trait trLocal = trait;
                            TooltipHandler.TipRegion(tip: new TipSignal(() => trLocal.TipString(curSleeve), (int)currentY2 * 37), rect: r);
                        }
                    }, (Trait trait) => Text.CalcSize(trait.LabelCap).x + 10f);
                }

                if (ModCompatibility.RimJobWorldIsActive && ModCompatibility.HelixienAlteredCarbonIsActive)
                {
                    Rect setBodyParts = new Rect(healthBox.x, currentY2 + 60f, healthBox.width, buttonHeight);
                    if (ButtonTextSubtleCentered(setBodyParts, "AC.SetBodyParts".Translate().CapitalizeFirst()))
                    {
                        Find.WindowStack.Add(new Window_BodyPartPicker(curSleeve, this));
                    }
                }


                Rect saveTemplateRect = new Rect(inRect.x, inRect.y + inRect.height - 32, 150, 32);
                if (Widgets.ButtonText(saveTemplateRect, "AC.SaveTemplate".Translate()))
                {
                    Find.WindowStack.Add(new Dialog_PresetList_Save(this));
                }
                Rect loadTemplateRect = new Rect(saveTemplateRect.xMax + 15, saveTemplateRect.y, saveTemplateRect.width, saveTemplateRect.height);
                if (Widgets.ButtonText(loadTemplateRect, "AC.LoadTemplate".Translate()))
                {
                    Find.WindowStack.Add(new Dialog_PresetList_Load(this));
                }
                Rect acceptRect = new Rect(loadTemplateRect.xMax + 60, loadTemplateRect.y, loadTemplateRect.width, loadTemplateRect.height);
                if (Widgets.ButtonText(acceptRect, "Accept".Translate().CapitalizeFirst()))
                {
                    sleeveGrower.StartGrowth(curSleeve, ticksToGrow, growCost);
                    Close();
                }

                Rect cancelRect = new Rect(acceptRect.xMax + 15, acceptRect.y, acceptRect.width, acceptRect.height);
                if (Widgets.ButtonText(cancelRect, "AC.Cancel".Translate().CapitalizeFirst()))
                {
                    Close();
                }
            }

            Text.Anchor = TextAnchor.UpperLeft;
        }
        private void DoColorButtons(ref Vector2 pos, string label, Color curColor, List<Color> colors, Action<Color> selectAction,
            bool includeColorPicker)
        {
            Text.Anchor = TextAnchor.MiddleLeft;
            Rect labelRect = GetLabelRect(ref pos);
            Widgets.Label(labelRect, label + ":");

            if (includeColorPicker)
            {
                Rect colorPickerButton = new Rect(labelRect.xMax + buttonOffsetFromText + (5 * 50) + 15, labelRect.y, 32, 32);
                if (Widgets.ButtonImage(colorPickerButton, BaseContent.BadTex))
                {
                    Find.WindowStack.Add(new Window_ColorPicker(curColor, delegate (Color pick)
                    {
                        selectAction(pick);
                    }));
                }
            }

            float j = 0;
            for (int i = 0; i < colors.Count; i++)
            {
                Rect rect = new Rect(labelRect.xMax + buttonOffsetFromText + (j * 50), pos.y - buttonHeight - 5, 50, buttonHeight - 2);
                GUI.DrawTexture(rect, BaseContent.GreyTex);
                if (Widgets.ButtonInvisible(rect))
                {
                    selectAction(colors[i]);
                }
                Widgets.DrawBoxSolid(rect.ExpandedBy(-2), colors[i]);
                j++;
                if (i > 0 && i % 4 == 0)
                {
                    if (i != colors.Count - 1)
                    {
                        pos.y += buttonHeight - 2;
                        j = 0;
                    }
                }
            }
        }

        public void DoSelectionButtons<T>(ref Vector2 pos, string label, ref int index, Func<T, string> labelGetter, List<T> list, Action<T> selectAction)
        {
            if (list.Any())
            {
                Text.Anchor = TextAnchor.MiddleLeft;
                Rect labelRect = GetLabelRect(ref pos);
                Widgets.Label(labelRect, label + ":");
                Rect highlightRect = new Rect(labelRect.xMax + buttonOffsetFromText, labelRect.y, (buttonWidth * 2) + buttonOffsetFromButton, buttonHeight);
                Widgets.DrawHighlight(highlightRect);
                Rect leftSelectRect = new Rect(highlightRect.x + 2, highlightRect.y, highlightRect.height, highlightRect.height);
                if (ButtonTextSubtleCentered(leftSelectRect, "<"))
                {
                    if (index == 0)
                    {
                        index = list.Count() - 1;
                    }
                    else
                    {
                        index--;
                    }
                    selectAction(list.ElementAt(index));
                }
                Rect centerButtonRect = new Rect(leftSelectRect.xMax + 2, leftSelectRect.y, highlightRect.width - (2 * leftSelectRect.width), buttonHeight);
                if (ButtonTextSubtleCentered(centerButtonRect, labelGetter(list[index])))
                {
                    FloatMenuUtility.MakeMenu<T>(list, x => labelGetter(x), (T x) => delegate
                    {
                        selectAction(x);
                    });
                }
                Rect rightButtonRect = new Rect(centerButtonRect.xMax + 2, leftSelectRect.y, leftSelectRect.width, leftSelectRect.height);
                if (ButtonTextSubtleCentered(rightButtonRect, ">"))
                {
                    if (index == list.Count() - 1)
                    {
                        index = 0;
                    }
                    else
                    {
                        index++;
                    }
                    selectAction(list.ElementAt(index));
                }
            }
        }
        private static Rect GetLabelRect(Vector2 pos)
        {
            Rect rect = new Rect(pos.x, pos.y, labelWidth, buttonHeight);
            return rect;
        }
        private static Rect GetLabelRect(ref Vector2 pos)
        {
            Rect rect = new Rect(pos.x, pos.y, labelWidth, buttonHeight);
            pos.y += buttonHeight + 5;
            return rect;
        }
        private static Rect GetLabelRect(ref Vector2 pos, float labelWidth, float labelHeight)
        {
            Rect rect = new Rect(pos.x, pos.y, labelWidth, labelHeight);
            pos.y += labelHeight + 5;
            return rect;
        }
        private void CopyBodyFrom(Pawn source)
        {
            curSleeve.gender = source.gender;
            curSleeve.kindDef = source.kindDef;
            curSleeve.story.bodyType = source.story.bodyType;
            curSleeve.story.hairDef = source.story.hairDef;
            curSleeve.style.beardDef = source.style.beardDef;
            if (ModCompatibility.AlienRacesIsActive)
            {
                ModCompatibility.SetSkinColorFirst(curSleeve, ModCompatibility.GetSkinColorFirst(source));
                ModCompatibility.SetSkinColorSecond(curSleeve, ModCompatibility.GetSkinColorSecond(source));

                ModCompatibility.SetHairColorFirst(curSleeve, ModCompatibility.GetHairColorFirst(source));
                ModCompatibility.SetHairColorSecond(curSleeve, ModCompatibility.GetHairColorSecond(source));

                ModCompatibility.CopyBodyAddons(source, curSleeve);
            }
            else
            {

                curSleeve.story.HairColor = source.story.HairColor;
                curSleeve.story.SkinColorBase = source.story.SkinColor;
                curSleeve.story.headType = source.story.headType;
            }
            InitializeIndexes();
            UpdateSleeveGraphic();
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
            headTypeIndex = GetPermittedHeads().IndexOf(curSleeve.story.headType);

            if (ModCompatibility.AlienRacesIsActive)
            {
                raceTypeIndex = GetPermittedRaces().IndexOf(curSleeve.def);
            }

            if (curSleeve.gender == Gender.Male)
            {
                maleBodyTypeIndex = GetAllowedBodyTypesFor(curSleeve).Where(x => x != BodyTypeDefOf.Female).ToList().IndexOf(curSleeve.story.bodyType);
            }
            else if (curSleeve.gender == Gender.Female)
            {
                femaleBodyTypeIndex = GetAllowedBodyTypesFor(curSleeve).Where(x => x != BodyTypeDefOf.Male).ToList().IndexOf(curSleeve.story.bodyType);
            }

            foreach (HediffDef hediff in sleeveQualities)
            {
                if (curSleeve.health.hediffSet.HasHediff(hediff))
                {
                    sleeveQualityIndex = sleeveQualities.IndexOf(hediff);
                }
            }

            Trait trait = curSleeve.story.traits.GetTrait(TraitDefOf.Beauty);
            sleeveBeautyIndex = trait != null ? beautyDegrees.IndexOf(trait.Degree) : beautyDegrees.IndexOf(0);
            trait = curSleeve.story.traits.GetTrait(TraitDefOf.PsychicSensitivity);
            sleeveSensitivityIndex = trait != null ? sensitivityDegrees.IndexOf(trait.Degree) : sensitivityDegrees.IndexOf(0);
        }
        private void CreateSleeve(Gender gender)
        {
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
                fixedGender: gender, fixedBiologicalAge: 20, fixedChronologicalAge: 20));
            curSleeve.story.Adulthood = null;
            curSleeve.Name = new NameSingle("AC.EmptySleeve".Translate());
            curSleeve.story.title = "";
            curSleeve?.equipment.DestroyAllEquipment();
            curSleeve?.inventory.DestroyAll();
            curSleeve.apparel.DestroyAll();
            RemoveAllTraits(curSleeve);
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
            curHairColor = curSleeve.story.HairColor;
            curSkinColor = ModCompatibility.AlienRacesIsActive
                ? ModCompatibility.GetSkinColorFirst(curSleeve)
                : curSleeve.story.SkinColor;
            if (ModsConfig.IdeologyActive)
            {
                curSleeve.style.BodyTattoo = TattooDefOf.NoTattoo_Body;
                curSleeve.style.FaceTattoo = TattooDefOf.NoTattoo_Face;
            }

            InitializeIndexes();
            ApplyHediffs();
            ApplyBeauty();
            ApplySensitivity();
            UpdateGrowCost();
        }
        private List<BeardDef> GetPermittedBeards()
        {
            permittedBeards = DefDatabase<BeardDef>.AllDefs.Where(x => (curSleeve.gender == Gender.Male
            && (x.styleGender == StyleGender.MaleUsually || x.styleGender == StyleGender.Male || x.styleGender == StyleGender.Any))
            || (curSleeve.gender == Gender.Female && (x.styleGender == StyleGender.FemaleUsually || x.styleGender == StyleGender.Female
            || x.styleGender == StyleGender.Any))).ToList();
            return permittedBeards;
        }

        private List<HairDef> GetPermittedHairs()
        {
            permittedHairs = ModCompatibility.AlienRacesIsActive
                ? ModCompatibility.GetPermittedHair(currentPawnKindDef.race)
                : DefDatabase<HairDef>.AllDefs.ToList();
            return permittedHairs;
        }

        private List<HeadTypeDef> GetPermittedHeads()
        {
            permittedHeads = DefDatabase<HeadTypeDef>.AllDefs.Where(x => CanUseHeadType(x)).ToList();
            bool CanUseHeadType(HeadTypeDef head)
            {
                if (ModsConfig.BiotechActive && !head.requiredGenes.NullOrEmpty())
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
            return permittedHeads;
        }

        //button text subtle copied from Rimworld basecode but with minor changes to fit this UI
        public static bool ButtonTextSubtleCentered(Rect rect, string label, Vector2 functionalSizeOffset = default)
        {
            Rect rect2 = rect;
            rect2.width += functionalSizeOffset.x;
            rect2.height += functionalSizeOffset.y;
            bool flag = false;
            if (Mouse.IsOver(rect2))
            {
                flag = true;
                GUI.color = GenUI.MouseoverColor;
            }
            Widgets.DrawAtlas(rect, Widgets.ButtonSubtleAtlas);
            GUI.color = Color.white;
            Rect rect3 = new Rect(rect);
            if (flag)
            {
                rect3.x += 2f;
                rect3.y -= 2f;
            }
            Text.Anchor = TextAnchor.MiddleCenter;
            Text.WordWrap = false;
            Text.Font = GameFont.Small;
            Widgets.Label(rect3, label);
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.WordWrap = true;
            return Widgets.ButtonInvisible(rect2, false);
        }

        public static float ReturnYfromPrevious(Rect rect)
        {
            float y;
            y = rect.y;
            y += rect.height;
            y += optionOffset;

            return y;
        }

        public static Color rgbConvert(float r, float g, float b)
        {
            return new Color(1f / 255f * r, 1f / 255f * g, 1f / 255f * b);
        }
    }
}
