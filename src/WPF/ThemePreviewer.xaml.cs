using System;
using System.Threading;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace WinDynamicDesktop.WPF
{
    public partial class ThemePreviewer
    {
        public ThemePreviewerViewModel ViewModel { get; }

        private readonly Storyboard fadeAnimation;
        private readonly DispatcherTimer triggerTimer;

        public ThemePreviewer()
        {
            ViewModel = new ThemePreviewerViewModel(StartAnimation, StopAnimation);
            DataContext = ViewModel;

            InitializeComponent();

            fadeAnimation = FindResource("FadeAnimation") as Storyboard;
            fadeAnimation.Completed += (s, e) => ViewModel.OnAnimationComplete();

            triggerTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1.0 / 60)
            };
            triggerTimer.Tick += (s, e) =>
            {
                triggerTimer.Stop();
                fadeAnimation.Begin(FrontImage, true);
            };
        }

        private void StartAnimation()
        {
            triggerTimer.Start();
        }

        private void StopAnimation()
        {
            fadeAnimation.Stop(FrontImage);
        }
    }
}
