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
    public class Hediff_PersonaStack : Hediff_Implant
    {
        public Ability_ArchoStackSkip skipAbility;
        public ThingDef SourceStack
        {
            get
            {
                if (this.def == AC_DefOf.AC_PersonaStack)
                {
                    return AC_DefOf.AC_FilledPersonaStack;
                }
                return AC_DefOf.AC_FilledArchoStack;
            }
        }
        private PersonaData personaData;
        public PersonaData PersonaData
        {
            get
            {
                if (personaData is null)
                {
                    personaData = new PersonaData();
                    personaData.CopyFromPawn(pawn, SourceStack, copyRaceGenderInfo: true);
                }
                return personaData;
            }
            set
            {
                personaData = value;
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            if (this.def == AC_DefOf.AC_ArchoStack)
            {
                if (skipAbility.ShowGizmoOnPawn())
                {
                    yield return skipAbility.GetGizmo();
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
                if (hediff != this && hediff is Hediff_PersonaStack otherStack)
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
                AlteredCarbonManager.Instance.TryAddRelationships(pawn, this.PersonaData.StackGroupData);
            }
            CreateSkipAbilityIfMissing();
        }

        private void CreateSkipAbilityIfMissing()
        {
            if (this.def == AC_DefOf.AC_ArchoStack && skipAbility is null)
            {
                skipAbility = (Ability_ArchoStackSkip)Activator.CreateInstance(AC_DefOf.AC_ArchoStackSkip.abilityClass);
                skipAbility.def = AC_DefOf.AC_ArchoStackSkip;
                skipAbility.holder = this.pawn;
                skipAbility.pawn = this.pawn;
                skipAbility.Init();
            }
        }

        public override bool ShouldRemove => false;

        public override void Notify_PawnDied(DamageInfo? dinfo, Hediff culprit = null)
        {
            if (!PersonaData.ContainsInnerPersona)
            {
                PersonaData.CopyFromPawn(this.pawn, SourceStack);
            }
            base.Notify_PawnDied(dinfo, culprit);
        }

        public override void Notify_PawnKilled()
        {
            if (!PersonaData.ContainsInnerPersona)
            {
                PersonaData.CopyFromPawn(this.pawn, SourceStack);
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

            if (this.def == AC_DefOf.AC_ArchoStack)
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
        public void SpawnStack(bool destroyPawn = false, ThingPlaceMode placeMode = ThingPlaceMode.Near, 
            Caravan caravan = null, bool psycastEffect = false, Map mapToSpawn = null)
        {
            if (preventSpawningStack)
            {
                return;
            }
            preventSpawningStack = true;
            try
            {
                var stackDef = SourceStack;
                var personaStack = ThingMaker.MakeThing(stackDef) as PersonaStack;
                personaStack.PersonaData.CopyFromPawn(this.pawn, stackDef);
                personaStack.PersonaData.CopyOriginalData(PersonaData);
                mapToSpawn ??= this.pawn.MapHeld;
                if (mapToSpawn != null)
                {
                    GenPlace.TryPlaceThing(personaStack, this.pawn.PositionHeld, (Map)mapToSpawn, placeMode);
                    if (psycastEffect)
                    {
                        FleckMaker.Static(personaStack.Position, personaStack.Map, AC_DefOf.PsycastAreaEffect, 3f);
                    }
                }
                else if (caravan != null)
                {
                    CaravanInventoryUtility.GiveThing(caravan, personaStack);
                }
                else
                {
                    Log.Error("Failed to spawn persona stack from " + pawn);
                }
                var degradationHediff = pawn.health.hediffSet.GetFirstHediff<Hediff_StackDegradation>();
                if (degradationHediff != null)
                {
                    personaStack.PersonaData.stackDegradation = degradationHediff.stackDegradation;
                    pawn.health.RemoveHediff(degradationHediff);
                }
                pawn.health.RemoveHediff(this);
                AlteredCarbonManager.Instance.RegisterStack(personaStack);
                AlteredCarbonManager.Instance.RegisterSleeve(this.pawn, personaStack);
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
            }
            catch (Exception ex)
            {
                Log.Error("Error spawning stack: " + this + " - " + ex.ToString());
            }
            preventSpawningStack = false;
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
            Scribe_Deep.Look(ref personaData, "personaData");
            Scribe_Deep.Look(ref skipAbility, "skipAbility");
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