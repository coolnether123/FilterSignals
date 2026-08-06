using Spine.Api;
using Verse;

namespace FilterSignals.Settings
{
    /// <summary>
    /// Stores the few durable player choices while dialog-specific filtering
    /// remains transient presentation state.
    /// </summary>
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
