using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using static AlteredCarbon.UIHelper;

namespace AlteredCarbon
{
    public static class GeneUtils
    {
        public static Gene ApplyGene(GeneDef geneDef, Pawn pawn, bool xenogene)
        {
            var gene = pawn.genes.GetGene(geneDef);
            if (gene is null)
            {
                gene = pawn.genes.AddGene(geneDef, xenogene);
            }
            if (gene != null)
            {
                ApplyGene(gene, pawn);
                return gene;
            }
            return null;
        }

        public static void ApplyGene(Gene gene, Pawn pawn)
        {
            OverrideAllConflicting(gene, pawn);
            if (gene.def.skinIsHairColor)
            {
                pawn.story.skinColorOverride = pawn.story.HairColor;
            }
            if (gene.def.hairColorOverride.HasValue)
            {
                Color value = gene.def.hairColorOverride.Value;
                if (gene.def.randomBrightnessFactor != 0f)
                {
                    value *= 1f + Rand.Range(0f - gene.def.randomBrightnessFactor, gene.def.randomBrightnessFactor);
                }
                pawn.story.HairColor = value.ClampToValueRange(GeneTuning.HairColorValueRange);
            }
            if (gene.def.skinColorBase.HasValue)
            {
                if (gene.def.skinColorBase.HasValue)
                {
                    pawn.story.SkinColorBase = gene.def.skinColorBase.Value;
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
                        pawn.story.skinColorOverride = value2.ClampToValueRange(GeneTuning.SkinColorValueRange);
                    }
                }
                if (gene.def.bodyType.HasValue && !pawn.DevelopmentalStage.Juvenile())
                {
                    if (gene.def.bodyType.HasValue)
                    {
                        pawn.story.bodyType = gene.def.bodyType.Value.ToBodyType(pawn);
                    }
                }
                if (!gene.def.forcedHeadTypes.NullOrEmpty())
                {
                    if (!gene.def.forcedHeadTypes.NullOrEmpty())
                    {
                        pawn.story.TryGetRandomHeadFromSet(gene.def.forcedHeadTypes);
                    }
                }
                if ((gene.def.forcedHair != null || gene.def.hairTagFilter != null)
                    && !PawnStyleItemChooser.WantsToUseStyle(pawn, pawn.story.hairDef))
                {
                    pawn.story.hairDef = PawnStyleItemChooser.RandomHairFor(pawn);
                }
                if (gene.def.beardTagFilter != null && pawn.style != null
                    && !PawnStyleItemChooser.WantsToUseStyle(pawn, pawn.style.beardDef))
                {
                    pawn.style.beardDef = PawnStyleItemChooser.RandomBeardFor(pawn);
                }
                if (gene.def.fur != null)
                {
                    pawn.story.furDef = gene.def.fur;
                }


                if (gene.def.soundCall != null)
                {
                    PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn);
                }
                pawn.needs?.AddOrRemoveNeedsAsAppropriate();
                pawn.health.hediffSet.DirtyCache();
                pawn.skills?.DirtyAptitudes();
                pawn.Notify_DisabledWorkTypesChanged();
            }
        }

        public static void OverrideAllConflicting(Gene gene, Pawn pawn)
        {
            gene.OverrideBy(null);
            foreach (Gene item in pawn.genes.GenesListForReading)
            {
                if (item != gene && item.def.ConflictsWith(gene.def))
                {
                    item.OverrideBy(gene);
                }
            }
        }
        public static void DrawGenes(ref Vector2 pos, TaggedString label, Rect pawnBox,
            float width, ref Rect geneBox, List<Gene> genes, ref Rect rect)
        {
            if (genes.Any())
            {
                genes.SortGenes();
                pos.x -= 15;
                ListSeparator(ref pos, width, label);
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
    }
}
