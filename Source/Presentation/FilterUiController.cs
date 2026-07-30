using System;
using System.Runtime.CompilerServices;
using TechSenseFilters.Domain;
using TechSenseFilters.Runtime;
using TechSenseFilters.Settings;
using UnityEngine;
using Verse;

namespace TechSenseFilters.Presentation
{
    internal static class FilterUiController
    {
        private const float ToolbarHeight = 34f;
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

            TechSenseFiltersSettings settings =
                TechSenseFiltersSettings.Current;
            if (!settings.FeatureEnabled ||
                !settings.ShowClassificationToolbar)
            {
                return;
            }

            Rect toolbarRect = new Rect(
                rect.x + 3f,
                rect.y + 2f,
                rect.width - 6f,
                ToolbarHeight - 4f);
            DrawToolbar(toolbarRect, state);
            rect.yMin += ToolbarHeight;
        }

        internal static void End()
        {
            current = null;
        }

        internal static bool ShouldShow(ThingDef thingDef)
        {
            if (current == null ||
                !TechSenseFiltersSettings.Current.FeatureEnabled ||
                !TechSenseFiltersSettings.Current.ShowClassificationToolbar)
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
                !TechSenseFiltersSettings.Current.FeatureEnabled ||
                !TechSenseFiltersSettings.Current.ShowStatusIndicators)
            {
                return;
            }

            Map effectiveMap = map ?? current?.Map ?? Find.CurrentMap;
            ClassificationResult result =
                ClassificationService.Get(thingDef, effectiveMap);
            Rect iconRect = new Rect(
                listing.ColumnWidth - 45f,
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

            TooltipHandler.TipRegion(
                iconRect.ExpandedBy(3f),
                ClassificationPresentation.FullLabel(result.Classification) +
                "\n\n" + result.Explanation);
        }

        private static void DrawToolbar(
            Rect rect,
            FilterPresentationState state)
        {
            Widgets.DrawMenuSection(rect);
            const float titleWidth = 74f;
            Rect titleRect = new Rect(
                rect.x + 6f,
                rect.y,
                titleWidth,
                rect.height);
            TextAnchor previousAnchor = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(titleRect, "TechSense_Title".Translate());
            Text.Anchor = previousAnchor;

            float gap = 3f;
            float buttonsX = titleRect.xMax + 2f;
            float buttonWidth =
                (rect.xMax - buttonsX - (gap * 4f) - 3f) / 4f;
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
                Rect buttonRect = new Rect(
                    buttonsX + (i * (buttonWidth + gap)),
                    rect.y + 3f,
                    buttonWidth,
                    rect.height - 6f);
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
                if (Widgets.ButtonText(
                    buttonRect,
                    ClassificationPresentation.ShortLabel(classification)))
                {
                    state.Toggle(classification);
                }

                GUI.color = previous;
                TooltipHandler.TipRegion(
                    buttonRect,
                    ClassificationPresentation.FullLabel(classification) +
                    "\n" + "TechSense_ToggleTooltip".Translate());
            }
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
