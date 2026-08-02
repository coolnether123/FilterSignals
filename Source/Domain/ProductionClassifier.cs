using System;
using System.Collections.Generic;
using System.Linq;

namespace FilterSignals.Domain
{
    public static class ProductionClassifier
    {
        public static ClassificationResult Classify(
            IEnumerable<ProductionPathAssessment> paths,
            ClassificationResult classificationOverride = null)
        {
            if (classificationOverride != null)
            {
                return classificationOverride;
            }

            ProductionPathAssessment[] candidates =
                paths?.Where(path => path != null).ToArray() ??
                Array.Empty<ProductionPathAssessment>();
            if (candidates.Length == 0)
            {
                return new ClassificationResult(
                    ProductionClassification.NotApplicable,
                    "This colony is unable to make this item.");
            }

            ProductionPathAssessment ready =
                candidates.FirstOrDefault(path => path.CanMakeNow);
            if (ready != null)
            {
                return new ClassificationResult(
                    ProductionClassification.CanMakeNow,
                    "Can be produced at " + ready.PathLabel + ".",
                    ready.PathLabel);
            }

            ProductionPathAssessment unlocked =
                candidates.FirstOrDefault(path => path.ResearchUnlocked);
            if (unlocked != null)
            {
                return new ClassificationResult(
                    ProductionClassification.ResearchUnlocked,
                    ExplainUnlockedButUnavailable(unlocked),
                    unlocked.PathLabel,
                    !unlocked.MaterialsAvailable
                        ? ClassificationReason.MaterialShortage
                        : ClassificationReason.General);
            }

            ProductionPathAssessment locked = candidates[0];
            string explanation = string.IsNullOrWhiteSpace(locked.LockedReason)
                ? "No currently unlocked production path exists."
                : locked.LockedReason;
            return new ClassificationResult(
                ProductionClassification.CannotMakeYet,
                explanation,
                locked.PathLabel);
        }

        private static string ExplainUnlockedButUnavailable(
            ProductionPathAssessment path)
        {
            if (!string.IsNullOrWhiteSpace(path.UnavailableReason))
            {
                return path.UnavailableReason;
            }

            if (!path.SourcePresent)
            {
                return "Research is complete, but no " +
                    path.PathLabel + " exists.";
            }

            if (!path.SourceUsable)
            {
                return "Research is complete, but no usable " +
                    path.PathLabel + " exists.";
            }

            if (!path.PawnCapable)
            {
                return "Research is complete, but no colonist currently " +
                    "meets this recipe's work and skill requirements.";
            }

            if (!path.MaterialsAvailable)
            {
                return "The production path is available, but required " +
                    "materials are currently missing.";
            }

            return "The production path is unlocked but is not currently usable.";
        }
    }
}
