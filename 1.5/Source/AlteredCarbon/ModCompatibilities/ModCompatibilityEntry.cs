using Verse;

namespace AlteredCarbon
{
    public abstract class ModCompatibilityEntry : IExposable
    {
        public bool isActive;
        public string modIdentifier;
        public ModCompatibilityEntry() { }
        public ModCompatibilityEntry(string modIdentifier)
        {
            isActive = ModsConfig.IsActive(modIdentifier);
            this.modIdentifier = modIdentifier;
        }

        public abstract void FetchData(Pawn pawn);
        public abstract void SetData(Pawn pawn);
        public abstract void CopyFrom(ModCompatibilityEntry other);
        public virtual void ExposeData()
        {
            Scribe_Values.Look(ref modIdentifier, "modIdentifier");
            isActive = ModsConfig.IsActive(modIdentifier);
        }
    }
}