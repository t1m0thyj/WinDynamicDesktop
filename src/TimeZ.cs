using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDynamicDesktop
{
    static class TimeZ
    {

        public static DateTime GetDateTime(DateTime dt) {
            //DateTime dt_utc = dt.ToUniversalTime();

            return TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dt.ToUniversalTime() ,JsonConfig.settings.timezone);
        }
    }
}
