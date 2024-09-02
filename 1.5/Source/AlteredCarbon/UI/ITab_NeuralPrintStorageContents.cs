using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AlteredCarbon
{
    [HotSwappable]
    public class ITab_NeuralStackStorageContents : ITab
    {
        private static readonly Vector2 WinSize = new Vector2(432f, 480f);
        private Vector2 scrollPosition;
        public Building_NeuralMatrix Building_NeuralMatrix => SelThing as Building_NeuralMatrix;
        public ITab_NeuralStackStorageContents()
        {
            size = WinSize;
            labelKey = "AC.NeuralStackStorage";
        }

        public override void FillTab()
        {
            Text.Font = GameFont.Small;
            Rect viewRect = new Rect(5f, 20f, size.x, size.y - 20f).ContractedBy(10f);
            GUI.BeginGroup(viewRect);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            float labelWidth = viewRect.width - 15f;
            float num = 0;
            DoAllowOption(ref num, labelWidth, "AC.AllowColonistNeuralStacks", ref Building_NeuralMatrix.allowColonistNeuralStacks);
            DoAllowOption(ref num, labelWidth, "AC.AllowStrangerNeuralStacks", ref Building_NeuralMatrix.allowStrangerNeuralStacks);
            DoAllowOption(ref num, labelWidth, "AC.AllowHostileNeuralStacks", ref Building_NeuralMatrix.allowHostileNeuralStacks);

            var storedStacks = Building_NeuralMatrix.StoredNeuralStacks.ToList();
            Widgets.ListSeparator(ref num, viewRect.width - 15, "AC.NeuralStacksStored".Translate(storedStacks.Count(), Building_NeuralMatrix.MaxActiveStackCapacity));
            Rect scrollRect = new Rect(0, num, viewRect.width - 16, viewRect.height);
            Rect outerRect = scrollRect;
            outerRect.width += 16;
            outerRect.height -= 120;
            scrollRect.height = storedStacks.Count() * 28f;
            Widgets.BeginScrollView(outerRect, ref scrollPosition, scrollRect);
            foreach (var stack in storedStacks)
            {
                DrawThingRow(ref num, scrollRect.width, stack);
            }
            Widgets.EndScrollView();
            GUI.EndGroup();
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void DoAllowOption(ref float num, float labelWidth, string optionKey, ref bool option)
        {
            Rect labelRect = new Rect(0f, num, labelWidth, 24);
            Widgets.DrawHighlightIfMouseover(labelRect);
            Text.Anchor = TextAnchor.MiddleLeft;
            labelRect.yMax += 5f;
            labelRect.yMin -= 5f;
            Widgets.CheckboxLabeled(labelRect, optionKey.Translate().Truncate(labelRect.width), ref option);
            Text.Anchor = TextAnchor.UpperLeft;
            num += 24f;
        }
        private void DrawThingRow(ref float y, float width, NeuralStack stack)
        {
            Rect rect1 = new Rect(0.0f, y, width, 28f);
            Widgets.InfoCardButton(0, y, stack);
            Rect rect2 = new Rect(rect1.width - 24, y, 24f, 24f);
            TooltipHandler.TipRegion(rect2, "AC.EjectNeuralStackTooltip".Translate());
            if (Widgets.ButtonImage(rect2, ContentFinder<Texture2D>.Get("UI/Buttons/Drop", true)))
            {
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
                Find.WindowStack.Add(new Dialog_MessageBox("AC.EjectNeuralStackConfirmation".Translate(stack.def.label + " (" + stack.NeuralData.name.ToStringFull + ")"),
                     "Confirm".Translate(), delegate
                     {
                         Building_NeuralMatrix.innerContainer.TryDrop(stack, Building_NeuralMatrix.InteractionCell, Building_NeuralMatrix.Map, ThingPlaceMode.Near, 1, out Thing droppedThing);
                     }, "GoBack".Translate(), null));
            }
            Rect eraseNeuralStack = rect2;
            eraseNeuralStack.x -= 28;
            TooltipHandler.TipRegion(eraseNeuralStack, "AC.EraseNeuralStackTooltip".Translate());
            if (Widgets.ButtonImage(eraseNeuralStack, ContentFinder<Texture2D>.Get("UI/Icons/Erase", true)))
            {
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
                Find.WindowStack.Add(new Dialog_MessageBox("AC.EraseNeuralStackConfirmation".Translate(stack.def.label + " (" + stack.NeuralData.name.ToStringFull + ")"),
                     "Confirm".Translate(), delegate
                     {
                         Building_NeuralMatrix.innerContainer.Remove(stack);
                         stack.Destroy();
                     }, "GoBack".Translate(), null));
            }

            rect1.width -= 54f;
            Rect rect3 = rect1;
            rect3.xMin = rect3.xMax - 40f;
            rect1.width -= 15f;

            if (Mouse.IsOver(rect1))
            {
                GUI.color = ITab_Pawn_Gear.HighlightColor;
                GUI.DrawTexture(rect1, TexUI.HighlightTex);
            }
            Rect thingIconRect = new Rect(24, y, 28f, 28f);
            Widgets.ThingIcon(thingIconRect, stack, 1f);
            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = ITab_Pawn_Gear.ThingLabelColor;
            Rect pawnLabelRect = new Rect(thingIconRect.xMax + 5, y, rect1.width - 36f, rect1.height);
            TaggedString pawnLabel = stack.NeuralData.PawnNameColored.Truncate(pawnLabelRect.width);
            Widgets.Label(pawnLabelRect, pawnLabel);
            string str2 = stack.DescriptionDetailed;
            TooltipHandler.TipRegion(rect1, str2);
            y += 28f;
        }

        public static bool InfoCardButton(float x, float y, Thing thing)
        {
            if (Widgets.InfoCardButtonWorker(x, y))
            {
                Find.WindowStack.Add(new Dialog_InfoCardStack(thing));
                return true;
            }
            return false;
        }
    }
}
