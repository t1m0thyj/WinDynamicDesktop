// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using SkiaSharp;
using System;
using System.Drawing;

namespace WinDynamicDesktop.Skia
{
    internal class ThemePreviewRenderer
    {
        private const int MARGIN_STANDARD = 20;
        private const int BORDER_RADIUS = 5;
        private const int ARROW_AREA_WIDTH = 80;
        private const byte OVERLAY_ALPHA = 127;
        private const float OPACITY_NORMAL = 0.5f;
        private const float OPACITY_HOVER = 1.0f;

        // FontAwesome icons
        private const string ICON_PLAY = "\uf04b";
        private const string ICON_PAUSE = "\uf04c";
        private const string ICON_CHEVRON_LEFT = "\uf053";
        private const string ICON_CHEVRON_RIGHT = "\uf054";

        private readonly SKPaint basePaint;
        private readonly SKTypeface systemFont;
        private readonly SKTypeface systemFontBold;
        private readonly SKFont titleFont;
        private readonly SKFont textFont;
        private readonly SKFont iconFont16;
        private readonly SKFont iconFont20;
        private readonly SKColor overlayColor;
        private readonly SKSamplingOptions samplingOptions;

        // Hit test regions (updated during rendering)
        public Rectangle TitleBoxRect { get; private set; }
        public Rectangle PlayButtonRect { get; private set; }
        public Rectangle DownloadSizeLabelRect { get; private set; }
        public Rectangle AuthorLabelRect { get; private set; }
        public Rectangle DownloadMessageRect { get; private set; }
        public Rectangle LeftArrowRect { get; private set; }
        public Rectangle RightArrowRect { get; private set; }
        public Rectangle[] CarouselIndicatorRects { get; private set; }

        private enum Side { Left, Right }

        public ThemePreviewRenderer(SKTypeface fontAwesome, string systemFontName)
        {
            basePaint = new SKPaint { IsAntialias = true };
            systemFont = SKTypeface.FromFamilyName(systemFontName);
            systemFontBold = SKTypeface.FromFamilyName(systemFontName, SKFontStyle.Bold);
            titleFont = new SKFont(systemFontBold, 19);
            textFont = new SKFont(systemFont, 16);
            iconFont16 = new SKFont(fontAwesome, 16);
            iconFont20 = new SKFont(fontAwesome, 20);
            overlayColor = new SKColor(0, 0, 0, OVERLAY_ALPHA);
            samplingOptions = new SKSamplingOptions(SKCubicResampler.Mitchell);
        }

        public void DrawImage(SKCanvas canvas, SKImage image, SKImageInfo info, float opacity)
        {
            var destRect = new SKRect(0, 0, info.Width, info.Height);

            if (opacity >= 1.0f)
            {
                // Fast path for fully opaque images
                canvas.DrawImage(image, destRect, samplingOptions, null);
            }
            else
            {
                // Apply opacity with color filter
                using (var paint = new SKPaint())
                using (var colorFilter = SKColorFilter.CreateBlendMode(
                    SKColors.White.WithAlpha((byte)(255 * opacity)),
                    SKBlendMode.DstIn))
                {
                    paint.IsAntialias = true;
                    paint.ColorFilter = colorFilter;
                    canvas.DrawImage(image, destRect, samplingOptions, paint);
                }
            }
        }

