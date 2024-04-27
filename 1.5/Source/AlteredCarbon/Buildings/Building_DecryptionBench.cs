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
            var wipeStacks = new Command_ActionOnStack(this, ForFilledStack(includeArchotechStack: false), InstallWipeStackBill)
            {
                defaultLabel = "AC.WipeStack".Translate(),
                defaultDesc = "AC.WipeStackDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Icons/WipeStack"),
                activateSound = SoundDefOf.Tick_Tiny,
                action = delegate ()
                {
                    BeginTargetingForWipingStack();
                },
            };
            if (powerComp.PowerOn is false)
            {
                wipeStacks.Disable("NoPower".Translate().CapitalizeFirst());
            }
            yield return wipeStacks;
            var wipeStacksBills = this.billStack.Bills.Where(x => x.recipe == AC_DefOf.AC_WipeFilledPersonaStack).ToList();
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
            var rewriteStack = new Command_ActionOnStack(this, ForFilledStack(includeArchotechStack: AC_Utils.rewriteStacksSettings.enableArchostackRewriting), InstallRewriteBill)
            {
                defaultLabel = "AC.RewriteStack".Translate(),
                defaultDesc = "AC.RewriteStackDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Icons/EditStack"),
                activateSound = SoundDefOf.Tick_Tiny,
                action = delegate ()
                {
                    Find.Targeter.BeginTargeting(ForFilledStack(includeArchotechStack: AC_Utils.rewriteStacksSettings.enableArchostackRewriting), delegate (LocalTargetInfo x)
                    {
                        InstallRewriteBill(x);
                    });
                }
            };
            rewriteStack.LockBehindReseach(AC_DefOf.AC_RewriteFilledPersonaStack.researchPrerequisites);
            if (powerComp.PowerOn is false)
            {
                rewriteStack.Disable("NoPower".Translate().CapitalizeFirst());
            }
            yield return rewriteStack;

            var rewriteStacksBills = this.billStack.Bills.Where(x => x.recipe == AC_DefOf.AC_RewriteFilledPersonaStack).ToList();
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

        public bool CanAddOperationOn(PersonaStack personaStack)
        {
            var bill = this.billStack.Bills.OfType<Bill_OperateOnStack>().Where(x => x.personaStack == personaStack).FirstOrDefault();
            if (bill != null)
            {
                if (bill.recipe == AC_DefOf.AC_WipeFilledPersonaStack)
                {
                    Messages.Message("AC.AlreadyOrderedToWipeStack".Translate(), MessageTypeDefOf.CautionInput);
                }
                else if (bill.recipe == AC_DefOf.AC_RewriteFilledPersonaStack)
                {
                    Messages.Message("AC.AlreadyOrderedToRewriteStack".Translate(), MessageTypeDefOf.CautionInput);
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
                validator = (TargetInfo x) => x.Thing is PersonaStack stack && stack.PersonaData.ContainsInnerPersona && (includeArchotechStack ||
                stack.IsArchotechStack is false)
            };
            return targetingParameters;
        }

        public void InstallWipeStackBill(LocalTargetInfo x)
        {
            if (x.Thing is PersonaStack personaStack && CanAddOperationOn(personaStack))
            {
                billStack.AddBill(new Bill_OperateOnStack(personaStack, AC_DefOf.AC_WipeFilledPersonaStack, null));
            }
        }

        private void InstallRewriteBill(LocalTargetInfo x)
        {
            if (x.Thing is PersonaStack personaStack && CanAddOperationOn(personaStack))
            {
                Find.WindowStack.Add(new Window_StackEditor(this, personaStack));
            }
        }

    }
}