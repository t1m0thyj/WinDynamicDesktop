using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace WinDynamicDesktop.WPF
{
    public class ThemePreviewItem
    {
        public string PreviewText { get; set; }
        public MemoryStream Data { get; set; }

        public ThemePreviewItem() { }

        public ThemePreviewItem(string previewText, string path)
        {
            PreviewText = previewText;

            Data = new MemoryStream();
            using (var file = File.OpenRead(path))
            {
                file.CopyTo(Data);
            }
        }
    }

    public class ThemePreviewerViewModel : INotifyPropertyChanged
    {
        #region Properties

        public Visibility ControlsVisible => string.IsNullOrEmpty(Title) ? Visibility.Hidden : Visibility.Visible;
        public Visibility MessageVisible => string.IsNullOrEmpty(Message) ? Visibility.Hidden : Visibility.Visible;

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
            }
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

        private static readonly Func<string, string> _L = Localization.GetTranslation;

        private const int TRANSITION_TIME = 5;

        private readonly DispatcherTimer transitionTimer;
        private readonly ConcurrentQueue<int> fadeQueue = new ConcurrentQueue<int>();
        private readonly SemaphoreSlim fadeSemaphore = new SemaphoreSlim(1, 1);
        private readonly Action startAnimation;
        private readonly Action stopAnimation;
        private readonly int maxWidth;
        private readonly int maxHeight;

        public ThemePreviewerViewModel(Action startAnimation, Action stopAnimation)
        {
            this.startAnimation = startAnimation;
            this.stopAnimation = stopAnimation;

            transitionTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(TRANSITION_TIME)
            };
            transitionTimer.Tick += (s, e) => Next();

            IsPlaying = true;

            int maxArea = 0;
            foreach (Screen screen in Screen.AllScreens)
            {
                int area = screen.Bounds.Width * screen.Bounds.Height;
                if (area > maxArea)
                {
                    maxArea = area;
                    maxWidth = screen.Bounds.Width;
                    maxHeight = screen.Bounds.Height;
                }
            }
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
                FrontImage = CreateImage(Items[nextIndex].Data);
                startAnimation();
            }
            else
            {
                TryRelease(fadeSemaphore);

                if (IsPlaying)
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

                if (ThemeManager.IsThemeDownloaded(theme))
                {
                    if (!theme.sunriseImageList.SequenceEqual(theme.dayImageList))
                    {
                        sunrise = ImagePaths(theme, theme.sunriseImageList);
                    }

                    day = ImagePaths(theme, theme.dayImageList);

                    if (!theme.sunsetImageList.SequenceEqual(theme.dayImageList))
                    {
                        sunset = ImagePaths(theme, theme.sunsetImageList);
                    }

                    night = ImagePaths(theme, theme.nightImageList);
                }
                else
                {
                    Message = _L("Theme is not downloaded. Click Download button to enable full preview.");

                    string path = Path.Combine("assets", "images", theme.themeId + "_{0}.jpg");

                    string file = string.Format(path, "sunrise");
                    if (File.Exists(file))
                    {
                        sunrise = new[] { file };
                    }

                    file = string.Format(path, "day");
                    if (File.Exists(file))
                    {
                        day = new[] { file };
                    }

                    file = string.Format(path, "sunset");
                    if (File.Exists(file))
                    {
                        sunset = new[] { file };
                    }

                    file = string.Format(path, "night");
                    if (File.Exists(file))
                    {
                        night = new[] { file };
                    }
                }
            }
            else
            {
                Author = "Microsoft";
                Items.Add(new ThemePreviewItem(string.Empty, ThemeThumbLoader.GetWindowsWallpaper()));
            }

            AddItems(string.Format(_L("Previewing {0}"), _L("Sunrise")), sunrise);
            AddItems(string.Format(_L("Previewing {0}"), _L("Day")), day);
            AddItems(string.Format(_L("Previewing {0}"), _L("Sunset")), sunset);
            AddItems(string.Format(_L("Previewing {0}"), _L("Night")), night);

            if (wpState.daySegment4 >= 1)
            {
                activeImage += sunrise?.Length ?? 0;
            }
            if (wpState.daySegment4 >= 2)
            {
                activeImage += day?.Length ?? 0;
            }
            if (wpState.daySegment4 == 3)
            {
                activeImage += sunset?.Length ?? 0;
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
                FrontImage = CreateImage(Items[index].Data);
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
            while (fadeQueue.TryDequeue(out _)) ;
            TryRelease(fadeSemaphore);

            transitionTimer.Stop();

            Title = null;
            Author = null;
            PreviewText = null;
            Message = null;
            BackImage = null;
            FrontImage = null;
            SelectedIndex = -1;

            foreach (var v in Items)
            {
                v.Data.Dispose();
            }

            Items.Clear();
        }

        private void AddItems(string preview, string[] items)
        {
            if (items == null) return;

            for (int i = 0; i < items.Length; i++)
            {
                Items.Add(new ThemePreviewItem($"{preview} ({i + 1}/{items.Length})", items[i]));
            }
        }

        private void Start(int index)
        {
            var item = Items[index];

            PreviewText = item.PreviewText;
            BackImage = CreateImage(item.Data);

            selectedIndex = index;
            OnPropertyChanged(nameof(SelectedIndex));

            if (IsPlaying)
            {
                transitionTimer.Start();
            }
        }

        private BitmapImage CreateImage(MemoryStream memory)
        {
            memory.Position = 0;

            BitmapImage img = new BitmapImage();
            img.BeginInit();
            img.CacheOption = BitmapCacheOption.None;
            img.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            img.StreamSource = memory;

            if (maxWidth >= maxHeight)
            {
                img.DecodePixelWidth = maxWidth;
            }
            else
            {
                img.DecodePixelHeight = maxHeight;
            }

            img.EndInit();
            img.Freeze();
            return img;
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

        private void SetProperty<T>(ref T field, T value, [CallerMemberName]string propertyName = "")
        {
            field = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
