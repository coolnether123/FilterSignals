using System;

namespace TechSenseFilters.Presentation
{
    internal enum ToolbarLayoutMode
    {
        Inline,
        TwoColumn,
        SingleColumn
    }

    internal readonly struct LayoutRect
    {
        internal LayoutRect(
            float x,
            float y,
            float width,
            float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        internal float X { get; }
        internal float Y { get; }
        internal float Width { get; }
        internal float Height { get; }
        internal float XMax => X + Width;
        internal float YMax => Y + Height;

        internal bool Overlaps(LayoutRect other)
        {
            return X < other.XMax &&
                XMax > other.X &&
                Y < other.YMax &&
                YMax > other.Y;
        }
    }

    internal sealed class ToolbarLayoutPlan
    {
        internal ToolbarLayoutPlan(
            ToolbarLayoutMode mode,
            float height,
            LayoutRect title,
            LayoutRect[] buttons)
        {
            Mode = mode;
            Height = height;
            Title = title;
            Buttons = buttons ?? Array.Empty<LayoutRect>();
        }

        internal ToolbarLayoutMode Mode { get; }
        internal float Height { get; }
        internal LayoutRect Title { get; }
        internal LayoutRect[] Buttons { get; }
    }

    internal static class ToolbarLayout
    {
        private const int ButtonCount = 4;
        private const float Padding = 3f;
        private const float Gap = 3f;
        private const float TitleWidth = 74f;
        private const float InlineHeight = 30f;
        private const float ButtonHeight = 26f;
        private const float StackedTitleHeight = 20f;
        private const float MinimumInlineButtonWidth = 72f;
        private const float MinimumTwoColumnButtonWidth = 68f;

        internal static ToolbarLayoutPlan Calculate(float width)
        {
            float safeWidth = IsFinite(width)
                ? Math.Max(0f, width)
                : 0f;
            float inlineButtonsX = Padding + TitleWidth + 2f;
            float inlineButtonWidth =
                (safeWidth - inlineButtonsX - Padding -
                    (Gap * (ButtonCount - 1))) /
                ButtonCount;
            if (inlineButtonWidth >= MinimumInlineButtonWidth)
            {
                var inlineButtons = new LayoutRect[ButtonCount];
                for (int index = 0; index < ButtonCount; index++)
                {
                    inlineButtons[index] = new LayoutRect(
                        inlineButtonsX +
                            (index * (inlineButtonWidth + Gap)),
                        Padding,
                        inlineButtonWidth,
                        InlineHeight - (Padding * 2f));
                }

                return new ToolbarLayoutPlan(
                    ToolbarLayoutMode.Inline,
                    InlineHeight,
                    new LayoutRect(
                        Padding + 3f,
                        0f,
                        TitleWidth,
                        InlineHeight),
                    inlineButtons);
            }

            float contentWidth = Math.Max(
                0f,
                safeWidth - (Padding * 2f));
            int columns =
                ((contentWidth - Gap) / 2f) >=
                    MinimumTwoColumnButtonWidth
                    ? 2
                    : 1;
            int rows = (ButtonCount + columns - 1) / columns;
            float buttonWidth =
                Math.Max(
                    0f,
                    (contentWidth - (Gap * (columns - 1))) /
                    columns);
            float buttonsY =
                Padding + StackedTitleHeight + 2f;
            var stackedButtons = new LayoutRect[ButtonCount];
            for (int index = 0; index < ButtonCount; index++)
            {
                int column = index % columns;
                int row = index / columns;
                stackedButtons[index] = new LayoutRect(
                    Padding + (column * (buttonWidth + Gap)),
                    buttonsY + (row * (ButtonHeight + Gap)),
                    buttonWidth,
                    ButtonHeight);
            }

            float height =
                buttonsY +
                (rows * ButtonHeight) +
                ((rows - 1) * Gap) +
                Padding;
            return new ToolbarLayoutPlan(
                columns == 2
                    ? ToolbarLayoutMode.TwoColumn
                    : ToolbarLayoutMode.SingleColumn,
                height,
                new LayoutRect(
                    Padding + 3f,
                    Padding,
                    Math.Max(0f, contentWidth - 6f),
                    StackedTitleHeight),
                stackedButtons);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) &&
                !float.IsInfinity(value);
        }
    }
}
