using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    [HotSwappable]
    public class NeuralData : IExposable
    {
        public NeuralData neuralDataRewritten;

        public static bool debug => false;
        public ThingDef sourceStack;
        public Name name;
        public PawnKindDef kindDef;
        public Pawn hostPawn;
        private HostilityResponseMode hostilityMode;
        private Dictionary<Map, Area> allowedAreas = new Dictionary<Map, Area>();
        private MedicalCareCategory medicalCareCategory;
        private bool selfTend;
        public long ageBiologicalTicks;
        public long ageChronologicalTicks;
        private List<TimeAssignmentDef> times;
        private FoodPolicy foodPolicy;
        private ApparelPolicy apparelPolicy;
        private DrugPolicy drugPolicy;
        public Faction faction;
        public bool isFactionLeader;
        public List<Thought_Memory> thoughts;
        public List<Trait> traits;
        private List<DirectPawnRelation> relations;

        private List<Pawn> relatedPawns;
        public List<SkillRecord> skills;

        public BackstoryDef childhood;
        public BackstoryDef adulthood;

        public string title;
        public bool everSeenByPlayer;
        public bool canGetRescuedThought = true;
        public Pawn relativeInvolvedInRescueQuest;
        public MarriageNameChange nextMarriageNameChange;
        public bool hidePawnRelations;

        private Dictionary<WorkTypeDef, int> priorities;
        public GuestStatus guestStatusInt;
        private PrisonerInteractionModeDef interactionMode;
        private SlaveInteractionModeDef slaveInteractionMode;
        public Faction hostFactionInt;
        private JoinStatus joinStatus;
        private Faction slaveFactionInt;
        private string lastRecruiterName;
        private int lastRecruiterOpinion;
        private bool hasOpinionOfLastRecruiter;
        private float lastRecruiterResistanceReduce;
        private bool releasedInt;
        private int ticksWhenAllowedToEscapeAgain;
        public IntVec3 spotToWaitInsteadOfEscaping;
        public int lastPrisonBreakTicks = -1;
        public bool everParticipatedInPrisonBreak;
        public float resistance = -1f;
        public float will = -1f;
        public Ideo ideoForConversion;
        private bool everEnslaved = false;
        public bool getRescuedThoughtOnUndownedBecauseOfPlayer;
        public bool recruitable;

        private DefMap<RecordDef, float> records = new DefMap<RecordDef, float>();
        private Battle battleActive;
        private int battleExitTick;

        public List<Hediff> savedHediffs = new List<Hediff>();

        public bool ContainsData => hostPawn != null || name != null;

        public static HashSet<Pawn> dummyPawns = new HashSet<Pawn>();

        // original pawn data before sleeving
        private Gender originalGender;
        private ThingDef originalRace;
        private XenotypeDef originalXenotypeDef;
        private string originalXenotypeName;

        public Gender OriginalGender { get => originalGender; set => originalGender = value; }
        public ThingDef OriginalRace { get => originalRace; set => originalRace = value; }
        public XenotypeDef OriginalXenotypeDef { get => originalXenotypeDef; set => originalXenotypeDef = value; }
        public string OriginalXenotypeName { get => originalXenotypeName; set => originalXenotypeName = value; }

        private Gender? dummyGender;

        private int pawnID;

        // Royalty
        private List<RoyalTitle> royalTitles;
        private Dictionary<Faction, int> favor = new Dictionary<Faction, int>();
        private Dictionary<Faction, Pawn> heirs = new Dictionary<Faction, Pawn>();
        private List<Thing> bondedThings = new List<Thing>();
        private List<FactionPermit> factionPermits = new List<FactionPermit>();

        private int? psylinkLevel;
        private float currentEntropy;
        public bool limitEntropyAmount = true;
        private float currentPsyfocus = -1f;
        private float targetPsyfocus = 0.5f;

        // VE
        private List<AbilityDef> abilities = new List<AbilityDef>();
        private List<VFECore.Abilities.AbilityDef> VEAbilities = new List<VFECore.Abilities.AbilityDef>();
        private Hediff VPE_PsycastAbilityImplant;

        // Ideology
        public Ideo ideo;
        public Color? favoriteColor;
        public int joinTick;
        public List<Ideo> previousIdeos;
        public float certainty;
        public Precept_RoleMulti precept_RoleMulti;
        public Precept_RoleSingle precept_RoleSingle;

        // [SYR] Individuality
        private int sexuality;
        private float romanceFactor;

        // Psychology
        private PsychologyData psychologyData;
        // RJW
        private RJWData rjwData;
        // Vanilla Skills Expanded
        private List<IExposable> expertiseRecords;

        // Vanilla Aspiration Expanded
        private List<string> aspirations = new List<string>();
        private List<int> aspirationsCompletedTicks = new List<int>();

        // misc
        public bool? diedFromCombat;
        public bool isCopied = false;
        public int stackGroupID = -1;
        public int? lastTimeBackedUp;

        public int editTime;
        public float stackDegradation;
        public float stackDegradationToAdd;
        public Pawn dummyPawn;
        public NeuralData()
        {
            this.stackGroupID = AlteredCarbonManager.Instance.stacksRelationships.Count + 1;
            this.lastTimeBackedUp = GenTicks.TicksAbs;
        }

        public static Pawn lastDummyPawn;

        public Pawn DummyPawn
        {
            get
            {
                if (dummyPawn is null)
                {
                    RefreshDummyPawn();
                }
                lastDummyPawn = dummyPawn;
                return dummyPawn;
            }
        }
        public int PawnID => hostPawn != null ? hostPawn.thingIDNumber : pawnID;
        public void RefreshDummyPawn()
        {
            if (dummyPawn is null)
            {
                long ticks = (long)Mathf.FloorToInt(18f * 3600000f);
                if (ageBiologicalTicks != default)
                {
                    ticks = ageBiologicalTicks;
                }
                else if (hostPawn != null)
                {
                    ticks = ageBiologicalTicks = hostPawn.ageTracker.AgeBiologicalTicks;
                }
                dummyPawn = AC_Utils.CreateEmptyPawn(hostPawn?.kindDef ?? kindDef ?? PawnKindDefOf.Colonist,
                    faction, OriginalRace ?? ThingDefOf.Human, ticks, OriginalXenotypeDef != null
                    ? OriginalXenotypeDef : XenotypeDefOf.Baseliner);
                dummyPawn.gender = dummyGender ?? originalGender;
            }
            dummyPawns.Add(dummyPawn);
            OverwritePawn(dummyPawn, null, overwriteOriginalPawn: false, copyFromOrigPawn: hostPawn != null
                && hostPawn.Dead is false && hostPawn.IsEmptySleeve() is false);
            if (hostPawn != null)
            {
                dummyGender = hostPawn.gender;
                AC_Utils.CopyBody(hostPawn, dummyPawn, copyAgeInfo: true, copyGenesFully: true);
                dummyPawn.RefreshGraphic();
            }
            DummyPawnCleanup();
            AssignDummyPawnReferences();
        }

        private void DummyPawnCleanup()
        {
            dummyPawn.genes.GenesListForReading.Where(x => x.def.aptitudes.NullOrEmpty() is false
            || x.def.forcedTraits.NullOrEmpty() is false || x.def.passionMod is not null).ToList()
                .ForEach(x => dummyPawn.genes.RemoveGene(x));
            dummyPawn.story.backstoriesCache = null;
            dummyPawn.Notify_DisabledWorkTypesChanged();
            if (this.skills != null)
            {
                foreach (var skill in this.skills)
                {
                    skill.cachedPermanentlyDisabled = BoolUnknown.Unknown;
                    skill.cachedTotallyDisabled = BoolUnknown.Unknown;
                }
            }
            if (hostPawn != null && (hostPawn.IsEmptySleeve() || hostPawn.Dead))
            {
                dummyPawn.health.healthState = PawnHealthState.Dead;
            }
        }

        public TaggedString PawnNameColored
        {
            get
            {
                var title = TitleShort;
                var pawnName = name?.ToStringShort.Colorize(PawnNameColorUtility.PawnNameColorOf(DummyPawn));
                return title.NullOrEmpty() ? pawnName : pawnName + ", " + title.CapitalizeFirst();
            }
        }
        public string TitleShort
        {
            get
            {
                if (title != null)
                {
                    return title;
                }
                return adulthood != null ? adulthood.TitleShortFor(OriginalGender)
                    : childhood != null ? childhood.TitleShortFor(OriginalGender) : "";
            }
        }

        public StackGroupData StackGroupData
        {
            get
            {
                if (!AlteredCarbonManager.Instance.stacksRelationships.TryGetValue(stackGroupID, out var stackData))
                {
                    AlteredCarbonManager.Instance.stacksRelationships[stackGroupID] = stackData = new StackGroupData();
                }
                return stackData;
            }
        }

        private Color GetFactionRelationColor(Faction faction)
        {
            if (faction == null)
            {
                return Color.white;
            }
            return faction.Color;

            if (faction.IsPlayer)
            {
                return faction.Color;
            }
            switch (faction.RelationKindWith(Faction.OfPlayer))
            {
                case FactionRelationKind.Ally:
                    return ColoredText.FactionColor_Ally;
                case FactionRelationKind.Hostile:
                    return ColoredText.FactionColor_Hostile;
                case FactionRelationKind.Neutral:
                    return ColoredText.FactionColor_Neutral;
                default:
                    return faction.Color;
            }
        }

        public void AppendInfoStack(StringBuilder stringBuilder)
        {
            if (this.ContainsData)
            {
                if (this.faction != null)
                {
                    stringBuilder.AppendLineTagged("AC.Faction".Translate() + ": " + this.faction.NameColored);
                }
                if (ModCompatibility.AlienRacesIsActive && this.OriginalRace != null)
                {
                    stringBuilder.AppendLineTagged("AC.Race".Translate() + ": " + this.OriginalRace.LabelCap);
                }
                if (this.OriginalXenotypeName != null)
                {
                    stringBuilder.AppendLineTagged("AC.Xenotype".Translate() + ": " + this.OriginalXenotypeName);
                }
                else if (this.OriginalXenotypeDef != null)
                {
                    stringBuilder.AppendLineTagged("AC.Xenotype".Translate() + ": " + this.OriginalXenotypeDef.LabelCap);
                }

                if (this.childhood != null)
                {
                    stringBuilder.Append("AC.Childhood".Translate() + ": " + this.childhood.title.CapitalizeFirst() + "\n");
                }

                if (this.adulthood != null)
                {
                    stringBuilder.Append("AC.Adulthood".Translate() + ": " + this.adulthood.title.CapitalizeFirst() + "\n");
                }
                stringBuilder.Append("AC.AgeChronologicalTicks".Translate() + ": " + (int)(this.ageChronologicalTicks / 3600000) + "\n");
                if (this.stackDegradation > 0)
                {
                    stringBuilder.AppendLineTagged("AC.StackDegradation".Translate((TaggedString)(this.stackDegradation.ToStringPercent().Colorize(Color.red))));
                }
            }
        }

        public void CopyFromPawn(Pawn pawn, ThingDef sourceStack, bool copyRaceGenderInfo = false, bool canBackupPsychicStuff = true)
        {
            this.hostPawn = pawn;
            this.sourceStack = sourceStack ?? AC_DefOf.AC_ActiveNeuralStack;
            name = GetNameCopy(pawn.Name);
            this.kindDef = pawn.kindDef;
            if (pawn.playerSettings != null)
            {
                hostilityMode = pawn.playerSettings.hostilityResponse;
                allowedAreas = pawn.playerSettings.allowedAreas;
                medicalCareCategory = pawn.playerSettings.medCare;
                selfTend = pawn.playerSettings.selfTend;
            }
            if (pawn.ageTracker != null)
            {
                ageChronologicalTicks = pawn.ageTracker.AgeChronologicalTicks;
                ageBiologicalTicks = pawn.ageTracker.AgeBiologicalTicks;
            }
            foodPolicy = pawn.foodRestriction?.CurrentFoodPolicy;
            apparelPolicy = pawn.outfits?.curApparelPolicy;
            drugPolicy = pawn.drugs?.CurrentPolicy;
            times = pawn.timetable?.times.CopyList();
            thoughts = pawn.needs?.mood?.thoughts?.memories?.Memories.CopyList();
            faction = pawn.Faction;
            if (pawn.Faction?.leader == pawn)
            {
                isFactionLeader = true;
            }
            traits = new List<Trait>();
            if (pawn.story?.traits != null)
            {
                foreach (var trait in pawn.story.traits.allTraits)
                {
                    if (trait.sourceGene is null && trait.suppressedByGene is null)
                    {
                        var extension = this.sourceStack.GetModExtension<StackSavingOptionsModExtension>();
                        if (extension != null && extension.ignoresTraits != null && extension.ignoresTraits.Contains(trait.def.defName))
                        {
                            continue;
                        }
                        traits.Add(new Trait(trait.def, trait.degree));
                    }
                }
            }
            if (pawn.relations != null)
            {
                if (debug) Log.Message("CopyFromPawn START");

                everSeenByPlayer = pawn.relations.everSeenByPlayer;
                canGetRescuedThought = pawn.relations.canGetRescuedThought;
                relativeInvolvedInRescueQuest = pawn.relations.relativeInvolvedInRescueQuest;
                nextMarriageNameChange = pawn.relations.nextMarriageNameChange;
                hidePawnRelations = pawn.relations.hidePawnRelations;

                relations = pawn.relations.DirectRelations.CopyList();
                if (debug)
                {
                    Log.Message("CopyFromPawn: relations: " + string.Join(", ", relations.Select(x => x.def + " - " + x.otherPawn.GetFullName())));
                    foreach (var rel in relations)
                    {
                        foreach (var other in rel.otherPawn.relations.DirectRelations)
                        {
                            Log.Message("CopyFromPawn: other rel: " + rel.otherPawn.GetFullName() + " - " + other.def + " - " + other.otherPawn.GetFullName());
                        }
                    }
                }


                relatedPawns = pawn.relations.PotentiallyRelatedPawns.ToList();
                if (debug)
                {
                    Log.Message("CopyFromPawn: relatedPawns: " + string.Join(", ", relatedPawns.Select(x => x.GetFullName())));
                    //foreach (var rel in relatedPawns)
                    //{
                    //    foreach (var other in rel.relations.DirectRelations)
                    //    {
                    //        Log.Message("CopyFromPawn: other rel: " + rel.GetFullName() + " - " + other.def + " - " + other.otherPawn.GetFullName());
                    //    }
                    //}
                }
                foreach (Pawn otherPawn in pawn.relations.RelatedPawns)
                {
                    foreach (PawnRelationDef rel2 in pawn.GetRelations(otherPawn))
                    {
                        if (!relations.Any(r => r.def == rel2 && r.otherPawn == otherPawn))
                        {
                            if (!rel2.implied)
                            {
                                relations.Add(new DirectPawnRelation(rel2, otherPawn, 0));
                                if (debug) Log.Message("CopyFromPawn: added otherRelation: " + rel2 + " - " + otherPawn.GetFullName());

                            }
                        }
                    }
                    if (debug) Log.Message("CopyFromPawn: added otherPawn: " + otherPawn.GetFullName());
                    relatedPawns.Add(otherPawn);
                }
                if (debug) Log.Message("CopyFromPawn END");

            }

            skills = new List<SkillRecord>();
            if (pawn.skills?.skills != null)
            {
                foreach (var skill in pawn.skills.skills)
                {
                    var gene = pawn.genes.GenesListForReading.FirstOrDefault(x => x.def.passionMod != null && x.def.passionMod.skill == skill.def);
                    var originPassion = gene?.passionPreAdd ?? skill.passion;
                    skills.Add(new SkillRecord
                    {
                        def = skill.def,
                        levelInt = skill.levelInt,
                        xpSinceLastLevel = skill.xpSinceLastLevel,
                        xpSinceMidnight = skill.xpSinceMidnight,
                        passion = originPassion,
                    });
                }
            }
            childhood = pawn.story?.Childhood;
            adulthood = pawn.story?.Adulthood;
            title = pawn.story?.title;

            priorities = new Dictionary<WorkTypeDef, int>();
            if (pawn.workSettings != null && pawn.workSettings.priorities != null)
            {
                foreach (WorkTypeDef w in DefDatabase<WorkTypeDef>.AllDefs)
                {
                    priorities[w] = pawn.workSettings.GetPriority(w);
                }
            }
            if (this.sourceStack == AC_DefOf.AC_ActiveArchotechStack && canBackupPsychicStuff)
            {
                if (pawn.HasPsylink)
                {
                    psylinkLevel = pawn.GetPsylinkLevel();
                }
                if (pawn.psychicEntropy != null)
                {
                    this.currentEntropy = pawn.psychicEntropy.currentEntropy;
                    this.currentPsyfocus = pawn.psychicEntropy.currentPsyfocus;
                    this.limitEntropyAmount = pawn.psychicEntropy.limitEntropyAmount;
                    this.targetPsyfocus = pawn.psychicEntropy.targetPsyfocus;
                }
                VPE_PsycastAbilityImplant = pawn.health.hediffSet.hediffs.FirstOrDefault(x => x.def.defName == "VPE_PsycastAbilityImplant");

                if (pawn.abilities?.abilities != null)
                {
                    abilities = new List<AbilityDef>();
                    foreach (var ability in pawn.abilities.abilities.Select(x => x.def).ToList())
                    {
                        if (CanStoreAbility(pawn, ability))
                        {
                            abilities.Add(ability);
                        }
                    }
                }

                var comp = pawn.GetComp<VFECore.Abilities.CompAbilities>();
                if (comp != null && comp.LearnedAbilities != null)
                {
                    VEAbilities = comp.LearnedAbilities.Select(x => x.def).Where(x => CanStoreAbility(pawn, x)).ToList();
                }
            }

            if (pawn.guest != null)
            {
                guestStatusInt = pawn.guest.GuestStatus;
                interactionMode = pawn.guest.interactionMode;
                slaveInteractionMode = pawn.guest.slaveInteractionMode;
                hostFactionInt = pawn.guest.HostFaction;
                joinStatus = pawn.guest.joinStatus;
                slaveFactionInt = pawn.guest.SlaveFaction;
                lastRecruiterName = pawn.guest.lastRecruiterName;
                lastRecruiterOpinion = pawn.guest.lastRecruiterOpinion;
                hasOpinionOfLastRecruiter = pawn.guest.hasOpinionOfLastRecruiter;
                releasedInt = pawn.guest.Released;
                ticksWhenAllowedToEscapeAgain = pawn.guest.ticksWhenAllowedToEscapeAgain;
                spotToWaitInsteadOfEscaping = pawn.guest.spotToWaitInsteadOfEscaping;
                lastPrisonBreakTicks = pawn.guest.lastPrisonBreakTicks;
                everParticipatedInPrisonBreak = pawn.guest.everParticipatedInPrisonBreak;
                resistance = pawn.guest.resistance;
                will = pawn.guest.will;
                ideoForConversion = pawn.guest.ideoForConversion;
                everEnslaved = pawn.guest.EverEnslaved;
                getRescuedThoughtOnUndownedBecauseOfPlayer = pawn.guest.getRescuedThoughtOnUndownedBecauseOfPlayer;
                recruitable = pawn.guest.recruitable;
            }

            if (pawn.records != null)
            {
                records = pawn.records.records;
                battleActive = pawn.records.BattleActive;
                battleExitTick = pawn.records.LastBattleTick;
            }
            savedHediffs = new List<Hediff>();
            foreach (var hediff in pawn.health.hediffSet.hediffs)
            {
                if (ModsConfig.AnomalyActive && hediff.def == HediffDefOf.Inhumanized)
                {
                    savedHediffs.Add(MakeCopy(hediff, hostPawn));
                }
            }

            pawnID = pawn.thingIDNumber;

            if (copyRaceGenderInfo)
            {
                if (pawn.HasNeuralStack(out var hediff))
                {
                    var neuralData = hediff.NeuralData;
                    OriginalRace = neuralData.OriginalRace ?? pawn.def;
                    OriginalGender = neuralData.OriginalGender != Gender.None ? neuralData.OriginalGender : pawn.gender;
                    if (neuralData.OriginalXenotypeName.NullOrEmpty())
                    {
                        if (pawn.genes.xenotypeName.NullOrEmpty())
                        {
                            OriginalXenotypeDef = neuralData.OriginalXenotypeDef != null ? neuralData.OriginalXenotypeDef : pawn.genes.xenotype;
                        }
                        else
                        {
                            OriginalXenotypeName = pawn.genes.xenotypeName;
                        }
                    }
                    else
                    {
                        OriginalXenotypeName = neuralData.OriginalXenotypeName;
                    }
                }
                else
                {
                    OriginalRace = pawn.def;
                    OriginalGender = pawn.gender;
                    OriginalXenotypeDef = pawn.genes.xenotype;
                    OriginalXenotypeName = pawn.genes.xenotypeName;
                }
            }
            if (ModsConfig.RoyaltyActive && pawn.royalty != null)
            {
                royalTitles = pawn.royalty?.AllTitlesForReading;
                favor = pawn.royalty.favor;
                heirs = pawn.royalty.heirs;
                bondedThings = new List<Thing>();
                foreach (Map map in Find.Maps)
                {
                    foreach (Thing thing in map.listerThings.AllThings)
                    {
                        CompBladelinkWeapon comp = thing.TryGetComp<CompBladelinkWeapon>();
                        if (comp != null && comp.CodedPawn == pawn)
                        {
                            bondedThings.Add(thing);
                        }
                    }
                    foreach (Apparel gear in pawn.apparel?.WornApparel)
                    {
                        CompBladelinkWeapon comp = gear.TryGetComp<CompBladelinkWeapon>();
                        if (comp != null && comp.CodedPawn == pawn)
                        {
                            bondedThings.Add(gear);
                        }
                    }
                    foreach (ThingWithComps gear in pawn.equipment?.AllEquipmentListForReading)
                    {
                        CompBladelinkWeapon comp = gear.TryGetComp<CompBladelinkWeapon>();
                        if (comp != null && comp.CodedPawn == pawn)
                        {
                            bondedThings.Add(gear);
                        }
                    }
                    foreach (Thing gear in pawn.inventory?.innerContainer)
                    {
                        CompBladelinkWeapon comp = gear.TryGetComp<CompBladelinkWeapon>();
                        if (comp != null && comp.CodedPawn == pawn)
                        {
                            bondedThings.Add(gear);
                        }
                    }
                }
                factionPermits = pawn.royalty.factionPermits;
            }

            if (ModsConfig.IdeologyActive)
            {
                if (pawn.ideo != null && pawn.Ideo != null)
                {
                    ideo = pawn.Ideo;
                    certainty = pawn.ideo.Certainty;
                    previousIdeos = pawn.ideo.PreviousIdeos;
                    joinTick = pawn.ideo.joinTick;

                    Precept_Role role = pawn.Ideo.GetRole(pawn);
                    if (role is Precept_RoleMulti multi)
                    {
                        precept_RoleMulti = multi;
                        precept_RoleSingle = null;
                    }
                    else if (role is Precept_RoleSingle single)
                    {
                        precept_RoleMulti = null;
                        precept_RoleSingle = single;
                    }
                }

                if (pawn.story?.favoriteColor.HasValue ?? false)
                {
                    favoriteColor = pawn.story.favoriteColor.Value;
                }
            }

            if (ModCompatibility.IndividualityIsActive)
            {
                sexuality = ModCompatibility.GetSyrTraitsSexuality(pawn);
                romanceFactor = ModCompatibility.GetSyrTraitsRomanceFactor(pawn);
            }
            if (ModCompatibility.PsychologyIsActive)
            {
                psychologyData = ModCompatibility.GetPsychologyData(pawn);
            }
            if (ModCompatibility.RimJobWorldIsActive)
            {
                rjwData = ModCompatibility.GetRjwData(pawn);
            }
            if (ModCompatibility.VanillaSkillsExpandedIsActive)
            {
                expertiseRecords = ModCompatibility.GetExpertises(pawn);
            }

            if (ModCompatibility.VanillaAspirationsExpandedIsActive)
            {
                aspirations = ModCompatibility.GetAspirations(pawn);
                aspirationsCompletedTicks = ModCompatibility.GetCompletedAspirations(pawn);
            }

            var stackDegradationHediff = pawn.health.hediffSet.GetFirstHediffOfDef(AC_DefOf.AC_StackDegradation) as Hediff_StackDegradation;
            if (stackDegradationHediff != null)
            {
                this.stackDegradation = stackDegradationHediff.stackDegradation;
            }
            AssignDummyPawnReferences();
        }

        private Hediff MakeCopy(Hediff hediff, Pawn pawn)
        {
            var newHediff = HediffMaker.MakeHediff(hediff.def, pawn, hediff.Part);
            newHediff.CopyFrom(hediff);
            return newHediff;
        }

        public void CopyDataFrom(NeuralData other, bool isDuplicateOperation = false)
        {
            sourceStack = other.sourceStack;
            name = GetNameCopy(other.name);
            kindDef = other.kindDef;
            hostPawn = other.hostPawn;
            hostilityMode = other.hostilityMode;
            allowedAreas = other.allowedAreas;
            ageChronologicalTicks = other.ageChronologicalTicks;
            ageBiologicalTicks = other.ageBiologicalTicks;
            medicalCareCategory = other.medicalCareCategory;
            selfTend = other.selfTend;
            foodPolicy = other.foodPolicy;
            apparelPolicy = other.apparelPolicy;
            drugPolicy = other.drugPolicy;
            times = other.times.CopyList();
            thoughts = other.thoughts.CopyList();
            faction = other.faction;
            isFactionLeader = other.isFactionLeader;
            traits = new List<Trait>();
            if (other.traits != null)
            {
                foreach (var trait in other.traits)
                {
                    traits.Add(new Trait(trait.def, trait.degree));
                }
            }
            relations = other.relations.CopyList();
            everSeenByPlayer = other.everSeenByPlayer;
            canGetRescuedThought = other.canGetRescuedThought;
            relativeInvolvedInRescueQuest = other.relativeInvolvedInRescueQuest;
            nextMarriageNameChange = other.nextMarriageNameChange;
            hidePawnRelations = other.hidePawnRelations;
            relatedPawns = other.relatedPawns.CopyList();
            skills = new List<SkillRecord>();
            if (other.skills != null)
            {
                foreach (var skill in other.skills)
                {
                    skills.Add(new SkillRecord
                    {
                        def = skill.def,
                        levelInt = skill.levelInt,
                        xpSinceLastLevel = skill.xpSinceLastLevel,
                        xpSinceMidnight = skill.xpSinceMidnight,
                        passion = skill.passion,
                    });
                }
            }
            childhood = other.childhood;
            adulthood = other.adulthood;
            title = other.title;
            priorities = other.priorities.CopyDict();
            psylinkLevel = other.psylinkLevel;
            abilities = other.abilities.CopyList();
            VEAbilities = other.VEAbilities.CopyList();
            VPE_PsycastAbilityImplant = other.VPE_PsycastAbilityImplant;

            currentEntropy = other.currentEntropy;
            currentPsyfocus = other.currentPsyfocus;
            limitEntropyAmount = other.limitEntropyAmount;
            targetPsyfocus = other.targetPsyfocus;

            guestStatusInt = other.guestStatusInt;
            interactionMode = other.interactionMode;
            slaveInteractionMode = other.slaveInteractionMode;
            hostFactionInt = other.hostFactionInt;
            joinStatus = other.joinStatus;
            slaveFactionInt = other.slaveFactionInt;
            lastRecruiterName = other.lastRecruiterName;
            lastRecruiterOpinion = other.lastRecruiterOpinion;
            hasOpinionOfLastRecruiter = other.hasOpinionOfLastRecruiter;
            lastRecruiterResistanceReduce = other.lastRecruiterResistanceReduce;
            releasedInt = other.releasedInt;
            ticksWhenAllowedToEscapeAgain = other.ticksWhenAllowedToEscapeAgain;
            spotToWaitInsteadOfEscaping = other.spotToWaitInsteadOfEscaping;
            lastPrisonBreakTicks = other.lastPrisonBreakTicks;
            everParticipatedInPrisonBreak = other.everParticipatedInPrisonBreak;
            resistance = other.resistance;
            will = other.will;
            ideoForConversion = other.ideoForConversion;
            everEnslaved = other.everEnslaved;
            getRescuedThoughtOnUndownedBecauseOfPlayer = other.getRescuedThoughtOnUndownedBecauseOfPlayer;
            recruitable = other.recruitable;
            records = other.records;
            savedHediffs = new List<Hediff>();
            foreach (var otherHediff in other.savedHediffs)
            {
                savedHediffs.Add(MakeCopy(otherHediff, hostPawn));
            }
            battleActive = other.battleActive;
            battleExitTick = other.battleExitTick;

            CopyOriginalData(other);

            pawnID = other.pawnID;

            if (ModsConfig.RoyaltyActive)
            {
                royalTitles = other.royalTitles.CopyList();
                favor = other.favor.CopyDict();
                heirs = other.heirs.CopyDict();
                bondedThings = other.bondedThings.CopyList();
                factionPermits = other.factionPermits;
            }
            if (ModsConfig.IdeologyActive)
            {
                ideo = other.ideo;
                previousIdeos = other.previousIdeos;
                joinTick = other.joinTick;
                certainty = other.certainty;

                precept_RoleSingle = other.precept_RoleSingle;

                precept_RoleMulti = other.precept_RoleMulti;

                if (other.favoriteColor.HasValue)
                {
                    favoriteColor = other.favoriteColor.Value;
                }
            }

            isCopied = isDuplicateOperation || other.isCopied;
            diedFromCombat = other.diedFromCombat;
            stackGroupID = other.stackGroupID;

            sexuality = other.sexuality;
            romanceFactor = other.romanceFactor;
            psychologyData = other.psychologyData;
            rjwData = other.rjwData;
            expertiseRecords = other.expertiseRecords;

            stackDegradation = other.stackDegradation;
            AssignDummyPawnReferences();
        }

        public void CopyOriginalData(NeuralData other)
        {
            OriginalGender = other.OriginalGender;
            OriginalRace = other.OriginalRace;
            OriginalXenotypeDef = other.OriginalXenotypeDef;
            OriginalXenotypeName = other.OriginalXenotypeName;
        }

        private Name GetNameCopy(Name other)
        {
            if (other is NameTriple nameTriple)
            {
                return new NameTriple(nameTriple.firstInt, nameTriple.nickInt, nameTriple.lastInt);
            }
            else if (other is NameSingle nameSingle)
            {
                return new NameSingle(nameSingle.nameInt);
            }
            if (other != null)
            {
                return other.Clone();
            }
            return null;
        }

        public void OverwritePawn(Pawn pawn, StackSavingOptionsModExtension extension, bool overwriteOriginalPawn = true, 
            bool copyFromOrigPawn = true)
        {
            if (copyFromOrigPawn && hostPawn != null)
            {
                CopyFromPawn(hostPawn, sourceStack);
            }
            pawn.Name = GetNameCopy(name);
            if (kindDef != null)
            {
                pawn.kindDef = kindDef;
            }
            PawnComponentsUtility.CreateInitialComponents(pawn);
            if (pawn.Faction != faction)
            {
                pawn.SetFaction(faction);
            }
            if (isFactionLeader && overwriteOriginalPawn && pawn.Faction != null)
            {
                pawn.Faction.leader = pawn;
            }

            OverwriteThoughts(pawn);
            OverwriteTraits(pawn, extension);
            ResetRelationships(pawn);

            if (overwriteOriginalPawn)
            {
                OverwriteRelationships(pawn);
                this.hostPawn = pawn;
                this.pawnID = this.hostPawn.thingIDNumber;
            }

            if (ModsConfig.RoyaltyActive)
            {
                AssignRoyaltyData(pawn);
            }
            if (ModsConfig.IdeologyActive)
            {
                AssignIdeologyData(pawn);
            }

            AssignAbilities(pawn);
            AssignSkills(pawn);
            SetStoryData(pawn);
            SetGuestData(pawn);
            SetPawnSettings(pawn);

            if (savedHediffs != null)
            {
                foreach (var hediff in savedHediffs) 
                {
                    var newCopy = MakeCopy(hediff, pawn);
                    pawn.health.AddHediff(newCopy);
                }
            }

            if (ModCompatibility.IndividualityIsActive)
            {
                ModCompatibility.SetSyrTraitsSexuality(pawn, sexuality);
                ModCompatibility.SetSyrTraitsRomanceFactor(pawn, romanceFactor);
            }

            if (ModCompatibility.PsychologyIsActive && psychologyData != null)
            {
                ModCompatibility.SetPsychologyData(pawn, psychologyData);
            }
            if (ModCompatibility.RimJobWorldIsActive && rjwData != null)
            {
                ModCompatibility.SetRjwData(pawn, rjwData);
            }
            if (ModCompatibility.VanillaSkillsExpandedIsActive)
            {
                ModCompatibility.SetExpertises(pawn, expertiseRecords);
            }
            if (ModCompatibility.VanillaAspirationsExpandedIsActive)
            {
                ModCompatibility.SetAspirations(pawn, aspirations);
                ModCompatibility.SetCompletedAspirations(pawn, aspirationsCompletedTicks);
            }
        }

        private void ResetRelationships(Pawn pawn)
        {
            foreach (var rel in pawn.relations.directRelations)
            {
                rel.otherPawn.relations.directRelations.RemoveAll(x => x.otherPawn == pawn);
            }
            pawn.relations = new Pawn_RelationsTracker(pawn)
            {
                everSeenByPlayer = everSeenByPlayer,
                canGetRescuedThought = canGetRescuedThought,
                relativeInvolvedInRescueQuest = relativeInvolvedInRescueQuest,
                nextMarriageNameChange = nextMarriageNameChange,
                hidePawnRelations = hidePawnRelations
            };
        }

        private void SetStoryData(Pawn pawn)
        {
            pawn.story.backstoriesCache = null;
            pawn.story.childhood = childhood;
            pawn.story.adulthood = adulthood;
            pawn.story.title = title;
            if (pawn.records is null)
            {
                pawn.records = new Pawn_RecordsTracker(pawn);
            }
            if (records != null)
            {
                pawn.records.records = records;
                pawn.records.battleActive = battleActive;
                pawn.records.battleExitTick = battleExitTick;
            }
        }

        private void SetPawnSettings(Pawn pawn)
        {
            if (pawn.playerSettings is null)
            {
                pawn.playerSettings = new Pawn_PlayerSettings(pawn);
            }
            pawn.playerSettings.hostilityResponse = hostilityMode;
            pawn.playerSettings.allowedAreas = allowedAreas.CopyDict();
            pawn.playerSettings.medCare = medicalCareCategory;
            pawn.playerSettings.selfTend = selfTend;
            pawn.foodRestriction = new Pawn_FoodRestrictionTracker(pawn);
            pawn.foodRestriction.CurrentFoodPolicy = foodPolicy;
            pawn.outfits = new Pawn_OutfitTracker(pawn);
            try
            {
                pawn.outfits.curApparelPolicy = apparelPolicy;
            }
            catch { }
            pawn.drugs = new Pawn_DrugPolicyTracker(pawn);
            pawn.drugs.CurrentPolicy = drugPolicy;
            pawn.ageTracker.AgeChronologicalTicks = ageChronologicalTicks;
            pawn.timetable = new Pawn_TimetableTracker(pawn);

            if (times != null && times.Count == 24)
            {
                pawn.timetable.times = times.CopyList();
            }

            pawn.workSettings = new Pawn_WorkSettings(pawn);
            pawn.workSettings.priorities = new DefMap<WorkTypeDef, int>();
            pawn.Notify_DisabledWorkTypesChanged();
            if (priorities != null)
            {
                foreach (KeyValuePair<WorkTypeDef, int> priority in priorities)
                {
                    if (pawn.WorkTypeIsDisabled(priority.Key) is false)
                    {
                        pawn.workSettings.SetPriority(priority.Key, priority.Value);
                    }
                }
            }
        }

        private void SetGuestData(Pawn pawn)
        {
            if (pawn.guest is null)
            {
                pawn.guest = new Pawn_GuestTracker(pawn);
            }
            pawn.guest.guestStatusInt = guestStatusInt;
            pawn.guest.interactionMode = interactionMode;
            if (pawn.guest.interactionMode is null)
                pawn.guest.interactionMode = PrisonerInteractionModeDefOf.MaintainOnly;

            pawn.guest.slaveInteractionMode = slaveInteractionMode;
            pawn.guest.hostFactionInt = hostFactionInt;
            pawn.guest.joinStatus = joinStatus;
            pawn.guest.slaveFactionInt = slaveFactionInt;
            pawn.guest.lastRecruiterName = lastRecruiterName;
            pawn.guest.lastRecruiterOpinion = lastRecruiterOpinion;
            pawn.guest.hasOpinionOfLastRecruiter = hasOpinionOfLastRecruiter;
            pawn.guest.Released = releasedInt;
            pawn.guest.ticksWhenAllowedToEscapeAgain = ticksWhenAllowedToEscapeAgain;
            pawn.guest.spotToWaitInsteadOfEscaping = spotToWaitInsteadOfEscaping;
            pawn.guest.lastPrisonBreakTicks = lastPrisonBreakTicks;
            pawn.guest.everParticipatedInPrisonBreak = everParticipatedInPrisonBreak;
            pawn.guest.resistance = resistance;
            pawn.guest.will = will;
            pawn.guest.ideoForConversion = ideoForConversion;
            pawn.guest.everEnslaved = everEnslaved;
            pawn.guest.recruitable = recruitable;
            pawn.guest.getRescuedThoughtOnUndownedBecauseOfPlayer = getRescuedThoughtOnUndownedBecauseOfPlayer;
        }

        private void AssignSkills(Pawn pawn)
        {
            pawn.skills.skills.Clear();
            if (skills != null)
            {
                foreach (SkillRecord skill in skills)
                {
                    SkillRecord newSkill = new SkillRecord(pawn, skill.def)
                    {
                        passion = skill.passion,
                        levelInt = skill.levelInt,
                        xpSinceLastLevel = skill.xpSinceLastLevel,
                        xpSinceMidnight = skill.xpSinceMidnight
                    };
                    pawn.skills.skills.Add(newSkill);
                }
            }
        }

        private void AssignAbilities(Pawn pawn)
        {
            var oldAbilities = pawn.abilities?.abilities.Select(x => x.def).ToList();
            pawn.abilities = new Pawn_AbilityTracker(pawn);
            if (oldAbilities != null)
            {
                foreach (var ability in oldAbilities)
                {
                    if (IsNaturalAbility(pawn, ability))
                    {
                        pawn.abilities.GainAbility(ability);
                    }
                    else if (IsPsycastAbility(ability))
                    {
                        pawn.abilities.GainAbility(ability);
                    }
                }
            }
            var compAbilities = pawn.GetComp<VFECore.Abilities.CompAbilities>();
            if (compAbilities?.LearnedAbilities != null)
            {
                compAbilities.LearnedAbilities.RemoveAll(x => IsNaturalAbility(pawn, x.def) is false && IsPsycastAbility(x.def) is false);
            }
            pawn.psychicEntropy = new Pawn_PsychicEntropyTracker(pawn);
            if (this.sourceStack == AC_DefOf.AC_ActiveArchotechStack)
            {
                var hediff_Psylink = pawn.GetMainPsylinkSource() as Hediff_Psylink;
                if (this.psylinkLevel.HasValue)
                {
                    if (hediff_Psylink == null)
                    {
                        Hediff_Psylink_TryGiveAbilityOfLevel_Patch.suppress = true;
                        hediff_Psylink = HediffMaker.MakeHediff(HediffDefOf.PsychicAmplifier, pawn,
                            pawn.health.hediffSet.GetBrain()) as Hediff_Psylink;
                        pawn.health.AddHediff(hediff_Psylink);
                        Hediff_Psylink_TryGiveAbilityOfLevel_Patch.suppress = false;
                    }
                    var levelOffset = this.psylinkLevel.Value - hediff_Psylink.level;
                    hediff_Psylink.level = (int)Mathf.Clamp(hediff_Psylink.level + levelOffset, hediff_Psylink.def.minSeverity, hediff_Psylink.def.maxSeverity);
                }

                pawn.psychicEntropy.currentEntropy = currentEntropy;
                pawn.psychicEntropy.currentPsyfocus = currentPsyfocus;
                pawn.psychicEntropy.limitEntropyAmount = limitEntropyAmount;
                pawn.psychicEntropy.targetPsyfocus = targetPsyfocus;

                if (abilities.NullOrEmpty() is false)
                {
                    foreach (var def in abilities)
                    {
                        pawn.abilities.GainAbility(def);
                    }
                }

                if (VPE_PsycastAbilityImplant?.def != null)
                {
                    pawn.health.hediffSet.hediffs.RemoveAll(x => x.def.defName == "VPE_PsycastAbilityImplant");
                    pawn.health.AddHediff(VPE_PsycastAbilityImplant);
                    Traverse.Create(VPE_PsycastAbilityImplant).Field("psylink").SetValue(hediff_Psylink);
                }
                if (VEAbilities.NullOrEmpty() is false)
                {
                    if (compAbilities != null)
                    {
                        foreach (var ability in VEAbilities)
                        {
                            compAbilities.GiveAbility(ability);
                        }
                    }
                }
            }
        }

        private void OverwriteTraits(Pawn pawn, StackSavingOptionsModExtension extension)
        {
            var traitsToRemove = pawn.story.traits.allTraits.Where(x => x.sourceGene is null).ToList();
            if (extension != null)
            {
                traitsToRemove.RemoveAll(x => extension.ignoresTraits.Contains(x.def.defName));
            }
            foreach (var trait in traitsToRemove)
            {
                pawn.story.traits.RemoveTrait(trait);
            }
            if (traits != null)
            {
                foreach (Trait trait in traits)
                {
                    if (extension != null && extension.ignoresTraits != null && extension.ignoresTraits.Contains(trait.def.defName))
                    {
                        continue;
                    }
                    pawn.story.traits.GainTrait(new Trait(trait.def, trait.degree));
                }
            }
        }

        private void OverwriteThoughts(Pawn pawn)
        {
            if (pawn.CanThink())
            {
                for (int num = pawn.needs.mood.thoughts.memories.Memories.Count - 1; num >= 0; num--)
                {
                    pawn.needs.mood.thoughts.memories.RemoveMemory(pawn.needs.mood.thoughts.memories.Memories[num]);
                }

                if (thoughts != null)
                {
                    if (OriginalGender == pawn.gender)
                    {
                        thoughts.RemoveAll(x => x.def == AC_DefOf.AC_WrongGender);
                        thoughts.RemoveAll(x => x.def == AC_DefOf.AC_WrongGenderPregnant);
                    }
                    if (ModCompatibility.AlienRacesIsActive && OriginalRace == pawn.kindDef.race)
                    {
                        thoughts.RemoveAll(x => x.def == AC_DefOf.AC_WrongRace);
                    }
                    if (OriginalXenotypeDef != null && OriginalXenotypeDef == pawn.genes.xenotype
                        || OriginalXenotypeName.NullOrEmpty() is false && OriginalXenotypeName == pawn.genes.xenotypeName)
                    {
                        thoughts.RemoveAll(x => x.def == AC_DefOf.AC_WrongXenotype);
                    }

                    foreach (Thought_Memory thought in thoughts)
                    {
                        if (thought is Thought_MemorySocial && thought.otherPawn == null)
                        {
                            continue;
                        }
                        pawn.needs.mood.thoughts.memories.TryGainMemory(thought, thought.otherPawn);
                    }
                }
                pawn.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
            }
            pawn.mindState = new Pawn_MindState(pawn);
        }

        private void OverwriteRelationships(Pawn pawn)
        {
            var allPotentialRelatedPawns = new HashSet<Pawn>();
            allPotentialRelatedPawns.AddRange(PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead);
            if (relations != null)
            {
                for (var i = relations.Count - 1; i >= 0; i--)
                {
                    var rel = relations[i];
                    if (rel != null && rel.otherPawn != null)
                    {
                        allPotentialRelatedPawns.Add(rel.otherPawn);
                    }
                }
            }

            if (relatedPawns != null)
            {
                for (var i = relatedPawns.Count - 1; i >= 0; i--)
                {
                    var relatedPawn = relatedPawns[i];
                    if (relatedPawn != null)
                    {
                        allPotentialRelatedPawns.Add(relatedPawn);
                    }
                }
            }
            allPotentialRelatedPawns = allPotentialRelatedPawns.Where(x => x.RaceProps.Humanlike).ToHashSet();
            var oldOrigPawn = this.hostPawn;
            if (oldOrigPawn is null)
            {
                oldOrigPawn = allPotentialRelatedPawns.FirstOrDefault(x => x.Dead && x.thingIDNumber == pawnID);
                if (debug) Log.Message("Found orig pawn: " + oldOrigPawn + " + " + string.Join(", ", oldOrigPawn?.relations?.DirectRelations?.Select(x => x?.def + " - " + x?.otherPawn?.GetFullName()) ?? new List<string>()));
                if (oldOrigPawn is null)
                {
                    oldOrigPawn = allPotentialRelatedPawns.FirstOrDefault(x => x.Dead && IsPresetPawn(x) && x != pawn);
                    if (debug) Log.Message("2 Found orig pawn: " + oldOrigPawn + " + " + string.Join(", ", oldOrigPawn?.relations?.DirectRelations.Select(x => x?.def + " - " + x?.otherPawn?.GetFullName()) ?? new List<string>()));
                }
            }

            if (oldOrigPawn?.relations != null)
            {
                var potentiallyRelatedPawns = oldOrigPawn.relations.PotentiallyRelatedPawns.ToList();
                for (var i = potentiallyRelatedPawns.Count - 1; i >= 0; i--)
                {
                    var relatedPawn = potentiallyRelatedPawns[i];
                    if (relatedPawn != null)
                    {
                        allPotentialRelatedPawns.Add(relatedPawn);
                    }
                }
            }

            if (oldOrigPawn != null)
            {
                allPotentialRelatedPawns.Remove(oldOrigPawn);
            }
            foreach (var potentiallyRelatedPawn in allPotentialRelatedPawns)
            {
                ReplaceSocialReferences(potentiallyRelatedPawn, pawn, oldOrigPawn);
            }

            if (oldOrigPawn != null)
            {
                ReplaceTales(pawn, oldOrigPawn);
            }

            if (oldOrigPawn?.relations.DirectRelations != null)
            {
                if (debug) Log.Message("OverwritePawn: oldOrigPawn.relations.DirectRelations: " + string.Join(", ", oldOrigPawn.relations.DirectRelations.Select(x => x.def + " - " + x.otherPawn.GetFullName())));
                for (var i = oldOrigPawn.relations.DirectRelations.Count - 1; i >= 0; i--)
                {
                    var oldDirectRelation = oldOrigPawn.relations.DirectRelations[i];
                    oldOrigPawn.relations.directRelations.Remove(oldDirectRelation);
                    if (pawn.relations.directRelations.Any(x => x.def == oldDirectRelation.def && x.otherPawn == oldDirectRelation.otherPawn) is false)
                    {
                        var rel = new DirectPawnRelation(oldDirectRelation.def, oldDirectRelation.otherPawn,
                            oldDirectRelation.startTicks);
                        pawn.relations.directRelations.Add(rel);
                        if (debug) Log.Message("OverwritePawn: Adding rel: " + rel + " - " + oldDirectRelation.otherPawn.GetFullName());
                    }
                    oldDirectRelation.otherPawn.relations.pawnsWithDirectRelationsWithMe.Remove(oldOrigPawn);
                    oldDirectRelation.otherPawn.relations.pawnsWithDirectRelationsWithMe.Add(pawn);
                    if (debug) Log.Message("OverwritePawn: Adding pawnsWithDirectRelationsWithMe: " + pawn.GetFullName());
                }
                oldOrigPawn.relations = new Pawn_RelationsTracker(oldOrigPawn);
            }
        }

        private void ReplaceTales(Pawn pawn, Pawn oldOrigPawn)
        {
            foreach (var tale in Find.TaleManager.AllTalesListForReading)
            {
                if (tale is Tale_SinglePawn tale1)
                {
                    if (tale1.pawnData?.pawn == oldOrigPawn)
                        tale1.pawnData.pawn = pawn;
                }
                else if (tale is Tale_DoublePawn tale2)
                {
                    if (tale2.firstPawnData?.pawn == oldOrigPawn)
                        tale2.firstPawnData.pawn = pawn;
                    if (tale2.secondPawnData?.pawn == oldOrigPawn)
                        tale2.secondPawnData.pawn = pawn;
                }
            }
        }

        private void AssignIdeologyData(Pawn pawn)
        {
            pawn.ideo = new Pawn_IdeoTracker(pawn);
            if (ideo != null)
            {
                pawn.ideo.ideo = ideo;
                pawn.ideo.certaintyInt = certainty;
                pawn.ideo.previousIdeos = previousIdeos.CopyList();
                pawn.ideo.joinTick = joinTick;
                if (IsDummyPawn(pawn) is false)
                {
                    if (precept_RoleMulti != null)
                    {
                        precept_RoleMulti.chosenPawns ??= new List<IdeoRoleInstance>();
                        precept_RoleMulti.chosenPawns.Add(new IdeoRoleInstance(precept_RoleMulti)
                        {
                            pawn = pawn
                        });
                        precept_RoleMulti.FillOrUpdateAbilities();
                    }
                    if (precept_RoleSingle != null)
                    {
                        precept_RoleSingle.chosenPawn = new IdeoRoleInstance(precept_RoleMulti)
                        {
                            pawn = pawn
                        };
                        precept_RoleSingle.FillOrUpdateAbilities();
                    }
                }
            }

            if (favoriteColor.HasValue)
            {
                pawn.story.favoriteColor = favoriteColor.Value;
            }
        }

        private void AssignRoyaltyData(Pawn pawn)
        {
            pawn.royalty = new Pawn_RoyaltyTracker(pawn);
            if (royalTitles != null)
            {
                foreach (RoyalTitle title in royalTitles.ToList())
                {
                    int index = pawn.royalty.FindFactionTitleIndex(title.faction, createIfNotExisting: true);
                    if (title != null)
                    {
                        pawn.royalty.titles[index].def = title.def;
                        pawn.royalty.titles[index].receivedTick = GenTicks.TicksGame;
                    }
                    else
                    {
                        pawn.royalty.titles.RemoveAt(index);
                    }
                }
            }
            if (heirs != null)
            {
                foreach (KeyValuePair<Faction, Pawn> heir in heirs.ToList())
                {
                    heirs[heir.Key] = heir.Value;
                }
            }

            if (favor != null)
            {
                foreach (KeyValuePair<Faction, int> fav in favor.ToList())
                {
                    if (fav.Value == 0 && favor.ContainsKey(fav.Key) && pawn.royalty.FindFactionTitleIndex(fav.Key) == -1)
                    {
                        favor.Remove(fav.Key);
                    }
                    else
                    {
                        favor.SetOrAdd(fav.Key, fav.Value);
                    }
                }
            }

            if (bondedThings != null && IsDummyPawn(pawn) is false)
            {
                foreach (Thing bonded in bondedThings)
                {
                    CompBladelinkWeapon comp = bonded.TryGetComp<CompBladelinkWeapon>();
                    if (comp != null)
                    {
                        comp.CodeFor(pawn);
                    }
                }
            }
            if (factionPermits != null)
            {
                pawn.royalty.factionPermits = factionPermits.CopyList();
            }
        }

        private static Type VEPsycastModExtensionType = AccessTools.TypeByName("VanillaPsycastsExpanded.AbilityExtension_Psycast");
        public static bool CanStoreAbility(Pawn pawn, Def def)
        {
            if (IsNaturalAbility(pawn, def))
            {
                Log.Message("Cannot store: " + def + " is natural ability");
                return false;
            }
            if (IsPsycastAbility(def))
            {
                return true;
            }
            Log.Message("Cannot store: " + def + " is other ability");
            return false;
        }

        public static bool IsPsycastAbility(Def def)
        {
            if (def is AbilityDef abilityDef)
            {
                return typeof(Psycast).IsAssignableFrom(abilityDef.abilityClass);
            }
            else if (def is VFECore.Abilities.AbilityDef abilityDef2)
            {
                if (VEPsycastModExtensionType != null)
                {
                    if (def.modExtensions != null)
                    {
                        foreach (var modExtension in def.modExtensions)
                        {
                            if (VEPsycastModExtensionType.IsAssignableFrom(modExtension.GetType()))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public static bool IsNaturalAbility(Pawn pawn, Def def)
        {
            if (def is AbilityDef ability)
            {
                if (ModsConfig.BiotechActive && pawn.genes != null)
                {
                    foreach (var gene in pawn.genes.GenesListForReading)
                    {
                        if (gene.Active && gene.def.abilities.NullOrEmpty() is false && gene.def.abilities.Contains(ability))
                        {
                            return true;
                        }
                    }
                }
            }
            else if (def is VFECore.Abilities.AbilityDef ability2)
            {
                if (ModCompatibility.VanillaFactionsExpandedAncientsIsActive)
                {
                    if (ModCompatibility.HasPowerAbility(pawn, ability2))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private void ReplaceSocialReferences(Pawn relatedPawn, Pawn newReference, Pawn oldOriginPawn)
        {
            bool replacedReferences = false;
           if (relatedPawn.CanThink())
            {
                foreach (Thought_Memory thought in relatedPawn.needs.mood.thoughts.memories.Memories)
                {
                    if (oldOriginPawn != null && thought.otherPawn == oldOriginPawn 
                        || oldOriginPawn is null && IsPresetPawn(thought.otherPawn))
                    {
                        var other = thought.otherPawn;
                        thought.otherPawn = newReference;
                        ReplacePawnRelations(relatedPawn, newReference, other);
                        replacedReferences = true;
                    }
                }
            }

            if (relatedPawn.relations != null)
            {
                var pawnsWithDirectRelations = relatedPawn.relations.pawnsWithDirectRelationsWithMe.ToList();
                for (var j = pawnsWithDirectRelations.Count - 1; j >= 0; j--)
                {
                    var otherPawn = pawnsWithDirectRelations[j];
                    if (oldOriginPawn != null && otherPawn == oldOriginPawn
                        || oldOriginPawn is null && IsPresetPawn(otherPawn))
                    {
                        ReplacePawnRelations(relatedPawn, newReference, otherPawn);
                        replacedReferences = true;
                    }
                }

                var otherPawnRelations = relatedPawn.relations.DirectRelations;
                for (var i = otherPawnRelations.Count - 1; i >= 0; i--)
                {
                    var rel = otherPawnRelations[i];
                    if (rel != null)
                    {
                        if (oldOriginPawn != null && rel.otherPawn == oldOriginPawn
                            || oldOriginPawn is null && IsPresetPawn(rel.otherPawn))
                        {
                            if (rel.otherPawn != newReference)
                            {
                                var other = rel.otherPawn;
                                rel.otherPawn = newReference;
                                ReplacePawnRelations(relatedPawn, newReference, other);
                                replacedReferences = true;
                            }
                        }
                    }
                }
            }

            if (replacedReferences && relatedPawn.needs?.mood?.thoughts != null)
            {
                if (debug) Log.Message("END ReplacePawnRelations: relatedPawn relations: " + relatedPawn.GetFullName() + " - " + string.Join(", ", relatedPawn.relations.DirectRelations.Select(x => x.def + " - " + x.otherPawn.GetFullName())));
                if (debug) Log.Message("END ReplacePawnRelations: newReference relations: " + newReference.GetFullName() + " - " + string.Join(", ", newReference.relations.DirectRelations.Select(x => x.def + " - " + x.otherPawn.GetFullName())));
                relatedPawn.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
            }
        }

        private static void ReplacePawnRelations(Pawn relatedPawn, Pawn newReference, Pawn otherPawn)
        {
            relatedPawn.relations.pawnsWithDirectRelationsWithMe.Remove(otherPawn);
            relatedPawn.relations.pawnsWithDirectRelationsWithMe.Add(newReference);
            foreach (var relation in relatedPawn.relations.DirectRelations)
            {
                if (relation.def.reflexive)
                {
                    if (relation.otherPawn == otherPawn)
                    {
                        relation.otherPawn = newReference;
                    }
                    if (relation.otherPawn == newReference)
                    {
                        if (newReference.relations.DirectRelationExists(relation.def, relatedPawn) is false)
                        {
                            var rel = new DirectPawnRelation(relation.def, relatedPawn, relation.startTicks);
                            newReference.relations.directRelations.Add(rel);
                        }
                    }
                }
            }

            if (debug) Log.Message("2 ReplacePawnRelations: relatedPawn: " + relatedPawn.GetFullName() + " - " + string.Join(", ", relatedPawn.relations.DirectRelations.Select(x => x.def + " - " + x.otherPawn.GetFullName())));
            if (debug) Log.Message("2 ReplacePawnRelations: newReference: " + newReference.GetFullName() + " - " + string.Join(", ", newReference.relations.DirectRelations.Select(x => x.def + " - " + x.otherPawn.GetFullName())));
            if (debug) Log.Message("2 ReplacePawnRelations: otherPawn: " + otherPawn.GetFullName() + " - " + string.Join(", ", otherPawn.relations.DirectRelations.Select(x => x.def + " - " + x.otherPawn.GetFullName())));
        }

        public void ChangeIdeo(Ideo newIdeo, float certainty)
        {
            if (ideo != null)
            {
                previousIdeos ??= new List<Ideo>();
                if (!previousIdeos.Contains(ideo))
                {
                    previousIdeos.Add(ideo);
                }
            }
            ideo = newIdeo;
            joinTick = Find.TickManager.TicksGame;
            this.certainty = certainty;
            precept_RoleSingle = null;
            precept_RoleMulti = null;
        }

        public bool IsPresetPawn(Pawn pawn)
        {
            if (pawn == null || pawn.Name == null) return false;
            return pawn != null && (hostPawn == pawn || pawn.thingIDNumber == pawnID || name != null && pawn.Name != null && name.ToStringFull == pawn.Name.ToStringFull);
        }
        public bool IsPresetPawn(NeuralData otherNeuralData)
        {
            if (PawnID != 0)
            {
                return PawnID == otherNeuralData.PawnID;
            }
            if (hostPawn != null)
            {
                return hostPawn == otherNeuralData.hostPawn;
            }
            else if (name != null)
            {
                return name == otherNeuralData.name;
            }
            return false;
        }

        public bool AnyNeuralStackExist()
        {
            foreach (var map in Find.Maps)
            {
                if (map.listerThings.ThingsOfDef(AC_DefOf.AC_ActiveNeuralStack).Cast<NeuralStack>()
                    .Any(x => x.NeuralData.IsPresetPawn(this) && x.Spawned && !x.Destroyed))
                {
                    return true;
                }
                if (map.GetAllStackCaches()
                    .Any(x => x.TryGetComp<CompNeuralCache>() is CompNeuralCache comp
                    && comp.innerContainer.OfType<NeuralStack>().Any(y => y.NeuralData.IsPresetPawn(this))))
                {
                    return true;
                }
            }
            return false;
        }

        public bool AnyPawnExist()
        {
            foreach (var pawn in PawnsFinder.AllMapsWorldAndTemporary_AliveOrDead)
            {
                if (pawn.Destroyed is false && IsPresetPawn(pawn) && pawn.HasNeuralStack(out _))
                {
                    return true;
                }
            }
            return false;
        }

        //public void TryQueueAutoRestoration()
        //{
        //    if (AnyPawnExist() || AnyNeuralStackExist())
        //    {
        //        return;
        //    }
        //    var prints = new List<NeuralPrint>();
        //    foreach (var map in Find.Maps)
        //    {
        //        prints.AddRange(map.listerThings.ThingsOfDef(AC_DefOf.AC_NeuralPrint).Cast<NeuralPrint>());
        //        foreach (var matrix in map.listerThings.ThingsOfDef(AC_DefOf.AC_NeuralMatrix).Cast<Building_NeuralMatrix>())
        //        {
        //            prints.AddRange(matrix.StoredNeuralPrints);
        //        }
        //    }
        //    var foundPrint = prints.Where(x => x.CanAutoRestorePawn(this))
        //        .OrderByDescending(x => x.NeuralData.lastTimeBackedUp).FirstOrDefault();
        //    if (foundPrint != null)
        //    {
        //        var map = foundPrint.MapHeld;
        //        var editors = map.listerThings.ThingsOfDef(AC_DefOf.AC_NeuralEditor)
        //            .OfType<Building_NeuralEditor>().ToList();
        //        foreach (var editor in editors)
        //        {
        //            if (foundPrint.Spawned is false && foundPrint.ParentHolder is Building_NeuralMatrix 
        //                && foundPrint.ParentHolder != editor.ConnectedMatrix)
        //            {
        //                continue;
        //            }
        //            if (editor.autoRestoreIsEnabled is false)
        //            {
        //                continue;
        //            }
        //            if (editor.CanAddOperationOn(foundPrint))
        //            {
        //                editor.billStack.AddBill(new Bill_OperateOnStack(foundPrint, AC_DefOf.AC_RestoreStackFromNeuralPrint, null));
        //                var pawnArg = DummyPawn.Named("PAWN");
        //                Find.LetterStack.ReceiveLetter("AC.RestoringQueued".Translate(pawnArg),
        //                    "AC.RestoringQueuedDesc".Translate(pawnArg), LetterDefOf.NeutralEvent, editor);
        //            }
        //            break;
        //        }
        //    }
        //}

        public void ExposeData()
        {
            Scribe_Values.Look(ref pawnID, "pawnID");
            Scribe_Values.Look(ref stackGroupID, "stackGroupID", 0);
            Scribe_Values.Look(ref isCopied, "isCopied", false, false);
            Scribe_Values.Look(ref diedFromCombat, "diedFromCombat");
            Scribe_Deep.Look(ref name, "name", new object[0]);
            Scribe_References.Look(ref hostPawn, "hostPawn", true);
            Scribe_Values.Look(ref hostilityMode, "hostilityMode");
            Scribe_Collections.Look(ref allowedAreas, "allowedAreas", LookMode.Reference, LookMode.Reference, ref allowedAreasKeys, ref allowedAreasValues);
            if (Scribe.mode == LoadSaveMode.Saving && allowedAreas != null)
            {
                allowedAreas.RemoveAll((KeyValuePair<Map, Area> kvp) => kvp.Key == null || kvp.Value == null);
            }
            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs && allowedAreas == null)
            {
                allowedAreas = new Dictionary<Map, Area>();
            }
            if (Scribe.mode == LoadSaveMode.LoadingVars || Scribe.mode == LoadSaveMode.ResolvingCrossRefs || Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                Area refee = null;
                Scribe_References.Look(ref refee, "areaAllowed");
                if (refee != null && Find.AnyPlayerHomeMap != null)
                {
                    allowedAreas.Add(Find.AnyPlayerHomeMap, refee);
                }
            }
            Scribe_Values.Look(ref medicalCareCategory, "medicalCareCategory", MedicalCareCategory.NoCare, false);
            Scribe_Values.Look(ref selfTend, "selfTend", false, false);
            Scribe_Values.Look(ref ageBiologicalTicks, "ageBiologicalTicks", 0, false);
            Scribe_Values.Look(ref ageChronologicalTicks, "ageChronologicalTicks", 0, false);
            Scribe_Defs.Look(ref originalRace, "race");
            Scribe_Defs.Look(ref kindDef, "kindDef");
            Scribe_References.Look(ref apparelPolicy, "outfit", false);
            Scribe_References.Look(ref foodPolicy, "foodPolicy", false);
            Scribe_References.Look(ref drugPolicy, "drugPolicy", false);

            Scribe_Collections.Look(ref times, "times", LookMode.Def);
            Scribe_Collections.Look(ref thoughts, "thoughts", LookMode.Deep);
            Scribe_References.Look(ref faction, "faction", true);
            Scribe_Values.Look(ref isFactionLeader, "isFactionLeader", false, false);

            Scribe_Collections.Look(ref skills, "skills",LookMode.Deep);
            Scribe_Defs.Look(ref childhood, "childhood");
            Scribe_Defs.Look(ref adulthood, "adulthood");
            Scribe_Values.Look(ref title, "title", null, false);
            Scribe_Defs.Look(ref originalXenotypeDef, "originalXenotypeDef");
            Scribe_Values.Look(ref originalXenotypeName, "originalXenotypeName");

            Scribe_Collections.Look(ref traits, "traits", LookMode.Deep);
            Scribe_Collections.Look(ref skills, "skills", LookMode.Deep);
            Scribe_Collections.Look(ref relations, "otherPawnRelations", LookMode.Deep);

            Scribe_Values.Look(ref everSeenByPlayer, "everSeenByPlayer");
            Scribe_Values.Look(ref canGetRescuedThought, "canGetRescuedThought", true);
            Scribe_References.Look(ref relativeInvolvedInRescueQuest, "relativeInvolvedInRescueQuest");
            Scribe_Values.Look(ref nextMarriageNameChange, "nextMarriageNameChange");
            Scribe_Values.Look(ref hidePawnRelations, "hidePawnRelations");
            Scribe_Collections.Look(ref relatedPawns, saveDestroyedThings: true, "relatedPawns", LookMode.Reference);
            Scribe_Collections.Look(ref priorities, "priorities", LookMode.Def, LookMode.Value);
            Scribe_Values.Look(ref guestStatusInt, "guestStatusInt");
            Scribe_Defs.Look(ref interactionMode, "interactionMode");
            Scribe_Defs.Look(ref slaveInteractionMode, "slaveInteractionMode");
            Scribe_References.Look(ref hostFactionInt, "hostFactionInt");
            Scribe_References.Look(ref slaveFactionInt, "slaveFactionInt");
            Scribe_Values.Look(ref joinStatus, "joinStatus");
            Scribe_Values.Look(ref lastRecruiterName, "lastRecruiterName");
            Scribe_Values.Look(ref lastRecruiterOpinion, "lastRecruiterOpinion");
            Scribe_Values.Look(ref hasOpinionOfLastRecruiter, "hasOpinionOfLastRecruiter");
            Scribe_Values.Look(ref lastRecruiterResistanceReduce, "lastRecruiterResistanceReduce");
            Scribe_Values.Look(ref releasedInt, "releasedInt");
            Scribe_Values.Look(ref ticksWhenAllowedToEscapeAgain, "ticksWhenAllowedToEscapeAgain");
            Scribe_Values.Look(ref spotToWaitInsteadOfEscaping, "spotToWaitInsteadOfEscaping");
            Scribe_Values.Look(ref lastPrisonBreakTicks, "lastPrisonBreakTicks");
            Scribe_Values.Look(ref everParticipatedInPrisonBreak, "everParticipatedInPrisonBreak");
            Scribe_Values.Look(ref resistance, "resistance");
            Scribe_Values.Look(ref will, "will");
            Scribe_References.Look(ref ideoForConversion, "ideoForConversion");
            Scribe_Values.Look(ref everEnslaved, "everEnslaved");
            Scribe_Values.Look(ref recruitable, "recruitable");
            Scribe_Values.Look(ref getRescuedThoughtOnUndownedBecauseOfPlayer, "getRescuedThoughtOnUndownedBecauseOfPlayer");

            Scribe_Deep.Look(ref records, "records");
            Scribe_References.Look(ref battleActive, "battleActive");
            Scribe_Values.Look(ref battleExitTick, "battleExitTick", 0);
            Scribe_Values.Look(ref originalGender, "gender");
            Scribe_Collections.Look(ref savedHediffs, "savedHediffs", LookMode.Deep);
            Scribe_Values.Look(ref lastTimeBackedUp, "lastTimeUpdated");
            if (ModsConfig.RoyaltyActive)
            {
                Scribe_Collections.Look(ref favor, "favor", LookMode.Reference, LookMode.Value, ref favorKeys, ref favorValues);
                Scribe_Collections.Look(ref heirs, "heirs", LookMode.Reference, LookMode.Reference, ref heirsKeys, ref heirsValues);
                Scribe_Collections.Look(ref bondedThings, "bondedThings", LookMode.Reference);
                Scribe_Collections.Look(ref royalTitles, "royalTitles", LookMode.Deep);
                Scribe_Collections.Look(ref factionPermits, "permits", LookMode.Deep);
            }
            if (ModsConfig.IdeologyActive)
            {
                Scribe_References.Look(ref ideo, "ideo", saveDestroyedThings: true);
                Scribe_Collections.Look(ref previousIdeos, saveDestroyedThings: true, "previousIdeos", LookMode.Reference);
                Scribe_Values.Look(ref favoriteColor, "favoriteColor");
                Scribe_Values.Look(ref joinTick, "joinTick");
                Scribe_Values.Look(ref certainty, "certainty");
                Scribe_References.Look(ref precept_RoleSingle, "precept_RoleSingle");
                Scribe_References.Look(ref precept_RoleMulti, "precept_RoleMulti");
            }

            if (ModCompatibility.IndividualityIsActive)
            {
                Scribe_Values.Look(ref sexuality, "sexuality", -1);
                Scribe_Values.Look(ref romanceFactor, "romanceFactor", -1f);
            }
            if (ModCompatibility.PsychologyIsActive)
            {
                Scribe_Deep.Look(ref psychologyData, "psychologyData");
            }
            if (ModCompatibility.RimJobWorldIsActive)
            {
                Scribe_Deep.Look(ref rjwData, "rjwData");
            }
            if (ModCompatibility.VanillaSkillsExpandedIsActive)
            {
                Scribe_Collections.Look(ref expertiseRecords, "expertiseRecords", LookMode.Deep, hostPawn);
            }

            if (ModCompatibility.VanillaAspirationsExpandedIsActive)
            {
                Scribe_Collections.Look(ref aspirations, "aspirations", LookMode.Value);
                Scribe_Collections.Look(ref aspirationsCompletedTicks, nameof(aspirationsCompletedTicks), LookMode.Value);
            }

            Scribe_Defs.Look(ref sourceStack, "sourceStack");
            Scribe_Values.Look(ref psylinkLevel, "psylinkLevel");
            Scribe_Collections.Look(ref abilities, "abilities", LookMode.Def);
            Scribe_Collections.Look(ref VEAbilities, "VEAbilities", LookMode.Def);
            Scribe_Deep.Look(ref VPE_PsycastAbilityImplant, "VPE_PsycastAbilityImplant");

            Scribe_Values.Look(ref currentEntropy, "currentEntropy");
            Scribe_Values.Look(ref currentPsyfocus, "currentPsyfocus");
            Scribe_Values.Look(ref limitEntropyAmount, "limitEntropyAmount");
            Scribe_Values.Look(ref targetPsyfocus, "targetPsyfocus");

            if (VPE_PsycastAbilityImplant != null)
            {
                if (this.hostPawn != null)
                {
                    VPE_PsycastAbilityImplant.pawn = this.hostPawn;
                }
                VPE_PsycastAbilityImplant.loadID = Find.UniqueIDsManager.GetNextHediffID();
            }

            Scribe_Values.Look(ref editTime, "editTime");
            Scribe_Values.Look(ref stackDegradation, "stackDegradation");
            Scribe_Values.Look(ref stackDegradationToAdd, "stackDegradationToAdd");
            Scribe_Values.Look(ref dummyGender, "dummyGender");
            Scribe_Deep.Look(ref neuralDataRewritten, "neuralDataRewritten");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                times.CleanupList();
                thoughts.CleanupList();
                traits.CleanupList();
                relations.CleanupList();
                relatedPawns.CleanupList();
                skills.CleanupList();
                royalTitles.CleanupList();
                bondedThings.CleanupList();
                factionPermits.CleanupList();
                abilities.CleanupList();
                VEAbilities.CleanupList();
                previousIdeos.CleanupList();
                priorities.CleanupDict();
                favor.CleanupDict();
                heirs.CleanupDict();
                savedHediffs.CleanupList();
                if (pawnID == 0 && hostPawn != null)
                {
                    pawnID = hostPawn.thingIDNumber;
                }
                if (hostPawn != null && pawnID != hostPawn.thingIDNumber)
                {
                    Log.Error("[Altered Carbon] Error loading " + pawnID + " for " + hostPawn);
                }
                if (dummyPawn != null)
                {
                    dummyPawns.Add(dummyPawn);
                }
            }
        }

        private List<Map> allowedAreasKeys;

        private List<Area> allowedAreasValues;

        public bool IsDummyPawn(Pawn pawn)
        {
            return pawn == dummyPawn || dummyPawns.Contains(pawn);
        }

        private void AssignDummyPawnReferences()
        {
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                if (dummyPawns.Contains(hostPawn) is false)
                {
                    var dummyPawn = DummyPawn;
                    if (skills != null)
                    {
                        foreach (var skill in skills)
                        {
                            skill.pawn = dummyPawn;
                        }
                    }
                    if (traits != null)
                    {
                        foreach (var trait in traits)
                        {
                            trait.pawn = dummyPawn;
                        }
                    }
                    dummyPawn.Notify_DisabledWorkTypesChanged();
                }
            });
        }

        private List<Faction> favorKeys = new List<Faction>();
        private List<int> favorValues = new List<int>();

        private List<Faction> heirsKeys = new List<Faction>();
        private List<Pawn> heirsValues = new List<Pawn>();

        public override string ToString()
        {
            return name + " - " + faction;
        }
    }
}

