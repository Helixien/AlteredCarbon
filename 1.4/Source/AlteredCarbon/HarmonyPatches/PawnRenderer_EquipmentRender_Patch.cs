﻿using HarmonyLib;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(PawnRenderer), "DrawEquipment")]
    public static class PawnRenderer_EquipmentRender_Patch
    {
        public static bool Prefix(PawnRenderer __instance, Pawn ___pawn)
        {
            Pawn pawn = ___pawn;
            if (!pawn.Dead && pawn.RaceProps.Humanlike && pawn.Spawned)
            {
                var verb = GetCurrentVerb(__instance, pawn);
                if (verb?.HediffCompSource is HediffComp_MeleeWeapon hediffComp)
                {
                    var graphic = hediffComp.Graphic;
                    Vector3 drawLoc = pawn.DrawPos;
                    float aimAngle = 0f;

                    if (pawn.Rotation == Rot4.South)
                    {
                        drawLoc += new Vector3(0f, 0f, -0.22f);
                        drawLoc.y += 9f / 245f;
                        aimAngle = 143f;
                    }
                    else if (pawn.Rotation == Rot4.North)
                    {
                        drawLoc += new Vector3(0f, 0f, -0.11f);
                        drawLoc.y += 0f;
                        aimAngle = 143f;
                    }
                    else if (pawn.Rotation == Rot4.East)
                    {
                        drawLoc += new Vector3(0.2f, 0f, -0.22f);
                        drawLoc.y += 9f / 245f;
                        aimAngle = 143f;
                    }
                    else if (pawn.Rotation == Rot4.West)
                    {
                        drawLoc += new Vector3(-0.2f, 0f, -0.22f);
                        drawLoc.y += 9f / 245f;
                        aimAngle = 217f;
                    }

                    Mesh mesh;
                    float num = aimAngle - 90f;
                    float equippedAngleOffset = -65f;

                    if (aimAngle > 20f && aimAngle < 160f)
                    {
                        mesh = MeshPool.plane10;
                        num += equippedAngleOffset;
                    }
                    else if (aimAngle > 200f && aimAngle < 340f)
                    {
                        mesh = MeshPool.plane10Flip;
                        num -= 180f;
                        num -= equippedAngleOffset;
                    }
                    else
                    {
                        mesh = MeshPool.plane10;
                        num += equippedAngleOffset;
                    }

                    num %= 360f;
                    Matrix4x4 matrix = Matrix4x4.TRS(s: new Vector3(graphic.drawSize.x, 0f, graphic.drawSize.y), pos: drawLoc, q: Quaternion.AngleAxis(num, Vector3.up));
                    Graphics.DrawMesh(mesh, matrix, graphic.MatSingle, 0);
                    return false;
                }
            }
            return true;
        }

        private static Verb GetCurrentVerb(PawnRenderer __instance, Pawn pawn)
        {
            if (pawn.stances.curStance is Stance_Busy stanceBusy
                            && !stanceBusy.neverAimWeapon && stanceBusy.focusTarg.IsValid)
            {
                return stanceBusy.verb;
            }
            if (__instance.CarryWeaponOpenly())
            {
                var verb = pawn.jobs.curJob?.verbToUse;
                if (verb != null)
                {
                    return verb;
                }
                if (pawn.jobs.curDriver is JobDriver_AttackMelee || pawn.equipment.PrimaryEq is null)
                {
                    return pawn.meleeVerbs.curMeleeVerb;
                }
            }
            return null;
        }
    }
}

