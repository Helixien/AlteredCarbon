using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AlteredCarbon
{
    [HotSwappable]
    public class Hediff_RemoteStack : Hediff_Implant
    {
        public NeuralData originalPawnData;
        public Hediff_NeuralStack sourceHediff;
        public NeuralStack sourceStack;
        public INeedlecastable Source
        {
            get
            {
                if (sourceStack != null)
                {
                    return sourceStack;
                }
                return sourceHediff;
            }
        }

        public bool CanBeConnected(INeedlecastable needlecastable)
        {
            if (pawn.Dead || pawn.Downed || pawn.IsLost())
            {
                return false;
            }
            if (needlecastable is Hediff_NeuralStack hediff)
            {
                if (hediff.pawn.Dead || hediff.pawn.IsLost())
                {
                    return false;
                }
            }
            else if (needlecastable is NeuralStack stack)
            {
                if (stack.Destroyed || stack.ParentHolder is not CompNeuralCache comp)
                {
                    return false;
                }
                else
                {
                    Log.Message("comp: " + comp);
                    var matrix = comp.GetMatrix();
                    if (matrix is null || matrix.Powered is false || stack.NeuralData.trackedToMatrix != matrix)
                    {
                        return false;
                    }
                }
            }
            if (!InNeedlecastingRange(needlecastable.ThingHolder))
            {
                return false;
            }
            return true;
        }

        public bool InNeedlecastingRange(Thing source)
        {
            var matrices = Find.Maps.SelectMany(x => x.listerThings.GetThingsOfType<Building_NeuralMatrix>()).ToList();
            foreach (var mat in matrices)
            {
                if (mat.Powered)
                {
                    var rangeCovered = 1f + mat.NeedleCastRangeBoost();
                    var distance = Find.WorldGrid.ApproxDistanceInTiles(mat.Tile, pawn.Tile);
                    var distance2 = Find.WorldGrid.ApproxDistanceInTiles(mat.Tile, source.Tile);
                    if (distance <= rangeCovered && distance2 <= rangeCovered)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref originalPawnData, "originalPawnData");
            Scribe_References.Look(ref sourceHediff, "sourceHediff");
            Scribe_References.Look(ref sourceStack, "sourceStack");
        }

        public void Needlecast(INeedlecastable needlecastable)
        {
            originalPawnData = new NeuralData();
            if (needlecastable is Hediff_NeuralStack source)
            {
                this.sourceHediff = source;
                originalPawnData.CopyFromPawn(pawn, source.SourceStack);
            }
            else if (needlecastable is NeuralStack stack)
            {
                this.sourceStack = stack;
                originalPawnData.CopyFromPawn(pawn, stack.def);
            }

            var needlePawnData = new NeuralData();
            needlePawnData.CopyDataFrom(needlecastable.NeuralData);
            needlePawnData.OverwritePawn(pawn);
        }

        public override void Tick()
        {
            base.Tick();
            if (pawn.IsHashIntervalTick(30))
            {
                if (Needlecasted)
                {
                    if (CanBeConnected(Source) is false)
                    {
                        EndNeedlecasting();
                    }
                    else
                    {
                        UpdateSourcePawn();
                    }
                }
            }

        }

        public bool Needlecasted => sourceHediff != null || sourceStack != null;

        public override void PostRemoved()
        {
            base.PostRemoved();
            if (Needlecasted)
            {
                EndNeedlecasting();
            }
        }

        public void EndNeedlecasting()
        {
            UpdateSourcePawn();
            originalPawnData.OverwritePawn(pawn, copyFromOrigPawn: false);
            var source = Source;
            if (source is Hediff_NeuralStack hediff)
            {
                var coma = hediff.pawn.health.hediffSet.GetFirstHediffOfDef(AC_DefOf.AC_NeedlecastingStasis);
                hediff.needleCastingInto = null;
                hediff.pawn.health.RemoveHediff(coma);
                hediff.pawn.health.AddHediff(AC_DefOf.AC_NeedlecastingSickness);
                sourceHediff = null;
            }
            else if (source is NeuralStack stack)
            {
                stack.needleCastingInto = null;
                sourceStack = null;
            }
            originalPawnData = null;
        }

        private void UpdateSourcePawn()
        {
            var newPawnData = new NeuralData();
            var source = Source;
            newPawnData.trackedToMatrix = source.NeuralData.trackedToMatrix;
            if (source is Hediff_NeuralStack hediff)
            {
                newPawnData.CopyFromPawn(pawn, hediff.SourceStack);
                hediff.NeuralData = newPawnData;
                newPawnData.OverwritePawn(hediff.pawn, copyFromOrigPawn: false);
            }
            else if (source is NeuralStack stack)
            {
                newPawnData.CopyFromPawn(pawn, stack.def);
                stack.NeuralData = newPawnData;
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            yield break;
        }
    }
}