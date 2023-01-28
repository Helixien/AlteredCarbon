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
        private Building_DecryptionBench decryptionBench;
        private CorticalStack corticalStack;
        private PersonaData personaData;
        private int backstoryChildIndex;
        private int backstoryAdultIndex;
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
            DrawBackstoryPanel(ref pos, inRect);
            //DrawSkillsPanel(ref inRect);
            //DrawTraitsPanel(ref inRect);
            ////TODO: ideo panel
            //DrawIdeoPanel(ref inRect);
            ////TODO: faction panel
            //DrawFactionPanel(ref inRect);
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
        protected void DrawBackstoryPanel(ref Vector2 pos, Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            var title = "AC.Backstories".Translate();
            Rect backstoryHeader = GetLabelRect(title, ref pos, labelWidthOverride: LeftPanelWidth);
            Widgets.Label(backstoryHeader, title);
            Widgets.DrawLine(new Vector2(backstoryHeader.x, backstoryHeader.yMax + 5f), new Vector2(backstoryHeader.xMax, backstoryHeader.yMax + 5),
                GUI.color * new Color(1f, 1f, 1f, 0.4f), 1f);
            Text.Anchor = TextAnchor.UpperLeft;
            pos.y += 15f;
            Text.Font = GameFont.Small;
            var allChildhoodBackstories = DefDatabase<BackstoryDef>.AllDefsListForReading.Where(x => x.slot == BackstorySlot.Childhood).ToList();
            var filters = new List<Func<BackstoryDef, (string, bool)>>();
            (string, bool) NoFilters(BackstoryDef backstoryDef)
            {
                return ("AC.NoFilters".Translate(), true);
            }
            filters.Add(NoFilters);
            (string, bool) NoDisabledWorkTypes(BackstoryDef backstoryDef)
            {
                return ("AC.NoDisabledWorkTypes".Translate(), backstoryDef.workDisables == WorkTags.None);
            }
            filters.Add(NoDisabledWorkTypes);
            (string, bool) NoSkillPenalties(BackstoryDef backstoryDef)
            {
                return ("AC.NoSkillPenalties".Translate(), backstoryDef.skillGains is null || backstoryDef.skillGains.Any(x => x.Value < 0) is false);
            }
            filters.Add(NoSkillPenalties);

            DoSelectionButtons(ref pos, "Childhood".Translate(), ref backstoryChildIndex,
                (BackstoryDef x) => x.TitleCapFor(personaData.gender), allChildhoodBackstories, delegate (BackstoryDef x)
                {
                    SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                    personaData.childhood = x;
                    this.backstoryChildIndex = allChildhoodBackstories
                        .FindIndex(x => x == personaData.childhood);
                }, buttonOffsetFromTextOverride: 5f, labelWidthOverride: 80f, filter: null, includeInfoCard: false,
                tooltipGetter: (BackstoryDef x) => x.FullDescriptionFor(personaData.GetDummyPawn),
                filters: filters);
            if (personaData.adulthood != null)
            {
                var allAdultBackstories = DefDatabase<BackstoryDef>.AllDefsListForReading.Where(x => x.slot == BackstorySlot.Adulthood).ToList();
                DoSelectionButtons(ref pos, "Adulthood".Translate(), ref backstoryAdultIndex,
                (BackstoryDef x) => x.TitleCapFor(personaData.gender), allAdultBackstories, delegate (BackstoryDef x)
                {
                    SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                    personaData.adulthood = x;
                    this.backstoryAdultIndex = allChildhoodBackstories
                        .FindIndex(x => x == personaData.adulthood);
                }, buttonOffsetFromTextOverride: 5f, labelWidthOverride: 80f, filter: null, includeInfoCard: false,
                tooltipGetter: (BackstoryDef x) => x.FullDescriptionFor(personaData.GetDummyPawn),
                filters: filters);
            }
            else
            {
                pos.y += buttonHeight + 5;
            }
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

        private void DrawFactionPanel(ref Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Rect factionHeader = new Rect(inRect.x + this.Margin * 2f, inRect.y, inRect.width / 2f - this.Margin, inRect.height);
            Widgets.Label(factionHeader, "Faction");
            
            

            Rect backstoryHighlightRect = new Rect(inRect.x + this.Margin, inRect.y + Text.LineHeight, inRect.width / 2f, inRect.height);
            GUI.BeginGroup(backstoryHighlightRect);
            Rect rect2 = new Rect(0f, 0f, backstoryHighlightRect.width, (Text.LineHeight * 1.5f) + (this.Margin));
            Widgets.DrawRectFast(rect2, Widgets.MenuSectionBGFillColor, null);
            
            GUI.EndGroup();
            
            inRect.y += rect2.yMax + Text.lineHeights[2];
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

        private void DrawTraitsPanel(ref Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Rect traitHeader = new Rect(inRect.x + this.Margin * 2f, inRect.y, inRect.width / 2f - this.Margin, inRect.height);
            Widgets.Label(traitHeader, "Traits");
            inRect.y += Text.LineHeight;

            Rect addTraitRect = new Rect(inRect.x + (traitHeader.width), traitHeader.y + (Text.LineHeight / 2 - 13f), 26f, 26f);
            //TODO: add Traits float menu
            GUI.DrawTexture(addTraitRect, TexButton.Add);

            Rect traitsFill = new Rect(inRect.x + this.Margin, inRect.y, inRect.width / 2f, inRect.height/ 5f);
            Widgets.DrawRectFast(traitsFill, Widgets.MenuSectionBGFillColor, null);

            GUI.BeginGroup(traitsFill);
            Rect traitsContainer = new Rect(0f, 0f, traitsFill.width - this.Margin, traitsFill.height);

            traitsContainer.y += this.Margin / 4f;
            traitsContainer.x += this.Margin / 4f;
            Text.Font = GameFont.Small;

            GUI.BeginGroup(traitsContainer);

            GenUI.DrawElementStack(traitsContainer, Text.LineHeight, personaData.traits, delegate (Rect rect, Trait element)
            {
                //TODO: hover event for trait desc
                Widgets.DrawRectFast(rect, Color.black, null);
                rect.x += 5f;
                Widgets.Label(rect, element.LabelCap);
                var buttonRect = new Rect(
                    rect.x + Text.CalcSize(element.LabelCap).x + Text.LineHeight * 0.25f,
                    rect.y + rect.height / 2 - Text.LineHeight * 0.3f,
                    Text.LineHeight * 0.7f,
                    Text.LineHeight * 0.7f
                );

                GUI.DrawTexture(buttonRect, TexButton.Minus);
                if (Widgets.ButtonInvisible(buttonRect, true))
                {
                    SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                    personaData.traits.Remove(element);
                }
            }, trait => Text.CalcSize(trait.LabelCap).x + Text.LineHeight + 10f);

            GUI.EndGroup();
            GUI.EndGroup();
            inRect.y += traitsFill.height + this.Margin;
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