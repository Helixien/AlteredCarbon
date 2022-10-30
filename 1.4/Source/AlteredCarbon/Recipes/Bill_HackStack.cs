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
    public class Bill_HackStack : Bill_Production
    {
        public CorticalStack corticalStack;
        public Bill_HackStack()
        {

        }
        public Bill_HackStack(CorticalStack corticalStack, RecipeDef recipe, Precept_ThingStyle precept = null)
            : base(recipe, precept)
        {
            this.corticalStack = corticalStack;
        }
        public override string Label => base.Label + " (" + (corticalStack?.PersonaData?.PawnNameColored ?? "Destroyed".Translate()) + ")";
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref corticalStack, "corticalStack");
        }

        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            base.Notify_IterationCompleted(billDoer, ingredients);
            this.billStack.Bills.Remove(this);
        }
        public override Bill Clone()
		{
            var obj = base.Clone() as Bill_HackStack;
            obj.corticalStack = corticalStack;
			return obj;
		}
	}
}

