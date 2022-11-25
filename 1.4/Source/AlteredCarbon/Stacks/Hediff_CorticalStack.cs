using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace AlteredCarbon
{
    public class Hediff_CorticalStack : Hediff_Implant
    {
        public ThingDef SourceStack
        {
            get
            {
                if (this.def == AC_DefOf.VFEU_CorticalStack)
                {
                    return AC_DefOf.VFEU_FilledCorticalStack;
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
                    personaData.CopyPawn(pawn, SourceStack, true);
                }
                return personaData;
            }
            set
            {
                personaData = value;
            }
        }

        public override void PostAdd(DamageInfo? dinfo)
        {
            this.Part = pawn.def.race.body.AllParts.FirstOrDefault((BodyPartRecord x) => x.def == BodyPartDefOf.Neck);
            base.PostAdd(dinfo);
            var emptySleeveHediff = pawn.health.hediffSet.GetFirstHediffOfDef(AC_DefOf.VFEU_EmptySleeve);
            if (emptySleeveHediff != null)
            {
                pawn.health.RemoveHediff(emptySleeveHediff);
            }
            if (!this.pawn.HasStack() && this.PersonaData.stackGroupID == -1)
            {
                this.PersonaData.gender = pawn.gender;
                this.PersonaData.race = pawn.kindDef.race;
                if (AlteredCarbonManager.Instance.stacksRelationships != null)
                {
                    this.PersonaData.stackGroupID = AlteredCarbonManager.Instance.stacksRelationships.Count + 1;
                }
                else
                {
                    this.PersonaData.stackGroupID = 0;
                }
                AlteredCarbonManager.Instance.RegisterPawn(pawn);
                AlteredCarbonManager.Instance.TryAddRelationships(pawn);
            }
        }
        public override bool ShouldRemove => false;
        public override void Notify_PawnDied()
        {
            if (!PersonaData.ContainsInnerPersona)
            {
                PersonaData.CopyPawn(this.pawn, SourceStack);
            }
            base.Notify_PawnDied();
        }

        public override void Notify_PawnKilled()
        {
            if (!PersonaData.ContainsInnerPersona)
            {
                PersonaData.CopyPawn(this.pawn, SourceStack);
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

            if (this.def == AC_DefOf.AC_ArchoStack && !spawningStack)
            {
                SpawnStack(placeMode: ThingPlaceMode.Near);
            }
        }
        public bool spawningStack;
        public void SpawnStack(bool destroyPawn = false, ThingPlaceMode placeMode = ThingPlaceMode.Near, Caravan caravan = null, bool psycastEffect = false)
        {
            spawningStack = true;
            var stackDef = SourceStack;
            var corticalStack = ThingMaker.MakeThing(stackDef) as CorticalStack;
            corticalStack.PersonaData.CopyPawn(this.pawn, stackDef);
            if (this.pawn.MapHeld != null)
            {
                GenPlace.TryPlaceThing(corticalStack, this.pawn.PositionHeld, this.pawn.MapHeld, placeMode);
                if (psycastEffect)
                {
                    FleckMaker.Static(corticalStack.Position, corticalStack.Map, AC_DefOf.PsycastAreaEffect, 3f);
                }
            }
            else if (caravan != null)
            {
                CaravanInventoryUtility.GiveThing(caravan, corticalStack);
            }
            this.pawn.health.RemoveHediff(this);
            AlteredCarbonManager.Instance.RegisterStack(corticalStack);
            AlteredCarbonManager.Instance.RegisterSleeve(this.pawn, corticalStack);
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
            spawningStack = false;
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref personaData, "personaData");
        }
    }
}