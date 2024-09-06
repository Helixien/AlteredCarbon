using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace AlteredCarbon
{
    [HotSwappable]
    public class Hediff_NeuralStack : Hediff_Implant
    {
        public Ability_ArchotechStackSkip skipAbility;
        public ThingDef SourceStack
        {
            get
            {
                if (this.def == AC_DefOf.AC_NeuralStack)
                {
                    return AC_DefOf.AC_ActiveNeuralStack;
                }
                return AC_DefOf.AC_ActiveArchotechStack;
            }
        }
        private NeuralData neuralData;
        public NeuralData NeuralData
        {
            get
            {
                if (neuralData is null)
                {
                    neuralData = new NeuralData();
                    neuralData.CopyFromPawn(pawn, SourceStack, copyRaceGenderInfo: true);
                }
                return neuralData;
            }
            set
            {
                neuralData = value;
            }
        }

        public Hediff_ParroterStack needleCastingInto;

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (this.def == AC_DefOf.AC_ArchotechStack)
            {
                if (skipAbility.ShowGizmoOnPawn())
                {
                    yield return skipAbility.GetGizmo();
                }
            }
            else
            {
                if (pawn.IsColonistPlayerControlled)
                {
                    if (needleCastingInto is not null)
                    {

                    }
                    else
                    {

                    }
                }
            }
            yield break;
        }

        public override void PostAdd(DamageInfo? dinfo)
        {
            this.Part = pawn.GetNeck();
            base.PostAdd(dinfo);

            foreach (var hediff in pawn.health.hediffSet.hediffs.ToList())
            {
                if (hediff != this && hediff is Hediff_NeuralStack otherStack)
                {
                    otherStack.preventKill = otherStack.preventSpawningStack = true;
                    pawn.health.RemoveHediff(otherStack);
                    otherStack.preventKill = otherStack.preventSpawningStack = false;
                }
            }

            var emptySleeveHediff = pawn.health.hediffSet.GetFirstHediffOfDef(AC_DefOf.AC_EmptySleeve);
            if (emptySleeveHediff != null)
            {
                pawn.health.RemoveHediff(emptySleeveHediff);
            }
            if (AlteredCarbonManager.Instance.PawnsWithStacks.Contains(pawn) is false)
            {
                AlteredCarbonManager.Instance.RegisterPawn(pawn);
                AlteredCarbonManager.Instance.TryAddRelationships(pawn, this.NeuralData.StackGroupData);
            }
            CreateSkipAbilityIfMissing();
        }

        private void CreateSkipAbilityIfMissing()
        {
            if (this.def == AC_DefOf.AC_ArchotechStack && skipAbility is null)
            {
                skipAbility = (Ability_ArchotechStackSkip)Activator.CreateInstance(AC_DefOf.AC_ArchotechStackSkip.abilityClass);
                skipAbility.def = AC_DefOf.AC_ArchotechStackSkip;
                skipAbility.holder = this.pawn;
                skipAbility.pawn = this.pawn;
                skipAbility.Init();
            }
        }

        public override bool ShouldRemove => false;

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            if (!NeuralData.ContainsNeural)
            {
                NeuralData.CopyFromPawn(this.pawn, SourceStack);
            }
            base.Notify_PawnDied(dinfo, culprit);
        }

        public override void Notify_PawnKilled()
        {
            if (!NeuralData.ContainsNeural)
            {
                NeuralData.CopyFromPawn(this.pawn, SourceStack);
            }
            base.Notify_PawnKilled();
        }

        public bool preventKill;
        public override void PostRemoved()
        {
            base.PostRemoved();
            if (!preventKill && !this.pawn.Dead)
            {
                this.pawn.Kill(null);
            }

            if (this.def == AC_DefOf.AC_ArchotechStack)
            {
                if (!preventSpawningStack)
                {
                    SpawnStack(placeMode: ThingPlaceMode.Near);
                }
                pawn.health.hediffSet.hediffs.RemoveAll(x => x.def.defName == "VPE_PsycastAbilityImplant");
                pawn.health.hediffSet.hediffs.RemoveAll(x => x.def == HediffDefOf.PsychicAmplifier);
            }
        }
        public bool preventSpawningStack;
        public NeuralStack SpawnStack(bool destroyPawn = false, ThingPlaceMode placeMode = ThingPlaceMode.Near, 
            Caravan caravan = null, bool psycastEffect = false, Map mapToSpawn = null)
        {
            if (preventSpawningStack)
            {
                return null;
            }
            preventSpawningStack = true;
            try
            {
                float partHealth = pawn.health.hediffSet.GetPartHealth(part);
                if (partHealth <= 0)
                {
                    preventSpawningStack = false;
                    return null;
                }
                var healthRatio = partHealth / part.def.GetMaxHealth(pawn);
                var stackDef = SourceStack;
                var neuralStack = ThingMaker.MakeThing(stackDef) as NeuralStack;
                if (neuralStack.IsArchotechStack is false)
                {
                    neuralStack.HitPoints = (int)(neuralStack.MaxHitPoints * healthRatio);
                }
                neuralStack.NeuralData.CopyFromPawn(this.pawn, stackDef);
                neuralStack.NeuralData.CopyOriginalData(NeuralData);
                mapToSpawn ??= this.pawn.MapHeld;
                if (mapToSpawn != null)
                {
                    GenPlace.TryPlaceThing(neuralStack, this.pawn.PositionHeld, (Map)mapToSpawn, placeMode);
                    if (psycastEffect)
                    {
                        FleckMaker.Static(neuralStack.Position, neuralStack.Map, AC_DefOf.PsycastAreaEffect, 3f);
                    }
                }
                else if (caravan != null)
                {
                    CaravanInventoryUtility.GiveThing(caravan, neuralStack);
                }
                else
                {
                    Log.Error("Failed to spawn neural stack from " + pawn);
                }
                var degradationHediff = pawn.health.hediffSet.GetFirstHediff<Hediff_StackDegradation>();
                if (degradationHediff != null)
                {
                    neuralStack.NeuralData.stackDegradation = degradationHediff.stackDegradation;
                    pawn.health.RemoveHediff(degradationHediff);
                }
                pawn.health.RemoveHediff(this);
                AlteredCarbonManager.Instance.RegisterStack(neuralStack);
                AlteredCarbonManager.Instance.RegisterSleeve(this.pawn, neuralStack);
                AlteredCarbonManager.Instance.ReplacePawnWithStack(pawn, neuralStack);
                AlteredCarbonManager.Instance.deadPawns.Add(pawn);
                neuralStack.NeuralData.hostPawn = null;
                if (LookTargets_Patch.targets.TryGetValue(pawn, out var targets))
                {
                    foreach (var target in targets)
                    {
                        target.targets.Remove(pawn);
                        target.targets.Add(neuralStack);
                    }
                }
                if (destroyPawn)
                {
                    if (this.pawn.Dead)
                    {
                        this.pawn.Corpse.Destroy();
                    }
                    else
                    {
                        this.pawn.Destroy();
                    }
                }
                return neuralStack;
            }
            catch (Exception ex)
            {
                Log.Error("Error spawning stack: " + this + " - " + ex.ToString());
            }
            preventSpawningStack = false;
            return null;
        }

        [HarmonyPatch(typeof(HediffSet), "ExposeData")]
        public static class HediffSet_ExposeData_Patch
        {
            public static Pawn curPawn;

            public static void Postfix(HediffSet __instance)
            {
                curPawn = __instance.pawn;
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref neuralData, "neuralData");
            Scribe_Deep.Look(ref skipAbility, "skipAbility");
            Scribe_References.Look(ref needleCastingInto, "needleCastingInto");
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                CreateSkipAbilityIfMissing();
            }
            var curPawn = this.pawn ?? HediffSet_ExposeData_Patch.curPawn;
            HediffSet_ExposeData_Patch.curPawn = null;
            if (skipAbility != null)
            {
                skipAbility.holder = curPawn;
                skipAbility.pawn = curPawn;
            }
        }
    }
}