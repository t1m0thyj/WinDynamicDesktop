// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace WinDynamicDesktop.WPF
{
    public class ThemePreviewItem
    {
        public string PreviewText { get; set; }
        public Uri Uri { get; set; }

        public ThemePreviewItem(string previewText, string path)
        {
            PreviewText = previewText;

            string fullPath = Path.GetFullPath(path);
            if (File.Exists(fullPath))
            {
                Uri = new Uri(fullPath, UriKind.Absolute);
            }
            else
            {
                Uri = new Uri(path, UriKind.Relative);
            }
        }
    }

    public class ThemePreviewerViewModel : INotifyPropertyChanged
    {
        #region Properties

        public bool ControlsVisible => !string.IsNullOrEmpty(Title);
        public bool MessageVisible => !string.IsNullOrEmpty(Message);
        public bool CarouselIndicatorsVisible => string.IsNullOrEmpty(Message);
        public bool DownloadSizeVisible => !string.IsNullOrEmpty(DownloadSize);

        private string title;
        public string Title
        {
            get => title;
            set
            {
                SetProperty(ref title, value);
                OnPropertyChanged(nameof(ControlsVisible));
            }
        }

        private string author;
        public string Author
        {
            get => author;
            set => SetProperty(ref author, value);
        }

        private string previewText;
        public string PreviewText
        {
            get => previewText;
            set => SetProperty(ref previewText, value);
        }

        private string message;
        public string Message
        {
            get => message;
            set
            {
                SetProperty(ref message, value);
                OnPropertyChanged(nameof(MessageVisible));
                OnPropertyChanged(nameof(CarouselIndicatorsVisible));
            }
        }

        private string downloadSize;
        public string DownloadSize
        {
            get => downloadSize;
            set => SetProperty(ref downloadSize, value);
        }

        private BitmapImage backImage;
        public BitmapImage BackImage
        {
            get => backImage;
            set => SetProperty(ref backImage, value);
        }

        private BitmapImage frontImage;
        public BitmapImage FrontImage
        {
            get => frontImage;
            set => SetProperty(ref frontImage, value);
        }

        private bool isPlaying;
        public bool IsPlaying
        {
            get => isPlaying;
            set => SetProperty(ref isPlaying, value);
        }

        private bool isMouseOver;
        public bool IsMouseOver
        {
            get => isMouseOver;
            set
            {
                SetProperty(ref isMouseOver, value);
                if (value)
                {
                    transitionTimer.Stop();
                }
                else if (IsPlaying && fadeQueue.IsEmpty)
                {
                    transitionTimer.Start();
                }
            }
        }

        private int selectedIndex;
        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                if (value != selectedIndex)
                {
                    GoTo(value);
                }
                SetProperty(ref selectedIndex, value);
            }
        }

        public ObservableCollection<ThemePreviewItem> Items { get; } = new ObservableCollection<ThemePreviewItem>();

        #endregion

        #region Commands

        public ICommand PlayCommand => new RelayCommand(() =>
        {
            IsPlaying = !IsPlaying;
            if (IsPlaying && fadeQueue.IsEmpty)
            {
                transitionTimer.Start();
            }
        });

        public ICommand PreviousCommand => new RelayCommand(Previous);

        public ICommand NextCommand => new RelayCommand(Next);

        #endregion

        private static readonly Func<string, string> _ = Localization.GetTranslation;

        private const int TRANSITION_TIME = 5;

        private readonly BitmapCache cache = new BitmapCache();
        private readonly DispatcherTimer transitionTimer;
        private readonly ConcurrentQueue<int> fadeQueue = new ConcurrentQueue<int>();
        private readonly SemaphoreSlim fadeSemaphore = new SemaphoreSlim(1, 1);
        private readonly Action startAnimation;
        private readonly Action stopAnimation;

        public ThemePreviewerViewModel(Action startAnimation, Action stopAnimation)
        {
            this.startAnimation = startAnimation;
            this.stopAnimation = stopAnimation;

            transitionTimer = new DispatcherTimer(DispatcherPriority.Send)
            {
                Interval = TimeSpan.FromSeconds(TRANSITION_TIME)
            };
            transitionTimer.Tick += (s, e) => Next();

            IsPlaying = true;
        }

        public void OnAnimationComplete()
        {
            BackImage = FrontImage;
            FrontImage = null;

            int nextIndex = -1;
            while (fadeQueue.TryDequeue(out int index))
            {
                nextIndex = index;
            }

            if (nextIndex != -1)
            {
                FrontImage = cache[Items[nextIndex].Uri];
                startAnimation();
            }
            else
            {
                TryRelease(fadeSemaphore);

                if (IsPlaying && !IsMouseOver)
                {
                    transitionTimer.Start();
                }
            }
        }

        public void PreviewTheme(ThemeConfig theme)
        {
            Stop();

            int activeImage = 0;
            string[] sunrise = null;
            string[] day = null;
            string[] sunset = null;
            string[] night = null;

            SolarData solarData = SunriseSunsetService.GetSolarData(DateTime.Today);
            SchedulerState wpState = AppContext.wpEngine.GetImageData(solarData, theme, DateTime.Now);

            if (theme != null)
            {
                Title = ThemeManager.GetThemeName(theme);
                Author = ThemeManager.GetThemeAuthor(theme);
                bool isDownloaded = ThemeManager.IsThemeDownloaded(theme);

                if (isDownloaded)
                {
                    ThemeManager.CalcThemeInstallSize(theme, size => { DownloadSize = size; });

                    if (theme.sunriseImageList != null && !theme.sunriseImageList.SequenceEqual(theme.dayImageList))
                    {
                        sunrise = ImagePaths(theme, theme.sunriseImageList);
                    }

                    day = ImagePaths(theme, theme.dayImageList);

                    if (theme.sunsetImageList != null && !theme.sunsetImageList.SequenceEqual(theme.dayImageList))
                    {
                        sunset = ImagePaths(theme, theme.sunsetImageList);
                    }

                    night = ImagePaths(theme, theme.nightImageList);
                }
                else
                {
                    Message = _("Theme is not downloaded. Click Download button to enable full preview.");
                    ThemeManager.CalcThemeDownloadSize(theme, size => { DownloadSize = size; });

                    string[] resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
                    string path = "WinDynamicDesktop.resources.images." + theme.themeId + "_{0}.jpg";

                    string rsrcName = string.Format(path, "sunrise");
                    if (resourceNames.Contains(rsrcName))
                    {
                        sunrise = new[] { rsrcName };
                    }

                    rsrcName = string.Format(path, "day");
                    if (resourceNames.Contains(rsrcName))
                    {
                        day = new[] { rsrcName };
                    }

                    rsrcName = string.Format(path, "sunset");
                    if (resourceNames.Contains(rsrcName))
                    {
                        sunset = new[] { rsrcName };
                    }

                    rsrcName = string.Format(path, "night");
                    if (resourceNames.Contains(rsrcName))
                    {
                        night = new[] { rsrcName };
                    }
                }

                AddItems(_("Sunrise"), sunrise, isDownloaded);
                AddItems(_("Day"), day, isDownloaded);
                AddItems(_("Sunset"), sunset, isDownloaded);
                AddItems(_("Night"), night, isDownloaded);

                if (isDownloaded)
                {
                    activeImage = wpState.imageNumber;
                    bool is4Segment = ThemeManager.IsTheme4Segment(theme);

                    if (is4Segment && wpState.daySegment4 >= 1)
                    {
                        activeImage += sunrise?.Length ?? 0;
                    }
                    if ((is4Segment && wpState.daySegment4 >= 2) || (!is4Segment && wpState.daySegment2 == 1))
                    {
                        activeImage += day?.Length ?? 0;
                    }
                    if (is4Segment && wpState.daySegment4 == 3)
                    {
                        activeImage += sunset?.Length ?? 0;
                    }
                }
                else
                {
                    activeImage = (Items.Count == 2) ? wpState.daySegment2 : wpState.daySegment4.Value;
                }
            }
            else
            {
                Author = "Microsoft";
                Items.Add(new ThemePreviewItem(string.Empty, ThemeThumbLoader.GetWindowsWallpaper()));
                activeImage = 0;
            }

            Start(activeImage);
        }

        private void Previous()
        {
            if (SelectedIndex == 0)
            {
                SelectedIndex = Items.Count - 1;
            }
            else
            {
                SelectedIndex--;
            }
        }

        private void Next()
        {
            if (SelectedIndex == Items.Count - 1)
            {
                SelectedIndex = 0;
            }
            else
            {
                SelectedIndex++;
            }
        }

        private void GoTo(int index)
        {
            if (index < 0 || index >= Items.Count) return;

            transitionTimer.Stop();

            if (fadeSemaphore.Wait(0))
            {
                FrontImage = cache[Items[index].Uri];
                startAnimation();
            }
            else
            {
                fadeQueue.Enqueue(index);
            }

            PreviewText = Items[index].PreviewText;
        }

        public void Stop()
        {
            stopAnimation();
            while (fadeQueue.TryDequeue(out int temp)) ;
            TryRelease(fadeSemaphore);

            transitionTimer.Stop();

            Title = null;
            Author = null;
            PreviewText = null;
            Message = null;
            DownloadSize = null;
            BackImage = null;
            FrontImage = null;
            SelectedIndex = -1;

            Items.Clear();
            cache.Clear();
        }

        private void AddItems(string previewName, string[] items, bool isDownloaded)
        {
            if (items == null) return;

            for (int i = 0; i < items.Length; i++)
            {
                string previewText = previewName;

                if (isDownloaded)
                {
                    previewText = string.Format(_("Previewing {0} ({1}/{2})"), previewName, i + 1, items.Length);
                }
                else
                {
                    previewText = string.Format(_("Previewing {0}"), previewName);
                }

                Items.Add(new ThemePreviewItem(previewText, items[i]));
            }
        }

        private void Start(int index)
        {
            var item = Items[index];

            PreviewText = item.PreviewText;
            BackImage = cache[item.Uri];

            selectedIndex = index;
            OnPropertyChanged(nameof(SelectedIndex));

            if (IsPlaying && !IsMouseOver)
            {
                transitionTimer.Start();
            }
        }

        private static string[] ImagePaths(ThemeConfig theme, int[] imageList)
        {
            return imageList.Select(id =>
                Path.Combine("themes", theme.themeId, theme.imageFilename.Replace("*", id.ToString()))).ToArray();
        }

        private static void TryRelease(SemaphoreSlim semaphore)
        {
            try
            {
                semaphore.Release();
            }
            catch (SemaphoreFullException) { }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            field = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
