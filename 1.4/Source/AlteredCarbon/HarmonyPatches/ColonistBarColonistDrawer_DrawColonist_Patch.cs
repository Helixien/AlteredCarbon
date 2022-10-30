using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{

    [HarmonyPatch(typeof(ColonistBarColonistDrawer), "DrawColonist")]
    [StaticConstructorOnStartup]
    public static class ColonistBarColonistDrawer_DrawColonist_Patch
    {
        private static readonly Texture2D Icon_StackDead = ContentFinder<Texture2D>.Get("UI/Icons/StackDead");

        public static bool Prefix(ColonistBarColonistDrawer __instance, Rect rect, Pawn colonist, Map pawnMap, bool highlight, bool reordering,
            Dictionary<string, string> ___pawnLabelsCache, Vector2 ___PawnTextureSize,
            Texture2D ___MoodBGTex)
        {
            if (colonist.Dead && colonist.HasStack())
            {
                float alpha = Find.ColonistBar.GetEntryRectAlpha(rect);
                __instance.ApplyEntryInAnotherMapAlphaFactor(pawnMap, ref alpha);
                if (reordering)
                {
                    alpha *= 0.5f;
                }
                Color color2 = GUI.color = new Color(1f, 1f, 1f, alpha);
                GUI.DrawTexture(rect, ColonistBar.BGTex);
                if (colonist.needs != null && colonist.needs.mood != null)
                {
                    Rect position = rect.ContractedBy(2f);
                    float num = position.height * colonist.needs.mood.CurLevelPercentage;
                    position.yMin = position.yMax - num;
                    position.height = num;
                    GUI.DrawTexture(position, ___MoodBGTex);
                }
                if (highlight)
                {
                    int thickness = (rect.width <= 22f) ? 2 : 3;
                    GUI.color = Color.white;
                    Widgets.DrawBox(rect, thickness);
                    GUI.color = color2;
                }
                Rect rect2 = rect.ContractedBy(-2f * Find.ColonistBar.Scale);
                if ((colonist.Dead ? Find.Selector.SelectedObjects.Contains(colonist.Corpse) : Find.Selector.SelectedObjects.Contains(colonist)) && !WorldRendererUtility.WorldRenderedNow)
                {
                    __instance.DrawSelectionOverlayOnGUI(colonist, rect2);
                }

                GUI.DrawTexture(__instance.GetPawnTextureRect(rect.position), PortraitsCache.Get(colonist, ColonistBarColonistDrawer.PawnTextureSize, Rot4.South, ColonistBarColonistDrawer.PawnTextureCameraOffset, 1.28205f, true, true, true, true, null));
                GUI.color = new Color(1f, 1f, 1f, alpha * 0.8f);

                float num3 = 20f * Find.ColonistBar.Scale;
                Vector2 pos2 = new Vector2(rect.x + 1f, rect.yMax - num3 - 1f);
                DrawIcon(Icon_StackDead, ref pos2, "ActivityIconMedicalRest".Translate());
                GUI.color = color2;

                float num2 = 4f * Find.ColonistBar.Scale;
                Vector2 pos = new Vector2(rect.center.x, rect.yMax - num2);
                GenMapUI.DrawPawnLabel(colonist, pos, alpha, rect.width + Find.ColonistBar.SpaceBetweenColonistsHorizontal - 2f, ___pawnLabelsCache);
                Text.Font = GameFont.Small;
                GUI.color = Color.white;
                return false;
            }
            return true;
        }

        private static void DrawIcon(Texture2D icon, ref Vector2 pos, string tooltip)
        {
            float num = 20f * Find.ColonistBar.Scale;
            Rect rect = new Rect(pos.x, pos.y, num, num);
            GUI.DrawTexture(rect, icon);
            TooltipHandler.TipRegion(rect, tooltip);
            pos.x += num;
        }
    }
}

