using Verse;

namespace AlteredCarbon
{
    public class HediffCompProperties_MeleeWeapon : HediffCompProperties_VerbGiver
    {
        public GraphicData weaponGraphicData;
        public HediffCompProperties_MeleeWeapon()
        {
            this.compClass = typeof(HediffComp_MeleeWeapon);
        }
    }

    public class HediffComp_MeleeWeapon : HediffComp_VerbGiver
    {
        public HediffCompProperties_MeleeWeapon Props => props as HediffCompProperties_MeleeWeapon;
        private Graphic cachedGraphic;
        public Graphic Graphic
        {
            get
            {
                if (cachedGraphic == null)
                {
                    cachedGraphic = Props.weaponGraphicData.Graphic;
                }
                return cachedGraphic;
            }
        }
    }
}

