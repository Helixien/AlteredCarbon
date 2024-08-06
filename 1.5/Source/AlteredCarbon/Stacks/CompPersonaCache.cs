using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    public class CompPersonaCache : CompThingContainer
    {
        public bool allowColonistPersonaStacks = true;
        public bool allowStrangerPersonaStacks = true;
        public bool allowHostilePersonaStacks = true;
        public bool allowArchoStacks = true;
        public List<PersonaStack> StoredStacks => innerContainer.OfType<PersonaStack>().ToList();
        public override bool Accepts(Thing thing)
        {
            if (thing is PersonaStack stack && stack.IsFilledStack && stack.autoLoad && Full is false)
            {
                if (!this.allowArchoStacks && stack.IsArchotechStack)
                {
                    return false;
                }
                if (this.allowColonistPersonaStacks && stack.PersonaData.faction != null && stack.PersonaData.faction == Faction.OfPlayer)
                {
                    return true;
                }
                if (this.allowHostilePersonaStacks && stack.PersonaData.faction.HostileTo(Faction.OfPlayer))
                {
                    return true;
                }
                if (this.allowStrangerPersonaStacks && (stack.PersonaData.faction is null || stack.PersonaData.faction != Faction.OfPlayer && !stack.PersonaData.faction.HostileTo(Faction.OfPlayer)))
                {
                    return true;
                }
            }
            return false;
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

        public override string CompInspectStringExtra()
        {
            return "AC.PersonaStacksStored".Translate(innerContainer.Count(), Props.stackLimit);
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            yield break;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref this.allowColonistPersonaStacks, "allowColonistPersonaStacks", true);
            Scribe_Values.Look(ref this.allowHostilePersonaStacks, "allowHostilePersonaStacks", true);
            Scribe_Values.Look(ref this.allowStrangerPersonaStacks, "allowStrangerPersonaStacks", true);
            Scribe_Values.Look(ref this.allowArchoStacks, "allowArchoStacks", true);
        }
    }
}