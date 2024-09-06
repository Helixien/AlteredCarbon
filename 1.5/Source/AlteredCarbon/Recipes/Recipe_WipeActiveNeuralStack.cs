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
            var stack = Thing(billDoer) as NeuralStack;
            AC_DefOf.Message_NegativeEvent.PlayOneShot(stack);
            var neuralData = NeuralData(billDoer);
            if (neuralData.faction != null && billDoer != null && billDoer.Faction != null 
                && billDoer.Faction != neuralData.faction)
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

