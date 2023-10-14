using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace AlteredCarbon
{
	[StaticConstructorOnStartup]
	public static class ModCompatibility
    {
        public static bool FacialAnimationsIsActive;
        public static bool AlienRacesIsActive;
        public static bool IndividualityIsActive;
        public static bool PsychologyIsActive;
        public static bool RimJobWorldIsActive;
        private static readonly MethodInfo tryGetRaceGroupDef;
        private static readonly Type raceGroupDef_HelperType;

        public static bool DubsBadHygieneActive;
        public static bool HelixienAlteredCarbonIsActive;
        public static bool VanillaSkillsExpandedIsActive;
        public static bool VanillaFactionsExpandedAncientsIsActive;
        public static bool VanillaRacesExpandedAndroidIsActive;
        static ModCompatibility()
        {
            HelixienAlteredCarbonIsActive = ModsConfig.IsActive("Hlx.UltratechAlteredCarbon");
            AlienRacesIsActive = ModsConfig.IsActive("erdelf.HumanoidAlienRaces");
            FacialAnimationsIsActive = ModsConfig.IsActive("Nals.FacialAnimation");
            IndividualityIsActive = ModLister.HasActiveModWithName("[SYR] Individuality");
            PsychologyIsActive = ModsConfig.IsActive("Community.Psychology.UnofficialUpdate");
            RimJobWorldIsActive = ModsConfig.IsActive("rim.job.world");
            if (RimJobWorldIsActive)
            {
                raceGroupDef_HelperType = AccessTools.TypeByName("RaceGroupDef_Helper");
                tryGetRaceGroupDef = raceGroupDef_HelperType.GetMethods().FirstOrDefault(x => x.Name == "TryGetRaceGroupDef");
            }
            DubsBadHygieneActive = ModsConfig.IsActive("Dubwise.DubsBadHygiene") || ModsConfig.IsActive("Dubwise.DubsBadHygiene.Lite");
            VanillaSkillsExpandedIsActive = ModsConfig.IsActive("vanillaexpanded.skills");
			if (VanillaSkillsExpandedIsActive)
            {
                AddVSEPassions();
            }
            VanillaFactionsExpandedAncientsIsActive = ModsConfig.IsActive("VanillaExpanded.VFEA");
            VanillaRacesExpandedAndroidIsActive = ModsConfig.IsActive("vanillaracesexpanded.android");
			if (VanillaRacesExpandedAndroidIsActive)
			{
				ACUtils.harmony.Patch(AccessTools.Method("VREAndroids.CharacterCardUtility_DoLeftSection_Patch:Prefix"),
				new HarmonyMethod(AccessTools.Method(typeof(ModCompatibility), "CharacterCardUtility_DoLeftSection_PatchPrefix")));
            }
        }

		public static bool CharacterCardUtility_DoLeftSection_PatchPrefix(Pawn __0, ref bool __result)
		{
			if (__0.HasCorticalStack())
			{
				__result = true;
                return false;
			}
			return true;
		}


        public static bool IsAndroid(this Pawn pawn)
		{
			if (VanillaRacesExpandedAndroidIsActive)
            {
                return IsAndroidInt(pawn);
            }
            return false;
		}

        private static bool IsAndroidInt(Pawn pawn)
        {
            if (VREAndroids.Utils.IsAndroid(pawn))
            {
                return true;
            }
            return false;
        }

		public static bool IsAwakenedAndroid(Pawn pawn)
		{
			if (pawn.IsAndroid())
			{
				return VREAndroids.Utils.IsAwakened(pawn);
            }
			return false;
		}

		public static RecipeDef GetRecipeForAndroid(RecipeDef recipe)
		{
			return VREAndroids.Utils.RecipeForAndroid(recipe);
		}

		public static bool IsAndroidGene(GeneDef gene)
		{
			return VREAndroids.Utils.IsAndroidGene(gene);
		}

        private static void AddVSEPassions()
        {
            Window_StackEditor.AllPassions.Clear();
            foreach (var def in DefDatabase<VSE.Passions.PassionDef>.AllDefs)
            {
                Window_StackEditor.AllPassions[def.index] = def.Icon;
            }
        }
		public static List<IExposable> GetExpertises(Pawn pawn)
        {
			VSE.ExpertiseTracker expertiseTracker = VSE.ExpertiseTrackers.Expertise(pawn);
			if (expertiseTracker != null)
			{
				return expertiseTracker.AllExpertise.Cast<IExposable>().ToList();
			}
			return null;
		}
        public static void SetExpertises(Pawn pawn, List<IExposable> expertises)
        {
            VSE.ExpertiseTracker expertiseTracker = VSE.ExpertiseTrackers.Expertise(pawn);
            if (expertiseTracker != null)
            {
				expertiseTracker.ClearExpertise();
				foreach (var expertise in expertises.Cast<VSE.ExpertiseRecord>())
				{
                    expertiseTracker.AllExpertise.Add(expertise);
                }
            }
        }

        public static List<Def> GetPowers(Pawn pawn)
        {
            VFEAncients.Pawn_PowerTracker powerTracker = VFEAncients.Pawn_PowerTracker.Get(pawn);
            if (powerTracker != null)
            {
                return powerTracker.AllPowers.Cast<Def>().ToList();
            }
            return null;
        }

		public static bool HasPowerAbility(Pawn pawn, VFECore.Abilities.AbilityDef abilityDef)
		{
            VFEAncients.Pawn_PowerTracker powerTracker = VFEAncients.Pawn_PowerTracker.Get(pawn);
            if (powerTracker != null)
            {
				foreach (var power in powerTracker.AllPowers)
				{
					if (power.abilities.Contains(abilityDef))
					{
						return true;
					}
				}
            }
			return false;
        }
        public static void SetPowers(Pawn pawn, List<Def> powers)
        {
            VFEAncients.Pawn_PowerTracker powerTracker = VFEAncients.Pawn_PowerTracker.Get(pawn);
            if (powerTracker != null)
            {
                foreach (var powerDef in powers.Cast<VFEAncients.PowerDef>())
                {
                    powerTracker.AddPower(powerDef);
                }
            }
        }

        public static Color GetSkinColorFirst(Pawn pawn)
		{
			AlienRace.AlienPartGenerator.AlienComp alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
			return alienComp != null ? alienComp.GetChannel("skin").first : Color.white;
		}

		public static Color GetSkinColorSecond(Pawn pawn)
		{
			AlienRace.AlienPartGenerator.AlienComp alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
			return alienComp != null ? alienComp.GetChannel("skin").second : Color.white;
		}
		public static void SetSkinColorFirst(Pawn pawn, Color color)
		{
			AlienRace.AlienPartGenerator.AlienComp alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
			if (alienComp != null)
			{
				alienComp.OverwriteColorChannel("skin", color, null);
			}
		}
		public static void SetSkinColorSecond(Pawn pawn, Color color)
		{
			AlienRace.AlienPartGenerator.AlienComp alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
			if (alienComp != null)
			{
				alienComp.OverwriteColorChannel("skin", null, color);
			}
		}


		public static Color GetHairColorFirst(Pawn pawn)
		{
			AlienRace.AlienPartGenerator.AlienComp alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
			return alienComp != null ? alienComp.GetChannel("hair").first : Color.white;
		}

		public static Color GetHairColorSecond(Pawn pawn)
		{
			AlienRace.AlienPartGenerator.AlienComp alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
			return alienComp != null ? alienComp.GetChannel("hair").second : Color.white;
		}
		public static void SetHairColorFirst(Pawn pawn, Color color)
		{
			AlienRace.AlienPartGenerator.AlienComp alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
			if (alienComp != null)
			{
				alienComp.OverwriteColorChannel("hair", color, null);
				pawn.story.HairColor = color;
			}
		}
		public static void SetHairColorSecond(Pawn pawn, Color color)
		{
			AlienRace.AlienPartGenerator.AlienComp alienComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(pawn);
			if (alienComp != null)
			{
				alienComp.OverwriteColorChannel("hair", null, color);
			}
		}

		public static void CopyBodyAddons(Pawn source, Pawn to)
		{
			AlienRace.AlienPartGenerator.AlienComp sourceComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(source);
			if (sourceComp != null && sourceComp.addonGraphics != null && sourceComp.addonVariants != null)
			{
				AlienRace.AlienPartGenerator.AlienComp toComp = ThingCompUtility.TryGetComp<AlienRace.AlienPartGenerator.AlienComp>(to);
				if (toComp != null)
				{
					toComp.addonGraphics = sourceComp.addonGraphics.ListFullCopy();
					toComp.addonVariants = sourceComp.addonVariants.ListFullCopy();
				}
			}
		}

		public static void CopyPrivateParts(Pawn source, Pawn to)
		{
            foreach (var hediff in source.health.hediffSet.hediffs.ToList())
            {
				if (hediff is rjw.Hediff_PartBaseNatural part)
				{
					var existingHediff = to.health.hediffSet.GetFirstHediffOfDef(part.def);
					if (existingHediff != null)
					{
						to.health.RemoveHediff(existingHediff);
					}
					var newPart = rjw.SexPartAdder.MakePart(part.def, to, part.Part);
					newPart.Severity = part.Severity;
					to.health.AddHediff(newPart);
				}
            }
        }

		public static List<Color> GetRacialColorPresets(ThingDef thingDef, string channelName)
		{
			ColorGenerator generator = null;
			AlienRace.ThingDef_AlienRace raceDef = thingDef as AlienRace.ThingDef_AlienRace;
			for (int ii = 0; ii < raceDef.alienRace.generalSettings.alienPartGenerator.colorChannels.Count(); ++ii)
			{
				if (raceDef.alienRace.generalSettings.alienPartGenerator.colorChannels[ii].name == channelName)
				{
					generator = raceDef.alienRace.generalSettings.alienPartGenerator.colorChannels[ii].entries.First().first;
					break;
				}
			}
			if (generator != null)
			{
				if (generator is ColorGenerator_Options options)
				{
					return options.options.Where((ColorOption option) =>
					{
						return option.only.a > -1.0f;
					}).Select((ColorOption option) =>
					{
						return option.only;
					}).ToList();
				}
				if (generator is ColorGenerator_Single single)
				{
					return new List<Color>() { single.color };
				}
				if (generator is ColorGenerator_White white)
				{
					return new List<Color>() { Color.white };
				}
			}
			return new List<Color>();
		}
        public static List<ThingDef> GetPermittedRaces()
        {
            List<ThingDef> excludedRaces = new List<ThingDef>();
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs.Where(def => def.category == ThingCategory.Pawn))
            {
                if (def.GetModExtension<ExcludeRacesModExtension>() is ExcludeRacesModExtension props)
                {
                    if (!props.canBeGrown)
                    {
                        excludedRaces.Add(def);
                    }
                }
            }
            return GetGrowableRaces(excludedRaces).OrderBy(entry => entry.LabelCap.RawText).ToList();
        }
        public static int GetSyrTraitsSexuality(Pawn pawn)
		{
			SyrTraits.CompIndividuality comp = ThingCompUtility.TryGetComp<SyrTraits.CompIndividuality>(pawn);
			return comp != null ? (int)comp.sexuality : -1;
		}
		public static float GetSyrTraitsRomanceFactor(Pawn pawn)
		{
			SyrTraits.CompIndividuality comp = ThingCompUtility.TryGetComp<SyrTraits.CompIndividuality>(pawn);
			return comp != null ? comp.RomanceFactor : -1f;
		}

		public static void CopyFacialFeatures(Pawn source, Pawn dest)
		{
			foreach (var comp in source.AllComps)
			{
				var type = comp.GetType();
				if (type.Namespace == "FacialAnimation")
				{
                    var faceTypeField = Traverse.Create(comp).Field("faceType");
                    if (faceTypeField != null && faceTypeField.FieldExists())
					{
						var colorTypeField = Traverse.Create(comp).Field("color");
						foreach (var compDest in dest.AllComps)
						{
							if (compDest.GetType() == type)
							{
								var faceType = faceTypeField.GetValue();
                                Traverse.Create(compDest).Field("faceType").SetValue(faceType);
                                Traverse.Create(compDest).Field("color").SetValue(colorTypeField.GetValue());
                                Traverse.Create(compDest).Field("pawn").SetValue(dest);
                                Traverse.Create(compDest).Field("prevGender").SetValue(dest.gender);
                                Traverse.Create(compDest).Field("prevFaceType").SetValue(faceType);
                            }
                        }
					}
				}
			}
		}
		public static void SetSyrTraitsSexuality(Pawn pawn, int sexuality)
		{
			SyrTraits.CompIndividuality comp = ThingCompUtility.TryGetComp<SyrTraits.CompIndividuality>(pawn);
			if (comp != null)
			{
				comp.sexuality = (SyrTraits.CompIndividuality.Sexuality)sexuality;
			}
		}
		public static void SetSyrTraitsRomanceFactor(Pawn pawn, float romanceFactor)
		{
			SyrTraits.CompIndividuality comp = ThingCompUtility.TryGetComp<SyrTraits.CompIndividuality>(pawn);
			if (comp != null)
			{
				comp.RomanceFactor = romanceFactor;
			}
		}

		public static PsychologyData GetPsychologyData(Pawn pawn)
		{
			Psychology.CompPsychology comp = ThingCompUtility.TryGetComp<Psychology.CompPsychology>(pawn);
			if (comp != null)
			{
				PsychologyData psychologyData = new PsychologyData();
				Psychology.Pawn_SexualityTracker sexualityTracker = comp.Sexuality;
				psychologyData.sexDrive = sexualityTracker.sexDrive;
				psychologyData.romanticDrive = sexualityTracker.romanticDrive;
				psychologyData.kinseyRating = sexualityTracker.kinseyRating;
                psychologyData.knownSexualities = sexualityTracker.knownSexualities;
				return psychologyData;
			}
			return null;
		}

		public static void SetPsychologyData(Pawn pawn, PsychologyData psychologyData)
		{
			Psychology.CompPsychology comp = ThingCompUtility.TryGetComp<Psychology.CompPsychology>(pawn);
			if (comp != null)
			{
				Psychology.Pawn_SexualityTracker sexualityTracker = new Psychology.Pawn_SexualityTracker(pawn)
				{
					sexDrive = psychologyData.sexDrive,
					romanticDrive = psychologyData.romanticDrive,
					kinseyRating = psychologyData.kinseyRating
				};
				sexualityTracker.knownSexualities = psychologyData.knownSexualities;
				comp.Sexuality = sexualityTracker;
			}
		}

		public static void UpdateGenderRestrictions(ThingDef raceDef, out bool allowMales, out bool allowFemales)
		{
			float maleProb = ((AlienRace.ThingDef_AlienRace)raceDef).alienRace.generalSettings.maleGenderProbability;
			allowMales = maleProb != 0.0f;
			allowFemales = maleProb != 1.0f;
		}

		public static List<HairDef> GetPermittedHair(ThingDef raceDef)
		{
			if (((AlienRace.ThingDef_AlienRace)raceDef).alienRace.styleSettings[typeof(HairDef)].styleTags == null)
			{
				//no good way to distinguish between alien specific hair and hair suitable for generic humanlikes
				return DefDatabase<HairDef>.AllDefs.ToList();
			}
			else
			{
				List<string> allowedTags = ((AlienRace.ThingDef_AlienRace)raceDef).alienRace.styleSettings[typeof(HairDef)].styleTags.ToList();
				return DefDatabase<HairDef>.AllDefs.Where(x => x.styleTags.Intersect(allowedTags).Any()).ToList();
			}
		}
        public static List<HeadTypeDef> GetAllowedHeadTypes(ThingDef raceDef)
        {
            AlienRace.ThingDef_AlienRace alienRace = raceDef as AlienRace.ThingDef_AlienRace;
            return alienRace.alienRace?.generalSettings?.alienPartGenerator?.headTypes?.Any() ?? false
                ? alienRace.alienRace.generalSettings.alienPartGenerator.headTypes
                : DefDatabase<HeadTypeDef>.AllDefsListForReading;
        }
        public static List<BodyTypeDef> GetAllowedBodyTypes(ThingDef raceDef)
		{
			AlienRace.ThingDef_AlienRace alienRace = raceDef as AlienRace.ThingDef_AlienRace;
			return alienRace.alienRace?.generalSettings?.alienPartGenerator?.bodyTypes?.Any() ?? false
				? alienRace.alienRace.generalSettings.alienPartGenerator.bodyTypes
				: DefDatabase<BodyTypeDef>.AllDefsListForReading;
		}
		public static List<ThingDef> GetGrowableRaces(List<ThingDef> excluded)
		{
			return DefDatabase<AlienRace.ThingDef_AlienRace>.AllDefs.Where(x => !excluded.Contains(x)).Cast<ThingDef>().ToList();
		}

        public static void FillThirstNeed(Pawn pawn, float value)
        {
            FillNeed(pawn.needs?.TryGetNeed<DubsBadHygiene.Need_Thirst>(), value);
        }

        public static void FillHygieneNeed(Pawn pawn, float value)
        {
            FillNeed(pawn.needs?.TryGetNeed<DubsBadHygiene.Need_Hygiene>(), value);
		}

        public static void FillBladderNeed(Pawn pawn, float value)
        {
            FillNeed(pawn.needs?.TryGetNeed<DubsBadHygiene.Need_Bladder>(), value);
        }
        private static void FillNeed(Need need, float value)
        {
            if (need != null)
            {
                if (need.MaxLevel > need.CurLevel)
                {
                    need.CurLevel += value;
                }
            }
        }

		public static bool RJWAllowsThisFor(this HediffDef hediffDef, Pawn pawn)
		{
			try
			{
				rjw.RacePartDef part = DefDatabase<rjw.RacePartDef>.GetNamedSilentFail(hediffDef.defName);
				if (part != null)
				{
					object[] parms = new object[2];
					parms[0] = pawn.kindDef;

					if ((bool)tryGetRaceGroupDef.Invoke(null, parms))
					{
						rjw.RaceGroupDef def = parms[1] as rjw.RaceGroupDef;
						if (hediffDef.IsBreasts())
						{
							if (def.femaleBreasts != null || def.maleBreasts != null)
							{
								if (def.femaleBreasts != null && def.femaleBreasts.Contains(hediffDef.defName))
								{
									return true;
								}
								else if (def.maleBreasts != null && def.maleBreasts.Contains(hediffDef.defName))
								{
									return true;
								}
								return false;
							}
						}

						if (hediffDef.IsGenitals())
						{
							if (def.femaleGenitals != null || def.maleGenitals != null)
							{
								if (def.femaleGenitals != null && def.femaleGenitals.Contains(hediffDef.defName))
								{
									return true;
								}
								else if (def.maleGenitals != null && def.maleGenitals.Contains(hediffDef.defName))
								{
									return true;
								}
								return false;
							}
						}
						if (hediffDef.IsAnus())
						{
							if (def.anuses != null)
							{
								if (!def.anuses.Contains(hediffDef.defName))
								{
									return false;
								}
							}
						}

						if (hediffDef.IsOvipositor())
						{
							if (!def.oviPregnancy)
							{
								return false;
							}
						}
					}
				}

				if (hediffDef.IsGenitals())
				{
					return hediffDef == rjw.Genital_Helper.average_penis || hediffDef == rjw.Genital_Helper.average_vagina;
				}
				else if (hediffDef.IsBreasts())
				{
					return hediffDef == rjw.Genital_Helper.average_breasts;
				}
				else if (hediffDef.IsAnus())
				{
					return hediffDef == rjw.Genital_Helper.average_anus;
				}
				else if (hediffDef.IsOvipositor())
				{
					return false;
				}
			}
			catch (Exception ex)
			{
				Log.Error("ERROR: " + ex);
			}
			return true;
		}

		private static bool IsOvipositor(this HediffDef hediffDef)
		{
			return hediffDef.defName.ToLower().Contains("ovipositor");
		}
		private static bool IsBreasts(this HediffDef hediffDef)
		{
			return hediffDef.defName.ToLower().Contains("breasts");
		}
		private static bool IsGenitals(this HediffDef hediffDef)
		{
			return hediffDef.defName.ToLower().Contains("penis") || hediffDef.defName.ToLower().Contains("vagina");
		}
		private static bool IsAnus(this HediffDef hediffDef)
		{
			return hediffDef.defName.ToLower().Contains("anus");
		}

		public static RJWData GetRjwData(Pawn pawn)
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

		public static void SetRjwData(Pawn pawn, RJWData rjwData)
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
	}

}
