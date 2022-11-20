using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
	public class PlaceWorker_MatrixStorage : PlaceWorker
	{
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            if (map.listerThings.ThingsOfDef(AC_Extra_DefOf.AC_StackArray).Any())
            {
                return "AC.OneMatrixStorageCanBeBuild".Translate();
            }
            return true;
        }
    }
}

