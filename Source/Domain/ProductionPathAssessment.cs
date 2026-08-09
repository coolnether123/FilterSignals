using System;

namespace FilterSignals.Domain
{
    /// <summary>
    /// Represents one independently viable way to make an item so competing
    /// recipes and mod integrations can be reduced by pure policy.
    /// </summary>
    public sealed class ProductionPathAssessment
    {
        /// <summary>
        /// Retains the original public ABI and source call shape. Keep these
        /// eight CLR parameters stable for existing provider assemblies.
        /// </summary>
        public ProductionPathAssessment(
            string pathLabel,
            bool researchUnlocked,
            bool sourcePresent,
            bool sourceUsable,
            bool pawnCapable,
            bool materialsAvailable,
            string lockedReason = null,
            string unavailableReason = null)
            : this(
                pathLabel,
                researchUnlocked,
                sourcePresent,
                sourceUsable,
                pawnCapable,
                materialsAvailable,
                lockedReason,
                unavailableReason,
                ClassificationReason.General,
                null,
                false)
        {
        }

        /// <summary>
        /// Carries structured classification metadata without adding optional
        /// parameters that would compete with the legacy constructor.
        /// </summary>
        public ProductionPathAssessment(
            string pathLabel,
            bool researchUnlocked,
            bool sourcePresent,
            bool sourceUsable,
            bool pawnCapable,
            bool materialsAvailable,
            string lockedReason,
            string unavailableReason,
            ClassificationReason reason,
            string pathId)
            : this(
                pathLabel,
                researchUnlocked,
                sourcePresent,
                sourceUsable,
                pawnCapable,
                materialsAvailable,
                lockedReason,
                unavailableReason,
                reason,
                pathId,
                false)
        {
        }

        internal ProductionPathAssessment(
            string pathLabel,
            bool researchUnlocked,
            bool sourcePresent,
            bool sourceUsable,
            bool pawnCapable,
            bool materialsAvailable,
            string lockedReason,
            string unavailableReason,
            ClassificationReason reason,
            string pathId,
            bool isVanillaRecipePath)
        {
            PathLabel = pathLabel ?? string.Empty;
            ResearchUnlocked = researchUnlocked;
            SourcePresent = sourcePresent;
            SourceUsable = sourceUsable;
            PawnCapable = pawnCapable;
            MaterialsAvailable = materialsAvailable;
            LockedReason = lockedReason ?? string.Empty;
            UnavailableReason = unavailableReason ?? string.Empty;
            Reason = reason;
            PathId = pathId ?? string.Empty;
            IsVanillaRecipePath = isVanillaRecipePath;
        }

        public string PathLabel { get; }

        public bool ResearchUnlocked { get; }

        public bool SourcePresent { get; }

        public bool SourceUsable { get; }

        public bool PawnCapable { get; }

        public bool MaterialsAvailable { get; }

        public string LockedReason { get; }

        public string UnavailableReason { get; }

        public ClassificationReason Reason { get; }

        public string PathId { get; }

        internal bool IsVanillaRecipePath { get; private set; }

        public bool CanMakeNow =>
            ResearchUnlocked &&
            SourcePresent &&
            SourceUsable &&
            PawnCapable &&
            MaterialsAvailable;
    }
}
