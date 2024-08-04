using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    public class CompPersonaCache : CompThingContainer
    {
        public override bool Accepts(Thing thing)
        {
            return thing is PersonaStack stack && stack.IsFilledStack && stack.autoLoad;// && base.Accepts(thing);
        }

        public override bool Accepts(ThingDef thingDef)
        {
            return false;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (innerContainer.Any())
            {
                var ejectAll = new Command_Action();
                ejectAll.defaultLabel = "AC.EjectAll".Translate();
                ejectAll.defaultDesc = "AC.EjectAllPersonaStacksDesc".Translate();
                ejectAll.icon = ContentFinder<Texture2D>.Get("UI/Gizmos/EjectAllStacks");
                ejectAll.action = delegate
                {
                    innerContainer.TryDropAll(parent.InteractionCell, parent.Map, ThingPlaceMode.Near);
                };
                yield return ejectAll;
            }
        }
    }
}