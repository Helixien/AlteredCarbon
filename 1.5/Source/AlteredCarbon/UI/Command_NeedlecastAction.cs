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
                return neuralStack.GetAllConnectablePawns();
            }
        }

        public override IEnumerable<FloatMenuOption> FloatMenuOptions
        {
            get
            {
                foreach (Pawn pawn in Things.OfType<Pawn>())
                {
                    yield return new FloatMenuOption(pawn.NameShortColored, delegate ()
                    {
                        info.action(pawn);
                        Find.Targeter.StopTargeting();
                    }, iconThing: pawn, iconColor: Color.white);
                }
            }
        }
    }
}