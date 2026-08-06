using System;

namespace FilterSignals.Domain
{
    /// <summary>
    /// Identifies policy-sensitive causes without making presentation parse
    /// the human-readable explanation.
    /// </summary>
    public enum ClassificationReason
    {
        General,
        MaterialShortage
    }

    /// <summary>
    /// Carries the winning colony-level answer across the runtime and UI
    /// boundary without exposing recipe or cache internals.
    /// </summary>
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
