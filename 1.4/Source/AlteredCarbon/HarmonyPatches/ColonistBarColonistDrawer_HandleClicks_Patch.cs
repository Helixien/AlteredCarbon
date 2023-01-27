using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(ColonistBarColonistDrawer), "HandleClicks")]
    public static class HandleClicks_Patch
    {
        public static bool Prefix(Rect rect, Pawn colonist, int reorderableGroup, out bool reordering)
        {
            reordering = false;
            if (colonist.Dead)
            {
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.clickCount == 1 && Mouse.IsOver(rect))
                {
                    Event.current.Use();
                    if (AlteredCarbonManager.Instance.StacksIndex.TryGetValue(colonist.thingIDNumber, out var corticalStack))
                    {
                        if (corticalStack != null)
                        {
                            CameraJumper.TryJumpAndSelect(colonist);
                        }
                    }
                }

                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.clickCount == 2 && Mouse.IsOver(rect))
                {
                    Event.current.Use();
                    if (AlteredCarbonManager.Instance.StacksIndex.TryGetValue(colonist.thingIDNumber, out var corticalStack))
                    {
                        if (corticalStack is null)
                        {
                            CameraJumper.TryJumpAndSelect(colonist);
                        }
                        else
                        {
                            CameraJumper.TryJumpAndSelect(corticalStack);
                        }
                    }
                    else
                    {
                        CameraJumper.TryJumpAndSelect(colonist);
                    }
                }
                reordering = ReorderableWidget.Reorderable(reorderableGroup, rect, useRightButton: true);
                if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && Mouse.IsOver(rect))
                {
                    Event.current.Use();
                }
                return false;
            }
            return true;
        }
    }
}

