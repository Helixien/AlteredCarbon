using RimWorld;
using Verse;

namespace AlteredCarbon
{
    public class ThingFilterBiocodable : ThingFilter
    {
        public override bool Allows(Thing t)
        {
            var comp = t.TryGetComp<CompBiocodable>();
            if (comp != null && comp is not CompBladelinkWeapon && comp.Biocoded)
            {
                return true;
            }
            return false;
        }
    }
}

