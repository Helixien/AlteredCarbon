using RimWorld;
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
        protected CommandInfo info;

        public Command_ActionOnThing(Building_NeuralEditor neuralEditor, CommandInfo info)
        {
            this.neuralEditor = neuralEditor;
            this.info = info;
        }

        public abstract HashSet<Thing> Things { get; }
        public abstract IEnumerable<FloatMenuOption> FloatMenuOptions {  get; }

        public void BeginTargeting()
        {
            Find.Targeter.BeginTargeting(new TargetingParameters
            {
                canTargetPawns = true,
                canTargetItems = true,
                mapObjectTargetsMustBeAutoAttackable = false,
                validator = (TargetInfo x) => Things.Contains(x.Thing),
            }, delegate (LocalTargetInfo x)
            {
                info.action(x);
                if (Event.current.shift)
                {
                    BeginTargeting();
                }
            });
        }

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