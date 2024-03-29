using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using VanillaGenesExpanded;
using Verse;
using Verse.Sound;

namespace AlteredCarbon
{
    [HotSwappable]
    public class Window_SeparateGene : Window
    {
        public Building_GeneCentrifuge centrifuge;
        public Genepack genepack;
        private static Vector2 scrollPosition;
        public GeneDef chosenGene;
        public List<List<GeneDef>> GeneChunks => genepack.GeneSet.GenesListForReading.ChunkBy(3);
        public override Vector2 InitialSize => new Vector2(600, GeneChunks.Count == 1 ? 500 : 675);
        public Window_SeparateGene(Building_GeneCentrifuge centrifuge, Genepack genepack)
        {
            this.centrifuge = centrifuge;
            this.genepack = genepack;
        }
        public override void DoWindowContents(Rect inRect)
        {
            var separatingGenesTitleRect = new Rect(inRect.x, inRect.y, inRect.width, 32);
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(separatingGenesTitleRect, "AC.SeparatingGenes".Translate());
            Text.Font = GameFont.Small;

            var selectGeneRect = new Rect(inRect.x, separatingGenesTitleRect.yMax + 24, inRect.width, 24);
            Widgets.Label(selectGeneRect, "AC.SelectGene".Translate());

            var selectGeneExplanationRect = new Rect(selectGeneRect.x, selectGeneRect.yMax, selectGeneRect.width, 24);
            GUI.color = Color.grey;
            Text.Font = GameFont.Tiny;
            Widgets.Label(selectGeneExplanationRect, "AC.SelectGeneExplanation".Translate());
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.DrawLine(new Vector2(inRect.x + 15, selectGeneExplanationRect.yMax + 5), 
                new Vector2(inRect.width - 15, selectGeneExplanationRect.yMax + 5), GUI.color * new Color(1f, 1f, 1f, 0.4f), 1f);
            float geneBoxWidth = 150;
            float geneBoxHeight = 150;
            var geneChunks = GeneChunks;
            float curY = selectGeneExplanationRect.yMax + 50;
            var totalRect = new Rect(15f, curY, inRect.width - 30, geneChunks.Count > 1 ? (geneBoxHeight * 2f) + 30 : geneBoxHeight + 15);
            var viewRect = new Rect(totalRect.x, totalRect.y, totalRect.width - 16, geneChunks.Count * (geneBoxHeight + 15));
            Widgets.BeginScrollView(totalRect, ref scrollPosition, viewRect);
            for (var i = 0; i < geneChunks.Count; i++)
            {
                var geneList = geneChunks[i];
                var xPos = geneList.Count == 3 ? 45f : geneList.Count == 2 ? 45f + (geneBoxWidth / 2f) : 45f + geneBoxWidth + 15;
                for (var j = 0; j < geneList.Count; j++)
                {
                    var gene = geneList[j];
                    var geneRect = new Rect(xPos, curY, geneBoxHeight, geneBoxHeight);
                    DrawGene(gene, geneRect, GeneType.Xenogene);
                    if (chosenGene == gene)
                    {
                        Widgets.DrawHighlightSelected(geneRect);
                    }
                    xPos += geneBoxWidth + 15;
                }
                curY += geneBoxHeight + 15;
            }
            Widgets.EndScrollView();

            Widgets.DrawLine(new Vector2(inRect.x + 15, totalRect.yMax + 30),
                new Vector2(inRect.width - 15, totalRect.yMax + 30), GUI.color * new Color(1f, 1f, 1f, 0.4f), 1f);

            var geneSeparationExplanationRect = new Rect(inRect.x + 15, totalRect.yMax + 45, inRect.width - 30, 50);
            GUI.color = Color.grey;
            Text.Font = GameFont.Tiny;
            Widgets.Label(geneSeparationExplanationRect, "AC.SeparateGeneExplanation".Translate());
            Text.Font = GameFont.Small;
            GUI.color = Color.white;

            var buttonWidth = 200;
            var cancelButtonRect = new Rect(inRect.x + 30, inRect.height - 32, buttonWidth, 32);
            if (Widgets.ButtonText(cancelButtonRect, "Cancel".Translate()))
            {
                this.Close();
            }
            var acceptButtonRect = new Rect(inRect.width - 30 - buttonWidth, cancelButtonRect.y, buttonWidth, 32);
            if (Widgets.ButtonText(acceptButtonRect, "AC.StartSeparating".Translate()))
            {
                if (chosenGene != null)
                {
                    this.centrifuge.genepackToStore = genepack;
                    this.centrifuge.geneToSeparate = chosenGene;
                    this.Close();
                }
                else
                {
                    Messages.Message("AC.SelectAGeneToSeparate".Translate(), MessageTypeDefOf.CautionInput);
                }
            }
        }

        public void DrawGene(GeneDef gene, Rect geneRect, GeneType geneType, bool doBackground = true)
        {
            DrawGeneBasics(gene, geneRect, geneType, doBackground);
            if (Mouse.IsOver(geneRect))
            {
                string text = gene.LabelCap.Colorize(ColoredText.TipSectionTitleColor) + "\n\n" + gene.DescriptionFull;
                TooltipHandler.TipRegion(geneRect, text);
                if (Widgets.ButtonInvisible(geneRect))
                {
                    SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                    chosenGene = gene;
                }
            }
        }

        private static void DrawGeneBasics(GeneDef gene, Rect geneRect, GeneType geneType, bool doBackground)
        {
            GUI.BeginGroup(geneRect);
            Rect rect = geneRect.AtZero();
            if (doBackground)
            {
                Widgets.DrawHighlight(rect);
                GUI.color = new Color(1f, 1f, 1f, 0.05f);
                Widgets.DrawBox(rect);
                GUI.color = Color.white;
            }
            float num = rect.width - Text.LineHeight;
            Rect rect2 = new Rect(geneRect.width / 2f - num / 2f, 0f, num, num).ContractedBy(15);
            Color iconColor = gene.IconColor;
            CachedTexture cachedTexture = GeneUIUtility.GeneBackground_Archite;
            if (gene.biostatArc == 0)
            {
                switch (geneType)
                {
                    case GeneType.Endogene:
                        cachedTexture = (CachedTexture)VanillaGenesExpanded_GeneUIUtility_DrawGeneBasics_Patch.ChooseEndogeneBackground(gene);
                        break;
                    case GeneType.Xenogene:
                        cachedTexture = (CachedTexture)VanillaGenesExpanded_GeneUIUtility_DrawGeneBasics_Patch.ChooseXenogeneBackground(gene);
                        break;
                }
            }
            GUI.DrawTexture(rect2, cachedTexture.Texture);
            Widgets.DefIcon(rect2, gene, null, 0.9f, null, drawPlaceholder: false, iconColor);
            Text.Font = GameFont.Tiny;
            float num2 = Text.CalcHeight(gene.LabelCap, rect.width);
            Rect rect3 = new Rect(0f, rect.yMax - num2, rect.width, num2);
            GUI.DrawTexture(new Rect(rect3.x, rect3.yMax - num2, rect3.width, num2), TexUI.GrayTextBG);
            Text.Anchor = TextAnchor.LowerCenter;
            if (doBackground && num2 < (Text.LineHeight - 2f) * 2f)
            {
                rect3.y -= 3f;
            }
            Widgets.Label(rect3, gene.LabelCap);
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;
            GUI.EndGroup();
        }
    }
}