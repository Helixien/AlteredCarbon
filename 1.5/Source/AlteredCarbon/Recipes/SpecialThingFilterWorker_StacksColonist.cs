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

	public class SpecialThingFilterWorker_StacksColonist : SpecialThingFilterWorker
	{
		public override bool Matches(Thing t)
		{
			var stack = t as NeuralStack;
			if (stack != null && stack.NeuralData.ContainsData && stack.NeuralData.faction == Faction.OfPlayer)
			{
				return true;
			}
			return false;
		}
	}
}

