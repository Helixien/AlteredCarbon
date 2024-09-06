using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(Dialog_InfoCard), "DoWindowContents")]
    public static class Dialog_InfoCard_DoWindowContents_Patch
    {
        public static bool Prefix(Dialog_InfoCard __instance, Rect inRect)
        {
            if (__instance.thing is ThingWithNeuralData neuralStack && neuralStack.NeuralData.ContainsData)
            {
                DoWindowContents(__instance, inRect);
                return false;
            }
            return true;
        }
        public static void DoWindowContents(Dialog_InfoCard __instance, Rect inRect)
        {
            Rect rect = new Rect(inRect);
            rect = rect.ContractedBy(18f);
            rect.height = 34f;
            rect.x += 34f;
            Text.Font = GameFont.Medium;
            Widgets.Label(rect, __instance.GetTitle());
            Rect rect2 = new Rect(inRect.x + 9f, rect.y, 34f, 34f);
            Widgets.ThingIcon(rect2, __instance.thing);

            Rect rect3 = new Rect(inRect);
            rect3.yMin = rect.yMax;
            rect3.yMax -= 38f;
            Rect rect4 = rect3;
            List<TabRecord> list = new List<TabRecord>();
            TabRecord item = new TabRecord("TabStats".Translate(), delegate
            {
                __instance.tab = Dialog_InfoCard.InfoCardTab.Stats;
            }, __instance.tab == Dialog_InfoCard.InfoCardTab.Stats);
            list.Add(item);

            TabRecord item2 = new TabRecord("TabCharacter".Translate(), delegate
            {
                __instance.tab = Dialog_InfoCard.InfoCardTab.Character;
            }, __instance.tab == Dialog_InfoCard.InfoCardTab.Character);
            list.Add(item2);
            rect4.yMin += 45f;
            TabDrawer.DrawTabs(rect4, list);
            FillCard(__instance, rect4.ContractedBy(18f));
        }

        private static void FillCard(Dialog_InfoCard __instance, Rect cardRect)
        {
            if (__instance.tab == Dialog_InfoCard.InfoCardTab.Stats)
            {
                StatsReportUtility.DrawStatsReport(cardRect, __instance.thing);
            }
            else if (__instance.tab == Dialog_InfoCard.InfoCardTab.Character)
            {
                var stack = __instance.thing as ThingWithNeuralData;
                CharacterCardUtility.DrawCharacterCard(cardRect, stack.NeuralData.DummyPawn);
            }
        }
    }
}

