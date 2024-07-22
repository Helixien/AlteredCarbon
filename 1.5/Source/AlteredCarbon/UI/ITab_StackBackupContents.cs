using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AlteredCarbon
{
    [HotSwappable]
    public class ITab_StackBackupContents : ITab
    {
        private static readonly Vector2 WinSize = new Vector2(432f, 480f);
        private Vector2 scrollPosition;
        public Building_PersonaMatrix Building_PersonaMatrix => SelThing as Building_PersonaMatrix;
        public ITab_StackBackupContents()
        {
            size = WinSize;
            labelKey = "AC.Backups";
        }
        public override void FillTab()
        {
            Text.Font = GameFont.Small;
            Rect viewRect = new Rect(5f, 0f, size.x, size.y - 20f).ContractedBy(10f);
            GUI.BeginGroup(viewRect);
            Text.Font = GameFont.Small;
            GUI.color = Color.white;
            float num = 0;
            List<PersonaData> backedUpStacks = GameComponent_DigitalStorage.Instance.StoredBackedUpStacks.ToList();
            Widgets.ListSeparator(ref num, viewRect.width - 15, "AC.BackedUpStacksInArray".Translate(backedUpStacks.Count()));
            Rect scrollRect = new Rect(0, num, viewRect.width - 16, viewRect.height);
            Rect outerRect = scrollRect;
            outerRect.width += 16;
            outerRect.height -= 20;
            scrollRect.height = backedUpStacks.Count() * 28f;
            Widgets.BeginScrollView(outerRect, ref scrollPosition, scrollRect);

            foreach (PersonaData backedUpStack in backedUpStacks)
            {
                DoRow(ref num, viewRect.width, backedUpStack);
            }

            Widgets.EndScrollView();
            GUI.EndGroup();
            GUI.color = Color.white;
            Text.Anchor = TextAnchor.UpperLeft;
        }
        private void DoRow(ref float y, float width, PersonaData personaData)
        {
            Rect rect1 = new Rect(0.0f, y, width, 28f);
            Rect rect2 = new Rect(rect1.width - 38f, y, 24f, 24f);
            TooltipHandler.TipRegion(rect2, "AC.Erase".Translate());
            if (Widgets.ButtonImage(rect2, ContentFinder<Texture2D>.Get("UI/Icons/Erase", true)))
            {
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
                GameComponent_DigitalStorage.Instance.backedUpStacks.Remove(personaData.PawnID);
            }

            Rect installStackRect = rect2;
            installStackRect.x -= 28;

            TooltipHandler.TipRegion(installStackRect, "AC.RestoreToStackAutomatically".Translate());
            if (Widgets.ButtonImage(installStackRect, personaData.restoreToEmptyStack
                ? ContentFinder<Texture2D>.Get("UI/Icons/RestoreToStackOn", true)
                : ContentFinder<Texture2D>.Get("UI/Icons/RestoreToStackOff", true)))
            {
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
                personaData.restoreToEmptyStack = !personaData.restoreToEmptyStack;
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

            Text.Anchor = TextAnchor.MiddleLeft;
            GUI.color = ITab_Pawn_Gear.ThingLabelColor;
            Rect rect4 = new Rect(35, y, 150, rect1.height);
            TaggedString pawnLabel = personaData.PawnNameColored.Truncate(rect4.width);
            Widgets.Label(rect4, pawnLabel);
            if (Widgets.ButtonInvisible(rect4))
            {
                if (personaData.hostPawn != null && personaData.hostPawn.Destroyed is false)
                {
                    if (personaData.hostPawn.SpawnedOrAnyParentSpawned || personaData.hostPawn.IsCaravanMember())
                    {
                        CameraJumper.TryJumpAndSelect(personaData.hostPawn);
                    }
                    else
                    {
                        Messages.Message("AC.MessageCantSelectOffMapPawn".Translate(personaData.hostPawn.LabelShort, personaData.hostPawn).CapitalizeFirst(), MessageTypeDefOf.RejectInput, historical: false);
                    }
                }
                else
                {
                    Messages.Message("AC.MessageCantSelectOffMapPawn".Translate(personaData.hostPawn.LabelShort, personaData.hostPawn).CapitalizeFirst(), MessageTypeDefOf.RejectInput, historical: false);
                }
            }
            Widgets.InfoCardButton(0, y, personaData.GetDummyPawn);
            Rect timeRect = new Rect(rect4.xMax, rect1.y, 165, rect1.height);
            Widgets.Label(timeRect, "AC.TimeSinceLastBackup".Translate((Find.TickManager.TicksAbs - personaData.lastTimeUpdated).ToStringTicksToPeriod()));
            y += 28;
        }
    }
}
