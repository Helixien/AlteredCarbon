using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    public class Command_WipeStacks : Command_Action
    {
        public Building_DecryptionBench decryptionBench;
        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
        {
            get
            {
                foreach (CorticalStack corticalStack in CorticalStack.corticalStacks)
                {
                    if (corticalStack.PersonaData.ContainsInnerPersona && !decryptionBench.billStack.Bills.Any(x => x is Bill_HackStack hackStack
                            && hackStack.corticalStack == corticalStack && hackStack.recipe == AC_DefOf.VFEU_WipeFilledCorticalStack)
                            && corticalStack.MapHeld == Find.CurrentMap)
                    {
                        yield return new FloatMenuOption(corticalStack.PersonaData.PawnNameColored, delegate ()
                        {
                            decryptionBench.InstallWipeStackRecipe(corticalStack);
                        });
                    }
                }
            }
        }
    }

    public class Building_DecryptionBench : Building_WorkTable
    {
        public TargetingParameters ForWipableStack()
        {
            TargetingParameters targetingParameters = new TargetingParameters
            {
                canTargetItems = true,
                mapObjectTargetsMustBeAutoAttackable = false,
                validator = (TargetInfo x) => x.Thing is CorticalStack stack && stack.PersonaData.ContainsInnerPersona
            };
            return targetingParameters;
        }
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }

            yield return new Command_WipeStacks
            {
                defaultLabel = "AC.WipeStack".Translate(),
                defaultDesc = "AC.WipeStackDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Icons/WipeStack"),
                action = delegate ()
                {
                    Find.Targeter.BeginTargeting(ForWipableStack(), delegate (LocalTargetInfo x)
                    {
                        InstallWipeStackRecipe(x.Thing as CorticalStack);
                    });
                },
                decryptionBench = this
            };

        }

        public void InstallWipeStackRecipe(CorticalStack corticalStack)
        {
            if (billStack.Bills.Any(y => y is Bill_HackStack bill && bill.corticalStack == corticalStack && bill.recipe == AC_DefOf.VFEU_WipeFilledCorticalStack))
            {
                Messages.Message("AC.AlreadyOrderedToWipeStack".Translate(), MessageTypeDefOf.CautionInput);
            }
            else
            {
                billStack.AddBill(new Bill_HackStack(corticalStack, AC_DefOf.VFEU_WipeFilledCorticalStack, null));
            }
        }
    }
}