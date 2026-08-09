using System;

namespace FilterSignals.Domain
{
    /// <summary>
    /// Identifies policy-sensitive causes without making presentation parse
    /// the human-readable explanation.
    /// </summary>
    public enum ClassificationReason
    {
        General = 0,
        MaterialShortage = 1,
        ResearchRequired = 2,
        MissingProductionSource = 3,
        ProductionSourceUnavailable = 4,
        NoCapableColonist = 5
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
            : this(
                classification,
                explanation,
                pathLabel,
                reason,
                null)
        {
        }

        public ClassificationResult(
            ProductionClassification classification,
            string explanation,
            string pathLabel,
            ClassificationReason reason,
            string pathId)
        {
            Classification = classification;
            Explanation = explanation ?? string.Empty;
            PathLabel = pathLabel ?? string.Empty;
            Reason = reason;
            PathId = pathId ?? string.Empty;
            IsVanillaRecipePath = false;
        }

        internal ClassificationResult(
            ProductionClassification classification,
            string explanation,
            string pathLabel,
            ClassificationReason reason,
            string pathId,
            bool isVanillaRecipePath)
            : this(
                classification,
                explanation,
                pathLabel,
                reason,
                pathId)
        {
            IsVanillaRecipePath = isVanillaRecipePath;
        }

        public ProductionClassification Classification { get; }

        public string Explanation { get; }

        public string PathLabel { get; }

        public ClassificationReason Reason { get; }

        /// <summary>
        /// Stable domain identity of the winning production path. An empty
        /// value means the result came from an override or an extension that
        /// did not provide a navigable path identity.
        /// </summary>
        public string PathId { get; }

        internal bool IsVanillaRecipePath { get; private set; }
    }
}
