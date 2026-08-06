using System;
using System.Collections.Generic;
using RimWorld;
using Spine.Caching;
using FilterSignals.Compatibility;
using FilterSignals.Domain;
using FilterSignals.Settings;
using Verse;

namespace FilterSignals.Runtime
{
    /// <summary>
    /// Orchestrates indexing, map snapshots, integrations, and bounded caches
    /// behind the single classification entry point used by presentation.
    /// </summary>
    internal static class ClassificationService
    {
        private const int SnapshotMaxAgeTicks = 120;
        private const long CacheBudgetBytes = 256 * 1024;
        private const long EstimatedEntryBytes = 320;

        private static readonly Dictionary<Map, MapState> States =
            new Dictionary<Map, MapState>();
        private static readonly MapState NoMapState = new MapState();
        private static DefinitionProductionIndex index;

        internal static ClassificationResult Get(
            ThingDef item,
            Map map)
        {
            if (item == null)
            {
                return new ClassificationResult(
                    ProductionClassification.NotApplicable,
                    "No item definition was supplied.");
            }

            EnsureInitialized();
            MapState state = GetState(map);
            int gameTick = CurrentGameTick();
            if (state.Snapshot == null ||
                gameTick - state.Snapshot.GameTick >= SnapshotMaxAgeTicks)
            {
                // A short safety refresh covers pawn, fuel, breakdown, and
                // power changes without Harmony hooks on their hot paths.
                state.Snapshot =
                    MapCapabilitySnapshot.Capture(map, index, gameTick);

                // Cached answers are meaningful only for the snapshot that
                // produced them.
                state.Results.Reset();
            }

            if (state.Results.TryGet(item, out ClassificationResult cached))
            {
                return cached;
            }

            ClassificationResult result = Evaluate(item, map, state.Snapshot);
            state.Results.AddOrUpdate(
                item,
                result,
                EstimatedEntryBytes);
            return result;
        }

        internal static bool IsProductionSource(ThingDef thingDef)
        {
            EnsureInitialized();
            return index.IsProductionSource(thingDef);
        }

        internal static DefinitionProductionIndex ProductionIndex
        {
            get
            {
                EnsureInitialized();
                return index;
            }
        }

        internal static void Invalidate(Map map)
        {
            if (map == null)
            {
                NoMapState.Snapshot = null;
                NoMapState.Results.Reset();
            }
            else if (States.TryGetValue(map, out MapState state))
            {
                state.Snapshot = null;
                state.Results.Reset();
            }
        }

        internal static void InvalidateAll()
        {
            foreach (MapState state in States.Values)
            {
                state.Snapshot = null;
                state.Results.Reset();
            }
            NoMapState.Snapshot = null;
            NoMapState.Results.Reset();
        }

        internal static void Release(Map map)
        {
            if (map != null &&
                States.TryGetValue(map, out MapState state))
            {
                state.Results.Reset();
                States.Remove(map);
            }
        }

        private static ClassificationResult Evaluate(
            ThingDef item,
            Map map,
            MapCapabilitySnapshot snapshot)
        {
            ClassificationResult classificationOverride =
                EvaluateOverrides(item, map);
            if (classificationOverride != null)
            {
                // Overrides exist for semantics that cannot compose as paths,
                // so they intentionally bypass vanilla and provider results.
                return classificationOverride;
            }

            var paths = new List<ProductionPathAssessment>();
            IReadOnlyList<RecipeDef> recipes = index.RecipesFor(item);
            for (int recipeIndex = 0;
                recipeIndex < recipes.Count;
                recipeIndex++)
            {
                paths.Add(RecipeAssessmentFactory.Evaluate(
                    recipes[recipeIndex],
                    map,
                    snapshot,
                    index));
            }

            AddCustomPaths(paths, item, map);
            return ProductionClassifier.Classify(paths);
        }

        private static ClassificationResult EvaluateOverrides(
            ThingDef item,
            Map map)
        {
            IReadOnlyList<IFilterSignalsClassificationOverride> overrides =
                FilterSignalsApi.GetClassificationOverrides();
            for (int i = 0; i < overrides.Count; i++)
            {
                IFilterSignalsClassificationOverride itemOverride = overrides[i];
                try
                {
                    if (itemOverride.TryClassify(
                        item,
                        map,
                        out ClassificationResult result) &&
                        result != null)
                    {
                        return result;
                    }
                }
                catch (Exception exception)
                {
                    // Compatibility failures are isolated so one integration
                    // cannot suppress vanilla or other providers' answers.
                    LogProviderFailure(
                        itemOverride.Id,
                        "classification override",
                        exception);
                }
            }

            return null;
        }

        private static void AddCustomPaths(
            ICollection<ProductionPathAssessment> paths,
            ThingDef item,
            Map map)
        {
            IReadOnlyList<IFilterSignalsProductionProvider> providers =
                FilterSignalsApi.GetProductionProviders();
            for (int i = 0; i < providers.Count; i++)
            {
                IFilterSignalsProductionProvider provider = providers[i];
                try
                {
                    IEnumerable<ProductionPathAssessment> supplied =
                        provider.GetProductionPaths(
                            item,
                            map,
                            FilterSignals.Bootstrap.FilterSignalsMod.Settings
                                .ConsiderMaterialShortages);
                    if (supplied == null)
                    {
                        continue;
                    }

                    foreach (ProductionPathAssessment path in supplied)
                    {
                        if (path != null)
                        {
                            paths.Add(path);
                        }
                    }
                }
                catch (Exception exception)
                {
                    // Providers are optional inputs; their failure must not
                    // make the filter UI unavailable.
                    LogProviderFailure(
                        provider.Id,
                        "production provider",
                        exception);
                }
            }
        }

        private static void EnsureInitialized()
        {
            if (index == null)
            {
                index = DefinitionProductionIndex.Build();
            }
        }

        private static MapState GetState(Map map)
        {
            if (map == null)
            {
                return NoMapState;
            }

            if (States.TryGetValue(map, out MapState state))
            {
                return state;
            }

            state = new MapState();
            States.Add(map, state);
            return state;
        }

        private static int CurrentGameTick()
        {
            return Find.TickManager?.TicksGame ?? 0;
        }

        private static void LogProviderFailure(
            string providerId,
            string providerKind,
            Exception exception)
        {
            string safeId = string.IsNullOrWhiteSpace(providerId)
                ? "<unnamed>"
                : providerId;
            int key = StableHash("FilterSignals." + providerKind + "." + safeId);
            Log.ErrorOnce(
                "[Filter Signals] " + providerKind + " '" + safeId +
                "' failed and was ignored: " + exception,
                key);
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

        /// <summary>
        /// Keeps each map's snapshot and results together so invalidation and
        /// disposal cannot accidentally cross colony boundaries.
        /// </summary>
        private sealed class MapState
        {
            internal readonly BoundedLruCache<ThingDef, ClassificationResult>
                Results =
                    new BoundedLruCache<ThingDef, ClassificationResult>(
                        CacheBudgetBytes);
            internal MapCapabilitySnapshot Snapshot;
        }
    }
}
