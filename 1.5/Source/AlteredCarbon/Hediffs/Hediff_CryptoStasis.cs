using RimWorld;
using Verse;

namespace AlteredCarbon
{
    public class Hediff_CryptoStasis : HediffWithComps
    {
        public override bool ShouldRemove => pawn.CurrentBed() is not Building_SleeveCasket;

        public override void PostRemoved()
        {
            base.PostRemoved();
            if (Hediff_RemoteStack.lookingForConnectStatus)
            {
                return;
            }
            pawn.health.AddHediff(HediffDefOf.CryptosleepSickness);
        }
    }
}