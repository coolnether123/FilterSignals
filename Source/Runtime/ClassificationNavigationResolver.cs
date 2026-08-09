using System.Collections.Generic;
using System.Linq;
using RimWorld;
using FilterSignals.Domain;
using Verse;

namespace FilterSignals.Runtime
{
    internal sealed class ClassificationNavigationTarget
    {
        internal ClassificationNavigationTarget(
            ProductionNavigationDecision decision,
            Map map,
            Building productionSource,
            ResearchProjectDef research,
            BuildableDef buildable,
            Designator_Build buildDesignator)
        {
            Decision = decision;
            Map = map;
            ProductionSource = productionSource;
            Research = research;
            Buildable = buildable;
            BuildDesignator = buildDesignator;
        }

        internal ProductionNavigationDecision Decision { get; }
        internal Map Map { get; }
        internal Building ProductionSource { get; }
        internal ResearchProjectDef Research { get; }
        internal BuildableDef Buildable { get; }
        internal Designator_Build BuildDesignator { get; }
        internal bool IsActionable
        {
            get
            {
                if (!Decision.IsActionable)
                {
                    return false;
                }

                switch (Decision.Kind)
                {
                    case ProductionNavigationKind.SelectProductionSource:
                        return ProductionSource != null &&
                            ProductionSource.Spawned &&
                            ProductionSource.Map != null &&
                            Map != null &&
                            Find.CurrentMap == Map &&
                            ProductionSource.Map == Map;
                    case ProductionNavigationKind.OpenResearch:
                        return Research != null && !Research.IsFinished;
                    case ProductionNavigationKind.SelectBuildOption:
                        return Buildable != null &&
                            BuildDesignator != null &&
                            BuildDesignator.Visible &&
                            BuildDesignator.PlacingDef == Buildable &&
                            Map != null &&
                            Find.CurrentMap == Map;
                    default:
                        return false;
                }
            }
        }

        internal static ClassificationNavigationTarget None =>
            new ClassificationNavigationTarget(
                ProductionNavigationDecision.None,
                null,
                null,
                null,
                null,
                null);
    }

    internal static class ClassificationNavigationResolver
    {
        internal static ClassificationNavigationTarget Resolve(
            ThingDef item,
            Map map,
            ClassificationResult result)
        {
            if (item == null ||
                map == null ||
                result == null ||
                result.Classification ==
                    ProductionClassification.NotApplicable)
            {
                return ClassificationNavigationTarget.None;
            }

            try
            {
                return ResolveCore(item, map, result);
            }
            catch (System.Exception exception)
            {
                ClassificationDiagnostics.LogFailure(
                    "navigation resolution",
                    ClassificationDiagnostics.SafeId(() => item.defName),
                    "the indicator was treated as non-actionable",
                    exception);
                return ClassificationNavigationTarget.None;
            }
        }

