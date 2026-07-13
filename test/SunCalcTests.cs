using FakeTimeZone;
using TimeZoneConverter;

namespace WinDynamicDesktop.Tests
{
    public class SunCalcTests
    {
        private Func<DateTime, DateTime> CreateTimeConverter(string localTz)
        {
            TimeZoneInfo timeZone = TZConvert.GetTimeZoneInfo(localTz);
            return date => TimeZoneInfo.ConvertTime(date, timeZone);
        }

        public SunCalcTests()
        {
            JsonConfig.settings.locationMode = 0;
            SunriseSunsetService.GetDateTimeNow = () => DateTime.UtcNow.Date.ToLocalTime();
        }

        [Fact]
        public void TestWesternHemisphere()
        {
            const string localTz = "America/Los_Angeles";  // California (UTC-8)
            var convertTime = CreateTimeConverter(localTz);
            DateTime testDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime();
            JsonConfig.settings.latitude = 36.8;
            JsonConfig.settings.longitude = -119.4;
            SolarData data = SunriseSunsetService.GetSolarData(testDate.Date);
            Assert.Equal(PolarPeriod.None, data.polarPeriod);
            Assert.Equal(convertTime(testDate).Date, convertTime(data.sunriseTime).Date);
            Assert.Equal((7, 11), (convertTime(data.sunriseTime).Hour, convertTime(data.sunriseTime).Minute));
            Assert.Equal(convertTime(testDate).Date, convertTime(data.sunsetTime).Date);
            Assert.Equal((16, 51), (convertTime(data.sunsetTime).Hour, convertTime(data.sunsetTime).Minute));
        }

        [Fact]
        public void TestEasternHemisphere()
        {
            const string localTz = "Asia/Baku";  // Azerbaijan (UTC+4)
            var convertTime = CreateTimeConverter(localTz);
            DateTime testDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc).ToLocalTime();
            JsonConfig.settings.latitude = 40.1;
            JsonConfig.settings.longitude = 47.6;
            SolarData data = SunriseSunsetService.GetSolarData(testDate.Date);
            Assert.Equal(PolarPeriod.None, data.polarPeriod);
            Assert.Equal(convertTime(testDate).Date, convertTime(data.sunriseTime).Date);
            Assert.Equal((8, 12), (convertTime(data.sunriseTime).Hour, convertTime(data.sunriseTime).Minute));
            Assert.Equal(convertTime(testDate).Date, convertTime(data.sunsetTime).Date);
            Assert.Equal((17, 34), (convertTime(data.sunsetTime).Hour, convertTime(data.sunsetTime).Minute));
        }

        [Fact]
        public void TestPolarDayWithTwilight()
        {
            const string localTz = "Arctic/Longyearbyen";  // Svalbard (UTC+1)
            var convertTime = CreateTimeConverter(localTz);
            DateTime testDate = new DateTime(2024, 4, 18, 0, 0, 0, DateTimeKind.Utc).ToLocalTime();
            JsonConfig.settings.latitude = 78.22;
            JsonConfig.settings.longitude = 15.63;
            SolarData data = SunriseSunsetService.GetSolarData(testDate.Date);
            Assert.Equal(PolarPeriod.CivilPolarDay, data.polarPeriod);
            Assert.Equal(convertTime(testDate).Date, convertTime(data.sunriseTime).Date);
            Assert.Equal((1, 6), (convertTime(data.sunriseTime).Hour, convertTime(data.sunriseTime).Minute));
            Assert.Equal(convertTime(testDate).AddDays(1).Date, convertTime(data.sunsetTime).Date);
            Assert.Equal((0, 49), (convertTime(data.sunsetTime).Hour, convertTime(data.sunsetTime).Minute));
            Assert.Equal(convertTime(testDate).Date, convertTime(data.solarNoon).Date);
            Assert.Equal((12, 57), (convertTime(data.solarNoon).Hour, convertTime(data.solarNoon).Minute));
            Assert.Equal(12 * 60, data.solarNoon.Subtract(data.solarTimes[0]).TotalMinutes);
            Assert.Equal(convertTime(testDate).Date, convertTime(data.solarTimes[1]).Date);
            Assert.Equal(convertTime(testDate).Date, convertTime(data.solarTimes[2]).Date);
            Assert.Equal(12 * 60, (int)Math.Round(data.solarTimes[3].Subtract(data.solarNoon).TotalMinutes));
        }

