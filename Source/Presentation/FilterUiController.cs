using System;
using System.Runtime.CompilerServices;
using RimWorld;
using Spine.UI.ContextualSettings;
using FilterSignals.Bootstrap;
using FilterSignals.Domain;
using FilterSignals.Runtime;
using FilterSignals.Settings;
using UnityEngine;
using Verse;

namespace FilterSignals.Presentation
{
    internal static class FilterUiController
    {
        private const float ToolbarOuterPadding = 4f;
        private const float StatusIndicatorRightInset = 45f;
        private static readonly ConditionalWeakTable<
            ThingFilterUI.UIState,
            FilterPresentationState> States =
                new ConditionalWeakTable<
                    ThingFilterUI.UIState,
                    FilterPresentationState>();

        [ThreadStatic]
        private static FilterDialogContext current;

        internal static void BeginAndDraw(
            ref Rect rect,
            ThingFilterUI.UIState uiState,
            Map map)
        {
            Map effectiveMap = map ?? Find.CurrentMap;
            FilterPresentationState state = uiState == null
                ? new FilterPresentationState()
                : States.GetValue(
                    uiState,
                    _ => new FilterPresentationState());
            current = new FilterDialogContext(state, effectiveMap);

            FilterSignalsSettings settings =
                FilterSignalsMod.Settings;
            if (!settings.FeatureEnabled ||
                !settings.ShowClassificationToolbar)
            {
                return;
            }

            float toolbarWidth = Mathf.Max(0f, rect.width - 6f);
            string title = "FilterSignals_Title".Translate();
            float titleWidth =
                Text.CalcSize(title).x +
                ToolbarLayout.TitleHorizontalPadding;
            ToolbarLayoutPlan layout =
                ToolbarLayout.Calculate(toolbarWidth, titleWidth);
            Rect toolbarRect = new Rect(
                rect.x + 3f,
                rect.y + 2f,
                toolbarWidth,
                layout.Height);
            DrawToolbar(toolbarRect, state, layout, title);
            rect.yMin += layout.Height + ToolbarOuterPadding;
        }

        internal static void End()
        {
            current = null;
        }

        internal static bool StatusIndicatorsActive =>
            FilterSignalsMod.Settings.FeatureEnabled &&
            FilterSignalsMod.Settings.ShowStatusIndicators;

        internal static bool ShouldShow(ThingDef thingDef)
        {
            if (current == null ||
                !FilterSignalsMod.Settings.FeatureEnabled ||
                !FilterSignalsMod.Settings.ShowClassificationToolbar)
            {
                return true;
            }

            ClassificationResult result =
                ClassificationService.Get(thingDef, current.Map);
            return current.State.IsEnabled(result.Classification);
        }

        internal static void DrawIndicator(
            Listing_TreeThingFilter listing,
            ThingDef thingDef,
            Map map,
            float rowY)
        {
            if (listing == null ||
                thingDef == null ||
                !StatusIndicatorsActive)
            {
                return;
            }

            Map effectiveMap = map ?? current?.Map ?? Find.CurrentMap;
            ClassificationResult result =
                ClassificationService.Get(thingDef, effectiveMap);
            Rect iconRect = new Rect(
                listing.ColumnWidth - StatusIndicatorRightInset,
                rowY + 4f,
                12f,
                12f);
            Color color =
                ClassificationPresentation.ColorFor(result.Classification);
            Color previous = GUI.color;
            Widgets.DrawBoxSolid(iconRect, color);
            GUI.color = new Color(0f, 0f, 0f, 0.85f);
            Widgets.DrawBox(iconRect, 1);
            GUI.color = previous;

            Rect interactionRect = iconRect.ExpandedBy(3f);
            if (!Mouse.IsOver(interactionRect))
            {
                return;
            }

            ClassificationNavigationTarget target =
                ClassificationNavigationResolver.Resolve(
                    thingDef,
                    effectiveMap,
                    result);
            string navigation =
                target.IsActionable
                    ? ClassificationPresentation.NavigationTooltip(
                        target.Decision)
                    : string.Empty;
            string explanation =
                ClassificationPresentation.BriefExplanation(
                    result,
                    target.Decision);
            string tooltip =
                ClassificationPresentation.FullLabel(result.Classification);
            if (explanation.Length > 0)
            {
                tooltip += "\n" + explanation;
            }

            if (thingDef.IsStuff && thingDef.smallVolume)
            {
                tooltip += "\n" + "FilterSignals_SmallVolume".Translate();
            }

            if (navigation.Length > 0)
            {
                tooltip += "\n" + navigation;
            }

            TooltipHandler.ClearTooltipsFrom(interactionRect);
            TooltipHandler.TipRegion(interactionRect, tooltip);
            ContextualSettingsTarget settingsTarget =
                result.Reason == ClassificationReason.MaterialShortage
                    ? ContextualSettingsTarget.Exact(
                        "classification.materials",
                        "feature.enabled")
                    : ContextualSettingsTarget.Exact(
                        "presentation.indicators",
                        "feature.enabled");
            if (FilterSignalsMod.ContextualSettings?.Bind(
                interactionRect,
                settingsTarget,
                new ContextualSettingsBindingOptions(priority: 20)) == true)
            {
                return;
            }

            if (!Widgets.ButtonInvisible(interactionRect))
            {
                return;
            }

            if (target.IsActionable)
            {
                ClassificationNavigationController.TryNavigate(target);
            }
        }

