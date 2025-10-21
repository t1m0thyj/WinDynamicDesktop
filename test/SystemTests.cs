using Microsoft.Win32;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using System.Drawing;

namespace WinDynamicDesktop.Tests
{
    public class SystemTests : IDisposable
    {
        private const string AppiumServerUrl = "http://127.0.0.1:4723";
        private readonly string AppPath = Path.GetFullPath(@"..\..\..\bin\WinDynamicDesktop.exe");
        private readonly WindowsDriver<WindowsElement> driver;

        public SystemTests()
        {
            var appCapabilities = new AppiumOptions();
            appCapabilities.AddAdditionalCapability("app", AppPath);
            appCapabilities.AddAdditionalCapability("platformName", "Windows");
            appCapabilities.AddAdditionalCapability("deviceName", "WindowsPC");
            driver = new WindowsDriver<WindowsElement>(new Uri(AppiumServerUrl), appCapabilities);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        }

        [Fact, Trait("type", "system")]
        public void ShouldUpdateWallpaper()
        {
            try
            {
                driver.FindElementByXPath("//Window[@Name='Select Language']").Click();
                driver.FindElementByXPath("//Button[@Name='OK']").Click();
                Thread.Sleep(TimeSpan.FromSeconds(2));

                if (HandleLocationPrompt()) Thread.Sleep(TimeSpan.FromSeconds(2));
                driver.SwitchTo().Window(driver.WindowHandles[0]);
                driver.FindElementByXPath("//Window[@Name='Configure Schedule']").Click();
                driver.FindElementByAccessibilityId("radioButton3").Click();
                driver.FindElementByXPath("//Button[@Name='OK']").Click();
                Thread.Sleep(TimeSpan.FromSeconds(2));

                driver.SwitchTo().Window(driver.WindowHandles[0]);
                driver.FindElementByXPath("//Window[@Name='Select Theme']").Click();
                driver.FindElementByAccessibilityId("listView1").SendKeys(Keys.Control + Keys.End);
                driver.FindElementByXPath("//ListItem[@Name='Windows 11']").Click();
                driver.FindElementByXPath("//Button[@Name='Apply']").Click();
                Thread.Sleep(TimeSpan.FromSeconds(2));

                Assert.Contains(["scripts", "settings.json", "themes"],
                    Directory.GetFileSystemEntries(Path.GetDirectoryName(AppPath)).Select(Path.GetFileName).ToArray());
                Assert.StartsWith(Path.Combine(Path.GetDirectoryName(AppPath), "themes", "Windows_11", "img"), GetWallpaperPath());
            }
            catch (WebDriverException)
            {
                TakeScreenshot(Path.Combine(Path.GetDirectoryName(AppPath), "screenshot.png"));
                throw;
            }
        }

        public void Dispose()
        {
            driver?.Quit();
        }

        private bool HandleLocationPrompt()
        {
            if (driver.WindowHandles.Count == 0)
            {
                // Default focus is on No button, so Shift+Tab to focus Yes, then Enter to confirm
                System.Windows.Forms.SendKeys.SendWait("+{TAB}");
                Thread.Sleep(500);
                System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                return true;
            }
            return false;
        }

        private string? GetWallpaperPath()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop"))
            {
                return key?.GetValue("WallPaper") as string;
            }
        }

        private void TakeScreenshot(string filePath)
        {
            Rectangle bounds = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
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
