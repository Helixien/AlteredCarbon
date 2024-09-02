using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AlteredCarbon
{
    public class Recipe_WipeActiveNeuralStack : Recipe_OperateOnNeuralStack
    {
        public override void Notify_IterationCompleted(Pawn billDoer, List<Thing> ingredients)
        {
            base.Notify_IterationCompleted(billDoer, ingredients);
            var stack = NeuralStack(billDoer);
            AC_DefOf.Message_NegativeEvent.PlayOneShot(stack);
            if (stack.NeuralData.faction != null && billDoer != null && billDoer.Faction != null 
                && billDoer.Faction != stack.NeuralData.faction)
            {
                stack.EmptyStack(billDoer, true);
            }
            else
            {
                stack.EmptyStack(billDoer);
            }
        }
    }
}

