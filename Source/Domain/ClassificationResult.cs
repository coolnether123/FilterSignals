using System;

namespace TechSenseFilters.Domain
{
    public sealed class ClassificationResult
    {
        public ClassificationResult(
            ProductionClassification classification,
            string explanation,
            string pathLabel = null)
        {
            Classification = classification;
            Explanation = explanation ?? string.Empty;
            PathLabel = pathLabel ?? string.Empty;
        }

        public ProductionClassification Classification { get; }

        public string Explanation { get; }

        public string PathLabel { get; }
    }
}
