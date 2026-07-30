using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Spine.Caching;
using TechSenseFilters.Compatibility;
using TechSenseFilters.Domain;
using TechSenseFilters.Settings;
using Verse;

namespace TechSenseFilters.Runtime
{
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
                state.Snapshot =
                    MapCapabilitySnapshot.Capture(map, index, gameTick);
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
                return classificationOverride;
            }

            var paths = new List<ProductionPathAssessment>();
            IReadOnlyList<RecipeDef> recipes = index.RecipesFor(item);
            for (int recipeIndex = 0;
                recipeIndex < recipes.Count;
                recipeIndex++)
            {
                paths.Add(EvaluateRecipe(
                    recipes[recipeIndex],
                    map,
                    snapshot));
            }

            AddCustomPaths(paths, item, map);
            return ProductionClassifier.Classify(paths);
        }

        private static ProductionPathAssessment EvaluateRecipe(
            RecipeDef recipe,
            Map map,
            MapCapabilitySnapshot snapshot)
        {
            bool researchUnlocked = recipe.AvailableNow;
            IReadOnlyList<ThingDef> sourceDefs = index.SourcesFor(recipe);
            ThingDef preferredSource =
                sourceDefs.Count > 0 ? sourceDefs[0] : null;
            string fallbackPathLabel = preferredSource?.label ??
                recipe.label ??
                recipe.defName;
            ProductionSourceSelection sourceSelection =
                snapshot.SelectSource(
                    recipe,
                    sourceDefs,
                    fallbackPathLabel);
            string pathLabel = sourceSelection.PathLabel;
            bool pawnCapable = snapshot.HasCapablePawn(recipe);
            bool materialsAvailable =
                !TechSenseFiltersSettings.Current.ConsiderMaterialShortages ||
                MaterialsAvailable(recipe, map);
            string lockedReason =
                BuildLockedReason(recipe, researchUnlocked);
            string unavailableReason = BuildUnavailableReason(
                researchUnlocked,
                sourceSelection.SourcePresent,
                sourceSelection.BillGiverUsable,
                sourceSelection.SourceUsable,
                pawnCapable,
                materialsAvailable,
                pathLabel);

            return new ProductionPathAssessment(
                pathLabel,
                researchUnlocked,
                sourceSelection.SourcePresent,
                sourceSelection.SourceUsable,
                pawnCapable,
                materialsAvailable,
                lockedReason,
                unavailableReason);
        }

        private static ClassificationResult EvaluateOverrides(
            ThingDef item,
            Map map)
        {
            IReadOnlyList<ITechSenseClassificationOverride> overrides =
                TechSenseApi.GetClassificationOverrides();
            for (int i = 0; i < overrides.Count; i++)
            {
                ITechSenseClassificationOverride itemOverride = overrides[i];
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
            IReadOnlyList<ITechSenseProductionProvider> providers =
                TechSenseApi.GetProductionProviders();
            for (int i = 0; i < providers.Count; i++)
            {
                ITechSenseProductionProvider provider = providers[i];
                try
                {
                    IEnumerable<ProductionPathAssessment> supplied =
                        provider.GetProductionPaths(
                            item,
                            map,
                            TechSenseFiltersSettings.Current
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
                    LogProviderFailure(
                        provider.Id,
                        "production provider",
                        exception);
                }
            }
        }

        private static bool MaterialsAvailable(RecipeDef recipe, Map map)
        {
            if (map == null ||
                recipe.ingredients == null ||
                recipe.ingredients.Count == 0)
            {
                return true;
            }

            return !recipe.PotentiallyMissingIngredients(null, map).Any();
        }

        private static string BuildLockedReason(
            RecipeDef recipe,
            bool availableNow)
        {
            if (availableNow)
            {
                return string.Empty;
            }

            var missingResearch = new List<ResearchProjectDef>();
            if (recipe.researchPrerequisite != null &&
                !recipe.researchPrerequisite.IsFinished)
            {
                missingResearch.Add(recipe.researchPrerequisite);
            }

            if (recipe.researchPrerequisites != null)
            {
                for (int i = 0;
                    i < recipe.researchPrerequisites.Count;
                    i++)
                {
                    ResearchProjectDef project =
                        recipe.researchPrerequisites[i];
                    if (project != null &&
                        !project.IsFinished &&
                        !missingResearch.Contains(project))
                    {
                        missingResearch.Add(project);
                    }
                }
            }

            if (missingResearch.Count > 0)
            {
                return "Requires " +
                    string.Join(
                        ", ",
                        missingResearch.Select(project => project.label)) +
                    " research.";
            }

            return "The production path is locked by current colony prerequisites.";
        }

        private static string BuildUnavailableReason(
            bool researchUnlocked,
            bool sourcePresent,
            bool billGiverUsable,
            bool sourceUsable,
            bool pawnCapable,
            bool materialsAvailable,
            string pathLabel)
        {
            if (!researchUnlocked)
            {
                return string.Empty;
            }

            if (!sourcePresent)
            {
                return "Research is complete, but no " +
                    pathLabel + " exists.";
            }

            if (!billGiverUsable)
            {
                return "Research is complete, but no usable " +
                    pathLabel + " exists.";
            }

            if (!sourceUsable)
            {
                return "Research is complete, but no currently usable " +
                    pathLabel + " accepts this recipe.";
            }

            if (!pawnCapable)
            {
                return "Research is complete, but no colonist currently " +
                    "meets the recipe's work and skill requirements.";
            }

            if (!materialsAvailable)
            {
                return "The production path is available, but required " +
                    "materials are currently missing.";
            }

            return string.Empty;
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
            int key = StableHash("TechSense." + providerKind + "." + safeId);
            Log.ErrorOnce(
                "[TechSense Filters] " + providerKind + " '" + safeId +
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
