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

        internal static string NavigationTooltip(
            ClassificationResult result)
        {
            if (result == null)
            {
                return "TechSense_NavigationUnavailable".Translate();
            }

            string instruction;
            switch (result.Classification)
            {
                case ProductionClassification.CanMakeNow:
                    instruction =
                        "TechSense_NavigateCanMake".Translate();
                    break;
                case ProductionClassification.ResearchUnlocked:
                    instruction =
                        "TechSense_NavigateUnlocked".Translate();
                    break;
                case ProductionClassification.CannotMakeYet:
                    instruction =
                        "TechSense_NavigateLocked".Translate();
                    break;
                default:
                    return "TechSense_NavigationUnavailable".Translate();
            }

            string path = string.IsNullOrWhiteSpace(result.PathLabel)
                ? string.Empty
                : "\n" +
                    "TechSense_SelectedPath".Translate() +
                    " " + result.PathLabel.CapitalizeFirst() + ".";
            return instruction +
                path +
                "\n" +
                "TechSense_NavigationChoiceRule".Translate();
        }
    }
}
