using System.Timers;

namespace WinDynamicDesktop.Tests
{
    [CollectionDefinition("Scheduler state", DisableParallelization = true)]
    public class SchedulerStateCollection { }

    [Collection("Scheduler state")]
    public class EventSchedulerTests : IDisposable
    {
        private readonly AppConfig originalSettings = JsonConfig.settings;
        private readonly List<ThemeConfig> originalThemes = ThemeManager.themeSettings;
        private readonly List<string> originalImportPaths = ThemeManager.importPaths;
        private readonly ManualTimer backgroundTimer = new();
        private readonly ManualTimer schedulerTimer = new();
        private readonly RecordingDesktop desktop = new();
        private readonly EventScheduler scheduler;
        private int displayCount = 2;

        public EventSchedulerTests()
        {
            JsonConfig.settings = new AppConfig
            {
                locationMode = -1,
                sunriseTime = "00:00:00",
                sunsetTime = "23:59:59",
                activeThemes = [null!, "A", "B"],
                autoUpdateCheck = false
            };
            ThemeManager.themeSettings = [CreateTheme("A"), CreateTheme("B")];
            ThemeManager.importPaths = [];
            scheduler = new EventScheduler(backgroundTimer, schedulerTimer, () => displayCount, () => desktop);
        }

        public void Dispose()
        {
            backgroundTimer.Dispose();
            schedulerTimer.Dispose();
            JsonConfig.settings = originalSettings;
            ThemeManager.themeSettings = originalThemes;
            ThemeManager.importPaths = originalImportPaths;
        }

        [Fact]
        public void AppliesEachDisplayThemeAndSyncsLockScreen()
        {
            JsonConfig.settings.lockScreenDisplayIndex = 2;

            Assert.True(scheduler.Run());

            Assert.Collection(desktop.Writes,
                write => AssertImage(write, 0, "A"),
                write => AssertImage(write, 1, "B"),
                write => AssertImage(write, DisplayEvent.LockScreenIndex, "B"));
            Assert.True(schedulerTimer.Enabled);
            Assert.True(backgroundTimer.Enabled);
            Assert.Equal(60e3, backgroundTimer.Interval);
        }

        [Fact]
        public void SkipsUnchangedImagesAndReappliesOnForce()
        {
            scheduler.Run();
            desktop.Clear();

            scheduler.Run();
            Assert.Empty(desktop.Writes);

            scheduler.Run(true);
            Assert.Equal(2, desktop.Writes.Length);
        }

        [Fact]
        public void UsesEarliestDeadlineAcrossDisplayThemes()
        {
            ThemeConfig secondTheme = ThemeManager.themeSettings[1];
            secondTheme.dayImageList = secondTheme.nightImageList = Enumerable.Range(1, 8).ToArray();
            SolarData data = SunriseSunsetService.GetSolarData(DateTime.Today);
            var display = new DisplayEvent { currentTheme = secondTheme };
            SolarScheduler.CalcNextUpdateTime(data, display);
            DateTime expected = new[] { display.nextUpdateTime, SolarScheduler.CalcNextUpdateTime(data) }.Min();
            DateTime before = DateTime.Now;

            scheduler.Run();

            Assert.InRange(schedulerTimer.Interval,
                Math.Max(double.Epsilon, (expected - DateTime.Now).TotalMilliseconds - 1),
                (expected - before).TotalMilliseconds + 1);
        }

        [Fact]
        public void SchedulerTimerCallbackAppliesUpdatedConfigurationAndRearms()
        {
            scheduler.Run();
            desktop.Clear();
            JsonConfig.settings.activeThemes = [null!, "B", "A"];
            int starts = schedulerTimer.StartCount;

            schedulerTimer.Fire();

            Assert.Collection(desktop.Writes,
                write => AssertImage(write, 0, "B"),
                write => AssertImage(write, 1, "A"));
            Assert.Equal(starts + 1, schedulerTimer.StartCount);
            Assert.True(schedulerTimer.Enabled);
        }

        [Fact]
        public void BackgroundTimerDoesNotRunBeforeDeadline()
        {
            scheduler.Run();
            desktop.Clear();
            int starts = schedulerTimer.StartCount;

            backgroundTimer.Fire();

            Assert.Empty(desktop.Writes);
            Assert.Equal(starts, schedulerTimer.StartCount);
        }

