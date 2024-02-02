using TimeZoneConverter;

namespace WinDynamicDesktop.Tests
{
    public class SunCalcTests
    {
        private string localTz;

        private DateTime ConvertTime(DateTime date)
        {
            return TimeZoneInfo.ConvertTime(date, TZConvert.GetTimeZoneInfo(localTz));
        }

        public SunCalcTests()
        {
            JsonConfig.settings.locationMode = 0;
            SunriseSunsetService.GetDateTimeNow = () => DateTime.UtcNow.Date.ToLocalTime();
        }

        [Fact]
        public void TestWesternHemisphere()
        {
            localTz = "America/Los_Angeles";  // California (UTC-8)
            DateTime testDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime();
            JsonConfig.settings.latitude = 36.8;
            JsonConfig.settings.longitude = -119.4;
            SolarData data = SunriseSunsetService.GetSolarData(testDate.Date);
            Assert.Equal(PolarPeriod.None, data.polarPeriod);
            Assert.Equal(ConvertTime(testDate).Date, ConvertTime(data.sunriseTime).Date);
            Assert.Equal((7, 11), (ConvertTime(data.sunriseTime).Hour, ConvertTime(data.sunriseTime).Minute));
            Assert.Equal(ConvertTime(testDate).Date, ConvertTime(data.sunsetTime).Date);
            Assert.Equal((16, 51), (ConvertTime(data.sunsetTime).Hour, ConvertTime(data.sunsetTime).Minute));
        }

        [Fact]
        public void TestEasternHemisphere()
        {
            localTz = "Asia/Baku";  // Azerbaijan (UTC+4)
            DateTime testDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime();
            JsonConfig.settings.latitude = 40.1;
            JsonConfig.settings.longitude = 47.6;
            SolarData data = SunriseSunsetService.GetSolarData(testDate.Date);
            Assert.Equal(PolarPeriod.None, data.polarPeriod);
            Assert.Equal(ConvertTime(testDate).Date, ConvertTime(data.sunriseTime).Date);
            Assert.Equal((8, 12), (ConvertTime(data.sunriseTime).Hour, ConvertTime(data.sunriseTime).Minute));
            Assert.Equal(ConvertTime(testDate).Date, ConvertTime(data.sunsetTime).Date);
            Assert.Equal((17, 34), (ConvertTime(data.sunsetTime).Hour, ConvertTime(data.sunsetTime).Minute));
        }

        [Fact]
        public void TestPolarDayWithTwilight()
        {
            localTz = "Arctic/Longyearbyen";  // Svalbard (UTC+1)
            DateTime testDate = new DateTime(2024, 4, 18, 0, 0, 0, DateTimeKind.Utc).ToLocalTime();
            JsonConfig.settings.latitude = 78.22;
            JsonConfig.settings.longitude = 15.63;
            SolarData data = SunriseSunsetService.GetSolarData(testDate.Date);
            Assert.Equal(PolarPeriod.None, data.polarPeriod);
            Assert.Equal(ConvertTime(testDate).Date, ConvertTime(data.sunriseTime).Date);
            Assert.Equal((1, 6), (ConvertTime(data.sunriseTime).Hour, ConvertTime(data.sunriseTime).Minute));
            Assert.Equal(ConvertTime(testDate).AddDays(1).Date, ConvertTime(data.sunsetTime).Date);
            Assert.Equal((0, 49), (ConvertTime(data.sunsetTime).Hour, ConvertTime(data.sunsetTime).Minute));
            Assert.Equal(ConvertTime(testDate).Date, ConvertTime(data.solarNoon).Date);
            Assert.Equal((12, 57), (ConvertTime(data.solarNoon).Hour, ConvertTime(data.solarNoon).Minute));
            Assert.Equal(12 * 60, data.solarNoon.Subtract(data.solarTimes[0]).TotalMinutes);
            Assert.Equal(ConvertTime(testDate).Date, ConvertTime(data.solarTimes[1]).Date);
            Assert.Equal(ConvertTime(testDate).Date, ConvertTime(data.solarTimes[2]).Date);
            Assert.Equal(12 * 60, (int)Math.Round(data.solarTimes[3].Subtract(data.solarNoon).TotalMinutes));
        }