        private static void DrawToolbar(
            Rect rect,
            FilterPresentationState state,
            ToolbarLayoutPlan layout,
            string title)
        {
            if (FilterSignalsMod.ContextualSettings?.Bind(
                rect,
                ContextualSettingsTarget.Group("presentation.toolbar"),
                new ContextualSettingsBindingOptions(priority: 0)) == true)
            {
                return;
            }

            Widgets.DrawMenuSection(rect);
            Rect titleRect = ToRect(rect, layout.Title);
            TextAnchor previousAnchor = Text.Anchor;
            bool previousWordWrap = Text.WordWrap;
            try
            {
                Text.Anchor = TextAnchor.MiddleLeft;
                Text.WordWrap = false;
                Widgets.Label(titleRect, title);
            }
            finally
            {
                Text.Anchor = previousAnchor;
                Text.WordWrap = previousWordWrap;
            }

            ProductionClassification[] classifications =
            {
                ProductionClassification.CanMakeNow,
                ProductionClassification.ResearchUnlocked,
                ProductionClassification.CannotMakeYet,
                ProductionClassification.NotApplicable
            };

            for (int i = 0; i < classifications.Length; i++)
            {
                ProductionClassification classification =
                    classifications[i];
                Rect buttonRect = ToRect(
                    rect,
                    layout.Buttons[i]);
                bool enabled = state.IsEnabled(classification);
                Color previous = GUI.color;
                Color statusColor =
                    ClassificationPresentation.ColorFor(classification);
                GUI.color = enabled
                    ? statusColor
                    : new Color(
                        statusColor.r,
                        statusColor.g,
                        statusColor.b,
                        0.34f);
                string buttonTooltip =
                    ClassificationPresentation.FullLabel(classification) +
                    "\n" + "FilterSignals_ToggleTooltip".Translate();
                if (FilterSignalsMod.ContextualSettings?.Bind(
                    buttonRect,
                    ContextualSettingsTarget.Exact(
                        "presentation.toolbar",
                        "feature.enabled"),
                    ContextualSettingsBindingOptions.WithTooltip(
                        buttonTooltip,
                        priority: 10)) == true)
                {
                    GUI.color = previous;
                    continue;
                }

                if (Widgets.ButtonText(
                    buttonRect,
                    ClassificationPresentation.ShortLabel(classification)))
                {
                    state.Toggle(classification);
                }

                GUI.color = previous;
            }
        }

        private static Rect ToRect(
            Rect origin,
            LayoutRect relative)
        {
            return new Rect(
                origin.x + relative.X,
                origin.y + relative.Y,
                relative.Width,
                relative.Height);
        }

        private sealed class FilterDialogContext
        {
            internal FilterDialogContext(
                FilterPresentationState state,
                Map map)
            {
                State = state;
                Map = map;
            }

            internal FilterPresentationState State { get; }
            internal Map Map { get; }
        }
    }
}
