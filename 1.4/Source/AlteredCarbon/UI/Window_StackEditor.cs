using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using static AlteredCarbon.UIHelper;

namespace AlteredCarbon
{
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

        private Building_DecryptionBench decryptionBench;
        private CorticalStack corticalStack;
        private PersonaData personaData;
        private int backstoryChildIndex;
        private int backstoryAdultIndex;
        private int factionIndex;
        private int ideoIndex;
        private List<SkillRecord> initialSkills;
        private float LeftPanelWidth => 450;
        private List<BackstoryDef> allChildhoodBackstories;
        public override Vector2 InitialSize => new Vector2(900, 1000);

        public Window_StackEditor(Building_DecryptionBench decryptionBench, CorticalStack corticalStack)
        {
            this.decryptionBench = decryptionBench;
            this.corticalStack = corticalStack;
            corticalStack.personaDataRewritten = new PersonaData();
            corticalStack.personaDataRewritten.CopyDataFrom(corticalStack.PersonaData);
            personaData = corticalStack.personaDataRewritten;
            this.allChildhoodBackstories = DefDatabase<BackstoryDef>.AllDefsListForReading
                .Where(x => x.slot == BackstorySlot.Childhood).ToList();
            this.backstoryChildIndex = allChildhoodBackstories
                .FindIndex(x => x == personaData.childhood);

            if (personaData.adulthood != null)
            {
                this.backstoryAdultIndex = DefDatabase<BackstoryDef>.AllDefsListForReading
                    .Where(x => x.slot == BackstorySlot.Adulthood).ToList()
                    .FindIndex(x => x == personaData.adulthood);
            }
            if (personaData.skills != null)
            {
                initialSkills = new List<SkillRecord>();
                foreach (var skill in personaData.skills)
                {
                    initialSkills.Add(new SkillRecord
                    {
                        def = skill.def,
                        levelInt = skill.levelInt,
                        xpSinceLastLevel = skill.xpSinceLastLevel,
                        xpSinceMidnight = skill.xpSinceMidnight,
                        passion = skill.passion,
                        pawn = skill.pawn
                    });
                }
            }

        }


        private Vector2 scrollPos;
        public override void DoWindowContents(Rect inRect)
        {
            Vector2 pos = new Vector2(inRect.x + Margin, inRect.y);
            DrawTitle(ref pos, inRect);
            DrawBackstoryPanel(ref pos);
            DrawTraitsPanel(ref pos);
            DrawFactionPanel(ref pos);
            if (ModsConfig.IdeologyActive)
            {
                DrawIdeoPanel(ref pos);
            }
            pos.x += (inRect.width - LeftPanelWidth) + 100;
            pos.y = inRect.y + 100;
            DrawSkillsPanel(ref pos, inRect);
            ////TODO: ideo panel
            ////TODO: shadow tutorial panel
            //DrawTutorialPanel(ref inRect);
            ////TODO: editing time panel
            //DrawTimePanel(ref inRect);
            ////TODO: accept/cancel buttons
            //DrawAcceptCancelButtons(ref inRect);
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
        }

