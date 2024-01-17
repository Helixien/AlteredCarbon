using HarmonyLib;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    [HarmonyPatch(typeof(PawnRenderer), "DrawEquipment")]
    public static class PawnRenderer_EquipmentRender_Patch
    {
        public static void Postfix(PawnRenderer __instance, Pawn ___pawn)
        {
            Pawn pawn = ___pawn;
            if (!pawn.Dead && pawn.RaceProps.Humanlike && pawn.Spawned && ((pawn.stances.curStance is Stance_Busy stanceBusy && !stanceBusy.neverAimWeapon
                        && stanceBusy.focusTarg.IsValid) || __instance.CarryWeaponOpenly()))
            {
                var verb = pawn.CurrentEffectiveVerb;
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
                    Graphics.DrawMesh(material: graphic.MatSingle, mesh: mesh, position: drawLoc, rotation: Quaternion.AngleAxis(num, Vector3.up), layer: 0);

                }
            }
        }
    }
}

