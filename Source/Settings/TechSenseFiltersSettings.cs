using Spine.UI.SettingsFramework;
using Verse;

namespace TechSenseFilters.Settings
{
    public sealed class TechSenseFiltersSettings : ModSettings
    {
        private static TechSenseFiltersSettings current =
            new TechSenseFiltersSettings();

        public bool FeatureEnabled = true;
        public bool ShowClassificationToolbar;
        public bool ShowStatusIndicators = true;
        public bool ConsiderMaterialShortages;

        internal static TechSenseFiltersSettings Current => current;

        internal static void Bind(TechSenseFiltersSettings settings)
        {
            current = settings ?? new TechSenseFiltersSettings();
        }

        public override void ExposeData()
        {
            SettingsScribe.ScribeAll(
                this,
                TechSenseSettingsRegistry.Definitions);
            base.ExposeData();
        }
    }
}
