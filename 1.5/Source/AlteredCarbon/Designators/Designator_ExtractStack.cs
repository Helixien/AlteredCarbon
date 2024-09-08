using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
	public class Designator_ExtractStack : Designator
	{
		public override int DraggableDimensions => 2;

		public override DesignationDef Designation => AC_DefOf.AC_ExtractStackDesignation;

		public Designator_ExtractStack()
		{
			defaultLabel = "AC.DesignatorExtractStack".Translate();
			defaultDesc = "AC.DesignatorExtractStackDesc".Translate();
			activateSound = SoundDefOf.Tick_Tiny;
			icon = ContentFinder<Texture2D>.Get("UI/Gizmos/ExtractStacks");
			soundDragSustain = SoundDefOf.Designate_DragStandard;
			soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
			useMouseIcon = true;
			soundSucceeded = SoundDefOf.Designate_Claim;
			hotKey = KeyBindingDefOf.Misc11;
		}

		public override AcceptanceReport CanDesignateCell(IntVec3 c)
		{
			if (!c.InBounds(Map))
			{
				return false;
			}
			return !PawnsWithStacksInCell(c).Any() ? (AcceptanceReport)"AC.MessageMustDesignateHasStack".Translate() : (AcceptanceReport)true;
		}

		public override void DesignateSingleCell(IntVec3 c)
		{
			foreach (Thing item in PawnsWithStacksInCell(c))
			{
				DesignateThing(item);
			}
		}

		public override AcceptanceReport CanDesignateThing(Thing t)
		{
			if (Map.designationManager.DesignationOn(t, Designation) != null)
			{
				return false;
			}
			return t is Corpse corpse && corpse.InnerPawn.HasNeuralStack(out _)
				? (AcceptanceReport)true
				: (AcceptanceReport)false;
		}

		public override void DesignateThing(Thing t)
		{
            Map.designationManager.AddDesignation(new Designation(t, Designation));
		}

		private IEnumerable<Thing> PawnsWithStacksInCell(IntVec3 c)
		{
			if (c.Fogged(Map))
			{
				yield break;
			}
			List<Thing> thingList = c.GetThingList(Map);
			for (int i = 0; i < thingList.Count; i++)
			{
				if (CanDesignateThing(thingList[i]).Accepted)
				{
					yield return thingList[i];
				}
			}
		}
	}
}

