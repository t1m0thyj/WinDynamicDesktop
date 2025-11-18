// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace WinDynamicDesktop.SkiaSharp
{
    public class ThemePreviewer : SKControl
    {
        public ThemePreviewerViewModel ViewModel { get; }

        private readonly Timer animationTimer;
        private readonly Timer fadeTimer;
        private float fadeProgress = 0f;
        private bool isAnimating = false;
        private const int ANIMATION_DURATION_MS = 600;
        private const int ANIMATION_FPS = 60;
        private DateTime animationStartTime;
        private static SKTypeface fontAwesome;
        private Point mousePosition;
        private bool isMouseOverPlay = false;
        private bool isMouseOverLeft = false;
        private bool isMouseOverRight = false;
        private bool isMouseOverDownload = false;

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
                    if (fontStream != null)
                    {
                        fontAwesome = SKTypeface.FromStream(fontStream);
                    }
                }
            }

            // Timer for smooth fade animations
            fadeTimer = new Timer
            {
                Interval = 1000 / ANIMATION_FPS
            };
            fadeTimer.Tick += FadeTimer_Tick;

            // Timer for auto-advance
            animationTimer = new Timer
            {
                Interval = 1000 / 60
            };

            MouseEnter += (s, e) => ViewModel.IsMouseOver = true;
            MouseLeave += (s, e) => ViewModel.IsMouseOver = false;

            ViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ThemePreviewerViewModel.BackImage) ||
                    e.PropertyName == nameof(ThemePreviewerViewModel.FrontImage))
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
                ViewModel.PreviousCommand.Execute(null);
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Right)
            {
                ViewModel.NextCommand.Execute(null);
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

        private void DrawImage(SKCanvas canvas, SKBitmap bitmap, SKImageInfo info, float opacity)
        {
            using (var paint = new SKPaint())
            {
                paint.IsAntialias = true;
                paint.FilterQuality = SKFilterQuality.High;
                paint.Color = paint.Color.WithAlpha((byte)(255 * opacity));

                var destRect = new SKRect(0, 0, info.Width, info.Height);
                canvas.DrawBitmap(bitmap, destRect, paint);
            }
        }

        private void DrawOverlay(SKCanvas canvas, SKImageInfo info)
        {
            using (var paint = new SKPaint())
            {
                paint.IsAntialias = true;

                // Draw left and right arrow button areas
                if (ViewModel.ControlsVisible)
                {
                    DrawArrowArea(canvas, info, true, paint);
                    DrawArrowArea(canvas, info, false, paint);
                }

                // Title and preview text box (top left) - Border with Margin=20, StackPanel with Margin=10
                paint.Color = new SKColor(0, 0, 0, 127);
                paint.Style = SKPaintStyle.Fill;
                
                // Measure text to calculate proper box size
                paint.TextSize = 19;
                paint.Typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
                var titleBounds = new SKRect();
                paint.MeasureText(ViewModel.Title ?? "", ref titleBounds);
                
                paint.TextSize = 16;
                paint.Typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
                var previewBounds = new SKRect();
                paint.MeasureText(ViewModel.PreviewText ?? "", ref previewBounds);
                
                float boxWidth = Math.Max(titleBounds.Width, previewBounds.Width) + 20; // 10 margin each side
                float boxHeight = 19 + 4 + 16 + 20; // title size + margin + preview size + top/bottom margin
                
                var titleRect = SKRect.Create(20, 20, boxWidth, boxHeight);
                paint.Color = new SKColor(0, 0, 0, 127);
                canvas.DrawRoundRect(titleRect, 5, 5, paint);

                // Title text - 10px margin from border
                paint.Color = SKColors.White;
                paint.TextSize = 19;
                paint.Typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
                canvas.DrawText(ViewModel.Title ?? "", 30, 20 + 10 + 19, paint); // margin + top padding + font size

                // Preview text - 4px below title, 16px font
                paint.TextSize = 16;
                paint.Typeface = SKTypeface.FromFamilyName("Segoe UI", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
                canvas.DrawText(ViewModel.PreviewText ?? "", 30, 20 + 10 + 19 + 4 + 16, paint); // add 4px margin + 16px for text

                // Play/Pause button (top right) - MinWidth=40, MinHeight=40, Margin=20
                paint.Color = new SKColor(0, 0, 0, 127);
                var playButtonRect = SKRect.Create(info.Width - 40 - 20, 20, 40, 40);
                canvas.DrawRoundRect(playButtonRect, 5, 5, paint);

                float playOpacity = isMouseOverPlay ? 1.0f : 0.5f;
                paint.Color = SKColors.White.WithAlpha((byte)(255 * playOpacity));
                paint.TextSize = 16;
                if (fontAwesome != null)
                {
                    paint.Typeface = fontAwesome;
                    string playIcon = ViewModel.IsPlaying ? "\uf04c" : "\uf04b";
                    var textBounds = new SKRect();
                    paint.MeasureText(playIcon, ref textBounds);
                    float centerX = info.Width - 20 - 20;
                    float centerY = 20 + 20;
                    canvas.DrawText(playIcon, centerX - textBounds.MidX, centerY - textBounds.MidY, paint);
                }
                else
                {
                    string playIcon = ViewModel.IsPlaying ? "❚❚" : "▶";
                    canvas.DrawText(playIcon, info.Width - 48, 48, paint);
                }

                paint.Typeface = SKTypeface.FromFamilyName("Segoe UI");

                // Author text (bottom right) - Margin="-3,0,0,-3", TextBlock Margin="8,4,11,9"
                if (!string.IsNullOrEmpty(ViewModel.Author))
                {
                    paint.Color = new SKColor(0, 0, 0, 127);
                    paint.TextSize = 16;
                    var authorBounds = new SKRect();
                    paint.MeasureText(ViewModel.Author, ref authorBounds);
                    // TextBlock margin: left=8, top=4, right=11, bottom=9
                    float borderWidth = authorBounds.Width + 8 + 11;
                    float borderHeight = 4 + 16 + 9; // top margin + text height + bottom margin
                    var authorRect = SKRect.Create(info.Width - borderWidth + 3, info.Height - borderHeight + 3, borderWidth, borderHeight);
                    canvas.DrawRoundRect(authorRect, 5, 5, paint);

                    paint.Color = SKColors.White.WithAlpha(127);
                    // Text positioned: border top + top margin + font baseline
                    canvas.DrawText(ViewModel.Author, info.Width - authorBounds.Width - 11 + 3, info.Height - borderHeight + 3 + 4 + 16, paint);
                }

                // Download size (bottom left) - Margin="-3,0,0,-3", TextBlock Margin="11,4,8,9"
                if (!string.IsNullOrEmpty(ViewModel.DownloadSize))
                {
                    paint.Color = new SKColor(0, 0, 0, 127);
                    paint.TextSize = 16;
                    var sizeBounds = new SKRect();
                    paint.MeasureText(ViewModel.DownloadSize, ref sizeBounds);
                    // TextBlock margin: left=11, top=4, right=8, bottom=9
                    float borderWidth = sizeBounds.Width + 11 + 8;
                    float borderHeight = 4 + 16 + 9; // top margin + text height + bottom margin
                    var sizeRect = SKRect.Create(-3, info.Height - borderHeight + 3, borderWidth, borderHeight);
                    canvas.DrawRoundRect(sizeRect, 5, 5, paint);

                    paint.Color = SKColors.White.WithAlpha(127);
                    // Text positioned: border top + top margin + font baseline
                    canvas.DrawText(ViewModel.DownloadSize, 11 - 3, info.Height - borderHeight + 3 + 4 + 16, paint);
                }

                // Download message (centered bottom) - Margin="0,0,0,15", TextBlock Margin="8,6,8,6"
                if (!string.IsNullOrEmpty(ViewModel.Message))
                {
                    paint.TextSize = 16;
                    var msgBounds = new SKRect();
                    paint.MeasureText(ViewModel.Message, ref msgBounds);
                    // TextBlock margin: 8,6,8,6 = left+right=16, top+bottom=12
                    float msgWidth = msgBounds.Width + 16;
                    float msgHeight = 6 + 16 + 6; // top margin + text height + bottom margin
                    var msgRect = SKRect.Create(info.Width / 2 - msgWidth / 2, info.Height - msgHeight - 15, msgWidth, msgHeight);
                    
                    paint.Color = new SKColor(0, 0, 0, 127);
                    canvas.DrawRoundRect(msgRect, 5, 5, paint);

                    float msgOpacity = isMouseOverDownload ? 1.0f : 0.8f;
                    paint.Color = SKColors.White.WithAlpha((byte)(255 * msgOpacity));
                    // Text positioned: border top + top margin + font baseline
                    canvas.DrawText(ViewModel.Message, info.Width / 2 - msgBounds.Width / 2, info.Height - msgHeight - 15 + 6 + 16, paint);
                }

                // Carousel indicators - Margin=16, Height=32, Rectangle Height=3, Width=30, Margin="3,0"
                if (ViewModel.CarouselIndicatorsVisible && ViewModel.Items.Count > 0)
                {
                    DrawCarouselIndicators(canvas, info, paint);
                }
            }
        }

        private void DrawArrowArea(SKCanvas canvas, SKImageInfo info, bool isLeft, SKPaint paint)
        {
            bool isHovered = isLeft ? isMouseOverLeft : isMouseOverRight;
            float opacity = isHovered ? 1.0f : 0.5f;
            
            float x = isLeft ? 40 : info.Width - 40;
            float y = info.Height / 2;

            paint.Color = SKColors.White.WithAlpha((byte)(255 * opacity));
            paint.TextSize = 20;

            if (fontAwesome != null)
            {
                paint.Typeface = fontAwesome;
                string icon = isLeft ? "\uf053" : "\uf054";
                var textBounds = new SKRect();
                paint.MeasureText(icon, ref textBounds);
                canvas.DrawText(icon, x - textBounds.MidX, y - textBounds.MidY, paint);
                paint.Typeface = SKTypeface.FromFamilyName("Segoe UI");
            }
            else
            {
                string icon = isLeft ? "◀" : "▶";
                canvas.DrawText(icon, x - 10, y + 7, paint);
            }
        }

        private void DrawCarouselIndicators(SKCanvas canvas, SKImageInfo info, SKPaint paint)
        {
            int count = ViewModel.Items.Count;
            int indicatorWidth = 30;  // Rectangle Width=30
            int indicatorHeight = 3;  // Rectangle Height=3
            int itemSpacing = 6;      // Margin="3,0" means 3px on each side = 6px spacing
            int totalWidth = count * indicatorWidth + (count - 1) * itemSpacing;
            int startX = (info.Width - totalWidth) / 2;
            int y = info.Height - 16 - 32 / 2; // Margin=16 from bottom, Height=32, centered vertically

            for (int i = 0; i < count; i++)
            {
                float opacity = (i == ViewModel.SelectedIndex) ? 1.0f : 0.5f;
                paint.Color = SKColors.White.WithAlpha((byte)(255 * opacity));
                
                int rectX = startX + i * (indicatorWidth + itemSpacing);
                var rect = SKRect.Create(rectX, y - indicatorHeight / 2, indicatorWidth, indicatorHeight);
                canvas.DrawRect(rect, paint);
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (!ViewModel.ControlsVisible) return;

            // Check if play button was clicked - MinWidth=40, MinHeight=40, Margin=20
            var playButtonRect = new Rectangle(Width - 40 - 20, 20, 40, 40);
            if (playButtonRect.Contains(e.Location))
            {
                ViewModel.PlayCommand.Execute(null);
                return;
            }

            // Check if left arrow area was clicked
            if (e.X < 80)
            {
                ViewModel.PreviousCommand.Execute(null);
                return;
            }

            // Check if right arrow area was clicked
            if (e.X > Width - 80)
            {
                ViewModel.NextCommand.Execute(null);
                return;
            }

            // Check if download message was clicked - Margin="0,0,0,15", TextBlock Margin="8,6,8,6"
            if (!string.IsNullOrEmpty(ViewModel.Message))
            {
                using (var paint = new SKPaint())
                {
                    paint.TextSize = 16;
                    var msgBounds = new SKRect();
                    paint.MeasureText(ViewModel.Message, ref msgBounds);
                    float msgWidth = msgBounds.Width + 16; // 8+8
                    float msgHeight = 6 + 16 + 6; // top + text + bottom margin
                    var msgRect = new Rectangle((int)(Width / 2 - msgWidth / 2), Height - (int)msgHeight - 15, (int)msgWidth, (int)msgHeight);
                    if (msgRect.Contains(e.Location))
                    {
                        ViewModel.DownloadCommand.Execute(null);
                        return;
                    }
                }
            }

            // Check if carousel indicator was clicked - Margin=16, Height=32
            if (ViewModel.CarouselIndicatorsVisible && ViewModel.Items.Count > 0)
            {
                int count = ViewModel.Items.Count;
                int indicatorWidth = 30;
                int itemSpacing = 6;
                int totalWidth = count * indicatorWidth + (count - 1) * itemSpacing;
                int startX = (Width - totalWidth) / 2;
                int y = Height - 16 - 32 / 2;

                for (int i = 0; i < count; i++)
                {
                    int rectX = startX + i * (indicatorWidth + itemSpacing);
                    var rect = new Rectangle(rectX, y - 16, indicatorWidth, 32); // Full clickable height
                    if (rect.Contains(e.Location))
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

            mousePosition = e.Location;
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

            // Play button - MinWidth=40, MinHeight=40, Margin=20
            var playButtonRect = new Rectangle(Width - 40 - 20, 20, 40, 40);
            isMouseOverPlay = playButtonRect.Contains(e.Location);

            // Left arrow (80px wide area on left side, full height)
            isMouseOverLeft = e.X < 80;

            // Right arrow (80px wide area on right side, full height)
            isMouseOverRight = e.X > Width - 80;

            // Download message
            isMouseOverDownload = false;
            if (!string.IsNullOrEmpty(ViewModel.Message))
            {
                using (var paint = new SKPaint())
                {
                    paint.TextSize = 16;
                    var msgBounds = new SKRect();
                    paint.MeasureText(ViewModel.Message, ref msgBounds);
                    float msgWidth = msgBounds.Width + 16; // 8+8
                    float msgHeight = 6 + 16 + 6; // top + text + bottom margin
                    var msgRect = new Rectangle((int)(Width / 2 - msgWidth / 2), Height - (int)msgHeight - 15, (int)msgWidth, (int)msgHeight);
                    isMouseOverDownload = msgRect.Contains(e.Location);
                }
            }

            needsRedraw = (isMouseOverPlay != wasOverPlay) || (isMouseOverLeft != wasOverLeft) || 
                         (isMouseOverRight != wasOverRight) || (isMouseOverDownload != wasOverDownload);

            bool isOverClickable = isMouseOverPlay || isMouseOverLeft || isMouseOverRight || isMouseOverDownload;
            Cursor = isOverClickable ? Cursors.Hand : Cursors.Default;

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
                animationTimer?.Dispose();
                ViewModel?.Stop();
            }
            base.Dispose(disposing);
        }
    }
}
