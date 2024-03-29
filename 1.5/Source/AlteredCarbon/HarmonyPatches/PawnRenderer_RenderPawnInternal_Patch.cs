using System;
using System.Globalization;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    [HotSwappable]
    [HarmonyPatch(typeof(PawnRenderer), "RenderPawnAt")]
    public static class PawnRenderer_RenderPawnInternal_Patch
    {
        public static void Prefix(Pawn ___pawn, ref Vector3 drawLoc)
        {
            if (___pawn.CurrentBed() is Building_SleeveCasket bed)
            {
                if (bed.Rotation == Rot4.South)
                {
                    drawLoc.z -= 0.1f;
                }
                else if (bed.Rotation == Rot4.North)
                {
                    drawLoc.z += 0.3f;
                }
                else if (bed.Rotation == Rot4.East)
                {
                    drawLoc.x += 0.3f;
                }
                else if (bed.Rotation == Rot4.West)
                {
                    drawLoc.x -= 0.3f;
                }
            }
        }
    }
}