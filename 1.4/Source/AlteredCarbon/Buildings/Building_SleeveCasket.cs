using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace AlteredCarbon
{
	public class Building_SleeveCasket : Building_Bed
    {
        public int runningOutFuelInTicks;
        public bool isRunningOutFuel;
        protected CompRefuelable compRefuelable;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.compRefuelable = base.GetComp<CompRefuelable>();
        }
        public override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			base.DrawAt(drawLoc, flip);
			base.Comps_PostDraw();
		}
        public override string GetInspectString()
        {
            this.Medical = false;
            this.def.building.bed_humanlike = false;
            var sb = new StringBuilder(base.GetInspectString() + "\n");
            if (isRunningOutFuel)
            {
                if (runningOutFuelInTicks > GenDate.TicksPerDay)
                {
                    sb.AppendLine("AC.FuelSleeveIsDeteriorating".Translate());
                }
                else
                {
                    sb.AppendLine("AC.FuelSleeveWillBeDeteriorated".Translate((60000 - runningOutFuelInTicks).ToStringTicksToPeriod()));
                }
            }
            this.Medical = true;
            this.def.building.bed_humanlike = true;
            return sb.ToString().TrimEndNewlines();
        }

        public override void Tick()
        {
            base.Tick();
            compRefuelable.ConsumeFuel(2 / 60000f);
            if (!compRefuelable.HasFuel && !isRunningOutFuel)
            {
                Messages.Message("AC.IsRunningOutFuel".Translate(), this, MessageTypeDefOf.NegativeEvent);
                this.isRunningOutFuel = true;
            }
            if (!compRefuelable.HasFuel)
            {
                runningOutFuelInTicks++;
            }
            else
            {
                runningOutFuelInTicks = 0;
                this.isRunningOutFuel = false;
            }
			if (runningOutFuelInTicks > 60000 && this.IsHashIntervalTick(2500))
			{
                foreach (var occupant in this.CurOccupants)
				{
                    occupant.TakeDamage(new DamageInfo(AC_DefOf.VFEU_Deterioration, 5, armorPenetration: 1));
				}
            }

			if (this.compRefuelable.HasFuel)
			{
                foreach (var occupant in this.CurOccupants)
                {
                    if (occupant.IsEmptySleeve())
                    {
                        if (occupant.needs.food.CurLevel < 1f)
                        {
                            occupant.needs.food.CurLevel += 0.001f;
                        }
                        if (ModCompatibility.DubsBadHygieneActive)
                        {
                            ModCompatibility.FillThirstNeed(occupant, 0.001f);
                            ModCompatibility.FillHygieneNeed(occupant, 0.001f);
                            ModCompatibility.FillBladderNeed(occupant, 0.001f);
                        }
                        var malnutrition = occupant.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Malnutrition);
                        if (malnutrition != null)
                        {
                            occupant.health.RemoveHediff(malnutrition);
                        }
                        var dehydration = occupant.health.hediffSet.hediffs.FirstOrDefault(x => x.def.defName == "DBHDehydration");
                        if (dehydration != null)
                        {
                            occupant.health.RemoveHediff(dehydration);
                        }
                    }
                }
            }


        }
        public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				if (gizmo is Command_Toggle toggle)
				{
					if (toggle.defaultLabel == "CommandBedSetForPrisonersLabel".Translate() || toggle.defaultLabel == "CommandBedSetAsMedicalLabel".Translate())
                    {
						continue;
					}
				}
				else if (gizmo is Command_SetBedOwnerType)
                {
					continue;
                }
				yield return gizmo;
			}
		}

		public override void ExposeData()
		{
			base.ExposeData();
            Scribe_Values.Look(ref this.isRunningOutFuel, "isRunningOutFuel", false, true);
            Scribe_Values.Look(ref this.runningOutFuelInTicks, "runningOutFuelInTicks", 0, true);
        }
	}
}