        protected void DrawTitle(ref Vector2 pos, Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            var title = "AC.RewriteCorticalStack".Translate();
            Widgets.Label(GetLabelRect(title, ref pos, labelWidthOverride: inRect.width - (Margin * 2f)), title);
            Text.Anchor = TextAnchor.UpperLeft;
            pos.y += 15;
            var explanation = "AC.CorticalStackEditExplanation".Translate();
            Text.Font = GameFont.Tiny;
            GUI.color = Color.grey;
            Widgets.Label(GetLabelRect(explanation, ref pos, labelWidthOverride: inRect.width - (Margin * 2f)), explanation);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            pos.y += 15;
        }
        private void DrawSectionTitle(ref Vector2 pos, string title)
        {
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Rect header = GetLabelRect(title, ref pos, labelWidthOverride: LeftPanelWidth);
            Widgets.Label(header, title);
            Widgets.DrawLine(new Vector2(header.x, header.yMax + 5f), new Vector2(header.xMax, header.yMax + 5),
                GUI.color * new Color(1f, 1f, 1f, 0.4f), 1f);
            Text.Anchor = TextAnchor.UpperLeft;
            pos.y += 15f;
            Text.Font = GameFont.Small;
        }
        protected void DrawBackstoryPanel(ref Vector2 pos)
        {
            DrawSectionTitle(ref pos, "AC.Backstories".Translate());
            var backstoryFilters = new List<Func<BackstoryDef, (string, bool)>>();
            (string, bool) NoFilters(BackstoryDef backstoryDef)
            {
                return ("AC.NoFilters".Translate(), true);
            }
            backstoryFilters.Add(NoFilters);
            (string, bool) NoDisabledWorkTypes(BackstoryDef backstoryDef)
            {
                return ("AC.NoDisabledWorkTypes".Translate(), backstoryDef.workDisables == WorkTags.None);
            }
            backstoryFilters.Add(NoDisabledWorkTypes);
            (string, bool) NoSkillPenalties(BackstoryDef backstoryDef)
            {
                return ("AC.NoSkillPenalties".Translate(), backstoryDef.skillGains is null || backstoryDef.skillGains.Any(x => x.Value < 0) is false);
            }
            backstoryFilters.Add(NoSkillPenalties);

            var allChildhoodBackstories = DefDatabase<BackstoryDef>.AllDefsListForReading.Where(x => x.slot == BackstorySlot.Childhood)
                .OrderBy(x => x.TitleCapFor(personaData.gender)).ToList();
            DoSelectionButtons(ref pos, "Childhood".Translate(), ref backstoryChildIndex,
                (BackstoryDef x) => x.TitleCapFor(personaData.gender), allChildhoodBackstories, delegate (BackstoryDef x)
                {
                    SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                    personaData.childhood = x;
                    this.backstoryChildIndex = allChildhoodBackstories
                        .FindIndex(x => x == personaData.childhood);
                }, floatMenu: false, buttonOffsetFromTextOverride: 5f, labelWidthOverride: 80f, filter: null, includeInfoCard: false,
                tooltipGetter: (BackstoryDef x) => x.FullDescriptionFor(personaData.GetDummyPawn).Resolve(),
                filters: backstoryFilters);
            if (personaData.adulthood != null)
            {
                var allAdultBackstories = DefDatabase<BackstoryDef>.AllDefsListForReading.Where(x => x.slot == BackstorySlot.Adulthood)
                    .OrderBy(x => x.TitleCapFor(personaData.gender)).ToList();
                DoSelectionButtons(ref pos, "Adulthood".Translate(), ref backstoryAdultIndex,
                (BackstoryDef x) => x.TitleCapFor(personaData.gender), allAdultBackstories, delegate (BackstoryDef x)
                {
                    SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                    personaData.adulthood = x;
                    this.backstoryAdultIndex = allChildhoodBackstories
                        .FindIndex(x => x == personaData.adulthood);
                }, floatMenu: false, buttonOffsetFromTextOverride: 5f, labelWidthOverride: 80f, filter: null, includeInfoCard: false,
                tooltipGetter: (BackstoryDef x) => x.FullDescriptionFor(personaData.GetDummyPawn).Resolve(),
                filters: backstoryFilters);
            }
            else
            {
                pos.y += buttonHeight + 5;
            }
        }
        private void DrawTraitsPanel(ref Vector2 pos)
        {
            pos.y += 15;
            Rect addTraitRect = new Rect(pos.x + LeftPanelWidth - 24, pos.y + 5, 24f, 24f);
            DrawSectionTitle(ref pos, "Traits".Translate());
            if (Widgets.ButtonImage(addTraitRect, StackAddTrait))
            {
                var pawn = personaData.GetDummyPawn;
                var allTraits = DefDatabase<TraitDef>.AllDefsListForReading
                    .Where(x => x.GetGenderSpecificCommonality(pawn.gender) > 0);
                var traitCandidates = new List<Trait>();
                foreach (var newTraitDef in allTraits)
                {
                    if (pawn.story.traits.HasTrait(newTraitDef) || (newTraitDef == TraitDefOf.Gay 
                        && (LovePartnerRelationUtility.HasAnyLovePartnerOfTheOppositeGender(pawn) 
                        || LovePartnerRelationUtility.HasAnyExLovePartnerOfTheOppositeGender(pawn))))
                    {
                        continue;
                    }
                    if (personaData.traits.Any((Trait tr) => newTraitDef.ConflictsWith(tr)) 
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
                            if (personaData.traits.Any((Trait tr) => newTraitDef == tr.def && degree == tr.degree) is false
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
                (string, bool) NoFilters(Trait trait)
                {
                    return ("AC.NoFilters".Translate(), true);
                }
                traitFilters.Add(NoFilters);
                (string, bool) NoDisabledWorkTypes(Trait trait)
                {
                    return ("AC.NoDisabledWorkTypes".Translate(), trait.def.disabledWorkTags == WorkTags.None && trait.def.disabledWorkTypes.NullOrEmpty());
                }
                traitFilters.Add(NoDisabledWorkTypes);
                (string, bool) NoSkillPenalties(Trait trait)
                {
                    return ("AC.NoSkillPenalties".Translate(), trait.def.DataAtDegree(trait.degree).skillGains is null 
                        || trait.def.DataAtDegree(trait.degree).skillGains.Any(x => x.Value < 0) is false);
                }
                traitFilters.Add(NoSkillPenalties);

                Find.WindowStack.Add(new Window_SelectItem<Trait>(traitCandidates.First(), traitCandidates, delegate (Trait trait)
                {
                    personaData.traits.Add(trait);
                }, labelGetter: (Trait t) => t.LabelCap,
                    tooltipGetter: (Trait t) => t.TipString(pawn), filters: traitFilters, includeInfoCard: false));
            }
            Rect traitsContainer = new Rect(pos.x, pos.y, LeftPanelWidth, 100);
            var rect = GenUI.DrawElementStack(traitsContainer, Text.LineHeight, personaData.traits, delegate (Rect r, Trait trait)
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
                    TooltipHandler.TipRegion(tip: new TipSignal(trLocal.TipString(personaData.GetDummyPawn)), rect: r);
                }

                var buttonRect = new Rect(r.xMax - r.height, r.y, r.height, r.height);
                GUI.DrawTexture(buttonRect, StackRemoveTrait);
                if (Widgets.ButtonInvisible(buttonRect, true))
                {
                    SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                    personaData.traits.Remove(trait);
                }
            }, trait => Text.CalcSize(trait.LabelCap).x + Text.LineHeight + 10f);
            pos.y += Mathf.Max(100, rect.height);
        }

        private void DrawFactionPanel(ref Vector2 pos)
        {
            pos.y += 15;
            DrawSectionTitle(ref pos, "AC.FactionAllegiance".Translate());
            var allFactions = Find.FactionManager.AllFactions.Where(x => x.def.humanlikeFaction).ToList();
            DoSelectionButtons(ref pos, "Faction".Translate(), ref factionIndex, (Faction x) => x.Name, allFactions,
                delegate (Faction faction)
            {
                if (personaData.faction != faction)
                {
                    personaData.isFactionLeader = false;
                }
                personaData.faction = faction;
                factionIndex = allFactions.IndexOf(faction);
            }, false, buttonOffsetFromTextOverride: 5f, labelWidthOverride: 80f, includeInfoCard: false, 
                tooltipGetter: (Faction t) => t.def.Description, icon: (Faction t) => t.def.FactionIcon, 
                iconColor: (Faction t) => t.def.DefaultColor, labelGetterPostfix: (Faction t) => t != Faction.OfPlayer ? (", " + 
                t.GoodwillWith(Faction.OfPlayer).ToString().Colorize(ColoredText.GetFactionRelationColor(t))) : "");

            var removeUnwaveringLoyalTrait = "AC.RemoveUnwaveringLoyalTrait".Translate();
            var removeRect = GetLabelRect(removeUnwaveringLoyalTrait, ref pos, Text.CalcSize(removeUnwaveringLoyalTrait).x + 50);
            var unwaveringLoyal = personaData.recruitable is false;
            Widgets.CheckboxLabeled(removeRect, removeUnwaveringLoyalTrait, ref unwaveringLoyal);
            personaData.recruitable = !unwaveringLoyal;
        }

        private void DrawIdeoPanel(ref Vector2 pos)
        {
            pos.y += 15;
            DrawSectionTitle(ref pos, "AC.Ideology".Translate());
            var allIdeos = Find.IdeoManager.IdeosListForReading;
            DoSelectionButtons(ref pos, "AC.Belief".Translate(), ref ideoIndex, (Ideo x) => x.name, allIdeos,
                delegate (Ideo ideo)
                {
                    personaData.ChangeIdeo(ideo, personaData.certainty);
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
            personaData.certainty = Widgets.HorizontalSlider_NewTemp(centaintySliderRect, personaData.certainty, 0, 1f, true, personaData.certainty.ToStringPercent());
            var explanation = "AC.ChangingPawnIdeologyWarning".Translate();
            Text.Font = GameFont.Tiny;
            GUI.color = Color.grey;
            Widgets.Label(GetLabelRect(explanation, ref pos, labelWidthOverride: LeftPanelWidth), explanation);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
        }

        private void DrawSkillsPanel(ref Vector2 pos, Rect inRect)
        {
            if (personaData.skills != null)
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
                    personaData.skills = new List<SkillRecord>();
                    foreach (var skill in initialSkills)
                    {
                        personaData.skills.Add(new SkillRecord
                        {
                            def = skill.def,
                            levelInt = skill.levelInt,
                            xpSinceLastLevel = skill.xpSinceLastLevel,
                            xpSinceMidnight = skill.xpSinceMidnight,
                            passion = skill.passion,
                            pawn = skill.pawn,
                        });
                    }
                }

                var skillBoxHeight = personaData.skills.Count() * (Text.LineHeight + 5) + (this.Margin * 2) - 5;
                Rect traitsFill = new Rect(pos.x, pos.y, inRect.width - pos.x - Margin, skillBoxHeight);
                Rect skillsContainer = new Rect(traitsFill.x, traitsFill.y, traitsFill.width, skillBoxHeight);
                Widgets.DrawRectFast(traitsFill, Widgets.MenuSectionBGFillColor, null);


                Widgets.BeginScrollView(traitsFill, ref scrollPos, skillsContainer);
                skillsContainer.x += this.Margin;
                skillsContainer.y += this.Margin;

                GenUI.DrawElementStackVertical(skillsContainer, Text.LineHeight, personaData.skills, delegate(Rect rect, SkillRecord skill)
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
                                var minLevel = MinLevelOfSkill(personaData.GetDummyPawn, skill.def);
                                if (skill.Level > minLevel)
                                {
                                    skill.Level -= 1;
                                }
                            }

                            Rect skillBar = new Rect(decrementSkillRect.xMax + 5, decrementSkillRect.y, 100, rect.height);
                            float fillPercent = Mathf.Max(0.01f, skill.Level / 20f);
                            Texture2D fillTex = SkillUI.SkillBarFillTex;

                            var initialSkill = initialSkills.First(x => x.def == skill.def);

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
            }
            Widgets.EndScrollView();
        }

        private static int MinLevelOfSkill(Pawn pawn, SkillDef sk)
        {
            float num = 0;
            foreach (BackstoryDef item in pawn.story.AllBackstories.Where((BackstoryDef bs) => bs != null))
            {
                foreach (KeyValuePair<SkillDef, int> skillGain in item.skillGains)
                {
                    if (skillGain.Key == sk)
                    {
                        num += (float)skillGain.Value * Rand.Range(1f, 1.4f);
                    }
                }
            }
            for (int i = 0; i < pawn.story.traits.allTraits.Count; i++)
            {
                int value = 0;
                if (!pawn.story.traits.allTraits[i].Suppressed && pawn.story.traits.allTraits[i].CurrentData.skillGains.TryGetValue(sk, out value))
                {
                    num += (float)value;
                }
            }
            return Mathf.Clamp(Mathf.RoundToInt(num), 0, 20);
        }

        public static void FillableBar(Rect rect, float fillPercent, Texture2D fillTex)
        {
            rect.width *= fillPercent;
            GUI.DrawTexture(rect, fillTex);
        }


        private void DrawTimePanel(ref Rect inRect)
        {
            
            // Widgets.DrawBoxSolidWithOutline(inRect, Color.black, Color.blue, 1);
            
            Text.Font = GameFont.Medium;
            Rect editTime = new Rect(inRect.x + this.Margin, inRect.y + this.Margin, inRect.width / 2f - this.Margin, inRect.height);
            
            Widgets.Label(editTime, "Total time to edit:");
            editTime.y += Text.LineHeight;
            
            Widgets.Label(editTime, "Total stack degeneration:");
        }

        private Color col =  new Color(19f / 255f, 22f / 255f, 22f / 255f);
        private void DrawAcceptCancelButtons(ref Rect inRect)
        {
            //throw new System.NotImplementedException();
        }

        private void DrawTutorialPanel(ref Rect inRect)
        {
            //throw new System.NotImplementedException();
        }
    }
}