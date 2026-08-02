using Spine.Api;
using Verse;

namespace FilterSignals.Settings
{
    public sealed class FilterSignalsSettings : ModSettings
    {
        public bool FeatureEnabled = true;
        public bool ShowClassificationToolbar;
        public bool ShowStatusIndicators = true;
        public bool ConsiderMaterialShortages;

        public override void ExposeData()
        {
            SpineApi.Settings.Scribe(
                this,
                FilterSignalsSettingsRegistry.Definitions);
            base.ExposeData();
        }
    }
}
