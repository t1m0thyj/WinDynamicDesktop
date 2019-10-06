using System;
using System.Management;

namespace WinDynamicDesktop
{
    public class BrightnessController
    {
        public static bool IsDDCSupported { get; set; }

        private static readonly ManagementScope scope = new ManagementScope("root\\WMI");
        private static readonly SelectQuery queryWmiMonitorBrightness = new SelectQuery("WmiMonitorBrightness");
        private static readonly SelectQuery queryWmiMonitorBrightnessMethods = new SelectQuery("WmiMonitorBrightnessMethods");

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

        /** GetBrightness
         *  For the application to allow brightness change, it must query Windows Management Instrumentation first.
         *  In this case, it will query "WmiMonitorBrightness" from the root\wmi namespace. For PCs or monitors that do not have
         *  DDC/CI or any form of EDID support will throw an "Not Supported" exception.
         */
        public static int GetBrightness()
        {
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, queryWmiMonitorBrightness))
            {
                using (ManagementObjectCollection objectCollection = searcher.Get())
                {
                    try
                    {
                        foreach (ManagementObject mObj in objectCollection)
                        {       
                            IsDDCSupported = true;
                            return Convert.ToInt32(mObj.Properties["CurrentBrightness"].Value);
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
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, queryWmiMonitorBrightnessMethods))
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
