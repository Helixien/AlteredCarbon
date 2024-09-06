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
        public Action<LocalTargetInfo> action;
        public List<ResearchProjectDef> lockedProjects;
        public RecipeDef recipe;
        public string defaultLabelCancel, defaultDescCancel;
        public bool enableArchostacks;
        public Building building;
        public bool neuralConnectorIntegration;
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
                action = InstallWipeStackBill,
                lockedProjects = new List<ResearchProjectDef> { AC_DefOf.AC_NeuralEditing },
                recipe = AC_DefOf.AC_WipeActiveNeuralStack,
                defaultLabelCancel = "AC.CancelStackReset".Translate(),
                defaultDescCancel = "AC.CancelStackResetDesc".Translate(),
            }))
            {
                yield return g;
            }

            foreach (var g in GetCommands<Command_ActionOnStack>(new CommandInfo
            {
                icon = "UI/Gizmos/DuplicateStack",
                action = InstallDuplicateStackBill,
                lockedProjects = new List<ResearchProjectDef> { AC_DefOf.AC_NeuralEditing },
                recipe = AC_DefOf.AC_DuplicateNeuralStack,
                defaultLabelCancel = "AC.CancelStackDuplication".Translate(),
                defaultDescCancel = "AC.CancelStackDuplicationDesc".Translate(),
                neuralConnectorIntegration = true
            }))
            {
                yield return g;
            }
            foreach (var g in GetCommands<Command_ActionOnStack>(new CommandInfo
            {
                icon = "UI/Gizmos/EditStack",
                action = InstallEditBill,
                lockedProjects = AC_DefOf.AC_EditActiveNeuralStack.researchPrerequisites,
                recipe = AC_DefOf.AC_EditActiveNeuralStack,
                defaultLabelCancel = "AC.CancelStackEdit".Translate(),
                defaultDescCancel = "AC.CancelStackEditDesc".Translate(),
                enableArchostacks = AC_Utils.editStacksSettings.enableArchostackEditing
            }))
            {
                yield return g;
            }
        }

        private IEnumerable<Gizmo> GetCommands<T>(CommandInfo info) where T : Command_ActionOnThing
        {
            var command = (T)Activator.CreateInstance(typeof(T), new object[]
            { this, info });
            command.defaultLabel = info.recipe.label;
            command.defaultDesc = info.recipe.description;
            command.icon = ContentFinder<Texture2D>.Get(info.icon);
            command.activateSound = SoundDefOf.Tick_Tiny;
            command.action = delegate ()
            {
                if (command.Things.Any())
                {
                    command.BeginTargeting();
                }
            };
            info.building = this;
            command.TryDisableCommand(info);
            yield return command;
            var bills = this.billStack.Bills.Where(x => x.recipe.label == info.recipe.label).ToList();
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

        public bool CanAddOperationOn(Thing neuralStack)
        {
            var bill = this.billStack.Bills.OfType<Bill_OperateOnStack>().Where(x => x.thingWithStack == neuralStack).FirstOrDefault();
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
                else if (bill.recipe == AC_DefOf.AC_DuplicateNeuralStack || bill.recipe == AC_DefOf.AC_DuplicateNeuralStackPawn)
                {
                    Messages.Message("AC.AlreadyOrderedToDuplicateStack".Translate(), MessageTypeDefOf.CautionInput);
                }
                return false;
            }
            return true;
        }

        public void InstallWipeStackBill(LocalTargetInfo x)
        {
            var neuralData = x.Thing.GetNeuralData();
            if (neuralData != null && CanAddOperationOn(x.Thing))
            {
                if (neuralData.faction != null && neuralData.faction.HostileTo(Faction.OfPlayer) is false)
                {
                    Find.WindowStack.Add(new Dialog_MessageBox("AC.WipingFriendlyStackWarning".Translate(), "Cancel".Translate(), null,
                    "Confirm".Translate(), delegate ()
                    {
                        billStack.AddBill(new Bill_OperateOnStack(x.Thing, AC_DefOf.AC_WipeActiveNeuralStack, null));
                    }, null, false, null, null));
                }
                else
                {
                    billStack.AddBill(new Bill_OperateOnStack(x.Thing, AC_DefOf.AC_WipeActiveNeuralStack, null));
                }
            }
        }

        public void InstallDuplicateStackBill(LocalTargetInfo x)
        {
            var neuralData = x.Thing.GetNeuralData();
            if (neuralData != null && CanAddOperationOn(x.Thing))
            {
                var recipe = x.Thing is Pawn ? AC_DefOf.AC_DuplicateNeuralStackPawn : AC_DefOf.AC_DuplicateNeuralStack;
                if (neuralData.faction != null && neuralData.faction.HostileTo(Faction.OfPlayer) is false)
                {
                    Find.WindowStack.Add(new Dialog_MessageBox("AC.DuplicatingFriendlyStackWarning".Translate(), "Cancel".Translate(), null,
                    "Confirm".Translate(), delegate ()
                    {
                        billStack.AddBill(new Bill_OperateOnStack(x.Thing, recipe, null));
                    }, null, false, null, null));
                }
                else
                {
                    billStack.AddBill(new Bill_OperateOnStack(x.Thing, recipe, null));
                }
            }
        }

        private void InstallEditBill(LocalTargetInfo x)
        {
            var neuralData = x.Thing.GetNeuralData();
            if (neuralData != null && CanAddOperationOn(x.Thing))
            {
                if (neuralData.faction != null && neuralData.faction.HostileTo(Faction.OfPlayer) is false)
                {
                    Find.WindowStack.Add(new Dialog_MessageBox("AC.EditingFriendlyStackWarning".Translate(), "Cancel".Translate(), null,
                    "Confirm".Translate(), delegate ()
                    {
                        Find.WindowStack.Add(new Window_StackEditor(this, x.Thing));
                    }, null, false, null, null));
                }
                else
                {
                    Find.WindowStack.Add(new Window_StackEditor(this, x.Thing));
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref this.autoRestoreIsEnabled, "autoRestoreIsEnabled", true);
        }
    }
}