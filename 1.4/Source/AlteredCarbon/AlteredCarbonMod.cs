using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
    class AlteredCarbonMod : Mod
    {
        public static ModContentPack modContentPack;
        public AlteredCarbonMod(ModContentPack pack) : base(pack)
        {
            modContentPack = pack;
        }
    }
}
