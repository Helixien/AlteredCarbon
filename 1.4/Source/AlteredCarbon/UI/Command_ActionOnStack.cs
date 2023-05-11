using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AlteredCarbon
{

    [HotSwappable]
    public class Command_ActionOnStack : Command_Action
    {
        private Building_DecryptionBench decryptionBench;
        private TargetingParameters targetParameters;
        private Action<LocalTargetInfo> actionOnStack;
        public Command_ActionOnStack(Building_DecryptionBench decryptionBench, TargetingParameters targetParameters, Action<LocalTargetInfo> actionOnStack)
        {
            this.decryptionBench = decryptionBench;
            this.targetParameters = targetParameters;
            this.actionOnStack = actionOnStack;
        }

        public override IEnumerable<FloatMenuOption> RightClickFloatMenuOptions
        {
            get
            {
                Find.Targeter.StopTargeting();
                var stacks = decryptionBench.Map.listerThings
                    .ThingsOfDef(AC_DefOf.VFEU_FilledCorticalStack).OfType<CorticalStack>().Where(x => x.PersonaData.ContainsInnerPersona).ToList();
                foreach (CorticalStack corticalStack in stacks)
                {
                    if (targetParameters is null || targetParameters.CanTarget(corticalStack))
                    {
                        yield return new FloatMenuOption(corticalStack.PersonaData.PawnNameColored, delegate ()
                        {
                            actionOnStack(corticalStack);
                        }, iconThing: corticalStack, iconColor: corticalStack.DrawColor);
                    }
                }
            }
        }
    }
}