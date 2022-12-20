using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace AlteredCarbon
{
    [HotSwappable]
    public class Building_SleeveGrower : Building_Incubator, IThingHolderWithDrawnPawn
    {
        public Xenogerm xenogermToConsume;

        public bool innerPawnIsDead;
        public override int OpenTicks => 300;
        public Pawn InnerPawn => innerContainer.OfType<Pawn>().FirstOrDefault();
        protected override bool InnerThingIsDead => innerPawnIsDead;

        public BodyTypeDef targetBodyType;
        public override Thing InnerThing => innerContainer.FirstOrDefault();

        [Unsaved(false)]
        private Effecter bubbleEffecter;

        [Unsaved(false)]
        private Graphic fetusEarlyStageGraphic;

        [Unsaved(false)]
        private Graphic fetusLateStageGraphic;

        [Unsaved(false)]
        private Sustainer sustainerWorking;

        private Graphic FetusEarlyStage
        {
            get
            {
                if (fetusEarlyStageGraphic == null)
                {
                    fetusEarlyStageGraphic = GraphicDatabase.Get<Graphic_Single>("Other/VatGrownFetus_EarlyStage", 
                        ShaderDatabase.Cutout, Vector2.one, InnerPawn.story.SkinColor);
                }
                return fetusEarlyStageGraphic;
            }
        }

        private Graphic FetusLateStage
        {
            get
            {
                if (fetusLateStageGraphic == null)
                {
                    fetusLateStageGraphic = GraphicDatabase.Get<Graphic_Single>("Other/VatGrownFetus_LateStage",
                        ShaderDatabase.Cutout, Vector2.one, InnerPawn.story.SkinColor);
                }
                return fetusLateStageGraphic;
            }
        }

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
                    Command_Action cancelSleeveBody = new Command_Action
                    {
                        action = OrderToCancel,
                        defaultLabel = "AC.CancelSleeveBodyGrowing".Translate(),
                        defaultDesc = "AC.CancelSleeveBodyGrowingDesc".Translate(),
                        hotKey = KeyBindingDefOf.Misc8,
                        activateSound = SoundDefOf.Tick_Tiny,
                        icon = ContentFinder<Texture2D>.Get("UI/Icons/CancelSleeve")
                    };
                    yield return cancelSleeveBody;
                }

                if (InnerPawn == null || innerPawnIsDead)
                {
                    Command_Action createSleeveBody = new Command_Action
                    {
                        action = CreateSleeve,
                        defaultLabel = "AC.CreateSleeveBody".Translate(),
                        defaultDesc = "AC.CreateSleeveBodyDesc".Translate(),
                        activateSound = SoundDefOf.Tick_Tiny,
                        hotKey = KeyBindingDefOf.Misc8,
                        icon = ContentFinder<Texture2D>.Get("UI/Icons/CreateSleeve", true)
                    };
                    yield return createSleeveBody;

                    Command_Action copySleeveBody = new Command_Action
                    {
                        action = CopyPawnBody,
                        defaultLabel = "AC.CloneSleeve".Translate(),
                        defaultDesc = "AC.CloneSleeveDesc".Translate(),
                        hotKey = KeyBindingDefOf.Misc8,
                        activateSound = SoundDefOf.Tick_Tiny,
                        icon = ContentFinder<Texture2D>.Get("UI/Icons/CloneSleeve", true)
                    };
                    yield return copySleeveBody;


                }
                if (Prefs.DevMode && incubatorState == IncubatorState.Growing)
                {
                    Command_Action command_Action = new Command_Action
                    {
                        defaultLabel = "Debug: Instant grow",
                        action = FinishGrowth
                    };
                    yield return command_Action;

                    command_Action = new Command_Action
                    {
                        defaultLabel = "Debug: grow +1%",
                        action = AddGrowth
                    };
                    yield return command_Action;
                }
            }
            yield break;
        }
        public Vector3 PawnDrawOffset => CompBiosculpterPod.FloatingOffset(Find.TickManager.TicksGame);

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            if (incubatorState != IncubatorState.ToBeActivated && InnerPawn != null)
            {
                Vector3 newPos = drawLoc;
                newPos.z += 0.2f;
                newPos.y += 1;
                float growthProgress = GrowthProgress;
                if (growthProgress < 0.05f)
                {
                    Vector2 drawSize = Vector2.one * Mathf.Lerp(0.3f, 0.7f, growthProgress / 0.05f);
                    if (growthProgress < 0.02f)
                    {
                        FetusEarlyStage.drawSize = drawSize;
                        FetusEarlyStage.DrawFromDef(newPos + PawnDrawOffset, Rot4.North, null);
                    }
                    else
                    {
                        FetusLateStage.drawSize = drawSize;
                        FetusLateStage.DrawFromDef(newPos + PawnDrawOffset, Rot4.North, null);
                    }
                }
                else
                {
                    InnerPawn.Rotation = Rotation;
                    InnerPawn.DrawAt(newPos + PawnDrawOffset, flip);
                }
            }
            base.Comps_PostDraw();
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

        public void StartGrowth(Pawn newSleeve, Xenogerm xenogerm, int totalTicksToGrow, int totalGrowthCost)
        {
            targetBodyType = newSleeve.story.bodyType;
            xenogermToConsume = xenogerm;
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
            InnerPawn.Rotation = Rot4.South;
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
        public void AddGrowth()
        {
            curTicksToGrow += totalTicksToGrow / 100;
        }

        public void FinishGrowth()
        {
            curTicksToGrow = totalTicksToGrow;
            AdjustAge();
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

        public float HeldPawnDrawPos_Y => DrawPos.y + 3f / 74f;

        public float HeldPawnBodyAngle => base.Rotation.AsAngle;

        public PawnPosture HeldPawnPosture => PawnPosture.Standing;

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
                        DoWork();
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

        private void DoWork()
        {
            if (runningOutPowerInTicks > 0)
            {
                runningOutPowerInTicks = 0;
            }
            float fuelCost = totalGrowthCost / totalTicksToGrow;
            compRefuelable.ConsumeFuel(fuelCost);
            if (curTicksToGrow < totalTicksToGrow)
            {
                if (this.IsHashIntervalTick(132))
                {
                    var offset = new Vector3(0, 1, -0.5f);
                    MoteMaker.MakeStaticMote(DrawPos + offset, base.MapHeld, AC_DefOf.VFEU_Mote_VatGlow, 1.6f);
                }

                if (bubbleEffecter == null || Rand.Chance(0.01f))
                {
                    var offset = new Vector3(0, 0, -0.5f);
                    bubbleEffecter = AC_DefOf.VFEU_Vat_Bubbles.Spawn(this.TrueCenter().ToIntVec3(), base.MapHeld, offset);
                }
                bubbleEffecter.EffectTick(this, this);

                if (sustainerWorking == null || sustainerWorking.Ended)
                {
                    sustainerWorking = SoundDefOf.GrowthVat_Working.TrySpawnSustainer(SoundInfo.InMap(this, MaintenanceType.PerTick));
                }
                else
                {
                    sustainerWorking.Maintain();
                }
                curTicksToGrow++;
                AdjustAge();
            }
            else
            {
                FinishGrowth();
            }
        }

        private BodyTypeDef GetActualBodyType()
        {
            if (InnerPawn.DevelopmentalStage.Juvenile())
            {
                if (InnerPawn.DevelopmentalStage == DevelopmentalStage.Baby)
                {
                    return BodyTypeDefOf.Baby;
                }
                return BodyTypeDefOf.Child;
            }
            return targetBodyType;
        }

        private void AdjustAge()
        {
            var growthProgress = GrowthProgress;
            var lastAdultAge = InnerPawn.RaceProps.lifeStageAges.LastOrDefault((LifeStageAge lifeStageAge) => lifeStageAge.def.developmentalStage.Adult())?.minAge ?? 0f;
            InnerPawn.ageTracker.AgeBiologicalTicks = (long)(Mathf.FloorToInt(lastAdultAge * 3600000f) * growthProgress);
            InnerPawn.ageTracker.AgeChronologicalTicks = InnerPawn.ageTracker.AgeBiologicalTicks;
            var bodyType = GetActualBodyType();
            if (bodyType != InnerPawn.story.bodyType)
            {
                if (targetBodyType is null)
                {
                    targetBodyType = InnerPawn.story.bodyType;
                }
                InnerPawn.story.bodyType = bodyType;
                InnerPawn.RefreshGraphic();
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
            Scribe_References.Look(ref xenogermToConsume, "xenogermToConsume");
            Scribe_Defs.Look(ref targetBodyType, "targetBodyType");
        }
    }
}