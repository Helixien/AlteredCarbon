using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AlteredCarbon
{
    public class Command_ActionOnStack : Command_ActionOnThing
    {
        public Command_ActionOnStack(Building_NeuralEditor neuralEditor, TargetingParameters targetParameters, Action<LocalTargetInfo> actionOnStack) : base(neuralEditor, targetParameters, actionOnStack)
        {
        }

        public override IEnumerable<FloatMenuOption> FloatMenuOptions
        {
            get
            {
                var stacks = neuralEditor.Map.listerThings
                    .ThingsOfDef(AC_DefOf.AC_ActiveNeuralStack).OfType<NeuralStack>()
                    .Where(x => x.NeuralData.ContainsNeural).ToList();
                foreach (var cache in neuralEditor.Map.listerThings.ThingsOfDef(AC_DefOf.AC_NeuralCache))
                {
                    var comp = cache.TryGetComp<CompNeuralCache>();
                    foreach (var thing in comp.innerContainer)
                    {
                        if (thing is NeuralStack stack && stack.def == AC_DefOf.AC_ActiveNeuralStack && stack.NeuralData.ContainsNeural)
                        {
                            stacks.Add(stack);
                        }
                    }
                }
                foreach (NeuralStack neuralStack in stacks)
                {
                    if (targetParameters is null || targetParameters.CanTarget(neuralStack))
                    {
                        yield return new FloatMenuOption(neuralStack.NeuralData.PawnNameColored, delegate ()
                        {
                            actionOnStack(neuralStack);
                            Find.Targeter.StopTargeting();
                        }, iconThing: neuralStack, iconColor: neuralStack.DrawColor);
                    }
                }
            }
        }
    }
}