        [Fact]
        public void TestPolarDay()
        {
            const string localTz = "Arctic/Longyearbyen";  // Svalbard (UTC+1)
            var convertTime = CreateTimeConverter(localTz);
            DateTime testDate = new DateTime(2024, 4, 19, 0, 0, 0, DateTimeKind.Utc).ToLocalTime();
            JsonConfig.settings.latitude = 78.22;
            JsonConfig.settings.longitude = 15.63;
            SolarData data = SunriseSunsetService.GetSolarData(testDate.Date);
            Assert.Equal(PolarPeriod.PolarDay, data.polarPeriod);
            Assert.Equal(DateTime.MinValue, data.sunriseTime);
            Assert.Equal(DateTime.MinValue, data.sunsetTime);
            Assert.Equal(convertTime(testDate).Date, convertTime(data.solarNoon).Date);
            Assert.Equal((12, 57), (convertTime(data.solarNoon).Hour, convertTime(data.solarNoon).Minute));
            Assert.Equal(DateTime.MinValue, data.solarTimes[0]);
            Assert.Equal(convertTime(testDate).Date, convertTime(data.solarTimes[1]).Date);
            Assert.Equal(convertTime(testDate).Date, convertTime(data.solarTimes[2]).Date);
            Assert.Equal(DateTime.MinValue, data.solarTimes[3]);
        }

        [Fact]
        public void TestPolarNightWithTwilight()
        {
            const string localTz = "Arctic/Longyearbyen";  // Svalbard (UTC+1)
            var convertTime = CreateTimeConverter(localTz);
            DateTime testDate = new DateTime(2024, 10, 26, 0, 0, 0, DateTimeKind.Utc).ToLocalTime();
            JsonConfig.settings.latitude = 78.22;
            JsonConfig.settings.longitude = 15.63;
            SolarData data = SunriseSunsetService.GetSolarData(testDate.Date);
            Assert.Equal(PolarPeriod.CivilPolarNight, data.polarPeriod);
            Assert.Equal(convertTime(testDate).Date, convertTime(data.sunriseTime).Date);
            Assert.Equal((12, 17), (convertTime(data.sunriseTime).Hour, convertTime(data.sunriseTime).Minute));
            Assert.Equal(convertTime(testDate).Date, convertTime(data.sunsetTime).Date);
            Assert.Equal((13, 7), (convertTime(data.sunsetTime).Hour, convertTime(data.sunsetTime).Minute));
            Assert.Equal(convertTime(testDate).Date, convertTime(data.solarNoon).Date);
            Assert.Equal((12, 42), (convertTime(data.solarNoon).Hour, convertTime(data.solarNoon).Minute));
            Assert.Equal(convertTime(testDate).Date, convertTime(data.solarTimes[0]).Date);
            Assert.Equal(data.solarNoon, data.solarTimes[1]);
            Assert.Equal(data.solarNoon, data.solarTimes[2]);
            Assert.Equal(convertTime(testDate).Date, convertTime(data.solarTimes[3]).Date);
        }

        [Fact]
        public void TestPolarNight()
        {
            const string localTz = "Arctic/Longyearbyen";  // Svalbard (UTC+1)
            var convertTime = CreateTimeConverter(localTz);
            DateTime testDate = new DateTime(2024, 10, 27, 0, 0, 0, DateTimeKind.Utc).ToLocalTime();
            JsonConfig.settings.latitude = 78.22;
            JsonConfig.settings.longitude = 15.63;
            SolarData data = SunriseSunsetService.GetSolarData(testDate.Date);
            Assert.Equal(PolarPeriod.PolarNight, data.polarPeriod);
            Assert.Equal(DateTime.MinValue, data.sunriseTime);
            Assert.Equal(DateTime.MinValue, data.sunsetTime);
            Assert.Equal(convertTime(testDate).Date, convertTime(data.solarNoon).Date);
            Assert.Equal((11, 42), (convertTime(data.solarNoon).Hour, convertTime(data.solarNoon).Minute));
            Assert.Equal(convertTime(testDate).Date, convertTime(data.solarTimes[0]).Date);
            Assert.Equal(DateTime.MinValue, data.solarTimes[1]);
            Assert.Equal(DateTime.MinValue, data.solarTimes[2]);
            Assert.Equal(convertTime(testDate).Date, convertTime(data.solarTimes[3]).Date);
        }

        [Fact]
        public void TestDaylightSavingTime()
        {
            const string localTz = "Europe/London";  // London (UTC -> UTC+1)
            var convertTime = CreateTimeConverter(localTz);
            using var _ = new FakeLocalTimeZone(TZConvert.GetTimeZoneInfo(localTz));
            SunriseSunsetService.GetDateTimeNow = () => DateTime.Today.AddMinutes(90);
            DateTime testDate = new DateTime(2024, 3, 31, 0, 0, 0, DateTimeKind.Local);
            JsonConfig.settings.latitude = 51.50;
            JsonConfig.settings.longitude = -0.129;
            SolarData data = SunriseSunsetService.GetSolarData(testDate.Date);
            Assert.Equal(PolarPeriod.None, data.polarPeriod);
            Assert.Equal(convertTime(testDate).Date, convertTime(data.sunriseTime).Date);
            Assert.Equal((6, 38), (convertTime(data.sunriseTime).Hour, convertTime(data.sunriseTime).Minute));
            Assert.Equal(convertTime(testDate).Date, convertTime(data.sunsetTime).Date);
            Assert.Equal((19, 32), (convertTime(data.sunsetTime).Hour, convertTime(data.sunsetTime).Minute));
        }
    }
}
