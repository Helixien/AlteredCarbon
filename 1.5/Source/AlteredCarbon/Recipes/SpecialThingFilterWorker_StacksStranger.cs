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
	public class SpecialThingFilterWorker_StacksStranger : SpecialThingFilterWorker
	{
		public override bool Matches(Thing t)
		{
			var stack = t as NeuralStack;
			if (stack != null && stack.NeuralData.ContainsData && stack.NeuralData.faction != Faction.OfPlayer && !stack.NeuralData.faction.HostileTo(Faction.OfPlayer))
            {
				return true;
            }
			return false;
		}
	}
}

