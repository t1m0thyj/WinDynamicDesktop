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
        private static readonly Guid GUID_CONSOLE_DISPLAY_STATE =
            new Guid("6fe69556-704a-47a0-8f24-c28d936fda47");

        private static IntPtr hPowerNotify;
        private static int? lastDisplayState = null;

        [DllImport("user32.dll")]
        private static extern IntPtr RegisterPowerSettingNotification(
            IntPtr hRecipient, ref Guid PowerSettingGuid, int Flags);

        [DllImport("user32.dll")]
        private static extern bool UnregisterPowerSettingNotification(IntPtr Handle);

        public static void RegisterWindow(IntPtr windowHandle)
        {
            Guid guid = GUID_CONSOLE_DISPLAY_STATE;
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

            if (powerSetting.PowerSetting.Equals(GUID_CONSOLE_DISPLAY_STATE))
            {
                IntPtr dataPtr = new(msgPtr.ToInt64() + Marshal.SizeOf<POWERBROADCAST_SETTING>());
                int displayState = Marshal.ReadInt32(dataPtr);

                bool stateChanged = lastDisplayState.HasValue && lastDisplayState.Value != displayState;
                lastDisplayState = displayState;

                // If state changed and display is turning on, assume we are exiting standby
                return stateChanged && displayState == 1;
            }

            return false;
        }
    }
}
