using UnityEngine;
using Verse;
using TechSenseFilters.Compatibility;
using TechSenseFilters.Patches;
using TechSenseFilters.Settings;

namespace TechSenseFilters.Bootstrap
{
    public sealed class TechSenseFiltersMod : Mod
    {
        private readonly TechSenseFiltersSettings settings;

        public TechSenseFiltersMod(ModContentPack content)
            : base(content)
        {
            settings = GetSettings<TechSenseFiltersSettings>();
            CompatibilityRegistry.InitializeAll();
            PatchInstaller.InstallAll();
        }

        public override string SettingsCategory()
        {
            return "TechSense Filters";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            var listing = new Listing_Standard();
            listing.Begin(inRect);
            listing.CheckboxLabeled(
                "Feature enabled",
                ref settings.FeatureEnabled);
            listing.End();
        }
    }
}
