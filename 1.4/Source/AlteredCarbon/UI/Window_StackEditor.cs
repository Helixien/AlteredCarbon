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
                .FindIndex(x => x.defName==this.corticalStack.PersonaData.childhood);
        }

        public override Vector2 InitialSize
        {
            get { return new Vector2(768f, 690f); }
        }

        private int backstoryChildIndex;
        private int backstoryAdult = 0;
        private List<TraitDef> traitsList;
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

            Rect backstoryHeader = new Rect(inRect.x + this.Margin * 2f, inRect.y, inRect.width / 2f - this.Margin, inRect.height);
            Widgets.Label(backstoryHeader, "Backstory");
            inRect.y += Text.LineHeight;

            Rect backstoryHighlightRect = new Rect(inRect.x + this.Margin, inRect.y, inRect.width / 2f, inRect.height);
            DrawBackstoryPanel(backstoryHighlightRect);
            
            //TODO: traits panel
            //TODO: ideo panel
            //TODO: faction panel
            //TODO: editing time panel
            //TODO: skills panel
            //TODO: shadow tutorial panel
            //TODO: accept/cancel buttons
        }
        protected void DrawBackstoryPanel(Rect rect)
        {
            GUI.BeginGroup(rect);
            Rect rect2 = new Rect(0f, 0f, rect.width, (Text.LineHeight * 2.5f) + (this.Margin));
            Widgets.DrawRectFast(rect2, Widgets.MenuSectionBGFillColor, null);

            rect2.y += this.Margin;

            Text.Font = GameFont.Small;
            Rect rect3 = new Rect(rect2.x + (this.Margin / 2), rect2.y, rect2.width, rect2.height);
            Widgets.Label(rect3, "Childhood");

            Rect lftButton =  new Rect(rect2.x + 100f, rect2.y+2, Text.LineHeight/2, Text.LineHeight-4f);
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
            
            Rect rghtButton =  new Rect(rect2.width - (this.Margin * 4f), rect2.y+2, Text.LineHeight/2, Text.LineHeight-4f);
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
            Widgets.DrawRectFast(background, new Color(24f/255f, 20f/255f, 20f/255f), null);

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(background, DefDatabase<BackstoryDef>.AllDefsListForReading.Where(x => x.slot == BackstorySlot.Childhood).ToList()[backstoryChildIndex].title);
            Text.Anchor = TextAnchor.UpperLeft;
            
            rect2.y += Text.LineHeight * 1.5f;

            Rect rect4 = new Rect(rect2.x + (this.Margin / 2), rect2.y, rect2.width, rect2.height);
            Widgets.Label(rect4, "Adulthood");

            Rect lftButtonAdult = new Rect(rect2.x + 100f, rect2.y+2, Text.LineHeight/2, Text.LineHeight-4f);
            GUI.DrawTexture(lftButtonAdult, TexUI.ArrowTexLeft);
            Rect rghtButtonAdult = new Rect(rect2.width - (this.Margin * 4f), rect2.y+2, Text.LineHeight/2, Text.LineHeight-4f);
            GUI.DrawTexture(rghtButtonAdult, TexUI.ArrowTexRight);

            Rect backgroundAdult = new Rect(
                lftButtonAdult.x + lftButtonAdult.width + 2f,
                rect4.y,
                rghtButtonAdult.x - lftButtonAdult.xMax - 4f,
                Text.LineHeight
            );
            Widgets.DrawRectFast(backgroundAdult, new Color(24f/255f, 20f/255f, 20f/255f), null);

            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(backgroundAdult, "Adult Background");
            
            Text.Anchor = TextAnchor.UpperLeft;
            GUI.EndGroup();
        }
    }
}