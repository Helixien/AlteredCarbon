using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    public class Building_DecryptionBench : Building_WorkTable
    {
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }

            var wipeStacks = new Command_WipeStacks
            {
                defaultLabel = "AC.WipeStack".Translate(),
                defaultDesc = "AC.WipeStackDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Icons/WipeStack"),
                activateSound = SoundDefOf.Tick_Tiny,
                action = delegate ()
                {
                    Find.Targeter.BeginTargeting(ForFilledStack(includeArchoStack: false), delegate (LocalTargetInfo x)
                    {
                        InstallWipeStackRecipe(x.Thing as CorticalStack);
                    });
                },
                decryptionBench = this
            };
            if (powerComp.PowerOn is false)
            {
                wipeStacks.Disable("NoPower".Translate().CapitalizeFirst());
            }
            yield return wipeStacks;

            if (ModCompatibility.HelixienAlteredCarbonIsActive)
            {
                var rewriteStack =  new Command_Action
                {
                    defaultLabel = "AC.RewriteStack".Translate(),
                    defaultDesc = "AC.RewriteStackDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Icons/EditStack"),
                    action = delegate ()
                    {
                        Find.Targeter.BeginTargeting(ForFilledStack(includeArchoStack: false), delegate (LocalTargetInfo x)
                        {
                            if (x.Thing is CorticalStack corticalStack && CanAddOperationOn(corticalStack))
                            {
                                Find.WindowStack.Add(new Window_StackEditor(this, corticalStack));
                            }
                        });
                    }
                };
                if (IsStackRewriteResearchFinished is false)
                {
                    rewriteStack.Disable("MissingRequiredResearch".Translate() + ": " 
                        + (from x in AC_DefOf.AC_RewriteFilledCorticalStack.researchPrerequisites where !x.IsFinished select x.label)
                        .ToCommaList(useAnd: true).CapitalizeFirst());
                }
                if (powerComp.PowerOn is false)
                {
                    rewriteStack.Disable("NoPower".Translate().CapitalizeFirst());
                }
                yield return rewriteStack;
            }
        }

        public bool IsStackRewriteResearchFinished
        {
            get
            {
                if (AC_DefOf.AC_RewriteFilledCorticalStack.researchPrerequisites != null)
                {
                    for (int i = 0; i < AC_DefOf.AC_RewriteFilledCorticalStack.researchPrerequisites.Count; i++)
                    {
                        if (!AC_DefOf.AC_RewriteFilledCorticalStack.researchPrerequisites[i].IsFinished)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }
        private bool CanAddOperationOn(CorticalStack corticalStack)
        {
            var bill = this.billStack.Bills.OfType<Bill_OperateOnStack>().Where(x => x.corticalStack == corticalStack).FirstOrDefault();
            if (bill != null)
            {
                if (bill.recipe == AC_DefOf.VFEU_HackFilledCorticalStack)
                {
                    Messages.Message("AC.AlreadyOrderedToHackStack".Translate(), MessageTypeDefOf.CautionInput);
                }
                else if (bill.recipe == AC_DefOf.VFEU_WipeFilledCorticalStack)
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

        public void InstallWipeStackRecipe(CorticalStack corticalStack)
        {
            if (CanAddOperationOn(corticalStack))
            {
                billStack.AddBill(new Bill_OperateOnStack(corticalStack, AC_DefOf.VFEU_WipeFilledCorticalStack, null));
            }
        }
    }
}