        private static ClassificationNavigationTarget ResolveCore(
            ThingDef item,
            Map map,
            ClassificationResult result)
        {

            DefinitionProductionIndex index =
                ClassificationService.ProductionIndex;
            MapCapabilitySnapshot snapshot =
                MapCapabilitySnapshot.Capture(
                    map,
                    index,
                    Find.TickManager?.TicksGame ?? 0);
            IReadOnlyList<RecipeDef> recipes = index.RecipesFor(item);
            var candidates = new List<ProductionNavigationCandidate>();
            var productionSources = new Dictionary<string, Building>();
            var researchTargets =
                new Dictionary<string, ResearchProjectDef>();
            var buildTargets = new Dictionary<string, BuildableDef>();
            var buildDesignators =
                new Dictionary<string, Designator_Build>();

            for (int recipeIndex = 0;
                recipeIndex < recipes.Count;
                recipeIndex++)
            {
                RecipeDef recipe = recipes[recipeIndex];
                ProductionPathAssessment assessment =
                    RecipeAssessmentFactory.Evaluate(
                        recipe,
                        map,
                        snapshot,
                        index);
                IReadOnlyList<ThingDef> sourceDefs =
                    index.SourcesFor(recipe);
                Building usableSource = assessment.CanMakeNow &&
                    Find.CurrentMap == map
                    ? snapshot.FindUsableSource(recipe, sourceDefs)
                    : null;
                string productionSourceId = usableSource == null
                    ? string.Empty
                    : "source:" + usableSource.thingIDNumber;
                if (usableSource != null)
                {
                    productionSources[productionSourceId] = usableSource;
                }

                ResearchProjectDef research = null;
                BuildableDef buildable = null;
                Designator_Build buildDesignator = null;
                if (!assessment.ResearchUnlocked)
                {
                    IReadOnlyList<ResearchProjectDef> missing =
                        RecipeAssessmentFactory.MissingResearch(recipe);
                    research = missing.Count > 0 ? missing[0] : null;
                }
                else if (!assessment.SourcePresent)
                {
                    BuildRequirement requirement =
                        ResolveBuildRequirement(sourceDefs, map);
                    research = requirement.Research;
                    buildable = requirement.Buildable;
                    buildDesignator = requirement.BuildDesignator;
                }

                string researchId = research == null
                    ? string.Empty
                    : "research:" + research.defName;
                string buildId = buildable == null
                    ? string.Empty
                    : "build:" + buildable.defName;
                if (research != null)
                {
                    researchTargets[researchId] = research;
                }

                if (buildable != null)
                {
                    buildTargets[buildId] = buildable;
                    if (buildDesignator != null)
                    {
                        buildDesignators[buildId] = buildDesignator;
                    }
                }

                candidates.Add(new ProductionNavigationCandidate(
                    recipe.defName,
                    assessment.PathLabel,
                    assessment.CanMakeNow,
                    assessment.ResearchUnlocked,
                    assessment.SourcePresent,
                    productionSourceId,
                    researchId,
                    buildId,
                    assessment.Reason));
            }

            ProductionNavigationDecision decision =
                ProductionNavigationPolicy.Decide(
                    result,
                    candidates);
            if (!decision.IsActionable)
            {
                return ClassificationNavigationTarget.None;
            }

            productionSources.TryGetValue(
                decision.TargetId,
                out Building selectedSource);
            researchTargets.TryGetValue(
                decision.TargetId,
                out ResearchProjectDef selectedResearch);
            buildTargets.TryGetValue(
                decision.TargetId,
                out BuildableDef selectedBuildable);
            buildDesignators.TryGetValue(
                decision.TargetId,
                out Designator_Build selectedDesignator);
            return new ClassificationNavigationTarget(
                decision,
                map,
                selectedSource,
                selectedResearch,
                selectedBuildable,
                selectedDesignator);
        }

        private static BuildRequirement ResolveBuildRequirement(
            IReadOnlyList<ThingDef> sourceDefs,
            Map map)
        {
            if (sourceDefs == null)
            {
                return BuildRequirement.None;
            }

            var visited = new HashSet<ThingDef>();
            for (int index = 0; index < sourceDefs.Count; index++)
            {
                BuildRequirement requirement =
                    ResolveBuildRequirement(
                        sourceDefs[index],
                        map,
                        visited);
                if (requirement.IsActionable)
                {
                    return requirement;
                }
            }

            return BuildRequirement.None;
        }

        private static BuildRequirement ResolveBuildRequirement(
            ThingDef buildable,
            Map map,
            ISet<ThingDef> visited)
        {
            if (buildable == null ||
                map == null ||
                !visited.Add(buildable))
            {
                return BuildRequirement.None;
            }

            ResearchProjectDef missingResearch =
                MissingBuildResearch(buildable);
            if (missingResearch != null)
            {
                return BuildRequirement.ForResearch(missingResearch);
            }

            if (buildable.buildingPrerequisites != null)
            {
                ThingDef[] missingPrerequisites =
                    buildable.buildingPrerequisites
                        .Where(prerequisite =>
                            prerequisite != null &&
                            !map.listerBuildings
                                .ColonistsHaveBuilding(prerequisite))
                        .OrderBy(
                            prerequisite => prerequisite.defName,
                            System.StringComparer.Ordinal)
                        .ToArray();
                for (int index = 0;
                    index < missingPrerequisites.Length;
                    index++)
                {
                    BuildRequirement prerequisite =
                        ResolveBuildRequirement(
                            missingPrerequisites[index],
                            map,
                            visited);
                    if (prerequisite.IsActionable)
                    {
                        return prerequisite;
                    }
                }
            }

            if (!buildable.BuildableByPlayer ||
                Find.CurrentMap != map ||
                Find.MainTabsRoot == null ||
                MainButtonDefOf.Architect == null)
            {
                return BuildRequirement.None;
            }

            Designator_Build designator = FindBuildDesignator(buildable);
            return designator == null
                ? BuildRequirement.None
                : BuildRequirement.ForBuildable(buildable, designator);
        }

