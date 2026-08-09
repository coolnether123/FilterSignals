using FilterSignals.Domain;
using UnityEngine;
using Verse;

namespace FilterSignals.Presentation
{
    internal static class ClassificationPresentation
    {
        internal static string ShortLabel(
            ProductionClassification classification)
        {
            switch (classification)
            {
                case ProductionClassification.CanMakeNow:
                    return "FilterSignals_CanMakeShort".Translate();
                case ProductionClassification.ResearchUnlocked:
                    return "FilterSignals_UnlockedShort".Translate();
                case ProductionClassification.CannotMakeYet:
                    return "FilterSignals_LockedShort".Translate();
                default:
                    return "FilterSignals_NotApplicableShort".Translate();
            }
        }

        internal static string FullLabel(
            ProductionClassification classification)
        {
            switch (classification)
            {
                case ProductionClassification.CanMakeNow:
                    return "FilterSignals_CanMake".Translate();
                case ProductionClassification.ResearchUnlocked:
                    return "FilterSignals_Unlocked".Translate();
                case ProductionClassification.CannotMakeYet:
                    return "FilterSignals_Locked".Translate();
                default:
                    return "FilterSignals_NotApplicable".Translate();
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
                        return "FilterSignals_WorkbenchMissing".Translate();
                    }

                    switch (result.Reason)
                    {
                        case ClassificationReason.MaterialShortage:
                            return "FilterSignals_MissingMaterials".Translate();
                        case ClassificationReason.NoCapableColonist:
                            return "FilterSignals_NoCapableColonist".Translate();
                        case ClassificationReason.MissingProductionSource:
                            return string.IsNullOrWhiteSpace(result.PathLabel)
                                ? "FilterSignals_NotCurrentlyUsable".Translate()
                                : "FilterSignals_NeedsProductionSource".Translate(
                                    result.PathLabel);
                        case ClassificationReason.ProductionSourceUnavailable:
                            return "FilterSignals_NotCurrentlyUsable".Translate();
                        default:
                            return string.IsNullOrWhiteSpace(result.PathLabel)
                                ? "FilterSignals_NotCurrentlyUsable".Translate()
                                : "FilterSignals_NeedsProductionSource".Translate(
                                    result.PathLabel);
                    }
                case ProductionClassification.CannotMakeYet:
                    return "FilterSignals_ResearchRequired".Translate();
                default:
                    return "FilterSignals_UnableToMake".Translate();
            }
        }

        internal static string NavigationTooltip(
            ProductionNavigationDecision decision)
        {
            switch (decision.Kind)
            {
                case ProductionNavigationKind.SelectProductionSource:
                    return "FilterSignals_Navigation_SelectSource".Translate();
                case ProductionNavigationKind.OpenResearch:
                    return "FilterSignals_Navigation_OpenResearch".Translate();
                case ProductionNavigationKind.SelectBuildOption:
                    return "FilterSignals_Navigation_OpenArchitect".Translate();
                default:
                    return string.Empty;
            }
        }
    }
}
