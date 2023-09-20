using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    public class Hediff_CortexOverseer : HediffWithComps
    {
        public Need_Suppression suppresion;
        public Need_Suppression Suppresion => suppresion ??= pawn.needs.TryGetNeed<Need_Suppression>();

        public bool activated = true;

        public override void Tick()
        {
            base.Tick();
            if (activated && pawn.IsSlave)
            {
                Suppresion.CurLevel = Mathf.Max(Suppresion.CurLevel, 0.8f);
            }
        }

        public override string LabelInBrackets => activated ? "" : "AC.Deactivated".Translate();

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var g in base.GetGizmos())
            {
                yield return g;
            }

            if (activated && pawn.Faction == Faction.OfPlayer)
            {
                yield return new Command_Action
                {
                    defaultLabel = "AC.Detonate".Translate(),
                    defaultDesc = "AC.DetonateDesc".Translate(),
                    icon = ContentFinder<Texture2D>.Get("UI/Icons/DetonateOverseer"),
                    action = delegate
                    {
                        Detonate();
                    }
                };
            }
        }

        public override void PostRemoved()
        {
            base.PostRemoved();
            if (pawn.MapHeld != null && pawn.health.hediffSet.hediffs.Any(x => x.def == HediffDefOf.MissingBodyPart && x.part == part))
            {
                Detonate();
            }
        }

        private void Detonate()
        {
            if (part != null && !pawn.health.hediffSet.hediffs.Any(x => x.def == HediffDefOf.MissingBodyPart && x.part == part))
            {
                pawn.health.AddHediff(HediffDefOf.MissingBodyPart, part);
            }
            GenExplosion.DoExplosion(pawn.PositionHeld, pawn.MapHeld, 1.9f, DamageDefOf.Bomb, null);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref activated, "activated", true);
        }
    }
}

