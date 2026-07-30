using System.Linq;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;

namespace TechSenseFilters.Diagnostics
{
    internal static class TechSenseDebugActions
    {
        [DebugAction(
            "TechSense Filters",
            "Open TechSense filter fixture",
            actionType = DebugActionType.Action)]
        private static void OpenFixture()
        {
            Dialog_TechSenseFixture fixture =
                new Dialog_TechSenseFixture();
            Find.WindowStack.Add(fixture);
        }

        [DebugAction(
            "TechSense Filters",
            "Log TechSense fixture state",
            actionType = DebugActionType.Action)]
        private static void LogFixtureState()
        {
            Dialog_TechSenseFixture fixture =
                Find.WindowStack.WindowOfType<Dialog_TechSenseFixture>();
            if (fixture == null)
            {
                Log.Warning(
                    "[TechSense Filters] fixtureState=open:false");
                return;
            }

            Log.Message(
                "[TechSense Filters] fixtureState=open:true " +
                "filterUnchanged:" + fixture.FilterUnchanged.ToString()
                    .ToLowerInvariant() + " " +
                "allowedCount:" + fixture.AllowedCount);
        }
    }

    internal sealed class Dialog_TechSenseFixture : Window
    {
        private readonly ThingFilter filter =
            ThingFilter.CreateOnlyEverStorableThingFilter();
        private readonly ThingFilterUI.UIState uiState =
            new ThingFilterUI.UIState();
        private readonly int initialFingerprint;

        internal Dialog_TechSenseFixture()
        {
            initialFingerprint = Fingerprint(filter);
            doCloseX = true;
            draggable = true;
            resizeable = true;
            absorbInputAroundWindow = true;
        }

        public override Vector2 InitialSize =>
            new Vector2(780f, 720f);

        internal bool FilterUnchanged =>
            initialFingerprint == Fingerprint(filter);

        internal int AllowedCount =>
            filter.AllowedThingDefs.Count();

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(
                new Rect(inRect.x, inRect.y, inRect.width, 30f),
                "TechSense Filters verification fixture");
            Text.Font = GameFont.Small;
            string status = FilterUnchanged
                ? "Permanent filter state: unchanged"
                : "Permanent filter state: changed through vanilla checkboxes";
            Widgets.Label(
                new Rect(inRect.x, inRect.y + 31f, inRect.width, 24f),
                status + "  |  Allowed definitions: " + AllowedCount);

            Rect filterRect = new Rect(
                inRect.x,
                inRect.y + 58f,
                inRect.width,
                inRect.height - 58f);
            ThingFilterUI.DoThingFilterConfigWindow(
                filterRect,
                uiState,
                filter,
                map: Find.CurrentMap);
        }

        private static int Fingerprint(ThingFilter thingFilter)
        {
            unchecked
            {
                int hash = 17;
                foreach (ThingDef thingDef in
                    thingFilter.AllowedThingDefs
                        .OrderBy(definition => definition.shortHash))
                {
                    hash = (hash * 31) + thingDef.shortHash;
                }

                return hash;
            }
        }
    }
}
