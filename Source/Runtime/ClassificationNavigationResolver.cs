using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TechSenseFilters.Domain;
using Verse;

namespace TechSenseFilters.Runtime
{
    internal sealed class ClassificationNavigationTarget
    {
        internal ClassificationNavigationTarget(
            ProductionNavigationDecision decision,
            Map map,
            Building productionSource,
            ResearchProjectDef research,
            BuildableDef buildable)
        {
            Decision = decision;
            Map = map;
            ProductionSource = productionSource;
            Research = research;
            Buildable = buildable;
        }

        internal ProductionNavigationDecision Decision { get; }
        internal Map Map { get; }
        internal Building ProductionSource { get; }
        internal ResearchProjectDef Research { get; }
        internal BuildableDef Buildable { get; }
        internal bool IsActionable => Decision.IsActionable;

        internal static ClassificationNavigationTarget None =>
            new ClassificationNavigationTarget(
                ProductionNavigationDecision.None,
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
                Building usableSource = assessment.CanMakeNow
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
                }

                candidates.Add(new ProductionNavigationCandidate(
                    recipe.defName,
                    assessment.PathLabel,
                    assessment.CanMakeNow,
                    assessment.ResearchUnlocked,
                    assessment.SourcePresent,
                    productionSourceId,
                    researchId,
                    buildId));
            }

            ProductionNavigationDecision decision =
                ProductionNavigationPolicy.Decide(
                    result.Classification,
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
            return new ClassificationNavigationTarget(
                decision,
                map,
                selectedSource,
                selectedResearch,
                selectedBuildable);
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

            return buildable.BuildableByPlayer
                ? BuildRequirement.ForBuildable(buildable)
                : BuildRequirement.None;
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
                BuildableDef buildable)
            {
                Research = research;
                Buildable = buildable;
            }

            internal ResearchProjectDef Research { get; }
            internal BuildableDef Buildable { get; }
            internal bool IsActionable =>
                Research != null || Buildable != null;

            internal static BuildRequirement None =>
                new BuildRequirement(null, null);

            internal static BuildRequirement ForResearch(
                ResearchProjectDef research)
            {
                return new BuildRequirement(research, null);
            }

            internal static BuildRequirement ForBuildable(
                BuildableDef buildable)
            {
                return new BuildRequirement(null, buildable);
            }
        }
    }
}
