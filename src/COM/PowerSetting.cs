// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WinDynamicDesktop.COM
{
    [StructLayout(LayoutKind.Sequential)]
    public struct POWERBROADCAST_SETTING
    {
        public Guid PowerSetting;
        public int DataLength;
        // Variable length data follows
    }

    internal class PowerSetting
    {
        // Interactive applications monitor the display associated with their own session.
        // https://learn.microsoft.com/windows/win32/power/power-setting-guids
        private static readonly Guid GUID_SESSION_DISPLAY_STATUS =
            new Guid("2b84c20e-ad23-4ddf-93db-05ffbd7efca5");

        private static IntPtr hPowerNotify;
        private static int? lastDisplayState = null;

        [DllImport("user32.dll")]
        private static extern IntPtr RegisterPowerSettingNotification(
            IntPtr hRecipient, ref Guid PowerSettingGuid, int Flags);

        [DllImport("user32.dll")]
        private static extern bool UnregisterPowerSettingNotification(IntPtr Handle);

        public static void RegisterWindow(IntPtr windowHandle)
        {
            Guid guid = GUID_SESSION_DISPLAY_STATUS;
            hPowerNotify = RegisterPowerSettingNotification(windowHandle, ref guid, 0);

            if (hPowerNotify != IntPtr.Zero)
            {
                Application.ApplicationExit += (object sender, EventArgs e) =>
                {
                    UnregisterPowerSettingNotification(hPowerNotify);
                };
            }
            else
            {
                LoggingHandler.LogMessage("Failed to register for power notifications");
            }
        }

        public static bool IsExitingStandby(nint msgPtr)
        {
            var powerSetting = Marshal.PtrToStructure<POWERBROADCAST_SETTING>(msgPtr);

            if (powerSetting.PowerSetting.Equals(GUID_SESSION_DISPLAY_STATUS) &&
                powerSetting.DataLength == sizeof(uint))
            {
                IntPtr dataPtr = new(msgPtr.ToInt64() + Marshal.SizeOf<POWERBROADCAST_SETTING>());
                int displayState = Marshal.ReadInt32(dataPtr);

                bool stateChanged = lastDisplayState.HasValue && lastDisplayState.Value != displayState;
                lastDisplayState = displayState;

                // Display-on also catches ordinary monitor wake and undimming; checking the schedule is harmless.
                return stateChanged && displayState == 1;
            }

            return false;
        }
    }
}
