using System.Reflection;
using Spine.Api;
using Spine.Harmony;
using Spine.UI.SettingsFramework;
using FilterSignals.Runtime;
using FilterSignals.Settings;
using Verse;

namespace FilterSignals.Bootstrap
{
    public sealed class FilterSignalsMod : SpineMod<FilterSignalsSettings>
    {
        private static System.IDisposable tooltipSizingLease;
        private static readonly IHarmonyPatchInstaller PatchInstaller =
            SpineApi.Patching.CreateInstaller(
                "CoolNether123.FilterSignals",
                "[Filter Signals]");
        public FilterSignalsMod(ModContentPack content)
            : base(
                content,
                "CoolNether123.FilterSignals",
                new SemanticVersion(1, 0, 0),
                FilterSignalsSettingsRegistry.Definitions,
                SpineCapability.HarmonyPatching |
                SpineCapability.BoundedCaches |
                SpineCapability.TooltipSizing,
                new ModSettingsPageOptions { RowHeight = 34f })
        {
            if (tooltipSizingLease == null)
            {
                tooltipSizingLease = SpineApi.Tooltips.Acquire(
                    "CoolNether123.FilterSignals");
            }

            InstallPatches();
        }

        protected override string SettingsCategoryLabel =>
            "Filter Signals";

        private static void InstallPatches()
        {
            if (PatchInstaller.PatchAllOnce(
                Assembly.GetExecutingAssembly(),
                new HarmonyPatchOptions
                {
                    // The one value-returning target is
                    // Listing_TreeThingFilter.Visible(ThingDef). Its bool
                    // result is amended by a postfix; no struct payload is
                    // copied or rewritten.
                    AllowStructReturns = true
                }))
            {
                ClassificationService.InvalidateAll();
            }
        }
    }
}
