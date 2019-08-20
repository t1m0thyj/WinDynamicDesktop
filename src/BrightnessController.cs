using System;
using System.Management;

namespace WinDynamicDesktop
{
    public class BrightnessController
    {
        public static bool IsDDCSupported { get; set; }

        private static readonly ManagementScope scope = new ManagementScope("root\\WMI");

        public static void ChangeBrightness(int value)
        {
            byte[] byteArray = ConvertInt32ToByteArray(value);
            byte brightnessByte = byteArray[0];
            SetBrightness(brightnessByte);

            if (JsonConfig.settings.showBrightnessChangeNotificationToast)
            {
                AppContext.ShowPopup(("Display brightness set to " + value.ToString() + "%"));
            }
        }

        /**
         * 
         * Query the Windows Management Instrumentation to check if the user's PC has
         * the ability to change display brightness. 
         * 
         * **/
        public static int GetBrightness()
        {
            SelectQuery query = new SelectQuery("WmiMonitorBrightness");

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query))
            {
                using (ManagementObjectCollection objectCollection = searcher.Get())
                {
                    try
                    {
                        foreach (ManagementObject mObj in objectCollection)
                        {
                            var currentBrightnessObject = mObj.Properties["CurrentBrightness"].Value;
                            int.TryParse(currentBrightnessObject + "", out int brightnessValue);

                            IsDDCSupported = true;

                            return brightnessValue;
                        }
                    }
                    catch (ManagementException e)
                    {
                        IsDDCSupported = false;
                    }
                }
            }
            return 0;

        }
        private static void SetBrightness(byte brightnessValue)
        {
            SelectQuery query = new SelectQuery("WmiMonitorBrightnessMethods");

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query))
            {
                using (ManagementObjectCollection objectCollection = searcher.Get())
                {
                    foreach (ManagementObject managementObject in objectCollection)
                    {
                        managementObject.InvokeMethod("WmiSetBrightness",
                            new Object[] { UInt32.MaxValue, brightnessValue });
                        break;
                    }
                }
            }
        }
        private static byte[] ConvertInt32ToByteArray(Int32 int32)
        {
            return BitConverter.GetBytes(int32);
        }
    }
}
