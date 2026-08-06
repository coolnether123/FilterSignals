using System.Collections.Generic;

namespace FilterSignals.Domain
{
    /// <summary>
    /// Captures the capabilities of one real workstation instance for the
    /// engine-free source-selection policy.
    /// </summary>
    internal readonly struct ProductionSourceCandidate
    {
        internal ProductionSourceCandidate(
            string pathLabel,
            bool currentlyUsableForBills,
            bool recipeAvailableOnInstance)
        {
            PathLabel = pathLabel ?? string.Empty;
            CurrentlyUsableForBills = currentlyUsableForBills;
            RecipeAvailableOnInstance = recipeAvailableOnInstance;
        }

        internal string PathLabel { get; }

        internal bool CurrentlyUsableForBills { get; }

        internal bool RecipeAvailableOnInstance { get; }

        internal bool CanUseRecipe =>
            CurrentlyUsableForBills && RecipeAvailableOnInstance;
    }

    /// <summary>
    /// Preserves the strongest workstation evidence so the UI can distinguish
    /// absence, bill-giver failure, and recipe rejection.
    /// </summary>
    internal readonly struct ProductionSourceSelection
    {
        internal ProductionSourceSelection(
            string pathLabel,
            bool sourcePresent,
            bool billGiverUsable,
            bool sourceUsable)
        {
            PathLabel = pathLabel ?? string.Empty;
            SourcePresent = sourcePresent;
            BillGiverUsable = billGiverUsable;
            SourceUsable = sourceUsable;
        }

        internal string PathLabel { get; }

        internal bool SourcePresent { get; }

        internal bool BillGiverUsable { get; }

        internal bool SourceUsable { get; }
    }

    /// <summary>
    /// Ensures any usable instance can win instead of collapsing a building
    /// definition to a misleading colony-wide Boolean.
    /// </summary>
    internal static class ProductionSourceSelector
    {
        internal static ProductionSourceSelection Select(
            string fallbackPathLabel,
            IEnumerable<ProductionSourceCandidate> candidates)
        {
            string presentLabel = null;
            string billGiverUsableLabel = null;

            if (candidates != null)
            {
                foreach (ProductionSourceCandidate candidate in candidates)
                {
                    if (presentLabel == null)
                    {
                        presentLabel = candidate.PathLabel;
                    }

                    if (candidate.CurrentlyUsableForBills &&
                        billGiverUsableLabel == null)
                    {
                        billGiverUsableLabel = candidate.PathLabel;
                    }

                    if (candidate.CanUseRecipe)
                    {
                        return new ProductionSourceSelection(
                            candidate.PathLabel,
                            sourcePresent: true,
                            billGiverUsable: true,
                            sourceUsable: true);
                    }
                }
            }

            return new ProductionSourceSelection(
                billGiverUsableLabel ??
                    presentLabel ??
                    fallbackPathLabel,
                sourcePresent: presentLabel != null,
                billGiverUsable: billGiverUsableLabel != null,
                sourceUsable: false);
        }
    }
}
