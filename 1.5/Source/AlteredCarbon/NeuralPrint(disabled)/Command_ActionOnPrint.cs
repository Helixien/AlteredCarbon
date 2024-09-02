using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AlteredCarbon
{
    public class Command_ActionOnPrint : Command_ActionOnThing
    {
        public Command_ActionOnPrint(Building_NeuralEditor neuralEditor, TargetingParameters targetParameters, Action<LocalTargetInfo> actionOnStack) : base(neuralEditor, targetParameters, actionOnStack)
        {
        }

        public override IEnumerable<FloatMenuOption> FloatMenuOptions
        {
            get
            {
                var prints = neuralEditor.Map.listerThings
                    .ThingsOfDef(AC_DefOf.AC_NeuralPrint).OfType<NeuralPrint>()
                    .Where(x => x.NeuralData.ContainsNeural).ToList();
                foreach (var neuralMatrix in neuralEditor.Map.listerThings.ThingsOfDef(AC_DefOf.AC_NeuralMatrix)
                    .OfType<Building_NeuralMatrix>())
                {
                    prints.AddRange(neuralMatrix.StoredNeuralPrints);
                }

                foreach (NeuralPrint neuralPrint in prints)
                {
                    if (targetParameters is null || targetParameters.CanTarget(neuralPrint))
                    {
                        yield return new FloatMenuOption(neuralPrint.NeuralData.PawnNameColored, delegate ()
                        {
                            actionOnStack(neuralPrint);
                            Find.Targeter.StopTargeting();
                        }, iconThing: neuralPrint, iconColor: neuralPrint.DrawColor);
                    }
                }
            }
        }
    }
}