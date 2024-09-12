using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AlteredCarbon
{
    [HotSwappable]
    public class Hediff_RemoteStack : Hediff_Implant
    {
        public NeuralData originalPawnData;
        public Hediff_NeuralStack source;

        public bool CanBeConnected(Pawn sourcePawn)
        {
            if (pawn.Dead || pawn.Downed)
            {
                return false;
            }
            if (sourcePawn == null || sourcePawn.Dead)
            {
                return false;
            }
            if (!InNeedlecastingRange(sourcePawn))
            {
                return false;
            }
            if (pawn.IsLost() || sourcePawn.IsLost())
            {
                return false;
            }
            return true;
        }

        public bool InNeedlecastingRange(Pawn sourcePawn)
        {
            var matrices = Find.Maps.SelectMany(x => x.listerThings.GetThingsOfType<Building_NeuralMatrix>()).ToList();
            foreach (var mat in matrices)
            {
                if (mat.Powered)
                {
                    var rangeCovered = 1f + mat.NeedleCastRangeBoost();
                    var distance = Find.WorldGrid.ApproxDistanceInTiles(mat.Tile, pawn.Tile);
                    var distance2 = Find.WorldGrid.ApproxDistanceInTiles(mat.Tile, sourcePawn.Tile);
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
            Scribe_References.Look(ref source, "source");
        }

        public void Needlecast(Hediff_NeuralStack source)
        {
            this.source = source;
            originalPawnData = new NeuralData();
            var sourceStack = source.SourceStack;
            originalPawnData.CopyFromPawn(pawn, sourceStack);

            var needlePawnData = new NeuralData();
            needlePawnData.CopyDataFrom(source.NeuralData);
            needlePawnData.OverwritePawn(pawn);
        }

        public override void Tick()
        {
            base.Tick();
            if (pawn.IsHashIntervalTick(30))
            {
                if (Needlecasted())
                {
                    if (CanBeConnected(source.pawn) is false)
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

        private bool Needlecasted()
        {
            return source != null;
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            if (Needlecasted())
            {
                EndNeedlecasting();
            }
        }

        public void EndNeedlecasting()
        {
            UpdateSourcePawn();
            originalPawnData.OverwritePawn(pawn, copyFromOrigPawn: false);
            var coma = source.pawn.health.hediffSet.GetFirstHediffOfDef(AC_DefOf.AC_NeedlecastingStasis);
            source.needleCastingInto = null;
            source.pawn.health.RemoveHediff(coma);
            source.pawn.health.AddHediff(AC_DefOf.AC_NeedlecastingSickness);
            source = null;
            originalPawnData = null;
        }

        private void UpdateSourcePawn()
        {
            var newPawnData = new NeuralData();
            newPawnData.CopyFromPawn(pawn, source.SourceStack);
            source.NeuralData = newPawnData;
            newPawnData.OverwritePawn(source.pawn, copyFromOrigPawn: false);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            yield break;
        }
    }
}