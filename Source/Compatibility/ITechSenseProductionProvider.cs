using System.Collections.Generic;
using TechSenseFilters.Domain;
using Verse;

namespace TechSenseFilters.Compatibility
{
    /// <summary>
    /// Adds definition-driven production paths that vanilla RecipeDefs cannot express.
    /// Providers must be deterministic for a given map revision and must not mutate game state.
    /// </summary>
    public interface ITechSenseProductionProvider
    {
        string Id { get; }

        IEnumerable<ProductionPathAssessment> GetProductionPaths(
            ThingDef item,
            Map map,
            bool considerMaterialShortages);
    }
}
