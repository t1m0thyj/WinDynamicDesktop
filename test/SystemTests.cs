using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.Core.Tools;
using FlaUI.UIA3;
using Microsoft.Win32;
using System.Drawing;
using Xunit.Abstractions;

namespace WinDynamicDesktop.Tests
{
    public class SystemTests : IDisposable
    {
        private static readonly TimeSpan defaultTimeout = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan retryInterval = TimeSpan.FromMilliseconds(200);
        private readonly string appPath = Path.GetFullPath(@"..\..\..\bin\WinDynamicDesktop.exe");
        private readonly string appDirectory;
        private readonly Application app;
        private readonly UIA3Automation automation;
        private readonly ITestOutputHelper output;

        public SystemTests(ITestOutputHelper output)
        {
            this.output = output;
            appDirectory = Path.GetDirectoryName(appPath) ?? throw new InvalidOperationException("Failed to resolve app directory.");
            app = Application.Launch(appPath);
            automation = new UIA3Automation();
        }

        [Fact, Trait("type", "system")]
        public void ShouldUpdateWallpaper()
        {
            try
            {
                Window languageWindow = WaitForWindow("Select Language")
                    ?? throw new InvalidOperationException("Select Language window was not found.");

                languageWindow.Focus();
                FindElement(languageWindow, "//Button[@Name='OK']").AsButton().Invoke();
                WaitForWindowToClose("Select Language");

                HandleLocationPrompt();

                Window scheduleWindow = WaitForWindow("Configure Schedule") ?? throw new InvalidOperationException("Configure Schedule window was not found.");
                FindElementByAutomationId(scheduleWindow, "radioButton3").Click();
                FindElement(scheduleWindow, "//Button[@Name='OK']").AsButton().Invoke();
                WaitForWindowToClose("Configure Schedule");

                Window themeWindow = WaitForWindow("Select Theme") ?? throw new InvalidOperationException("Select Theme window was not found.");
                var themeList = FindElementByAutomationId(themeWindow, "listView1");
                themeList.Focus();
                using (Keyboard.Pressing(FlaUI.Core.WindowsAPI.VirtualKeyShort.CONTROL))
                {
                    Keyboard.Type(FlaUI.Core.WindowsAPI.VirtualKeyShort.END);
                }

                FindElement(themeWindow, "//ListItem[@Name='Windows 11']", TimeSpan.FromSeconds(5)).Click();
                var applyButton = FindElement(themeWindow, "//Button[@Name='Apply']").AsButton();
                applyButton.Invoke();
                WaitForButtonToBeEnabled(applyButton);

                Assert.Contains(["scripts", "settings.json", "themes"],
                    Directory.GetFileSystemEntries(appDirectory).Select(Path.GetFileName).OfType<string>().ToArray());

                string expectedPrefix = Path.Combine(appDirectory, "themes", "Windows_11", "img");
                string? actualWallpaperPath = GetWallpaperPath();
                output.WriteLine($"Expected prefix: [{expectedPrefix}] (len={expectedPrefix.Length})");
                output.WriteLine($"Actual wallpaper path: [{actualWallpaperPath ?? "<null>"}] (len={actualWallpaperPath?.Length ?? 0})");

                Assert.True(
                    actualWallpaperPath is not null &&
                    actualWallpaperPath.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase),
                    $"Wallpaper path mismatch. Expected prefix: [{expectedPrefix}] (len={expectedPrefix.Length}) | Actual: [{actualWallpaperPath ?? "<null>"}] (len={actualWallpaperPath?.Length ?? 0})");
            }
            catch
            {
                TakeScreenshot(Path.Combine(appDirectory, "screenshot.png"));
                throw;
            }
        }

        public void Dispose()
        {
            app?.Close();
            automation?.Dispose();
            app?.Dispose();
        }

        private void HandleLocationPrompt()
        {
            if (app.GetAllTopLevelWindows(automation).Length == 0)
            {
                // Default focus is on No button, so Shift+Tab to focus Yes, then Enter to confirm.
                System.Windows.Forms.SendKeys.SendWait("+{TAB}");
                Thread.Sleep(500);
                System.Windows.Forms.SendKeys.SendWait("{ENTER}");
            }
        }

        private Window? WaitForWindow(string title, TimeSpan? timeout = null, bool throwOnTimeout = true)
        {
            var result = Retry.WhileNull(
                () => app.GetAllTopLevelWindows(automation).FirstOrDefault(window => window.Title == title),
                timeout ?? defaultTimeout,
                retryInterval,
                throwOnTimeout,
                true);
            return result.Result;
        }

        private AutomationElement FindElement(AutomationElement root, string xpath, TimeSpan? timeout = null)
        {
            var result = Retry.WhileNull(
                () => root.FindFirstByXPath(xpath),
                timeout ?? defaultTimeout,
                retryInterval,
                true,
                true);
            return result.Result!;
        }

        private AutomationElement FindElementByAutomationId(AutomationElement root, string automationId, TimeSpan? timeout = null)
        {
            var result = Retry.WhileNull(
                () => root.FindFirstDescendant(cf => cf.ByAutomationId(automationId)),
                timeout ?? defaultTimeout,
                retryInterval,
                true,
                true);
            return result.Result!;
        }

        private void WaitForWindowToClose(string title)
        {
            Retry.WhileNotNull(
                () => app.GetAllTopLevelWindows(automation).FirstOrDefault(window => window.Title == title),
                defaultTimeout,
                retryInterval,
                true,
                true);
        }

        private void WaitForButtonToBeEnabled(Button button)
        {
            Retry.WhileFalse(
                () => button.IsEnabled,
                defaultTimeout,
                retryInterval,
                true,
                true);
        }

        private string? GetWallpaperPath()
        {
            using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop"))
            {
                return key?.GetValue("WallPaper") as string;
            }
        }

        private void TakeScreenshot(string filePath)
        {
            var primaryScreen = System.Windows.Forms.Screen.PrimaryScreen
                ?? throw new InvalidOperationException("No primary screen is available for screenshot capture.");
            Rectangle bounds = primaryScreen.Bounds;
            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size);
                }
                bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
            }
        }
    }
}
