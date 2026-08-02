using System.Collections.Generic;
using FilterSignals.Domain;
using Verse;

namespace FilterSignals.Compatibility
{
    /// <summary>
    /// Adds definition-driven production paths that vanilla RecipeDefs cannot express.
    /// Providers must be deterministic for a given map revision and must not mutate game state.
    /// </summary>
    public interface IFilterSignalsProductionProvider
    {
        string Id { get; }

        IEnumerable<ProductionPathAssessment> GetProductionPaths(
            ThingDef item,
            Map map,
            bool considerMaterialShortages);
    }
}
