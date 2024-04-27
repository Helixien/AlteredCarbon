using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AlteredCarbon
{
    [HotSwappable]
    public class ITab_StackStorageContents : ITab
    {
        private static readonly Vector2 WinSize = new Vector2(432f, 480f);
        private Vector2 scrollPosition;
        public Building_StackStorage Building_StackStorage => SelThing as Building_StackStorage;
        public ITab_StackStorageContents()
        {
            size = WinSize;
            labelKey = "AC.StackStorage";
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
            DoAllowOption(ref num, labelWidth, "AC.AllowColonistStacks", ref Building_StackStorage.allowColonistPersonaStacks);
            DoAllowOption(ref num, labelWidth, "AC.AllowStrangerStacks", ref Building_StackStorage.allowStrangerPersonaStacks);
            DoAllowOption(ref num, labelWidth, "AC.AllowHostileStacks", ref Building_StackStorage.allowHostilePersonaStacks);
            DoAllowOption(ref num, labelWidth, "AC.AllowArchoStacks", ref Building_StackStorage.allowArchoStacks);

            System.Collections.Generic.List<PersonaStack> storedStacks = Building_StackStorage.StoredStacks.ToList();
            Widgets.ListSeparator(ref num, viewRect.width - 15, "AC.PersonaStacksInArray".Translate(storedStacks.Count(), Building_StackStorage.MaxFilledStackCapacity));
            Rect scrollRect = new Rect(0, num, viewRect.width - 16, viewRect.height);
            Rect outerRect = scrollRect;
            outerRect.width += 16;
            outerRect.height -= 120;
            scrollRect.height = storedStacks.Count() * 28f;
            Widgets.BeginScrollView(outerRect, ref scrollPosition, scrollRect);
            foreach (PersonaStack personaStack in storedStacks)
            {
                bool showDuplicateStatus = storedStacks.Count(x => x.PersonaData.stackGroupID == personaStack.PersonaData.stackGroupID) > 1;
                DrawThingRow(ref num, scrollRect.width, personaStack, showDuplicateStatus);
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
        private void DrawThingRow(ref float y, float width, PersonaStack personaStack, bool showDuplicateStatus)
        {
            Rect rect1 = new Rect(0.0f, y, width, 28f);
            Widgets.InfoCardButton(0, y, personaStack);
            Rect rect2 = new Rect(rect1.width - 24, y, 24f, 24f);
            TooltipHandler.TipRegion(rect2, "AC.EjectStackTooltip".Translate());
            if (Widgets.ButtonImage(rect2, ContentFinder<Texture2D>.Get("UI/Buttons/Drop", true)))
            {
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
                Find.WindowStack.Add(new Dialog_MessageBox("AC.EjectStackConfirmation".Translate(personaStack.def.label + " (" + personaStack.PersonaData.name.ToStringFull + ")"),
                     "Confirm".Translate(), delegate
                     {
                         Building_StackStorage.innerContainer.TryDrop(personaStack, Building_StackStorage.InteractionCell, Building_StackStorage.Map, ThingPlaceMode.Near, 1, out Thing droppedThing);
                     }, "GoBack".Translate(), null));
            }
            Rect installStackRect = rect2;
            installStackRect.x -= 28;

            TooltipHandler.TipRegion(installStackRect, personaStack.IsArchoStack ? "AC.InstallArchoStack".Translate() : "AC.InstallStack".Translate());
            if (Widgets.ButtonImage(installStackRect, ContentFinder<Texture2D>.Get("UI/Icons/Install", true)))
            {
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
                Find.Targeter.BeginTargeting(personaStack.ForPawn(), delegate (LocalTargetInfo x)
                {
                    Building_StackStorage.innerContainer.TryDrop(personaStack, Building_StackStorage.InteractionCell, Building_StackStorage.Map, ThingPlaceMode.Near, 1, out Thing droppedThing);
                    personaStack.InstallStackRecipe(x.Pawn, AC_Utils.stackRecipesByDef[personaStack.def].recipe);
                });
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
            Widgets.ThingIcon(thingIconRect, personaStack, 1f);
            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = ITab_Pawn_Gear.ThingLabelColor;
            Rect pawnLabelRect = new Rect(thingIconRect.xMax + 5, y, rect1.width - 36f, rect1.height);
            TaggedString pawnLabel = personaStack.PersonaData.PawnNameColored.Truncate(pawnLabelRect.width);
            if (showDuplicateStatus)
            {
                pawnLabel += " (" + (personaStack.PersonaData.isCopied ? "AC.Copy".Translate() : "AC.Original".Translate()) + ")";
            }
            Widgets.Label(pawnLabelRect, pawnLabel);
            string str2 = personaStack.DescriptionDetailed;
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
