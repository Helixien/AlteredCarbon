using RimWorld;
using System.Collections.Generic;
using Verse;

namespace AlteredCarbon
{
    public class CompProperties_ApparelAbilities : CompProperties
    {
        public List<AbilityDef> abilityDefs;

        public CompProperties_ApparelAbilities()
        {
            this.compClass = typeof(CompApparelAbilities);
        }
    }

    public class CompApparelAbilities : ThingComp
    {
        private CompProperties_ApparelAbilities Props => (CompProperties_ApparelAbilities)props;

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            for (int i = 0; i < Props.abilityDefs.Count; i++)
            {
                pawn.abilities.GainAbility(Props.abilityDefs[i]);
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            for (int i = 0; i < Props.abilityDefs.Count; i++)
            {
                pawn.abilities.RemoveAbility(Props.abilityDefs[i]);
            }
        }
    }
}