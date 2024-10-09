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
    [HotSwappable]
	public class Building_SleeveCasket : Building_Bed
    {
        public int runningOutFuelInTicks;
        public bool isRunningOutFuel;
        protected CompPowerTrader compPower;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.compPower = GetComp<CompPowerTrader>();
        }

        public Graphic topGraphic;
        public Graphic TopGraphic
        {
            get
            {
                if (topGraphic == null)
                {
                    topGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Building/Furniture/Bed/SleeveCasketTop",
                        ShaderDatabase.CutoutComplex, this.def.graphicData.drawSize, Color.white);
                }
                return topGraphic;
            }
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			base.DrawAt(drawLoc, flip);
            Vector3 vector = this.DrawPos;
            vector.y += 3f;
            TopGraphic.Draw(vector, this.Rotation, this);
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
            if (this.CurOccupants.Any())
            {
                compPower.PowerOutput = 0f - compPower.Props.PowerConsumption;
            }
            else
            {
                compPower.PowerOutput = 0f - compPower.Props.idlePowerDraw;
            }
            foreach (var occupant in this.CurOccupants)
            {
                if (occupant.HasHediff(AC_DefOf.AC_CryptoStasis) is false)
                {
                    occupant.health.AddHediff(AC_DefOf.AC_CryptoStasis);
                }
                if (occupant.needs.food.CurLevel < occupant.needs.food.MaxLevel)
                {
                    occupant.needs.food.CurLevel += 0.001f;
                }
                if (ModCompatibility.DubsBadHygieneActive)
                {
                    ModCompatibility.FillThirstNeed(occupant, 0.001f);
                    ModCompatibility.FillHygieneNeed(occupant, 0.001f);
                    ModCompatibility.FillBladderNeed(occupant, 0.001f);
                }
                var malnutrition = occupant.GetHediff(HediffDefOf.Malnutrition);
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

