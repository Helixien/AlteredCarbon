using RimWorld;
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
        private bool wasEmptySleeve;
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

        public ConnectStatus GetConnectStatus(INeedlecastable needlecastable)
        {
            if (pawn.Faction != Faction.OfPlayer && pawn.IsPrisonerOfColony is false 
                && pawn.IsSlaveOfColony is false && pawn.IsEmptySleeve() is false)
            {
                return ConnectStatus.NotConnectable;
            }
            if (pawn.Downed)
            {
                if (pawn.IsEmptySleeve())
                {
                    pawn.UndoEmptySleeve();
                    if (pawn.Downed)
                    {
                        pawn.MakeEmptySleeve();
                        return ConnectStatus.Downed;
                    }
                }
                else
                {
                    return ConnectStatus.Downed;
                }
            }
            if (pawn.Dead || pawn.IsLost())
            {
                return ConnectStatus.NotConnectable;
            }
            if (needlecastable is Hediff_NeuralStack hediff)
            {
                if (hediff.pawn.Dead || hediff.pawn.IsLost())
                {
                    return ConnectStatus.NotConnectable;
                }
            }
            else if (needlecastable is NeuralStack stack)
            {
                if (stack.Destroyed || stack.ParentHolder is not CompNeuralCache comp)
                {
                    return ConnectStatus.NotConnectable;
                }
                else
                {
                    var matrix = comp.GetMatrix();
                    if (matrix is null || matrix.Powered is false || stack.NeuralData.trackedToMatrix != matrix)
                    {
                        return ConnectStatus.NotConnectable;
                    }
                }
            }
            if (!InNeedlecastingRange(needlecastable.ThingHolder))
            {
                return ConnectStatus.OutOfRange;
            }
            return ConnectStatus.Connectable;
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
            Scribe_Values.Look(ref wasEmptySleeve, "wasEmptySleeve");
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

        public void Needlecast(INeedlecastable needlecastable)
        {
            wasEmptySleeve = pawn.IsEmptySleeve();
            if (wasEmptySleeve)
            {
                pawn.UndoEmptySleeve();
            }

            var needlePawnData = new NeuralData();
            originalPawnData = new NeuralData();
            originalPawnData.CopyFromPawn(pawn, needlecastable.NeuralData.sourceStack, copyRaceGenderInfo: true);
            if (needlecastable is Hediff_NeuralStack source)
            {
                this.sourceHediff = source;
                needlePawnData.CopyFromPawn(source.Pawn, source.SourceStack);
            }
            else if (needlecastable is NeuralStack stack)
            {
                this.sourceStack = stack;
            }
            needlePawnData.CopyDataFrom(needlecastable.NeuralData);
            needlePawnData.OverwritePawn(pawn);
        }

        public void EndNeedlecasting()
        {
            UpdateSourcePawn();
            originalPawnData.OverwritePawn(pawn);
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
            if (wasEmptySleeve)
            {
                pawn.MakeEmptySleeve();
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (pawn.IsHashIntervalTick(30))
            {
                if (Needlecasted)
                {
                    if (GetConnectStatus(Source) != ConnectStatus.Connectable)
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

        private void UpdateSourcePawn()
        {
            var newSourceData = new NeuralData();
            var source = Source;
            newSourceData.trackedToMatrix = source.NeuralData.trackedToMatrix;
            if (source is Hediff_NeuralStack hediff)
            {
                newSourceData.CopyFromPawn(pawn, hediff.SourceStack);
                hediff.NeuralData = newSourceData;
                newSourceData.OverwritePawn(hediff.pawn);
            }
            else if (source is NeuralStack stack)
            {
                newSourceData.CopyFromPawn(pawn, stack.def);
                stack.NeuralData = newSourceData;
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            yield break;
        }
    }
}