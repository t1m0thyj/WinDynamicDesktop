using Microsoft.Win32;

namespace WinDynamicDesktop.Tests
{
    public class EventSchedulerTests
    {
        [Theory]
        [InlineData(SessionSwitchReason.SessionUnlock, true)]
        [InlineData(SessionSwitchReason.RemoteConnect, true)]
        [InlineData(SessionSwitchReason.ConsoleConnect, true)]
        [InlineData(SessionSwitchReason.RemoteDisconnect, false)]
        public void RefreshesWallpaperWhenReturningToTheSession(
            SessionSwitchReason reason, bool expected)
        {
            Assert.Equal(expected, EventScheduler.ShouldRefreshAfterSessionSwitch(reason));
        }
    }
}
