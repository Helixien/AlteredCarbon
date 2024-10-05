using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AlteredCarbon
{
    public class AspirationsCompatibilityEntry : ModCompatibilityEntry
    {
        private List<string> aspirations = new List<string>();
        private List<int> aspirationsCompletedTicks = new List<int>();
        public AspirationsCompatibilityEntry() { }
        public AspirationsCompatibilityEntry(string modIdentifier) : base(modIdentifier) { }

        public override void FetchData(Pawn pawn)
        {
            aspirations = GetAspirations(pawn);
            aspirationsCompletedTicks = GetCompletedAspirations(pawn);
        }

        public override void SetData(Pawn pawn)
        {
            if (aspirations != null)
            {
                SetAspirations(pawn, aspirations);
            }
            if (aspirationsCompletedTicks != null)
            {
                SetCompletedAspirations(pawn, aspirationsCompletedTicks);
            }
        }

        private List<string> GetAspirations(Pawn pawn)
        {
            var need = pawn?.needs?.TryGetNeed<VAspirE.Need_Fulfillment>();
            if (need != null)
            {
                var list = new List<string>();
                foreach (var asp in need.Aspirations)
                {
                    list.Add(asp.defName);
                }
                return list;
            }
            return null;
        }

        private void SetAspirations(Pawn pawn, List<string> aspirations)
        {
            var need = pawn?.needs?.TryGetNeed<VAspirE.Need_Fulfillment>();
            if (need != null)
            {
                need.Aspirations.Clear();
                if (aspirations != null)
                {
                    foreach (var aspiration in aspirations)
                    {
                        var def = DefDatabase<VAspirE.AspirationDef>.GetNamed(aspiration);
                        if (def != null)
                        {
                            need.Aspirations.Add(def);
                        }
                    }
                }
            }
        }

        private List<int> GetCompletedAspirations(Pawn pawn)
        {
            var need = pawn?.needs?.TryGetNeed<VAspirE.Need_Fulfillment>();
            if (need != null)
            {
                return need.completedTicks.ToList();
            }
            return null;
        }

        private void SetCompletedAspirations(Pawn pawn, List<int> completedTicks)
        {
            var need = pawn?.needs?.TryGetNeed<VAspirE.Need_Fulfillment>();
            if (need != null)
            {
                need.completedTicks.Clear();
                if (completedTicks != null)
                {
                    foreach (var ticks in completedTicks)
                    {
                        need.completedTicks.Add(ticks);
                    }
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref aspirations, "aspirations", LookMode.Value);
            Scribe_Collections.Look(ref aspirationsCompletedTicks, nameof(aspirationsCompletedTicks), LookMode.Value);
        }

        public override void CopyFrom(ModCompatibilityEntry other)
        {
            var otherData = (other as AspirationsCompatibilityEntry);
            aspirations = otherData.aspirations.CopyList();
            aspirationsCompletedTicks = otherData.aspirationsCompletedTicks.CopyList();
        }
    }
}