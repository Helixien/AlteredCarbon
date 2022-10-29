using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
	public class Building_SleeveGrower : Building_Incubator
	{
		public bool innerPawnIsDead;
		public override int OpenTicks => 300;
		public Pawn InnerPawn => innerContainer.OfType<Pawn>().FirstOrDefault();
		protected override bool InnerThingIsDead => innerPawnIsDead;
		public override Thing InnerThing => innerContainer.FirstOrDefault();
		public override IEnumerable<Gizmo> GetGizmos()
		{
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			if (base.Faction == Faction.OfPlayer)
			{
				if (innerContainer.Count > 0 && incubatorState == IncubatorState.Growing)
				{
					Command_Action command_Action = new Command_Action
					{
						action = OrderToCancel,
						defaultLabel = "AC.CancelSleeveBodyGrowing".Translate(),
						defaultDesc = "AC.CancelSleeveBodyGrowingDesc".Translate(),
						hotKey = KeyBindingDefOf.Misc8,
						icon = ContentFinder<Texture2D>.Get("UI/Icons/CancelSleeve")
					};
					yield return command_Action;
				}

				if (InnerPawn == null || innerPawnIsDead)
				{
					Command_Action createSleeveBody = new Command_Action
					{
						action = new Action(CreateSleeve),
						defaultLabel = "AC.CreateSleeveBody".Translate(),
						defaultDesc = "AC.CreateSleeveBodyDesc".Translate(),
						hotKey = KeyBindingDefOf.Misc8,
						icon = ContentFinder<Texture2D>.Get("UI/Icons/CreateSleeve", true)
					};
					yield return createSleeveBody;

					Command_Action copySleeveBody = new Command_Action
					{
						action = new Action(CopyPawnBody),
						defaultLabel = "AC.CloneSleeve".Translate(),
						defaultDesc = "AC.CloneSleeveDesc".Translate(),
						hotKey = KeyBindingDefOf.Misc8,
						icon = ContentFinder<Texture2D>.Get("UI/Icons/CloneSleeve", true)
					};
					yield return copySleeveBody;


				}
				if (Prefs.DevMode && incubatorState == IncubatorState.Growing)
				{
					Command_Action command_Action = new Command_Action
					{
						defaultLabel = "Debug: Instant grow",
						action = InstantGrowth
					};
					yield return command_Action;
				}
			}
			yield break;
		}
		public Graphic fetus;
		public Graphic Fetus
		{
			get
			{
				if (fetus == null)
				{
					fetus = GraphicDatabase.Get<Graphic_Single>("Things/Pawn/Humanlike/Vat/Fetus", ShaderDatabase.CutoutSkin, Vector3.one, InnerPawn.story.SkinColor);
				}
				return fetus;
			}
		}

		public Graphic child;
		public Graphic Child
		{
			get
			{
				if (child == null)
				{
					child = GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Humanlike/Vat/Child", ShaderDatabase.CutoutSkin, Vector3.one, InnerPawn.story.SkinColor);
				}
				return child;
			}
		}

		public Graphic fetus_dead;
		public Graphic Fetus_Dead
		{
			get
			{
				if (fetus_dead == null)
				{
					fetus_dead = GraphicDatabase.Get<Graphic_Single>("Things/Pawn/Humanlike/Vat/Fetus_Dead", ShaderDatabase.CutoutSkin, Vector3.one, InnerPawn.story.SkinColor);
				}
				return fetus_dead;
			}
		}
		public Graphic child_dead;
		public Graphic Child_Dead
		{
			get
			{
				if (child_dead == null)
				{
					child_dead = GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Humanlike/Vat/Child_Dead", ShaderDatabase.CutoutSkin, Vector3.one, InnerPawn.story.SkinColor);
				}
				return child_dead;
			}
		}

		public Graphic adult_dead;
		public Graphic Adult_Dead
		{
			get
			{
				if (adult_dead == null)
				{
					adult_dead = GraphicDatabase.Get<Graphic_Multi>("Things/Pawn/Humanlike/Vat/Adult_Dead", ShaderDatabase.CutoutSkin, Vector3.one, InnerPawn.story.SkinColor);
				}
				return adult_dead;
			}
		}

		public override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			base.DrawAt(drawLoc, flip);
			if (incubatorState != IncubatorState.ToBeActivated && InnerPawn != null)
			{
				Vector3 newPos = drawLoc;
				newPos.z += 0.2f;
				newPos.y += 1;
				float growthValue = GrowthProgress;
				if (!innerPawnIsDead)
				{
					if (growthValue < 0.33f)
					{
						Fetus.Draw(newPos, Rot4.North, this);
					}
					else if (growthValue < 0.66f)
					{
						Child.Draw(newPos, Rot4.North, this);
					}
					else
					{
						InnerPawn.Rotation = Rotation;
						InnerPawn.DrawAt(newPos, flip);
					}
				}
				else if (innerPawnIsDead)
				{
					if (growthValue < 0.33f)
					{
						Fetus_Dead.Draw(newPos, Rot4.North, this);
					}
					else if (growthValue < 0.66f)
					{
						Child_Dead.Draw(newPos, Rot4.North, this);
					}
					else
					{
						Adult_Dead.Draw(newPos, Rot4.North, this);
					}
				}
			}
			base.Comps_PostDraw();
		}
		public void ResetGraphics()
		{
			fetus = null;
			child = null;
			fetus_dead = null;
			child_dead = null;
			adult_dead = null;
		}

		public void OrderToCancel()
		{
			incubatorState = IncubatorState.ToBeCanceled;
		}
		public void CancelGrowing()
		{
			incubatorState = IncubatorState.Inactive;
			Reset();
			Pawn innerPawn = InnerPawn;
			if (innerPawn != null)
			{
				innerPawn.Destroy();
				innerContainer.Remove(innerPawn);
			}
			ResetGraphics();
		}
		public void CreateSleeve()
		{
			if (Find.Targeter.IsTargeting)
			{
				Find.Targeter.StopTargeting();
			}
			Find.WindowStack.Add(new Window_SleeveCustomization(this));
		}
		public static TargetingParameters ForPawn()
		{
			TargetingParameters targetingParameters = new TargetingParameters
			{
				canTargetPawns = true,
				canTargetItems = true,
				mapObjectTargetsMustBeAutoAttackable = false,
				validator = (TargetInfo x) => (x.Thing is Pawn pawn && pawn.RaceProps.Humanlike) || (x.Thing is Corpse corpse && corpse.InnerPawn.RaceProps.Humanlike)
			};
			return targetingParameters;
		}
		public void CopyPawnBody()
		{
			Find.Targeter.BeginTargeting(ForPawn(), delegate (LocalTargetInfo x)
			{
				if (x.Thing is Pawn pawn)
				{
					Find.WindowStack.Add(new Window_SleeveCustomization(this, pawn));
				}
				else if (x.Thing is Corpse corpse)
				{
					Find.WindowStack.Add(new Window_SleeveCustomization(this, corpse.InnerPawn));
				}
			});
		}

		public void StartGrowth(Pawn newSleeve, int totalTicksToGrow, int totalGrowthCost)
		{
			ResetGraphics();
			Pawn pawn = InnerPawn;
			if (pawn != null)
			{
				innerContainer.Remove(InnerPawn);
				pawn.Destroy(DestroyMode.Vanish);
			}
			TryAcceptThing(newSleeve);
			Reset();
			this.totalTicksToGrow = totalTicksToGrow;
			this.totalGrowthCost = totalGrowthCost;
			incubatorState = IncubatorState.ToBeActivated;
		}

		private void Reset()
		{
			curTicksToGrow = 0;
			innerPawnIsDead = false;
			runningOutPowerInTicks = 0;
			runningOutFuelInTicks = 0;
			totalGrowthCost = 0;
			totalTicksToGrow = 0;
		}
		public void InstantGrowth()
		{
			curTicksToGrow = totalTicksToGrow;
			FinishGrowth();
		}
		public void FinishGrowth()
		{
			incubatorState = IncubatorState.Inactive;
			if (AlteredCarbonManager.Instance.emptySleeves == null)
			{
				AlteredCarbonManager.Instance.emptySleeves = new HashSet<Pawn>();
			}

			AlteredCarbonManager.Instance.emptySleeves.Add(InnerPawn);
			Messages.Message("AC.FinishedGrowingSleeve".Translate(), this, MessageTypeDefOf.CautionInput);
		}

		public override void Open()
		{
			Pawn sleeve = InnerPawn;
			base.Open();
			Pawn pawn = OpeningPawn();
			if (pawn != null)
			{
				Building_Bed bed = RestUtility.FindBedFor(sleeve, pawn, checkSocialProperness: false);
				if (bed == null)
				{
					bed = RestUtility.FindBedFor(sleeve, pawn, checkSocialProperness: false, ignoreOtherReservations: true);
				}
				if (bed != null)
				{
					Job job = JobMaker.MakeJob(JobDefOf.Rescue, sleeve, bed);
					job.count = 1;
					pawn.jobs.jobQueue.EnqueueFirst(job, JobTag.Misc);
				}
			}
		}
		private Pawn OpeningPawn()
		{
			foreach (ReservationManager.Reservation reserv in Map.reservationManager.ReservationsReadOnly)
			{
				if (reserv.Target == this)
				{
					if (reserv.Claimant.CurJob != null && reserv.Claimant.CurJob.def == JobDefOf.Open && reserv.Claimant.CurJob.targetA.Thing == this)
					{
						return reserv.Claimant;
					}
				}
			}
			return null;
		}

		public override bool HasAnyContents => InnerPawn != null;
		public override void EjectContents()
		{
			base.EjectContents();
			ThingDef filth_Slime = ThingDefOf.Filth_Slime;
			foreach (Thing thing in innerContainer)
			{
				if (thing is Pawn pawn)
				{
					PawnComponentsUtility.AddComponentsForSpawn(pawn);
					pawn.filth.GainFilth(filth_Slime);
					pawn.health.AddHediff(AC_DefOf.VFEU_EmptySleeve);
				}
			}
			Pawn openingPawn = OpeningPawn();
			if (openingPawn != null)
			{
				innerContainer.TryDrop(InnerThing, openingPawn.Position, Map, ThingPlaceMode.Direct, 1, out Thing resultingThing);
			}
			else
			{
				innerContainer.TryDrop(InnerThing, ThingPlaceMode.Near, 1, out Thing resultingThing);
			}
			ResetGraphics();
		}

		public override void KillInnerThing()
		{
			innerPawnIsDead = true;
		}
		public override void Tick()
		{
			base.Tick();
			if (InnerPawn == null && curTicksToGrow > 0)
			{
				curTicksToGrow = 0;
			}
			if (InnerPawn != null)
			{
				if (incubatorState == IncubatorState.Growing || incubatorState == IncubatorState.ToBeCanceled)
				{
					if (compRefuelable.HasFuel && powerTrader.PowerOn)
					{
						if (runningOutPowerInTicks > 0)
						{
							runningOutPowerInTicks = 0;
						}

						float fuelCost = totalGrowthCost / totalTicksToGrow;
						compRefuelable.ConsumeFuel(fuelCost);
						if (curTicksToGrow < totalTicksToGrow)
						{
							curTicksToGrow++;
						}
						else
						{
							FinishGrowth();
						}
					}

					if (!powerTrader.PowerOn && runningOutPowerInTicks < 60000)
					{
						runningOutPowerInTicks++;
					}
					else if (runningOutPowerInTicks >= 60000)
					{
						incubatorState = IncubatorState.Inactive;
						KillInnerThing();
						Messages.Message("AC.SleeveInIncubatorIsDead".Translate(), this, MessageTypeDefOf.NegativeEvent);
					}
					else if (!compRefuelable.HasFuel && runningOutFuelInTicks < 60000)
					{
						runningOutFuelInTicks++;
					}
					else if (runningOutFuelInTicks >= 60000)
					{
						incubatorState = IncubatorState.Inactive;
						KillInnerThing();
						Messages.Message("AC.SleeveInIncubatorIsDead".Translate(), this, MessageTypeDefOf.NegativeEvent);
					}
				}

				if (!powerTrader.PowerOn && !isRunningOutPower)
				{
					Messages.Message("AC.IsRunningOutPower".Translate(), this, MessageTypeDefOf.NegativeEvent);
					isRunningOutPower = true;
				}
				if (!compRefuelable.HasFuel && !isRunningOutFuel)
				{
					Messages.Message("AC.IsRunningOutFuel".Translate(), this, MessageTypeDefOf.NegativeEvent);
					isRunningOutFuel = true;
				}
			}
		}

		public override string GetInspectString()
		{
			StringBuilder sb = new StringBuilder(base.GetInspectString() + "\n");
			if (runningOutFuelInTicks > runningOutPowerInTicks)
			{
				sb.AppendLine("AC.FuelSleeveWillBeLostIn".Translate((60000 - runningOutFuelInTicks).ToStringTicksToPeriod()));
			}
			else if (runningOutPowerInTicks > runningOutFuelInTicks)
			{
				sb.AppendLine("AC.PowerSleeveWillBeLostIn".Translate((60000 - runningOutPowerInTicks).ToStringTicksToPeriod()));
			}
			return sb.ToString().TrimEndNewlines();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref innerPawnIsDead, "innerPawnIsDead", false, true);
		}
	}
}