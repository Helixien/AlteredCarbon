using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    public class CompNeuralCache : CompThingContainer, INotifyHauledTo
    {
        public bool allowColonistNeuralStacks = true;
        public bool allowStrangerNeuralStacks = true;
        public bool allowHostileNeuralStacks = true;
        public List<NeuralStack> StoredStacks => innerContainer.OfType<NeuralStack>().ToList();

        public override bool Accepts(Thing thing)
        {
            if (thing is NeuralStack stack && stack.IsActiveStack && stack.autoLoad && Full is false)
            {
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
                    EjectContents();
                };
                yield return ejectAll;
            }
        }

        public void EjectContents()
        {
            innerContainer.TryDropAll(parent.InteractionCell, parent.Map, ThingPlaceMode.Near);
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
        }

        public void Notify_HauledTo(Pawn hauler, Thing thing, int count)
        {
            if (thing is NeuralStack stack)
            {
                var matrix = GetMatrix();
                if (matrix != null)
                {
                    stack.NeuralData.trackedToMatrix = matrix;
                }
            }
        }

        public Building_NeuralMatrix GetMatrix()
        {
            if (parent is Building_NeuralMatrix matrix)
            {
                return matrix;
            }
            var compFacility = parent.GetComp<CompAffectedByFacilities>();
            return compFacility.LinkedFacilitiesListForReading.OfType<Building_NeuralMatrix>().FirstOrDefault();
        }
    }
}