        [Fact]
        public void ResumeRearmsFutureDeadlineWithoutReapplyingImages()
        {
            scheduler.Run();
            desktop.Clear();
            int starts = schedulerTimer.StartCount;
            schedulerTimer.Interval = TimeSpan.FromDays(10).TotalMilliseconds;

            scheduler.HandleResumeFromStandby();

            Assert.Equal(starts + 1, schedulerTimer.StartCount);
            Assert.InRange(schedulerTimer.Interval, double.Epsilon, TimeSpan.FromDays(1).TotalMilliseconds);
            Assert.Empty(desktop.Writes);
        }

        [Fact]
        public void EarlyNotificationsDoNotStartUninitializedScheduler()
        {
            scheduler.OnDisplaySettingsChanged(null, EventArgs.Empty);
            scheduler.HandleResumeFromStandby();
            backgroundTimer.Fire();

            Assert.Empty(desktop.Writes);
            Assert.False(schedulerTimer.Enabled);

            Assert.True(scheduler.Run());
            Assert.Equal(2, desktop.Writes.Length);
        }

        [Fact]
        public void RefreshesSurvivorsOnDisconnectAndAllDisplaysOnReconnect()
        {
            scheduler.Run();
            desktop.Clear();
            displayCount = 1;

            scheduler.OnDisplaySettingsChanged(null, EventArgs.Empty);

            AssertImage(Assert.Single(desktop.Writes), 0, "A");
            Assert.True(schedulerTimer.Enabled);
            desktop.Clear();
            displayCount = 2;

            scheduler.OnDisplaySettingsChanged(null, EventArgs.Empty);

            Assert.Collection(desktop.Writes,
                write => AssertImage(write, 0, "A"),
                write => AssertImage(write, 1, "B"));
            desktop.Clear();
            scheduler.OnDisplaySettingsChanged(null, EventArgs.Empty);
            Assert.Empty(desktop.Writes);
        }

        [Fact]
        public void MissingLockScreenSyncThemeDoesNotStopScheduling()
        {
            JsonConfig.settings.lockScreenDisplayIndex = 2;
            JsonConfig.settings.activeThemes = [null!];

            Assert.True(scheduler.Run(true, new DisplayEvent { displayIndex = -1, lastImagePath = "original.jpg" }));

            Assert.Equal((-1, "original.jpg"), Assert.Single(desktop.Writes));
            Assert.True(schedulerTimer.Enabled);
            Assert.Equal(2, JsonConfig.settings.lockScreenDisplayIndex);
            desktop.Clear();
            JsonConfig.settings.activeThemes = [null!, "A", "B"];
            scheduler.Run(true);
            AssertImage(desktop.Writes.Single(w => w.index == DisplayEvent.LockScreenIndex),
                DisplayEvent.LockScreenIndex, "B");
        }

        [Fact]
        public void DoesNotStartSchedulingWithoutReadyConfiguration()
        {
            JsonConfig.settings.activeThemes = [];

            Assert.False(scheduler.Run());

            Assert.False(schedulerTimer.Enabled);
            Assert.Empty(desktop.Writes);
        }

        [Fact]
        public async Task ConcurrentRunsKeepSelectedWallpaperAndCacheConsistent()
        {
            JsonConfig.settings.activeThemes = ["A"];
            var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using var release = new ManualResetEventSlim();
            var secondStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            bool released = false;
            desktop.BeforeWrite = (path, _) =>
            {
                if (path.Contains(Path.DirectorySeparatorChar + "A" + Path.DirectorySeparatorChar))
                {
                    entered.TrySetResult();
                    released = release.Wait(TimeSpan.FromSeconds(10));
                }
            };

            Task first = Task.Run(() => scheduler.Run());
            Task? second = null;
            try
            {
                await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
                JsonConfig.settings.activeThemes = ["B"];
                second = Task.Factory.StartNew(() =>
                {
                    secondStarted.TrySetResult();
                    scheduler.Run(true);
                }, TaskCreationOptions.LongRunning);
                await secondStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
                Assert.NotSame(second, await Task.WhenAny(second, Task.Delay(100)));
            }
            finally
            {
                release.Set();
                await Task.WhenAll(first, second ?? Task.CompletedTask).WaitAsync(TimeSpan.FromSeconds(5));
            }

            Assert.True(released);
            AssertImage(desktop.Writes.Last(), -1, "B");
            desktop.Clear();
            scheduler.Run();
            Assert.Empty(desktop.Writes);
        }

