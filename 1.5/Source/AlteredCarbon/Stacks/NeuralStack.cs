using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    [HotSwappable]
    public class NeuralStack : ThingWithNeuralData
    {
        public override Graphic Graphic
        {
            get
            {
                NeuralData neuralData = NeuralData;
                if (neuralData.ContainsData)
                {
                    if (neuralData.guestStatusInt == GuestStatus.Slave)
                    {
                        return GetStackGraphic(ref slaveGraphic, ref slaveGraphicData, 
                            "Things/Item/ArchoStacks/SlaveArchoStack", "Things/Item/NeuralStacks/SlaveStack");
                    }
                    else if (neuralData.faction == Faction.OfPlayer)
                    {
                        return GetStackGraphic(ref friendlyGraphic, ref friendlyGraphicData,
                            "Things/Item/ArchoStacks/FriendlyArchoStack", "Things/Item/NeuralStacks/FriendlyStack");
                    }
                    else if (neuralData.faction is null || !neuralData.faction.HostileTo(Faction.OfPlayer))
                    {
                        return GetStackGraphic(ref strangerGraphic, ref strangerGraphicData,
                            "Things/Item/ArchoStacks/NeutralArchoStack", "Things/Item/NeuralStacks/NeutralStack");
                    }
                    else
                    {
                        return GetStackGraphic(ref hostileGraphic, ref hostileGraphicData,
                            "Things/Item/ArchoStacks/HostileArchoStack", "Things/Item/NeuralStacks/HostileStack");
                    }
                }
                else
                {
                    return base.Graphic;
                }
            }
        }

        private Graphic GetStackGraphic(ref Graphic graphic, ref GraphicData graphicData, string archotechStackTexPath, string stackTexPath)
        {
            if (graphic is null)
            {
                if (graphicData is null)
                {
                    var path = this.IsArchotechStack ? archotechStackTexPath : stackTexPath;
                    graphicData = GetGraphicDataWithOtherPath(path);
                }
                graphic = graphicData.GraphicColoredFor(this);
            }
            return graphic;
        }

        public override string LabelNoCount
        {
            get
            {
                var label = base.LabelNoCount;
                if (this.IsActiveStack)
                {
                    label += " (" + this.NeuralData.PawnNameColored.ToStringSafe() + ")";
                }
                return label;
            }
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            NeuralData.AppendInfoStack(stringBuilder);
            stringBuilder.Append(base.GetInspectString());
            return stringBuilder.ToString().TrimEndNewlines();
        }

        public override void Tick()
        {
            base.Tick();
            if (this.Spawned && this.IsArchotechStack)
            {
                var edifice = this.Position.GetEdifice(Map);
                if (edifice != null && this.Position.Walkable(Map) is false)
                {
                    var map = this.Map;
                    var pos = this.Position;
                    this.DeSpawn();
                    GenPlace.TryPlaceThing(this, pos, map, ThingPlaceMode.Near);
                    FleckMaker.Static(this.Position, this.Map, AC_DefOf.PsycastAreaEffect, 3f);
                }

                if (hediff?.skipAbility != null && hediff.pawn.health.hediffSet.hediffs.Contains(hediff))
                {
                    hediff.pawn.positionInt = Position;
                    hediff.pawn.mapIndexOrState = (sbyte)Find.Maps.IndexOf(Map);
                }
                else
                {
                    casterPawn = NeuralData.DummyPawn;
                    casterPawn.positionInt = Position;
                    casterPawn.mapIndexOrState = (sbyte)Find.Maps.IndexOf(Map);
                    PawnComponentsUtility.AddComponentsForSpawn(casterPawn);
                    PawnComponentsUtility.AddAndRemoveDynamicComponents(casterPawn);
                    hediff = casterPawn.health.AddHediff(AC_DefOf.AC_ArchotechStack) as Hediff_NeuralStack;
                }
                casterPawn.jobs.JobTrackerTick();
            }
        }

        public bool IsActiveStack => this.def == AC_DefOf.AC_ActiveNeuralStack || this.def == AC_DefOf.AC_ActiveArchotechStack;
        public bool IsArchotechStack => this.def == AC_DefOf.AC_EmptyArchotechStack || this.def == AC_DefOf.AC_ActiveArchotechStack;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            try
            {
                if (!respawningAfterLoad && !NeuralData.ContainsData && IsActiveStack)
                {
                    GenerateNeural();
                    NeuralData.stackGroupID = AlteredCarbonManager.Instance.GetStackGroupID(this);
                    AlteredCarbonManager.Instance.RegisterStack(this);
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception spawning " + this + ": " + ex);
            }
            if (def == AC_DefOf.AC_ActiveNeuralStack && stackCount != 1)
            {
                stackCount = 1;
            }
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public TargetingParameters ForPawn()
        {
            TargetingParameters targetingParameters = new TargetingParameters
            {
                canTargetPawns = true,
                validator = (TargetInfo x) => x.Thing is Pawn pawn
            };
            return targetingParameters;
        }

        private Pawn casterPawn;
        private Hediff_NeuralStack hediff;

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }
            if (AC_Utils.stackRecipesByDef.TryGetValue(this.def, out var installInfo))
            {
                var installStack = new Command_Action
                {
                    defaultLabel = installInfo.installLabel,
                    defaultDesc = installInfo.installDesc,
                    activateSound = SoundDefOf.Tick_Tiny,
                    icon = installInfo.installIcon,
                    action = delegate ()
                    {
                        Find.Targeter.BeginTargeting(ForPawn(), delegate (LocalTargetInfo x)
                        {
                            if (AC_Utils.CanImplantStackTo(AC_Utils.stackRecipesByDef[this.def].recipe.addsHediff, x.Pawn, this, true))
                            {
                                InstallStackRecipe(x.Pawn, installInfo.recipe);
                            }
                        });
                    }
                };
                yield return installStack;
                if (IsArchotechStack && IsActiveStack)
                {
                    hediff.NeuralData = NeuralData;
                    hediff.skipAbility.archoStackForAbility = this;
                    var gizmo = hediff.skipAbility.GetGizmo() as VFECore.Abilities.Command_Ability;
                    var archoSkip = new VFECore.Abilities.Command_Ability(hediff.pawn, hediff.skipAbility)
                    {
                        defaultLabel = gizmo.defaultLabel,
                        defaultDesc = gizmo.defaultDesc,
                        activateSound = gizmo.activateSound,
                        icon = gizmo.icon,
                        action = delegate ()
                        {
                            Find.Targeter.BeginTargeting(ForPawn(), delegate (LocalTargetInfo x)
                            {
                                if (AC_Utils.CanImplantStackTo(AC_Utils.stackRecipesByDef[this.def].recipe.addsHediff,
                                    x.Pawn, this, true) && Ability_ArchotechStackSkip.Validate(x, true))
                                {
                                    hediff.skipAbility.CreateCastJob(x);
                                }
                            });
                        }
                    };
                    yield return archoSkip;
                }
                if (DebugSettings.godMode)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "DEV: Instant implant",
                        action = delegate
                        {
                            Find.Targeter.BeginTargeting(new TargetingParameters
                            {
                                canTargetHumans = true,
                                canTargetPawns = true,
                                canTargetAnimals = false,
                                canTargetCorpses = false,
                                canTargetMechs = false,
                                validator = (TargetInfo x) => x.Thing is Pawn pawn

                            }, delegate (LocalTargetInfo x)
                            {
                                if (AC_Utils.CanImplantStackTo(installInfo.recipe.addsHediff, x.Pawn, this, true))
                                {
                                    Recipe_InstallNeuralStack.ApplyNeuralStack(installInfo.recipe, x.Pawn, x.Pawn.GetNeck(), this);
                                    this.Destroy();
                                }
                            });
                        }
                    };
                }
            }

            if (this.IsActiveStack)
            {
                yield return new Command_Toggle
                {
                    defaultLabel = "AC.AutoLoad".Translate(),
                    defaultDesc = "AC.AutoLoadDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Gizmos/AutoLoadStack"),
                    isActive = () => autoLoad,
                    toggleAction = delegate
                    {
                        autoLoad = !autoLoad;
                    }
                };
            }
        }

        public void InstallStackRecipe(Pawn medPawn, RecipeDef recipe)
        {
            if (medPawn.HasNeuralStack(out var stackHediff) && (stackHediff.def == recipe.addsHediff || stackHediff.def == AC_DefOf.AC_ArchotechStack))
            {
                if (stackHediff.def != recipe.addsHediff)
                {
                    Messages.Message("AC.PawnStackCannotDowngrade".Translate(medPawn.Named("PAWN")), MessageTypeDefOf.CautionInput);
                }
                else if (stackHediff.def == AC_DefOf.AC_NeuralStack)
                {
                    Messages.Message("AC.PawnAlreadyHasStack".Translate(medPawn.Named("PAWN")), MessageTypeDefOf.CautionInput);
                }
                else
                {
                    Messages.Message("AC.PawnAlreadyHasArchotechStack".Translate(medPawn.Named("PAWN")), MessageTypeDefOf.CautionInput);
                }
            }
            else if (recipe.Worker.GetPartsToApplyOn(medPawn, recipe).FirstOrDefault() is null)
            {
                Messages.Message("AC.CannotInstallStackNeckIsMissingOrAnotherImplantInstalled".Translate(), MessageTypeDefOf.CautionInput);
            }
            else
            {
                medPawn.BillStack.Bills.RemoveAll(x => x is Bill_InstallStack);
                if (medPawn.IsAndroid())
                {
                    recipe = ModCompatibility.GetRecipeForAndroid(recipe);
                }
                Bill_InstallStack bill_Medical = new Bill_InstallStack(recipe, this);
                medPawn.BillStack.AddBill(bill_Medical);
                bill_Medical.Part = recipe.Worker.GetPartsToApplyOn(medPawn, recipe).First();
                var compForbiddable = this.GetComp<CompForbiddable>();
                if (compForbiddable.Forbidden)
                {
                    compForbiddable.Forbidden = false;
                }
                if (recipe.conceptLearned != null)
                {
                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(recipe.conceptLearned, KnowledgeAmount.Total);
                }
                Map map = medPawn.Map;
                if (!map.mapPawns.FreeColonists.Any((Pawn col) => recipe.PawnSatisfiesSkillRequirements(col)))
                {
                    Bill.CreateNoPawnsWithSkillDialog(recipe);
                }
                if (!medPawn.InBed() && medPawn.RaceProps.IsFlesh)
                {
                    if (medPawn.RaceProps.Humanlike)
                    {
                        if (!map.listerBuildings.allBuildingsColonist.Any((Building x) => x is Building_Bed && RestUtility.CanUseBedEver(medPawn, x.def) && ((Building_Bed)x).Medical))
                        {
                            Messages.Message("MessageNoMedicalBeds".Translate(), medPawn, MessageTypeDefOf.CautionInput, historical: false);
                        }
                    }
                    else if (!map.listerBuildings.allBuildingsColonist.Any((Building x) => x is Building_Bed && RestUtility.CanUseBedEver(medPawn, x.def)))
                    {
                        Messages.Message("MessageNoAnimalBeds".Translate(), medPawn, MessageTypeDefOf.CautionInput, historical: false);
                    }
                }
                if (medPawn.Faction != null && !medPawn.Faction.Hidden && !medPawn.Faction.HostileTo(Faction.OfPlayer) && recipe.Worker.IsViolationOnPawn(medPawn, bill_Medical.Part, Faction.OfPlayer))
                {
                    Messages.Message("MessageMedicalOperationWillAngerFaction".Translate(medPawn.HomeFaction), medPawn, MessageTypeDefOf.CautionInput, historical: false);
                }
                recipe.Worker.CheckForWarnings(medPawn);
            }
        }

        public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostApplyDamage(dinfo, totalDamageDealt);
            if (Destroyed)
            {
                KillInnerPawn();
            }
        }

        public void DestroyWithConfirmation()
        {
            Find.WindowStack.Add(new Dialog_MessageBox("AC.DestroyStackConfirmation".Translate(),
                    "No".Translate(), null,
                    "Yes".Translate(), delegate ()
            {
                Destroy();
            }, null, false, null, null));
        }

        public bool dontKillThePawn = false;
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (this.IsArchotechStack && allowDestroyNonDestroyable is false)
            {
                return;
            }
            base.Destroy(mode);
            if (NeuralData.ContainsData && dontKillThePawn is false)
            {
                //if (IsArchotechStack is false)
                //{
                //    NeuralData.TryQueueAutoRestoration();
                //}
                KillInnerPawn();
            }
        }

        public void KillInnerPawn(bool affectFactionRelationship = false, Pawn affecter = null)
        {
            if (NeuralData.ContainsData)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer));
                NeuralData.OverwritePawn(pawn, def.GetModExtension<StackSavingOptionsModExtension>());
                if (affectFactionRelationship)
                {
                    NeuralData.faction.TryAffectGoodwillWith(affecter.Faction, NeuralData.faction.GoodwillToMakeHostile(affecter.Faction), canSendMessage: true, reason: AC_DefOf.AC_ErasedStackEvent);
                }
                if (NeuralData.isFactionLeader)
                {
                    pawn.Faction.leader = pawn;
                }
                try
                {
                    pawn.DisableKillEffects();
                    pawn.Kill(null);
                    pawn.EnableKillEffects();
                }
                catch { }

            }
        }
        public void EmptyStack(Pawn affecter, bool affectFactionRelationship = false)
        {
            Thing newStack = ThingMaker.MakeThing(this.GetEmptyStackVariant());
            GenPlace.TryPlaceThing(newStack, affecter.Position, affecter.Map, ThingPlaceMode.Near);
            if (NeuralData.hostPawn != null)
            {
                AlteredCarbonManager.Instance.StacksIndex.Remove(NeuralData.PawnID);
            }
            KillInnerPawn(affectFactionRelationship, affecter);
            foreach (Pawn otherPawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists)
            {
                otherPawn?.needs?.mood?.thoughts.memories.TryGainMemory(AC_DefOf.AC_ErasedStack);
            }
            Destroy();
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref casterPawn, "casterPawn");
            Scribe_References.Look(ref hediff, "hediff");
        }
    }
}