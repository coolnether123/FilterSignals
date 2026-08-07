using System.Collections.Generic;
using RimWorld;
using FilterSignals.Domain;
using Verse;

namespace FilterSignals.Runtime
{
    /// <summary>
    /// Freezes the map capabilities needed by a classification pass so many
    /// filter rows share bounded, internally consistent colony observations.
    /// </summary>
    internal sealed class MapCapabilitySnapshot
    {
        private readonly Dictionary<ThingDef, List<Building>> sources;
        private readonly IReadOnlyList<Pawn> capablePawns;

        private MapCapabilitySnapshot(
            Dictionary<ThingDef, List<Building>> sources,
            IReadOnlyList<Pawn> capablePawns,
            int gameTick)
        {
            this.sources = sources;
            this.capablePawns = capablePawns;
            GameTick = gameTick;
        }

        internal int GameTick { get; }

        internal ProductionSourceSelection SelectSource(
            RecipeDef recipe,
            IReadOnlyList<ThingDef> sourceDefs,
            string fallbackPathLabel)
        {
            var candidates = new List<ProductionSourceCandidate>();
            if (recipe == null || sourceDefs == null)
            {
                return ProductionSourceSelector.Select(
                    fallbackPathLabel,
                    candidates);
            }

            for (int sourceIndex = 0;
                sourceIndex < sourceDefs.Count;
                sourceIndex++)
            {
                ThingDef sourceDef = sourceDefs[sourceIndex];
                if (sourceDef == null ||
                    !sources.TryGetValue(
                        sourceDef,
                        out List<Building> instances))
                {
                    continue;
                }

                string pathLabel =
                    sourceDef.label ??
                    sourceDef.defName ??
                    fallbackPathLabel;
                for (int instanceIndex = 0;
                    instanceIndex < instances.Count;
                    instanceIndex++)
                {
                    Building building = instances[instanceIndex];

                    // Modded RecipeWorkers may accept one instance of a
                    // building definition and reject another.
                    bool billGiverUsable =
                        !(building is IBillGiver billGiver) ||
                        billGiver.CurrentlyUsableForBills();
                    bool recipeAvailable =
                        billGiverUsable &&
                        RecipeAvailableOnInstance(recipe, building);
                    candidates.Add(new ProductionSourceCandidate(
                        pathLabel,
                        billGiverUsable,
                        recipeAvailable));
                }
            }

            return ProductionSourceSelector.Select(
                fallbackPathLabel,
                candidates);
        }

        internal bool HasCapablePawn(RecipeDef recipe)
        {
            if (recipe == null)
            {
                return false;
            }

            bool hasWorkRequirement = recipe.requiredGiverWorkType != null;
            bool hasSkillRequirement =
                recipe.skillRequirements != null &&
                recipe.skillRequirements.Count > 0;
#if FILTER_SIGNALS_HAS_MECHS
            bool hasMechanitorRequirement = recipe.mechanitorOnlyRecipe;
#else
            bool hasMechanitorRequirement = false;
#endif
            if (!hasWorkRequirement &&
                !hasSkillRequirement &&
                !hasMechanitorRequirement)
            {
                return true;
            }

            for (int i = 0; i < capablePawns.Count; i++)
            {
                Pawn pawn = capablePawns[i];
#if FILTER_SIGNALS_HAS_MECHS
                if (hasMechanitorRequirement &&
                    !MechanitorUtility.IsMechanitor(pawn))
                {
                    continue;
                }
#endif

                if (recipe.requiredGiverWorkType != null &&
                    pawn.WorkTypeIsDisabled(recipe.requiredGiverWorkType))
                {
                    continue;
                }

                bool skillsSatisfied = true;
                if (recipe.skillRequirements != null)
                {
                    for (int requirementIndex = 0;
                        requirementIndex < recipe.skillRequirements.Count;
                        requirementIndex++)
                    {
                        if (!recipe.skillRequirements[requirementIndex]
                            .PawnSatisfies(pawn))
                        {
                            skillsSatisfied = false;
                            break;
                        }
                    }
                }

                if (skillsSatisfied)
                {
                    return true;
                }
            }

            return false;
        }

