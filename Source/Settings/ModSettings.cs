using Verse;

namespace TechSenseFilters.Settings
{
    public sealed class TechSenseFiltersSettings : ModSettings
    {
        public bool FeatureEnabled;

        public override void ExposeData()
        {
            Scribe_Values.Look(
                ref FeatureEnabled,
                "featureEnabled",
                false);
            base.ExposeData();
        }
    }
}
