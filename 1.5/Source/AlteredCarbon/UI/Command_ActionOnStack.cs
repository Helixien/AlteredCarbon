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
        private Building_PersonaEditor decryptionBench;
        private TargetingParameters targetParameters;
        private Action<LocalTargetInfo> actionOnStack;
        public Command_ActionOnStack(Building_PersonaEditor decryptionBench, TargetingParameters targetParameters, Action<LocalTargetInfo> actionOnStack)
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
                    .ThingsOfDef(AC_DefOf.AC_FilledPersonaStack).OfType<PersonaStack>().Where(x => x.PersonaData.ContainsPersona).ToList();
                foreach (PersonaStack personaStack in stacks)
                {
                    if (targetParameters is null || targetParameters.CanTarget(personaStack))
                    {
                        yield return new FloatMenuOption(personaStack.PersonaData.PawnNameColored, delegate ()
                        {
                            actionOnStack(personaStack);
                        }, iconThing: personaStack, iconColor: personaStack.DrawColor);
                    }
                }
            }
        }
    }
}