using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AlteredCarbon
{
    public class VanillaAnomalyInsanityCompatibilityEntry : ModCompatibilityEntry
    {
        public VanillaAnomalyInsanityCompatibilityEntry() { }

        public VanillaAnomalyInsanityCompatibilityEntry(string modIdentifier) : base(modIdentifier) { }

        public bool shouldBeVisible;
        public List<IExposable> records = new List<IExposable>();
        public HashSet<Pawn> killedShamblers = new HashSet<Pawn>();
        public TraitDef rehumanizedTrait;
        public float curLevel;

        public override void FetchData(Pawn pawn)
        {
            var need = pawn.needs?.TryGetNeed<VAEInsanity.Need_Sanity>();
            if (need != null)
            {
                shouldBeVisible = need.shouldBeVisible;
                records = need.records.Cast<IExposable>().ToList();
                killedShamblers = need.killedShamblers?.ToHashSet() ?? new HashSet<Pawn>();
                rehumanizedTrait = need.rehumanizedTrait;
                curLevel = need.CurLevel;
            }
        }

        public override void SetData(Pawn pawn)
        {
            var need = pawn.needs?.TryGetNeed<VAEInsanity.Need_Sanity>();
            if (need != null)
            {
                need.shouldBeVisible = shouldBeVisible;
                need.records = records.Cast<VAEInsanity.SanityChangeRecord>().ToList();
                need.killedShamblers = killedShamblers.ToHashSet() ?? new HashSet<Pawn>();
                need.rehumanizedTrait = rehumanizedTrait;
                need.CurLevel = curLevel;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref shouldBeVisible, "shouldBeVisible");
            Scribe_Values.Look(ref curLevel, "curLevel");
            Scribe_Collections.Look(ref records, "records", LookMode.Deep);
            Scribe_Collections.Look(ref killedShamblers, "killedShamblers", LookMode.Reference);
            Scribe_Defs.Look(ref rehumanizedTrait, "rehumanizedTrait");
        }

        public override void CopyFrom(ModCompatibilityEntry other)
        {
            var otherData = (other as VanillaAnomalyInsanityCompatibilityEntry);
            curLevel = otherData.curLevel;
            shouldBeVisible = otherData.shouldBeVisible;
            records = otherData.records.CopyList();
            killedShamblers = otherData.killedShamblers?.ToHashSet() ?? new HashSet<Pawn>();
            rehumanizedTrait = otherData.rehumanizedTrait;
        }
    }
}