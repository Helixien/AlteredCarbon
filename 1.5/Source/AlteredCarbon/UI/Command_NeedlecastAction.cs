using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    [HotSwappable]
    public class Command_NeedlecastAction : Command_ActionOnThing
    {
        public Command_NeedlecastAction(Thing source, CommandInfo info) : base(source, info)
        {
        }

        public override HashSet<Thing> Things
        {
            get
            {
                var sourcePawn = (source as Pawn);
                var neuralStack = sourcePawn.GetNeuralStack();
                return AC_Utils.GetAllConnectablePawnsFor(neuralStack).Select(x => x.Key).Cast<Thing>().ToHashSet();
            }
        }

        public override IEnumerable<FloatMenuOption> FloatMenuOptions
        {
            get
            {
                var sourcePawn = (source as Pawn);
                var neuralStack = sourcePawn.GetNeuralStack();
                var list = Window_NeuralMatrixManagement.GetFloatList(neuralStack, AC_Utils.GetAllConnectablePawnsFor(neuralStack));
                foreach (var entry in list)
                {
                    yield return entry;
                }
            }
        }
    }
}