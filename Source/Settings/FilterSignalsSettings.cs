using Spine.UI.SettingsFramework;
using Verse;

namespace FilterSignals.Settings
{
    public sealed class FilterSignalsSettings : ModSettings
    {
        private static FilterSignalsSettings current =
            new FilterSignalsSettings();

        public bool FeatureEnabled = true;
        public bool ShowClassificationToolbar;
        public bool ShowStatusIndicators = true;
        public bool ConsiderMaterialShortages;

        internal static FilterSignalsSettings Current => current;

        internal static void Bind(FilterSignalsSettings settings)
        {
            current = settings ?? new FilterSignalsSettings();
        }

        public override void ExposeData()
        {
            SettingsScribe.ScribeAll(
                this,
                FilterSignalsSettingsRegistry.Definitions);
            base.ExposeData();
        }
    }
}
