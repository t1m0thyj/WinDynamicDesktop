using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace WinDynamicDesktop.WPF
{
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

        private ImageSource backImage;
        public ImageSource BackImage
        {
            get => backImage;
            set => SetProperty(ref backImage, value);
        }

        private ImageSource frontImage;
        public ImageSource FrontImage
        {
            get => frontImage;
            set => SetProperty(ref frontImage, value);
        }

        private double frontOpacity;
        public double FrontOpacity
        {
            get => frontOpacity;
            set => SetProperty(ref frontOpacity, value);
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

        public ObservableCollection<(string preview, string path)> Items { get; } = new ObservableCollection<(string, string)>();

        #endregion

        #region Commands

        public ICommand PlayCommand => new RelayCommand(() =>
        {
            IsPlaying = !IsPlaying;
            if (IsPlaying && indexQueue.IsEmpty)
            {
                transitionTimer.Start();
            }
        });

        public ICommand PreviousCommand => new RelayCommand(Previous);

        public ICommand NextCommand => new RelayCommand(Next);

        #endregion

        private static readonly Func<string, string> _L = Localization.GetTranslation;

        private const int TRANSITION_TIME = 5;
        private const int FRAME_RATE = 60;
        private const float FADE_RATE = (1.0f / FRAME_RATE) / 0.5f;

        private readonly DispatcherTimer fadeTimer;
        private readonly DispatcherTimer transitionTimer;
        private readonly ConcurrentQueue<int> indexQueue = new ConcurrentQueue<int>();

        private bool fading;

        public ThemePreviewerViewModel()
        {
            fadeTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1.0 / FRAME_RATE)
            };
            fadeTimer.Tick += Fade_Tick;

            transitionTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(TRANSITION_TIME)
            };
            transitionTimer.Tick += (s, e) => Next();

            IsPlaying = true;
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

                    int imageCount = 0;
                    string path = Path.Combine("assets", "images", theme.themeId + "_{0}.jpg");

                    string file = string.Format(path, "sunrise");
                    if (File.Exists(file))
                    {
                        sunrise = new[] { file };
                        imageCount++;
                    }

                    file = string.Format(path, "day");
                    if (File.Exists(file))
                    {
                        day = new[] { file };
                        imageCount++;
                    }

                    file = string.Format(path, "sunset");
                    if (File.Exists(file))
                    {
                        sunset = new[] { file };
                        imageCount++;
                    }

                    file = string.Format(path, "night");
                    if (File.Exists(file))
                    {
                        night = new[] { file };
                        imageCount++;
                    }
                }
            }
            else
            {
                Author = "Microsoft";
                Items.Add((string.Empty, new Uri(ThemeThumbLoader.GetWindowsWallpaper()).AbsoluteUri));
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

        private void Fade_Tick(object sender, EventArgs e)
        {
            if (fading)
            {
                FrontOpacity += FADE_RATE * (indexQueue.Count + 1);
                if (FrontOpacity >= 1)
                { 
                    BackImage = FrontImage;
                    FrontOpacity = 0;
                    FrontImage = null;
                    fading = false;
                }
            }
            else if (indexQueue.TryDequeue(out int index))
            {
                FrontImage = new ImageSourceConverter().ConvertFromString(Items[index].path) as ImageSource;
                fading = true;
            }
            else
            {
                fadeTimer.Stop();
                if (IsPlaying)
                {
                    transitionTimer.Start();
                }
            }
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
            indexQueue.Enqueue(index);
            fadeTimer.Start();
            PreviewText = Items[index].preview;
        }

        private void Stop()
        {
            fadeTimer.Stop();
            transitionTimer.Stop();
            while (indexQueue.TryDequeue(out _)) ;

            Title = null;
            Author = null;
            PreviewText = null;
            Message = null;
            BackImage = null;
            FrontImage = null;
            FrontOpacity = 0;
            SelectedIndex = -1;

            Items.Clear();

            fading = false;
        }

        private void AddItems(string preview, string[] items)
        {
            if (items == null) return;
            for (int i = 0; i < items.Length; i++)
            {
                Items.Add(($"{preview} ({i + 1}/{items.Length})", items[i]));
            }
        }

        private void Start(int index)
        {
            var (preview, path) = Items[index];
            PreviewText = preview;
            BackImage = new ImageSourceConverter().ConvertFromString(path) as ImageSource;

            selectedIndex = index;
            OnPropertyChanged(nameof(SelectedIndex));

            if (IsPlaying)
            {
                transitionTimer.Start();
            }
        }

        private static string[] ImagePaths(ThemeConfig theme, int[] imageList)
        {
            return imageList.Select(id =>
                Path.Combine("themes", theme.themeId, theme.imageFilename.Replace("*", id.ToString()))).ToArray();
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