        internal Building FindUsableSource(
            RecipeDef recipe,
            IReadOnlyList<ThingDef> sourceDefs)
        {
            if (recipe == null || sourceDefs == null)
            {
                return null;
            }

            for (int sourceIndex = 0;
                sourceIndex < sourceDefs.Count;
                sourceIndex++)
            {
                ThingDef sourceDef = sourceDefs[sourceIndex];
                if (sourceDef == null ||
                    !sources.TryGetValue(
                        sourceDef,
                        out List<Building> instances))
                {
                    continue;
                }

                for (int instanceIndex = 0;
                    instanceIndex < instances.Count;
                    instanceIndex++)
                {
                    Building building = instances[instanceIndex];
                    bool billGiverUsable =
                        !(building is IBillGiver billGiver) ||
                        billGiver.CurrentlyUsableForBills();
                    if (billGiverUsable &&
                        RecipeAvailableOnInstance(recipe, building))
                    {
                        return building;
                    }
                }
            }

            return null;
        }

        internal static MapCapabilitySnapshot Capture(
            Map map,
            DefinitionProductionIndex index,
            int gameTick)
        {
            var sources = new Dictionary<ThingDef, List<Building>>();
            var pawns = new List<Pawn>();
            if (map == null)
            {
                return new MapCapabilitySnapshot(sources, pawns, gameTick);
            }

            List<Building> buildings =
                map.listerBuildings.allBuildingsColonist;
            for (int i = 0; i < buildings.Count; i++)
            {
                Building building = buildings[i];
                if (building == null ||
                    !building.Spawned ||
                    !index.IsProductionSource(building.def))
                {
                    continue;
                }

                if (sources.TryGetValue(
                    building.def,
                    out List<Building> existing))
                {
                    existing.Add(building);
                }
                else
                {
                    sources.Add(
                        building.def,
                        new List<Building> { building });
                }
            }

            foreach (List<Building> instances in sources.Values)
            {
                // Navigation must select the same usable building until the
                // map itself changes, regardless of lister enumeration order.
                instances.Sort((left, right) =>
                    left.thingIDNumber.CompareTo(right.thingIDNumber));
            }

            IReadOnlyList<Pawn> spawnedPawns = map.mapPawns.AllPawnsSpawned;
            for (int i = 0; i < spawnedPawns.Count; i++)
            {
                Pawn pawn = spawnedPawns[i];
                if (pawn != null &&
                    pawn.Faction == Faction.OfPlayer &&
                    !pawn.Dead &&
                    !pawn.Downed &&
                    !pawn.InMentalState &&
#if FILTER_SIGNALS_HAS_MECHS
                    (pawn.IsColonist || pawn.IsColonyMech))
#else
                    pawn.IsColonist)
#endif
                {
                    // Guests, downed pawns, and pawns unable to work should
                    // not make a cosmetic signal claim a usable recipe.
                    pawns.Add(pawn);
                }
            }

            return new MapCapabilitySnapshot(sources, pawns, gameTick);
        }

        private static bool RecipeAvailableOnInstance(
            RecipeDef recipe,
            Building building)
        {
            try
            {
                return recipe.AvailableOnNow(building);
            }
            catch (System.Exception exception)
            {
                // AvailableOnNow is an extensibility boundary. One broken
                // RecipeWorker must not break the entire filter dialog.
                int key = StableHash(
                    "FilterSignals.AvailableOnNow." +
                    (recipe.defName ?? "<unnamed>"));
                Log.ErrorOnce(
                    "[Filter Signals] RecipeDef.AvailableOnNow failed " +
                    "for recipe '" + (recipe.defName ?? "<unnamed>") +
                    "' on '" +
                    (building?.def?.defName ?? "<unknown source>") +
                    "' and that source instance was ignored: " + exception,
                    key);
                return false;
            }
        }

        private static int StableHash(string value)
        {
            unchecked
            {
                int hash = 17;
                for (int i = 0; i < value.Length; i++)
                {
                    hash = (hash * 31) + value[i];
                }

                return hash;
            }
        }
    }
}