        public void DrawOverlay(SKCanvas canvas, SKImageInfo info, ThemePreviewerViewModel viewModel,
            ThemePreviewer.HoveredItem hoveredItem)
        {
            // Set arrow hit regions
            LeftArrowRect = new Rectangle(0, 0, ARROW_AREA_WIDTH, info.Height);
            RightArrowRect = new Rectangle(info.Width - ARROW_AREA_WIDTH, 0, ARROW_AREA_WIDTH, info.Height);

            // Draw left and right arrow button areas
            DrawArrowArea(canvas, info, Side.Left, hoveredItem == ThemePreviewer.HoveredItem.LeftArrow);
            DrawArrowArea(canvas, info, Side.Right, hoveredItem == ThemePreviewer.HoveredItem.RightArrow);

            // Title and preview text box (top left)
            SKRect titleBounds;
            titleFont.MeasureText(viewModel.Title ?? "", out titleBounds);
            SKRect previewBounds;
            textFont.MeasureText(viewModel.PreviewText ?? "", out previewBounds);

            float boxWidth = Math.Max(titleBounds.Width, previewBounds.Width) + MARGIN_STANDARD;
            float boxHeight = 19 + 4 + 16 + MARGIN_STANDARD;
            TitleBoxRect = new Rectangle(MARGIN_STANDARD, MARGIN_STANDARD, (int)boxWidth, (int)boxHeight);

            basePaint.Color = overlayColor;
            canvas.DrawRoundRect(SKRect.Create(TitleBoxRect.X, TitleBoxRect.Y, TitleBoxRect.Width, TitleBoxRect.Height), BORDER_RADIUS, BORDER_RADIUS, basePaint);

            basePaint.Color = SKColors.White;
            canvas.DrawText(viewModel.Title ?? "", TitleBoxRect.X + 10, TitleBoxRect.Y + 8 + 19, titleFont, basePaint);
            canvas.DrawText(viewModel.PreviewText ?? "", TitleBoxRect.X + 10, TitleBoxRect.Y + 8 + 19 + 5 + 16, textFont, basePaint);

            // Play/Pause button (top right)
            int playButtonSize = 40;
            PlayButtonRect = new Rectangle(info.Width - playButtonSize - MARGIN_STANDARD, MARGIN_STANDARD, playButtonSize, playButtonSize);

            basePaint.Color = overlayColor;
            canvas.DrawRoundRect(SKRect.Create(PlayButtonRect.X, PlayButtonRect.Y, PlayButtonRect.Width, PlayButtonRect.Height), BORDER_RADIUS, BORDER_RADIUS, basePaint);

            float playOpacity = hoveredItem == ThemePreviewer.HoveredItem.PlayButton ? OPACITY_HOVER : OPACITY_NORMAL;
            basePaint.Color = SKColors.White.WithAlpha((byte)(255 * playOpacity));
            string playIcon = viewModel.IsPlaying ? ICON_PAUSE : ICON_PLAY;
            SKRect textBounds;
            iconFont16.MeasureText(playIcon, out textBounds);
            float centerX = PlayButtonRect.X + PlayButtonRect.Width / 2;
            float centerY = PlayButtonRect.Y + PlayButtonRect.Height / 2;
            canvas.DrawText(playIcon, centerX - textBounds.MidX, centerY - textBounds.MidY, iconFont16, basePaint);

            // Corner labels
            DrawCornerLabel(canvas, info, viewModel.DownloadSize, Side.Left, out var downloadSizeRect);
            DownloadSizeLabelRect = downloadSizeRect;
            DrawCornerLabel(canvas, info, viewModel.Author, Side.Right, out var authorRect);
            AuthorLabelRect = authorRect;

            // Download message (centered bottom)
            if (!string.IsNullOrEmpty(viewModel.Message))
            {
                SKRect msgBounds;
                textFont.MeasureText(viewModel.Message, out msgBounds);
                float msgWidth = msgBounds.Width + 16;
                float msgHeight = 6 + 16 + 6;
                DownloadMessageRect = new Rectangle((int)(info.Width / 2 - msgWidth / 2), info.Height - (int)msgHeight - 15, (int)msgWidth, (int)msgHeight);

                basePaint.Color = overlayColor;
                canvas.DrawRoundRect(SKRect.Create(DownloadMessageRect.X, DownloadMessageRect.Y, DownloadMessageRect.Width, DownloadMessageRect.Height), BORDER_RADIUS, BORDER_RADIUS, basePaint);

                float msgOpacity = hoveredItem == ThemePreviewer.HoveredItem.DownloadButton ? OPACITY_HOVER : OPACITY_NORMAL;
                basePaint.Color = SKColors.White.WithAlpha((byte)(255 * msgOpacity));
                canvas.DrawText(viewModel.Message, DownloadMessageRect.X + 8, DownloadMessageRect.Y + 5 + 16, textFont, basePaint);
            }
            else
            {
                DownloadMessageRect = Rectangle.Empty;
            }

            // Carousel indicators
            if (viewModel.CarouselIndicatorsVisible && viewModel.Items.Count > 0)
            {
                DrawCarouselIndicators(canvas, info, viewModel.Items.Count, viewModel.SelectedIndex);
            }
            else
            {
                CarouselIndicatorRects = null;
            }
        }

