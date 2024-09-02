using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    [HotSwappable]
    public abstract class Command_ActionOnThing : Command_Action
    {
        protected Building_NeuralEditor neuralEditor;
        protected TargetingParameters targetParameters;
        protected Action<LocalTargetInfo> actionOnStack;
        public Command_ActionOnThing(Building_NeuralEditor neuralEditor, TargetingParameters targetParameters, Action<LocalTargetInfo> actionOnStack)
        {
            this.neuralEditor = neuralEditor;
            this.targetParameters = targetParameters;
            this.actionOnStack = actionOnStack;
        }

        public abstract IEnumerable<FloatMenuOption> FloatMenuOptions {  get; }

        public override void ProcessInput(Event ev)
        {
            if (ev.button == 0)
            {
                var list = FloatMenuOptions.ToList();
                if (list.Any())
                {
                    Find.WindowStack.Add(new FloatMenu(list));
                }
            }
            base.ProcessInput(ev);
        }
    }
}