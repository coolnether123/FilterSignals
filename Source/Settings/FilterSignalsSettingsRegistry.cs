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
                tooltip: "Adds transient production classifications to vanilla " +
                    "ThingFilter dialogs. Disabling this leaves every stored " +
                    "ThingFilter unchanged.")
                .ControlsChildren()
                .ScribeAs("feature.enabled")
                .Localized("FilterSignals_Settings_Feature_Label",
                    "FilterSignals_Settings_Feature_Tooltip");

            var featureChildren = Schema.Under("feature.enabled");
            featureChildren.Toggle(
                    "presentation.toolbar",
                    settings => settings.ShowClassificationToolbar,
                    "Show optional filter toolbar",
                    tooltip: "Shows four temporary classification toggles above " +
                        "vanilla item filters. Hidden by default because the " +
                        "colored status squares remain available.")
                .ScribeAs("presentation.toolbar")
                .Localized("FilterSignals_Settings_Toolbar_Label",
                    "FilterSignals_Settings_Toolbar_Tooltip");
            featureChildren.Toggle(
                    "presentation.indicators",
                    settings => settings.ShowStatusIndicators,
                    "Show status indicators",
                    tooltip: "Draws a compact colored indicator beside each " +
                        "visible item. Hover it for the production-path " +
                        "explanation.")
                .ScribeAs("presentation.indicators")
                .Localized("FilterSignals_Settings_Indicators_Label",
                    "FilterSignals_Settings_Indicators_Tooltip");
            featureChildren.Toggle(
                    "classification.materials",
                    settings => settings.ConsiderMaterialShortages,
                    "Consider current material shortages",
                    tooltip: "When enabled, a technologically available recipe is " +
                        "shown as unavailable if none of an ingredient's allowed " +
                        "materials are present. Disabled by default because " +
                        "inventory changes are temporary.")
                .AdvancedOnly()
                .ScribeAs("classification.materials")
                .Localized("FilterSignals_Settings_Materials_Label",
                    "FilterSignals_Settings_Materials_Tooltip");
        }
    }
}
