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
            if (colonist.Dead && AlteredCarbonManager.Instance.StacksIndex.TryGetValue(colonist.thingIDNumber, out var personaStack))
            {
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && Event.current.clickCount == 2 
                    && Mouse.IsOver(rect))
                {
                    Event.current.Use();
                    if (personaStack is null)
                    {
                        CameraJumper.TryJumpAndSelect(colonist);
                        return false;
                    }
                    else
                    {
                        CameraJumper.TryJumpAndSelect(personaStack);
                        return false;
                    }
                }
            }
            return true;
        }
    }
}

