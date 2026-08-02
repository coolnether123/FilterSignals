using System.Collections.Generic;
using FilterSignals.Domain;

namespace FilterSignals.Presentation
{
    internal sealed class FilterPresentationState
    {
        private readonly HashSet<ProductionClassification> enabled =
            new HashSet<ProductionClassification>
            {
                ProductionClassification.CanMakeNow,
                ProductionClassification.ResearchUnlocked,
                ProductionClassification.CannotMakeYet,
                ProductionClassification.NotApplicable
            };

        internal bool IsEnabled(
            ProductionClassification classification)
        {
            return enabled.Contains(classification);
        }

        internal void Toggle(
            ProductionClassification classification)
        {
            if (!enabled.Remove(classification))
            {
                enabled.Add(classification);
            }
        }
    }
}
