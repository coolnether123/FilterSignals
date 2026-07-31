using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using LudeonTK;
using RimWorld;
using TechSenseFilters.Domain;
using TechSenseFilters.Runtime;
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
            "Open small-volume tooltip fixture",
            actionType = DebugActionType.Action)]
        private static void OpenSmallVolumeTooltipFixture()
        {
            Dialog_TechSenseFixture fixture =
                new Dialog_TechSenseFixture("Gold");
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

        [DebugAction(
            "TechSense Filters",
            "Run TechSense capability probes",
            actionType = DebugActionType.Action)]
        private static void RunCapabilityProbes()
        {
            Map map = Find.CurrentMap;
            if (map == null)
            {
                Log.Warning(
                    "[TechSense Filters] capabilityProbes=failed noMap");
                return;
            }

            RunConditionalInstanceProbe(map);
            RunWorkstationInvalidationProbe(map);
            RunResearchInvalidationProbe(map);
            RunMultiPathDefinitionProbe(map);
        }

        [DebugAction(
            "TechSense Filters",
            "Log TechSense explanations",
            actionType = DebugActionType.Action)]
        private static void LogRepresentativeExplanations()
        {
            Map map = Find.CurrentMap;
            var remaining = new HashSet<ProductionClassification>(
                new[]
                {
                    ProductionClassification.CanMakeNow,
                    ProductionClassification.ResearchUnlocked,
                    ProductionClassification.CannotMakeYet,
                    ProductionClassification.NotApplicable
                });

            foreach (ThingDef item in
                ThingFilter.CreateOnlyEverStorableThingFilter()
                    .AllowedThingDefs
                    .OrderBy(definition => definition.defName))
            {
                ClassificationResult result =
                    ClassificationService.Get(item, map);
                if (!remaining.Remove(result.Classification))
                {
                    continue;
                }

                Log.Message(
                    "[TechSense Filters] explanation classification=" +
                    result.Classification + " item=" + item.defName +
                    " path=" + (result.PathLabel ?? string.Empty) +
                    " text=\"" + result.Explanation + "\"");
                if (remaining.Count == 0)
                {
                    break;
                }
            }

            Log.Message(
                "[TechSense Filters] explanationCoverage=" +
                (4 - remaining.Count) + "/4 missing=" +
                string.Join(",", remaining.Select(value => value.ToString())));
        }

        [DebugAction(
            "TechSense Filters",
            "Measure TechSense classification cache",
            actionType = DebugActionType.Action)]
        private static void MeasureClassificationCache()
        {
            Map map = Find.CurrentMap;
            ThingDef[] items =
                ThingFilter.CreateOnlyEverStorableThingFilter()
                    .AllowedThingDefs
                    .OrderBy(definition => definition.shortHash)
                    .ToArray();

            ClassificationService.Invalidate(map);
            int gameTick = Find.TickManager?.TicksGame ?? 0;
            var timer = Stopwatch.StartNew();
            for (int i = 0; i < items.Length; i++)
            {
                ClassificationService.Get(items[i], map);
            }

            timer.Stop();
            long coldMilliseconds = timer.ElapsedMilliseconds;
            timer.Restart();
            for (int i = 0; i < items.Length; i++)
            {
                ClassificationService.Get(items[i], map);
            }

            timer.Stop();
            Log.Message(
                "[TechSense Filters] cacheProbe items=" + items.Length +
                " coldMs=" + coldMilliseconds +
                " warmMs=" + timer.ElapsedMilliseconds +
                " startTick=" + gameTick +
                " endTick=" + (Find.TickManager?.TicksGame ?? 0));
        }

        private static void RunConditionalInstanceProbe(Map map)
        {
            ThingDef sourceDef =
                DefDatabase<ThingDef>.GetNamedSilentFail("CraftingSpot");
            if (sourceDef == null)
            {
                Log.Warning(
                    "[TechSense Filters] conditionalInstanceProbe=failed " +
                    "missingSourceDef");
                return;
            }

            Building evenSource = null;
            Building oddSource = null;
            try
            {
                if (!TrySpawnSource(
                        sourceDef,
                        map,
                        cell => cell.x % 2 == 0,
                        out evenSource) ||
                    !TrySpawnSource(
                        sourceDef,
                        map,
                        cell => cell.x % 2 != 0,
                        out oddSource))
                {
                    Log.Warning(
                        "[TechSense Filters] conditionalInstanceProbe=failed " +
                        "noSpawnCells");
                    return;
                }

                var recipe = new RecipeDef
                {
                    defName = "TechSense_ConditionalInstanceProbe",
                    label = "conditional instance probe",
                    workerClass =
                        typeof(RecipeWorker_TechSenseConditionalInstanceProbe)
                };
                DefinitionProductionIndex index =
                    DefinitionProductionIndex.Build();
                MapCapabilitySnapshot snapshot =
                    MapCapabilitySnapshot.Capture(
                        map,
                        index,
                        Find.TickManager?.TicksGame ?? 0);
                ProductionSourceSelection selection =
                    snapshot.SelectSource(
                        recipe,
                        new[] { sourceDef },
                        sourceDef.label);

                Log.Message(
                    "[TechSense Filters] conditionalInstanceProbe=complete " +
                    "sourceDef=" + sourceDef.defName +
                    " rejectedCell=" + oddSource.Position +
                    " acceptedCell=" + evenSource.Position +
                    " rejectedAvailable=" +
                    recipe.AvailableOnNow(oddSource)
                        .ToString().ToLowerInvariant() +
                    " acceptedAvailable=" +
                    recipe.AvailableOnNow(evenSource)
                        .ToString().ToLowerInvariant() +
                    " sourcePresent=" +
                    selection.SourcePresent.ToString().ToLowerInvariant() +
                    " billGiverUsable=" +
                    selection.BillGiverUsable.ToString().ToLowerInvariant() +
                    " recipeUsable=" +
                    selection.SourceUsable.ToString().ToLowerInvariant());
            }
            finally
            {
                evenSource?.Destroy(DestroyMode.Vanish);
                oddSource?.Destroy(DestroyMode.Vanish);
            }
        }

        private static void RunWorkstationInvalidationProbe(Map map)
        {
            ThingDef sourceDef =
                DefDatabase<ThingDef>.GetNamedSilentFail("CraftingSpot");
            if (sourceDef == null)
            {
                Log.Warning(
                    "[TechSense Filters] workstationProbe=failed " +
                    "missingSourceDef");
                return;
            }

            RecipeDef[] recipes = sourceDef.AllRecipes
                .Where(recipe =>
                    recipe != null &&
                    recipe.AvailableNow &&
                    recipe.products != null &&
                    recipe.products.Count > 0)
                .OrderBy(recipe => recipe.defName)
                .ToArray();
            for (int recipeIndex = 0;
                recipeIndex < recipes.Length;
                recipeIndex++)
            {
                ThingDef product = recipes[recipeIndex]
                    .products
                    .FirstOrDefault(productEntry =>
                        productEntry?.thingDef != null)
                    ?.thingDef;
                if (product == null)
                {
                    continue;
                }

                ClassificationResult before =
                    ClassificationService.Get(product, map);
                if (before.Classification !=
                    ProductionClassification.ResearchUnlocked)
                {
                    continue;
                }

                Building source = null;
                try
                {
                    if (!TrySpawnSource(
                            sourceDef,
                            map,
                            cell => true,
                            out source))
                    {
                        break;
                    }

                    int gameTick =
                        Find.TickManager?.TicksGame ?? 0;
                    ClassificationResult afterSpawn =
                        ClassificationService.Get(product, map);
                    source.Destroy(DestroyMode.Vanish);
                    source = null;
                    ClassificationResult afterRemoval =
                        ClassificationService.Get(product, map);
                    if (afterSpawn.Classification ==
                            ProductionClassification.CanMakeNow &&
                        afterRemoval.Classification ==
                            before.Classification)
                    {
                        Log.Message(
                            "[TechSense Filters] workstationProbe=complete " +
                            "tick=" + gameTick +
                            " recipe=" + recipes[recipeIndex].defName +
                            " product=" + product.defName +
                            " before=" + before.Classification +
                            " afterSpawn=" +
                            afterSpawn.Classification +
                            " afterRemoval=" +
                            afterRemoval.Classification);
                        return;
                    }
                }
                finally
                {
                    source?.Destroy(DestroyMode.Vanish);
                }
            }

            Log.Warning(
                "[TechSense Filters] workstationProbe=failed " +
                "noDeterministicCandidate");
        }

        private static void RunResearchInvalidationProbe(Map map)
        {
            foreach (RecipeDef recipe in
                DefDatabase<RecipeDef>.AllDefsListForReading
                    .Where(definition =>
                        definition != null &&
                        !definition.IsSurgery &&
                        !definition.AvailableNow &&
                        definition.products != null &&
                        definition.products.Count > 0)
                    .OrderBy(definition => definition.defName))
            {
                ThingDef product = recipe.products
                    .FirstOrDefault(productEntry =>
                        productEntry?.thingDef != null)
                    ?.thingDef;
                if (product == null)
                {
                    continue;
                }

                var projects = new List<ResearchProjectDef>();
                if (recipe.researchPrerequisite != null &&
                    !recipe.researchPrerequisite.IsFinished)
                {
                    projects.Add(recipe.researchPrerequisite);
                }

                if (recipe.researchPrerequisites != null)
                {
                    projects.AddRange(
                        recipe.researchPrerequisites.Where(project =>
                            project != null &&
                            !project.IsFinished &&
                            !projects.Contains(project)));
                }

                if (projects.Count == 0)
                {
                    continue;
                }

                ClassificationResult before =
                    ClassificationService.Get(product, map);
                if (before.Classification !=
                    ProductionClassification.CannotMakeYet)
                {
                    continue;
                }

                int gameTick = Find.TickManager?.TicksGame ?? 0;
                for (int projectIndex = 0;
                    projectIndex < projects.Count;
                    projectIndex++)
                {
                    Find.ResearchManager.FinishProject(
                        projects[projectIndex],
                        doCompletionDialog: false,
                        researcher: null,
                        doCompletionLetter: false);
                }

                ClassificationResult after =
                    ClassificationService.Get(product, map);
                if (after.Classification !=
                    ProductionClassification.CannotMakeYet)
                {
                    Log.Message(
                        "[TechSense Filters] researchProbe=complete " +
                        "tick=" + gameTick +
                        "recipe=" + recipe.defName +
                        "product=" + product.defName +
                        "projects=" +
                        string.Join(
                            ",",
                            projects.Select(project => project.defName)) +
                        " before=" + before.Classification +
                        " after=" + after.Classification);
                    return;
                }
            }

            Log.Warning(
                "[TechSense Filters] researchProbe=failed " +
                "noDeterministicCandidate");
        }

        private static void RunMultiPathDefinitionProbe(Map map)
        {
            var group = DefDatabase<RecipeDef>.AllDefsListForReading
                .Where(recipe =>
                    recipe != null &&
                    !recipe.IsSurgery &&
                    recipe.products != null)
                .SelectMany(recipe => recipe.products
                    .Where(product => product?.thingDef != null)
                    .Select(product => new
                    {
                        Product = product.thingDef,
                        Recipe = recipe
                    }))
                .GroupBy(entry => entry.Product)
                .OrderBy(entries => entries.Key.defName)
                .FirstOrDefault(entries =>
                    entries.Select(entry => entry.Recipe)
                        .Distinct()
                        .Count() > 1);
            if (group == null)
            {
                Log.Warning(
                    "[TechSense Filters] multiPathProbe=failed " +
                    "noDefinitionCandidate");
                return;
            }

            RecipeDef[] recipes = group
                .Select(entry => entry.Recipe)
                .Distinct()
                .OrderBy(recipe => recipe.defName)
                .ToArray();
            ClassificationResult result =
                ClassificationService.Get(group.Key, map);
            Log.Message(
                "[TechSense Filters] multiPathProbe=complete " +
                "product=" + group.Key.defName +
                "pathCount=" + recipes.Length +
                "paths=" +
                string.Join(
                    ",",
                    recipes.Select(recipe => recipe.defName)) +
                " classification=" + result.Classification +
                " selectedPath=" + (result.PathLabel ?? string.Empty));
        }

        private static bool TrySpawnSource(
            ThingDef sourceDef,
            Map map,
            Predicate<IntVec3> additionalCellValidator,
            out Building building)
        {
            building = null;
            if (!CellFinder.TryRandomClosewalkCellNear(
                    map.Center,
                    map,
                    35,
                    out IntVec3 cell,
                    candidate =>
                        candidate.Standable(map) &&
                        candidate.GetEdifice(map) == null &&
                        (additionalCellValidator?.Invoke(candidate) ?? true)))
            {
                return false;
            }

            ThingDef stuff = sourceDef.MadeFromStuff
                ? GenStuff.DefaultStuffFor(sourceDef)
                : null;
            Thing source = ThingMaker.MakeThing(sourceDef, stuff);
            source.SetFaction(Faction.OfPlayer);
            building = GenSpawn.Spawn(
                source,
                cell,
                map,
                WipeMode.Vanish) as Building;
            return building != null;
        }
    }

    public sealed class RecipeWorker_TechSenseConditionalInstanceProbe :
        RecipeWorker
    {
        public override bool AvailableOnNow(
            Thing thing,
            BodyPartRecord part = null)
        {
            return thing != null && thing.Position.x % 2 == 0;
        }
    }

    internal sealed class Dialog_TechSenseFixture : Window
    {
        private readonly ThingFilter filter =
            ThingFilter.CreateOnlyEverStorableThingFilter();
        private readonly ThingFilterUI.UIState uiState =
            new ThingFilterUI.UIState();
        private readonly int initialFingerprint;

        internal Dialog_TechSenseFixture(string initialSearch = null)
        {
            initialFingerprint = Fingerprint(filter);
            if (!string.IsNullOrWhiteSpace(initialSearch))
            {
                uiState.quickSearch.filter.Text = initialSearch;
            }

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
