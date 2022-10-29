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
	public class SpecialThingFilterWorker_StacksHostile : SpecialThingFilterWorker
	{
		public override bool Matches(Thing t)
		{
			var stack = t as CorticalStack;
			if (stack != null && stack.PersonaData.ContainsInnerPersona && stack.PersonaData.faction.HostileTo(Faction.OfPlayer))
			{
				return true;
			}
			return false;
		}
	}
}