        private static Designator_Build FindBuildDesignator(
            BuildableDef target)
        {
            DesignationCategoryDef category =
                target?.designationCategory;
            if (category == null || !category.Visible)
            {
                return null;
            }

            var matches = new List<Designator_Build>();
            foreach (Designator designator in
                category.AllResolvedAndIdeoDesignators)
            {
                CollectBuildDesignators(
                    designator,
                    target,
                    matches);
            }

            return SelectValidatedBuildDesignator(matches);
        }

        private static Designator_Build SelectValidatedBuildDesignator(
            IReadOnlyList<Designator_Build> matches)
        {
            // A custom Designator_Build can expose the same PlacingDef while
            // changing selection semantics. Require the exact RimWorld 1.6
            // runtime type and its Assembly-CSharp identity before sharing it
            // between the tooltip and click path.
            if (matches == null || matches.Count != 1)
            {
                return null;
            }

            Designator_Build candidate = matches[0];
            System.Type runtimeType = candidate?.GetType();
            return runtimeType == typeof(Designator_Build) &&
                runtimeType.Assembly == typeof(Designator_Build).Assembly
                ? candidate
                : null;
        }

        private static void CollectBuildDesignators(
            Designator designator,
            BuildableDef target,
            ICollection<Designator_Build> matches)
        {
            if (designator == null || !designator.Visible)
            {
                return;
            }

            if (designator is Designator_Build build &&
                build.PlacingDef == target)
            {
                matches.Add(build);
            }

            if (designator is Designator_Dropdown dropdown)
            {
                IReadOnlyList<Designator> elements = dropdown.Elements;
                for (int index = 0; index < elements.Count; index++)
                {
                    CollectBuildDesignators(
                        elements[index],
                        target,
                        matches);
                }
            }
        }

        private static ResearchProjectDef MissingBuildResearch(
            BuildableDef buildable)
        {
            var missing = new List<ResearchProjectDef>();
            AddMissingResearch(
                missing,
                buildable?.researchPrerequisites);
            AddMissingResearch(
                missing,
                buildable?.designationCategory?.researchPrerequisites);
            return missing
                .OrderBy(
                    project => project.defName,
                    System.StringComparer.Ordinal)
                .FirstOrDefault();
        }

        private static void AddMissingResearch(
            ICollection<ResearchProjectDef> destination,
            IEnumerable<ResearchProjectDef> projects)
        {
            if (projects == null)
            {
                return;
            }

            foreach (ResearchProjectDef project in projects)
            {
                if (project != null &&
                    !project.IsFinished &&
                    !destination.Contains(project))
                {
                    destination.Add(project);
                }
            }
        }

        private readonly struct BuildRequirement
        {
            private BuildRequirement(
                ResearchProjectDef research,
                BuildableDef buildable,
                Designator_Build buildDesignator)
            {
                Research = research;
                Buildable = buildable;
                BuildDesignator = buildDesignator;
            }

            internal ResearchProjectDef Research { get; }
            internal BuildableDef Buildable { get; }
            internal Designator_Build BuildDesignator { get; }
            internal bool IsActionable =>
                Research != null || Buildable != null;

            internal static BuildRequirement None =>
                new BuildRequirement(null, null, null);

            internal static BuildRequirement ForResearch(
                ResearchProjectDef research)
            {
                return new BuildRequirement(research, null, null);
            }

            internal static BuildRequirement ForBuildable(
                BuildableDef buildable,
                Designator_Build designator)
            {
                return new BuildRequirement(null, buildable, designator);
            }
        }
    }
}
