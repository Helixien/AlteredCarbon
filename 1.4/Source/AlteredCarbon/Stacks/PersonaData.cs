using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AlteredCarbon
{
    [HotSwappable]
    public class PersonaData : IExposable
    {
        public ThingDef sourceStack;
        public Name name;
        public Pawn origPawn;
        private int hostilityMode;
        private Area areaRestriction;
        private MedicalCareCategory medicalCareCategory;
        private bool selfTend;
        public long ageChronologicalTicks;
        private List<TimeAssignmentDef> times;
        private FoodRestriction foodRestriction;
        private Outfit outfit;
        private DrugPolicy drugPolicy;
        public Faction faction;
        public bool isFactionLeader;
        private List<Thought_Memory> thoughts;
        public List<Trait> traits;
        private List<DirectPawnRelation> relations;

        private HashSet<Pawn> relatedPawns;
        public List<SkillRecord> skills;

        public string childhood;
        public string adulthood;
        public string title;
        public XenotypeDef xenotypeDef;
        public string xenotypeName;

        public bool everSeenByPlayer;
        public bool canGetRescuedThought = true;
        public Pawn relativeInvolvedInRescueQuest;
        public MarriageNameChange nextMarriageNameChange;
        public bool hidePawnRelations;

        private Dictionary<WorkTypeDef, int> priorities;
        private GuestStatus guestStatusInt;
        private PrisonerInteractionModeDef interactionMode;
        private SlaveInteractionModeDef slaveInteractionMode;
        private Faction hostFactionInt;
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

        public bool ContainsInnerPersona => origPawn != null || name != null;

        public Gender gender;
        public ThingDef race;
        public int pawnID;

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
        // misc
        public bool? diedFromCombat;
        public bool hackedWhileOnStack;
        public bool isCopied = false;
        public int stackGroupID = -1;
        public int lastTimeUpdated;

        public bool restoreToEmptyStack = true;

        private Pawn dummyPawn;
        public Pawn GetDummyPawn
        {
            get
            {
                if (dummyPawn is null)
                {
                    dummyPawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfPlayer);
                    RefreshDummyPawn();
                }
                return dummyPawn;
            }
        }
        public void RefreshDummyPawn()
        {
            if (dummyPawn is null)
            {
                dummyPawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfPlayer);
            }
            OverwritePawn(dummyPawn, null, origPawn, overwriteOriginalPawn: false);
            if (origPawn != null)
            {
                ACUtils.CopyBody(origPawn, dummyPawn);
                dummyPawn.RefreshGraphic();
            }
        }
        public TaggedString PawnNameColored => TitleShort?.CapitalizeFirst().NullOrEmpty() ?? false
                    ? (TaggedString)(name?.ToStringShort.Colorize(GetFactionRelationColor(faction)))
                    : (TaggedString)(name?.ToStringShort.Colorize(GetFactionRelationColor(faction)) + ", " + TitleShort?.CapitalizeFirst());
        public string TitleShort
        {
            get
            {
                if (title != null)
                {
                    return title;
                }
                return !adulthood.NullOrEmpty() && DefDatabase<BackstoryDef>.GetNamedSilentFail(adulthood) is BackstoryDef newAdulthood
                    ? newAdulthood.TitleShortFor(gender)
                    : !childhood.NullOrEmpty() && DefDatabase<BackstoryDef>.GetNamedSilentFail(childhood) is BackstoryDef newChildhood
                    ? newChildhood.TitleShortFor(gender)
                    : "";
            }
        }
        private Color GetFactionRelationColor(Faction faction)
        {
            if (faction == null)
            {
                return Color.white;
            }
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

        public void CopyFromPawn(Pawn pawn, ThingDef sourceStack, bool copyRaceGenderInfo = false)
        {
            if (this.origPawn is null)
            {
                this.origPawn = pawn;
            }
            this.sourceStack = sourceStack ?? AC_DefOf.VFEU_FilledCorticalStack;
            name = pawn.Name;
            if (pawn.playerSettings != null)
            {
                hostilityMode = (int)pawn.playerSettings.hostilityResponse;
                areaRestriction = pawn.playerSettings.AreaRestriction;
                medicalCareCategory = pawn.playerSettings.medCare;
                selfTend = pawn.playerSettings.selfTend;
            }
            if (pawn.ageTracker != null)
            {
                ageChronologicalTicks = pawn.ageTracker.AgeChronologicalTicks;
            }
            foodRestriction = pawn.foodRestriction?.CurrentFoodRestriction;
            outfit = pawn.outfits?.CurrentOutfit;
            drugPolicy = pawn.drugs?.CurrentPolicy;
            times = pawn.timetable?.times;
            thoughts = pawn.needs?.mood?.thoughts?.memories?.Memories;
            faction = pawn.Faction;
            if (pawn.Faction?.leader == pawn)
            {
                isFactionLeader = true;
            }
            traits = pawn.story?.traits?.allTraits.Where(x => x.sourceGene is null && x.suppressedByGene is null).ToList();
            if (pawn.relations != null)
            {
                everSeenByPlayer = pawn.relations.everSeenByPlayer;
                canGetRescuedThought = pawn.relations.canGetRescuedThought;
                relativeInvolvedInRescueQuest = pawn.relations.relativeInvolvedInRescueQuest;
                nextMarriageNameChange = pawn.relations.nextMarriageNameChange;
                hidePawnRelations = pawn.relations.hidePawnRelations;

                relations = pawn.relations.DirectRelations ?? new List<DirectPawnRelation>();
                relatedPawns = pawn.relations.RelatedPawns?.ToList() ?? new List<Pawn>();
                foreach (Pawn otherPawn in pawn.relations.RelatedPawns)
                {
                    foreach (PawnRelationDef rel2 in pawn.GetRelations(otherPawn))
                    {
                        if (!relations.Any(r => r.def == rel2 && r.otherPawn == otherPawn))
                        {
                            if (!rel2.implied)
                            {
                                relations.Add(new DirectPawnRelation(rel2, otherPawn, 0));
                            }
                        }
                    }
                    relatedPawns.Add(otherPawn);
                }
            }

            skills = pawn.skills?.skills;
            childhood = pawn.story?.Childhood?.defName;
            if (pawn.story?.Adulthood != null)
            {
                adulthood = pawn.story.Adulthood.defName;
            }
            title = pawn.story?.title;
            if (ModsConfig.BiotechActive && pawn.genes != null)
            {
                xenotypeDef = pawn.genes.Xenotype;
                xenotypeName = pawn.genes.xenotypeName;
            }
            priorities = new Dictionary<WorkTypeDef, int>();
            if (pawn.workSettings != null && pawn.workSettings.priorities != null)
            {
                foreach (WorkTypeDef w in DefDatabase<WorkTypeDef>.AllDefs)
                {
                    priorities[w] = pawn.workSettings.GetPriority(w);
                }
            }
            if (this.sourceStack == AC_DefOf.AC_FilledArchoStack)
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

            pawnID = pawn.thingIDNumber;

            if (copyRaceGenderInfo)
            {
                if (pawn.HasCorticalStack(out var hediff))
                {
                    race = hediff.PersonaData.race;
                    gender = hediff.PersonaData.gender;
                }
                else
                {
                    race = pawn.def;
                    gender = pawn.gender;
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
        }

        private static Type VEPsycastModExtensionType = AccessTools.TypeByName("VanillaPsycastsExpanded.AbilityExtension_Psycast");
        private bool CanStoreAbility(Pawn pawn, Def def)
        {
            if (def is AbilityDef abilityDef)
            {
                if (IsNaturalAbilityFor(pawn, abilityDef))
                {
                    return false;
                }
                if (typeof(Psycast).IsAssignableFrom(abilityDef.abilityClass))
                {
                    return true;
                }
            }
            else if (def is VFECore.Abilities.AbilityDef && VEPsycastModExtensionType != null)
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
            return false;
        }

        private static bool IsNaturalAbilityFor(Pawn pawn, AbilityDef ability)
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
            return false;
        }

        public void CopyDataFrom(PersonaData other, bool isDuplicateOperation = false)
        {
            sourceStack = other.sourceStack;
            name = other.name;
            origPawn = other.origPawn;
            hostilityMode = other.hostilityMode;
            areaRestriction = other.areaRestriction;
            ageChronologicalTicks = other.ageChronologicalTicks;
            medicalCareCategory = other.medicalCareCategory;
            selfTend = other.selfTend;
            foodRestriction = other.foodRestriction;
            outfit = other.outfit;
            drugPolicy = other.drugPolicy;
            times = other.times;
            thoughts = other.thoughts;
            faction = other.faction;
            isFactionLeader = other.isFactionLeader;
            traits = other.traits;
            relations = other.relations;
            everSeenByPlayer = other.everSeenByPlayer;
            canGetRescuedThought = other.canGetRescuedThought;
            relativeInvolvedInRescueQuest = other.relativeInvolvedInRescueQuest;
            nextMarriageNameChange = other.nextMarriageNameChange;
            hidePawnRelations = other.hidePawnRelations;

            relatedPawns = other.relatedPawns;
            skills = other.skills;
            xenotypeDef = other.xenotypeDef;
            xenotypeName = other.xenotypeName;
            childhood = other.childhood;
            adulthood = other.adulthood;
            title = other.title;
            priorities = other.priorities;

            psylinkLevel = other.psylinkLevel;
            abilities = other.abilities;
            VEAbilities = other.VEAbilities;
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
            battleActive = other.battleActive;
            battleExitTick = other.battleExitTick;

            if (gender == Gender.None)
            {
                gender = other.gender;
            }
            if (race == null)
            {
                race = other.race;
            }


            pawnID = other.pawnID;

            if (ModsConfig.RoyaltyActive)
            {
                royalTitles = other.royalTitles;
                favor = other.favor;
                heirs = other.heirs;
                bondedThings = other.bondedThings;
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
            hackedWhileOnStack = other.hackedWhileOnStack;
            stackGroupID = other.stackGroupID;

            sexuality = other.sexuality;
            romanceFactor = other.romanceFactor;
            psychologyData = other.psychologyData;
            rjwData = other.rjwData;
        }


        public void OverwritePawn(Pawn pawnToOverwrite, StackSavingOptionsModExtension extension, Pawn original = null, bool overwriteOriginalPawn = true)
        {
            this.origPawn = FindOrigPawn(original);
            if (origPawn != null)
            {
                CopyFromPawn(origPawn, sourceStack);
            }
            pawnToOverwrite.Name = name;
            PawnComponentsUtility.CreateInitialComponents(pawnToOverwrite);
            if (pawnToOverwrite.Faction != faction)
            {
                pawnToOverwrite.SetFaction(faction);
            }
            if (isFactionLeader && overwriteOriginalPawn && pawnToOverwrite.Faction != null)
            {
                pawnToOverwrite.Faction.leader = pawnToOverwrite;
            }
            if (pawnToOverwrite.CanThink())
            {
                for (int num = pawnToOverwrite.needs.mood.thoughts.memories.Memories.Count - 1; num >= 0; num--)
                {
                    pawnToOverwrite.needs.mood.thoughts.memories.RemoveMemory(pawnToOverwrite.needs.mood.thoughts.memories.Memories[num]);
                }
            }
            
            if (thoughts != null)
            {
                if (gender == pawnToOverwrite.gender)
                {
                    thoughts.RemoveAll(x => x.def == AC_DefOf.VFEU_WrongGender);
                    thoughts.RemoveAll(x => x.def == AC_DefOf.VFEU_WrongGenderDouble);
                    thoughts.RemoveAll(x => x.def == AC_DefOf.VFEU_WrongGenderPregnant);
                    thoughts.RemoveAll(x => x.def == AC_DefOf.VFEU_WrongGenderChild);
                }
                if (race == pawnToOverwrite.kindDef.race)
                {
                    thoughts.RemoveAll(x => x.def == AC_DefOf.VFEU_WrongRace);
                }
                if (pawnToOverwrite.CanThink())
                {
                    foreach (Thought_Memory thought in thoughts)
                    {
                        if (thought is Thought_MemorySocial && thought.otherPawn == null)
                        {
                            continue;
                        }
                        pawnToOverwrite.needs.mood.thoughts.memories.TryGainMemory(thought, thought.otherPawn);
                    }
                }
            }
            if (extension != null)
            {
                pawnToOverwrite.story.traits.allTraits.RemoveAll(x => !extension.ignoresTraits.Contains(x.def.defName));
            }
            else
            {
                pawnToOverwrite.story.traits.allTraits.Clear();
            }
            if (traits != null)
            {
                foreach (Trait trait in traits)
                {
                    if (extension != null && extension.ignoresTraits != null && extension.ignoresTraits.Contains(trait.def.defName))
                    {
                        continue;
                    }
                    pawnToOverwrite.story.traits.GainTrait(trait);
                }
            }
            
            pawnToOverwrite.mindState = new Pawn_MindState(pawnToOverwrite);
            pawnToOverwrite.relations = new Pawn_RelationsTracker(pawnToOverwrite)
            {
                everSeenByPlayer = everSeenByPlayer,
                canGetRescuedThought = canGetRescuedThought,
                relativeInvolvedInRescueQuest = relativeInvolvedInRescueQuest,
                nextMarriageNameChange = nextMarriageNameChange,
                hidePawnRelations = hidePawnRelations
            };
            
            if (overwriteOriginalPawn)
            {
                var oldOrigPawn = this.origPawn;
                this.origPawn = pawnToOverwrite;
                this.pawnID = this.origPawn.thingIDNumber;
                if (relations != null)
                {
                    for (var i = relations.Count - 1; i >= 0; i--)
                    {
                        var rel = relations[i];
                        if (rel.otherPawn != null)
                        {
                            ReplaceSocialReferences(rel.otherPawn, pawnToOverwrite);
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
                            ReplaceSocialReferences(relatedPawn, pawnToOverwrite);
                        }
                    }
                }
                if (oldOrigPawn != null)
                {
                    var potentiallyRelatedPawns = oldOrigPawn.relations.PotentiallyRelatedPawns.ToList();
                    for (var i = potentiallyRelatedPawns.Count - 1; i >= 0; i--)
                    {
                        var relatedPawn = potentiallyRelatedPawns[i];
                        if (relatedPawn != null)
                        {
                            ReplaceSocialReferences(relatedPawn, pawnToOverwrite);
                        }
                    }

                    for (var i = oldOrigPawn.relations.DirectRelations.Count - 1; i >= 0; i--)
                    {
                        var oldDirectRelation = oldOrigPawn.relations.DirectRelations[i];
                        oldOrigPawn.relations.directRelations.Remove(oldDirectRelation);
                        if (pawnToOverwrite.relations.directRelations.Any(x => x.def == oldDirectRelation.def && x.otherPawn == oldDirectRelation.otherPawn) is false)
                        {
                            pawnToOverwrite.relations.directRelations.Add(new DirectPawnRelation(oldDirectRelation.def,
                            oldDirectRelation.otherPawn, oldDirectRelation.startTicks));
                        }
                        if (oldDirectRelation.otherPawn.relations.pawnsWithDirectRelationsWithMe.Contains(oldOrigPawn))
                        {
                            oldDirectRelation.otherPawn.relations.pawnsWithDirectRelationsWithMe.Remove(oldOrigPawn);
                        }
                        if (oldDirectRelation.otherPawn.relations.pawnsWithDirectRelationsWithMe.Contains(pawnToOverwrite) is false)
                        {
                            oldDirectRelation.otherPawn.relations.pawnsWithDirectRelationsWithMe.Add(pawnToOverwrite);
                        }
                    }
                    oldOrigPawn.relations = new Pawn_RelationsTracker(oldOrigPawn);
                }

                if (pawnToOverwrite.needs?.mood?.thoughts != null)
                {
                    pawnToOverwrite.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
                }
            }

            var oldAbilities = pawnToOverwrite.abilities?.abilities.Select(x => x.def);
            pawnToOverwrite.abilities = new Pawn_AbilityTracker(pawnToOverwrite);
            if (oldAbilities != null)
            {
                foreach (var ability in oldAbilities)
                {
                    if (IsNaturalAbilityFor(pawnToOverwrite, ability))
                    {
                        pawnToOverwrite.abilities.GainAbility(ability);
                    }
                }
            }
            var compAbilities = pawnToOverwrite.GetComp<VFECore.Abilities.CompAbilities>();
            if (compAbilities != null)
            {
                compAbilities.LearnedAbilities?.Clear();
            }
            pawnToOverwrite.psychicEntropy = new Pawn_PsychicEntropyTracker(pawnToOverwrite);
            if (this.sourceStack == AC_DefOf.AC_FilledArchoStack)
            {
                var hediff_Psylink = pawnToOverwrite.GetMainPsylinkSource() as Hediff_Psylink;
                if (this.psylinkLevel.HasValue)
                {
                    if (hediff_Psylink == null)
                    {
                        Hediff_Psylink_TryGiveAbilityOfLevel_Patch.suppress = true;
                        hediff_Psylink = HediffMaker.MakeHediff(HediffDefOf.PsychicAmplifier, pawnToOverwrite,
                            pawnToOverwrite.health.hediffSet.GetBrain()) as Hediff_Psylink;
                        pawnToOverwrite.health.AddHediff(hediff_Psylink);
                        Hediff_Psylink_TryGiveAbilityOfLevel_Patch.suppress = false;
                    }
                    var levelOffset = this.psylinkLevel.Value - hediff_Psylink.level;
                    hediff_Psylink.level = (int)Mathf.Clamp(hediff_Psylink.level + levelOffset, hediff_Psylink.def.minSeverity, hediff_Psylink.def.maxSeverity);
                }
            
                pawnToOverwrite.psychicEntropy.currentEntropy = currentEntropy;
                pawnToOverwrite.psychicEntropy.currentPsyfocus = currentPsyfocus;
                pawnToOverwrite.psychicEntropy.limitEntropyAmount = limitEntropyAmount;
                pawnToOverwrite.psychicEntropy.targetPsyfocus = targetPsyfocus;
            
                if (abilities.NullOrEmpty() is false)
                {
                    foreach (var def in abilities)
                    {
                        pawnToOverwrite.abilities.GainAbility(def);
                    }
                }

                if (VPE_PsycastAbilityImplant != null)
                {
                    pawnToOverwrite.health.hediffSet.hediffs.RemoveAll(x => x.def.defName == "VPE_PsycastAbilityImplant");
                    pawnToOverwrite.health.AddHediff(VPE_PsycastAbilityImplant);
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
            
            pawnToOverwrite.skills.skills.Clear();
            if (skills != null)
            {
                foreach (SkillRecord skill in skills)
                {
                    SkillRecord newSkill = new SkillRecord(pawnToOverwrite, skill.def)
                    {
                        passion = skill.passion,
                        levelInt = skill.levelInt,
                        xpSinceLastLevel = skill.xpSinceLastLevel,
                        xpSinceMidnight = skill.xpSinceMidnight
                    };
                    pawnToOverwrite.skills.skills.Add(newSkill);
                }
            }
            
            if (!childhood.NullOrEmpty())
            {
                pawnToOverwrite.story.Childhood = DefDatabase<BackstoryDef>.GetNamedSilentFail(childhood);
            }
            
            pawnToOverwrite.story.Adulthood = !adulthood.NullOrEmpty() ? DefDatabase<BackstoryDef>.GetNamedSilentFail(adulthood) : null;
            pawnToOverwrite.story.title = title;
            
            if (pawnToOverwrite.workSettings == null)
            {
                pawnToOverwrite.workSettings = new Pawn_WorkSettings(pawnToOverwrite);
            }
            pawnToOverwrite.workSettings.priorities = new DefMap<WorkTypeDef, int>();
            pawnToOverwrite.Notify_DisabledWorkTypesChanged();
            if (priorities != null)
            {
                foreach (KeyValuePair<WorkTypeDef, int> priority in priorities)
                {
                    pawnToOverwrite.workSettings.SetPriority(priority.Key, priority.Value);
                }
            }
            if (pawnToOverwrite.guest is null)
            {
                pawnToOverwrite.guest = new Pawn_GuestTracker(pawnToOverwrite);
            }
            pawnToOverwrite.guest.guestStatusInt = guestStatusInt;
            pawnToOverwrite.guest.interactionMode = interactionMode;
            pawnToOverwrite.guest.slaveInteractionMode = slaveInteractionMode;
            pawnToOverwrite.guest.hostFactionInt = hostFactionInt;
            pawnToOverwrite.guest.joinStatus = joinStatus;
            pawnToOverwrite.guest.slaveFactionInt = slaveFactionInt;
            pawnToOverwrite.guest.lastRecruiterName = lastRecruiterName;
            pawnToOverwrite.guest.lastRecruiterOpinion = lastRecruiterOpinion;
            pawnToOverwrite.guest.hasOpinionOfLastRecruiter = hasOpinionOfLastRecruiter;
            pawnToOverwrite.guest.Released = releasedInt;
            pawnToOverwrite.guest.ticksWhenAllowedToEscapeAgain = ticksWhenAllowedToEscapeAgain;
            pawnToOverwrite.guest.spotToWaitInsteadOfEscaping = spotToWaitInsteadOfEscaping;
            pawnToOverwrite.guest.lastPrisonBreakTicks = lastPrisonBreakTicks;
            pawnToOverwrite.guest.everParticipatedInPrisonBreak = everParticipatedInPrisonBreak;
            pawnToOverwrite.guest.resistance = resistance;
            pawnToOverwrite.guest.will = will;
            pawnToOverwrite.guest.ideoForConversion = ideoForConversion;
            pawnToOverwrite.guest.everEnslaved = everEnslaved;
            pawnToOverwrite.guest.recruitable = recruitable;
            pawnToOverwrite.guest.getRescuedThoughtOnUndownedBecauseOfPlayer = getRescuedThoughtOnUndownedBecauseOfPlayer;
            
            if (pawnToOverwrite.records is null)
            {
                pawnToOverwrite.records = new Pawn_RecordsTracker(pawnToOverwrite);
            }
            if (records != null)
            {
                pawnToOverwrite.records.records = records;
                pawnToOverwrite.records.battleActive = battleActive;
                pawnToOverwrite.records.battleExitTick = battleExitTick;
            }
            
            if (pawnToOverwrite.playerSettings is null)
            {
                pawnToOverwrite.playerSettings = new Pawn_PlayerSettings(pawnToOverwrite);
            }
            pawnToOverwrite.playerSettings.hostilityResponse = (HostilityResponseMode)hostilityMode;
            pawnToOverwrite.playerSettings.AreaRestriction = areaRestriction;
            pawnToOverwrite.playerSettings.medCare = medicalCareCategory;
            pawnToOverwrite.playerSettings.selfTend = selfTend;
            pawnToOverwrite.foodRestriction = new Pawn_FoodRestrictionTracker(pawnToOverwrite);
            pawnToOverwrite.foodRestriction.CurrentFoodRestriction = foodRestriction;
            pawnToOverwrite.outfits = new Pawn_OutfitTracker(pawnToOverwrite);
            try
            {
                pawnToOverwrite.outfits.CurrentOutfit = outfit;
            }
            catch { }
            pawnToOverwrite.drugs = new Pawn_DrugPolicyTracker(pawnToOverwrite);
            pawnToOverwrite.drugs.CurrentPolicy = drugPolicy;
            pawnToOverwrite.ageTracker.AgeChronologicalTicks = ageChronologicalTicks;
            pawnToOverwrite.timetable = new Pawn_TimetableTracker(pawnToOverwrite);

            if (times != null && times.Count == 24)
            {
                pawnToOverwrite.timetable.times = times;
            }

            if (pawnToOverwrite.CanThink())
            {
                if (pawnToOverwrite.gender != gender)
                {
                    if (pawnToOverwrite.story.traits.HasTrait(TraitDefOf.BodyPurist))
                    {
                        pawnToOverwrite.needs.mood.thoughts.memories.TryGainMemory(AC_DefOf.VFEU_WrongGenderDouble);
                    }
                    else
                    {
                        pawnToOverwrite.needs.mood.thoughts.memories.TryGainMemory(AC_DefOf.VFEU_WrongGender);
                    }
                }


                if (pawnToOverwrite.kindDef.race != race)
                {
                    pawnToOverwrite.needs.mood.thoughts.memories.TryGainMemory(AC_DefOf.VFEU_WrongRace);
                }
            }


            if (ModsConfig.RoyaltyActive)
            {
                pawnToOverwrite.royalty = new Pawn_RoyaltyTracker(pawnToOverwrite);
                if (royalTitles != null)
                {
                    foreach (RoyalTitle title in royalTitles)
                    {
                        pawnToOverwrite.royalty.SetTitle(title.faction, title.def, false, false, false);
                    }
                }
                if (heirs != null)
                {
                    foreach (KeyValuePair<Faction, Pawn> heir in heirs)
                    {
                        pawnToOverwrite.royalty.SetHeir(heir.Value, heir.Key);
                    }
                }
            
                if (favor != null)
                {
                    foreach (KeyValuePair<Faction, int> fav in favor)
                    {
                        pawnToOverwrite.royalty.SetFavor(fav.Key, fav.Value);
                    }
                }
            
                if (bondedThings != null)
                {
                    foreach (Thing bonded in bondedThings)
                    {
                        CompBladelinkWeapon comp = bonded.TryGetComp<CompBladelinkWeapon>();
                        if (comp != null)
                        {
                            comp.CodeFor(pawnToOverwrite);
                        }
                    }
                }
                if (factionPermits != null)
                {
                    pawnToOverwrite.royalty.factionPermits = factionPermits;
                }
            }
            
            if (ModsConfig.IdeologyActive)
            {
                if (precept_RoleMulti != null)
                {
                    if (precept_RoleMulti.chosenPawns is null)
                    {
                        precept_RoleMulti.chosenPawns = new List<IdeoRoleInstance>();
                    }
            
                    precept_RoleMulti.chosenPawns.Add(new IdeoRoleInstance(precept_RoleMulti)
                    {
                        pawn = pawnToOverwrite
                    });
                    precept_RoleMulti.FillOrUpdateAbilities();
                }
                if (precept_RoleSingle != null)
                {
                    precept_RoleSingle.chosenPawn = new IdeoRoleInstance(precept_RoleMulti)
                    {
                        pawn = pawnToOverwrite
                    };
                    precept_RoleSingle.FillOrUpdateAbilities();
                }
            
                if (ideo != null)
                {
                    pawnToOverwrite.ideo.SetIdeo(ideo);
                    pawnToOverwrite.ideo.certaintyInt = certainty;
                    pawnToOverwrite.ideo.previousIdeos = previousIdeos;
                    pawnToOverwrite.ideo.joinTick = joinTick;
                }
                if (favoriteColor.HasValue)
                {
                    pawnToOverwrite.story.favoriteColor = favoriteColor.Value;
                }
            
            }
            
            if (ModCompatibility.IndividualityIsActive)
            {
                ModCompatibility.SetSyrTraitsSexuality(pawnToOverwrite, sexuality);
                ModCompatibility.SetSyrTraitsRomanceFactor(pawnToOverwrite, romanceFactor);
            }
            
            if (ModCompatibility.PsychologyIsActive && psychologyData != null)
            {
                ModCompatibility.SetPsychologyData(pawnToOverwrite, psychologyData);
            }
            if (ModCompatibility.RimJobWorldIsActive && rjwData != null)
            {
                ModCompatibility.SetRjwData(pawnToOverwrite, rjwData);
            }
        }

        private void ReplaceSocialReferences(Pawn relatedPawn, Pawn newReference)
        {
            if (relatedPawn.CanThink())
            {
                foreach (Thought_Memory thought in relatedPawn.needs.mood.thoughts.memories.Memories)
                {
                    if (IsPresetPawn(thought.otherPawn))
                    {
                        thought.otherPawn = newReference;
                    }
                }
            }

            if (relatedPawn.relations != null)
            {
                var pawnsWithDirectRelations = relatedPawn.relations.pawnsWithDirectRelationsWithMe.ToList();
                for (var j = pawnsWithDirectRelations.Count - 1; j >= 0; j--)
                {
                    var otherPawn = pawnsWithDirectRelations[j];
                    if (IsPresetPawn(otherPawn))
                    {
                        relatedPawn.relations.pawnsWithDirectRelationsWithMe.Remove(otherPawn);
                        relatedPawn.relations.pawnsWithDirectRelationsWithMe.Add(newReference);
                    }
                }

                var otherPawnRelations = relatedPawn.relations.DirectRelations;
                for (var i = otherPawnRelations.Count - 1; i >= 0; i--)
                {
                    var rel = otherPawnRelations[i];
                    if (rel != null)
                    {
                        if (IsPresetPawn(rel.otherPawn))
                        {
                            if (rel.otherPawn != newReference)
                            {
                                rel.otherPawn = newReference;
                            }
                        }
                    }
                }
            }
            if (relatedPawn.needs?.mood?.thoughts != null)
            {
                relatedPawn.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
            }
        }

        public bool IsPresetPawn(Pawn pawn)
        {
            if (pawn == null || pawn.Name == null) return false;
            return pawn != null && (pawn.thingIDNumber == pawnID || origPawn == pawn || name != null && name.ToString() == pawn.Name.ToString());
        }

        public Pawn FindOrigPawn(Pawn pawn)
        {
            if (origPawn != null)
            {
                return origPawn;
            }
            if (pawn?.Name != null)
            {
                foreach (Pawn otherPawn in AlteredCarbonManager.Instance.PawnsWithStacks)
                {
                    if (otherPawn.Name != null && otherPawn.Name.ToStringFull == pawn.Name.ToStringFull)
                    {
                        origPawn = otherPawn;
                        return otherPawn;
                    }
                }

                if (relatedPawns != null)
                {
                    foreach (Pawn otherPawn in relatedPawns)
                    {
                        if (otherPawn?.relations?.DirectRelations != null)
                        {
                            foreach (DirectPawnRelation rel in otherPawn.relations.DirectRelations)
                            {
                                if (rel?.otherPawn?.Name != null)
                                {
                                    if (rel.otherPawn.Name.ToStringFull == pawn.Name.ToStringFull && rel.otherPawn != pawn)
                                    {
                                        origPawn = rel.otherPawn;
                                        return rel.otherPawn;
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (Pawn otherPawn in PawnsFinder.AllMaps)
                {
                    if (otherPawn.Name != null && otherPawn.Name.ToStringFull == pawn.Name.ToStringFull && otherPawn != pawn)
                    {
                        origPawn = otherPawn;
                        return otherPawn;
                    }
                }
                origPawn = pawn;
            }
            return origPawn;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref stackGroupID, "stackGroupID", 0);
            Scribe_Values.Look(ref isCopied, "isCopied", false, false);
            Scribe_Values.Look(ref diedFromCombat, "diedFromCombat");
            Scribe_Values.Look(ref hackedWhileOnStack, "hackedWhileOnStack");
            Scribe_Deep.Look(ref name, "name", new object[0]);
            Scribe_References.Look(ref origPawn, "origPawn", true);
            Scribe_Values.Look(ref hostilityMode, "hostilityMode");
            Scribe_References.Look(ref areaRestriction, "areaRestriction", false);
            Scribe_Values.Look(ref medicalCareCategory, "medicalCareCategory", MedicalCareCategory.NoCare, false);
            Scribe_Values.Look(ref selfTend, "selfTend", false, false);
            Scribe_Values.Look(ref ageChronologicalTicks, "ageChronologicalTicks", 0, false);
            Scribe_Defs.Look(ref race, "race");
            Scribe_References.Look(ref outfit, "outfit", false);
            Scribe_References.Look(ref foodRestriction, "foodPolicy", false);
            Scribe_References.Look(ref drugPolicy, "drugPolicy", false);

            Scribe_Collections.Look(ref times, "times", LookMode.Def);
            Scribe_Collections.Look(ref thoughts, "thoughts", LookMode.Deep);
            Scribe_References.Look(ref faction, "faction", true);
            Scribe_Values.Look(ref isFactionLeader, "isFactionLeader", false, false);

            Scribe_Collections.Look(ref skills, "skills",LookMode.Deep);
            Scribe_Values.Look(ref childhood, "childhood", null, false);
            Scribe_Values.Look(ref adulthood, "adulthood", null, false);
            Scribe_Values.Look(ref title, "title", null, false);
            Scribe_Defs.Look(ref xenotypeDef, "xenotypeDef");
            Scribe_Values.Look(ref xenotypeName, "xenotypeName");
            Scribe_Values.Look(ref pawnID, "pawnID", 0, false);
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
            Scribe_Values.Look(ref gender, "gender");
            Scribe_Values.Look(ref lastTimeUpdated, "lastTimeUpdated");
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
            Scribe_Values.Look(ref sexuality, "sexuality", -1);
            Scribe_Values.Look(ref romanceFactor, "romanceFactor", -1f);
            Scribe_Deep.Look(ref psychologyData, "psychologyData");
            Scribe_Deep.Look(ref rjwData, "rjwData");
            Scribe_Values.Look(ref restoreToEmptyStack, "restoreToEmptyStack", true);
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
                if (this.origPawn != null)
                {
                    VPE_PsycastAbilityImplant.pawn = this.origPawn;
                }
                VPE_PsycastAbilityImplant.loadID = Find.UniqueIDsManager.GetNextHediffID();
            }

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
            }
        }

        private List<Faction> favorKeys = new List<Faction>();
        private List<int> favorValues = new List<int>();

        private List<Faction> heirsKeys = new List<Faction>();
        private List<Pawn> heirsValues = new List<Pawn>();

        public override string ToString()
        {
            return name + " - " + faction + " - " + pawnID;
        }
    }
}

