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
    public class PersonaStack : ThingWithComps
    {
        public bool autoLoad = true;

        public PersonaData personaDataRewritten;

        private PersonaData personaData;
        public PersonaData PersonaData
        {
            get
            {
                if (personaData is null)
                {
                    personaData = new PersonaData();
                }
                return personaData;
            }
            set
            {
                personaData = value;
            }
        }

        private GraphicData hostileGraphicData;
        private GraphicData friendlyGraphicData;
        private GraphicData strangerGraphicData;
        private GraphicData slaveGraphicData;

        private Graphic hostileGraphic;
        private Graphic friendlyGraphic;
        private Graphic strangerGraphic;
        private Graphic slaveGraphic;

        public override Graphic Graphic
        {
            get
            {
                PersonaData personaData = PersonaData;
                if (personaData.ContainsInnerPersona)
                {
                    if (personaData.guestStatusInt == GuestStatus.Slave)
                    {
                        return GetStackGraphic(ref slaveGraphic, ref slaveGraphicData, 
                            "Things/Item/ArchotechStacks/SlaveArchoStack", "Things/Item/Stacks/SlaveStack");
                    }
                    else if (personaData.faction == Faction.OfPlayer)
                    {
                        return GetStackGraphic(ref friendlyGraphic, ref friendlyGraphicData,
                            "Things/Item/ArchotechStacks/FriendlyArchoStack", "Things/Item/Stacks/FriendlyStack");
                    }
                    else if (personaData.faction is null || !personaData.faction.HostileTo(Faction.OfPlayer))
                    {
                        return GetStackGraphic(ref strangerGraphic, ref strangerGraphicData,
                            "Things/Item/ArchotechStacks/NeutralArchoStack", "Things/Item/Stacks/NeutralStack");
                    }
                    else
                    {
                        return GetStackGraphic(ref hostileGraphic, ref hostileGraphicData,
                            "Things/Item/ArchotechStacks/HostileArchoStack", "Things/Item/Stacks/HostileStack");
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

        private GraphicData GetGraphicDataWithOtherPath(string texPath)
        {
            return new GraphicData
            {
                texPath = texPath,
                graphicClass = def.graphicData.graphicClass,
                shadowData = def.graphicData.shadowData,
                shaderType = def.graphicData.shaderType,
                shaderParameters = def.graphicData.shaderParameters,
                onGroundRandomRotateAngle = def.graphicData.onGroundRandomRotateAngle,
                linkType = def.graphicData.linkType,
                linkFlags = def.graphicData.linkFlags,
                flipExtraRotation = def.graphicData.flipExtraRotation,
                drawSize = def.graphicData.drawSize,
                drawRotated = !def.graphicData.drawRotated,
                drawOffsetWest = def.graphicData.drawOffsetWest,
                drawOffsetSouth = def.graphicData.drawOffsetSouth,
                drawOffsetNorth = def.graphicData.drawOffsetNorth,
                drawOffsetEast = def.graphicData.drawOffsetEast,
                drawOffset = def.graphicData.drawOffset,
                damageData = def.graphicData.damageData,
                colorTwo = def.graphicData.colorTwo,
                color = def.graphicData.color,
                allowFlip = def.graphicData.allowFlip
            };
        }

        //public override string Label
        //{
        //    get
        //    {
        //        var label = base.Label;
        //        if (this.IsFilledStack)
        //        {
        //            label += " (" + this.PersonaData.PawnNameColored.ToStringSafe() + ")";
        //        }
        //        return label;
        //    }
        //}

        public override string LabelNoCount
        {
            get
            {
                var label = base.LabelNoCount;
                if (this.IsFilledStack)
                {
                    label += " (" + this.PersonaData.PawnNameColored.ToStringSafe() + ")";
                }
                return label;
            }
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
            }
        }
        public bool IsFilledStack => this.def == AC_DefOf.AC_FilledPersonaStack || this.def == AC_DefOf.AC_FilledArchotechStack;
        public bool IsArchotechStack => this.def == AC_DefOf.AC_EmptyArchotechStack || this.def == AC_DefOf.AC_FilledArchotechStack;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            try
            {
                if (!respawningAfterLoad && !PersonaData.ContainsInnerPersona && IsFilledStack)
                {
                    PawnKindDef pawnKind = DefDatabase<PawnKindDef>.AllDefs.Where(x => x.RaceProps.Humanlike).RandomElement();
                    Faction faction = Find.FactionManager.AllFactions.Where(x => x.def.humanlikeFaction).RandomElement();
                    Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnKind, faction));
                    PersonaData.CopyFromPawn(pawn, this.def, copyRaceGenderInfo: true);
                    PersonaData.OverwritePawn(pawn, this.def.GetModExtension<StackSavingOptionsModExtension>());
                    PersonaData.dummyPawn = pawn;
                    PersonaData.stackGroupID = AlteredCarbonManager.Instance.GetStackGroupID(this);
                    AlteredCarbonManager.Instance.RegisterStack(this);
                    if (LookTargets_Patch.targets.TryGetValue(pawn, out List<LookTargets> targets))
                    {
                        foreach (LookTargets target in targets)
                        {
                            target.targets.Remove(pawn);
                            target.targets.Add(this);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Exception spawning " + this + ": " + ex);
            }
            if (def == AC_DefOf.AC_FilledPersonaStack && stackCount != 1)
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
                                    Recipe_InstallPersonaStack.ApplyPersonaStack(installInfo.recipe, x.Pawn, x.Pawn.GetNeck(), this);
                                    this.Destroy();
                                }
                            });
                        }
                    };
                }
            }
            if (this.IsFilledStack)
            {
                yield return new Command_Toggle
                {
                    defaultLabel = "AC.AutoLoad".Translate(),
                    defaultDesc = "AC.AutoLoadDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Icons/AutoLoadStack"),
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
            if (medPawn.HasPersonaStack(out var stackHediff) && (stackHediff.def == recipe.addsHediff || stackHediff.def == AC_DefOf.AC_ArchotechStack))
            {
                if (stackHediff.def != recipe.addsHediff)
                {
                    Messages.Message("AC.PawnStackCannotDowngrade".Translate(medPawn.Named("PAWN")), MessageTypeDefOf.CautionInput);
                }
                else if (stackHediff.def == AC_DefOf.AC_PersonaStack)
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

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (PersonaData.ContainsInnerPersona)
            {
                if (PersonaData.faction != null)
                {
                    stringBuilder.AppendLineTagged("AC.Faction".Translate() + ": " + PersonaData.faction.NameColored);
                }
                if (ModCompatibility.AlienRacesIsActive && PersonaData.OriginalRace != null)
                {
                    stringBuilder.AppendLineTagged("AC.Race".Translate() + ": " + PersonaData.OriginalRace.LabelCap);
                }
                if (PersonaData.OriginalXenotypeName != null)
                {
                    stringBuilder.AppendLineTagged("AC.Xenotype".Translate() + ": " + PersonaData.OriginalXenotypeName);
                }
                else if (PersonaData.OriginalXenotypeDef != null)
                {
                    stringBuilder.AppendLineTagged("AC.Xenotype".Translate() + ": " + PersonaData.OriginalXenotypeDef.LabelCap);
                }

                if (PersonaData.childhood != null)
                {
                    stringBuilder.Append("AC.Childhood".Translate() + ": " + PersonaData.childhood.title.CapitalizeFirst() + "\n");
                }

                if (PersonaData.adulthood != null)
                {
                    stringBuilder.Append("AC.Adulthood".Translate() + ": " + PersonaData.adulthood.title.CapitalizeFirst() + "\n");
                }
                stringBuilder.Append("AC.AgeChronologicalTicks".Translate() + ": " + (int)(PersonaData.ageChronologicalTicks / 3600000) + "\n");
                if (PersonaData.stackDegradation > 0)
                {
                    stringBuilder.AppendLineTagged("AC.StackDegradation".Translate((TaggedString)(PersonaData.stackDegradation.ToStringPercent().Colorize(Color.red))));
                }
            }
            stringBuilder.Append(base.GetInspectString());
            return stringBuilder.ToString().TrimEndNewlines();
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
            if (PersonaData.ContainsInnerPersona && !dontKillThePawn)
            {
                KillInnerPawn();
            }
        }

        public void KillInnerPawn(bool affectFactionRelationship = false, Pawn affecter = null)
        {
            if (PersonaData.ContainsInnerPersona)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(PawnKindDefOf.Colonist, Faction.OfPlayer));
                PersonaData.OverwritePawn(pawn, def.GetModExtension<StackSavingOptionsModExtension>());
                if (affectFactionRelationship)
                {
                    PersonaData.faction.TryAffectGoodwillWith(affecter.Faction, -70, canSendMessage: true);
                    QuestUtility.SendQuestTargetSignals(pawn.questTags, "SurgeryViolation", pawn.Named("SUBJECT"));
                }
                if (PersonaData.isFactionLeader)
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
            if (PersonaData.hostPawn != null)
            {
                AlteredCarbonManager.Instance.StacksIndex.Remove(PersonaData.PawnID);
            }
            KillInnerPawn(affectFactionRelationship, affecter);
            Destroy();
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref personaData, "personaData");
            Scribe_Deep.Look(ref personaDataRewritten, "personaDataRewritten");
            Scribe_Values.Look(ref autoLoad, "autoLoad", true);
        }
    }
}