using RimWorld;
using RimWorld.Planet;
using Verse;
using Ability = VFECore.Abilities.Ability;

namespace AlteredCarbon
{
    public class Ability_Jump : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            var map = Caster.Map;
            this.pawn.rotationTracker.FaceTarget(targets[0].Cell);
            var flyer = (JumpingPawn)PawnFlyer.MakeFlyer(AC_Extra_DefOf.AC_JumpingPawn, CasterPawn, targets[0].Cell, null, null);
            flyer.ability = this;
            flyer.target = targets[0].Cell.ToVector3Shifted();
            GenSpawn.Spawn(flyer, Caster.Position, map);
            base.Cast(targets);
        }
    }
}