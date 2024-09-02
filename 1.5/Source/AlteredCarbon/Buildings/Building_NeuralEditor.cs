using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    public class CommandInfo
    {
        public string icon;
        public Action targetAction;
        public Action<LocalTargetInfo> action;
        public List<ResearchProjectDef> lockedProjects;
        public RecipeDef recipe;
        public string defaultLabelCancel, defaultDescCancel;
        public TargetingParameters targetParameters;
        public Building building;
    }

    public interface IMatrixConnectable
    {
        Building_NeuralMatrix ConnectedMatrix { get; }
    }

    [StaticConstructorOnStartup]
    public class Building_NeuralEditor : Building_WorkTable, IMatrixConnectable
    {
        public bool autoRestoreIsEnabled = true;
        public Building_NeuralMatrix ConnectedMatrix => CompAffectedByFacilities.LinkedFacilitiesListForReading.OfType<Building_NeuralMatrix>().FirstOrDefault();
        private CompAffectedByFacilities _compAffectedByFacilities;
        public CompAffectedByFacilities CompAffectedByFacilities =>
            _compAffectedByFacilities ??= GetComp<CompAffectedByFacilities>();

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }

            foreach (var g in GetCommands<Command_ActionOnStack>(new CommandInfo
            {
                icon = "UI/Gizmos/WipeStack",
                targetAction = BeginTargetingForWipingStack,
                action = InstallWipeStackBill,
                lockedProjects = new List<ResearchProjectDef> { AC_DefOf.AC_NeuralEditing },
                recipe = AC_DefOf.AC_WipeActiveNeuralStack,
                defaultLabelCancel = "AC.CancelStackReset".Translate(),
                defaultDescCancel = "AC.CancelStackResetDesc".Translate(),
                targetParameters = ForActiveStack(includeArchotechStack: false),
            }))
            {
                yield return g;
            }

            foreach (var g in GetCommands<Command_ActionOnStack>(new CommandInfo
            {
                icon = "UI/Gizmos/DuplicateStack",
                targetAction = BeginTargetingForDuplicatingStack,
                action = InstallDuplicateStackBill,
                lockedProjects = new List<ResearchProjectDef> { AC_DefOf.AC_NeuralEditing },
                recipe = AC_DefOf.AC_DuplicateNeuralStack,
                defaultLabelCancel = "AC.CancelStackDuplication".Translate(),
                defaultDescCancel = "AC.CancelStackDuplicationDesc".Translate(),
                targetParameters = ForActiveStack(includeArchotechStack: false),
            }))
            {
                yield return g;
            }

            foreach (var g in GetCommands<Command_ActionOnStack>(new CommandInfo
            {
                icon = "UI/Gizmos/EditStack",
                targetAction = BeginTargetingForEditingStack,
                action = InstallEditBill,
                lockedProjects = AC_DefOf.AC_EditActiveNeuralStack.researchPrerequisites,
                recipe = AC_DefOf.AC_EditActiveNeuralStack,
                defaultLabelCancel = "AC.CancelStackEdit".Translate(),
                defaultDescCancel = "AC.CancelStackEditDesc".Translate(),
                targetParameters = ForActiveStack(includeArchotechStack: AC_Utils.editStacksSettings.enableArchostackEditing),
            }))
            {
                yield return g;
            }

            //foreach (var g in GetCommands<Command_ActionOnPrint>(new CommandInfo
            //{
            //    icon = "UI/Gizmos/RestoreFromNeuralPrint",
            //    targetAction = BeginTargetingForRestoringFromPrint,
            //    action = InstallRestoreFromPrintBill,
            //    lockedProjects = new List<ResearchProjectDef> { AC_DefOf.AC_NeuralDigitalization },
            //    recipe = AC_DefOf.AC_RestoreStackFromNeuralPrint,
            //    defaultLabelCancel = "AC.CancelNeuralPrintRestoration".Translate(),
            //    defaultDescCancel = "AC.CancelNeuralPrintRestorationDesc".Translate(),
            //    targetParameters = ForNeuralPrint(),
            //}))
            //{
            //    yield return g;
            //}
            //
            //var autoRestore = new Command_Toggle()
            //{
            //    defaultLabel = "AC.EnableAutoRestore".Translate(),
            //    defaultDesc = "AC.EnableAutoRestoreDesc".Translate(),
            //    icon = ContentFinder<Texture2D>.Get("UI/Gizmos/EnableAutoRestore"),
            //    activateSound = SoundDefOf.Tick_Tiny,
            //    toggleAction = delegate ()
            //    {
            //        autoRestoreIsEnabled = !autoRestoreIsEnabled;
            //    },
            //    isActive = () => autoRestoreIsEnabled
            //};
            //autoRestore.TryDisableCommand(new CommandInfo
            //{
            //    lockedProjects = new List<ResearchProjectDef> { AC_DefOf.AC_NeuralDigitalization },
            //    building = this,
            //});
            //yield return autoRestore;
        }

        private IEnumerable<Gizmo> GetCommands<T>(CommandInfo info) where T : Command_ActionOnThing
        {
            var command = (T)Activator.CreateInstance(typeof(T), new object[]
            { this, info.targetParameters, info.action });
            command.defaultLabel = info.recipe.label;
            command.defaultDesc = info.recipe.description;
            command.icon = ContentFinder<Texture2D>.Get(info.icon);
            command.activateSound = SoundDefOf.Tick_Tiny;
            command.action = delegate ()
            {
                info.targetAction();
            };
            info.building = this;
            command.TryDisableCommand(info);
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
            Find.Targeter.BeginTargeting(ForActiveStack(includeArchotechStack: false), delegate (LocalTargetInfo x)
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
            Find.Targeter.BeginTargeting(ForActiveStack(includeArchotechStack: false), delegate (LocalTargetInfo x)
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
            Find.Targeter.BeginTargeting(ForActiveStack(includeArchotechStack: AC_Utils.editStacksSettings.enableArchostackEditing), delegate (LocalTargetInfo x)
            {
                InstallEditBill(x);
            });
        }

        //private void BeginTargetingForRestoringFromPrint()
        //{
        //    Find.Targeter.BeginTargeting(ForNeuralPrint(), delegate (LocalTargetInfo x)
        //    {
        //        InstallRestoreFromPrintBill(x);
        //        if (Event.current.shift)
        //        {
        //            BeginTargetingForRestoringFromPrint();
        //        }
        //    });
        //}

        public bool CanAddOperationOn(NeuralStack neuralStack)
        {
            var bill = this.billStack.Bills.OfType<Bill_OperateOnStack>().Where(x => x.thingWithNeuralData == neuralStack).FirstOrDefault();
            if (bill != null)
            {
                if (bill.recipe == AC_DefOf.AC_WipeActiveNeuralStack)
                {
                    Messages.Message("AC.AlreadyOrderedToWipeStack".Translate(), MessageTypeDefOf.CautionInput);
                }
                else if (bill.recipe == AC_DefOf.AC_EditActiveNeuralStack)
                {
                    Messages.Message("AC.AlreadyOrderedToEditStack".Translate(), MessageTypeDefOf.CautionInput);
                }
                else if (bill.recipe == AC_DefOf.AC_DuplicateNeuralStack)
                {
                    Messages.Message("AC.AlreadyOrderedToDuplicateStack".Translate(), MessageTypeDefOf.CautionInput);
                }
                return false;
            }
            return true;
        }

        //public bool CanAddOperationOn(NeuralPrint neuralPrint, bool allowMessages = true)
        //{
        //    var bill = this.billStack.Bills.OfType<Bill_OperateOnStack>().Where(x => x.thingWithNeuralData == neuralPrint).FirstOrDefault();
        //    if (bill != null)
        //    {
        //        if (bill.recipe == AC_DefOf.AC_RestoreStackFromNeuralPrint && allowMessages)
        //        {
        //            Messages.Message("AC.AlreadyOrderedToRestoreStack".Translate(), MessageTypeDefOf.CautionInput);
        //        }
        //        return false;
        //    }
        //    return true;
        //}

        private TargetingParameters ForActiveStack(bool includeArchotechStack)
        {
            TargetingParameters targetingParameters = new TargetingParameters
            {
                canTargetItems = true,
                mapObjectTargetsMustBeAutoAttackable = false,
                validator = (TargetInfo x) => x.Thing is NeuralStack stack && stack.NeuralData.ContainsNeural && (includeArchotechStack ||
                stack.IsArchotechStack is false)
            };
            return targetingParameters;
        }

        //private TargetingParameters ForNeuralPrint()
        //{
        //    TargetingParameters targetingParameters = new TargetingParameters
        //    {
        //        canTargetItems = true,
        //        mapObjectTargetsMustBeAutoAttackable = false,
        //        validator = (TargetInfo x) => x.Thing is NeuralPrint stack && stack.NeuralData.ContainsNeural
        //    };
        //    return targetingParameters;
        //}

        public void InstallWipeStackBill(LocalTargetInfo x)
        {
            if (x.Thing is NeuralStack neuralStack && CanAddOperationOn(neuralStack))
            {
                if (neuralStack.NeuralData.faction != null && neuralStack.NeuralData.faction.HostileTo(Faction.OfPlayer) is false)
                {
                    Find.WindowStack.Add(new Dialog_MessageBox("AC.WipingFriendlyStackWarning".Translate(), "Cancel".Translate(), null,
                    "Confirm".Translate(), delegate ()
                    {
                        billStack.AddBill(new Bill_OperateOnStack(neuralStack, AC_DefOf.AC_WipeActiveNeuralStack, null));
                    }, null, false, null, null));
                }
                else
                {
                    billStack.AddBill(new Bill_OperateOnStack(neuralStack, AC_DefOf.AC_WipeActiveNeuralStack, null));
                }
            }
        }

        public void InstallDuplicateStackBill(LocalTargetInfo x)
        {
            if (x.Thing is NeuralStack neuralStack && CanAddOperationOn(neuralStack))
            {
                if (neuralStack.NeuralData.faction != null && neuralStack.NeuralData.faction.HostileTo(Faction.OfPlayer) is false)
                {
                    Find.WindowStack.Add(new Dialog_MessageBox("AC.DuplicatingFriendlyStackWarning".Translate(), "Cancel".Translate(), null,
                    "Confirm".Translate(), delegate ()
                    {
                        billStack.AddBill(new Bill_OperateOnStack(neuralStack, AC_DefOf.AC_DuplicateNeuralStack, null));
                    }, null, false, null, null));
                }
                else
                {
                    billStack.AddBill(new Bill_OperateOnStack(neuralStack, AC_DefOf.AC_DuplicateNeuralStack, null));
                }
            }
        }

        private void InstallEditBill(LocalTargetInfo x)
        {
            if (x.Thing is NeuralStack neuralStack && CanAddOperationOn(neuralStack))
            {
                if (neuralStack.NeuralData.faction != null && neuralStack.NeuralData.faction.HostileTo(Faction.OfPlayer) is false)
                {
                    Find.WindowStack.Add(new Dialog_MessageBox("AC.EditingFriendlyStackWarning".Translate(), "Cancel".Translate(), null,
                    "Confirm".Translate(), delegate ()
                    {
                        Find.WindowStack.Add(new Window_StackEditor(this, neuralStack));
                    }, null, false, null, null));
                }
                else
                {
                    Find.WindowStack.Add(new Window_StackEditor(this, neuralStack));
                }
            }
        }


        //public void InstallRestoreFromPrintBill(LocalTargetInfo x)
        //{
        //    if (x.Thing is NeuralPrint neuralStack && CanAddOperationOn(neuralStack))
        //    {
        //        billStack.AddBill(new Bill_OperateOnStack(neuralStack, AC_DefOf.AC_RestoreStackFromNeuralPrint, null));
        //    }
        //}

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.autoRestoreIsEnabled, "autoRestoreIsEnabled", true);
        }
    }
}