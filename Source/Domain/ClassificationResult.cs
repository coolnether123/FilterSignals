using System;

namespace TechSenseFilters.Domain
{
    public enum ClassificationReason
    {
        General,
        MaterialShortage
    }

    public sealed class ClassificationResult
    {
        public ClassificationResult(
            ProductionClassification classification,
            string explanation,
            string pathLabel = null,
            ClassificationReason reason = ClassificationReason.General)
        {
            Classification = classification;
            Explanation = explanation ?? string.Empty;
            PathLabel = pathLabel ?? string.Empty;
            Reason = reason;
        }

        public ProductionClassification Classification { get; }

        public string Explanation { get; }

        public string PathLabel { get; }

        public ClassificationReason Reason { get; }
    }
}
