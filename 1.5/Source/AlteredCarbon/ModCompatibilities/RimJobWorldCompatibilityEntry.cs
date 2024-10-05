using RimWorld;
using System;
using System.Reflection;
using Verse;

namespace AlteredCarbon
{
    public class RimJobWorldCompatibilityEntry : ModCompatibilityEntry
    {
        private RJWData rjwData;
        public RimJobWorldCompatibilityEntry() { }

        public RimJobWorldCompatibilityEntry(string modIdentifier) : base(modIdentifier) { }

        public override void FetchData(Pawn pawn)
        {
            rjwData = GetRjwData(pawn);
        }

        public override void SetData(Pawn pawn)
        {
            if (rjwData != null)
            {
                SetRjwData(pawn, rjwData);
            }
        }



        private RJWData GetRjwData(Pawn pawn)
        {
            RJWData rjwData = null;
            try
            {
                rjw.DataStore dataStore = Find.World.GetComponent<rjw.DataStore>();
                if (dataStore != null)
                {
                    rjwData = new RJWData();
                    rjw.PawnData pawnData = dataStore.GetPawnData(pawn);
                    if (pawnData != null)
                    {
                        foreach (FieldInfo fieldInfo in typeof(rjw.PawnData).GetFields())
                        {
                            try
                            {
                                FieldInfo newField = rjwData.GetType().GetField(fieldInfo.Name);
                                newField.SetValue(rjwData, fieldInfo.GetValue(pawnData));
                            }
                            catch { }
                        }
                    }

                }
                rjw.CompRJW comp = ThingCompUtility.TryGetComp<rjw.CompRJW>(pawn);
                if (comp != null)
                {
                    if (rjwData is null)
                    {
                        rjwData = new RJWData();
                    }
                    rjwData.quirksave = comp.quirksave;
                    if (rjwData.quirksave != null)
                    {
                        rjwData.quirksave = rjwData.quirksave.Replace("Fertile", "");
                        rjwData.quirksave = rjwData.quirksave.Replace("Infertile", "");
                    }
                    rjwData.orientation = (OrientationAC)(int)comp.orientation;
                    rjwData.NextHookupTick = comp.NextHookupTick;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error getting RJW data: " + ex.ToString());
            }
            return rjwData;
        }

        private void SetRjwData(Pawn pawn, RJWData rjwData)
        {
            try
            {
                rjw.DataStore dataStore = Find.World.GetComponent<rjw.DataStore>();
                if (dataStore != null)
                {
                    rjw.PawnData pawnData = dataStore.GetPawnData(pawn);
                    if (pawnData != null)
                    {
                        foreach (FieldInfo fieldInfo in typeof(RJWData).GetFields())
                        {
                            try
                            {
                                FieldInfo newField = pawnData.GetType().GetField(fieldInfo.Name);
                                newField.SetValue(pawnData, fieldInfo.GetValue(rjwData));
                            }
                            catch { }
                        }
                        if (pawnData.Hero)
                        {
                            foreach (Pawn otherPawn in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction)
                            {
                                if (otherPawn != pawn)
                                {
                                    rjw.PawnData otherPawnData = dataStore.GetPawnData(otherPawn);
                                    otherPawnData.Hero = false;
                                }
                            }
                        }
                    }
                }

                rjw.CompRJW comp = ThingCompUtility.TryGetComp<rjw.CompRJW>(pawn);
                if (comp != null)
                {
                    comp.quirksave = rjwData.quirksave;
                    comp.quirks = new System.Text.StringBuilder(comp.quirksave);

                    comp.orientation = (rjw.Orientation)(int)rjwData.orientation;
                    comp.NextHookupTick = rjwData.NextHookupTick;
                }
            }
            catch (Exception e)
            {
                Log.Error("Error setting RJW data: " + e.ToString());
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref rjwData, "rjwData");
        }

        public override void CopyFrom(ModCompatibilityEntry other)
        {
            rjwData = (other as RimJobWorldCompatibilityEntry).rjwData;
        }
    }
}