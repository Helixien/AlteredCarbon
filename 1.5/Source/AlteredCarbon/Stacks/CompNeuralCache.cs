using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    public class CompNeuralCache : CompThingContainer
    {
        public bool allowColonistNeuralStacks = true;
        public bool allowStrangerNeuralStacks = true;
        public bool allowHostileNeuralStacks = true;
        public bool allowArchoStacks = true;
        public List<NeuralStack> StoredStacks => innerContainer.OfType<NeuralStack>().ToList();
        public override bool Accepts(Thing thing)
        {
            if (thing is NeuralStack stack && stack.IsActiveStack && stack.autoLoad && Full is false)
            {
                if (!this.allowArchoStacks && stack.IsArchotechStack)
                {
                    return false;
                }
                if (this.allowColonistNeuralStacks && stack.NeuralData.faction != null && stack.NeuralData.faction == Faction.OfPlayer)
                {
                    return true;
                }
                if (this.allowHostileNeuralStacks && stack.NeuralData.faction.HostileTo(Faction.OfPlayer))
                {
                    return true;
                }
                if (this.allowStrangerNeuralStacks && (stack.NeuralData.faction is null || stack.NeuralData.faction != Faction.OfPlayer && !stack.NeuralData.faction.HostileTo(Faction.OfPlayer)))
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
                ejectAll.defaultDesc = "AC.EjectAllNeuralStacksDesc".Translate();
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
            return "AC.NeuralStacksStored".Translate(innerContainer.Count(), Props.stackLimit);
        }

        public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
        {
            yield break;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref this.allowColonistNeuralStacks, "allowColonistNeuralStacks", true);
            Scribe_Values.Look(ref this.allowHostileNeuralStacks, "allowHostileNeuralStacks", true);
            Scribe_Values.Look(ref this.allowStrangerNeuralStacks, "allowStrangerNeuralStacks", true);
            Scribe_Values.Look(ref this.allowArchoStacks, "allowArchoStacks", true);
        }
    }
}