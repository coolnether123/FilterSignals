using System.Collections.Generic;
using Spine.UI.SettingsFramework;
using FilterSignals.Runtime;

namespace FilterSignals.Settings
{
    internal static class FilterSignalsSettingsRegistry
    {
        internal static readonly IReadOnlyList<SettingDefinition> Definitions =
            new[]
            {
                SettingDefinitions.Toggle(
                    "feature.enabled",
                    nameof(FilterSignalsSettings.FeatureEnabled),
                    "Enable Filter Signals",
                    "FilterSignals_Settings_Feature_Label",
                    "Adds transient production classifications to vanilla " +
                    "ThingFilter dialogs. Disabling this leaves every stored " +
                    "ThingFilter unchanged.",
                    "FilterSignals_Settings_Feature_Tooltip",
                    controlsChildren: true,
                    scribeKey: "feature.enabled",
                    onChanged: _ => ClassificationService.InvalidateAll()),
                SettingDefinitions.Toggle(
                    "presentation.toolbar",
                    nameof(FilterSignalsSettings.ShowClassificationToolbar),
                    "Show optional filter toolbar",
                    "FilterSignals_Settings_Toolbar_Label",
                    "Shows four temporary classification toggles above " +
                    "vanilla item filters. Hidden by default because the " +
                    "colored status squares remain available.",
                    "FilterSignals_Settings_Toolbar_Tooltip",
                    parentId: "feature.enabled",
                    scribeKey: "presentation.toolbar",
                    onChanged: _ => ClassificationService.InvalidateAll()),
                SettingDefinitions.Toggle(
                    "presentation.indicators",
                    nameof(FilterSignalsSettings.ShowStatusIndicators),
                    "Show status indicators",
                    "FilterSignals_Settings_Indicators_Label",
                    "Draws a compact colored indicator beside each visible " +
                    "item. Hover it for the production-path explanation.",
                    "FilterSignals_Settings_Indicators_Tooltip",
                    parentId: "feature.enabled",
                    scribeKey: "presentation.indicators",
                    onChanged: _ => ClassificationService.InvalidateAll()),
                SettingDefinitions.Toggle(
                    "classification.materials",
                    nameof(FilterSignalsSettings.ConsiderMaterialShortages),
                    "Consider current material shortages",
                    "FilterSignals_Settings_Materials_Label",
                    "When enabled, a technologically available recipe is " +
                    "shown as unavailable if none of an ingredient's allowed " +
                    "materials are present. Disabled by default because " +
                    "inventory changes are temporary.",
                    "FilterSignals_Settings_Materials_Tooltip",
                    parentId: "feature.enabled",
                    simple: false,
                    scribeKey: "classification.materials",
                    onChanged: _ => ClassificationService.InvalidateAll())
            };
    }
}
