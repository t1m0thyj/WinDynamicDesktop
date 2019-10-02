namespace WinDynamicDesktop
{
    public class BrightnessManager
    {
        // Default brightness values for 'Auto'
        private static readonly int ALL_DAY_AUTO = 80;
        private static readonly int ALL_NIGHT_AUTO = 20;
        private static readonly int SUNRISE_AUTO = 40;
        private static readonly int DAY_AUTO = 80;
        private static readonly int SUNSET_AUTO = 30;
        private static readonly int NIGHT_AUTO = 20;
        public static void Initialize()
        {
            BrightnessController.GetBrightness();
        }
        public static void ChangeBrightness(int timeofDayIndex)
        {
            if (IsDDCSupported())
            {
                if (JsonConfig.settings.useAutoBrightness || JsonConfig.settings.useCustomAutoBrightness)
                {
                    switch (timeofDayIndex)
                    {
                        // ALL DAY
                        case 0:
                            if (JsonConfig.settings.useAutoBrightness)
                                BrightnessController.ChangeBrightness(ALL_DAY_AUTO);
                            else
                                BrightnessController.ChangeBrightness(JsonConfig.settings.allDayBrightness);
                            break;

                        // ALL NIGHT
                        case 1:
                            if (JsonConfig.settings.useAutoBrightness)
                                BrightnessController.ChangeBrightness(ALL_NIGHT_AUTO);
                            else
                                BrightnessController.ChangeBrightness(JsonConfig.settings.allNightBrightness);
                            break;

                        // SUNRISE
                        case 2:
                            if (JsonConfig.settings.useAutoBrightness)
                                BrightnessController.ChangeBrightness(SUNRISE_AUTO);
                            else
                                BrightnessController.ChangeBrightness(JsonConfig.settings.sunriseBrightness);
                            break;

                        // DAY
                        case 3:
                            if (JsonConfig.settings.useAutoBrightness)
                                BrightnessController.ChangeBrightness(DAY_AUTO);
                            else
                                BrightnessController.ChangeBrightness(JsonConfig.settings.dayBrightness);
                            break;

                        // SUNSET
                        case 4:
                            if (JsonConfig.settings.useAutoBrightness)
                                BrightnessController.ChangeBrightness(SUNSET_AUTO);
                            else
                                BrightnessController.ChangeBrightness(JsonConfig.settings.sunsetBrightness);
                            break;

                        // NIGHT
                        case 5:
                            if (JsonConfig.settings.useAutoBrightness)
                                BrightnessController.ChangeBrightness(NIGHT_AUTO);
                            else
                                BrightnessController.ChangeBrightness(JsonConfig.settings.nightBrightness);
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        public static bool IsDDCSupported()
        {
            return BrightnessController.IsDDCSupported;
        }

        public static string GetDDCStatus()
        {
            if (!BrightnessController.IsDDCSupported)
            {
                return "Not Supported";
            }
            else
            {
                return "Supported";
            }
        }
        public static string CurrentDisplayBrightnessValue()
        {
            if (!BrightnessController.IsDDCSupported)
            {
                return "Unknown";
            }
            else
            {
                return BrightnessController.GetBrightness().ToString() + "%";
            }
        }
    }
}
