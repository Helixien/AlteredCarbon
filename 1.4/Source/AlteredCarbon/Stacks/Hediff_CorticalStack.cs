using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AlteredCarbon
{
    public class Hediff_CorticalStack : Hediff_Implant
    {
        private PersonaData personaData;
        public PersonaData PersonaData
        {
            get
            {
                if (personaData is null)
                {
                    personaData = new PersonaData();
                    personaData.CopyPawn(pawn, AC_DefOf.VFEU_FilledCorticalStack, true);
                }
                return personaData;
            }
            set
            {
                personaData = value;
            }
        }

        public void TryRecoverOrSpawnOnGround()
        {
            PersonaData.CopyPawn(this.pawn, PersonaData.sourceStack);
            if (Rand.Chance(0.25f))
            {
                StatsRecord_Notify_ColonistKilled_Patch.disableKilledEffect = true;
                var corticalStack = ThingMaker.MakeThing(this.def.spawnThingOnRemoved) as CorticalStack;
                corticalStack.PersonaData.CopyPawn(pawn, corticalStack.def);
                GenPlace.TryPlaceThing(corticalStack, pawn.Position, pawn.Map, ThingPlaceMode.Near);
            }
            pawn.health.RemoveHediff(this);
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
                PersonaData.CopyPawn(this.pawn, PersonaData.sourceStack);
            }
            base.Notify_PawnDied();
        }

        public override void Notify_PawnKilled()
        {
            if (!PersonaData.ContainsInnerPersona)
            {
                PersonaData.CopyPawn(this.pawn, PersonaData.sourceStack);
            }
            base.Notify_PawnKilled();
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            if (!this.pawn.Dead)
            {
                this.pawn.Kill(null);
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref personaData, "personaData");
        }
    }
}