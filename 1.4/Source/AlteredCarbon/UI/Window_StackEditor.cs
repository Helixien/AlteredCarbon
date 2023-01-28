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
        private Building_DecryptionBench decryptionBench;
        private CorticalStack corticalStack;
        private PersonaData personaData;
        private int backstoryChildIndex;
        private int backstoryAdultIndex;
        private int factionIndex;
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
        }


        private Vector2 scrollPos;
        public override void DoWindowContents(Rect inRect)
        {
            Vector2 pos = new Vector2(inRect.x + Margin, inRect.y);
            DrawTitle(ref pos, inRect);
            DrawBackstoryPanel(ref pos);
            DrawTraitsPanel(ref pos);
            DrawFactionPanel(ref pos);

            //DrawSkillsPanel(ref inRect);
            ////TODO: ideo panel
            //DrawIdeoPanel(ref inRect);
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
            Text.Font = GameFont.Small;
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
            Rect traitsContainer = new Rect(pos.x, pos.y, LeftPanelWidth, 600);
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
            pos.y += Mathf.Max(200, rect.height + 15);
        }

        private void DrawFactionPanel(ref Vector2 pos)
        {
            pos.y += 15;
            DrawSectionTitle(ref pos, "AC.FactionAllegiance".Translate());
            Text.Font = GameFont.Small;
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
        }

        private void DrawAcceptCancelButtons(ref Rect inRect)
        {
            //throw new System.NotImplementedException();
        }

        private void DrawTutorialPanel(ref Rect inRect)
        {
            //throw new System.NotImplementedException();
        }

        private void DrawSkillsPanel(ref Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Rect backstoryHeader = new Rect(inRect.x+inRect.width / 2f + this.Margin*3f, inRect.y, inRect.width / 2f - this.Margin * 4f, inRect.height);
            Widgets.Label(backstoryHeader, "Skills");


            GUI.BeginGroup(backstoryHeader);
            Rect traitsFill = new Rect(0f, Text.LineHeight, backstoryHeader.width, inRect.height * 0.55f);
            Widgets.DrawRectFast(traitsFill, Widgets.MenuSectionBGFillColor, null);
            Text.Font = GameFont.Small;

            if (personaData.skills != null)
            {
                Rect skillsContainer = new Rect(traitsFill.x, traitsFill.y, traitsFill.width - this.Margin, personaData.skills.Count() * (Text.LineHeight + 5f));
                Widgets.BeginScrollView(traitsFill, ref scrollPos, skillsContainer);

                skillsContainer.x += this.Margin / 2;
                skillsContainer.y += 5f;

                GenUI.DrawElementStackVertical(skillsContainer, Text.LineHeight, personaData.skills, delegate(Rect rect, SkillRecord skill)
                    {
                        //Reimplmented from Rimworld.SkillsUI
                        Rect labelRect = new Rect(rect.x, rect.y, rect.width / 2.5f, rect.height);
                        Widgets.Label(labelRect, skill.def.skillLabel.CapitalizeFirst());

                        Rect position = new Rect(labelRect.xMax, labelRect.y, labelRect.height, labelRect.height);
                        //TODO: keep in mind vanilla skills expanded
                        if (Mouse.IsOver(position))
                        {
                            RenderRect(position);
                        }

                        if (Widgets.ButtonInvisible(position))
                        {
                            SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                            skill.passion = Enums.Next(skill.passion);
                        }

                        if (!skill.TotallyDisabled)
                        {
                            if (skill.passion > Passion.None)
                            {
                                Texture2D image = (skill.passion == Passion.Major) ? SkillUI.PassionMajorIcon : SkillUI.PassionMinorIcon;
                                GUI.DrawTexture(position, image);
                            }

                            Rect rect2 = new Rect(position.xMax, position.y, rect.width - position.width - 45f, rect.height);
                            float fillPercent = Mathf.Max(0.01f, skill.Level / 20f);
                            Texture2D fillTex = SkillUI.SkillBarFillTex;
                            Widgets.FillableBar(rect2, fillPercent, fillTex, null, false);
                        }


                        Rect rect3 = new Rect(position.xMax + 4f, position.y, 999f, rect.height);
                        rect3.yMin += 3f;
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
                        Widgets.Label(rect3, label);
                        GenUI.ResetLabelAlign();
                        GUI.color = Color.white;
                    }, element => skillsContainer.width
                );
            }

            Widgets.EndScrollView();
            GUI.EndGroup();
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

        private void DrawIdeoPanel(ref Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Rect ideoHeader = new Rect(inRect.x + this.Margin * 2f, inRect.y, inRect.width / 2f - this.Margin, inRect.height);
            Widgets.Label(ideoHeader, "Ideology");
            inRect.y += Text.LineHeight;
            

            Rect ideoBody = new Rect(inRect.x + this.Margin, inRect.y, inRect.width / 2f, inRect.height);
            GUI.BeginGroup(ideoBody);
            Rect ideoBodyFill = new Rect(0f, 0f, ideoBody.width, (Text.LineHeight * 1.5f) + (this.Margin));
            Widgets.DrawRectFast(ideoBodyFill, Widgets.MenuSectionBGFillColor, null);

            GUI.EndGroup();
            inRect.y += ideoBodyFill.height + this.Margin;


            // Widgets.DrawBoxSolidWithOutline(inRect, Color.black, Color.blue, 1);
        }

        private Color col =  new Color(19f / 255f, 22f / 255f, 22f / 255f);
        private void RenderRect(Rect debugRect)
        {
            
            Widgets.DrawRectFast(debugRect,col, null);
        }
    }
}


public static class Enums
{
    public static T Next<T>(this T v) where T : struct
    {
        return Enum.GetValues(v.GetType()).Cast<T>().Concat(new[] { default(T) }).SkipWhile(e => !v.Equals(e)).Skip(1).First();
    }

    public static T Previous<T>(this T v) where T : struct
    {
        return Enum.GetValues(v.GetType()).Cast<T>().Concat(new[] { default(T) }).Reverse().SkipWhile(e => !v.Equals(e)).Skip(1).First();
    }
}