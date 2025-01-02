using Microsoft.Win32;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

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
            driver.FindElementByXPath("//Window[@Name='Select Language']").Click();
            driver.FindElementByXPath("//Button[@Name='OK']").Click();
            Thread.Sleep(TimeSpan.FromSeconds(1));

            driver.SwitchTo().Window(driver.WindowHandles[0]);
            driver.FindElementByXPath("//Window[@Name='Configure Schedule']").Click();
            driver.FindElementByAccessibilityId("radioButton1").Click();
            driver.FindElementByAccessibilityId("locationBox").SendKeys("New York NY");
            driver.FindElementByXPath("//Button[@Name='OK']").Click();
            driver.SwitchTo().Window(driver.WindowHandles.Last());
            driver.FindElementByXPath("//Button[@Name='Yes']").Click();
            Thread.Sleep(TimeSpan.FromSeconds(1));

            driver.SwitchTo().Window(driver.WindowHandles[0]);
            driver.FindElementByXPath("//Window[@Name='Select Theme']").Click();
            driver.FindElementByAccessibilityId("listView1").SendKeys(Keys.Control + Keys.End);
            driver.FindElementByXPath("//ListItem[@Name='Windows 11']").Click();
            driver.FindElementByXPath("//Button[@Name='Apply']").Click();

            Assert.Contains(["scripts", "settings.json", "themes"],
                Directory.GetFileSystemEntries(Path.GetDirectoryName(AppPath)).Select(Path.GetFileName).ToArray());
            Assert.StartsWith(Path.Combine(Path.GetDirectoryName(AppPath), "themes", "Windows_11", "img"), GetWallpaperPath());
        }

        public void Dispose()
        {
            driver?.Quit();
        }

        private string? GetWallpaperPath()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop"))
            {
                return key?.GetValue("WallPaper") as string;
            }
        }
    }
}
