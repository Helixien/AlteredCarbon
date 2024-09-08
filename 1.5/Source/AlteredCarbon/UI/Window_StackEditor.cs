using System;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.Sound;
using static AlteredCarbon.UIHelper;

namespace AlteredCarbon
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [StaticConstructorOnStartup]
    [HotSwappable]
    public class Window_StackEditor : Window
    {
        public static Texture2D StackAddTrait = ContentFinder<Texture2D>.Get("UI/Icons/StackAddTrait");
        public static Texture2D StackRemoveTrait = ContentFinder<Texture2D>.Get("UI/Icons/StackRemoveTrait");
        public static Texture2D SkillBarFill = SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.1f));
        public static Texture2D ButtonPrevious = ContentFinder<Texture2D>.Get("UI/Icons/ButtonPrevious", true);
        public static Texture2D ButtonNext = ContentFinder<Texture2D>.Get("UI/Icons/ButtonNext", true);
        public static Dictionary<int, Texture2D> AllPassions = new Dictionary<int, Texture2D>
        {
            {1,  SkillUI.PassionMinorIcon },
            {2,  SkillUI.PassionMajorIcon }
        };

        private Building_NeuralEditor neuralEditor;
        private Thing thingWithStack;
        private NeuralData neuralData;
        private NeuralData neuralDataCopy;

        private int backstoryChildIndex;
        private int backstoryAdultIndex;
        private int factionIndex;
        private int ideoIndex;
        private List<BackstoryDef> allChildhoodBackstories;
        private List<BackstoryDef> allAdulthoodBackstories;
        private List<Faction> allFactions;
        private List<Ideo> allIdeos;
        private List<TraitDef> allTraits;

        public static int editTimeOffsetPerNameChange = 4000;
        public static int editTimeOffsetPerGenderChange = 8000;
        public static int editTimeOffsetPerSkillLevelChange = 500;
        public static int editTimeOffsetPerSkillPassionChange = 4000;
        public static int editTimeOffsetPerTraitChange = 4000;
        public static int editTimeOffsetPerChildhoodChange = 8000;
        public static int editTimeOffsetPerAdulthoodChange = 8000;
        public static int editTimeOffsetPerIdeologyChange = 6000;
        public static int editTimeOffsetPerCertaintyChange = 10;
        public static int editTimeOffsetPerFactionChange = 6000;
        public static int editTimeOffsetPerUnwaveringLoyalChange = 5000;

        public static float stackDegradationOffsetPerNameChange = 0.10f;
        public static float stackDegradationOffsetPerGenderChange = 0.5f;
        public static float stackDegradationOffsetPerSkillLevelChange = 0.05f;
        public static float stackDegradationOffsetPerSkillPassionChange = 0.10f;
        public static float stackDegradationOffsetPerTraitChange = 0.10f;
        public static float stackDegradationOffsetPerChildhoodChange = 0.5f;
        public static float stackDegradationOffsetPerAdulthoodChange = 0.5f;
        public static float stackDegradationOffsetPerIdeologyChange = 0.20f;
        public static float stackDegradationOffsetPerCertaintyChange = 0.01f;
        public static float stackDegradationOffsetPerFactionChange = 0.20f;
        public static float stackDegradationOffsetPerUnwaveringLoyalChange = 0.30f;

        private float LeftPanelWidth => 450;
        public override Vector2 InitialSize => new Vector2(900, Mathf.Min(UI.screenHeight, 975));
        public bool stackRecruitable;
        public Window_StackEditor(Building_NeuralEditor neuralEditor, Thing thingWithStack)
        {
            this.neuralEditor = neuralEditor;
            this.thingWithStack = thingWithStack;
            neuralData = new NeuralData();
            var toCopyFrom = thingWithStack.GetNeuralData();
            neuralData.CopyDataFrom(toCopyFrom);
            stackRecruitable = neuralData.recruitable;

            this.allChildhoodBackstories = DefDatabase<BackstoryDef>.AllDefsListForReading
                .Where(x => x.slot == BackstorySlot.Childhood && (x.spawnCategories?.Any(x => x == "ColonyAndroid" 
                || x == "AwakenedAndroid") is false)).ToList();
            if (neuralData.adulthood != null)
            {
                this.allAdulthoodBackstories = DefDatabase<BackstoryDef>.AllDefsListForReading
                .Where(x => x.slot == BackstorySlot.Adulthood && (x.spawnCategories?.Any(x => x == "ColonyAndroid"
                || x == "AwakenedAndroid") is false)).ToList();
            }
            allFactions = Find.FactionManager.AllFactions.Where(x => x.def.humanlikeFaction && x.Hidden is false).ToList();
            allIdeos = Find.IdeoManager.IdeosListForReading;
            allTraits = DefDatabase<TraitDef>.AllDefsListForReading;
            var modExtension = thingWithStack.GetStackSource().GetModExtension<StackSavingOptionsModExtension>();
            if (modExtension != null)
            {
                allTraits.RemoveAll(x => modExtension.ignoresTraits.Contains(x.defName));
            }
            neuralDataCopy = new NeuralData();
            neuralDataCopy.CopyDataFrom(neuralData);
            ResetIndices();
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
        }

        private void ResetIndices()
        {
            this.backstoryChildIndex = allChildhoodBackstories.FindIndex(x => x == neuralData.childhood);
            if (neuralData.adulthood != null)
            {
                this.backstoryAdultIndex = allAdulthoodBackstories.FindIndex(x => x == neuralData.adulthood);
            }
            if (neuralData.ideo != null)
            {
                ideoIndex = Find.IdeoManager.IdeosListForReading.IndexOf(neuralData.ideo);
            }
            if (neuralData.faction != null)
            {
                factionIndex = allFactions.IndexOf(neuralData.faction);
            }
        }

        private Vector2 windowScrollPos;
        private Vector2 traitAreaScrollPos;
        private float totalHeight = 0f;
        public override void DoWindowContents(Rect inRect)
        {
            var viewRect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height - 170);
            var totalRect = new Rect(viewRect.x, viewRect.y, viewRect.width - 16, totalHeight);
            Widgets.BeginScrollView(viewRect, ref windowScrollPos, totalRect);
            totalHeight = 0f;
            Vector2 pos = new Vector2(inRect.x + Margin, inRect.y);
            DrawTitle(ref pos, inRect);
            DrawNamePanel(ref pos);
            DrawGenderPanel(ref pos);
            DrawBackstoryPanel(ref pos);
            DrawFactionPanel(ref pos);
            if (ModsConfig.IdeologyActive)
            {
                DrawIdeoPanel(ref pos);
            }
            var leftY = pos.y;
            pos.x += (inRect.width - LeftPanelWidth) + 100;
            pos.y = inRect.y + 100;
            DrawSkillsPanel(ref pos, inRect);
            DrawTraitsPanel(ref pos, inRect);
            var rightY = pos.y;
            totalHeight = Mathf.Max(leftY, rightY);
            Widgets.EndScrollView();
            pos.x = inRect.x + Margin;
            pos.y = inRect.height - 175;
            DrawTimePanel(ref pos, inRect);
            DrawTutorialPanel(ref pos, inRect);
            DrawAcceptCancelButtons(inRect);
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
        }

        protected void DrawTitle(ref Vector2 pos, Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            var title = "AC.EditNeuralStack".Translate();
            Widgets.Label(GetLabelRect(title, ref pos, labelWidthOverride: inRect.width - (Margin * 2f)), title);
            Text.Anchor = TextAnchor.UpperLeft;
            pos.y += 15;
            var explanation = "AC.NeuralStackEditExplanation".Translate();
            Text.Font = GameFont.Tiny;
            GUI.color = Color.grey;
            Widgets.Label(GetLabelRect(explanation, ref pos, labelWidthOverride: inRect.width - (Margin * 2f)), explanation);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            pos.y += 15;
        }
        private void DrawSectionTitle(ref Vector2 pos, string title, float width)
        {
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Rect header = GetLabelRect(title, ref pos, labelWidthOverride: width);
            Widgets.Label(header, title);
            Widgets.DrawLine(new Vector2(header.x, header.yMax + 5f), new Vector2(header.xMax, header.yMax + 5),
                GUI.color * new Color(1f, 1f, 1f, 0.4f), 1f);
            Text.Anchor = TextAnchor.UpperLeft;
            pos.y += 15f;
            Text.Font = GameFont.Small;
        }
        protected void DrawNamePanel(ref Vector2 pos)
        {
            DrawSectionTitle(ref pos, "Name".Translate(), LeftPanelWidth);
            if (neuralData.name is NameTriple nameTriple)
            {
                DoNameInput(ref pos, "FirstName".Translate().CapitalizeFirst(), ref nameTriple.firstInt, delegate
                {
                    var name = PawnBioAndNameGenerator.GeneratePawnName(neuralData.DummyPawn, NameStyle.Full, null,
                        forceNoNick: false, neuralData.OriginalXenotypeDef);
                    if (name is NameTriple nameTriple1)
                    {
                        nameTriple.firstInt = nameTriple1.firstInt;
                    }
                    else if (name is NameSingle nameSingle)
                    {
                        nameTriple.firstInt = nameSingle.nameInt;
                    }
                });
                DoNameInput(ref pos, "NickName".Translate().CapitalizeFirst(), ref nameTriple.nickInt, delegate
                {
                    var name = PawnBioAndNameGenerator.GeneratePawnName(neuralData.DummyPawn, NameStyle.Full, null,
                        forceNoNick: false, neuralData.OriginalXenotypeDef);
                    if (name is NameTriple nameTriple1)
                    {
                        nameTriple.nickInt = nameTriple1.nickInt;
                    }
                    else if (name is NameSingle nameSingle)
                    {
                        nameTriple.nickInt = nameSingle.nameInt;
                    }
                });
                DoNameInput(ref pos, "LastName".Translate().CapitalizeFirst(), ref nameTriple.lastInt, delegate
                {
                    var name = PawnBioAndNameGenerator.GeneratePawnName(neuralData.DummyPawn, NameStyle.Full, null,
                        forceNoNick: false, neuralData.OriginalXenotypeDef);
                    if (name is NameTriple nameTriple1)
                    {
                        nameTriple.lastInt = nameTriple1.lastInt;
                    }
                    else if (name is NameSingle nameSingle)
                    {
                        nameTriple.lastInt = nameSingle.nameInt;
                    }
                });
            }
            else if (neuralData.name is NameSingle nameSingle)
            {
                DoNameInput(ref pos, "Name".Translate().CapitalizeFirst(), ref nameSingle.nameInt, delegate
                {
                    var name = PawnBioAndNameGenerator.GeneratePawnName(neuralData.DummyPawn, NameStyle.Full, null,
                    forceNoNick: false, neuralData.OriginalXenotypeDef);
                    neuralData.name = name;
                });
            }
        }

        private void DoNameInput(ref Vector2 pos, string nameField, ref string name, Action randomizeAction)
        {
            var nameLabel = nameField + ":";
            var nameRect = GetLabelRect(nameLabel, ref pos, 100);
            Widgets.Label(nameRect, nameLabel);
            var inputRect = new Rect(nameRect.xMax, nameRect.y, 250, 24);
            CharacterCardUtility.DoNameInputRect(inputRect, ref name, 250);
            var randomizeButton = new Rect(inputRect.xMax + 5, inputRect.y, 24, 24);
            if (Widgets.ButtonImage(randomizeButton, RandomizeSleeve))
            {
                SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                randomizeAction();
            }
        }

        protected void DrawGenderPanel(ref Vector2 pos)
        {
            DrawSectionTitle(ref pos, "Gender".Translate(), LeftPanelWidth);
            var originalGenderLabel = "AC.OriginalGender".Translate() + ":";
            var originalGenderRect = GetLabelRect(originalGenderLabel, ref pos, Text.CalcSize(originalGenderLabel).x + 5);
            Widgets.Label(originalGenderRect, originalGenderLabel);

            var maleLabel = "Male".Translate().CapitalizeFirst();
            var maleRect = new Rect(originalGenderRect.xMax + 15, originalGenderRect.y, Text.CalcSize(maleLabel).x + 15, originalGenderRect.height);
            Widgets.Label(maleRect, maleLabel);
            if (Widgets.RadioButton(new Vector2(maleRect.xMax, maleRect.y), neuralData.OriginalGender == Gender.Male))
            {
                neuralData.OriginalGender = Gender.Male;
            }
            var femaleLabel = "Female".Translate().CapitalizeFirst();
            var femaleRect = new Rect(maleRect.xMax + 50, maleRect.y, Text.CalcSize(femaleLabel).x + 15, maleRect.height);
            Widgets.Label(femaleRect, femaleLabel);
            if (Widgets.RadioButton(new Vector2(femaleRect.xMax, femaleRect.y), neuralData.OriginalGender == Gender.Female))
            {
                neuralData.OriginalGender = Gender.Female;
            }
        }
        protected void DrawBackstoryPanel(ref Vector2 pos)
        {
            DrawSectionTitle(ref pos, "AC.Backstories".Translate(), LeftPanelWidth);
            var backstoryFilters = new List<Func<BackstoryDef, (string, bool)>>();
            (string, bool) NoDisabledWorkTypes(BackstoryDef backstoryDef)
            {
                return ("AC.NoDisabledWorkTypes".Translate(), backstoryDef.workDisables == WorkTags.None);
            }
            backstoryFilters.Add(NoDisabledWorkTypes);
            (string, bool) NoSkillPenalties(BackstoryDef backstoryDef)
            {
                return ("AC.NoSkillPenalties".Translate(), backstoryDef.skillGains is null || backstoryDef.skillGains.Any(x => x.amount < 0) is false);
            }
            backstoryFilters.Add(NoSkillPenalties);

            DoSelectionButtons(ref pos, "Childhood".Translate(), ref backstoryChildIndex,
                (BackstoryDef x) => x.TitleCapFor(neuralData.OriginalGender), allChildhoodBackstories, delegate (BackstoryDef x)
                {
                    SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                    neuralData.childhood = x;
                    this.backstoryChildIndex = allChildhoodBackstories.FindIndex(x => x == neuralData.childhood);
                }, floatMenu: false, buttonOffsetFromTextOverride: 5f, labelWidthOverride: 80f, filter: null, includeInfoCard: false,
                tooltipGetter: (BackstoryDef x) => x.FullDescriptionFor(neuralData.DummyPawn).Resolve(),
                filters: backstoryFilters);
            if (neuralData.adulthood != null)
            {
                DoSelectionButtons(ref pos, "Adulthood".Translate(), ref backstoryAdultIndex,
                (BackstoryDef x) => x.TitleCapFor(neuralData.OriginalGender), allAdulthoodBackstories, delegate (BackstoryDef x)
                {
                    SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                    neuralData.adulthood = x;
                    this.backstoryAdultIndex = allAdulthoodBackstories.FindIndex(x => x == neuralData.adulthood);
                }, floatMenu: false, buttonOffsetFromTextOverride: 5f, labelWidthOverride: 80f, filter: null, includeInfoCard: false,
                tooltipGetter: (BackstoryDef x) => x.FullDescriptionFor(neuralData.DummyPawn).Resolve(),
                filters: backstoryFilters);
            }
            else
            {
                pos.y += buttonHeight + 5;
            }
        }
        private void DrawFactionPanel(ref Vector2 pos)
        {
            pos.y += 15;
            DrawSectionTitle(ref pos, "AC.FactionAllegiance".Translate(), LeftPanelWidth);
            DoSelectionButtons(ref pos, "Faction".Translate(), ref factionIndex, (Faction x) => x.Name, allFactions,
                delegate (Faction faction)
            {
                if (neuralData.faction != faction)
                {
                    neuralData.isFactionLeader = false;
                }
                neuralData.faction = faction;
                factionIndex = allFactions.IndexOf(faction);
            }, false, buttonOffsetFromTextOverride: 5f, labelWidthOverride: 80f, includeInfoCard: false, 
                tooltipGetter: (Faction t) => t.def.Description, icon: (Faction t) => t.def.FactionIcon, 
                iconColor: (Faction t) => t.def.DefaultColor, labelGetterPostfix: (Faction t) => t != Faction.OfPlayer ? (", " + 
                t.GoodwillWith(Faction.OfPlayer).ToString().Colorize(ColoredText.GetFactionRelationColor(t))) : "");
            
            if (stackRecruitable is false)
            {
                var unwaveringLoyal = neuralData.recruitable;
                var removeUnwaveringLoyalTrait = "AC.RemoveUnwaveringLoyalTrait".Translate();
                var removeRect = GetLabelRect(removeUnwaveringLoyalTrait, ref pos, Text.CalcSize(removeUnwaveringLoyalTrait).x + 50);
                Widgets.CheckboxLabeled(removeRect, removeUnwaveringLoyalTrait, ref unwaveringLoyal);
                neuralData.recruitable = unwaveringLoyal;
            }
        }

        private void DrawIdeoPanel(ref Vector2 pos)
        {
            pos.y += 15;
            DrawSectionTitle(ref pos, "AC.Ideology".Translate(), LeftPanelWidth);
            DoSelectionButtons(ref pos, "AC.Belief".Translate(), ref ideoIndex, (Ideo x) => x.name, allIdeos,
                delegate (Ideo ideo)
                {
                    neuralData.ChangeIdeo(ideo, neuralData.certainty);
                    ideoIndex = allIdeos.IndexOf(ideo);
                }, false, buttonOffsetFromTextOverride: 5f, labelWidthOverride: 80f, includeInfoCard: false,
                tooltipGetter: (Ideo t) => t.description, icon: (Ideo t) => t.Icon,
                iconColor: (Ideo t) => t.Color);

            Text.Anchor = TextAnchor.MiddleLeft;
            var certaintyLabel = "AC.Certainty".Translate() + ":";
            Rect labelRect = GetLabelRect(certaintyLabel, ref pos, 80f);
            Widgets.Label(labelRect, certaintyLabel);
            Text.Anchor = TextAnchor.UpperLeft;
            var buttonOffset = 5f;
            Rect centaintySliderRect = new Rect(labelRect.xMax + buttonOffset, labelRect.y, (buttonWidth * 2) + buttonOffsetFromButton, buttonHeight);
            neuralData.certainty = Widgets.HorizontalSlider(centaintySliderRect, neuralData.certainty, 0, 1f, true, neuralData.certainty.ToStringPercent());
            var explanation = "AC.ChangingPawnIdeologyWarning".Translate();
            Text.Font = GameFont.Tiny;
            GUI.color = Color.grey;
            Widgets.Label(GetLabelRect(explanation, ref pos, labelWidthOverride: LeftPanelWidth), explanation);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
        }

        private void DrawSkillsPanel(ref Vector2 pos, Rect inRect)
        {
            if (neuralData.skills != null)
            {
                var skillsLabel = "Skills".Translate();
                var resetButton = new Rect(inRect.width - 75 - Margin, pos.y, 75, 24);
                var labelRect = GetLabelRect(skillsLabel, ref pos, inRect.width - pos.x);
                Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(labelRect, skillsLabel);
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.UpperLeft;
                if (Widgets.ButtonText(resetButton, "Reset".Translate()))
                {
                    SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                    neuralData.skills = new List<SkillRecord>();
                    foreach (var skill in neuralDataCopy.skills)
                    {
                        neuralData.skills.Add(new SkillRecord
                        {
                            def = skill.def,
                            levelInt = skill.levelInt,
                            xpSinceLastLevel = skill.xpSinceLastLevel,
                            xpSinceMidnight = skill.xpSinceMidnight,
                            passion = skill.passion,
                        });
                    }
                }

                var skillBoxHeight = neuralData.skills.Count() * (Text.LineHeight + 5) + (this.Margin * 2) - 5;
                Rect traitsFill = new Rect(pos.x, pos.y, inRect.width - pos.x - Margin, skillBoxHeight);
                Rect skillsContainer = new Rect(traitsFill.x, traitsFill.y, traitsFill.width, skillBoxHeight);
                Widgets.DrawRectFast(traitsFill, Widgets.MenuSectionBGFillColor, null);


                Widgets.BeginScrollView(traitsFill, ref traitAreaScrollPos, skillsContainer);
                skillsContainer.x += this.Margin;
                skillsContainer.y += this.Margin;

                var rect = GenUI.DrawElementStackVertical(skillsContainer, Text.LineHeight, neuralData.skills, delegate(Rect rect, SkillRecord skill)
                    {
                        Rect labelRect = new Rect(rect.x, rect.y, 110, rect.height);
                        Widgets.Label(labelRect, skill.def.skillLabel.CapitalizeFirst());

                        Rect passionRect = new Rect(labelRect.xMax + (labelRect.height * 2) + 100 + 15, labelRect.y, labelRect.height, labelRect.height);
                        
                        if (Mouse.IsOver(passionRect))
                        {
                            Widgets.DrawHighlight(passionRect);
                        }

                        if (Widgets.ButtonInvisible(passionRect))
                        {
                            SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                            var ind = (int)skill.passion;
                            if (ind == AllPassions.Select(x => x.Key).Max())
                            {
                                ind = 0;
                            }
                            else
                            {
                                ind++;
                            }
                            skill.passion = (Passion)ind;
                        }

                        if (!skill.TotallyDisabled)
                        {
                            var decrementSkillRect = new Rect(labelRect.xMax, labelRect.y, labelRect.height, labelRect.height);
                            if (Widgets.ButtonImage(decrementSkillRect, ButtonPrevious))
                            {
                                SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                                var minLevel = MinLevelOfSkill(neuralData.DummyPawn, skill.def);
                                if (skill.Level > minLevel)
                                {
                                    skill.Level -= 1;
                                }
                            }

                            Rect skillBar = new Rect(decrementSkillRect.xMax + 5, decrementSkillRect.y, 100, rect.height);
                            float fillPercent = Mathf.Max(0.01f, skill.Level / 20f);
                            Texture2D fillTex = SkillUI.SkillBarFillTex;

                            var initialSkill = neuralDataCopy.skills.First(x => x.def == skill.def);

                            float barSize = (skill.Level > 0 ? (float)skill.Level : 0) / 20f;
                            FillableBar(skillBar, barSize, SkillBarFill);

                            int baseLevel = initialSkill.Level;
                            float baseBarSize = (baseLevel > 0 ? (float)baseLevel : 0) / 20f;
                            FillableBar(skillBar, baseBarSize, SkillBarFill);

                            GUI.color = Color.grey;
                            Widgets.DrawBox(skillBar);
                            GUI.color = Color.white;

                            var incrementLevelRect = new Rect(skillBar.xMax + 5, labelRect.y, labelRect.height, labelRect.height);
                            if (Widgets.ButtonImage(incrementLevelRect, ButtonNext))
                            {
                                SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                                skill.Level += 1;
                            }
                            
                            if (skill.passion > Passion.None)
                            {
                                Texture2D image = AllPassions[(int)skill.passion];
                                GUI.DrawTexture(passionRect, image);
                            }
                        }

                        Rect skillLevelLabel = new Rect(labelRect.xMax + labelRect.height + 10, passionRect.y, 999f, rect.height);
                        skillLevelLabel.yMin += 3f;
                        string label;
                        if (skill.TotallyDisabled)
                        {
                            GUI.color = SkillUI.DisabledSkillColor;
                            label = "-";
                        }
                        else
                        {
                            if (skill.Level == 0 && skill.Aptitude != 0)
                            {
                                GUI.color = ((skill.Aptitude > 0) ? ColorLibrary.BrightGreen : ColorLibrary.RedReadable);
                            }
                        
                            label = skill.Level.ToStringCached();
                        }
                        
                        GenUI.SetLabelAlign(TextAnchor.MiddleLeft);
                        Widgets.Label(skillLevelLabel, label);
                        GenUI.ResetLabelAlign();
                        GUI.color = Color.white;
                    }, element => skillsContainer.width
                );
                Widgets.EndScrollView();
                pos.y += rect.height;
            }
        }

        private static int MinLevelOfSkill(Pawn pawn, SkillDef sk)
        {
            float num = 0;
            foreach (BackstoryDef item in pawn.story.AllBackstories.Where((BackstoryDef bs) => bs != null))
            {
                foreach (var skillGain in item.skillGains)
                {
                    if (skillGain.skill == sk)
                    {
                        num += (float)skillGain.amount * Rand.Range(1f, 1.4f);
                    }
                }
            }
            for (int i = 0; i < pawn.story.traits.allTraits.Count; i++)
            {
                if (!pawn.story.traits.allTraits[i].Suppressed 
                    && pawn.story.traits.allTraits[i].CurrentData.skillGains.FirstOrDefault(x => x.skill == sk) is SkillGain skillGain)
                {
                    num += (float)skillGain.amount;
                }
            }
            return Mathf.Clamp(Mathf.RoundToInt(num), 0, 20);
        }

        public static void FillableBar(Rect rect, float fillPercent, Texture2D fillTex)
        {
            rect.width *= fillPercent;
            GUI.DrawTexture(rect, fillTex);
        }


        private void DrawTraitsPanel(ref Vector2 pos, Rect inRect)
        {
            pos.y += 15;
            var panelWidth = (inRect.width - pos.x) - Margin;
            Rect addTraitRect = new Rect(pos.x + panelWidth - 24, pos.y + 5, 24f, 24f);
            DrawSectionTitle(ref pos, "Traits".Translate(), panelWidth);
            if (Widgets.ButtonImage(addTraitRect, StackAddTrait))
            {
                var pawn = neuralData.DummyPawn;
                var traitCandidates = new List<Trait>();
                foreach (var newTraitDef in allTraits)
                {
                    if (pawn.story.traits.HasTrait(newTraitDef) || (newTraitDef == TraitDefOf.Gay
                        && (LovePartnerRelationUtility.HasAnyLovePartnerOfTheOppositeGender(pawn)
                        || LovePartnerRelationUtility.HasAnyExLovePartnerOfTheOppositeGender(pawn))))
                    {
                        continue;
                    }
                    if (neuralData.traits.Any((Trait tr) => newTraitDef.ConflictsWith(tr))
                        || (newTraitDef.requiredWorkTypes != null && pawn.OneOfWorkTypesIsDisabled(newTraitDef.requiredWorkTypes))
                        || pawn.WorkTagIsDisabled(newTraitDef.requiredWorkTags) || (newTraitDef.forcedPassions != null
                        && pawn.workSettings != null && newTraitDef.forcedPassions.Any((SkillDef p)
                        => p.IsDisabled(pawn.story.DisabledWorkTagsBackstoryTraitsAndGenes, pawn.GetDisabledWorkTypes(permanentOnly: true)))))
                    {
                        continue;
                    }
                    for (int i = 0; i < newTraitDef.degreeDatas.Count; i++)
                    {
                        int degree = newTraitDef.degreeDatas[i].degree;
                        if ((pawn.story.Childhood == null || !pawn.story.Childhood.DisallowsTrait(newTraitDef, degree))
                            && (pawn.story.Adulthood == null || !pawn.story.Adulthood.DisallowsTrait(newTraitDef, degree)))
                        {
                            Trait trait = new Trait(newTraitDef, degree);
                            if (neuralData.traits.Any((Trait tr) => newTraitDef == tr.def && degree == tr.degree) is false
                                && (pawn.kindDef.disallowedTraitsWithDegree.NullOrEmpty()
                                || !pawn.kindDef.disallowedTraitsWithDegree.Any((TraitRequirement t) => t.Matches(trait)))
                                && (pawn.mindState == null || pawn.mindState.mentalBreaker == null
                                || !((pawn.mindState.mentalBreaker.BreakThresholdMinor + trait.OffsetOfStat(StatDefOf.MentalBreakThreshold))
                                * trait.MultiplierOfStat(StatDefOf.MentalBreakThreshold) > 0.5f)))
                            {
                                traitCandidates.Add(trait);
                            }
                        }
                    }
                }
                var traitFilters = new List<Func<Trait, (string, bool)>>();
                (string, bool) NoDisabledWorkTypes(Trait trait)
                {
                    return ("AC.NoDisabledWorkTypes".Translate(), trait.def.disabledWorkTags == WorkTags.None && trait.def.disabledWorkTypes.NullOrEmpty());
                }
                traitFilters.Add(NoDisabledWorkTypes);
                (string, bool) NoSkillPenalties(Trait trait)
                {
                    return ("AC.NoSkillPenalties".Translate(), trait.def.DataAtDegree(trait.degree).skillGains is null
                        || trait.def.DataAtDegree(trait.degree).skillGains.Any(x => x.amount < 0) is false);
                }
                traitFilters.Add(NoSkillPenalties);

                Find.WindowStack.Add(new Window_SelectItem<Trait>(traitCandidates.First(), traitCandidates, delegate (Trait trait)
                {
                    neuralData.traits.Add(trait);
                }, labelGetter: (Trait t) => t.LabelCap,
                    tooltipGetter: (Trait t) => t.TipString(pawn), filters: traitFilters, includeInfoCard: false));
            }
            Rect traitsContainer = new Rect(pos.x, pos.y, panelWidth, 100);
            var rect = GenUI.DrawElementStack(traitsContainer, Text.LineHeight, neuralData.traits.ToList(), delegate (Rect r, Trait trait)
            {
                Color color3 = GUI.color;
                GUI.color = StackElementBackground;
                GUI.DrawTexture(r, BaseContent.WhiteTex);
                GUI.color = color3;
                if (Mouse.IsOver(r))
                {
                    Widgets.DrawHighlight(r);
                }
                if (trait.Suppressed)
                {
                    GUI.color = ColoredText.SubtleGrayColor;
                }
                else if (trait.sourceGene != null)
                {
                    GUI.color = ColoredText.GeneColor;
                }
                Widgets.Label(new Rect(r.x + 5f, r.y, r.width - 10f, r.height), trait.LabelCap);
                GUI.color = Color.white;
                if (Mouse.IsOver(r))
                {
                    Trait trLocal = trait;
                    TooltipHandler.TipRegion(tip: new TipSignal(trLocal.TipString(neuralData.DummyPawn)), rect: r);
                }

                var buttonRect = new Rect(r.xMax - r.height, r.y, r.height, r.height);
                GUI.DrawTexture(buttonRect, StackRemoveTrait);
                if (Widgets.ButtonInvisible(buttonRect, true))
                {
                    SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                    neuralData.traits.Remove(trait);
                }
            }, trait => Text.CalcSize(trait.LabelCap).x + Text.LineHeight + 10f);
            pos.y += Mathf.Max(100, rect.height);
        }

        private void DrawTimePanel(ref Vector2 pos, Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Rect editTime = new Rect(pos.x, pos.y, inRect.width - (this.Margin * 2), 32);
            string time = ToStringTicksToHours(GetEditTime());
            Widgets.Label(editTime, "AC.TotalTimeToEdit".Translate(time));
            editTime.y += Text.LineHeight + 5;
            if (AC_Utils.editStacksSettings.enableStackDegradation)
            {
                string stackDegradation = Mathf.Min(1f, this.neuralData.stackDegradation + GetDegradation()).ToStringPercent();
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.DrawHighlight(editTime);
                editTime.xMin += 10f;
                Text.Font = GameFont.Small;
                Widgets.Label(editTime, "AC.TotalStackDegradation".Translate(stackDegradation.Colorize(Color.red)));
                Text.Anchor = TextAnchor.UpperLeft;
            }
            pos.y += (editTime.yMax - pos.y) + 15;
        }

        public static string ToStringTicksToHours(int numTicks)
        {
            string text = ((float)numTicks / 2500f).ToString("0.#");
            if (text == "1")
            {
                return "Period1Hour".Translate();
            }
            return "PeriodHours".Translate(text);
        }

        private void DrawTutorialPanel(ref Vector2 pos, Rect inRect)
        {
            GUI.color = Color.grey;
            var tutorial = "AC.Tutorial".Translate();
            var label = GetLabelRect(tutorial, ref pos, inRect.width - (Margin * 2f));
            Widgets.Label(label, tutorial);
            GUI.color = Color.white;
        }

        private void DrawAcceptCancelButtons(Rect inRect)
        {
            var buttonWidth = 200;
            var resetAllButtonRect = new Rect(buttonWidth / 2f, inRect.height - 32, buttonWidth, 32); 
            if (Widgets.ButtonText(resetAllButtonRect, "AC.ResetAll".Translate()))
            {
                ResetAll();
            }
            var cancelButtonRect = new Rect((inRect.width / 2f) - (buttonWidth / 2f), resetAllButtonRect.y, buttonWidth, 32);
            if (Widgets.ButtonText(cancelButtonRect, "Cancel".Translate()))
            {
                ResetAll();
                this.Close();
            }
            var acceptButtonRect = new Rect(inRect.width - (buttonWidth + (buttonWidth / 2f)), resetAllButtonRect.y, buttonWidth, 32);
            if (Widgets.ButtonText(acceptButtonRect, "AC.StartEditing".Translate()))
            {
                var origData = thingWithStack.GetNeuralData();
                origData.editTime = GetEditTime();
                if (AC_Utils.editStacksSettings.enableStackDegradation)
                {
                    origData.stackDegradationToAdd = GetDegradation();
                }
                origData.neuralDataRewritten = neuralData;
                var recipe = thingWithStack is Pawn ? AC_DefOf.AC_EditActiveNeuralStackPawn : AC_DefOf.AC_EditActiveNeuralStack;
                neuralEditor.billStack.AddBill(new Bill_EditStack(thingWithStack, recipe, null));
                this.Close();
            }
        }

        private int GetEditTime()
        {
            var time = 0;
            if (neuralDataCopy.name.ToStringFull != neuralData.name.ToStringFull)
            {
                time += editTimeOffsetPerNameChange;
            }
            if (neuralDataCopy.OriginalGender != neuralData.OriginalGender)
            {
                time += editTimeOffsetPerGenderChange;
            }
            if (neuralDataCopy.skills != null)
            {
                foreach (var origSkill in neuralDataCopy.skills)
                {
                    var curSkill = neuralData.skills.FirstOrDefault(x => x.def == origSkill.def);
                    var skillLevelDiff = Mathf.Abs(curSkill.Level - origSkill.Level);
                    if (skillLevelDiff > 0)
                    {
                        time += skillLevelDiff * editTimeOffsetPerSkillLevelChange;
                    }
                    var skillPassionsDiff = Mathf.Abs((int)curSkill.passion - (int)origSkill.passion);
                    if (skillPassionsDiff > 0)
                    {
                        time += skillPassionsDiff * editTimeOffsetPerSkillPassionChange;
                    }
                }
            }
            if (neuralDataCopy.traits != null)
            {
                var origTraits = neuralDataCopy.traits;
                var curTraits = neuralData.traits;
                foreach (var trait in curTraits)
                {
                    if (origTraits.Any(x => trait.def == x.def && trait.degree == x.degree) is false)
                    {
                        time += editTimeOffsetPerTraitChange;
                    }
                }
                var missingTraits = origTraits.Where(x => curTraits.Any(y => y.degree == x.degree && y.def == x.def) is false);
                var missingTraitsCount = missingTraits.Count();
                time += missingTraitsCount * editTimeOffsetPerTraitChange;
            }

            if (neuralDataCopy.childhood != neuralData.childhood)
            {
                time += editTimeOffsetPerChildhoodChange;
            }
            if (neuralDataCopy.adulthood != neuralData.adulthood)
            {
                time += editTimeOffsetPerAdulthoodChange;
            }
            if (neuralDataCopy.ideo != neuralData.ideo)
            {
                time += editTimeOffsetPerIdeologyChange;
            }
            var certaintyDiff = Mathf.Abs(neuralDataCopy.certainty - neuralData.certainty) * 100f;
            if (certaintyDiff > 0)
            {
                time += (int)(certaintyDiff * editTimeOffsetPerCertaintyChange);
            }
            if (neuralDataCopy.faction != neuralData.faction)
            {
                time += editTimeOffsetPerFactionChange;
            }
            if (neuralDataCopy.recruitable != neuralData.recruitable)
            {
                time += editTimeOffsetPerUnwaveringLoyalChange;
            }
            return (int)(time * AC_Utils.editStacksSettings.stackEditEditTimeValueMultiplier);
        }

        private float GetDegradation()
        {
            var degradation = 0f;
            if (neuralDataCopy.name.ToStringFull != neuralData.name.ToStringFull)
            {
                degradation += stackDegradationOffsetPerNameChange;
            }
            if (neuralDataCopy.OriginalGender != neuralData.OriginalGender)
            {
                degradation += stackDegradationOffsetPerGenderChange;
            }
            if (neuralDataCopy.skills != null)
            {
                foreach (var origSkill in neuralDataCopy.skills)
                {
                    var curSkill = neuralData.skills.FirstOrDefault(x => x.def == origSkill.def);
                    var skillLevelDiff = Mathf.Abs(curSkill.Level - origSkill.Level);
                    if (skillLevelDiff > 0)
                    {
                        degradation += skillLevelDiff * stackDegradationOffsetPerSkillLevelChange;
                    }
                    var skillPassionsDiff = Mathf.Abs((int)curSkill.passion - (int)origSkill.passion);
                    if (skillPassionsDiff > 0)
                    {
                        degradation += skillPassionsDiff * stackDegradationOffsetPerSkillPassionChange;
                    }
                }
            }

            if (neuralDataCopy.traits != null)
            {
                var origTraits = neuralDataCopy.traits;
                var curTraits = neuralData.traits;
                foreach (var trait in curTraits)
                {
                    if (origTraits.Any(x => trait.def == x.def && trait.degree == x.degree) is false)
                    {
                        degradation += stackDegradationOffsetPerTraitChange;
                    }
                }
                var missingTraits = origTraits.Where(x => curTraits.Any(y => y.degree == x.degree && y.def == x.def) is false);
                var missingTraitsCount = missingTraits.Count();
                degradation += missingTraitsCount * stackDegradationOffsetPerTraitChange;
            }

            if (neuralDataCopy.childhood != neuralData.childhood)
            {
                degradation += stackDegradationOffsetPerChildhoodChange;
            }
            if (neuralDataCopy.adulthood != neuralData.adulthood)
            {
                degradation += stackDegradationOffsetPerChildhoodChange;
            }
            if (neuralDataCopy.ideo != neuralData.ideo)
            {
                degradation += stackDegradationOffsetPerIdeologyChange;
            }
            var certaintyDiff = Mathf.Abs(neuralDataCopy.certainty - neuralData.certainty) * 100f;
            if (certaintyDiff > 0)
            {
                degradation += (int)(certaintyDiff * stackDegradationOffsetPerCertaintyChange);
            }
            if (neuralDataCopy.faction != neuralData.faction)
            {
                degradation += stackDegradationOffsetPerFactionChange;
            }
            if (neuralDataCopy.recruitable != neuralData.recruitable)
            {
                degradation += stackDegradationOffsetPerUnwaveringLoyalChange;
            }
            return degradation * AC_Utils.editStacksSettings.stackEditDegradationValueMultiplier;
        }

        private void ResetAll()
        {
            SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
            neuralData.CopyDataFrom(neuralDataCopy);
            ResetIndices();
        }
    }
}