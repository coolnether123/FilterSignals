using TechSenseFilters.Domain;
using UnityEngine;
using Verse;

namespace TechSenseFilters.Presentation
{
    internal static class ClassificationPresentation
    {
        internal static string ShortLabel(
            ProductionClassification classification)
        {
            switch (classification)
            {
                case ProductionClassification.CanMakeNow:
                    return "TechSense_CanMakeShort".Translate();
                case ProductionClassification.ResearchUnlocked:
                    return "TechSense_UnlockedShort".Translate();
                case ProductionClassification.CannotMakeYet:
                    return "TechSense_LockedShort".Translate();
                default:
                    return "TechSense_NotApplicableShort".Translate();
            }
        }

        internal static string FullLabel(
            ProductionClassification classification)
        {
            switch (classification)
            {
                case ProductionClassification.CanMakeNow:
                    return "TechSense_CanMake".Translate();
                case ProductionClassification.ResearchUnlocked:
                    return "TechSense_Unlocked".Translate();
                case ProductionClassification.CannotMakeYet:
                    return "TechSense_Locked".Translate();
                default:
                    return "TechSense_NotApplicable".Translate();
            }
        }

        internal static Color ColorFor(
            ProductionClassification classification)
        {
            switch (classification)
            {
                case ProductionClassification.CanMakeNow:
                    return new Color(0.34f, 0.86f, 0.47f);
                case ProductionClassification.ResearchUnlocked:
                    return new Color(0.98f, 0.74f, 0.22f);
                case ProductionClassification.CannotMakeYet:
                    return new Color(0.91f, 0.34f, 0.32f);
                default:
                    return new Color(0.58f, 0.66f, 0.76f);
            }
        }

        internal static string BriefExplanation(
            ClassificationResult result,
            ProductionNavigationDecision decision)
        {
            if (result == null)
            {
                return string.Empty;
            }

            switch (result.Classification)
            {
                case ProductionClassification.CanMakeNow:
                    return string.Empty;
                case ProductionClassification.ResearchUnlocked:
                    if (decision.Kind ==
                        ProductionNavigationKind.SelectBuildOption)
                    {
                        return "Workbench missing.";
                    }

                    if (result.Explanation.IndexOf(
                        "material",
                        System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return "Missing materials.";
                    }

                    if (result.Explanation.IndexOf(
                        "colonist",
                        System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return "No capable colonist.";
                    }

                    return string.IsNullOrWhiteSpace(result.PathLabel)
                        ? "Not currently usable."
                        : "Needs " + result.PathLabel + ".";
                case ProductionClassification.CannotMakeYet:
                    return "Research required.";
                default:
                    return "This colony is unable to make it.";
            }
        }

        internal static string NavigationTooltip(
            ProductionNavigationDecision decision)
        {
            switch (decision.Kind)
            {
                case ProductionNavigationKind.SelectProductionSource:
                    return "Click to select the workbench.";
                case ProductionNavigationKind.OpenResearch:
                    return "Click to open research.";
                case ProductionNavigationKind.SelectBuildOption:
                    return "Click to open Architect.";
                default:
                    return string.Empty;
            }
        }
    }
}
