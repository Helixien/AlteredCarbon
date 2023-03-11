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
        public static AlteredCarbonSettings settings;
        public static ModContentPack modContentPack;
        public AlteredCarbonMod(ModContentPack pack) : base(pack)
        {
            settings = GetSettings<AlteredCarbonSettings>();
            modContentPack = pack;
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            settings.DoSettingsWindowContents(inRect);
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            ACUtils.ApplySettings();

        }
        public override string SettingsCategory()
        {
            return this.Content.Name;
        }
    }
}
