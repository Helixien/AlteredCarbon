using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    public class PsychologyData : IExposable
    {
        public int kinseyRating;
        public float sexDrive;
        public float romanticDrive;
        public Dictionary<Pawn, int> knownSexualities;
        public PsychologyData()
        {

        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref this.kinseyRating, "kinseyRating", 0, false);
            Scribe_Values.Look(ref this.sexDrive, "sexDrive", 1f, false);
            Scribe_Values.Look(ref this.romanticDrive, "romanticDrive", 1f, false);
            Scribe_Collections.Look(ref this.knownSexualities, "knownSexualities", LookMode.Reference, LookMode.Value, ref this.knownSexualitiesWorkingKeys, ref this.knownSexualitiesWorkingValues);
        }

        private List<Pawn> knownSexualitiesWorkingKeys;
        private List<int> knownSexualitiesWorkingValues;
    }
}