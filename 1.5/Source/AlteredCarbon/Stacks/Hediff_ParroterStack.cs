﻿using System.Collections.Generic;
using Verse;

namespace AlteredCarbon
{
    [HotSwappable]
    public class Hediff_ParroterStack : Hediff_Implant
    {
        public PersonaData originalPawnData;
        public PersonaData needlePawnData;
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref originalPawnData, "originalPawnData");
            Scribe_Deep.Look(ref needlePawnData, "needlePawnData");
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            yield break;
        }
    }
}