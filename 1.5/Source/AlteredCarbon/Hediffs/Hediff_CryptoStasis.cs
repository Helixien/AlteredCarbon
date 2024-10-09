using RimWorld;
using Verse;

namespace AlteredCarbon
{
    public class Hediff_CryptoStasis : HediffWithComps
    {
        public override bool ShouldRemove => pawn.CurrentBed() is not Building_SleeveCasket;
    }
}