using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    [HotSwappable]
    public class Command_ActionOnStack : Command_ActionOnThing
    {
        public Command_ActionOnStack(Thing source, CommandInfo info) : base(source, info)
        {
        }

        public override IEnumerable<FloatMenuOption> FloatMenuOptions
        {
            get
            {
                var things = Things;
                foreach (NeuralStack neuralStack in Things.OfType<NeuralStack>())
                {
                    yield return new FloatMenuOption(neuralStack.NeuralData.PawnNameColored, delegate ()
                    {
                        info.action(neuralStack);
                        Find.Targeter.StopTargeting();
                    }, iconThing: neuralStack, iconColor: Color.white);
                }

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

        public override HashSet<Thing> Things
        {
            get
            {
                var things = source.MapHeld.listerThings.AllThings.OfType<NeuralStack>()
                    .Where(x => StackValidator(x))
                    .Cast<Thing>().ToHashSet();
                foreach (var cache in source.MapHeld.GetAllStackCaches())
                {
                    var comp = cache.TryGetComp<CompNeuralCache>();
                    foreach (var thing in comp.innerContainer)
                    {
                        if (thing is NeuralStack stack && StackValidator(stack))
                        {
                            things.Add(stack);
                        }
                    }
                }
                if (info.neuralConnectorIntegration)
                {
                    var matrix = (source as IMatrixConnectable).ConnectedMatrix;
                    if (matrix != null && matrix.Powered)
                    {
                        var compFacility = matrix.GetComp<CompFacility>();
                        var connector = compFacility.LinkedBuildings
                            .OfType<Building_NeuralConnector>().FirstOrDefault(x => x.PowerOn);
                        if (connector != null)
                        {
                            foreach (var pawn in source.MapHeld.mapPawns.AllHumanlike
                                .Where(x => x.HasNeuralStack(out var hediff)
                                && (info.enableArchostacks || hediff.def != AC_DefOf.AC_ArchotechStack)))
                            {
                                things.Add(pawn);
                            }
                        }
                    }
                }
                return things.Where(x => x.PositionHeld.Fogged(x.MapHeld) is false).ToHashSet();
            }
        }

        private bool StackValidator(NeuralStack x)
        {
            return x.NeuralData.ContainsData && (info.enableArchostacks || x.IsArchotechStack is false);
        }
    }
}