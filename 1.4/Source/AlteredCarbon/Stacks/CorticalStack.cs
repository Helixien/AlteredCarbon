using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    public class CorticalStack : ThingWithComps
    {
        public static HashSet<CorticalStack> corticalStacks = new HashSet<CorticalStack>();

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
        }

        private GraphicData hostileGraphicData;
        private GraphicData friendlyGraphicData;
        private GraphicData strangerGraphicData;

        private Graphic hostileGraphic;
        private Graphic friendlyGraphic;
        private Graphic strangerGraphic;
        public override Graphic Graphic
        {
            get
            {
                PersonaData personaData = PersonaData;
                if (personaData.ContainsInnerPersona)
                {
                    if (personaData.faction == Faction.OfPlayer)
                    {
                        if (friendlyGraphic is null)
                        {
                            if (friendlyGraphicData is null)
                            {
                                friendlyGraphicData = GetGraphicDataWithOtherPath("Things/Item/Stacks/FriendlyStack");
                            }
                            friendlyGraphic = friendlyGraphicData.GraphicColoredFor(this);
                        }
                        return friendlyGraphic;
                    }
                    else if (personaData.faction is null || !personaData.faction.HostileTo(Faction.OfPlayer))
                    {
                        if (strangerGraphic is null)
                        {
                            if (strangerGraphicData is null)
                            {
                                strangerGraphicData = GetGraphicDataWithOtherPath("Things/Item/Stacks/NeutralStack");
                            }
                            strangerGraphic = strangerGraphicData.GraphicColoredFor(this);
                        }
                        return strangerGraphic;
                    }
                    else
                    {
                        if (hostileGraphic is null)
                        {
                            if (hostileGraphicData is null)
                            {
                                hostileGraphicData = GetGraphicDataWithOtherPath("Things/Item/Stacks/HostileStack");
                            }
                            hostileGraphic = hostileGraphicData.GraphicColoredFor(this);
                        }
                        return hostileGraphic;
                    }
                }
                else
                {
                    return base.Graphic;
                }
            }
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

        public bool FilledStack => this.def == AC_DefOf.VFEU_FilledCorticalStack || this.def == AC_DefOf.AC_FilledArchoStack;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            corticalStacks.Add(this);
            try
            {

                if (!respawningAfterLoad && !PersonaData.ContainsInnerPersona && FilledStack)
                {
                    PawnKindDef pawnKind = DefDatabase<PawnKindDef>.AllDefs.Where(x => x.RaceProps.Humanlike).RandomElement();
                    Faction faction = Find.FactionManager.AllFactions.Where(x => x.def.humanlikeFaction).RandomElement();
                    Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnKind, faction));
                    PersonaData.CopyPawn(pawn, this.def);
                    PersonaData.gender = pawn.gender;
                    PersonaData.race = pawn.kindDef.race;
                    PersonaData.stackGroupID = AlteredCarbonManager.Instance.GetStackGroupID(this);
                    AlteredCarbonManager.Instance.RegisterStack(this);
                    AlteredCarbonManager.Instance.StacksIndex[pawn.thingIDNumber] = this;

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
            if (def == AC_DefOf.VFEU_FilledCorticalStack && stackCount != 1)
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
                validator = (TargetInfo x) => x.Thing is Pawn pawn && pawn.RaceProps.Humanlike && pawn.DevelopmentalStage == DevelopmentalStage.Adult
            };
            return targetingParameters;
        }
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }
            if (def == AC_DefOf.VFEU_FilledCorticalStack)
            {
                var installStack = new Command_Action
                {
                    defaultLabel = "AC.InstallStack".Translate(),
                    defaultDesc = "AC.InstallStackDesc".Translate(),
                    activateSound = SoundDefOf.Tick_Tiny,
                    icon = ContentFinder<Texture2D>.Get("UI/Icons/InstallStack"),
                    action = delegate ()
                    {
                        Find.Targeter.BeginTargeting(ForPawn(), delegate (LocalTargetInfo x)
                        {
                            InstallStackRecipe(x.Pawn);
                        });
                    }
                };
                yield return installStack;
            }
        }

        public void InstallStackRecipe(Pawn medPawn)
        {
            RecipeDef recipe = AC_DefOf.VFEU_InstallCorticalStack;
            if (medPawn.HasStack())
            {
                Messages.Message("AC.PawnAlreadyHasStack".Translate(medPawn.Named("PAWN")), MessageTypeDefOf.CautionInput);
            }
            else if (recipe.Worker.GetPartsToApplyOn(medPawn, recipe).FirstOrDefault() is null)
            {
                Messages.Message("AC.CannotInstallStackNeckIsMissingOrAnotherImplantInstalled".Translate(), MessageTypeDefOf.CautionInput);
            }
            else
            {
                medPawn.BillStack.Bills.RemoveAll(x => x is Bill_InstallStack);
                Bill_InstallStack bill_Medical = new Bill_InstallStack(recipe, this);
                medPawn.BillStack.AddBill(bill_Medical);
                bill_Medical.Part = recipe.Worker.GetPartsToApplyOn(medPawn, recipe).First();

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
                ThingDef minRequiredMedicine = GetMinRequiredMedicine(recipe);
                if (minRequiredMedicine != null && medPawn.playerSettings != null && !medPawn.playerSettings.medCare.AllowsMedicine(minRequiredMedicine))
                {
                    Messages.Message("MessageTooLowMedCare".Translate(minRequiredMedicine.label, medPawn.LabelShort, medPawn.playerSettings.medCare.GetLabel(), medPawn.Named("PAWN")), medPawn, MessageTypeDefOf.CautionInput, historical: false);
                }
                recipe.Worker.CheckForWarnings(medPawn);
            }

        }

        private static readonly List<ThingDef> tmpMedicineBestToWorst = new List<ThingDef>();
        private static ThingDef GetMinRequiredMedicine(RecipeDef recipe)
        {
            tmpMedicineBestToWorst.Clear();
            List<ThingDef> allDefsListForReading = DefDatabase<ThingDef>.AllDefsListForReading;
            for (int i = 0; i < allDefsListForReading.Count; i++)
            {
                if (allDefsListForReading[i].IsMedicine)
                {
                    tmpMedicineBestToWorst.Add(allDefsListForReading[i]);
                }
            }
            tmpMedicineBestToWorst.SortByDescending((ThingDef x) => x.GetStatValueAbstract(StatDefOf.MedicalPotency));
            ThingDef thingDef = null;
            for (int j = 0; j < recipe.ingredients.Count; j++)
            {
                ThingDef thingDef2 = null;
                for (int k = 0; k < tmpMedicineBestToWorst.Count; k++)
                {
                    if (recipe.ingredients[j].filter.Allows(tmpMedicineBestToWorst[k]))
                    {
                        thingDef2 = tmpMedicineBestToWorst[k];
                    }
                }
                if (thingDef2 != null && (thingDef == null || thingDef2.GetStatValueAbstract(StatDefOf.MedicalPotency) > thingDef.GetStatValueAbstract(StatDefOf.MedicalPotency)))
                {
                    thingDef = thingDef2;
                }
            }
            tmpMedicineBestToWorst.Clear();
            return thingDef;
        }
        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (PersonaData.ContainsInnerPersona)
            {
                stringBuilder.AppendLineTagged("AC.Name".Translate() + ": " + PersonaData.PawnNameColored);
                if (PersonaData.faction != null)
                {
                    stringBuilder.AppendLineTagged("AC.Faction".Translate() + ": " + PersonaData.faction.NameColored);
                }
                if (ModCompatibility.AlienRacesIsActive && PersonaData.race != null)
                {
                    stringBuilder.AppendLineTagged("AC.Race".Translate() + ": " + PersonaData.race.LabelCap);
                }
                if (PersonaData.childhood?.Length > 0
                    && DefDatabase<BackstoryDef>.GetNamedSilentFail(PersonaData.childhood) is BackstoryDef childhood)
                {
                    stringBuilder.Append("AC.Childhood".Translate() + ": " + childhood.title.CapitalizeFirst() + "\n");
                }

                if (PersonaData.adulthood?.Length > 0 && DefDatabase<BackstoryDef>.GetNamedSilentFail(PersonaData.adulthood) is BackstoryDef adulthood)
                {
                    stringBuilder.Append("AC.Adulthood".Translate() + ": " + adulthood.title.CapitalizeFirst() + "\n");
                }
                stringBuilder.Append("AC.AgeChronologicalTicks".Translate() + ": " + (int)(PersonaData.ageChronologicalTicks / 3600000) + "\n");
                stringBuilder.Append("Gender".Translate() + ": " + PersonaData.gender.GetLabel().CapitalizeFirst() + "\n");

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

        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            base.DeSpawn(mode);
            corticalStacks.Remove(this);
        }
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            corticalStacks.Remove(this);
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
                    pawn.DisableKilledEffects();
                    pawn.Kill(null);
                }
                catch { }

            }
        }
        public void EmptyStack(Pawn affecter, bool affectFactionRelationship = false)
        {
            Thing newStack = ThingMaker.MakeThing(this.GetEmptyStackVariant());
            GenPlace.TryPlaceThing(newStack, affecter.Position, affecter.Map, ThingPlaceMode.Near);
            AlteredCarbonManager.Instance.StacksIndex.Remove(PersonaData.pawnID);
            KillInnerPawn(affectFactionRelationship, affecter);
            Destroy();
        }


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref personaData, "personaData");
        }


    }
}