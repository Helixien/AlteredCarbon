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
			var stack = t as PersonaStack;
			if (stack != null && stack.PersonaData.ContainsPersona && stack.PersonaData.faction.HostileTo(Faction.OfPlayer))
			{
				return true;
			}
			return false;
		}
	}
}