        [Fact]
        public void TestPolarDay()
        {
            localTz = "Arctic/Longyearbyen";  // Svalbard (UTC+1)
            DateTime testDate = new DateTime(2024, 4, 19, 0, 0, 0, DateTimeKind.Utc).ToLocalTime();
            JsonConfig.settings.latitude = 78.22;
            JsonConfig.settings.longitude = 15.63;
            SolarData data = SunriseSunsetService.GetSolarData(testDate.Date);
            Assert.Equal(PolarPeriod.PolarDay, data.polarPeriod);
            Assert.Equal(DateTime.MinValue, data.sunriseTime);
            Assert.Equal(DateTime.MinValue, data.sunsetTime);
            Assert.Equal(ConvertTime(testDate).Date, ConvertTime(data.solarNoon).Date);
            Assert.Equal((12, 57), (ConvertTime(data.solarNoon).Hour, ConvertTime(data.solarNoon).Minute));
            Assert.Equal(DateTime.MinValue, data.solarTimes[0]);
            Assert.Equal(ConvertTime(testDate).Date, ConvertTime(data.solarTimes[1]).Date);
            Assert.Equal(ConvertTime(testDate).Date, ConvertTime(data.solarTimes[2]).Date);
            Assert.Equal(DateTime.MinValue, data.solarTimes[3]);
        }

        [Fact]
        public void TestPolarNightWithTwilight()
        {
            localTz = "Arctic/Longyearbyen";  // Svalbard (UTC+1)
            DateTime testDate = new DateTime(2024, 10, 26, 0, 0, 0, DateTimeKind.Utc).ToLocalTime();
            JsonConfig.settings.latitude = 78.22;
            JsonConfig.settings.longitude = 15.63;
            SolarData data = SunriseSunsetService.GetSolarData(testDate.Date);
            Assert.Equal(PolarPeriod.None, data.polarPeriod);
            Assert.Equal(ConvertTime(testDate).Date, ConvertTime(data.sunriseTime).Date);
            Assert.Equal((12, 17), (ConvertTime(data.sunriseTime).Hour, ConvertTime(data.sunriseTime).Minute));
            Assert.Equal(ConvertTime(testDate).Date, ConvertTime(data.sunsetTime).Date);
            Assert.Equal((13, 7), (ConvertTime(data.sunsetTime).Hour, ConvertTime(data.sunsetTime).Minute));
            Assert.Equal(ConvertTime(testDate).Date, ConvertTime(data.solarNoon).Date);
            Assert.Equal((12, 42), (ConvertTime(data.solarNoon).Hour, ConvertTime(data.solarNoon).Minute));
            Assert.Equal(ConvertTime(testDate).Date, ConvertTime(data.solarTimes[0]).Date);
            Assert.Equal(data.solarNoon, data.solarTimes[1]);
            Assert.Equal(data.solarNoon, data.solarTimes[2]);
            Assert.Equal(ConvertTime(testDate).Date, ConvertTime(data.solarTimes[3]).Date);
        }

        [Fact]
        public void TestPolarNight()
        {
            localTz = "Arctic/Longyearbyen";  // Svalbard (UTC+1)
            DateTime testDate = new DateTime(2024, 10, 27, 0, 0, 0, DateTimeKind.Utc).ToLocalTime();
            JsonConfig.settings.latitude = 78.22;
            JsonConfig.settings.longitude = 15.63;
            SolarData data = SunriseSunsetService.GetSolarData(testDate.Date);
            Assert.Equal(PolarPeriod.PolarNight, data.polarPeriod);
            Assert.Equal(DateTime.MinValue, data.sunriseTime);
            Assert.Equal(DateTime.MinValue, data.sunsetTime);
            Assert.Equal(ConvertTime(testDate).Date, ConvertTime(data.solarNoon).Date);
            Assert.Equal((11, 42), (ConvertTime(data.solarNoon).Hour, ConvertTime(data.solarNoon).Minute));
            Assert.Equal(ConvertTime(testDate).Date, ConvertTime(data.solarTimes[0]).Date);
            Assert.Equal(DateTime.MinValue, data.solarTimes[1]);
            Assert.Equal(DateTime.MinValue, data.solarTimes[2]);
            Assert.Equal(ConvertTime(testDate).Date, ConvertTime(data.solarTimes[3]).Date);
        }
    }
}
