using System;

namespace WinDynamicDesktop
{
    public class DateTimeTZ
    {
        public TimeZoneInfo TimeZone;
        public DateTime Time;

        public DateTimeTZ(TimeZoneInfo tz, DateTime time)
        {
            this.TimeZone = tz ?? throw new ArgumentNullException("The time zone cannot be a null reference.");
            this.Time = time;
        }

        public DateTimeTZ(string timezoneid, DateTime time)
        {
            this.TimeZone = TimeZoneInfo.FindSystemTimeZoneById(timezoneid ?? throw new ArgumentNullException("The time zone cannot be a null reference."));
            this.Time = time;
        }

        public DateTimeTZ AddTime(TimeSpan interval)
        {
            DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(this.Time, this.TimeZone);

            utcTime = utcTime.Add(interval);

            return new DateTimeTZ(this.TimeZone, TimeZoneInfo.ConvertTime(utcTime, TimeZoneInfo.Utc, this.TimeZone));
        }

        public static DateTimeTZ Parse(string dateTime, TimeZoneInfo tz)
        {
            return new DateTimeTZ(tz, DateTime.Parse(dateTime));
        }

        public static DateTimeTZ Parse(string dateTime, TimeZoneInfo tz, IFormatProvider provider)
        {
            return new DateTimeTZ(tz, DateTime.Parse(dateTime, provider));
        }

        public DateTimeTZ AddSeconds(double value)
        {
            DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(this.Time, this.TimeZone);

            utcTime = utcTime.AddSeconds(value);

            return new DateTimeTZ(this.TimeZone, TimeZoneInfo.ConvertTime(utcTime, TimeZoneInfo.Utc, this.TimeZone));
        }

        public DateTimeTZ AddHours(int hrs)
        {
            DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(this.Time, this.TimeZone);

            utcTime = utcTime.AddHours(hrs);

            return new DateTimeTZ(this.TimeZone, TimeZoneInfo.ConvertTime(utcTime, TimeZoneInfo.Utc, this.TimeZone));
        }

        public DateTimeTZ AddDays(int days)
        {
            DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(this.Time, this.TimeZone);

            utcTime = utcTime.AddDays(days);

            return new DateTimeTZ(this.TimeZone, TimeZoneInfo.ConvertTime(utcTime, TimeZoneInfo.Utc, this.TimeZone));
        }

        public DateTimeTZ ConvertTime(TimeZoneInfo tz)
        {
            return new DateTimeTZ(tz, TimeZoneInfo.ConvertTime(this.Time, this.TimeZone, tz));
        }

        public DateTimeTZ ConvertTime(String timezoneid)
        {
            return this.ConvertTime(TimeZoneInfo.FindSystemTimeZoneById(timezoneid));
        }

        internal DateTimeTZ ToUniversalTimeTZ()
        {
            return new DateTimeTZ(TimeZoneInfo.Utc, TimeZoneInfo.ConvertTimeToUtc(this.Time, this.TimeZone));
        }

        internal DateTime ToUniversalTime()
        {
            return TimeZoneInfo.ConvertTimeToUtc(this.Time, this.TimeZone);
        }

        internal string ToShortTimeString()
        {
            return this.Time.ToShortTimeString();
        }

        internal class UTC
        {
            internal static DateTimeTZ Now
            {
                get
                {
                    return new DateTimeTZ(TimeZoneInfo.Utc, DateTime.UtcNow);
                }
            }

            internal static DateTimeTZ Today
            {
                get
                {
                    return new DateTimeTZ(TimeZoneInfo.Utc, DateTime.UtcNow.Date);
                }
            }
        }

        internal class Local
        {
            internal static DateTimeTZ Now
            {
                get
                {
                    return new DateTimeTZ(TimeZoneInfo.Local, DateTime.Now);
                }
            }

            internal static DateTimeTZ Today
            {
                get
                {
                    return new DateTimeTZ(TimeZoneInfo.Local, DateTime.Today);
                }
            }
        }
    }
}