        private void DrawArrowArea(SKCanvas canvas, SKImageInfo info, Side side, bool isHovered)
        {
            float opacity = isHovered ? OPACITY_HOVER : OPACITY_NORMAL;

            float x = side == Side.Left ? 40 : info.Width - 40;
            float y = info.Height / 2;

            basePaint.Color = SKColors.White.WithAlpha((byte)(255 * opacity));

            string icon = side == Side.Left ? ICON_CHEVRON_LEFT : ICON_CHEVRON_RIGHT;
            SKRect textBounds;
            iconFont20.MeasureText(icon, out textBounds);
            canvas.DrawText(icon, x - textBounds.MidX, y - textBounds.MidY, iconFont20, basePaint);
        }

        private void DrawCornerLabel(SKCanvas canvas, SKImageInfo info, string text, Side side, out Rectangle labelRect)
        {
            if (string.IsNullOrEmpty(text))
            {
                labelRect = Rectangle.Empty;
                return;
            }

            SKRect textBounds;
            textFont.MeasureText(text, out textBounds);

            var padding = new System.Windows.Forms.Padding(8, 4, 10, 10); // Left, Top, Right, Bottom
            float leftMargin = side == Side.Right ? padding.Left : padding.Right;
            float rightMargin = side == Side.Right ? padding.Right : padding.Left;
            float borderWidth = textBounds.Width + leftMargin + rightMargin;
            float borderHeight = padding.Top + 16 + padding.Bottom;

            int offset = 3;
            float rectX = side == Side.Right ? info.Width - borderWidth + offset : -offset;
            float rectY = info.Height - borderHeight + offset;
            labelRect = new Rectangle((int)rectX, (int)rectY, (int)borderWidth, (int)borderHeight);

            basePaint.Color = overlayColor;
            canvas.DrawRoundRect(SKRect.Create(rectX, rectY, borderWidth, borderHeight), BORDER_RADIUS, BORDER_RADIUS, basePaint);

            basePaint.Color = SKColors.White.WithAlpha(OVERLAY_ALPHA);
            float textX = side == Side.Right ? info.Width - textBounds.Width - rightMargin + offset : leftMargin - offset;
            float textY = rectY + padding.Top + 16;
            canvas.DrawText(text, textX, textY, textFont, basePaint);
        }

        private void DrawCarouselIndicators(SKCanvas canvas, SKImageInfo info, int count, int selectedIndex)
        {
            int indicatorWidth = 30;
            int indicatorHeight = 3;
            int itemSpacing = 6;
            int totalWidth = count * indicatorWidth + (count - 1) * itemSpacing;
            int startX = (info.Width - totalWidth) / 2;
            int y = info.Height - 16 - 16;  // bottom margin - half of clickable height

            CarouselIndicatorRects = new Rectangle[count];

            for (int i = 0; i < count; i++)
            {
                float opacity = (i == selectedIndex) ? OPACITY_HOVER : OPACITY_NORMAL;
                basePaint.Color = SKColors.White.WithAlpha((byte)(255 * opacity));

                int rectX = startX + i * (indicatorWidth + itemSpacing);
                canvas.DrawRect(rectX, y - indicatorHeight / 2, indicatorWidth, indicatorHeight, basePaint);

                // Cache full clickable area for hit testing
                CarouselIndicatorRects[i] = new Rectangle(rectX, y - 16, indicatorWidth, 32);
            }
        }

        public void Dispose()
        {
            basePaint?.Dispose();
            systemFont?.Dispose();
            systemFontBold?.Dispose();
            titleFont?.Dispose();
            textFont?.Dispose();
            iconFont16?.Dispose();
            iconFont20?.Dispose();
        }
    }
}
