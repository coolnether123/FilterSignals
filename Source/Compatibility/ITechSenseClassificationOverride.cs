using TechSenseFilters.Domain;
using Verse;

namespace TechSenseFilters.Compatibility
{
    /// <summary>
    /// Overrides the final classification for definitions with nonstandard semantics.
    /// Return false to leave the definition to vanilla recipes and other providers.
    /// </summary>
    public interface ITechSenseClassificationOverride
    {
        string Id { get; }

        bool TryClassify(
            ThingDef item,
            Map map,
            out ClassificationResult result);
    }
}
