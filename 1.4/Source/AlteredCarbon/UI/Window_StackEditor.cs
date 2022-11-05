using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AlteredCarbon
{
    //TODO: clean up code
    //TODO: add translations
    //TODO: move this to AC_E
    [StaticConstructorOnStartup]
    [HotSwappable]
    public class Window_StackEditor : Window
    {
        private Building_DecryptionBench decryptionBench;
        private CorticalStack corticalStack;

        public Window_StackEditor(Building_DecryptionBench decryptionBench, CorticalStack corticalStack)
        {
            this.decryptionBench = decryptionBench;
            this.corticalStack = corticalStack;
            this.backstoryChildIndex = DefDatabase<BackstoryDef>.AllDefsListForReading
                .Where(x => x.slot == BackstorySlot.Childhood).ToList()
                .FindIndex(x => x.defName == this.corticalStack.PersonaData.childhood);

            if (!corticalStack.PersonaData.adulthood.NullOrEmpty())
            {
                this.backstoryAdultIndex = DefDatabase<BackstoryDef>.AllDefsListForReading
                    .Where(x => x.slot == BackstorySlot.Adulthood).ToList()
                    .FindIndex(x => x.defName == this.corticalStack.PersonaData.adulthood);
            }
            
            this.traitsList = new List<Trait>(corticalStack.PersonaData.traits);
            this.ideo = corticalStack.PersonaData.ideo;
            this.faction = corticalStack.PersonaData.faction;
            this.skills = new List<SkillRecord>(corticalStack.PersonaData.skills);
        }

        public override Vector2 InitialSize
        {
            get { return new Vector2(768f, 768f); }
        }

        private int backstoryChildIndex;
        private int? backstoryAdultIndex = null;
        private List<Trait> traitsList;
        private Ideo ideo;
        private Faction faction;
        private List<SkillRecord> skills;

        // public Vector2 Margin = new Vector2(10f, 3f);
        // public float custMargin = 20f;
        
        private Vector2 scrollPos;
        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Rect rect3 = new Rect(inRect.x, inRect.y, inRect.width, Text.LineHeight);
            Text.Anchor = TextAnchor.MiddleCenter;
            // Widgets.Label(rect3, "ACE.EditStack".Translate());
            Widgets.Label(rect3, "Edit Cortical Stack");

            Text.Anchor = TextAnchor.UpperLeft;
            inRect.y += Text.LineHeight * 1.2f;
            Text.Font = GameFont.Small;
            // Widgets.DrawBoxSolidWithOutline(inRect, Color.black, Color.blue, 1);

            //TODO: skills panel
            DrawSkillsPanel(ref inRect);

            DrawBackstoryPanel(ref inRect);
            
            
            
            DrawTraitsPanel(ref inRect);
            //TODO: ideo panel
            DrawIdeoPanel(ref inRect);
            //TODO: faction panel
            DrawFactionPanel(ref inRect);
            
            
            //TODO: shadow tutorial panel
            DrawTutorialPanel(ref inRect);
            
            //TODO: editing time panel
            DrawTimePanel(ref inRect);
            
            
            
            //TODO: accept/cancel buttons
            DrawAcceptCancelButtons(ref inRect);
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

            Rect skillsContainer = new Rect(traitsFill.x, traitsFill.y, traitsFill.width - this.Margin, this.skills.Count() * (Text.LineHeight + 5f));
            Widgets.BeginScrollView(traitsFill, ref scrollPos, skillsContainer);

            skillsContainer.x += this.Margin/2;
            skillsContainer.y += 5f;

            GenUI.DrawElementStackVertical(skillsContainer, Text.LineHeight, this.skills, delegate(Rect rect, SkillRecord skill)
                {
                    //Reimplmented from Rimworld.SkillsUI
                    Rect labelRect = new Rect(rect.x, rect.y, rect.width / 2.5f, rect.height);
                    RenderRect(labelRect);
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

                    if (skill.passion > Passion.None)
                    {
                        Texture2D image = (skill.passion == Passion.Major) ? SkillUI.PassionMajorIcon : SkillUI.PassionMinorIcon;
                        GUI.DrawTexture(position, image);
                    }

                }, element => skillsContainer.width
            );
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
            if (this.traitsList != null)
            {
                var innerTraitsList = new List<Trait>(this.traitsList);
                GenUI.DrawElementStack(traitsContainer, Text.LineHeight, innerTraitsList, delegate(Rect rect, Trait element)
                {
                    //TODO: hover event for trait desc
                    Widgets.DrawRectFast(rect, Color.black,null);
                    rect.x += 5f;
                    Widgets.Label(rect, element.LabelCap);
                    var buttonRect = new Rect(
                        rect.x + Text.CalcSize(element.LabelCap).x + Text.LineHeight *0.25f, 
                        rect.y + rect.height/2 - Text.LineHeight * 0.3f,
                        Text.LineHeight * 0.7f, 
                        Text.LineHeight * 0.7f
                    );
                    
                    GUI.DrawTexture(buttonRect,TexButton.Minus);
                    if (Widgets.ButtonInvisible(buttonRect, true))
                    {
                        SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                        traitsList.Remove(element);
                    }
                },  trait => Text.CalcSize(trait.LabelCap).x + Text.LineHeight + 10f);
            }

            GUI.EndGroup();
            GUI.EndGroup();
            inRect.y += traitsFill.height + this.Margin;
        }

        protected void DrawBackstoryPanel(ref Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Rect backstoryHeader = new Rect(inRect.x + this.Margin * 2f, inRect.y, inRect.width / 2f - this.Margin, inRect.height);
            Widgets.Label(backstoryHeader, "Backstory");

            Rect backstoryHighlightRect = new Rect(inRect.x + this.Margin, inRect.y + Text.LineHeight, inRect.width / 2f, inRect.height);
            GUI.BeginGroup(backstoryHighlightRect);
            
            Rect rect2 = new Rect(0f, 0f, backstoryHighlightRect.width, Text.LineHeight * (1.25f + (this.backstoryAdultIndex != null ? 1f : -0.2f)) + (this.Margin));
            // Rect rect2 = new Rect(0f, 0f, backstoryHighlightRect.width, (Text.LineHeight * (2.25f) + (this.Margin)));
            Widgets.DrawRectFast(rect2, Widgets.MenuSectionBGFillColor, null);

            rect2.y += this.Margin * 0.75f;

            Text.Font = GameFont.Small;
            Rect childhoodLabel = new Rect(rect2.x + (this.Margin / 2), rect2.y, rect2.width, rect2.height);
            Widgets.Label(childhoodLabel, "Childhood");

            Rect lftButton = new Rect(rect2.x + 100f, rect2.y + 2, Text.LineHeight / 2, Text.LineHeight - 4f);
            if (Widgets.ButtonInvisible(lftButton, true))
            {
                SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                if (backstoryChildIndex == 0)
                {
                    backstoryChildIndex = DefDatabase<BackstoryDef>.AllDefsListForReading.Where(x => x.slot == BackstorySlot.Childhood).Count() - 1;
                }
                else backstoryChildIndex--;
            }
            GUI.DrawTexture(lftButton, TexUI.ArrowTexLeft);
        
            Rect rghtButton = new Rect(rect2.width - (this.Margin * 4f), rect2.y + 2, Text.LineHeight / 2, Text.LineHeight - 4f);
            //TODO: move this to generic, or refactor the method in Window_SleeveCustomization
            if (Widgets.ButtonInvisible(rghtButton, true))
            {
                SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                if (backstoryChildIndex == DefDatabase<BackstoryDef>.AllDefsListForReading.Where(x => x.slot == BackstorySlot.Childhood).Count() - 1)
                {
                    backstoryChildIndex = 0;
                }
                else backstoryChildIndex++;
            }
            GUI.DrawTexture(rghtButton, TexUI.ArrowTexRight);

            Rect background = new Rect(
                lftButton.x + lftButton.width + 2f,
                childhoodLabel.y,
                rghtButton.x - lftButton.xMax - 4f,
                Text.LineHeight
            );
            Widgets.DrawRectFast(background, new Color(19f / 255f, 22f / 255f, 22f / 255f), null);

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(background, DefDatabase<BackstoryDef>.AllDefsListForReading.Where(x => x.slot == BackstorySlot.Childhood).ToList()[backstoryChildIndex].title);
            Text.Anchor = TextAnchor.UpperLeft;

            rect2.y += Text.LineHeight * 1.5f;

            if (backstoryAdultIndex != null && backstoryAdultIndex is int backstoryAdultIndexInternal)
            {
                Rect rect4 = new Rect(rect2.x + (this.Margin / 2), rect2.y, rect2.width, rect2.height);
                Widgets.Label(rect4, "Adulthood");

                Rect lftButtonAdult = new Rect(rect2.x + 100f, rect2.y + 2, Text.LineHeight / 2, Text.LineHeight - 4f);
                if (Widgets.ButtonInvisible(lftButtonAdult, true))
                {
                    SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                    if (backstoryAdultIndexInternal == 0)
                    {
                        backstoryAdultIndex = DefDatabase<BackstoryDef>.AllDefsListForReading.Where(x => x.slot == BackstorySlot.Adulthood).Count() - 1;
                    }
                    else backstoryAdultIndex--;
                }
                GUI.DrawTexture(lftButtonAdult, TexUI.ArrowTexLeft);
                Rect rghtButtonAdult = new Rect(rect2.width - (this.Margin * 4f), rect2.y + 2, Text.LineHeight / 2, Text.LineHeight - 4f);
                if (Widgets.ButtonInvisible(rghtButtonAdult, true))
                {
                    SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                    if (backstoryAdultIndexInternal == DefDatabase<BackstoryDef>.AllDefsListForReading.Where(x => x.slot == BackstorySlot.Adulthood).Count() - 1)
                    {
                        backstoryAdultIndex = 0;
                    }
                    else backstoryAdultIndex++;
                }
                GUI.DrawTexture(rghtButtonAdult, TexUI.ArrowTexRight);

                Rect backgroundAdult = new Rect(
                    lftButtonAdult.x + lftButtonAdult.width + 2f,
                    rect4.y,
                    rghtButtonAdult.x - lftButtonAdult.xMax - 4f,
                    Text.LineHeight
                );
                Widgets.DrawRectFast(backgroundAdult, new Color(19f / 255f, 22f / 255f, 22f / 255f), null);

                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(backgroundAdult, DefDatabase<BackstoryDef>.AllDefsListForReading.Where(x => x.slot == BackstorySlot.Adulthood).ToList()[backstoryAdultIndexInternal].title);
                Text.Anchor = TextAnchor.UpperLeft;
            }

            inRect.y += rect2.yMax;
            GUI.EndGroup();
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