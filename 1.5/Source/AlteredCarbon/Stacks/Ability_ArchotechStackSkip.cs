using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using VFECore.Abilities;
using Ability = VFECore.Abilities.Ability;

namespace AlteredCarbon
{
    [HotSwappable]
    public class Ability_ArchotechStackSkip : Ability
    {
        public NeuralStack archoStackForAbility;
        public Hediff_NeuralStack Hediff_NeuralStack => pawn.GetHediff(AC_DefOf.AC_ArchotechStack) as Hediff_NeuralStack;
        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            return Validate(target, showMessages);
        }

        public static bool Validate(LocalTargetInfo target, bool showMessages)
        {
            var pawnTarget = target.Pawn;
            if (pawnTarget != null)
            {
                if (AC_Utils.CanImplantStackTo(AC_DefOf.AC_ArchotechStack, pawnTarget, null, showMessages))
                {
                    return true;
                }
            }
            return false;
        }

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            var target = targets.FirstOrDefault(x => x.Thing is Pawn);
            var pawnTarget = target.Thing as Pawn;
            if (pawnTarget != null)
            {
                var sourceHediff = Hediff_NeuralStack;
                SkipTo(CasterPawn.Faction, pawnTarget, pawn, sourceHediff.NeuralData, cooldown);
                if (archoStackForAbility is not null)
                {
                    Thing.allowDestroyNonDestroyable = true;
                    archoStackForAbility.Destroy();
                    Thing.allowDestroyNonDestroyable = false;
                }
                else
                {
                    sourceHediff.preventSpawningStack = true;
                    pawn.health.RemoveHediff(sourceHediff);
                    sourceHediff.preventSpawningStack = false;
                    AlteredCarbonManager.Instance.deadPawns.Add(pawn);
                    pawn.GetComp<CompAbilities>().currentlyCasting = null;
                }
                archoStackForAbility = null;
            }
        }

        public static void SkipTo(Faction casterFaction, Pawn pawnTarget, Pawn pawnSource, NeuralData sourceData, int cooldown)
        {
            if (pawnTarget.Faction != null && casterFaction != null && pawnTarget.Faction != casterFaction)
            {
                pawnTarget.Faction.TryAffectGoodwillWith(casterFaction, pawnTarget.Faction.GoodwillToMakeHostile(casterFaction), reason: AC_DefOf.AC_UsedArchotechStack, lookTarget: pawnTarget);
            }
            if (pawnTarget.HasNeuralStack(out var stackHediff))
            {
                stackHediff.preventKill = true;
                pawnTarget.health.RemoveHediff(stackHediff);
            }
            BodyPartRecord neckRecord = pawnTarget.GetNeck();
            var copyHediff = HediffMaker.MakeHediff(AC_DefOf.AC_ArchotechStack, pawnTarget, neckRecord) as Hediff_NeuralStack;
            copyHediff.NeuralData = sourceData;
            pawnTarget.health.AddHediff(copyHediff, neckRecord);
            if (pawnSource != null)
            {
                sourceData.CopyFromPawn(pawnSource, AC_DefOf.AC_ActiveArchotechStack);
            }
            sourceData.OverwritePawn(pawnTarget);
            Recipe_InstallNeuralStack.ApplyMindEffects(pawnTarget, copyHediff);
            copyHediff.skipAbility.cooldown = cooldown;
        }
    }
}