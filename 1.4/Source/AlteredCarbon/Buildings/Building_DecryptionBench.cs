using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AlteredCarbon
{
    [StaticConstructorOnStartup]
    public class Building_DecryptionBench : Building_WorkTable
    {
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }
            var wipeStacks = new Command_ActionOnStack(this, ForFilledStack(includeArchoStack: false), InstallWipeStackBill)
            {
                defaultLabel = "AC.WipeStack".Translate(),
                defaultDesc = "AC.WipeStackDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Icons/WipeStack"),
                activateSound = SoundDefOf.Tick_Tiny,
                action = delegate ()
                {
                    Find.Targeter.BeginTargeting(ForFilledStack(includeArchoStack: false), delegate (LocalTargetInfo x)
                    {
                        InstallWipeStackBill(x);
                    });
                },
            };
            if (powerComp.PowerOn is false)
            {
                wipeStacks.Disable("NoPower".Translate().CapitalizeFirst());
            }
            yield return wipeStacks;
            var wipeStacksBills = this.billStack.Bills.Where(x => x.recipe == AC_DefOf.VFEU_WipeFilledCorticalStack).ToList();
            if (wipeStacksBills.Any())
            {
                yield return new Command_Action
                {
                    defaultLabel = "AC.CancelStackReset".Translate(),
                    defaultDesc = "AC.CancelStackResetDesc".Translate(),
                    activateSound = SoundDefOf.Tick_Tiny,
                    icon = UIHelper.CancelIcon,
                    action = delegate ()
                    {
                        foreach (var bill in wipeStacksBills)
                        {
                            this.billStack.Delete(bill);
                        }
                    }
                };
            }
            if (ModCompatibility.HelixienAlteredCarbonIsActive)
            {
                var rewriteStack = new Command_ActionOnStack(this, ForFilledStack(includeArchoStack: ACUtils.generalRewriteStacks.enableArchostackRewriting), InstallRewriteBill)
                {
                    defaultLabel = "AC.RewriteStack".Translate(),
                    defaultDesc = "AC.RewriteStackDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Icons/EditStack"),
                    activateSound = SoundDefOf.Tick_Tiny,
                    action = delegate ()
                    {
                        Find.Targeter.BeginTargeting(ForFilledStack(includeArchoStack: ACUtils.generalRewriteStacks.enableArchostackRewriting), delegate (LocalTargetInfo x)
                        {
                            InstallRewriteBill(x);
                        });
                    }
                };
                rewriteStack.LockBehindReseach(AC_DefOf.AC_RewriteFilledCorticalStack.researchPrerequisites);
                if (powerComp.PowerOn is false)
                {
                    rewriteStack.Disable("NoPower".Translate().CapitalizeFirst());
                }
                yield return rewriteStack;

                var rewriteStacksBills = this.billStack.Bills.Where(x => x.recipe == AC_DefOf.AC_RewriteFilledCorticalStack).ToList();
                if (rewriteStacksBills.Any())
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "AC.CancelStackRewrite".Translate(),
                        defaultDesc = "AC.CancelStackRewriteDesc".Translate(),
                        activateSound = SoundDefOf.Tick_Tiny,
                        icon = UIHelper.CancelIcon,
                        action = delegate ()
                        {
                            foreach (var bill in rewriteStacksBills)
                            {
                                this.billStack.Delete(bill);
                            }
                        }
                    };
                }
            }
        }


        public bool CanAddOperationOn(CorticalStack corticalStack)
        {
            var bill = this.billStack.Bills.OfType<Bill_OperateOnStack>().Where(x => x.corticalStack == corticalStack).FirstOrDefault();
            if (bill != null)
            {
                if (bill.recipe == AC_DefOf.VFEU_WipeFilledCorticalStack)
                {
                    Messages.Message("AC.AlreadyOrderedToWipeStack".Translate(), MessageTypeDefOf.CautionInput);
                }
                else if (ModCompatibility.HelixienAlteredCarbonIsActive && bill.recipe == AC_DefOf.AC_RewriteFilledCorticalStack)
                {
                    Messages.Message("AC.AlreadyOrderedToRewriteStack".Translate(), MessageTypeDefOf.CautionInput);
                }
                return false;
            }
            return true;
        }

        private TargetingParameters ForFilledStack(bool includeArchoStack)
        {
            TargetingParameters targetingParameters = new TargetingParameters
            {
                canTargetItems = true,
                mapObjectTargetsMustBeAutoAttackable = false,
                validator = (TargetInfo x) => x.Thing is CorticalStack stack && stack.PersonaData.ContainsInnerPersona && (includeArchoStack ||
                stack.IsArchoStack is false)
            };
            return targetingParameters;
        }

        public void InstallWipeStackBill(LocalTargetInfo x)
        {
            if (x.Thing is CorticalStack corticalStack && CanAddOperationOn(corticalStack))
            {
                billStack.AddBill(new Bill_OperateOnStack(corticalStack, AC_DefOf.VFEU_WipeFilledCorticalStack, null));
            }
        }

        private void InstallRewriteBill(LocalTargetInfo x)
        {
            if (x.Thing is CorticalStack corticalStack && CanAddOperationOn(corticalStack))
            {
                Find.WindowStack.Add(new Window_StackEditor(this, corticalStack));
            }
        }

    }
}