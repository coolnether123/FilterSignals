using System.Reflection;
using HarmonyLib;
using Spine.Api;
using Spine.Harmony;
using Spine.UI.ContextualSettings;
using FilterSignals.Runtime;
using FilterSignals.Settings;
using UnityEngine;
using Verse;

namespace FilterSignals.Bootstrap
{
    public sealed class FilterSignalsMod : Mod
    {
        private static bool patchesInstalled;
        private static System.IDisposable tooltipSizingLease;
        private static IContextualSettingsLease contextualSettingsLease;

        private readonly FilterSignalsSettings settings;
        private readonly FilterSignalsSettingsUi settingsUi =
            new FilterSignalsSettingsUi();

        public FilterSignalsMod(ModContentPack content)
            : base(content)
        {
            SpineApi.Runtime.Require(new SpineRequirement(
                "CoolNether123.FilterSignals",
                new SemanticVersion(1, 1, 0),
                SpineCapability.Settings |
                SpineCapability.HarmonyPatching |
                SpineCapability.BoundedCaches |
                SpineCapability.TooltipSizing |
                SpineCapability.ContextualSettings));
            if (tooltipSizingLease == null)
            {
                tooltipSizingLease = SpineApi.Tooltips.Acquire(
                    "CoolNether123.FilterSignals");
            }

            settings = GetSettings<FilterSignalsSettings>();
            FilterSignalsSettings.Bind(settings);
            if (contextualSettingsLease == null)
            {
                contextualSettingsLease = SpineApi.ContextualSettings.Acquire(
                    "CoolNether123.FilterSignals",
                    this,
                    settingsUi.Drawer,
                    settings);
            }
            InstallPatches();
        }

        internal static IContextualSettingsLease ContextualSettings =>
            contextualSettingsLease;

        public override string SettingsCategory()
        {
            return "Filter Signals";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            settingsUi.Draw(inRect, settings);
        }

        private static void InstallPatches()
        {
            if (patchesInstalled)
            {
                return;
            }

            var harmony = new HarmonyLib.Harmony(
                "CoolNether123.FilterSignals");
            HarmonyUtil.PatchAll(
                harmony,
                Assembly.GetExecutingAssembly(),
                new HarmonyUtil.PatchOptions
                {
                    // The one value-returning target is
                    // Listing_TreeThingFilter.Visible(ThingDef). Its bool
                    // result is amended by a postfix; no struct payload is
                    // copied or rewritten.
                    AllowStructReturns = true,
                    OnResult = (target, result) =>
                    {
                        if (result.StartsWith("error:") ||
                            result.StartsWith("skipped:"))
                        {
                            Log.Warning(
                                "[Filter Signals] " + target + ": " +
                                result);
                        }
                        else if (Prefs.DevMode)
                        {
                            Log.Message(
                                "[Filter Signals] " + target + ": " +
                                result);
                        }
                    }
                });
            patchesInstalled = true;
            ClassificationService.InvalidateAll();
        }
    }
}
