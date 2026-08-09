using System;
using System.Collections.Generic;
using System.Linq;

namespace FilterSignals.Domain
{
    /// <summary>
    /// Names the safe UI destinations a status square may advertise.
    /// </summary>
    internal enum ProductionNavigationKind
    {
        None,
        SelectProductionSource,
        OpenResearch,
        SelectBuildOption
    }

    /// <summary>
    /// Describes one recipe's possible destination using stable identifiers
    /// so selection policy remains independent of RimWorld objects.
    /// </summary>
    internal sealed class ProductionNavigationCandidate
    {
        internal ProductionNavigationCandidate(
            string pathId,
            string pathLabel,
            bool canMakeNow,
            bool researchUnlocked,
            bool sourcePresent,
            string productionSourceTargetId = null,
            string researchTargetId = null,
            string buildTargetId = null,
            ClassificationReason reason = ClassificationReason.General)
        {
            PathId = pathId ?? string.Empty;
            PathLabel = pathLabel ?? string.Empty;
            CanMakeNow = canMakeNow;
            ResearchUnlocked = researchUnlocked;
            SourcePresent = sourcePresent;
            ProductionSourceTargetId =
                productionSourceTargetId ?? string.Empty;
            ResearchTargetId = researchTargetId ?? string.Empty;
            BuildTargetId = buildTargetId ?? string.Empty;
            Reason = reason;
        }

        internal string PathId { get; }
        internal string PathLabel { get; }
        internal bool CanMakeNow { get; }
        internal bool ResearchUnlocked { get; }
        internal bool SourcePresent { get; }
        internal string ProductionSourceTargetId { get; }
        internal string ResearchTargetId { get; }
        internal string BuildTargetId { get; }
        internal ClassificationReason Reason { get; }
    }

    /// <summary>
    /// Carries the single deterministic action chosen for an indicator click.
    /// </summary>
    internal readonly struct ProductionNavigationDecision
    {
        internal ProductionNavigationDecision(
            ProductionNavigationKind kind,
            string targetId,
            string targetLabel,
            string pathId,
            int alternativeCount)
        {
            Kind = kind;
            TargetId = targetId ?? string.Empty;
            TargetLabel = targetLabel ?? string.Empty;
            PathId = pathId ?? string.Empty;
            AlternativeCount = alternativeCount;
        }

        internal ProductionNavigationKind Kind { get; }
        internal string TargetId { get; }
        internal string TargetLabel { get; }
        internal string PathId { get; }
        internal int AlternativeCount { get; }
        internal bool IsActionable =>
            Kind != ProductionNavigationKind.None &&
            TargetId.Length > 0;

        internal static ProductionNavigationDecision None =>
            new ProductionNavigationDecision(
                ProductionNavigationKind.None,
                string.Empty,
                string.Empty,
                string.Empty,
                0);
    }

    /// <summary>
    /// Reduces recipe candidates to one safe action without coupling domain
    /// policy to tabs, designators, maps, or cameras.
    /// </summary>
    internal static class ProductionNavigationPolicy
    {
        internal static ProductionNavigationDecision Decide(
            ClassificationResult result,
            IEnumerable<ProductionNavigationCandidate> candidates)
        {
            ProductionNavigationCandidate[] ordered =
                candidates?
                    .Where(candidate => candidate != null)
                    .OrderBy(
                        candidate => candidate.PathId,
                        StringComparer.Ordinal)
                    .ThenBy(
                        candidate => candidate.PathLabel,
                        StringComparer.Ordinal)
                    .ToArray() ??
                Array.Empty<ProductionNavigationCandidate>();

            // Stable ordering prevents an item's click destination from
            // changing with definition or provider enumeration order.

            if (result == null ||
                !result.IsVanillaRecipePath ||
                result.PathId.Length == 0)
            {
                return ProductionNavigationDecision.None;
            }

            ProductionNavigationCandidate[] winningPath =
                ordered.Where(candidate =>
                    string.Equals(
                        candidate.PathId,
                        result.PathId,
                        StringComparison.Ordinal) &&
                    candidate.Reason == result.Reason)
                .ToArray();
            if (winningPath.Length != 1)
            {
                // A duplicate or missing domain identity is ambiguous. The
                // displayed classification remains useful, but navigation
                // must not guess at a different production path.
                return ProductionNavigationDecision.None;
            }

            switch (result.Classification)
            {
                case ProductionClassification.CanMakeNow:
                    return Choose(
                        winningPath.Where(candidate =>
                            candidate.CanMakeNow &&
                            candidate.ProductionSourceTargetId.Length > 0),
                        ProductionNavigationKind.SelectProductionSource,
                        candidate =>
                            candidate.ProductionSourceTargetId);
                case ProductionClassification.CannotMakeYet:
                    return Choose(
                        winningPath.Where(candidate =>
                            !candidate.ResearchUnlocked &&
                            candidate.ResearchTargetId.Length > 0),
                        ProductionNavigationKind.OpenResearch,
                        candidate => candidate.ResearchTargetId);
                case ProductionClassification.ResearchUnlocked:
                    ProductionNavigationCandidate[] unlocked =
                        winningPath.Where(candidate =>
                            candidate.ResearchUnlocked &&
                            !candidate.SourcePresent &&
                            (candidate.ResearchTargetId.Length > 0 ||
                                candidate.BuildTargetId.Length > 0))
                        .ToArray();
                    if (unlocked.Length == 0)
                    {
                        return ProductionNavigationDecision.None;
                    }

                    ProductionNavigationCandidate first = unlocked[0];
                    bool needsResearch =
                        first.ResearchTargetId.Length > 0;
                    return new ProductionNavigationDecision(
                        needsResearch
                            ? ProductionNavigationKind.OpenResearch
                            : ProductionNavigationKind.SelectBuildOption,
                        needsResearch
                            ? first.ResearchTargetId
                            : first.BuildTargetId,
                        first.PathLabel,
                        first.PathId,
                        unlocked.Length);
                default:
                    return ProductionNavigationDecision.None;
            }
        }

        private static ProductionNavigationDecision Choose(
            IEnumerable<ProductionNavigationCandidate> candidates,
            ProductionNavigationKind kind,
            Func<ProductionNavigationCandidate, string> target)
        {
            ProductionNavigationCandidate[] actionable =
                candidates.ToArray();
            if (actionable.Length == 0)
            {
                return ProductionNavigationDecision.None;
            }

            ProductionNavigationCandidate selected = actionable[0];
            return new ProductionNavigationDecision(
                kind,
                target(selected),
                selected.PathLabel,
                selected.PathId,
                actionable.Length);
        }
    }
}
