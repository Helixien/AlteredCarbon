using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AlteredCarbon
{
    public class Command_ActionOnPrint : Command_ActionOnThing
    {
        public Command_ActionOnPrint(Building_PersonaEditor personaEditor, TargetingParameters targetParameters, Action<LocalTargetInfo> actionOnStack) : base(personaEditor, targetParameters, actionOnStack)
        {
        }

        public override IEnumerable<FloatMenuOption> FloatMenuOptions
        {
            get
            {
                var prints = personaEditor.Map.listerThings
                    .ThingsOfDef(AC_DefOf.AC_PersonaPrint).OfType<PersonaPrint>()
                    .Where(x => x.PersonaData.ContainsPersona).ToList();
                foreach (var personaMatrix in personaEditor.Map.listerThings.ThingsOfDef(AC_DefOf.AC_PersonaMatrix)
                    .OfType<Building_PersonaMatrix>())
                {
                    prints.AddRange(personaMatrix.StoredPersonaPrints);
                }

                foreach (PersonaPrint personaPrint in prints)
                {
                    if (targetParameters is null || targetParameters.CanTarget(personaPrint))
                    {
                        yield return new FloatMenuOption(personaPrint.PersonaData.PawnNameColored, delegate ()
                        {
                            actionOnStack(personaPrint);
                            Find.Targeter.StopTargeting();
                        }, iconThing: personaPrint, iconColor: personaPrint.DrawColor);
                    }
                }
            }
        }
    }
}