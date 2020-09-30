namespace WinDynamicDesktop.WPF
{
    public partial class ThemePreviewer
    {
        public ThemePreviewerViewModel ViewModel { get; } = new ThemePreviewerViewModel();

        public ThemePreviewer()
        {
            InitializeComponent();
            DataContext = ViewModel;
        }
    }
}
