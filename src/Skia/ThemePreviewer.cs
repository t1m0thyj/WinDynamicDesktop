// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using SkiaSharp;
using SkiaSharp.Views.Desktop;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace WinDynamicDesktop.Skia
{
    public class ThemePreviewer : SKControl
    {
        private const int ANIMATION_DURATION_MS = 600;
        private const int ANIMATION_FPS = 120;

        public ThemePreviewerViewModel ViewModel { get; }

        private readonly Timer fadeTimer;
        private float fadeProgress = 0f;
        private bool isAnimating = false;
        private DateTime animationStartTime;

        private static SKTypeface fontAwesome;
        private readonly ThemePreviewRenderer renderer;
        private HoveredItem hoveredItem = HoveredItem.None;

        public enum HoveredItem
        {
            None,
            PlayButton,
            LeftArrow,
            RightArrow,
            DownloadButton
        }

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

            renderer = new ThemePreviewRenderer(fontAwesome, Control.DefaultFont.FontFamily.Name);

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
                renderer.DrawImage(canvas, ViewModel.BackImage, info, 1.0f);
            }

            // Draw front image with fade animation
            if (ViewModel.FrontImage != null && isAnimating)
            {
                renderer.DrawImage(canvas, ViewModel.FrontImage, info, fadeProgress);
            }

            // Draw UI overlay
            if (ViewModel.ControlsVisible)
            {
                renderer.DrawOverlay(canvas, info, ViewModel, hoveredItem);
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (!ViewModel.ControlsVisible) return;

            // Check if play button was clicked
            if (renderer.PlayButtonRect.Contains(e.Location))
            {
                ViewModel.TogglePlayPause();
                return;
            }

            // Check if left arrow area was clicked
            if (renderer.LeftArrowRect.Contains(e.Location))
            {
                ViewModel.Previous();
                return;
            }

            // Check if right arrow area was clicked
            if (renderer.RightArrowRect.Contains(e.Location))
            {
                ViewModel.Next();
                return;
            }

            // Check if download message was clicked
            if (renderer.DownloadMessageRect.Contains(e.Location))
            {
                ViewModel.InvokeDownload();
                return;
            }

            // Check if carousel indicator was clicked
            var clickedIndex = Array.FindIndex(renderer.CarouselIndicatorRects ?? [], r => r.Contains(e.Location));
            if (clickedIndex != -1)
            {
                ViewModel.SelectedIndex = clickedIndex;
                return;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!ViewModel.ControlsVisible)
            {
                Cursor = Cursors.Default;
                return;
            }

            var previousHoveredItem = hoveredItem;
            hoveredItem = HoveredItem.None;

            // Check UI elements in priority order
            if (renderer.PlayButtonRect.Contains(e.Location))
            {
                hoveredItem = HoveredItem.PlayButton;
            }
            else if (renderer.DownloadMessageRect.Contains(e.Location))
            {
                hoveredItem = HoveredItem.DownloadButton;
            }
            else if (renderer.LeftArrowRect.Contains(e.Location))
            {
                hoveredItem = HoveredItem.LeftArrow;
            }
            else if (renderer.RightArrowRect.Contains(e.Location))
            {
                hoveredItem = HoveredItem.RightArrow;
            }

            // Check carousel indicators for hand cursor
            bool isOverCarouselIndicator = renderer.CarouselIndicatorRects?.Any(r => r.Contains(e.Location)) ?? false;
            bool isOverClickable = hoveredItem != HoveredItem.None || isOverCarouselIndicator;
            Cursor = isOverClickable ? Cursors.Hand : Cursors.Default;

            if (hoveredItem != previousHoveredItem)
            {
                Invalidate();
            }
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            ViewModel.IsMouseOver = false;

            if (hoveredItem != HoveredItem.None)
            {
                hoveredItem = HoveredItem.None;
                Cursor = Cursors.Default;
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
                renderer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
