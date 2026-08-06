using System.Collections.Generic;
using System.Linq;
using RimWorld;
using FilterSignals.Domain;
using FilterSignals.Settings;
using Verse;

namespace FilterSignals.Runtime
{
    /// <summary>
    /// Translates RimWorld recipe and colony state into the domain model while
    /// keeping engine queries out of classification policy.
    /// </summary>
    internal static class RecipeAssessmentFactory
    {
        internal static ProductionPathAssessment Evaluate(
            RecipeDef recipe,
            Map map,
            MapCapabilitySnapshot snapshot,
            DefinitionProductionIndex index)
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
                !FilterSignals.Bootstrap.FilterSignalsMod.Settings.ConsiderMaterialShortages ||
                MaterialsAvailable(recipe, map);
            IReadOnlyList<ResearchProjectDef> missingResearch =
                MissingResearch(recipe);
            string lockedReason =
                BuildLockedReason(missingResearch, researchUnlocked);
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

        internal static IReadOnlyList<ResearchProjectDef> MissingResearch(
            RecipeDef recipe)
        {
            var missing = new List<ResearchProjectDef>();
            if (recipe?.researchPrerequisite != null &&
                !recipe.researchPrerequisite.IsFinished)
            {
                missing.Add(recipe.researchPrerequisite);
            }

            if (recipe?.researchPrerequisites != null)
            {
                for (int index = 0;
                    index < recipe.researchPrerequisites.Count;
                    index++)
                {
                    ResearchProjectDef project =
                        recipe.researchPrerequisites[index];
                    if (project != null &&
                        !project.IsFinished &&
                        !missing.Contains(project))
                    {
                        missing.Add(project);
                    }
                }
            }

            missing.Sort((left, right) =>
                string.CompareOrdinal(left.defName, right.defName));
            return missing;
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
            IReadOnlyList<ResearchProjectDef> missingResearch,
            bool availableNow)
        {
            if (availableNow)
            {
                return string.Empty;
            }

            if (missingResearch.Count > 0)
            {
                return "Requires " +
                    string.Join(
                        ", ",
                        missingResearch.Select(project => project.label)) +
                    " research.";
            }

            return "This colony has not met the production path's " +
                "prerequisites.";
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
                return "Research is complete, but this colony has no " +
                    pathLabel + ".";
            }

            if (!billGiverUsable)
            {
                return "Research is complete, but this colony has no usable " +
                    pathLabel + ".";
            }

            if (!sourceUsable)
            {
                return "Research is complete, but this colony has no " +
                    "currently usable " + pathLabel +
                    " that accepts this recipe.";
            }

            if (!pawnCapable)
            {
                return "Research is complete, but no colonist on this map " +
                    "currently meets the recipe's work and skill " +
                    "requirements.";
            }

            if (!materialsAvailable)
            {
                return "The production path is available, but this colony " +
                    "currently lacks required materials.";
            }

            return string.Empty;
        }
    }
}
