using System;

namespace FilterSignals.Domain
{
    /// <summary>
    /// Represents one independently viable way to make an item so competing
    /// recipes and mod integrations can be reduced by pure policy.
    /// </summary>
    public sealed class ProductionPathAssessment
    {
        public ProductionPathAssessment(
            string pathLabel,
            bool researchUnlocked,
            bool sourcePresent,
            bool sourceUsable,
            bool pawnCapable,
            bool materialsAvailable,
            string lockedReason = null,
            string unavailableReason = null)
        {
            PathLabel = pathLabel ?? string.Empty;
            ResearchUnlocked = researchUnlocked;
            SourcePresent = sourcePresent;
            SourceUsable = sourceUsable;
            PawnCapable = pawnCapable;
            MaterialsAvailable = materialsAvailable;
            LockedReason = lockedReason ?? string.Empty;
            UnavailableReason = unavailableReason ?? string.Empty;
        }

        public string PathLabel { get; }

        public bool ResearchUnlocked { get; }

        public bool SourcePresent { get; }

        public bool SourceUsable { get; }

        public bool PawnCapable { get; }

        public bool MaterialsAvailable { get; }

        public string LockedReason { get; }

        public string UnavailableReason { get; }

        public bool CanMakeNow =>
            ResearchUnlocked &&
            SourcePresent &&
            SourceUsable &&
            PawnCapable &&
            MaterialsAvailable;
    }
}
