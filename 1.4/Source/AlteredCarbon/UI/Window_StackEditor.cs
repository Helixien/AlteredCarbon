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

            this.traitsList = corticalStack.PersonaData.traits;
        }

        public override Vector2 InitialSize
        {
            get { return new Vector2(768f, 690f); }
        }

        private int backstoryChildIndex;
        private int backstoryAdult = 0;
        private List<Trait> traitsList;
        private Ideo ideo;
        private Faction faction = Faction.OfPlayer;
        private List<SkillRecord> skills;

        // public Vector2 Margin = new Vector2(10f, 3f);
        // public float custMargin = 20f;
        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Rect rect3 = new Rect(inRect.x, inRect.y, inRect.width, Text.LineHeight);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect3, "ACE.EditStack".Translate());

            Text.Anchor = TextAnchor.UpperLeft;
            inRect.y += Text.LineHeight;
            Text.Font = GameFont.Small;


            DrawBackstoryPanel(ref inRect);
            DrawTraitsPanel(ref inRect);
            //TODO: ideo panel
            DrawIdeoPanel(ref inRect);
            //TODO: faction panel
            DrawFactionPanel(ref inRect);
            //TODO: editing time panel
            DrawTimePanel(ref inRect);
            //TODO: skills panel
            DrawSkillsPanel(ref inRect);
            //TODO: shadow tutorial panel
            DrawTutorialPanel(ref inRect);
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
            //throw new System.NotImplementedException();
        }

        private void DrawTimePanel(ref Rect inRect)
        {
            //throw new System.NotImplementedException();
        }

        private void DrawFactionPanel(ref Rect inRect)
        {
            //throw new System.NotImplementedException();
        }

        private void DrawIdeoPanel(ref Rect inRect)
        {
            //throw new System.NotImplementedException();
        }

        private void DrawTraitsPanel(ref Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Rect backstoryHeader = new Rect(inRect.x + this.Margin * 2f, inRect.y, inRect.width / 2f - this.Margin, inRect.height);
            Widgets.Label(backstoryHeader, "Traits");

            Rect addTraitRect = new Rect(inRect.x + (backstoryHeader.width), backstoryHeader.y + (Text.LineHeight / 2 - 13f), 26f, 26f);
            //TODO: add Traits float menu
            GUI.DrawTexture(addTraitRect, TexButton.Add);

            Rect backstoryHighlightRect = new Rect(inRect.x + this.Margin, inRect.y + Text.LineHeight, inRect.width / 2f, inRect.height/ 4f);
            Widgets.DrawRectFast(backstoryHighlightRect, Widgets.MenuSectionBGFillColor, null);

            GUI.BeginGroup(backstoryHighlightRect);
            Rect traitsContainer = new Rect(0f, 0f, backstoryHighlightRect.width - this.Margin, backstoryHighlightRect.height);

            traitsContainer.y += this.Margin / 4f;
            traitsContainer.x += this.Margin / 4f;
            Text.Font = GameFont.Small;

            GUI.BeginGroup(traitsContainer);
            if (traitsList != null)
            {
                GenUI.DrawElementStack(traitsContainer, Text.LineHeight, traitsList, delegate(Rect rect, Trait element)
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
                    
                    //TODO: make button work
                    GUI.DrawTexture(buttonRect,TexButton.Minus);
                },  trait => Text.CalcSize(trait.LabelCap).x + Text.LineHeight + 10f);
            }

            GUI.EndGroup();
            GUI.EndGroup();
        }

        protected void DrawBackstoryPanel(ref Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Rect backstoryHeader = new Rect(inRect.x + this.Margin * 2f, inRect.y, inRect.width / 2f - this.Margin, inRect.height);
            Widgets.Label(backstoryHeader, "Backstory");

            Rect backstoryHighlightRect = new Rect(inRect.x + this.Margin, inRect.y + Text.LineHeight, inRect.width / 2f, inRect.height);
            GUI.BeginGroup(backstoryHighlightRect);
            Rect rect2 = new Rect(0f, 0f, backstoryHighlightRect.width, (Text.LineHeight * 2.5f) + (this.Margin));
            Widgets.DrawRectFast(rect2, Widgets.MenuSectionBGFillColor, null);

            rect2.y += this.Margin;

            Text.Font = GameFont.Small;
            Rect rect3 = new Rect(rect2.x + (this.Margin / 2), rect2.y, rect2.width, rect2.height);
            Widgets.Label(rect3, "Childhood");

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
                rect3.y,
                rghtButton.x - lftButton.xMax - 4f,
                Text.LineHeight
            );
            Widgets.DrawRectFast(background, new Color(24f / 255f, 20f / 255f, 20f / 255f), null);

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(background, DefDatabase<BackstoryDef>.AllDefsListForReading.Where(x => x.slot == BackstorySlot.Childhood).ToList()[backstoryChildIndex].title);
            Text.Anchor = TextAnchor.UpperLeft;

            rect2.y += Text.LineHeight * 1.5f;

            Rect rect4 = new Rect(rect2.x + (this.Margin / 2), rect2.y, rect2.width, rect2.height);
            Widgets.Label(rect4, "Adulthood");

            Rect lftButtonAdult = new Rect(rect2.x + 100f, rect2.y + 2, Text.LineHeight / 2, Text.LineHeight - 4f);
            GUI.DrawTexture(lftButtonAdult, TexUI.ArrowTexLeft);
            Rect rghtButtonAdult = new Rect(rect2.width - (this.Margin * 4f), rect2.y + 2, Text.LineHeight / 2, Text.LineHeight - 4f);
            GUI.DrawTexture(rghtButtonAdult, TexUI.ArrowTexRight);

            Rect backgroundAdult = new Rect(
                lftButtonAdult.x + lftButtonAdult.width + 2f,
                rect4.y,
                rghtButtonAdult.x - lftButtonAdult.xMax - 4f,
                Text.LineHeight
            );
            Widgets.DrawRectFast(backgroundAdult, new Color(24f / 255f, 20f / 255f, 20f / 255f), null);

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(backgroundAdult, "Adult Background");
            Text.Anchor = TextAnchor.UpperLeft;

            inRect.y += rect2.yMax;
            GUI.EndGroup();
        }
    }
}