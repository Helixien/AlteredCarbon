using System.Collections.Generic;
using Verse;

namespace AlteredCarbon
{
    [HotSwappable]
    public class Hediff_RemoteStack : Hediff_Implant
    {
        public NeuralData originalPawnData;
        public Hediff_NeuralStack source;
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

        public override void PostRemoved()
        {
            base.PostRemoved();
            if (source != null)
            {
                EndNeedlecast();
            }
        }

        public void EndNeedlecast()
        {
            var newPawnData = new NeuralData();
            newPawnData.CopyFromPawn(pawn, source.SourceStack);
            var data = source.NeuralData = newPawnData;
            data.OverwritePawn(pawn, copyFromOrigPawn: false);
            originalPawnData.OverwritePawn(pawn, copyFromOrigPawn: false);
            var coma = source.pawn.health.hediffSet.GetFirstHediffOfDef(AC_DefOf.AC_NeedlecastingStasis);
            source.pawn.health.RemoveHediff(coma);
            source.pawn.health.AddHediff(AC_DefOf.AC_NeedlecastingSickness);
            source = null;
            originalPawnData = null;
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            yield break;
        }
    }
}