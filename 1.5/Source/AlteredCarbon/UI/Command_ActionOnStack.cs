using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    [HotSwappable]
    public class Command_ActionOnStack : Command_Action
    {
        private Building_PersonaEditor personaEditor;
        private TargetingParameters targetParameters;
        private Action<LocalTargetInfo> actionOnStack;
        public Command_ActionOnStack(Building_PersonaEditor personaEditor, TargetingParameters targetParameters, Action<LocalTargetInfo> actionOnStack)
        {
            this.personaEditor = personaEditor;
            this.targetParameters = targetParameters;
            this.actionOnStack = actionOnStack;
        }

        public IEnumerable<FloatMenuOption> FloatMenuOptions
        {
            get
            {
                var stacks = personaEditor.Map.listerThings
                    .ThingsOfDef(AC_DefOf.AC_FilledPersonaStack).OfType<PersonaStack>()
                    .Where(x => x.PersonaData.ContainsPersona).ToList();
                foreach (var cache in personaEditor.Map.listerThings.ThingsOfDef(AC_DefOf.AC_PersonaCache))
                {
                    var comp = cache.TryGetComp<CompPersonaCache>();
                    foreach (var thing in comp.innerContainer)
                    {
                        if (thing is PersonaStack stack && stack.def == AC_DefOf.AC_FilledPersonaStack && stack.PersonaData.ContainsPersona)
                        {
                            stacks.Add(stack);
                        }
                    }
                }
                foreach (PersonaStack personaStack in stacks)
                {
                    if (targetParameters is null || targetParameters.CanTarget(personaStack))
                    {
                        yield return new FloatMenuOption(personaStack.PersonaData.PawnNameColored, delegate ()
                        {
                            actionOnStack(personaStack);
                            Find.Targeter.StopTargeting();
                        }, iconThing: personaStack, iconColor: personaStack.DrawColor);
                    }
                }

            }
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