using SkiaSharp;
using WinDynamicDesktop.COM;

namespace WinDynamicDesktop.Tests
{
    public class SpannedWallpaperRendererTests : IDisposable
    {
        private readonly string tempDirectory = Path.Combine(Path.GetTempPath(),
            "WinDynamicDesktop.Tests", Guid.NewGuid().ToString("N"));

        public SpannedWallpaperRendererTests()
        {
            Directory.CreateDirectory(tempDirectory);
        }

        [Fact]
        public void GetVirtualBoundsHandlesNegativeAndVerticalCoordinates()
        {
            List<WallpaperMonitor> monitors =
            [
                new WallpaperMonitor { Bounds = Rect(-1920, 0, 0, 1080), ImagePath = "left.jpg" },
                new WallpaperMonitor { Bounds = Rect(0, -2160, 3840, 0), ImagePath = "top.jpg" }
            ];

            RECT bounds = SpannedWallpaperRenderer.GetVirtualBounds(monitors);

            Assert.Equal((-1920, -2160, 3840, 1080),
                (bounds.Left, bounds.Top, bounds.Right, bounds.Bottom));
        }

        [Fact]
        public void RenderUsesPhysicalMonitorRectanglesAndPreservesLayoutGaps()
        {
            string red = CreateImage("red.png", 1, 1, SKColors.Red);
            string blue = CreateImage("blue.png", 1, 1, SKColors.Blue);
            List<WallpaperMonitor> monitors =
            [
                new WallpaperMonitor { Bounds = Rect(-2, 0, 0, 2), ImagePath = red },
                new WallpaperMonitor { Bounds = Rect(1, 0, 4, 2), ImagePath = blue }
            ];

            using SKBitmap bitmap = SpannedWallpaperRenderer.Render(monitors,
                DesktopWallpaperPosition.DWPOS_STRETCH, SKColors.Green);

            Assert.Equal((6, 2), (bitmap.Width, bitmap.Height));
            Assert.Equal(SKColors.Red, bitmap.GetPixel(0, 0));
            Assert.Equal(SKColors.Green, bitmap.GetPixel(2, 0));
            Assert.Equal(SKColors.Blue, bitmap.GetPixel(5, 1));
        }

        [Fact]
        public void FitKeepsImageAspectRatioWithinEachMonitor()
        {
            string imagePath = CreateImage("wide.png", 2, 1, SKColors.Red);
            List<WallpaperMonitor> monitors =
            [
                new WallpaperMonitor { Bounds = Rect(0, 0, 4, 4), ImagePath = imagePath }
            ];

            using SKBitmap bitmap = SpannedWallpaperRenderer.Render(monitors,
                DesktopWallpaperPosition.DWPOS_FIT, SKColors.Black);

            Assert.Equal(SKColors.Black, bitmap.GetPixel(0, 0));
            Assert.Equal(SKColors.Red, bitmap.GetPixel(0, 1));
        }

        public void Dispose()
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
        }

        private string CreateImage(string filename, int width, int height, SKColor color)
        {
            string path = Path.Combine(tempDirectory, filename);
            using SKBitmap bitmap = new SKBitmap(width, height);
            bitmap.Erase(color);
            using SKImage image = SKImage.FromBitmap(bitmap);
            using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
            using FileStream stream = File.Create(path);
            data.SaveTo(stream);
            return path;
        }

        private static RECT Rect(int left, int top, int right, int bottom)
        {
            return new RECT { Left = left, Top = top, Right = right, Bottom = bottom };
        }
    }
}
