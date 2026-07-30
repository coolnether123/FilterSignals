using System.Collections.Generic;

namespace TechSenseFilters.Domain
{
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
