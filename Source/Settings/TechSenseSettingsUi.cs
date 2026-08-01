using Spine.UI.SettingsFramework;
using UnityEngine;
using Verse;

namespace TechSenseFilters.Settings
{
    internal sealed class TechSenseSettingsUi
    {
        private readonly SettingsListDrawer drawer =
            new SettingsListDrawer(TechSenseSettingsRegistry.Hierarchy)
            {
                SimpleLabel = "Simple",
                AdvancedLabel = "Advanced",
                NoResultsLabel = "No settings match",
                ResetToDefaultLabel = "Reset to default",
                GetLabel = definition =>
                    TranslateOrFallback(
                        definition.LabelKey,
                        definition.Label),
                GetTooltip = definition =>
                    TranslateOrFallback(
                        definition.TooltipKey,
                        definition.Tooltip),
                RowHeight = 34f
            };
        private SettingsViewMode viewMode = SettingsViewMode.Simple;

        internal SettingsListDrawer Drawer => drawer;

        internal void Draw(
            Rect rect,
            TechSenseFiltersSettings settings)
        {
            drawer.Draw(
                rect,
                settings,
                ref viewMode,
                settings.Write);
        }

        private static string TranslateOrFallback(
            string key,
            string fallback)
        {
            if (string.IsNullOrEmpty(key))
            {
                return fallback ?? string.Empty;
            }

            return key.Translate().ToString();
        }
    }
}
