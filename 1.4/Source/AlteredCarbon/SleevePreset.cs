using Verse;

namespace AlteredCarbon
{
    public class SleevePreset : IExposable
    {
        public Pawn sleeve;
        public void ExposeData()
        {
            Scribe_Deep.Look(ref sleeve, "sleeve");
        }
    }
}
