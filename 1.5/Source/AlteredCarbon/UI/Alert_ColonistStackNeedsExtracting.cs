using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace AlteredCarbon
{
    public class Alert_ColonistStackNeedsExtracting : Alert_Critical
    {
        private List<Corpse> corpsesWithStacksResult = new List<Corpse>();
        private List<Corpse> CorpsesWithStacks
        {
            get
            {
                corpsesWithStacksResult.Clear();
                foreach (Map item in Find.Maps)
                {
                    foreach (var corpse in item.listerThings.ThingsInGroup(ThingRequestGroup.Corpse).OfType<Corpse>())
                    {
                        if (corpse.InnerPawn.IsColonist && corpse.InnerPawn.HasNeuralStack())
                        {
                            corpsesWithStacksResult.Add(corpse);
                        }
                    }
                }
                return corpsesWithStacksResult;
            }
        }

        public override string GetLabel()
        {
            return "AC.ColonistStackNeedsExtracting".Translate();
        }

        public override TaggedString GetExplanation()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (Corpse corpse in CorpsesWithStacks)
            {
                stringBuilder.AppendLine("  - " + corpse.InnerPawn.NameShortColored.Resolve());
            }
            return "AC.ColonistStackNeedsExtractingDesc".Translate(stringBuilder.ToString());
        }

        public override AlertReport GetReport()
        {
            return AlertReport.CulpritsAre(CorpsesWithStacks.Cast<Thing>().ToList());
        }
    }
}