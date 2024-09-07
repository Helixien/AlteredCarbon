using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    public class Building_NeuralMatrix : Building
    {
        public CompPowerTrader compPower;
        public CompNeuralCache compCache;
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            compPower = this.TryGetComp<CompPowerTrader>();
            compCache = this.TryGetComp<CompNeuralCache>();
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (compCache.innerContainer.Count > 0 && (mode == DestroyMode.Deconstruct || mode == DestroyMode.KillFinalize))
            {
                compCache.EjectContents();
            }
            compCache.innerContainer.ClearAndDestroyContents(DestroyMode.Vanish);
            base.Destroy(mode);
        }
        public bool Powered => this.compPower.PowerOn;

        public IEnumerable<NeuralStack> StoredNeuralStacks => compCache.innerContainer.OfType<NeuralStack>();

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }
            if (Faction == Faction.OfPlayer)
            {
                var manageNeuralMatrix = new Command_Action
                {
                    defaultLabel = "AC.ManageNeuralMatrix".Translate(),
                    defaultDesc = "AC.ManageNeuralMatrixDesc".Translate(),
                    action = delegate
                    {
                        Find.WindowStack.Add(new Window_NeuralMatrixManagement(this));
                    }
                };
                manageNeuralMatrix.TryDisableCommand(new CommandInfo
                {
                    lockedProjects = new List<ResearchProjectDef> { AC_DefOf.AC_NeuralDigitalization },
                    building = this
                });
                yield return manageNeuralMatrix;
                var stacks = StoredNeuralStacks.ToList();
                if (stacks.Any())
                {
                    var ejectAll = new Command_Action();
                    ejectAll.defaultLabel = "AC.EjectAll".Translate();
                    ejectAll.defaultDesc = "AC.EjectAllNeuralStacksDesc".Translate();
                    ejectAll.icon = ContentFinder<Texture2D>.Get("UI/Gizmos/EjectAllNeuralStacks");
                    ejectAll.action = delegate
                    {
                        compCache.EjectContents();
                    };
                    yield return ejectAll;
                }
            }
        }
    }
}