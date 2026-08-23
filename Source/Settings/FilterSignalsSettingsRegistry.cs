using Spine.UI.SettingsFramework;
using FilterSignals.Runtime;

namespace FilterSignals.Settings
{
    /// <summary>
    /// Declares the settings hierarchy once for Spine's UI, persistence, and
    /// contextual navigation facilities.
    /// </summary>
    internal static class FilterSignalsSettingsRegistry
    {
        internal static readonly SettingsSchema<FilterSignalsSettings> Schema =
            new SettingsSchema<FilterSignalsSettings>(
                scribeKeyConvention: null,
                onAdd: definition => definition.OnChanged =
                    _ => ClassificationService.InvalidateAll());

        static FilterSignalsSettingsRegistry()
        {
            Schema.Root.Toggle(
                "feature.enabled",
                settings => settings.FeatureEnabled,
                "Enable Filter Signals",
                tooltip: "Shows production status in vanilla item-filter " +
                    "dialogs. Turning it off does not change saved filters.")
                .ControlsChildren()
                .ScribeAs("feature.enabled")
                .Localized("FilterSignals_Settings_Feature_Label",
                    "FilterSignals_Settings_Feature_Tooltip");

            var featureChildren = Schema.Under("feature.enabled");
            featureChildren.Toggle(
                    "presentation.toolbar",
                    settings => settings.ShowClassificationToolbar,
                    "Show optional filter toolbar",
                    tooltip: "Shows four buttons above item filters. They hide " +
                        "or show status groups for the current dialog only. " +
                        "The colored status squares remain available when the " +
                        "toolbar is hidden.")
                .ScribeAs("presentation.toolbar")
                .Localized("FilterSignals_Settings_Toolbar_Label",
                    "FilterSignals_Settings_Toolbar_Tooltip");
            featureChildren.Toggle(
                    "presentation.indicators",
                    settings => settings.ShowStatusIndicators,
                    "Show status indicators",
                    tooltip: "Shows a colored status square beside each visible " +
                        "item. Hover over it to see why the item has that " +
                        "status.")
                .ScribeAs("presentation.indicators")
                .Localized("FilterSignals_Settings_Indicators_Label",
                    "FilterSignals_Settings_Indicators_Tooltip");
            featureChildren.Toggle(
                    "classification.materials",
                    settings => settings.ConsiderMaterialShortages,
                    "Consider current material shortages",
                    tooltip: "When enabled, a recipe counts as unavailable if " +
                        "the colony has none of an ingredient's allowed " +
                        "materials. This check is off by default because " +
                        "inventory changes often.")
                .AdvancedOnly()
                .ScribeAs("classification.materials")
                .Localized("FilterSignals_Settings_Materials_Label",
                    "FilterSignals_Settings_Materials_Tooltip");
        }
    }
}
