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
    public class Building_SleeveGestator : Building_Incubator, IThingHolderWithDrawnPawn
    {
        public Xenogerm xenogermToConsume;
        public Corpse corpseToRepurpose;
        public BodyTypeDef targetBodyType;
        public int targetSleeveAge;
        public bool innerPawnIsDead;
        public float initialRotTime;
        public override int OpenTicks => 300;
        public override int TotalTicksToGrow => GetTicksToGrow(base.TotalTicksToGrow);
        public override float TotalGrowthCost => GetGrowCost(base.TotalGrowthCost);

        public Pawn InnerPawn => innerContainer.OfType<Pawn>().FirstOrDefault() ?? innerContainer.OfType<Corpse>().FirstOrDefault()?.InnerPawn;
        protected override bool InnerThingIsDead => innerPawnIsDead;
        private CompAffectedByFacilities compAffectedByFacilities;

        public override Thing InnerThing => InnerPawn;
        public Thing StoredPawnOrCorpse => (Thing)innerContainer.OfType<Pawn>().FirstOrDefault() ?? innerContainer.OfType<Corpse>().FirstOrDefault();
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
        public Graphic topGraphic;
        public Graphic TopGraphic
        {
            get
            {
                if (topGraphic == null)
                {
                    topGraphic = GraphicDatabase.Get<Graphic_Single>("Things/Building/Misc/SleeveGastator/SleeveGastatorTop", ShaderDatabase.CutoutComplex, this.def.graphicData.drawSize, Color.white);
                }
                return topGraphic;
            }
        }
        public Vector3 PawnDrawOffset
        {
            get
            {
                var drawOffset = CompBiosculpterPod.FloatingOffset(Find.TickManager.TicksGame);
                drawOffset.z += 0.1f;
                return drawOffset;
            }
        }

        public int GetTicksToGrow(int baseValue)
        {
            return (int)(baseValue);
        }

        public float GetGrowCost(float baseValue)
        {
            return baseValue;
        }

        private float FacilityMultiplier(ThingDef facility, float baseMultiplier)
        {
            return (1f - (GetFacilities(facility).Count * baseMultiplier));
        }

        private List<Thing> GetFacilities(ThingDef facilityDef)
        {
            return compAffectedByFacilities.LinkedFacilitiesListForReading.Where(x => x.def == facilityDef && x.TryGetComp<CompPowerTrader>().PowerOn).ToList();
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.compAffectedByFacilities = this.GetComp<CompAffectedByFacilities>();
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            var innerPawn = InnerPawn;
            if (incubatorState != IncubatorState.ToBeActivated && innerPawn != null)
            {
                Vector3 newPos = drawLoc;
                newPos.z += 0.2f;
                newPos.y += 1;
                float growthProgress = GrowthProgress;
                if (growthProgress < 0.05f && innerPawn.Dead is false)
                {
                    Vector2 drawSize = Vector2.one * Mathf.Lerp(0.3f, 0.7f, growthProgress / 0.05f);
                    if (growthProgress < 0.02f)
                    {
                        DrawFetus(FetusEarlyStage, drawSize, newPos + PawnDrawOffset, this.Rotation == Rot4.East);
                    }
                    else
                    {
                        DrawFetus(FetusLateStage, drawSize, newPos + PawnDrawOffset, this.Rotation == Rot4.East);
                    }
                }
                else
                {
                    try
                    {
                        innerPawn.Rotation = Rotation;
                        innerPawn.DynamicDrawPhaseAt(DrawPhase.Draw, newPos + PawnDrawOffset, flip);
                    }
                    catch (Exception e)
                    {
                        Log.Error("Error drawing " + innerPawn + " - " + e.ToString());
                    }
                }
            }

            Vector3 vector = this.DrawPos + Altitudes.AltIncVect;
            vector.y += 3 + ((this.Map.Size.z - this.Position.z) * 0.0001f);
            TopGraphic.Draw(vector, Rot4.North, this);
        }

        public void DrawFetus(Graphic fetusGraphic, Vector2 drawSize, Vector3 loc, bool flip)
        {
            fetusGraphic.drawSize = drawSize;
            Mesh mesh = flip ? MeshPool.GridPlaneFlip(drawSize) : MeshPool.GridPlane(drawSize);
            Quaternion quat = fetusGraphic.QuatFromRot(Rot4.North);
            loc += fetusGraphic.DrawOffset(Rot4.North);
            Material mat = fetusGraphic.MatAt(Rot4.North, null);
            fetusGraphic.DrawMeshInt(mesh, loc, quat, mat);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            if (Faction == Faction.OfPlayer)
            {
                if (innerContainer.Count > 0 && incubatorState == IncubatorState.Growing && corpseToRepurpose is null
                    && (InnerPawn is null || InnerPawn?.Dead is false))
                {
                    Command_Action cancelSleeveBody = new Command_Action
                    {
                        action = OrderToCancel,
                        defaultLabel = "AC.CancelSleeveBodyGrowing".Translate(),
                        defaultDesc = "AC.CancelSleeveBodyGrowingDesc".Translate(),
                        hotKey = KeyBindingDefOf.Misc8,
                        activateSound = SoundDefOf.Tick_Tiny,
                        icon = ContentFinder<Texture2D>.Get("UI/Gizmos/CancelSleeve")
                    };
                    yield return cancelSleeveBody;
                }
                var pawn = InnerPawn;
                bool isOccuptied = pawn != null;

                Command_Action createSleeveBody = new Command_Action
                {
                    action = CreateSleeve,
                    defaultLabel = "AC.CreateSleeveBody".Translate(),
                    defaultDesc = "AC.CreateSleeveBodyDesc".Translate(),
                    activateSound = SoundDefOf.Tick_Tiny,
                    hotKey = KeyBindingDefOf.Misc8,
                    icon = ContentFinder<Texture2D>.Get("UI/Gizmos/CreateSleeve", true)
                };
                createSleeveBody.LockBehindReseach(this.def.researchPrerequisites);
                yield return createSleeveBody;

                Command_Action copySleeveBody = new Command_Action
                {
                    action = CopyPawnBody,
                    defaultLabel = "AC.CloneSleeve".Translate(),
                    defaultDesc = "AC.CloneSleeveDesc".Translate(),
                    hotKey = KeyBindingDefOf.Misc8,
                    activateSound = SoundDefOf.Tick_Tiny,
                    icon = ContentFinder<Texture2D>.Get("UI/Gizmos/CloneSleeve", true)
                };
                copySleeveBody.LockBehindReseach(this.def.researchPrerequisites);
                yield return copySleeveBody;

                if (powerTrader.PowerOn is false)
                {
                    createSleeveBody.Disable("NoPower".Translate().CapitalizeFirst());
                    copySleeveBody.Disable("NoPower".Translate().CapitalizeFirst());
                }

                if (isOccuptied)
                {
                    createSleeveBody.Disable("AC.SleeveGrowerIsOccupied".Translate());
                    copySleeveBody.Disable("AC.SleeveGrowerIsOccupied".Translate());
                }

                var repurposeCorpse = new Command_Action
                {
                    action = RepurposeCorpse,
                    defaultLabel = "AC.RepurposeCorpse".Translate(),
                    defaultDesc = "AC.RepurposeCorpseDesc".Translate(),
                    hotKey = KeyBindingDefOf.Misc8,
                    activateSound = SoundDefOf.Tick_Tiny,
                    icon = ContentFinder<Texture2D>.Get("UI/Gizmos/ReuseSleeve", true)
                };

                if (powerTrader.PowerOn is false)
                {
                    repurposeCorpse.Disable("NoPower".Translate().CapitalizeFirst());
                }
                if (isOccuptied)
                {
                    repurposeCorpse.Disable("AC.SleeveGrowerIsOccupied".Translate());
                }
                repurposeCorpse.LockBehindReseach(this.def.researchPrerequisites);
                yield return repurposeCorpse;

                if (this.corpseToRepurpose != null || incubatorState != IncubatorState.ToBeCanceled
                    && this.InnerPawn != null && this.InnerPawn.Dead)
                {
                    Command_Action cancelRepurposeCorpse = new Command_Action
                    {
                        action = delegate
                        {
                            OrderToCancel();
                            this.corpseToRepurpose = null;
                        },
                        defaultLabel = "AC.CancelRepurposingCorpse".Translate(),
                        defaultDesc = "AC.CancelRepurposingCorpseDesc".Translate(),
                        hotKey = KeyBindingDefOf.Misc8,
                        activateSound = SoundDefOf.Tick_Tiny,
                        icon = UIHelper.CancelIcon
                    };
                    yield return cancelRepurposeCorpse;
                }

                if (isOccuptied)
                {
                    yield return ContainingSelectionUtility.CreateSelectGizmo("CommandSelectStoredThing", "CommandSelectContainedPawnDesc", StoredPawnOrCorpse);
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

        public void CancelBodyRepurposing()
        {
            incubatorState = IncubatorState.Inactive;
            Reset();
            EjectContents();
        }
        public void CreateSleeve()
        {
            if (Find.Targeter.IsTargeting)
            {
                Find.Targeter.StopTargeting();
            }
            Find.WindowStack.Add(new Window_SleeveCustomization(this));
        }
        public TargetingParameters ForPawnToClone()
        {
            TargetingParameters targetingParameters = new TargetingParameters
            {
                canTargetPawns = true,
                canTargetItems = true,
                mapObjectTargetsMustBeAutoAttackable = false,
                validator = (TargetInfo x) => (x.Thing is Pawn pawn && BodyCanBeReused(pawn))
                || (x.Thing is Corpse corpse && BodyCanBeReused(corpse.InnerPawn))
            };
            return targetingParameters;
        }

        private bool BodyCanBeReused(Pawn pawn)
        {
            return pawn.RaceProps.Humanlike && pawn.DevelopmentalStage == DevelopmentalStage.Adult 
                && (!ModCompatibility.AlienRacesIsActive || ModCompatibility.GetPermittedRaces().Contains(pawn.def)) 
                && pawn.IsAndroid() is false && pawn.IsMutant is false;
        }

        public void CopyPawnBody()
        {
            Find.Targeter.BeginTargeting(ForPawnToClone(), delegate (LocalTargetInfo x)
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

        public TargetingParameters ForCorpseToRepurpose()
        {
            TargetingParameters targetingParameters = new TargetingParameters
            {
                canTargetItems = true,
                mapObjectTargetsMustBeAutoAttackable = false,
                validator = (TargetInfo x) => x.Thing is Corpse corpse && corpse.GetRotStage() != RotStage.Dessicated 
                && BodyCanBeReused(corpse.InnerPawn)
            };
            return targetingParameters;
        }

        public void RepurposeCorpse()
        {
            Find.Targeter.BeginTargeting(ForCorpseToRepurpose(), delegate (LocalTargetInfo x)
            {
                var corpse = x.Thing as Corpse;
                if (corpse.InnerPawn.HasNeuralStack())
                {
                    Messages.Message("AC.NeuralStackMustBeRemovedFirst".Translate(), MessageTypeDefOf.CautionInput);
                }
                else if (corpse.InnerPawn.health.hediffSet.HasHead is false)
                {
                    Messages.Message("AC.CannotRepurposeCorspeWithoutHead".Translate(), MessageTypeDefOf.CautionInput);
                }
                else 
                {
                    Reset();
                    this.corpseToRepurpose = corpse;
                    incubatorState = IncubatorState.ToBeActivated;
                    corpse.SetForbidden(false, false);
                }
            });
        }

        public void PutCorpseForRepurposing(Corpse corpse)
        {
            Reset();
            var clone = AC_Utils.ClonePawn(corpse.InnerPawn);
            clone.health.healthState = PawnHealthState.Dead;
            Find.WorldPawns.AddPawn(clone);
            initialRotTime = corpse.TryGetComp<CompRottable>().RotProgress;
            corpse.innerContainer.Clear();
            corpse.InnerPawn = clone;
            TryAcceptThing(corpse);
            var totalTicksToGrow = 60000f;
            var totalCostToGrow = 25f;
            var missingParts = InnerPawn.GetMissingParts();
            totalTicksToGrow += missingParts.Sum(x => x.Part.def.GetMaxHealth(InnerPawn)) * 10;
            totalCostToGrow += missingParts.Sum(x => x.Part.def.GetMaxHealth(InnerPawn)) / 10;
            var injuries = InnerPawn.health.hediffSet.hediffs.OfType<Hediff_Injury>();
            totalTicksToGrow += injuries.Sum(x => x.Severity) * 5;
            var scars = InnerPawn.health.hediffSet.hediffs.Where(x => x.IsPermanent());
            totalTicksToGrow += scars.Count() * 650;
            totalCostToGrow += scars.Count();
            var brainTraumas = InnerPawn.health.hediffSet.hediffs.Where(x => x.def == AC_DefOf.AC_BrainTrauma || x.def == AC_DefOf.TraumaSavant);
            totalTicksToGrow += brainTraumas.Count() * 2500;
            totalCostToGrow += brainTraumas.Count() * 10;
            if (corpse.GetRotStage() == RotStage.Rotting)
            {
                totalTicksToGrow *= 2f;
                totalCostToGrow *= 2f;
            }
            StartRepurpose((int)totalTicksToGrow, totalCostToGrow);
        }

        public void StartRepurpose(int totalTicksToGrow, float totalGrowthCost)
        {
            this.TotalTicksToGrow = totalTicksToGrow;
            this.TotalGrowthCost = totalGrowthCost;
            InnerPawn.Rotation = Rot4.South;
        }
        public void StartGrowth(Pawn newSleeve, Xenogerm xenogerm, int totalTicksToGrow, int totalGrowthCost, int targetAge)
        {
            Reset();
            targetBodyType = newSleeve.story.bodyType;
            targetSleeveAge = targetAge;
            xenogermToConsume = xenogerm;
            TryAcceptThing(newSleeve);
            this.TotalTicksToGrow = totalTicksToGrow;
            this.TotalGrowthCost = totalGrowthCost;
            incubatorState = IncubatorState.ToBeActivated;
            InnerPawn.Rotation = Rot4.South;
        }

        private void Reset()
        {
            curTicksToGrow = 0;
            innerPawnIsDead = false;
            runningOutPowerInTicks = 0;
            runningOutFuelInTicks = 0;
            TotalGrowthCost = 0;
            TotalTicksToGrow = 0;
            targetBodyType = null;
            xenogermToConsume = null; 
            corpseToRepurpose = null;
            initialRotTime = 0;
        }
        public void AddGrowth()
        {
            curTicksToGrow += TotalTicksToGrow / 100;
        }

        public void FinishGrowth()
        {
            curTicksToGrow = TotalTicksToGrow;
            AdjustAge();
            incubatorState = IncubatorState.Inactive;
            var pawn = InnerPawn;
            var injuries = pawn.health.hediffSet.hediffs.OfType<Hediff_Injury>();
            var missingParts = pawn.GetMissingParts();
            var scars = pawn.health.hediffSet.hediffs.Where(x => x.IsPermanent());
            var brainTraumas = pawn.health.hediffSet.hediffs.Where(x => x.def == AC_DefOf.AC_BrainTrauma || x.def == AC_DefOf.TraumaSavant);
            var toRemove = injuries.Cast<Hediff>().Concat(missingParts).Concat(scars).Concat(brainTraumas).ToList();
            var bloodlossHediff = pawn.GetHediff(HediffDefOf.BloodLoss);
            if (bloodlossHediff != null)
            {
                toRemove.Add(bloodlossHediff);
            }
            foreach (var h in toRemove)
            {
                pawn.health.RemoveHediff(h);
            }

            if (pawn.Dead)
            {
                pawn.CreateEmptySleeve(keepNaturalAbilities: true, keepPsycastAbilities: true);
                ResurrectionUtility.TryResurrect(pawn);
                pawn.DeSpawn();
                innerContainer.TryAddOrTransfer(pawn);
                if (Find.WorldPawns.Contains(pawn))
                {
                    Find.WorldPawns.RemovePawn(pawn);
                }
            }
            pawn.health.healthState = PawnHealthState.Mobile;
            Messages.Message("AC.FinishedGrowingSleeve".Translate(), this, MessageTypeDefOf.CautionInput);
            pawn.RefreshGraphic();
        }

        public override void Open()
        {
            Pawn sleeve = InnerPawn;
            base.Open();
            Pawn pawn = OpeningPawn();
            if (pawn != null && sleeve.Dead is false)
            {
                AC_Utils.AddTakeEmptySleeveJob(pawn, sleeve, false);
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

        public float HeldPawnBodyAngle => Rotation.AsAngle;

        public PawnPosture HeldPawnPosture => PawnPosture.Standing;

        public override void EjectContents()
        {
            var pawn = InnerPawn;
            base.EjectContents();
            PawnComponentsUtility.AddComponentsForSpawn(pawn);
            pawn.filth.GainFilth(ThingDefOf.Filth_Slime);
            pawn.MakeEmptySleeve();
            innerContainer.TryDrop(StoredPawnOrCorpse, this.InteractionCell, Map, ThingPlaceMode.Direct, 1, out Thing resultingThing);
        }

        public override void KillInnerThing()
        {
            innerPawnIsDead = true;
        }
        public override void Tick()
        {
            base.Tick();
            if (InnerPawn == null)
            {
                if (curTicksToGrow > 0)
                {
                    curTicksToGrow = 0;
                }
                powerTrader.PowerOutput = 0f - powerTrader.Props.idlePowerDraw;
            }
            else
            {
                powerTrader.PowerOutput = 0f - powerTrader.Props.PowerConsumption;
                var corpse = StoredPawnOrCorpse as Corpse;
                if (corpse != null)
                {
                    corpse.TryGetComp<CompRottable>().RotProgress--;
                }

                if (incubatorState == IncubatorState.Growing || incubatorState == IncubatorState.ToBeCanceled)
                {
                    bool hasFuel = compRefuelable.HasFuel;
                    bool hasPower = powerTrader.PowerOn;
                    if (hasFuel)
                    {
                        isRunningOutFuel = false;
                        runningOutFuelInTicks = 0;
                    }
                    if (hasPower)
                    {
                        isRunningOutPower = false;
                        runningOutPowerInTicks = 0;
                    }

                    if (hasFuel && hasPower)
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
            float fuelCost = TotalGrowthCost / TotalTicksToGrow;
            compRefuelable.ConsumeFuel(fuelCost);
            if (curTicksToGrow < TotalTicksToGrow)
            {
                curTicksToGrow++;
                DoWorkEffects();
                AdjustAge();
                AdjustHealth();
            }
            else
            {
                FinishGrowth();
            }
        }

        private void DoWorkEffects()
        {
            if (this.IsHashIntervalTick(132))
            {
                var offset = new Vector3(0, 1, -0.5f);
                MoteMaker.MakeStaticMote(DrawPos + offset, MapHeld, AC_DefOf.AC_Mote_VatGlow, 1.6f);
            }

            if (bubbleEffecter == null || Rand.Chance(0.01f))
            {
                var offset = new Vector3(0, 0, -0.5f);
                bubbleEffecter = AC_DefOf.AC_Vat_Bubbles.Spawn(this.TrueCenter().ToIntVec3(), MapHeld, offset);
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


        private void AdjustHealth()
        {
            var growthProgress = GrowthProgress;
            var remainingTime = TotalTicksToGrow - curTicksToGrow;
            var corpse = innerContainer.OfType<Corpse>().FirstOrDefault();
            if (corpse != null)
            {
                var compRot = corpse.TryGetComp<CompRottable>();
                compRot.RotProgress = initialRotTime * (1f - growthProgress);
            }
            var injuries = InnerPawn.health.hediffSet.hediffs.OfType<Hediff_Injury>();
            var allSeverities = injuries.Sum(x => x.Severity);
            if (remainingTime > 0)
            {
                var severityToSubtract = allSeverities / (float)remainingTime;
                if (severityToSubtract > 0)
                {
                    while (injuries.Any() && severityToSubtract > 0)
                    {
                        var injury = injuries.FirstOrDefault();
                        var toSubtract = Mathf.Min(injury.Severity, severityToSubtract);
                        severityToSubtract -= toSubtract;
                        injury.Severity -= toSubtract;
                        if (injury.Severity <= 0)
                        {
                            InnerPawn.health.RemoveHediff(injury);
                        }
                    }
                }

                var missingParts = InnerPawn.GetMissingParts();
                if (missingParts.Any())
                {
                    var hashInterval = (int)(remainingTime / (float)missingParts.Count());
                    if (InnerPawn.IsHashIntervalTick(hashInterval))
                    {
                        var toRemove = missingParts.FirstOrDefault(x => missingParts.Any(y => x.part.GetDirectChildParts().Contains(y.Part)) is false);
                        InnerPawn.health.RemoveHediff(toRemove);
                    }
                }

                var scars = InnerPawn.health.hediffSet.hediffs.Where(x => x.IsPermanent());
                if (scars.Any())
                {
                    var hashInterval = (int)(remainingTime / (float)scars.Count());
                    if (InnerPawn.IsHashIntervalTick(hashInterval))
                    {
                        var toRemove = scars.RandomElement();
                        InnerPawn.health.RemoveHediff(toRemove);
                    }
                }
                var brainTraumas = InnerPawn.health.hediffSet.hediffs.Where(x => x.def == AC_DefOf.AC_BrainTrauma || x.def == AC_DefOf.TraumaSavant);
                if (brainTraumas.Any())
                {
                    var hashInterval = (int)(remainingTime / (float)scars.Count());
                    if (InnerPawn.IsHashIntervalTick(hashInterval))
                    {
                        var toRemove = brainTraumas.RandomElement();
                        InnerPawn.health.RemoveHediff(toRemove);
                    }
                }
            }
            if (Find.TickManager.TicksGame % 60 == 0)
            {
                InnerPawn.RefreshGraphic();
            }
        }

        private void AdjustAge()
        {
            if (InnerPawn.Dead) return;
            var growthProgress = GrowthProgress;
            InnerPawn.ageTracker.AgeBiologicalTicks = (long)(Mathf.FloorToInt(targetSleeveAge * 3600000f) * growthProgress);
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
            Scribe_Values.Look(ref initialRotTime, "initialRotTime");
            Scribe_References.Look(ref xenogermToConsume, "xenogermToConsume");
            Scribe_References.Look(ref corpseToRepurpose, "corpseToRepurpose");
            Scribe_Defs.Look(ref targetBodyType, "targetBodyType");
            Scribe_Values.Look(ref targetSleeveAge, "targetSleeveAge", 18);
        }
    }
}