using RimWorld;
using UnityEngine;
using Verse;
using VFECore.Abilities;

namespace AlteredCarbon
{
    public class JumpingPawn : AbilityPawnFlyer
    {
        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            FlyingPawn.DrawAt(GetDrawPos(), flip);
        }

        private Vector3 GetDrawPos()
        {
            var x = ticksFlying / (float)ticksFlightTime;
            var drawPos = position;
            drawPos.y = AltitudeLayer.Skyfaller.AltitudeFor();
            return drawPos + Vector3.forward * (x - Mathf.Pow(x, 2)) * 15f;
        }
        protected override void RespawnPawn()
        {
            Pawn flyingPawn = FlyingPawn;
            base.RespawnPawn();
            FleckMaker.ThrowSmoke(flyingPawn.DrawPos, flyingPawn.Map, 1f);
            FleckMaker.ThrowDustPuffThick(flyingPawn.DrawPos, flyingPawn.Map, 2f, new Color(1f, 1f, 1f, 2.5f));
        }
    }
}