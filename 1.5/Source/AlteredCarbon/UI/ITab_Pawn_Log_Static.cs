using Verse;
using System.Collections.Generic;
using UnityEngine;
using RimWorld;
using LudeonTK;
using System.Text;

namespace AlteredCarbon
{
    [HotSwappable]
    public static class ITab_Pawn_Log_Static
    {
        [TweakValue("Interface", 0f, 1000f)]
        private static float ShowAllX = 18;

        [TweakValue("Interface", 0f, 1000f)]
        private static float ShowAllWidth = 100f;

        [TweakValue("Interface", 0f, 1000f)]
        private static float ShowCombatX = 270;

        [TweakValue("Interface", 0f, 1000f)]
        private static float ShowCombatWidth = 115f;

        [TweakValue("Interface", 0f, 1000f)]
        private static float ShowSocialX = 140;

        [TweakValue("Interface", 0f, 1000f)]
        private static float ShowSocialWidth = 105f;

        [TweakValue("Interface", 0f, 20f)]
        private static float ToolbarHeight = 2f;

        [TweakValue("Interface", 0f, 100f)]
        private static float ButtonOffset = 25;
        private static bool showAll;
        private static bool showCombat = true;
        private static bool showSocial = true;
        private static LogEntry logSeek;
        private static ITab_Pawn_Log_Utility.LogDrawData data = new ITab_Pawn_Log_Utility.LogDrawData();
        private static List<ITab_Pawn_Log_Utility.LogLineDisplayable> cachedLogDisplay;
        private static int cachedLogDisplayLastTick = -1;
        private static int cachedLogPlayLastTick = -1;
        private static Pawn cachedLogForPawn;
        private static Vector2 scrollPosition;

        public static void FillTab(Rect rect, Pawn pawn)
        {
            Widgets.BeginGroup(rect);
            GameFont font = Text.Font;
            Text.Font = GameFont.Small;

            Rect rect2 = new Rect(ShowAllX, ToolbarHeight, ShowAllWidth, 24f);
            bool flag = showAll;
            Widgets.CheckboxLabeled(rect2, "ShowAll".Translate(), ref showAll);
            if (flag != showAll)
            {
                cachedLogDisplay = null;
            }

            Rect rect3 = new Rect(ShowCombatX, ToolbarHeight, ShowCombatWidth, 24f);
            bool flag2 = showCombat;
            Widgets.CheckboxLabeled(rect3, "ShowCombat".Translate(), ref showCombat);
            if (flag2 != showCombat)
            {
                cachedLogDisplay = null;
            }

            Rect rect4 = new Rect(ShowSocialX, ToolbarHeight, ShowSocialWidth, 24f);
            bool flag3 = showSocial;
            Widgets.CheckboxLabeled(rect4, "ShowSocial".Translate(), ref showSocial);
            if (flag3 != showSocial)
            {
                cachedLogDisplay = null;
            }

            Text.Font = font;

            if (cachedLogDisplay == null || cachedLogDisplayLastTick != pawn.records.LastBattleTick || cachedLogPlayLastTick != Find.PlayLog.LastTick || cachedLogForPawn != pawn)
            {
                cachedLogDisplay = ITab_Pawn_Log_Utility.GenerateLogLinesFor(pawn, showAll, showCombat, showSocial, 300);
                cachedLogDisplayLastTick = pawn.records.LastBattleTick;
                cachedLogPlayLastTick = Find.PlayLog.LastTick;
                cachedLogForPawn = pawn;
            }

            Rect rect5 = new Rect(rect.width - ButtonOffset, 0f, 18f, 24f);
            if (Widgets.ButtonImage(rect5, TexButton.Copy))
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (ITab_Pawn_Log_Utility.LogLineDisplayable item in cachedLogDisplay)
                {
                    item.AppendTo(stringBuilder);
                }
                GUIUtility.systemCopyBuffer = stringBuilder.ToString();
            }

            TooltipHandler.TipRegionByKey(rect5, "CopyLogTip");
            rect.yMin = 24f;
            rect = rect.ContractedBy(10f);

            float width = rect.width - 16f - 10f;
            float num = 0f;

            foreach (ITab_Pawn_Log_Utility.LogLineDisplayable item2 in cachedLogDisplay)
            {
                if (item2.Matches(logSeek))
                {
                    scrollPosition.y = num - (item2.GetHeight(width) + rect.height) / 2f;
                }
                num += item2.GetHeight(width);
            }

            logSeek = null;

            if (num > 0f)
            {
                Rect rect6 = new Rect(0f, 0f, rect.width - 16f, num);
                data.StartNewDraw();
                Rect rect7 = rect6;
                rect7.yMin += scrollPosition.y;
                rect7.height = rect.height;
                Widgets.BeginScrollView(new Rect(rect.x - 370 + 15, rect.y, rect.width, rect.height), ref scrollPosition, rect6);
                float num2 = 0f;
                foreach (ITab_Pawn_Log_Utility.LogLineDisplayable item3 in cachedLogDisplay)
                {
                    float height = item3.GetHeight(width);
                    if (rect7.Overlaps(new Rect(0f, num2, width, height)))
                    {
                        item3.Draw(num2, width, data);
                    }
                    else
                    {
                        data.alternatingBackground = !data.alternatingBackground;
                    }
                    num2 += height;
                }
                Widgets.EndScrollView();
            }
            else
            {
                Text.Anchor = TextAnchor.MiddleCenter;
                GUI.color = Color.grey;
                Widgets.Label(new Rect(0f, 0f, 630f, 510f), "(" + "NoRecentEntries".Translate() + ")");
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;
            }
            Widgets.EndGroup();
        }
    }
}
