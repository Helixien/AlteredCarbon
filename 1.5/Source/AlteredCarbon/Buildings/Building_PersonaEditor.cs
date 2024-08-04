using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    [StaticConstructorOnStartup]
    public class Building_PersonaEditor : Building_WorkTable
    {
        public bool autoRestoreIsEnabled = true;
        public PersonaPrint personaPrintToRestore;
        public Building_PersonaMatrix ConnectedMatrix => CompAffectedByFacilities.LinkedFacilitiesListForReading.OfType<Building_PersonaMatrix>().FirstOrDefault();
        public bool Powered => (PowerComp as CompPowerTrader).PowerOn;
        private CompAffectedByFacilities _compAffectedByFacilities;
        public CompAffectedByFacilities CompAffectedByFacilities =>
            _compAffectedByFacilities ??= GetComp<CompAffectedByFacilities>();

        public bool HasPersonaPrintToRestore => GetPersonaPrintToRestore != null;

        public PersonaPrint GetPersonaPrintToRestore
        {
            get
            {
                if (personaPrintToRestore != null)
                {
                    return personaPrintToRestore;
                }
                if (autoRestoreIsEnabled)
                {
                    PersonaPrint personaPrint = null;
                    var matrix = ConnectedMatrix;
                    if (matrix != null)
                    {
                        foreach (var frame in matrix.StoredPersonaPrints)
                        {
                            if (frame.CanAutoRestorePawn)
                            {
                                personaPrint = frame;
                                break;
                            }
                        }
                    }

                    if (personaPrint != null)
                    {
                        return personaPrint;
                    }
                    foreach (var frame in Map.listerThings.AllThings.OfType<PersonaPrint>())
                    {
                        if (frame.CanAutoRestorePawn)
                        {
                            return frame;
                        }
                    }
                }
                return null;
            }
        }

        public class StackGizmoInfo
        {
            public string defaultLabel, defaultDesc, icon;
            public Action targetAction;
            public Action<LocalTargetInfo> action;
            public List<ResearchProjectDef> lockedProjects;
            public RecipeDef recipe;
            public string defaultLabelCancel, defaultDescCancel;
            public bool includeArchotechStack;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }
            foreach (var g in GetStackOperationGizmos(new StackGizmoInfo
            {
                defaultLabel = "AC.WipeStack".Translate(),
                defaultDesc = "AC.WipeStackDesc".Translate(),
                icon = "UI/Gizmos/WipeStack",
                targetAction = BeginTargetingForWipingStack,
                action = InstallWipeStackBill,
                lockedProjects = new List<ResearchProjectDef> { AC_DefOf.AC_NeuralEditing },
                recipe = AC_DefOf.AC_WipeFilledPersonaStack,
                defaultLabelCancel = "AC.CancelStackReset".Translate(),
                defaultDescCancel = "AC.CancelStackResetDesc".Translate()
            }))
            {
                yield return g;
            }

            foreach (var g in GetStackOperationGizmos(new StackGizmoInfo
            {
                defaultLabel = "AC.DuplicateStack".Translate(),
                defaultDesc = "AC.DuplicateStackDesc".Translate(),
                icon = "UI/Gizmos/DuplicateStack",
                targetAction = BeginTargetingForDuplicatingStack,
                action = InstallDuplicateStackBill,
                lockedProjects = new List<ResearchProjectDef> { AC_DefOf.AC_NeuralEditing },
                recipe = AC_DefOf.AC_DuplicatePersonaStack,
                defaultLabelCancel = "AC.CancelStackDuplication".Translate(),
                defaultDescCancel = "AC.CancelStackDuplicationDesc".Translate()
            }))
            {
                yield return g;
            }

            foreach (var g in GetStackOperationGizmos(new StackGizmoInfo
            {
                defaultLabel = "AC.EditStack".Translate(),
                defaultDesc = "AC.EditStackDesc".Translate(),
                icon = "UI/Gizmos/EditStack",
                targetAction = BeginTargetingForEditingStack,
                action = InstallEditBill,
                lockedProjects = AC_DefOf.AC_EditFilledPersonaStack.researchPrerequisites,
                recipe = AC_DefOf.AC_EditFilledPersonaStack,
                defaultLabelCancel = "AC.CancelStackEdit".Translate(),
                defaultDescCancel = "AC.CancelStackEditDesc".Translate(),
                includeArchotechStack = AC_Utils.editStacksSettings.enableArchostackEditing
            }))
            {
                yield return g;
            }

            var personaPrints = Map.listerThings.AllThings.OfType<PersonaPrint>().Where(x => x.PersonaData.ContainsPersona).ToList();
            if (ConnectedMatrix != null)
            {
                personaPrints.AddRange(ConnectedMatrix.StoredPersonaPrints);
            }
            var restoreFromPersonaPrint = new Command_Action
            {
                defaultLabel = "AC.RestoreFromPersonaPrint".Translate(),
                defaultDesc = "AC.RestoreFromPersonaPrintDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Gizmos/RestoreFromPersonaPrint"),
                activateSound = SoundDefOf.Tick_Tiny,
                action = delegate ()
                {
                    var floatList = new List<FloatMenuOption>();
                    foreach (var personaPrint in personaPrints.Where(x => x.PersonaData.ContainsPersona))
                    {
                        var option = new FloatMenuOption(personaPrint.PersonaData.PawnNameColored, delegate ()
                        {
                            this.personaPrintToRestore = personaPrint;
                        }, personaPrint, personaPrint.DrawColor);
                        floatList.Add(option);
                    }
                    Find.WindowStack.Add(new FloatMenu(floatList));
                }
            };

            if (this.personaPrintToRestore != null)
            {
                restoreFromPersonaPrint.Disable("AC.AlreadySetToRestore".Translate());
            }
            if (!personaPrints.Any())
            {
                restoreFromPersonaPrint.Disable("AC.NoPersonaPrintToRestore".Translate());
            }
            if (this.Powered is false)
            {
                restoreFromPersonaPrint.Disable("NoPower".Translate());
            }
            yield return restoreFromPersonaPrint;
            if (this.personaPrintToRestore != null)
            {
                yield return new Command_Action
                {
                    defaultLabel = "AC.CancelPersonaPrintRestoration".Translate(),
                    defaultDesc = "AC.CancelPersonaPrintRestorationDesc".Translate(),
                    activateSound = SoundDefOf.Tick_Tiny,
                    icon = UIHelper.CancelIcon,
                    action = delegate ()
                    {
                        this.personaPrintToRestore = null;
                    }
                };
            }

            yield return new Command_Toggle()
            {
                defaultLabel = "AC.EnableAutoRestore".Translate(),
                defaultDesc = "AC.EnableAutoRestoreDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Gizmos/EnableAutoRestore"),
                activateSound = SoundDefOf.Tick_Tiny,
                toggleAction = delegate ()
                {
                    autoRestoreIsEnabled = !autoRestoreIsEnabled;
                },
                isActive = () => autoRestoreIsEnabled
            };
        }

        private IEnumerable<Gizmo> GetStackOperationGizmos(StackGizmoInfo info)
        {
            var command = new Command_ActionOnStack(this, ForFilledStack(includeArchotechStack: info.includeArchotechStack),
                info.action)
            {
                defaultLabel = info.defaultLabel,
                defaultDesc = info.defaultDesc,
                icon = ContentFinder<Texture2D>.Get(info.icon),
                activateSound = SoundDefOf.Tick_Tiny,
                action = delegate ()
                {
                    info.targetAction();
                },
            };
            if (powerComp.PowerOn is false)
            {
                command.Disable("NoPower".Translate().CapitalizeFirst());
            }
            command.LockBehindReseach(info.lockedProjects);
            if (ConnectedMatrix is null)
            {
                command.Disable("AC.NoConnectedMatrix".Translate());
            }
            yield return command;
            var bills = this.billStack.Bills.Where(x => x.recipe == info.recipe).ToList();
            if (bills.Any())
            {
                yield return new Command_Action
                {
                    defaultLabel = info.defaultLabelCancel,
                    defaultDesc = info.defaultDescCancel,
                    activateSound = SoundDefOf.Tick_Tiny,
                    icon = UIHelper.CancelIcon,
                    action = delegate ()
                    {
                        foreach (var bill in bills)
                        {
                            this.billStack.Delete(bill);
                        }
                    }
                };
            }
        }

        private void BeginTargetingForWipingStack()
        {
            Find.Targeter.BeginTargeting(ForFilledStack(includeArchotechStack: false), delegate (LocalTargetInfo x)
            {
                InstallWipeStackBill(x);
                if (Event.current.shift)
                {
                    BeginTargetingForWipingStack();
                }
            });
        }

        private void BeginTargetingForDuplicatingStack()
        {
            Find.Targeter.BeginTargeting(ForFilledStack(includeArchotechStack: false), delegate (LocalTargetInfo x)
            {
                InstallDuplicateStackBill(x);
                if (Event.current.shift)
                {
                    BeginTargetingForDuplicatingStack();
                }
            });
        }

        private void BeginTargetingForEditingStack()
        {
            Find.Targeter.BeginTargeting(ForFilledStack(includeArchotechStack: AC_Utils.editStacksSettings.enableArchostackEditing), delegate (LocalTargetInfo x)
            {
                InstallEditBill(x);
            });
        }
        public bool CanAddOperationOn(PersonaStack personaStack)
        {
            var bill = this.billStack.Bills.OfType<Bill_OperateOnStack>().Where(x => x.personaStack == personaStack).FirstOrDefault();
            if (bill != null)
            {
                if (bill.recipe == AC_DefOf.AC_WipeFilledPersonaStack)
                {
                    Messages.Message("AC.AlreadyOrderedToWipeStack".Translate(), MessageTypeDefOf.CautionInput);
                }
                else if (bill.recipe == AC_DefOf.AC_EditFilledPersonaStack)
                {
                    Messages.Message("AC.AlreadyOrderedToEditStack".Translate(), MessageTypeDefOf.CautionInput);
                }
                else if (bill.recipe == AC_DefOf.AC_DuplicatePersonaStack)
                {
                    Messages.Message("AC.AlreadyOrderedToDuplicateStack".Translate(), MessageTypeDefOf.CautionInput);
                }
                return false;
            }
            return true;
        }

        private TargetingParameters ForFilledStack(bool includeArchotechStack)
        {
            TargetingParameters targetingParameters = new TargetingParameters
            {
                canTargetItems = true,
                mapObjectTargetsMustBeAutoAttackable = false,
                validator = (TargetInfo x) => x.Thing is PersonaStack stack && stack.PersonaData.ContainsPersona && (includeArchotechStack ||
                stack.IsArchotechStack is false)
            };
            return targetingParameters;
        }

        public void InstallWipeStackBill(LocalTargetInfo x)
        {
            if (x.Thing is PersonaStack personaStack && CanAddOperationOn(personaStack))
            {
                if (personaStack.PersonaData.faction != null && personaStack.PersonaData.faction.HostileTo(Faction.OfPlayer) is false)
                {
                    Find.WindowStack.Add(new Dialog_MessageBox("AC.WipingFriendlyStackWarning".Translate(), "Cancel".Translate(), null,
                    "Confirm".Translate(), delegate ()
                    {
                        billStack.AddBill(new Bill_OperateOnStack(personaStack, AC_DefOf.AC_WipeFilledPersonaStack, null));
                    }, null, false, null, null));
                }
                else
                {
                    billStack.AddBill(new Bill_OperateOnStack(personaStack, AC_DefOf.AC_WipeFilledPersonaStack, null));
                }
            }
        }

        public void InstallDuplicateStackBill(LocalTargetInfo x)
        {
            if (x.Thing is PersonaStack personaStack && CanAddOperationOn(personaStack))
            {
                if (personaStack.PersonaData.faction != null && personaStack.PersonaData.faction.HostileTo(Faction.OfPlayer) is false)
                {
                    Find.WindowStack.Add(new Dialog_MessageBox("AC.DuplicatingFriendlyStackWarning".Translate(), "Cancel".Translate(), null,
                    "Confirm".Translate(), delegate ()
                    {
                        billStack.AddBill(new Bill_OperateOnStack(personaStack, AC_DefOf.AC_DuplicatePersonaStack, null));
                    }, null, false, null, null));
                }
                else
                {
                    billStack.AddBill(new Bill_OperateOnStack(personaStack, AC_DefOf.AC_DuplicatePersonaStack, null));
                }
            }
        }

        private void InstallEditBill(LocalTargetInfo x)
        {
            if (x.Thing is PersonaStack personaStack && CanAddOperationOn(personaStack))
            {
                if (personaStack.PersonaData.faction != null && personaStack.PersonaData.faction.HostileTo(Faction.OfPlayer) is false)
                {
                    Find.WindowStack.Add(new Dialog_MessageBox("AC.EditingFriendlyStackWarning".Translate(), "Cancel".Translate(), null,
                    "Confirm".Translate(), delegate ()
                    {
                        Find.WindowStack.Add(new Window_StackEditor(this, personaStack));
                    }, null, false, null, null));
                }
                else
                {
                    Find.WindowStack.Add(new Window_StackEditor(this, personaStack));
                }
            }
        }

        public void PerformStackRestoration(Pawn doer, PersonaPrint personaPrint, Building_PersonaMatrix matrix)
        {
            var stackRestoreTo = (PersonaStack)ThingMaker.MakeThing(AC_DefOf.AC_FilledPersonaStack);
            stackRestoreTo.PersonaData.CopyDataFrom(personaPrint.PersonaData, true);
            AlteredCarbonManager.Instance.RegisterStack(stackRestoreTo);
            Messages.Message("AC.SuccessfullyRestoredStackFromBackup".Translate(doer.Named("PAWN")), stackRestoreTo, MessageTypeDefOf.TaskCompletion);
            GenPlace.TryPlaceThing(stackRestoreTo, doer.Position, doer.Map, ThingPlaceMode.Near);
            matrix?.innerContainer.Remove(personaPrint);
            personaPrint.Destroy();
            personaPrintToRestore = null;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.autoRestoreIsEnabled, "autoRestoreIsEnabled", true);
            Scribe_References.Look(ref this.personaPrintToRestore, "personaPrintToRestore");
        }
    }
}