        [Fact]
        public async Task ResumeRearmDoesNotOverwriteNewerRunTimer()
        {
            scheduler.Run();
            var rearming = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var runStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using var release = new ManualResetEventSlim();
            int intervalSets = 0;
            int newerRunThread = 0;
            bool released = false;
            schedulerTimer.BeforeSetInterval = _ =>
            {
                if (Interlocked.Increment(ref intervalSets) == 1)
                {
                    rearming.TrySetResult();
                    released = release.Wait(TimeSpan.FromSeconds(10));
                }
            };

            Task resume = Task.Run(scheduler.HandleResumeFromStandby);
            Task? newerRun = null;
            try
            {
                await rearming.Task.WaitAsync(TimeSpan.FromSeconds(5));
                JsonConfig.settings.activeThemes = ["B"];
                newerRun = Task.Factory.StartNew(() =>
                {
                    newerRunThread = Environment.CurrentManagedThreadId;
                    runStarted.TrySetResult();
                    scheduler.Run(true);
                }, TaskCreationOptions.LongRunning);
                await runStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
                Assert.NotSame(newerRun, await Task.WhenAny(newerRun, Task.Delay(100)));
            }
            finally
            {
                release.Set();
                await Task.WhenAll(resume, newerRun ?? Task.CompletedTask).WaitAsync(TimeSpan.FromSeconds(5));
            }

            Assert.True(released);
            Assert.Equal(newerRunThread, schedulerTimer.LastStartThreadId);
            AssertImage(desktop.Writes.Last(), -1, "B");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task TimerCallbackDoesNotQueueBehindActiveRun(bool background)
        {
            scheduler.Run();
            int starts = schedulerTimer.StartCount;
            var writing = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using var release = new ManualResetEventSlim();
            bool released = false;
            desktop.BeforeWrite = (_, _) =>
            {
                writing.TrySetResult();
                released = release.Wait(TimeSpan.FromSeconds(10));
            };

            Task run = Task.Run(() => scheduler.Run(true));
            Task? callback = null;
            try
            {
                await writing.Task.WaitAsync(TimeSpan.FromSeconds(5));
                callback = Task.Run(() => (background ? backgroundTimer : schedulerTimer).FireQueued());
                Assert.Same(callback, await Task.WhenAny(callback, Task.Delay(1000)));
                await callback;
            }
            finally
            {
                release.Set();
                await Task.WhenAll(run, callback ?? Task.CompletedTask).WaitAsync(TimeSpan.FromSeconds(5));
            }

            Assert.True(released);
            Assert.Equal(starts + 1, schedulerTimer.StartCount);
        }

        private static ThemeConfig CreateTheme(string id) => new()
        {
            themeId = id,
            imageFilename = "img*.jpg",
            dayImageList = [1],
            nightImageList = [1]
        };

        private static void AssertImage((int index, string path) write, int displayIndex, string themeId)
        {
            Assert.Equal(displayIndex, write.index);
            Assert.Equal(Path.GetFullPath(Path.Combine("themes", themeId, "img1.jpg")), write.path);
        }

        private sealed class ManualTimer : ISchedulerTimer
        {
            private double interval;
            public bool AutoReset { get; set; } = true;
            public double Interval
            {
                get => interval;
                set { BeforeSetInterval?.Invoke(value); interval = value; }
            }
            public Action<double>? BeforeSetInterval { get; set; }
            public bool Enabled { get; private set; }
            public int StartCount { get; private set; }
            public int LastStartThreadId { get; private set; }
            public event ElapsedEventHandler? Elapsed;
            public void Start()
            {
                Enabled = true;
                StartCount++;
                LastStartThreadId = Environment.CurrentManagedThreadId;
            }
            public void Stop() => Enabled = false;
            public void Dispose() { Stop(); Elapsed = null; }

            public void Fire()
            {
                Assert.True(Enabled);
                if (!AutoReset) Enabled = false;
                FireQueued();
            }

            // System.Timers.Timer can deliver an already-queued callback after Stop().
            public void FireQueued() => Elapsed?.Invoke(this, new ElapsedEventArgs(DateTime.Now));
        }

        private sealed class RecordingDesktop : PlatformHelper
        {
            private readonly List<(int index, string path)> writes = [];
            public Action<string, int>? BeforeWrite { get; set; }
            public (int index, string path)[] Writes { get { lock (writes) return writes.ToArray(); } }
            public void Clear() { lock (writes) writes.Clear(); }
            public override string GetLocalFolder() => Environment.CurrentDirectory;
            public override void CheckStartOnBoot() { }
            public override void ToggleStartOnBoot() { }
            public override void OpenUpdateLink() { }
            public override void SetWallpaper(string path, int displayIndex)
            {
                BeforeWrite?.Invoke(path, displayIndex);
                lock (writes) writes.Add((displayIndex, path));
            }
            public override void SetLockScreen(string path) => SetWallpaper(path, DisplayEvent.LockScreenIndex);
        }
    }
}
