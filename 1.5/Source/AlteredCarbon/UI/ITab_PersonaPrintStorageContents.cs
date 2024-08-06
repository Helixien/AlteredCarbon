using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AlteredCarbon
{
    [HotSwappable]
    public class ITab_PersonaPrintStorageContents : ITab
    {
        private static readonly Vector2 WinSize = new Vector2(432f, 480f);
        private Vector2 scrollPosition;
        public Building_PersonaMatrix Building_PersonaMatrix => SelThing as Building_PersonaMatrix;
        public ITab_PersonaPrintStorageContents()
        {
            size = WinSize;
            labelKey = "AC.PersonaPrintStorage";
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
            DoAllowOption(ref num, labelWidth, "AC.AllowColonistPersonaPrints", ref Building_PersonaMatrix.allowColonistPersonaPrints);
            DoAllowOption(ref num, labelWidth, "AC.AllowStrangerPersonaPrints", ref Building_PersonaMatrix.allowStrangerPersonaPrints);
            DoAllowOption(ref num, labelWidth, "AC.AllowHostilePersonaPrints", ref Building_PersonaMatrix.allowHostilePersonaPrints);

            var storedPrints = Building_PersonaMatrix.StoredPersonaPrints.ToList();
            Widgets.ListSeparator(ref num, viewRect.width - 15, "AC.PersonaPrintsStored".Translate(storedPrints.Count(), Building_PersonaMatrix.MaxFilledStackCapacity));
            Rect scrollRect = new Rect(0, num, viewRect.width - 16, viewRect.height);
            Rect outerRect = scrollRect;
            outerRect.width += 16;
            outerRect.height -= 120;
            scrollRect.height = storedPrints.Count() * 28f;
            Widgets.BeginScrollView(outerRect, ref scrollPosition, scrollRect);
            foreach (var print in storedPrints)
            {
                DrawThingRow(ref num, scrollRect.width, print);
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
        private void DrawThingRow(ref float y, float width, PersonaPrint print)
        {
            Rect rect1 = new Rect(0.0f, y, width, 28f);
            Widgets.InfoCardButton(0, y, print);
            Rect rect2 = new Rect(rect1.width - 24, y, 24f, 24f);
            TooltipHandler.TipRegion(rect2, "AC.EjectPersonaPrintTooltip".Translate());
            if (Widgets.ButtonImage(rect2, ContentFinder<Texture2D>.Get("UI/Buttons/Drop", true)))
            {
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
                Find.WindowStack.Add(new Dialog_MessageBox("AC.EjectPersonaPrintConfirmation".Translate(print.def.label + " (" + print.PersonaData.name.ToStringFull + ")"),
                     "Confirm".Translate(), delegate
                     {
                         Building_PersonaMatrix.innerContainer.TryDrop(print, Building_PersonaMatrix.InteractionCell, Building_PersonaMatrix.Map, ThingPlaceMode.Near, 1, out Thing droppedThing);
                     }, "GoBack".Translate(), null));
            }
            Rect erasePersonaPrint = rect2;
            erasePersonaPrint.x -= 28;
            TooltipHandler.TipRegion(erasePersonaPrint, "AC.ErasePersonaPrintTooltip".Translate());
            if (Widgets.ButtonImage(erasePersonaPrint, ContentFinder<Texture2D>.Get("UI/Icons/Erase", true)))
            {
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
                Find.WindowStack.Add(new Dialog_MessageBox("AC.ErasePersonaPrintConfirmation".Translate(print.def.label + " (" + print.PersonaData.name.ToStringFull + ")"),
                     "Confirm".Translate(), delegate
                     {
                         Building_PersonaMatrix.innerContainer.Remove(print);
                         print.Destroy();
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
            Widgets.ThingIcon(thingIconRect, print, 1f);
            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = ITab_Pawn_Gear.ThingLabelColor;
            Rect pawnLabelRect = new Rect(thingIconRect.xMax + 5, y, rect1.width - 36f, rect1.height);
            TaggedString pawnLabel = print.PersonaData.PawnNameColored.Truncate(pawnLabelRect.width);
            Widgets.Label(pawnLabelRect, pawnLabel);
            string str2 = print.DescriptionDetailed;
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
