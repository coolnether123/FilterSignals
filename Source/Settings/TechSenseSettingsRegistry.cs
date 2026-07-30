using System.Collections.Generic;
using Spine.UI.SettingsFramework;
using TechSenseFilters.Runtime;

namespace TechSenseFilters.Settings
{
    internal static class TechSenseSettingsRegistry
    {
        internal static readonly IReadOnlyList<SettingDefinition> Definitions =
            new[]
            {
                Toggle(
                    "feature.enabled",
                    nameof(TechSenseFiltersSettings.FeatureEnabled),
                    "Enable TechSense Filters",
                    "TechSense_Settings_Feature_Label",
                    "Adds transient production classifications to vanilla " +
                    "ThingFilter dialogs. Disabling this leaves every stored " +
                    "ThingFilter unchanged.",
                    "TechSense_Settings_Feature_Tooltip",
                    true,
                    0,
                    null,
                    true),
                Toggle(
                    "presentation.toolbar",
                    nameof(TechSenseFiltersSettings.ShowClassificationToolbar),
                    "Show classification toolbar",
                    "TechSense_Settings_Toolbar_Label",
                    "Shows four temporary view toggles above vanilla item " +
                    "filters. These toggles never change allowed items.",
                    "TechSense_Settings_Toolbar_Tooltip",
                    true,
                    10,
                    "feature.enabled",
                    true),
                Toggle(
                    "presentation.indicators",
                    nameof(TechSenseFiltersSettings.ShowStatusIndicators),
                    "Show status indicators",
                    "TechSense_Settings_Indicators_Label",
                    "Draws a compact colored indicator beside each visible " +
                    "item. Hover it for the production-path explanation.",
                    "TechSense_Settings_Indicators_Tooltip",
                    true,
                    20,
                    "feature.enabled",
                    true),
                Toggle(
                    "classification.materials",
                    nameof(TechSenseFiltersSettings.ConsiderMaterialShortages),
                    "Consider current material shortages",
                    "TechSense_Settings_Materials_Label",
                    "When enabled, a technologically available recipe is " +
                    "shown as unavailable if none of an ingredient's allowed " +
                    "materials are present. Disabled by default because " +
                    "inventory changes are temporary.",
                    "TechSense_Settings_Materials_Tooltip",
                    false,
                    30,
                    "feature.enabled",
                    false)
            };

        internal static readonly SettingsHierarchy Hierarchy =
            new SettingsHierarchy(Definitions);

        private static SettingDefinition Toggle(
            string id,
            string fieldName,
            string label,
            string labelKey,
            string tooltip,
            string tooltipKey,
            bool defaultValue,
            int sortOrder,
            string parentId,
            bool simple)
        {
            return new SettingDefinition
            {
                Id = id,
                FieldName = fieldName,
                ScribeKey = id,
                Label = label,
                LabelKey = labelKey,
                Tooltip = tooltip,
                TooltipKey = tooltipKey,
                Type = SettingType.Bool,
                DefaultValue = defaultValue,
                SortOrder = sortOrder,
                ParentId = parentId,
                ShowInSimpleView = simple,
                ShowInAdvancedView = true,
                ControlsChildVisibility = id == "feature.enabled",
                OnChanged = _ => ClassificationService.InvalidateAll()
            };
        }
    }
}
