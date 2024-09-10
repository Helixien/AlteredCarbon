﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    public class Command_ActionOnBiocoding : Command_ActionOnThing
    {
        public Command_ActionOnBiocoding(Building_NeuralEditor neuralEditor, CommandInfo info) : base(neuralEditor, info)
        {
        }

        public override HashSet<Thing> Things
        {
            get
            {
                var things = new HashSet<Thing>();
                foreach (var thing in neuralEditor.Map.listerThings.AllThings)
                {
                    var comp = thing.TryGetComp<CompBiocodable>();
                    if (comp != null && comp is not CompBladelinkWeapon && comp.CodedPawn != null)
                    {
                        things.Add(thing);
                    }
                }
                return things.Where(x => x.PositionHeld.Fogged(x.MapHeld) is false).ToHashSet();
            }
        }

        public override IEnumerable<FloatMenuOption> FloatMenuOptions
        {
            get
            {
                var things = Things;
                foreach (var thing in Things)
                {
                    if (things.Contains(thing))
                    {
                        yield return new FloatMenuOption(thing.LabelCap, delegate ()
                        {
                            info.action(thing);
                            Find.Targeter.StopTargeting();
                        }, iconThing: thing, iconColor: Color.white);
                    }
                }
            }
        }
    }
}