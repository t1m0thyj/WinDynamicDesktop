// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace WinDynamicDesktop.Skia
{
    public class ThemePreviewer : SKControl
    {
        private const int ANIMATION_DURATION_MS = 600;
        private const int ANIMATION_FPS = 120;
        private const int MARGIN_STANDARD = 20;
        private const int BORDER_RADIUS = 5;
        private const int ARROW_AREA_WIDTH = 80;
        private const byte OVERLAY_ALPHA = 127;
        private const float OPACITY_NORMAL = 0.5f;
        private const float OPACITY_HOVER = 1.0f;
        private const float OPACITY_MESSAGE = 0.8f;

        public ThemePreviewerViewModel ViewModel { get; }

        private readonly Timer fadeTimer;
        private float fadeProgress = 0f;
        private bool isAnimating = false;
        private DateTime animationStartTime;
        private static SKTypeface fontAwesome;

        // Cached objects to reduce allocations
        private readonly SKPaint basePaint = new SKPaint { IsAntialias = true };
        private readonly SKColor overlayColor = new SKColor(0, 0, 0, OVERLAY_ALPHA);
        private readonly SKFont titleFont;
        private readonly SKFont previewFont;
        private readonly SKFont textFont;
        private readonly SKFont iconFont16;
        private readonly SKFont iconFont20;
        private readonly SKSamplingOptions samplingOptions = new SKSamplingOptions(SKCubicResampler.Mitchell);

        private bool isMouseOverPlay = false;
        private bool isMouseOverLeft = false;
        private bool isMouseOverRight = false;
        private bool isMouseOverDownload = false;

        // Cached UI element rectangles for rendering and hit testing
        private Rectangle titleBoxRect;
        private Rectangle playButtonRect;
        private Rectangle downloadMessageRect;
        private Rectangle authorLabelRect;
        private Rectangle downloadSizeLabelRect;
        private Rectangle[] carouselIndicatorRects;

        public ThemePreviewer()
        {
            ViewModel = new ThemePreviewerViewModel(StartAnimation, StopAnimation);
            DoubleBuffered = true;

            // Load FontAwesome font once
            if (fontAwesome == null)
            {
                using (Stream fontStream = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("WinDynamicDesktop.resources.fonts.fontawesome-webfont.ttf"))
                {
                    fontAwesome = SKTypeface.FromStream(fontStream);
                }
            }

            // Initialize cached fonts
            titleFont = new SKFont(SKTypeface.FromFamilyName("Segoe UI", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), 19);
            previewFont = new SKFont(SKTypeface.FromFamilyName("Segoe UI", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright), 16);
            textFont = new SKFont(SKTypeface.FromFamilyName("Segoe UI"), 16);
            iconFont16 = new SKFont(fontAwesome, 16);
            iconFont20 = new SKFont(fontAwesome, 20);

            // Timer for smooth fade animations
            fadeTimer = new Timer
            {
                Interval = 1000 / ANIMATION_FPS
            };
            fadeTimer.Tick += FadeTimer_Tick;

            MouseEnter += (s, e) => ViewModel.IsMouseOver = true;
            MouseLeave += OnMouseLeave;

            ViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ThemePreviewerViewModel.BackImage) ||
                    e.PropertyName == nameof(ThemePreviewerViewModel.FrontImage) ||
                    e.PropertyName == nameof(ThemePreviewerViewModel.IsPlaying) ||
                    e.PropertyName == nameof(ThemePreviewerViewModel.DownloadSize))
                {
                    Invalidate();
                }
            };

            KeyDown += ThemePreviewer_KeyDown;
        }

        private void ThemePreviewer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left)
            {
                ViewModel.Previous();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Right)
            {
                ViewModel.Next();
                e.Handled = true;
            }
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            base.OnPaintSurface(e);

            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.Gray);

            var info = e.Info;

            // Draw back image
            if (ViewModel.BackImage != null)
            {
                DrawImage(canvas, ViewModel.BackImage, info, 1.0f);
            }

            // Draw front image with fade animation
            if (ViewModel.FrontImage != null && isAnimating)
            {
                DrawImage(canvas, ViewModel.FrontImage, info, fadeProgress);
            }

            // Draw UI overlay
            if (ViewModel.ControlsVisible)
            {
                DrawOverlay(canvas, info);
            }
        }

        private void DrawImage(SKCanvas canvas, SKImage image, SKImageInfo info, float opacity)
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
                {
                    paint.IsAntialias = true;
                    paint.ColorFilter = SKColorFilter.CreateBlendMode(
                        SKColors.White.WithAlpha((byte)(255 * opacity)),
                        SKBlendMode.DstIn);

                    canvas.DrawImage(image, destRect, samplingOptions, paint);
                }
            }
        }

        private void DrawOverlay(SKCanvas canvas, SKImageInfo info)
        {
            // Draw left and right arrow button areas
            DrawArrowArea(canvas, info, true);
            DrawArrowArea(canvas, info, false);

            // Title and preview text box (top left)
            var titleBounds = new SKRect();
            titleFont.MeasureText(ViewModel.Title ?? "", out titleBounds);
            var previewBounds = new SKRect();
            previewFont.MeasureText(ViewModel.PreviewText ?? "", out previewBounds);

            float boxWidth = Math.Max(titleBounds.Width, previewBounds.Width) + MARGIN_STANDARD;
            float boxHeight = 19 + 4 + 16 + MARGIN_STANDARD;
            titleBoxRect = new Rectangle(MARGIN_STANDARD, MARGIN_STANDARD, (int)boxWidth, (int)boxHeight);

            basePaint.Color = overlayColor;
            canvas.DrawRoundRect(SKRect.Create(titleBoxRect.X, titleBoxRect.Y, titleBoxRect.Width, titleBoxRect.Height), BORDER_RADIUS, BORDER_RADIUS, basePaint);

            basePaint.Color = SKColors.White;
            canvas.DrawText(ViewModel.Title ?? "", titleBoxRect.X + 10, titleBoxRect.Y + 8 + 19, titleFont, basePaint);
            canvas.DrawText(ViewModel.PreviewText ?? "", titleBoxRect.X + 10, titleBoxRect.Y + 8 + 19 + 5 + 16, previewFont, basePaint);

            // Play/Pause button (top right)
            int playButtonSize = 40;
            playButtonRect = new Rectangle(info.Width - playButtonSize - MARGIN_STANDARD, MARGIN_STANDARD, playButtonSize, playButtonSize);

            basePaint.Color = overlayColor;
            canvas.DrawRoundRect(SKRect.Create(playButtonRect.X, playButtonRect.Y, playButtonRect.Width, playButtonRect.Height), BORDER_RADIUS, BORDER_RADIUS, basePaint);

            float playOpacity = isMouseOverPlay ? OPACITY_HOVER : OPACITY_NORMAL;
            basePaint.Color = SKColors.White.WithAlpha((byte)(255 * playOpacity));
            string playIcon = ViewModel.IsPlaying ? "\uf04c" : "\uf04b";
            var textBounds = new SKRect();
            iconFont16.MeasureText(playIcon, out textBounds);
            float centerX = playButtonRect.X + playButtonRect.Width / 2;
            float centerY = playButtonRect.Y + playButtonRect.Height / 2;
            canvas.DrawText(playIcon, centerX - textBounds.MidX, centerY - textBounds.MidY, iconFont16, basePaint);

            // Corner labels
            DrawCornerLabel(canvas, info, ViewModel.Author, isBottomRight: true);
            DrawCornerLabel(canvas, info, ViewModel.DownloadSize, isBottomRight: false);

            // Download message (centered bottom)
            if (!string.IsNullOrEmpty(ViewModel.Message))
            {
                var msgBounds = new SKRect();
                textFont.MeasureText(ViewModel.Message, out msgBounds);
                float msgWidth = msgBounds.Width + 16;
                float msgHeight = 6 + 16 + 6;
                downloadMessageRect = new Rectangle((int)(info.Width / 2 - msgWidth / 2), info.Height - (int)msgHeight - 15, (int)msgWidth, (int)msgHeight);

                basePaint.Color = overlayColor;
                canvas.DrawRoundRect(SKRect.Create(downloadMessageRect.X, downloadMessageRect.Y, downloadMessageRect.Width, downloadMessageRect.Height), BORDER_RADIUS, BORDER_RADIUS, basePaint);

                float msgOpacity = isMouseOverDownload ? OPACITY_HOVER : OPACITY_MESSAGE;
                basePaint.Color = SKColors.White.WithAlpha((byte)(255 * msgOpacity));
                canvas.DrawText(ViewModel.Message, downloadMessageRect.X + 8, downloadMessageRect.Y + 5 + 16, textFont, basePaint);
            }
            else
            {
                downloadMessageRect = Rectangle.Empty;
            }

            // Carousel indicators - Margin=16, Height=32, Rectangle Height=3, Width=30, Margin="3,0"
            if (ViewModel.CarouselIndicatorsVisible && ViewModel.Items.Count > 0)
            {
                DrawCarouselIndicators(canvas, info);
            }
        }

        private void DrawArrowArea(SKCanvas canvas, SKImageInfo info, bool isLeft)
        {
            bool isHovered = isLeft ? isMouseOverLeft : isMouseOverRight;
            float opacity = isHovered ? OPACITY_HOVER : OPACITY_NORMAL;

            float x = isLeft ? 40 : info.Width - 40;
            float y = info.Height / 2;

            basePaint.Color = SKColors.White.WithAlpha((byte)(255 * opacity));

            string icon = isLeft ? "\uf053" : "\uf054";
            var textBounds = new SKRect();
            iconFont20.MeasureText(icon, out textBounds);
            canvas.DrawText(icon, x - textBounds.MidX, y - textBounds.MidY, iconFont20, basePaint);
        }

        private void DrawCornerLabel(SKCanvas canvas, SKImageInfo info, string text, bool isBottomRight)
        {
            if (string.IsNullOrEmpty(text))
            {
                if (isBottomRight)
                    authorLabelRect = Rectangle.Empty;
                else
                    downloadSizeLabelRect = Rectangle.Empty;
                return;
            }

            var textBounds = new SKRect();
            textFont.MeasureText(text, out textBounds);

            float leftMargin = isBottomRight ? 8 : 11;
            float rightMargin = isBottomRight ? 11 : 8;
            float borderWidth = textBounds.Width + leftMargin + rightMargin;
            float borderHeight = 4 + 16 + 9;

            float rectX = isBottomRight ? info.Width - borderWidth + 3 : -3;
            float rectY = info.Height - borderHeight + 3;
            Rectangle labelRect = new Rectangle((int)rectX, (int)rectY, (int)borderWidth, (int)borderHeight);

            if (isBottomRight)
                authorLabelRect = labelRect;
            else
                downloadSizeLabelRect = labelRect;

            basePaint.Color = overlayColor;
            canvas.DrawRoundRect(SKRect.Create(rectX, rectY, borderWidth, borderHeight), BORDER_RADIUS, BORDER_RADIUS, basePaint);

            basePaint.Color = SKColors.White.WithAlpha(OVERLAY_ALPHA);
            float textX = isBottomRight ? info.Width - textBounds.Width - rightMargin + 3 : leftMargin - 3;
            float textY = rectY + 4 + 16;
            canvas.DrawText(text, textX, textY, textFont, basePaint);
        }

        private void DrawCarouselIndicators(SKCanvas canvas, SKImageInfo info)
        {
            int count = ViewModel.Items.Count;
            int indicatorWidth = 30;
            int indicatorHeight = 3;
            int itemSpacing = 6;
            int totalWidth = count * indicatorWidth + (count - 1) * itemSpacing;
            int startX = (info.Width - totalWidth) / 2;
            int y = info.Height - 16 - 16;  // bottom margin - half of clickable height

            carouselIndicatorRects = new Rectangle[count];

            for (int i = 0; i < count; i++)
            {
                float opacity = (i == ViewModel.SelectedIndex) ? OPACITY_HOVER : OPACITY_NORMAL;
                basePaint.Color = SKColors.White.WithAlpha((byte)(255 * opacity));

                int rectX = startX + i * (indicatorWidth + itemSpacing);
                canvas.DrawRect(rectX, y - indicatorHeight / 2, indicatorWidth, indicatorHeight, basePaint);

                // Cache full clickable area for hit testing
                carouselIndicatorRects[i] = new Rectangle(rectX, y - 16, indicatorWidth, 32);
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (!ViewModel.ControlsVisible) return;

            // Check if play button was clicked
            if (playButtonRect.Contains(e.Location))
            {
                ViewModel.TogglePlayPause();
                return;
            }

            // Check if left arrow area was clicked
            if (e.X < ARROW_AREA_WIDTH)
            {
                ViewModel.Previous();
                return;
            }

            // Check if right arrow area was clicked
            if (e.X > Width - ARROW_AREA_WIDTH)
            {
                ViewModel.Next();
                return;
            }

            // Check if download message was clicked
            if (downloadMessageRect.Contains(e.Location))
            {
                ViewModel.InvokeDownload();
                return;
            }

            // Check if carousel indicator was clicked
            if (carouselIndicatorRects != null)
            {
                for (int i = 0; i < carouselIndicatorRects.Length; i++)
                {
                    if (carouselIndicatorRects[i].Contains(e.Location))
                    {
                        ViewModel.SelectedIndex = i;
                        return;
                    }
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            bool needsRedraw = false;

            // Update cursor based on location
            if (!ViewModel.ControlsVisible)
            {
                Cursor = Cursors.Default;
                return;
            }

            bool wasOverPlay = isMouseOverPlay;
            bool wasOverLeft = isMouseOverLeft;
            bool wasOverRight = isMouseOverRight;
            bool wasOverDownload = isMouseOverDownload;

            // Reset all hover states
            isMouseOverPlay = false;
            isMouseOverLeft = false;
            isMouseOverRight = false;
            isMouseOverDownload = false;

            // Check UI elements in priority order using cached rectangles
            isMouseOverPlay = playButtonRect.Contains(e.Location);

            if (!isMouseOverPlay)
                isMouseOverDownload = downloadMessageRect.Contains(e.Location);

            // Check if over any UI box (title, corner labels)
            bool isOverUIBox = false;
            if (!isMouseOverPlay && !isMouseOverDownload)
            {
                isOverUIBox = titleBoxRect.Contains(e.Location) ||
                             authorLabelRect.Contains(e.Location) ||
                             downloadSizeLabelRect.Contains(e.Location);
            }

            // Arrows only if not over other UI elements
            if (!isMouseOverPlay && !isMouseOverDownload && !isOverUIBox)
            {
                isMouseOverLeft = e.X < ARROW_AREA_WIDTH;
                isMouseOverRight = e.X > Width - ARROW_AREA_WIDTH;
            }

            // Check carousel indicators for hand cursor
            bool isOverCarouselIndicator = false;
            if (carouselIndicatorRects != null)
            {
                for (int i = 0; i < carouselIndicatorRects.Length; i++)
                {
                    if (carouselIndicatorRects[i].Contains(e.Location))
                    {
                        isOverCarouselIndicator = true;
                        break;
                    }
                }
            }

            needsRedraw = (isMouseOverPlay != wasOverPlay) || (isMouseOverLeft != wasOverLeft) ||
                         (isMouseOverRight != wasOverRight) || (isMouseOverDownload != wasOverDownload);

            bool isOverClickable = isMouseOverPlay || isMouseOverLeft || isMouseOverRight || isMouseOverDownload || isOverCarouselIndicator;
            Cursor = isOverClickable ? Cursors.Hand : Cursors.Default;

            if (needsRedraw)
            {
                Invalidate();
            }
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            ViewModel.IsMouseOver = false;

            // Reset all hover states
            bool needsRedraw = isMouseOverPlay || isMouseOverLeft || isMouseOverRight || isMouseOverDownload;
            
            isMouseOverPlay = false;
            isMouseOverLeft = false;
            isMouseOverRight = false;
            isMouseOverDownload = false;

            Cursor = Cursors.Default;

            if (needsRedraw)
            {
                Invalidate();
            }
        }

        private void StartAnimation()
        {
            fadeProgress = 0f;
            isAnimating = true;
            animationStartTime = DateTime.Now;
            fadeTimer.Start();
        }

        private void StopAnimation()
        {
            fadeTimer.Stop();
            isAnimating = false;
            fadeProgress = 0f;
            Invalidate();
        }

        private void FadeTimer_Tick(object sender, EventArgs e)
        {
            var elapsed = DateTime.Now - animationStartTime;
            fadeProgress = Math.Min(1.0f, (float)(elapsed.TotalMilliseconds / ANIMATION_DURATION_MS));

            // Ease in-out sine function
            fadeProgress = (float)(Math.Sin((fadeProgress - 0.5) * Math.PI) / 2 + 0.5);

            Invalidate();

            if (fadeProgress >= 1.0f)
            {
                fadeTimer.Stop();
                isAnimating = false;
                ViewModel.OnAnimationComplete();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                fadeTimer?.Dispose();
                ViewModel?.Stop();
                
                // Dispose cached SkiaSharp objects
                basePaint?.Dispose();
                titleFont?.Dispose();
                previewFont?.Dispose();
                textFont?.Dispose();
                iconFont16?.Dispose();
                iconFont20?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
